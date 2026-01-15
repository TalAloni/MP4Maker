/* Copyright (C) 2014-2026 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
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
using MediaFormatLibrary.AAC;
using MediaFormatLibrary.AC3;
using MediaFormatLibrary.H264;
using MediaFormatLibrary.MP4;

namespace MP4Maker
{
    public class MP4Helper
    {
        public static void PrintMP4Info(List<Box> parents, int level)
        {
            foreach (Box box in parents)
            {
                string boxType = box.Type.ToString();
                if (box is UserBox)
                {
                    if (box.GetType() == typeof(UserBox))
                    {
                        boxType = ((UserBox)box).UserType.ToString();
                    }
                    else
                    {
                        boxType = box.GetType().Name;
                    }
                }
                string description = String.Format("Box type: {0}, size: {1}", boxType, box.Size.ToString());
                for (int index = 0; index < level; index++)
                {
                    description = "   " + description;
                }
                Console.WriteLine(description);
                if (box.Children != null)
                {
                    PrintMP4Info(box.Children, level + 1);
                }
            }
        }

        public static void PrintTrackInfo(List<Box> rootBoxes)
        {
            Console.WriteLine();
            MovieBox movieBox = (MovieBox)BoxHelper.FindBox(rootBoxes, BoxType.MovieBox);
            if (movieBox == null)
            {
                Console.WriteLine("This MP4 file does not contain MovieBox('moov') box");
                return;
            }

            MovieHeaderBox movieHeader = (MovieHeaderBox)BoxHelper.FindBox(movieBox.Children, BoxType.MovieHeaderBox);
            List<Box> tracks = BoxHelper.FindBoxes(movieBox.Children, BoxType.TrackBox);
            foreach (Box track in tracks)
            {
                TrackHeaderBox trackHeader = (TrackHeaderBox)BoxHelper.FindBox(track.Children, BoxType.TrackHeaderBox);
                HandlerBox handlerBox = (HandlerBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.HandlerReferenceBox);
                SampleDescriptionBox sampleDescription = (SampleDescriptionBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox, BoxType.SampleDescriptionBox);
                if (handlerBox.HandlerType == HandlerType.Audio)
                {
                    AudioSampleEntry sampleEntry = sampleDescription.Children[0] as AudioSampleEntry;
                    if (sampleEntry != null)
                    {
                        int duration = (int)(trackHeader.Duration / movieHeader.Timescale);
                        Console.WriteLine($"{trackHeader.TrackID}: Audio track: {sampleEntry.Type.ToString()}, {sampleEntry.SampleRate}Hz, {sampleEntry.ChannelCount} channels, Duration: {ToTimeSpanString(duration)}");
                    }
                }
                else if (handlerBox.HandlerType == HandlerType.Video)
                {
                    VisualSampleEntry sampleEntry = sampleDescription.Children[0] as VisualSampleEntry;
                    if (sampleEntry != null)
                    {
                        int duration = (int)(trackHeader.Duration / movieHeader.Timescale);
                        Console.WriteLine($"{trackHeader.TrackID}: Video track: {sampleEntry.Type.ToString()}, {sampleEntry.Width}x{sampleEntry.Height}, Duration: {ToTimeSpanString(duration)}");
                    }
                }
            }
        }

        public static void PrintMSNVTrackProfileInfo(List<Box> rootBoxes)
        {
            ProfileBox profileBox = (ProfileBox)BoxHelper.FindUserBox(rootBoxes, ProfileBox.UserTypeGuid);
            if (profileBox != null)
            {
                Console.WriteLine();
                foreach (Box box in profileBox.Children)
                {
                    if (box is AudioProfileEntry)
                    {
                        AudioProfileEntry audioProfile = (AudioProfileEntry)box;
                        Console.WriteLine("Audio profile: {0}, {1}Hz, {2} channels", audioProfile.CodecType.ToString(), audioProfile.SamplingRate, audioProfile.NumberOfChannels);
                    }
                    else if (box is VideoProfileEntry)
                    {
                        VideoProfileEntry videoProfile = (VideoProfileEntry)box;
                        Console.Write("Video profile: {0}, {1}x{2}, {3} fps", videoProfile.CodecType.ToString(), videoProfile.Width, videoProfile.Height, videoProfile.MaxFramerate.ToString("00.000"));
                        if (videoProfile.VideoAttributeFlags != 0)
                        {
                            Console.Write(", attributes: " + videoProfile.VideoAttributeFlags.ToString("X"));
                        }
                        Console.WriteLine();
                    }
                }
            }
        }

        public static void PrintTemporalOffsetInfo(List<Box> rootBoxes)
        {
            Console.WriteLine();
            Box movieBox = BoxHelper.FindBox(rootBoxes, BoxType.MovieBox);
            if (movieBox == null)
            {
                return;
            }

            List<Box> tracks = BoxHelper.FindBoxes(movieBox.Children, BoxType.TrackBox);
            if (tracks.Count > 1)
            {
                Console.WriteLine("Temporal offset (relative to track 1):");
                SampleTableBox sampleTable1 = (SampleTableBox)BoxHelper.FindBoxFromPath(tracks[0].Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
                SampleEntry sampleEntry1 = GetSampleEntry((TrackBox)tracks[0]);
                SampleToChunkBox sampleToChunk1 = (SampleToChunkBox)BoxHelper.FindBox(sampleTable1.Children, BoxType.SampleToChunkBox);
                List<ulong> chunkOffsetList1 = TrackHelper.FindChunkOffsetList(sampleTable1);
                List<SampleToChunkEntry> sampleToChunkList1 = TrackHelper.UncompressSampleToChunkBox(sampleToChunk1, chunkOffsetList1.Count);
                for (int trackIndex = 1; trackIndex < tracks.Count; trackIndex++)
                {
                    SampleTableBox sampleTable2 = (SampleTableBox)BoxHelper.FindBoxFromPath(tracks[trackIndex].Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
                    SampleEntry sampleEntry2 = GetSampleEntry((TrackBox)tracks[trackIndex]);
                    if (sampleEntry2 == null)
                    {
                        continue;
                    }
                    SampleToChunkBox sampleToChunk2 = (SampleToChunkBox)BoxHelper.FindBox(sampleTable2.Children, BoxType.SampleToChunkBox);
                    List<ulong> chunkOffsetList2 = TrackHelper.FindChunkOffsetList(sampleTable2);
                    List<SampleToChunkEntry> sampleToChunkList2 = TrackHelper.UncompressSampleToChunkBox(sampleToChunk2, chunkOffsetList2.Count);
                    int count = Math.Min(chunkOffsetList1.Count, chunkOffsetList2.Count);

                    double track1DurationInSeconds = 0;
                    double track2DurationInSeconds = 0;
                    double maxPositiveOffset = 0;
                    double maxNegativeOffset = 0;
                    for (int chunkIndex = 0; chunkIndex < count - 1; chunkIndex++)
                    {
                        uint sampleCount1 = sampleToChunkList1[chunkIndex].SamplesPerChunk;
                        uint sampleCount2 = sampleToChunkList2[chunkIndex].SamplesPerChunk;
                        track1DurationInSeconds += sampleCount1 * GetSampleDuration(sampleEntry1);
                        track2DurationInSeconds += sampleCount2 * GetSampleDuration(sampleEntry2);
                        if (maxPositiveOffset < track2DurationInSeconds - track1DurationInSeconds)
                        {
                            maxPositiveOffset = track2DurationInSeconds - track1DurationInSeconds;
                        }

                        if (maxNegativeOffset > track2DurationInSeconds - track1DurationInSeconds)
                        {
                            maxNegativeOffset = track2DurationInSeconds - track1DurationInSeconds;
                        }
                    }
                    Console.WriteLine("Track {0}: Max: {1}, Min: {2}", trackIndex + 1, maxPositiveOffset.ToString("0.0000000000"), maxNegativeOffset.ToString("0.0000000000"));
                }
            }
        }

        private static double GetSampleDuration(SampleEntry sampleEntry)
        {
            if (sampleEntry is AudioSampleEntry)
            {
                if (sampleEntry is AC3AudioSampleEntry)
                {
                    return (double)SyncFrame.SampleCount / ((AudioSampleEntry)sampleEntry).SampleRate;
                }
                else if (sampleEntry is MP4AudioSampleEntry)
                {
                    return (double)AdtsFrame.SampleCount / ((AudioSampleEntry)sampleEntry).SampleRate;
                }
            }
            else if (sampleEntry is VisualSampleEntry)
            {
                if (sampleEntry is AVCVisualSampleEntry)
                {
                    AVCDecoderConfigurationRecordBox configuration = (AVCDecoderConfigurationRecordBox)BoxHelper.FindBox(sampleEntry.Children, BoxType.AVCDecoderConfigurationRecordBox);
                    if (configuration.SequenceParameterSetList.Count > 0)
                    {
                        VUIParameters vuiParameters = configuration.SequenceParameterSetList[0].VUIParameters;
                        if (vuiParameters != null && vuiParameters.TimingInfoPresentFlag && vuiParameters.FixedFrameRateFlag)
                        {
                            return 1 / vuiParameters.FrameRate.Value;
                        }
                    }
                }
            }
            throw new NotImplementedException();
        }

        private static SampleEntry GetSampleEntry(TrackBox track)
        {
            TrackHeaderBox trackHeader = (TrackHeaderBox)BoxHelper.FindBox(track.Children, BoxType.TrackHeaderBox);
            HandlerBox handlerBox = (HandlerBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.HandlerReferenceBox);
            SampleDescriptionBox sampleDescription = (SampleDescriptionBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox, BoxType.SampleDescriptionBox);
            if (handlerBox.HandlerType == HandlerType.Audio ||
                handlerBox.HandlerType == HandlerType.Video)
            {
                return sampleDescription.Children[0] as SampleEntry;
            }
            return null;
        }

        public static string ToTimeSpanString(int totalSeconds)
        {
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return String.Format("{0}m {1}s", minutes, seconds);
        }
    }
}
