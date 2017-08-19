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
using MediaFormatLibrary.AAC;
using MediaFormatLibrary.Mpeg2;
using MediaFormatLibrary.MP4;

namespace MediaFormatLibrary
{
    public class AACMultiplexerInput : IMultiplexerInput
    {
        private AudioDataTransportStream m_stream;
        private int m_sampleRate;
        private List<byte[]> m_queue = new List<byte[]>();
        private bool m_isRawDataDelivery;

        private AudioSampleEntry m_sampleEntry;

        public AACMultiplexerInput(Stream stream)
        {
            m_stream = new AudioDataTransportStream(stream);
        }

        public SampleData ReadSample()
        {
            SampleData sampleData = new SampleData();
            sampleData.DurationInTimeUnits = AdtsFrame.SampleCount;
            sampleData.IsSyncSample = true;
            sampleData.SampleDelayInTimeUnits = 0;
            sampleData.SampleDelayInSeconds = 0;

            if (m_queue.Count > 0)
            {
                sampleData.DurationInSeconds = (double)AdtsFrame.SampleCount / m_sampleRate;
                sampleData.RawSampleData = m_queue[0];
                m_queue.RemoveAt(0);
                return sampleData;
            }

            AdtsFrame frame = m_stream.ReadFrame();
            if (frame != null)
            {
                m_sampleRate = frame.FixedHeader.SamplingFrequency;
                if (m_isRawDataDelivery)
                {
                    sampleData.DurationInSeconds = (double)AdtsFrame.SampleCount / m_sampleRate;
                    for (int index = 1; index < frame.RawDataBlocks.Count; index++)
                    {
                        m_queue.Add(frame.RawDataBlocks[index]);
                    }
                    sampleData.RawSampleData = frame.RawDataBlocks[0];
                }
                else
                {
                    sampleData.DurationInSeconds = (double)(frame.RawDataBlocks.Count * AdtsFrame.SampleCount) / m_sampleRate;
                    sampleData.RawSampleData = frame.GetBytes();
                }
                return sampleData;
            }
            return null;
        }

        public List<Descriptor> GetMpeg2Descriptors()
        {
            return new List<Descriptor>();
        }

        public Mpeg2.StreamType Mpeg2StreamType
        {
            get
            {
                return Mpeg2.StreamType.Mpeg2ADTSAAC;
            }
        }

        public ElementaryStreamID Mpeg2StreamID
        {
            get
            {
                return ElementaryStreamID.Mpeg2AudioStream1;
            }
        }

        public byte? Mpeg2StreamIDExtention
        {
            get
            {
                return null;
            }
        }

        public SampleEntry GetMP4SampleEntry()
        {
            if (m_sampleEntry == null)
            {
                long position = m_stream.Position;
                m_stream.Position = 0;
                AdtsFrame frame = m_stream.ReadFrame();
                m_stream.Position = position;
                
                MP4AudioSampleEntry sampleEntry = new MP4AudioSampleEntry();
                sampleEntry.DataReferenceIndex = 1;
                sampleEntry.ChannelCount = (ushort)frame.FixedHeader.NumberOfChannels;
                if (sampleEntry.ChannelCount == 0)
                {
                    throw new NotImplementedException("Unsupported ADTS channel_configuration");
                }
                sampleEntry.SampleRate = frame.FixedHeader.SamplingFrequency;
                sampleEntry.SampleSize = 16;
                ElementaryStreamDescriptorBox elementaryStreamDescriptor = new ElementaryStreamDescriptorBox();
                elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor = new DecoderConfigDescriptor();
                ADTSProfile profile = frame.FixedHeader.AudioProfile;
                if (profile == ADTSProfile.AACMain || profile == ADTSProfile.AACLC || profile == ADTSProfile.AACSSR || profile == ADTSProfile.AACLTP)
                {
                    elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.ObjectTypeIndication = ObjectTypeIndication.Mpeg4AAC;
                }
                else if (profile == ADTSProfile.Mpeg2Main)
                {
                    elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.ObjectTypeIndication = ObjectTypeIndication.Mpeg2AACMain;
                }
                else if (profile == ADTSProfile.Mpeg2LC)
                {
                    elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.ObjectTypeIndication = ObjectTypeIndication.Mpeg2AACLC;
                }
                else if (profile == ADTSProfile.Mpeg2SSR)
                {
                    elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.ObjectTypeIndication = ObjectTypeIndication.Mpeg2AACSSR;
                }
                else // Mpeg2Reserved
                {
                    throw new NotImplementedException("Unsupported ADTS profile");
                }
                elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.BufferSizeDB = 1536;
                elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.StreamType = MP4.StreamType.AudioStream;
                elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.Reserved = true; // FIXME
                elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.AvgBitRate = 256000; // FIXME
                elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.MaxBitRate = 384000; // FIXME
                if (profile == ADTSProfile.AACMain || profile == ADTSProfile.AACLC || profile == ADTSProfile.AACSSR || profile == ADTSProfile.AACLTP)
                {
                    // when DecoderConfigDescriptor.objectTypeIndication refers to streams complying with ISO/IEC 14496-3 [..] the existence of AudioSpecificConfig() is mandatory
                    AudioSpecificConfig specificConfig = new AudioSpecificConfig();
                    if (profile == ADTSProfile.AACMain)
                    {
                        specificConfig.AudioObjectType = AudioObjectType.AACMain;
                    }
                    else if (profile == ADTSProfile.AACLC)
                    {
                        specificConfig.AudioObjectType = AudioObjectType.AACLC;
                    }
                    else if (profile == ADTSProfile.AACSSR)
                    {
                        specificConfig.AudioObjectType = AudioObjectType.AACSSR;
                    }
                    else
                    {
                        specificConfig.AudioObjectType = AudioObjectType.AACLTP;
                    }
                    specificConfig.ChannelConfiguration = frame.FixedHeader.ChannelConfiguration;
                    specificConfig.SamplingFrequencyIndex = frame.FixedHeader.SamplingFrequencyIndex;
                    elementaryStreamDescriptor.ESDescriptor.DecoderConfigDescriptor.DecSpecificInfo.Add(specificConfig);

                    elementaryStreamDescriptor.ESDescriptor.SLConfigDescriptor = new SLConfigDescriptor();
                    elementaryStreamDescriptor.ESDescriptor.SLConfigDescriptor.SLValue = new byte[] { 0x02 }; // As specified in IEC/TS 62592

                    sampleEntry.Children.Add(elementaryStreamDescriptor);
                }

                m_sampleEntry = sampleEntry;
            }
            return m_sampleEntry;
        }

        public ContentType ContentType
        {
            get
            {
                return ContentType.Audio;
            }
        }

        public Stream BaseStream
        {
            get
            {
                return m_stream.BaseStream;
            }
        }

        public bool IsRawDataDelivery
        {
            get
            {
                return m_isRawDataDelivery;
            }
            set
            {
                m_isRawDataDelivery = value;
            }
        }
    }
}
