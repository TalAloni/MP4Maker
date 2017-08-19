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
using MediaFormatLibrary.AAC;
using MediaFormatLibrary.H264;
using MediaFormatLibrary.MP4;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    public class DemuxHelper
    {
        public static string GetTrackFileExtention(TrackBox track)
        {
            SampleTableBox sampleTable = (SampleTableBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
            SampleDescriptionBox sampleDescriptionBox = (SampleDescriptionBox)BoxHelper.FindBox(sampleTable.Children, BoxType.SampleDescriptionBox);

            string extenstion = "unk";
            if (sampleDescriptionBox != null && sampleDescriptionBox.Children.Count > 0)
            {
                if (sampleDescriptionBox.Children[0] is AVCVisualSampleEntry)
                {
                    extenstion = "h264";
                }
                else if (sampleDescriptionBox.Children[0] is MP4AudioSampleEntry)
                {
                    extenstion = "aac";
                }
                else if (sampleDescriptionBox.Children[0] is AC3AudioSampleEntry)
                {
                    extenstion = "ac3";
                }
            }
            return extenstion;
        }

        public static void DemuxTrack(TrackBox track, Stream inputStream, Stream trackStream)
        {
            SampleTableBox sampleTable = (SampleTableBox)BoxHelper.FindBoxFromPath(track.Children, BoxType.MediaBox, BoxType.MediaInformationBox, BoxType.SampleTableBox);
            SampleSizeBox sampleSizeBox = (SampleSizeBox)BoxHelper.FindBox(sampleTable.Children, BoxType.SampleSizeBox);
            SampleToChunkBox sampleToChunk = (SampleToChunkBox)BoxHelper.FindBox(sampleTable.Children, BoxType.SampleToChunkBox);
            List<ulong> chunkOffsetList = TrackHelper.FindChunkOffsetList(sampleTable);
            List<long> chunkSizeList = TrackHelper.GetChunkSizeList(sampleSizeBox, sampleToChunk, chunkOffsetList.Count);
            SampleDescriptionBox sampleDescriptionBox = (SampleDescriptionBox)BoxHelper.FindBox(sampleTable.Children, BoxType.SampleDescriptionBox);

            SampleEntry sampleEntry = (SampleEntry)sampleDescriptionBox.Children[0];

            if (sampleEntry is AVCVisualSampleEntry)
            {
                // The SPS and PPS NAL units are stored in the AVCDecoderConfigurationRecordBox
                // During demux, we should inserted them to the beginning of the stream.
                AVCVisualSampleEntry avcBox = (AVCVisualSampleEntry)sampleEntry;
                AVCDecoderConfigurationRecordBox configuration = (AVCDecoderConfigurationRecordBox)BoxHelper.FindBox(avcBox.Children, BoxType.AVCDecoderConfigurationRecordBox);
                H264ElementaryStreamWriter avcStream = new H264ElementaryStreamWriter(trackStream);
                avcStream.WriteNalUnit(configuration.SequenceParameterSetList[0]);
                avcStream.WriteNalUnit(configuration.PictureParameterSetList[0]);
            }

            int samplesIndex = 0;
            for (int chunkIndex = 0; chunkIndex < chunkOffsetList.Count; chunkIndex++)
            {
                inputStream.Seek((long)chunkOffsetList[chunkIndex], SeekOrigin.Begin);

                if (chunkSizeList[chunkIndex] > Int32.MaxValue)
                {
                    throw new NotImplementedException("Cannot read chunk larger than 2GB");
                }

                int chunkSize = (int)chunkSizeList[chunkIndex];

                byte[] buffer = new byte[chunkSize];
                inputStream.Read(buffer, 0, chunkSize);

                if (sampleEntry is AVCVisualSampleEntry)
                {
                    AVCVisualSampleEntry avcBox = (AVCVisualSampleEntry)sampleEntry;
                    AVCDecoderConfigurationRecordBox avcConfiguration = (AVCDecoderConfigurationRecordBox)BoxHelper.FindBox(avcBox.Children, BoxType.AVCDecoderConfigurationRecordBox);
                    if (avcConfiguration != null)
                    {
                        int offset = 0;
                        while (offset < buffer.Length)
                        {
                            // MP4 (ISO/IEC 14496-12) stream, "mdat" payload carries NAL units in length-data format (i.e. [LengthOfNalUnit1][NalUnit1][LengthOfNalUnit2][NalUnit2])
                            // The size of the length field is signaled in AVCDecoderConfigurationRecord.LengthSizeMinusOne and it can be 1, 2, or 4 bytes.
                            // See ISO/IEC 14496-15 section 5.2.3 for the detail.
                            uint nalUnitSize;
                            if (avcConfiguration.LengthSizeMinus1 + 1 == 1)
                            {
                                nalUnitSize = ByteReader.ReadByte(buffer, offset);
                            }
                            else if (avcConfiguration.LengthSizeMinus1 + 1 == 1)
                            {
                                nalUnitSize = BigEndianConverter.ToUInt16(buffer, offset);
                            }
                            else
                            {
                                nalUnitSize = BigEndianConverter.ToUInt32(buffer, offset);
                            }
                            /* We can optimize and compact to 3 bytes where possible */
                            BigEndianWriter.WriteUInt32(trackStream, 0x00000001);
                            trackStream.Write(buffer, offset + (int)(avcConfiguration.LengthSizeMinus1 + 1), (int)nalUnitSize);
                            offset += (avcConfiguration.LengthSizeMinus1 + 1) + (int)nalUnitSize;
                        }
                    }
                }
                else if (sampleEntry is MP4AudioSampleEntry)
                {
                    MP4AudioSampleEntry mp4aEntry = (MP4AudioSampleEntry)sampleEntry;
                    ElementaryStreamDescriptorBox esDescriptor = (ElementaryStreamDescriptorBox)BoxHelper.FindBox(mp4aEntry.Children, BoxType.ElementaryStreamDescriptorBox);
                    int sampleOffset = 0;

                    while(sampleOffset < buffer.Length)
                    {
                        uint sampleSize = (sampleSizeBox.SampleSize > 0) ? sampleSizeBox.SampleSize : sampleSizeBox.Entries[samplesIndex];
                        byte[] sampleRawData = ByteReader.ReadBytes(buffer, sampleOffset, (int)sampleSize);
                        sampleOffset += (int)sampleSize;
                        samplesIndex++;

                        // Write ADTS fixed header and ADTS variable header
                        MP4AudioSampleEntry aacEntry = (MP4AudioSampleEntry)sampleEntry;
                        AdtsFrame adtsFrame = new AdtsFrame();
                        if (esDescriptor.AudioObjetcType == AudioObjectType.AACMain)
                        {
                            adtsFrame.FixedHeader.AudioProfile = ADTSProfile.AACMain;
                        }
                        else if (esDescriptor.AudioObjetcType == AudioObjectType.AACLC)
                        {
                            adtsFrame.FixedHeader.AudioProfile = ADTSProfile.AACLC;
                        }
                        else if (esDescriptor.AudioObjetcType == AudioObjectType.AACSSR)
                        {
                            adtsFrame.FixedHeader.AudioProfile = ADTSProfile.AACSSR;
                        }
                        else if (esDescriptor.AudioObjetcType == AudioObjectType.AACLTP)
                        {
                            adtsFrame.FixedHeader.AudioProfile = ADTSProfile.AACLTP;
                        }
                        adtsFrame.FixedHeader.Layer = 0;
                        adtsFrame.FixedHeader.ProtectionAbsent = true;
                        adtsFrame.FixedHeader.SamplingFrequencyIndex = AdtsFixedHeader.GetSamplingFrequencyIndex((int)mp4aEntry.SampleRate).Value;
                        adtsFrame.FixedHeader.PrivateBit = false;
                        adtsFrame.FixedHeader.ChannelConfiguration = (byte)aacEntry.ChannelCount;
                        adtsFrame.FixedHeader.OriginalCopy = false;
                        adtsFrame.VariableHeader.CopyrightIdentificationBit = false;
                        adtsFrame.VariableHeader.CopyrightIdentificationStart = false;
                        
                        adtsFrame.RawDataBlocks.Add(sampleRawData);
                        adtsFrame.WriteBytes(trackStream);
                    }
                }
                else
                {
                    trackStream.Write(buffer, 0, chunkSize);
                }
            }
        }
    }
}
