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
    public class MSNVHelper
    {
        public static AudioProfileEntry GetAudioProfileEntry(MP4AudioSampleEntry mp4aBox)
        {
            ElementaryStreamDescriptorBox esDescriptor = (ElementaryStreamDescriptorBox)BoxHelper.FindBox(mp4aBox.Children, BoxType.ElementaryStreamDescriptorBox);
            if (esDescriptor != null)
            {
                AudioProfileEntry audioProfile = new AudioProfileEntry();
                audioProfile.CodecType = AudioCodecType.Mpeg4Audio;
                audioProfile.AACObjectType = esDescriptor.AudioObjetcType; // Should be AAC-LC (0x02)
                audioProfile.AACProfileLevel = 0x29; // AAC L2
                audioProfile.AvgBitRateKbps = esDescriptor.AvgBitrate / 1000;
                audioProfile.MaxBitRateKbps = esDescriptor.MaxBitrate / 1000;
                audioProfile.SamplingRate = (uint)mp4aBox.SampleRate;
                audioProfile.NumberOfChannels = mp4aBox.ChannelCount;
                return audioProfile;
            }
            return null;
        }

        public static VideoProfileEntry GetVideoProfileEntry(AVCVisualSampleEntry avcBox)
        {
            AVCDecoderConfigurationRecordBox configuration = (AVCDecoderConfigurationRecordBox)BoxHelper.FindBox(avcBox.Children, BoxType.AVCDecoderConfigurationRecordBox);
            if (configuration != null)
            {
                SequenceParameterSet sps = configuration.SequenceParameterSetList[0];
                PictureParameterSet pps = configuration.PictureParameterSetList[0];

                if (sps != null && pps != null)
                {
                    VideoProfileEntry videoProfile = new VideoProfileEntry();
                    videoProfile.CodecType = VideoCodecType.AVC;
                    videoProfile.AVCConfigurationVersion = 1;
                    videoProfile.AVCProfileIndication = configuration.AVCProfileIndication;
                    videoProfile.AVCLevelIndication = configuration.AVCLevelIndication;
                    videoProfile.AvgBitRateKbps = 12000;
                    videoProfile.MaxBitRateKbps = 24000;
                    if (sps.VUIParameters != null && sps.VUIParameters.FrameRate.HasValue)
                    {
                        double frameRate = sps.VUIParameters.FrameRate.Value;
                        videoProfile.AvgFramerate = frameRate;
                        videoProfile.MaxFramerate = frameRate;
                    }
                    else
                    {
                        throw new Exception("H264 VUI timing info is not present");
                    }
                    videoProfile.Width = (ushort)sps.Width;
                    videoProfile.Height = (ushort)sps.Height;
                    videoProfile.SARNominator = 1;
                    videoProfile.SARDenominator = 1;
                    return videoProfile;
                }
            }
            return null;
        }

        public static UserSpecificMetaDataBox GetMovieMetaDataBox()
        {
            UserSpecificMetaDataBox movieUserMTBox = new UserSpecificMetaDataBox();
            MetaDataBox movieMetaDataBox = new MetaDataBox();
            movieUserMTBox.Children.Add(movieMetaDataBox);

            // The value used by the two Sony 720p 3D demo clips is "HMMP Video Encoder version 1.10"
            string writingLibrary = "MP4Maker v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            movieMetaDataBox.Entries.Add(new MetaDataEntry(EntryType.Product, LanguageCode.English, writingLibrary));
            return movieUserMTBox;
        }

        public static UserSpecificMetaDataBox GetTrackMetaDataBox()
        {
            UserSpecificMetaDataBox trackUserMTBox = new UserSpecificMetaDataBox();
            MetaDataBox trackMetaDataBox = new MetaDataBox();
            trackUserMTBox.Children.Add(trackMetaDataBox);

            byte[] trackProperty = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }; // An original or main track
            trackMetaDataBox.Entries.Add(new MetaDataEntry(EntryType.TrackProperty, LanguageCode.Undetermined, trackProperty));
            return trackUserMTBox;
        }

        public static bool IsValidResolution(int width, int height)
        {
            if (width == 1920 && height == 1080)
            {
                return true;
            }
            else if (width == 1440 && height == 1080)
            {
                return true;
            }
            else if (width == 1280 && height == 720)
            {
                return true;
            }
            else if (width == 720 && height == 576)
            {
                return true;
            }
            else if (width == 720 && height == 480)
            {
                return true;
            }
            else if (width == 640 && height == 576)
            {
                return true;
            }
            else if (width == 480 && height == 270)
            {
                return true;
            }
            else if (width == 320 && height == 240)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
