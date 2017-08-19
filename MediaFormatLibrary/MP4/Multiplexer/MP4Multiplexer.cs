/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;
using MediaFormatLibrary.H264;

namespace MediaFormatLibrary.MP4
{
    public class MP4Multiplexer
    {
        public static bool m_enableInterleaving = true;
        public static bool m_enableVideoTimeScaleCompression = true;

        public static void Mux(List<IMultiplexerInput> trackList, Stream outputStream, MultiplexerProfile profile)
        {
            Mux(trackList, outputStream, profile, false);
        }

        /// <param name="enableCTTSv1">
        /// true will utilize CompositionOffsetBox v1 which will allow negative sample offsets,
        /// false will require conversion to non-negative offsets and will utilize EditBox to trim the delay,
        /// </param>
        public static void Mux(List<IMultiplexerInput> trackList, Stream outputStream, MultiplexerProfile profile, bool enableCTTSv1)
        {
            // [IEC/TS 62592] "The Profile Box (Private Extension) shall occur next to the File Type
            // Compatibility Box, before the Movie Box and the Media Data Box.
            FileTypeBox fileTypeBox = GetFileTypeBox(profile);
            fileTypeBox.WriteBytes(outputStream);
            if (profile == MultiplexerProfile.MSNV || profile == MultiplexerProfile.MSNV3D)
            {
                ProfileBox profileBox = GetProfileBox(trackList, profile);
                profileBox.WriteBytes(outputStream);
            }
            // It's up to us to decide which should come first between the Movie Box and the Media Data Box,
            // We'll write the Media Data Box first.
            List<SampleTableBox> sampleTableBoxes = new List<SampleTableBox>();

            long mediaDataBoxOffset = outputStream.Position;
            BoxHelper.WriteBoxHeader(outputStream, BoxType.MediaDataBox, 0); // We'll write the box size later
            List<TrackInfo> trackInfoList = new List<TrackInfo>();
            
            for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
            {
                IMultiplexerInput track = trackList[trackIndex];
                TrackInfo trackInfo = new TrackInfo();
                trackInfo.SampleEntry = track.GetMP4SampleEntry();
                trackInfoList.Add(trackInfo);
            }

            long dataSegmentLength = 0;
            if (m_enableInterleaving)
            {
                WriteSamplesInterleaved(trackList, trackInfoList, outputStream, ref mediaDataBoxOffset, ref dataSegmentLength);
            }
            else
            {
                WriteSamplesSequential(trackList, trackInfoList, outputStream, ref mediaDataBoxOffset, ref dataSegmentLength);
            }

            List<TrackBox> trackBoxes = new List<TrackBox>();
            for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
            {
                IMultiplexerInput track = trackList[trackIndex];
                TrackInfo trackInfo = trackInfoList[trackIndex];
                TrackBox trackBox = GetTrackBox(trackInfo, enableCTTSv1);
                if (profile == MultiplexerProfile.MSNV || profile == MultiplexerProfile.MSNV3D)
                {
                    UserSpecificMetaDataBox trackUserMTBox = MSNVHelper.GetTrackMetaDataBox();
                    trackBox.Children.Add(trackUserMTBox);
                }
                ((TrackHeaderBox)trackBox.Children[0]).TrackID = (uint)trackIndex + 1;
                if (profile == MultiplexerProfile.MSNV3D && track is H264MultiplexerInput && ((H264MultiplexerInput)track).IsFrameSequential3D)
                {
                    MSNV3DHelper.FlagVideoTrackAs3D(trackBox);
                }
                trackBoxes.Add(trackBox);
            }

            MovieBox movieBox = GetMovieBox(trackBoxes, profile);
            movieBox.WriteBytes(outputStream);

            outputStream.Position = mediaDataBoxOffset;
            BigEndianWriter.WriteUInt32(outputStream, (uint)dataSegmentLength + 8);
        }

        public static void WriteSamplesInterleaved(List<IMultiplexerInput> trackList, List<TrackInfo> trackInfoList, Stream outputStream, ref long mediaDataBoxOffset, ref long dataSegmentLength)
        {
            SampleData[] pendingSample = new SampleData[trackList.Count];
            bool hasTrackWithSamples = true;
            double[] trackDurationInSeconds = new double[trackList.Count];
            
            uint chunkCount = 0;
            while (hasTrackWithSamples)
            {
                // We don't want to split an interleave unit over more than one media data box
                MemoryStream interleaveUnitStream = new MemoryStream();
                for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
                {
                    IMultiplexerInput track = trackList[trackIndex];
                    TrackInfo trackInfo = trackInfoList[trackIndex];

                    double chunkDurationInSeconds = 0;
                    SampleData sampleData;
                    if (pendingSample[trackIndex] != null)
                    {
                        sampleData = pendingSample[trackIndex];
                        pendingSample[trackIndex] = null;
                    }
                    else
                    {
                        sampleData = track.ReadSample();
                    }
                    if (sampleData != null)
                    {
                        // Set up a new chunk
                        trackInfo.ChunkOffsetList.Add((ulong)(outputStream.Position + interleaveUnitStream.Position));
                        trackInfo.SampleToChunkEntries.Add(new SampleToChunkEntry(chunkCount + 1, 0, 1));
                        hasTrackWithSamples = true;
                    }

                    // We use the first track as reference and make sure no other tracks creeps ahead of it.
                    const int ReferenceTrackIndex = 0;
                    // "data duration of a single chunk in each track shall be 1 second* maximum" (* samples per second rounded up, e.g. ~1.0027 seconds for 47*1024/48000 samples)
                    const double TargetChunkDuration = 1.0;
                    
                    while (sampleData != null)
                    {
                        byte[] rawSampleData = sampleData.RawSampleData;
                        bool isEndOfChunk = false;
                        
                        // The temporal offset between the first sample of each track in an interleave unit should be kept to a minimum
                        // (to prevent excessive seeking)
                        // The Sony W8 will have playback issues (video stuttering) if the audio track (#2) creeps too much ahead of the video track (#1)
                        // The two Sony 720p 3D demo clips have maximum temporal offset of 0.0 (and minimum of -0.0211666667) for the audio track (#2)
                        double temporalOffset = 0; // in seconds, positive if ahead
                        if (trackIndex != ReferenceTrackIndex)
                        {
                            temporalOffset = trackDurationInSeconds[trackIndex] - trackDurationInSeconds[ReferenceTrackIndex];
                        }

                        if (chunkDurationInSeconds > 0)
                        {
                            if (chunkDurationInSeconds >= TargetChunkDuration)
                            {
                                isEndOfChunk = true;
                            }
                            else if (chunkDurationInSeconds + sampleData.DurationInSeconds > TargetChunkDuration)
                            {
                                // If we can add one last sample to the chunk and still have a non-positive temporal offset, then we should add it
                                // This will minimize the negative temporal offset
                                if (!(trackIndex != ReferenceTrackIndex && (temporalOffset + sampleData.DurationInSeconds <= 0)))
                                {
                                    isEndOfChunk = true;
                                }
                            }
                            else if (trackIndex != ReferenceTrackIndex && pendingSample[ReferenceTrackIndex] != null && (temporalOffset + sampleData.DurationInSeconds >= 0))
                            {
                                isEndOfChunk = true;
                            }
                        }

                        if (!isEndOfChunk)
                        {
                            if (trackInfo.TimeToSampleEntries.Count == 0 || trackInfo.TimeToSampleEntries[trackInfo.TimeToSampleEntries.Count - 1].SampleDelta != sampleData.DurationInTimeUnits)
                            {
                                trackInfo.TimeToSampleEntries.Add(new TimeToSampleEntry(1, sampleData.DurationInTimeUnits));
                            }
                            else
                            {
                                trackInfo.TimeToSampleEntries[trackInfo.TimeToSampleEntries.Count - 1].SampleCount++;
                            }

                            if (trackInfo.CompositionOffsetEntries.Count == 0 || trackInfo.CompositionOffsetEntries[trackInfo.CompositionOffsetEntries.Count - 1].SampleOffset != sampleData.SampleDelayInTimeUnits)
                            {
                                trackInfo.CompositionOffsetEntries.Add(new CompositionOffsetEntry(1, sampleData.SampleDelayInTimeUnits));
                            }
                            else
                            {
                                trackInfo.CompositionOffsetEntries[trackInfo.CompositionOffsetEntries.Count - 1].SampleCount++;
                            }

                            if (sampleData.IsSyncSample)
                            {
                                uint sampleNumber = (uint)(trackInfo.SampleSizeEntries.Count + 1);
                                trackInfo.SyncSampleEntries.Add(sampleNumber);
                            }

                            // Note: we can later try to "compress" SampleToChunkEntries
                            trackInfo.SampleToChunkEntries[trackInfo.SampleToChunkEntries.Count - 1].SamplesPerChunk++;
                            // Note: we can later try to "compress" SampleSizeBox by using SampleSizeBox.SampleSize
                            trackInfo.SampleSizeEntries.Add((uint)rawSampleData.Length);
                            ByteWriter.WriteBytes(interleaveUnitStream, rawSampleData);

                            chunkDurationInSeconds += sampleData.DurationInSeconds;
                            trackInfo.TotalDurationInTimeUnits += sampleData.DurationInTimeUnits;
                            trackDurationInSeconds[trackIndex] += sampleData.DurationInSeconds;
                        }
                        else
                        {
                            pendingSample[trackIndex] = sampleData;
                            break;
                        }

                        sampleData = track.ReadSample();
                    }
                }

                // Write interleave unit to stream
                if (dataSegmentLength + interleaveUnitStream.Length > UInt32.MaxValue)
                {
                    // [IEC/TS 62592] "When the file size exceeds 4 GB, Fragment movie is required".
                    // This method does not conform to [IEC/TS 62592]
                    outputStream.Position = mediaDataBoxOffset;
                    BigEndianWriter.WriteUInt32(outputStream, (uint)dataSegmentLength + 8);
                    mediaDataBoxOffset = mediaDataBoxOffset + 8 + dataSegmentLength;

                    outputStream.Position = mediaDataBoxOffset;
                    BoxHelper.WriteBoxHeader(outputStream, BoxType.MediaDataBox, 0); // We'll write the box size later
                    dataSegmentLength = 0;

                    for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
                    {
                        TrackInfo trackInfo = trackInfoList[trackIndex];
                        if (trackInfo.SampleToChunkEntries[trackInfo.SampleToChunkEntries.Count - 1].FirstChunk == chunkCount + 1)
                        {
                            trackInfo.ChunkOffsetList[trackInfo.ChunkOffsetList.Count - 1] += 8;
                        }
                    }
                }
                interleaveUnitStream.Position = 0;
                ByteUtils.CopyStream(interleaveUnitStream, outputStream);

                dataSegmentLength += interleaveUnitStream.Length;
                chunkCount++;

                hasTrackWithSamples = false;
                for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
                {
                    if (pendingSample[trackIndex] != null)
                    {
                        hasTrackWithSamples = true;
                        break;
                    }
                }
            }
        }

        public static void WriteSamplesSequential(List<IMultiplexerInput> trackList, List<TrackInfo> trackInfoList, Stream outputStream, ref long mediaDataBoxOffset, ref long dataSegmentLength)
        {
            for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
            {
                IMultiplexerInput track = trackList[trackIndex];
                TrackInfo trackInfo = trackInfoList[trackIndex];

                double chunkDurationInSeconds = 0;
                uint chunkCount = 0;
                SampleData sampleData = track.ReadSample();
                while (sampleData != null)
                {
                    byte[] rawSampleData = sampleData.RawSampleData;
                    if (trackInfo.TimeToSampleEntries.Count == 0 || trackInfo.TimeToSampleEntries[trackInfo.TimeToSampleEntries.Count - 1].SampleDelta != sampleData.DurationInTimeUnits)
                    {
                        trackInfo.TimeToSampleEntries.Add(new TimeToSampleEntry(1, sampleData.DurationInTimeUnits));
                    }
                    else
                    {
                        trackInfo.TimeToSampleEntries[trackInfo.TimeToSampleEntries.Count - 1].SampleCount++;
                    }

                    if (trackInfo.CompositionOffsetEntries.Count == 0 || trackInfo.CompositionOffsetEntries[trackInfo.CompositionOffsetEntries.Count - 1].SampleOffset != sampleData.SampleDelayInTimeUnits)
                    {
                        trackInfo.CompositionOffsetEntries.Add(new CompositionOffsetEntry(1, sampleData.SampleDelayInTimeUnits));
                    }
                    else
                    {
                        trackInfo.CompositionOffsetEntries[trackInfo.CompositionOffsetEntries.Count - 1].SampleCount++;
                    }

                    if (sampleData.IsSyncSample)
                    {
                        uint sampleNumber = (uint)(trackInfo.SampleSizeEntries.Count + 1);
                        trackInfo.SyncSampleEntries.Add(sampleNumber);
                    }

                    if (dataSegmentLength + rawSampleData.Length > UInt32.MaxValue)
                    {
                        // [IEC/TS 62592] "When the file size exceeds 4 GB, Fragment movie is required".
                        // This method does not conform to [IEC/TS 62592]
                        outputStream.Position = mediaDataBoxOffset;
                        BigEndianWriter.WriteUInt32(outputStream, (uint)dataSegmentLength + 8);
                        mediaDataBoxOffset = mediaDataBoxOffset + 8 + dataSegmentLength;

                        outputStream.Position = mediaDataBoxOffset;
                        BoxHelper.WriteBoxHeader(outputStream, BoxType.MediaDataBox, 0); // We'll write the box size later
                        dataSegmentLength = 0;
                    }

                    // "data duration of a single chunk in each track shall be 1 second* maximum"
                    if ((chunkDurationInSeconds + sampleData.DurationInSeconds > 1) || chunkCount == 0 || dataSegmentLength == 0)
                    {
                        trackInfo.ChunkOffsetList.Add((ulong)outputStream.Position);
                        trackInfo.SampleToChunkEntries.Add(new SampleToChunkEntry(chunkCount + 1, 1, 1));
                        chunkDurationInSeconds = 0;
                        chunkCount++;
                    }
                    else
                    {
                        trackInfo.SampleToChunkEntries[trackInfo.SampleToChunkEntries.Count - 1].SamplesPerChunk++;
                    }

                    // Note: we can later try to "compress" SampleSizeBox by using SampleSizeBox.SampleSize
                    trackInfo.SampleSizeEntries.Add((uint)rawSampleData.Length);
                    dataSegmentLength += rawSampleData.Length;
                    ByteWriter.WriteBytes(outputStream, rawSampleData);

                    chunkDurationInSeconds += sampleData.DurationInSeconds;
                    trackInfo.TotalDurationInTimeUnits += sampleData.DurationInTimeUnits;
                    sampleData = track.ReadSample();
                }
            }
        }

        public static FileTypeBox GetFileTypeBox(MultiplexerProfile profile)
        {
            FileTypeBox fileTypeBox = new FileTypeBox();
            if (profile == MultiplexerProfile.MSNV || profile == MultiplexerProfile.MSNV3D)
            {
                fileTypeBox.MajorBrand = FileBrand.MSNV;
                if (profile == MultiplexerProfile.MSNV3D)
                {
                    fileTypeBox.MinorVersion = 0x013C07A6; // This is the value used by the two Sony 720p23.97 3D demo clips
                }
                fileTypeBox.CompatibleBrands.Add(FileBrand.MSNV);
                fileTypeBox.CompatibleBrands.Add(FileBrand.mp42); // As specified in IEC/TS 62592
                fileTypeBox.CompatibleBrands.Add(FileBrand.isom); // As specified in IEC/TS 62592
            }
            else if (profile == MultiplexerProfile.mp42)
            {
                fileTypeBox.MajorBrand = FileBrand.mp42;
                fileTypeBox.CompatibleBrands.Add(FileBrand.mp42);
                fileTypeBox.CompatibleBrands.Add(FileBrand.isom);
            }

            return fileTypeBox;
        }

        public static ProfileBox GetProfileBox(List<IMultiplexerInput> trackList, MultiplexerProfile profile)
        {
            ProfileBox profileBox = new ProfileBox();
            FileGolbalProfileEntry fileProfile = new FileGolbalProfileEntry();
            profileBox.Children.Add(fileProfile);
            // Sony demo clips has the AudioProfileBox before the VideoProfileBox,
            // It's not necessary, but makes for easier comparisons
            int audioEntryInsertIndex = 1;

            for (int trackIndex = 0; trackIndex < trackList.Count; trackIndex++)
            {
                IMultiplexerInput track = trackList[trackIndex];
                SampleEntry sampleEntry = track.GetMP4SampleEntry();
                if (sampleEntry is MP4AudioSampleEntry)
                {
                    AudioProfileEntry entry = MSNVHelper.GetAudioProfileEntry((MP4AudioSampleEntry)sampleEntry);
                    entry.TrackID = (uint)trackIndex + 1;
                    profileBox.Children.Insert(audioEntryInsertIndex, entry);
                    audioEntryInsertIndex++;
                }
                else if (sampleEntry is AVCVisualSampleEntry)
                {
                    VideoProfileEntry entry = MSNVHelper.GetVideoProfileEntry((AVCVisualSampleEntry)sampleEntry);
                    entry.TrackID = (uint)trackIndex + 1;
                    if (profile == MultiplexerProfile.MSNV3D && track is H264MultiplexerInput && ((H264MultiplexerInput)track).IsFrameSequential3D)
                    {
                        MSNV3DHelper.FlagVideoProfileAs3D(entry);
                    }
                    profileBox.Children.Add(entry);
                }
            }
            return profileBox;
        }

        private static MovieBox GetMovieBox(List<TrackBox> trackBoxes, MultiplexerProfile profile)
        {
            MovieBox movieBox = new MovieBox();

            long movieTimescale = 1;
            ulong maxDuration = 0;
            foreach (TrackBox trackBox in trackBoxes)
            {
                MediaHeaderBox mediaHeaderBox = (MediaHeaderBox)BoxHelper.FindBoxFromPath(trackBox.Children, BoxType.MediaBox, BoxType.MediaHeaderBox);
                TrackHeaderBox trackHeader = ((TrackHeaderBox)trackBox.Children[0]);
                movieTimescale = Math.Max(movieTimescale, mediaHeaderBox.Timescale);
            }

            foreach (TrackBox trackBox in trackBoxes)
            {
                MediaHeaderBox mediaHeaderBox = (MediaHeaderBox)BoxHelper.FindBoxFromPath(trackBox.Children, BoxType.MediaBox, BoxType.MediaHeaderBox);
                TrackHeaderBox trackHeader = ((TrackHeaderBox)trackBox.Children[0]);
                trackHeader.Duration = (ulong)(mediaHeaderBox.Duration * (ulong)movieTimescale / mediaHeaderBox.Timescale);
                maxDuration = (ulong)Math.Max(trackHeader.Duration, maxDuration);
                movieBox.Children.Add(trackBox);
            }

            MovieHeaderBox movieHeader = new MovieHeaderBox();
            movieHeader.Duration = maxDuration;
            movieHeader.Timescale = (uint)movieTimescale;
            movieHeader.NextTrackID = (uint)trackBoxes.Count + 1;
            movieBox.Children.Insert(0, movieHeader);

            if (profile == MultiplexerProfile.MSNV || profile == MultiplexerProfile.MSNV3D)
            {
                UserSpecificMetaDataBox movieUserMTBox = MSNVHelper.GetMovieMetaDataBox();
                movieBox.Children.Add(movieUserMTBox);
            }
            else
            {
                UserDataBox userData = GetUserDataBox();
                movieBox.Children.Add(userData);
            }

            return movieBox;
        }

        private static UserDataBox GetUserDataBox()
        {
            HandlerBox handlerBox = new HandlerBox();
            handlerBox.HandlerType = HandlerType.QuickTimeMetaData;
            handlerBox.Reserved = new byte[] { 0x61, 0x70, 0x70, 0x6C, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // 'appl\0\0\0\0\0\0\0\0'

            MetaDataValueBox dataBox = new MetaDataValueBox();
            dataBox.DataType = MetaDataValueType.UTF8;
            string writingLibrary = "MP4Maker v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            dataBox.Data = UTF8Encoding.UTF8.GetBytes(writingLibrary);

            EncoderBox encoderBox = new EncoderBox();
            encoderBox.Children.Add(dataBox);

            ItemListBox itemListBox = new ItemListBox();
            itemListBox.Children.Add(encoderBox);

            MetaBox metaBox = new MetaBox();
            metaBox.Children.Add(handlerBox);
            metaBox.Children.Add(itemListBox);

            UserDataBox userDataBox = new UserDataBox();
            userDataBox.Children.Add(metaBox);
            return userDataBox;
        }

        private static TrackBox GetTrackBox(TrackInfo trackInfo, bool enableCTTSv1)
        {
            TimeToSampleBox timeToSampleBox = new TimeToSampleBox();
            SampleToChunkBox sampleToChunkBox = new SampleToChunkBox();
            SampleSizeBox sampleSizeBox = new SampleSizeBox();
            SyncSampleBox syncSampleBox = null;
            CompositionOffsetBox compositionOffsetBox = null;
            
            timeToSampleBox.Entries = trackInfo.TimeToSampleEntries;
            sampleToChunkBox.Entries = trackInfo.SampleToChunkEntries;
            sampleSizeBox.Entries = trackInfo.SampleSizeEntries;
            sampleSizeBox.SampleCount = (uint)trackInfo.SampleSizeEntries.Count;

            if (trackInfo.SyncSampleEntries.Count != sampleSizeBox.SampleCount)
            {
                // [ISO/IEC 14496-12] "If the sync sample box is not present, every sample is a sync sample"
                syncSampleBox = new SyncSampleBox();
                syncSampleBox.Entries = trackInfo.SyncSampleEntries;
            }

            int trackDelay = 0;
            if (!(trackInfo.CompositionOffsetEntries.Count == 1 && trackInfo.CompositionOffsetEntries[0].SampleOffset == 0))
            {
                // [ISO/IEC 14496-12] "The composition time to sample table is optional and must only be present if DT and CT differ for any samples"
                compositionOffsetBox = new CompositionOffsetBox();
                compositionOffsetBox.Entries = trackInfo.CompositionOffsetEntries;

                if (enableCTTSv1)
                {
                    compositionOffsetBox.Version = 1;
                }
                else
                {
                    trackDelay = compositionOffsetBox.ConvertToNonNegative();
                }
            }

            //OptimizeSampleSizeBox(sampleSizeBox);
            return GetTrackBox(trackInfo.SampleEntry, timeToSampleBox, compositionOffsetBox, sampleToChunkBox, sampleSizeBox, trackInfo.ChunkOffsetList, syncSampleBox, trackInfo.TotalDurationInTimeUnits, trackDelay);
        }

        /// <param name="trackDelay">The delay added to the track that has to be edited out</param>
        private static TrackBox GetTrackBox(SampleEntry sampleEntry, TimeToSampleBox timeToSampleBox, CompositionOffsetBox compositionOffsetBox, 
            SampleToChunkBox sampleToChunkBox, SampleSizeBox sampleSizeBox, List<ulong> chunkOffsetList, SyncSampleBox syncSampleBox, uint totalDurationInTimeUnits, int trackDelay)
        {
            DateTime creationTime = DateTime.Now;
            SampleTableBox sampleTable = new SampleTableBox();
            // [IEC/TS 62592] "The boxes within the Sample Table Box should be in the following order:
            // Sample Description, Decoding Time to Sample, Composition Time to Sample,
            // Sample to Chunk, Sample Size, Chunk Offset, and Sync Sample"
            SampleDescriptionBox sampleDescription = new SampleDescriptionBox();
            sampleDescription.Children.Add(sampleEntry);
            sampleTable.Children.Add(sampleDescription);
            sampleTable.Children.Add(timeToSampleBox);
            if (compositionOffsetBox != null && compositionOffsetBox.Entries.Count > 0)
            {
                sampleTable.Children.Add(compositionOffsetBox);
            }
            sampleTable.Children.Add(sampleToChunkBox);
            sampleTable.Children.Add(sampleSizeBox);
            Box chunkOffsetBox = TrackHelper.CreateChunkOffsetBox(chunkOffsetList);
            sampleTable.Children.Add(chunkOffsetBox);
            if (syncSampleBox != null && syncSampleBox.Entries.Count > 0)
            {
                sampleTable.Children.Add(syncSampleBox);
            }

            MediaInformationBox mediaInformation = new MediaInformationBox();
            HandlerBox handler = new HandlerBox();
            uint timeScale;
            if (sampleEntry is AudioSampleEntry)
            {
                mediaInformation.Children.Add(new SoundMediaHeaderBox());
                handler.HandlerType = HandlerType.Audio;
                handler.Name = "Sound Media Handler";
                timeScale = (uint)((AudioSampleEntry)sampleEntry).SampleRate;
            }
            else if (sampleEntry is VisualSampleEntry)
            {
                mediaInformation.Children.Add(new VideoMediaHeaderBox());
                handler.HandlerType = HandlerType.Video;
                handler.Name = "Video Media Handler";
                AVCDecoderConfigurationRecordBox avcConfiguration = (AVCDecoderConfigurationRecordBox)BoxHelper.FindBox(sampleEntry.Children, BoxType.AVCDecoderConfigurationRecordBox);
                VUIParameters vuiParameters = avcConfiguration.SequenceParameterSetList[0].VUIParameters;
                if (vuiParameters != null && vuiParameters.TimingInfoPresentFlag)
                {
                    if (m_enableVideoTimeScaleCompression)
                    {
                        // The Sony demo clips have the smallest possible timescale for the video track, we immitate this behaviour
                        uint greatestCommonDivisor = (uint)MathUtils.DetermineGreatestCommonDivisor(vuiParameters.TimeScale, vuiParameters.MinimumFrameDurationInTimeScale.Value);
                        timeScale = vuiParameters.TimeScale / greatestCommonDivisor;
                        
                        if (greatestCommonDivisor > 1)
                        {
                            totalDurationInTimeUnits /= greatestCommonDivisor;
                            foreach (TimeToSampleEntry entry in timeToSampleBox.Entries)
                            {
                                entry.SampleDelta /= greatestCommonDivisor;
                            }

                            if (compositionOffsetBox != null)
                            {
                                foreach (CompositionOffsetEntry entry in compositionOffsetBox.Entries)
                                {
                                    entry.SampleOffset /= greatestCommonDivisor;
                                }
                            }

                            trackDelay = (int)(trackDelay / greatestCommonDivisor);
                        }
                    }
                    else
                    {
                        timeScale = vuiParameters.TimeScale;
                    }
                }
                else
                {
                    throw new Exception("H264 VUI timing info is not present");
                }
            }
            else
            {
                throw new NotImplementedException();
            }
            DataInformationBox dataInformation = new DataInformationBox();
            DataReferenceBox dataReference = new DataReferenceBox();
            dataReference.Children.Add(new DataEntryUrlBox());
            dataInformation.Children.Add(dataReference);
            mediaInformation.Children.Add(dataInformation);
            mediaInformation.Children.Add(sampleTable);

            MediaHeaderBox mediaHeader = new MediaHeaderBox();
            mediaHeader.CreationTime = creationTime;
            mediaHeader.ModificationTime = creationTime;
            mediaHeader.Timescale = timeScale;
            mediaHeader.Duration = totalDurationInTimeUnits;
            mediaHeader.LanguageCode = LanguageCode.English;

            TrackHeaderBox trackHeader = new TrackHeaderBox();
            trackHeader.CreationTime = creationTime;
            trackHeader.ModificationTime = creationTime;
            if (sampleEntry is VisualSampleEntry)
            {
                trackHeader.Width = ((VisualSampleEntry)sampleEntry).Width;
                trackHeader.Height = ((VisualSampleEntry)sampleEntry).Height;
            }
            else if (sampleEntry is AudioSampleEntry)
            {
                trackHeader.Volume = 1.0;
            }

            MediaBox mediaBox = new MediaBox();
            mediaBox.Children.Add(mediaHeader);
            mediaBox.Children.Add(handler);
            mediaBox.Children.Add(mediaInformation);

            TrackBox trackBox = new TrackBox();
            trackBox.Children.Add(trackHeader);
            if (trackDelay > 0)
            {
                EditListBox editListBox = new EditListBox();
                // See the ISO/IEC 14496-12 example in paragraph 8.6.6.1
                editListBox.Entries.Add(new EditListEntry(0, trackDelay, 1.0));

                EditBox editBox = new EditBox();
                editBox.Children.Add(editListBox);
                trackBox.Children.Add(editBox);
            }
            trackBox.Children.Add(mediaBox);
            return trackBox;
        }

        public static void OptimizeSampleSizeBox(SampleSizeBox sampleSizeBox)
        {
            if (sampleSizeBox.Entries != null && sampleSizeBox.Entries.Count > 0)
            {
                uint sampleSize = sampleSizeBox.Entries[0];
                for (int index = 1; index < sampleSizeBox.Entries.Count; index++)
                {
                    if (sampleSizeBox.Entries[index] != sampleSize)
                    {
                        sampleSize = 0;
                        break;
                    }
                }

                if (sampleSize != 0)
                {
                    sampleSizeBox.SampleSize = sampleSize;
                    sampleSizeBox.Entries = null;
                }
            }
        }
    }
}
