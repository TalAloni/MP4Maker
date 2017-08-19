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
using MediaFormatLibrary.AC3;
using MediaFormatLibrary.Mpeg2;
using MediaFormatLibrary.MP4;

namespace MediaFormatLibrary
{
    public class AC3MultiplexerInput : IMultiplexerInput
    {
        private AC3Stream m_stream;

        private AudioSampleEntry m_sampleEntry;

        public AC3MultiplexerInput(Stream stream)
        {
            m_stream = new AC3Stream(stream);
        }

        public SampleData ReadSample()
        {
            SyncFrame frame = m_stream.ReadFrame();

            if (frame != null)
            {
                SampleData sampleData = new SampleData();
                sampleData.DurationInTimeUnits = SyncFrame.SampleCount;
                sampleData.DurationInSeconds = (double)SyncFrame.SampleCount / frame.SyncInfo.SampleRate;
                sampleData.IsSyncSample = true;
                sampleData.SampleDelayInTimeUnits = 0;
                sampleData.SampleDelayInSeconds = 0;
                sampleData.RawSampleData = frame.GetBytes();
                return sampleData;
            }
            return null;
        }

        private SyncFrame PeekAtFirstSample()
        {
            long position = m_stream.Position;
            m_stream.Position = 0;
            SyncFrame frame = m_stream.ReadFrame();
            m_stream.Position = position;
            return frame;
        }

        public List<Descriptor> GetMpeg2Descriptors()
        {
            SyncFrame frame = PeekAtFirstSample();

            if (frame != null)
            {
                List<Descriptor> trackDescriptors = new List<Descriptor>();
                RegistrationDescriptor registrationDescriptor = new RegistrationDescriptor();
                registrationDescriptor.FormatIdentifier = FormatIdentifier.AC3;
                trackDescriptors.Add(registrationDescriptor);

                AC3AudioDescriptor ac3Descriptor = new AC3AudioDescriptor();
                ac3Descriptor.SampleRateCode = frame.SyncInfo.FSCod;
                ac3Descriptor.BSID = frame.BSI.BSID;
                ac3Descriptor.BitRateCode = AC3AudioDescriptor.GetBitrateCode(frame.SyncInfo.BitRate);
                ac3Descriptor.NumChannels = (byte)frame.BSI.ACMod;
                ac3Descriptor.LangCode = 0;
                trackDescriptors.Add(ac3Descriptor);

                return trackDescriptors;
            }
            throw new InvalidDataException("Cannot read first sample");
        }

        public Mpeg2.StreamType Mpeg2StreamType
        {
            get
            {
                return Mpeg2.StreamType.DolbyDigital;
            }
        }

        public ElementaryStreamID Mpeg2StreamID
        {
            get
            {
                return ElementaryStreamID.ExtendedStream;
            }
        }

        public byte? Mpeg2StreamIDExtention
        {
            get
            {
                return 0x71;
            }
        }

        public SampleEntry GetMP4SampleEntry()
        {
            if (m_sampleEntry == null)
            {
                SyncFrame frame = PeekAtFirstSample();

                AC3AudioSampleEntry sampleEntry = new AC3AudioSampleEntry();
                sampleEntry.DataReferenceIndex = 1;
                sampleEntry.ChannelCount = (ushort)frame.BSI.NumberOfChannels;
                if (sampleEntry.ChannelCount == 0)
                {
                    throw new NotImplementedException("Unsupported ADTS channel_configuration");
                }
                sampleEntry.SampleRate = frame.SyncInfo.SampleRate;
                sampleEntry.SampleSize = 16;
                AC3SpecificBox ac3SpecificBox = new AC3SpecificBox();
                ac3SpecificBox.FSCod = frame.SyncInfo.FSCod;
                ac3SpecificBox.BSID = frame.BSI.BSID;
                ac3SpecificBox.BSMod = frame.BSI.BSMod;
                ac3SpecificBox.ACMod = frame.BSI.ACMod;
                ac3SpecificBox.LfeOn = frame.BSI.LfeOn;
                ac3SpecificBox.BitRateCode = frame.SyncInfo.FSCod;
                
                sampleEntry.Children.Add(ac3SpecificBox);

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
    }
}
