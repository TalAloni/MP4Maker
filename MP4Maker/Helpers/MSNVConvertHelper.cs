/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using MediaFormatLibrary.H264;
using MediaFormatLibrary.MP4;
using Utilities;

namespace MP4Maker
{
    public class MSNVConvertHelper
    {
        public static void RemoveUnnecessaryBoxes(List<Box> rootBoxes)
        {
            BoxHelper.RemoveRecursively(rootBoxes, BoxType.InitialObjectDescriptorBox);
            BoxHelper.RemoveRecursively(rootBoxes, BoxType.BitRateBox);
            BoxHelper.RemoveRecursively(rootBoxes, BoxType.FreeSpaceBox);
            BoxHelper.RemoveRecursively(rootBoxes, BoxType.ItemListBox);
            BoxHelper.RemoveRecursively(rootBoxes, BoxType.UserDataBox);
        }

        public static AudioProfileEntry GetAudioProfile(TrackBox audioTrack)
        {
            SampleTableBox sampleTableBox = (SampleTableBox)BoxHelper.FindBoxFromPath(audioTrack.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
            MP4AudioSampleEntry mp4aBox = (MP4AudioSampleEntry)BoxHelper.FindBoxFromPath(sampleTableBox.Children, BoxType.SampleDescriptionBox, BoxType.MP4AudioSampleEntry);
            if (mp4aBox != null)
            {
                return MSNVHelper.GetAudioProfileEntry(mp4aBox);
            }
            return null;
        }

        public static VideoProfileEntry GetVideoProfile(TrackBox videoTrack)
        {
            SampleTableBox sampleTableBox = (SampleTableBox)BoxHelper.FindBoxFromPath(videoTrack.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
            AVCVisualSampleEntry avcBox = (AVCVisualSampleEntry)BoxHelper.FindBoxFromPath(sampleTableBox.Children, BoxType.SampleDescriptionBox, BoxType.AVCVisualSampleEntry);
            if (avcBox != null)
            {
                return MSNVHelper.GetVideoProfileEntry(avcBox);
            }
            return null;
        }

        public static void UpdateTracksAndProfiles(List<Box> rootBoxes)
        {
            Box movieBox = BoxHelper.FindBox(rootBoxes, BoxType.MovieBox);
            List<Box> tracks = BoxHelper.FindBoxes(movieBox.Children, BoxType.TrackBox);

            UserSpecificMetaDataBox trackUserMTBox = MSNVHelper.GetTrackMetaDataBox();

            ProfileBox profileBox = new ProfileBox();
            FileGolbalProfileEntry fileProfile = new FileGolbalProfileEntry();
            profileBox.Children.Add(fileProfile);

            // Sony demo clips has the AudioProfileBox before the VideoProfileBox,
            // It's not necessary, but makes for easier comparisons
            int audioEntryInsertIndex = 1;

            for (int index = 0; index < tracks.Count; index++)
            {
                TrackBox track = (TrackBox)tracks[index];
                HandlerBox handlerBox = (HandlerBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.HandlerReferenceBox);
                if (handlerBox.HandlerType == HandlerType.Audio)
                {
                    AudioProfileEntry audioProfile = GetAudioProfile(track);
                    if (audioProfile != null)
                    {
                        audioProfile.TrackID = (uint)index + 1;
                        profileBox.Children.Insert(audioEntryInsertIndex, audioProfile);
                        audioEntryInsertIndex++;
                    }
                }
                else if (handlerBox.HandlerType == HandlerType.Video)
                {
                    VideoProfileEntry videoProfile = GetVideoProfile(track);
                    if (videoProfile != null)
                    {
                        videoProfile.TrackID = (uint)index + 1;
                        profileBox.Children.Add(videoProfile);
                    }
                }
                BoxHelper.UpdateUserBox(track, trackUserMTBox);
            }

            int profileBoxIndex = BoxHelper.IndexOfUserBox(rootBoxes, profileBox.UserType);
            if (profileBoxIndex >= 0)
            {
                rootBoxes.RemoveAt(profileBoxIndex);
            }
            rootBoxes.Insert(1, profileBox);
        }

        public static void UpdateMSNVBoxes(List<Box> rootBoxes)
        {
            FileTypeBox fileTypeBox = (FileTypeBox)BoxHelper.FindBox(rootBoxes, BoxType.FileTypeBox);
            fileTypeBox.MajorBrand = FileBrand.MSNV;
            if (!fileTypeBox.CompatibleBrands.Contains(FileBrand.MSNV))
            {
                fileTypeBox.CompatibleBrands.Insert(0, FileBrand.MSNV);
            }

            UpdateTracksAndProfiles(rootBoxes);
            Box movieBox = BoxHelper.FindBox(rootBoxes, BoxType.MovieBox);

            UserSpecificMetaDataBox movieUserMTBox = MSNVHelper.GetMovieMetaDataBox();

            BoxHelper.UpdateUserBox(movieBox, movieUserMTBox);
        }

        public static void UpdateChunkOffsets(List<Box> rootBoxes, long mediaDataShift)
        {
            Box movieBox = BoxHelper.FindBox(rootBoxes, BoxType.MovieBox);
            List<Box> tracks = BoxHelper.FindBoxes(movieBox.Children, BoxType.TrackBox);
            foreach (Box track in tracks)
            {
                SampleTableBox sampleTable = (SampleTableBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
                List<ulong> chunkOffsetList = TrackHelper.FindChunkOffsetList(sampleTable);
                for(int index = 0; index < chunkOffsetList.Count; index++)
                {
                    chunkOffsetList[index] = (ulong)((long)chunkOffsetList[index] + mediaDataShift);
                }

                Box chunkOffsetBox = TrackHelper.CreateChunkOffsetBox(chunkOffsetList);
                BoxHelper.RemoveAll(sampleTable.Children, BoxType.ChunkOffsetBox);
                BoxHelper.RemoveAll(sampleTable.Children, BoxType.ChunkLargeOffsetBox);

                // [IEC/TS 62592] "The boxes within the Sample Table Box should be in the following order:
                // Sample Description, Decoding Time to Sample, Composition Time to Sample,
                // Sample to Chunk, Sample Size, Chunk Offset, and Sync Sample"
                int insertIndex = BoxHelper.IndexOfBox(sampleTable.Children, BoxType.SampleSizeBox);
                sampleTable.Children.Insert(insertIndex + 1, chunkOffsetBox);
            }
        }
    }
}
