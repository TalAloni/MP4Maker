using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;
using MediaFormatLibrary.H264;
using MediaFormatLibrary.Mpeg2;
using MediaFormatLibrary.MP4;

namespace MediaFormatLibrary
{
    public class H264MultiplexerInput : IMultiplexerInput
    {
        private H264ElementaryStream m_stream;
        private H264ElementaryStreamReader m_streamReader;
        private bool m_isOutOfBandParameterSetDelivery;
        private bool m_appendLengthFieldPrefix;
        private SequenceParameterSet m_sps;
        private PictureParameterSet m_pps;
        private bool m_isMVC;
        private bool m_isFrameSequential3D;

        private VisualSampleEntry m_sampleEntry;
        
        public H264MultiplexerInput(Stream stream)
        {
            m_stream = new H264ElementaryStream(stream);
            m_streamReader = new H264ElementaryStreamReader(m_stream);
        }

        public SampleData ReadSample()
        {
            if (m_sps == null || m_pps == null)
            {
                ReadParameterSets();
            }

            H264AccessUnit accessUnit = m_streamReader.ReadAccessUnitWithTimingInformation();
            if (accessUnit == null)
            {
                return null;
            }

            SampleData sampleData = new SampleData();
            uint durationInTimeUnits = m_sps.VUIParameters.MinimumFrameDurationInTimeScale.Value;
            sampleData.DurationInTimeUnits = durationInTimeUnits;
            sampleData.DurationInSeconds = (double)durationInTimeUnits / m_sps.VUIParameters.TimeScale;
            sampleData.IsSyncSample = accessUnit.IsIDRPicture;
            sampleData.SampleDelayInTimeUnits = (int)(accessUnit.Delay.Value * durationInTimeUnits);
            sampleData.SampleDelayInSeconds = (double)accessUnit.Delay.Value * durationInTimeUnits / m_sps.VUIParameters.TimeScale;
            MemoryStream sampleStream = new MemoryStream();
            for(int index = 0; index < accessUnit.Count; index++)
            {
                MemoryStream nalUnitStream = accessUnit[index];
                NalUnitType nalUnitType = NalUnitHelper.GetNalUnitType(nalUnitStream);
                bool isParameterSet = (nalUnitType == NalUnitType.SequenceParameterSet || nalUnitType == NalUnitType.PictureParameterSet);
                bool skipNalUnit = m_isOutOfBandParameterSetDelivery && isParameterSet;

                if (!skipNalUnit)
                {
                    if (m_appendLengthFieldPrefix)
                    {
                        BigEndianWriter.WriteUInt32(sampleStream, (uint)nalUnitStream.Length);
                    }
                    else
                    {
                        // When any of the following conditions are true, the zero_byte syntax element shall be present:
                        // – the nal_unit_type within the nal_unit( ) is equal to 7 (sequence parameter set) or 8 (picture parameter set).
                        // – the byte stream NAL unit syntax structure contains the first NAL unit of an access unit in decoding order.
                        if (index == 0 || isParameterSet)
                        {
                            sampleStream.WriteByte(0);
                        }
                        sampleStream.WriteByte(0);
                        sampleStream.WriteByte(0);
                        sampleStream.WriteByte(1);
                    }
                    ByteUtils.CopyStream(nalUnitStream, sampleStream);
                }
            }
            sampleData.RawSampleData = sampleStream.ToArray();
            return sampleData;
        }

        private void ReadParameterSets()
        {
            long position = m_stream.Position;
            m_stream.Position = 0;
            // We use a new reader to avoid corrupting the current reader variables
            H264ElementaryStreamReader reader = new H264ElementaryStreamReader(m_stream);
            List<NalUnit> firstAccessUnit = reader.ReadDecodedAccessUnit();
            foreach (NalUnit nalUnit in firstAccessUnit)
            {
                if (nalUnit.NalUnitType == NalUnitType.DependencyRepresentationDelimiter)
                {
                    m_isMVC = true;
                }
                
                if (nalUnit is SequenceParameterSet)
                {
                    m_sps = (SequenceParameterSet)nalUnit;
                }
                else if (nalUnit is PictureParameterSet)
                {
                    m_pps = (PictureParameterSet)nalUnit;
                }
                else if (nalUnit is SEI)
                {
                    List<SEIPayload> payloads = ((SEI)nalUnit).Payloads;
                    foreach (SEIPayload payload in payloads)
                    {
                        if (payload is FramePackingArrangement)
                        {
                            if (((FramePackingArrangement)payload).FramePackingArrangementType == FramePackingArrangementType.FrameSequential)
                            {
                                m_isFrameSequential3D = true;
                            }
                        }
                        else if (payload.PayloadType == SEIPayloadType.MvcScalableNesting)
                        {
                            m_isMVC = true;
                        }
                    }
                }
            }

            if (m_sps == null || m_pps == null)
            {
                throw new MissingParameterSetException("First access unit does not contain SequenceParameterSet or PictureParameterSet");
            }
            m_stream.Position = position;
        }

        public List<Descriptor> GetMpeg2Descriptors()
        {
            if (m_sps == null || m_pps == null)
            {
                ReadParameterSets();
            }

            List<Descriptor> trackDescriptors = new List<Descriptor>();
            RegistrationDescriptor registrationDescriptor = new RegistrationDescriptor();
            registrationDescriptor.FormatIdentifier = FormatIdentifier.HDMV;
            uint additionalIdentification = m_isMVC ? 0xFF20613F : 0xFF1B503F;
            registrationDescriptor.AdditionalIdentificationInfo = BigEndianConverter.GetBytes(additionalIdentification);
            trackDescriptors.Add(registrationDescriptor);

            AVCVideoDescriptor avcDescriptor = new AVCVideoDescriptor();
            avcDescriptor.ProfileIdc = m_sps.ProfileIdc;
            avcDescriptor.ConstraintSet0Flag = m_sps.ConstraintSet0Flag;
            avcDescriptor.ConstraintSet1Flag = m_sps.ConstraintSet1Flag;
            avcDescriptor.ConstraintSet2Flag = m_sps.ConstraintSet2Flag;
            avcDescriptor.LevelIdc = m_sps.LevelIdc;
            avcDescriptor.AVCStillPresent = true;
            trackDescriptors.Add(avcDescriptor);
            return trackDescriptors;
        }

        public Mpeg2.StreamType Mpeg2StreamType
        {
            get
            {
                if (m_isMVC)
                {
                    return Mpeg2.StreamType.MVC1;
                }
                else
                {
                    return Mpeg2.StreamType.H264;
                }
            }
        }

        public ElementaryStreamID Mpeg2StreamID
        {
            get
            {
                return ElementaryStreamID.H264;
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
                if (m_sps == null || m_pps == null)
                {
                    ReadParameterSets();
                }

                AVCVisualSampleEntry sampleEntry = new AVCVisualSampleEntry();
                sampleEntry.CompressorName = "AVC Coding";
                sampleEntry.DataReferenceIndex = 1;
                sampleEntry.Width = (ushort)m_sps.Width;
                sampleEntry.Height = (ushort)m_sps.Height;
                AVCDecoderConfigurationRecordBox decoderConfiguration = new AVCDecoderConfigurationRecordBox();
                decoderConfiguration.SequenceParameterSetList.Add(m_sps);
                decoderConfiguration.PictureParameterSetList.Add(m_pps);
                decoderConfiguration.ConfigurationVersion = 1;
                decoderConfiguration.AVCProfileIndication = m_sps.ProfileIdc;
                decoderConfiguration.AVCProfileCompatibility = GetProfileCompatibility(m_sps);
                decoderConfiguration.AVCLevelIndication = m_sps.LevelIdc;
                decoderConfiguration.LengthSizeMinus1 = 3;
                sampleEntry.Children.Add(decoderConfiguration);
                m_sampleEntry = sampleEntry;
            }
            return m_sampleEntry;
        }

        public ContentType ContentType
        {
            get
            {
                return ContentType.Video;
            }
        }

        public Stream BaseStream
        {
            get
            {
                return m_stream.BaseStream;
            }
        }

        /// <summary>
        /// Sequence and picture parameter sets may, in some applications, be conveyed "out-of-band" using a reliable transport mechanism.
        /// </summary>
        public bool IsOutOfBandParameterSetDelivery
        {
            get
            {
                return m_isOutOfBandParameterSetDelivery;
            }
            set
            {
                m_isOutOfBandParameterSetDelivery = value;
            }
        }

        /// <summary>
        /// The MP4 container format stores H.264 data without start codes.
        /// Instead, each NALU is prefixed by a length field, which gives the length of the NALU in bytes.
        /// The size of the length field can vary, but is typically 1, 2, or 4 bytes.
        /// </summary>
        public bool AppendLengthFieldPrefix
        {
            get
            {
                return m_appendLengthFieldPrefix;
            }
            set
            {
                m_appendLengthFieldPrefix = value;
            }
        }

        public bool IsFrameSequential3D
        {
            get
            {
                if (m_sps == null || m_pps == null)
                {
                    ReadParameterSets();
                }
                return m_isFrameSequential3D;
            }
        }

        public bool IsMvc
        {
            get
            {
                if (m_sps == null || m_pps == null)
                {
                    ReadParameterSets();
                }
                return m_isMVC;
            }
        }

        public static byte GetProfileCompatibility(SequenceParameterSet sps)
        {
            byte result = (byte)(
                Convert.ToByte(sps.ConstraintSet0Flag) << 7 |
                Convert.ToByte(sps.ConstraintSet1Flag) << 6 |
                Convert.ToByte(sps.ConstraintSet2Flag) << 5 |
                Convert.ToByte(sps.ConstraintSet3Flag) << 4 |
                Convert.ToByte(sps.ConstraintSet4Flag) << 3 |
                Convert.ToByte(sps.ConstraintSet5Flag) << 2);
            return result;
        }
    }
}
