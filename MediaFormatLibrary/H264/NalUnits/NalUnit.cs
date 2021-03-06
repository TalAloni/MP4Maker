/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.H264
{
    /// <summary>
    /// See: http://codesequoia.wordpress.com/2009/10/18/h-264-stream-structure/
    /// </summary>
    public class NalUnit
    {
        public byte NalRefIdc; // nal_ref_idc
        private NalUnitType m_nalUnitType; // nal_unit_type
        public bool SvcExtensionFlag; // svc_extension_flag
        public NalUnitHeaderSvcExtension SvcExtension;
        public NalUnitHeaderMvcExtension MvcExtension;

        private byte[] m_rawPayload; // payload data, only used for unrecognized NAL units

        protected NalUnit(NalUnitType nalUnitType)
        {
            m_nalUnitType = nalUnitType;
        }

        public NalUnit(MemoryStream stream)
        {
            BitStream bitStream = new BitStream(stream, true);
            ReadHeader(bitStream);
            // We check if the current class is NalUnit and not a class that inherits from NalUnit
            // For implemented NALs we wish to delay the decoding until all variables have been set
            if (this.GetType() == typeof(NalUnit))
            {
                ReadEncodedPayloadBytes(stream);
            }
        }

        private void ReadHeader(BitStream bitStream)
        {
            bool forbiddenZeroBit = bitStream.ReadBoolean();
            NalRefIdc = (byte)bitStream.ReadBits(2);
            m_nalUnitType = (NalUnitType)bitStream.ReadBits(5);

            if (m_nalUnitType == NalUnitType.PrefixNalUnit ||
                m_nalUnitType == NalUnitType.CodedSliceExtension ||
                m_nalUnitType == NalUnitType.CodedSliceExtensionForDepthView)
            {
                SvcExtensionFlag = bitStream.ReadBoolean();
                if (SvcExtensionFlag)
                {
                    SvcExtension = new NalUnitHeaderSvcExtension(bitStream);
                }
                else
                {
                    MvcExtension = new NalUnitHeaderMvcExtension(bitStream);
                }
            }
        }

        /// <summary>
        /// Read the encoded RBSP (raw byte sequence payload) bytes
        /// </summary>
        public void ReadEncodedPayloadBytes(MemoryStream stream)
        {
            byte[] buffer = ByteReader.ReadAllBytes(stream);
            MemoryStream rbspStream = NalUnitHelper.DecodeNalPayloadBytes(buffer);
            ReadDecodedPayloadBytes(new RawBitStream(rbspStream));
        }

        /// <summary>
        /// Read the decoded RBSP (raw byte sequence payload) bytes
        /// </summary>
        virtual public void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            // Implemented NALs should override and use their own storage mechanism
            m_rawPayload = ByteReader.ReadAllBytes(bitStream.BaseStream);
        }

        private void WriteHeader(BitStream bitStream)
        {
            bitStream.WriteBoolean(false); //forbiddenZeroBit
            bitStream.WriteBits(NalRefIdc, 2);
            bitStream.WriteBits((byte)m_nalUnitType, 5);

            if (m_nalUnitType == NalUnitType.PrefixNalUnit ||
                m_nalUnitType == NalUnitType.CodedSliceExtension ||
                m_nalUnitType == NalUnitType.CodedSliceExtensionForDepthView)
            {
                bitStream.WriteBoolean(SvcExtensionFlag);
                if (SvcExtensionFlag)
                {
                    SvcExtension.WriteBits(bitStream);
                }
                else
                {
                    MvcExtension.WriteBits(bitStream);
                }
            }
        }

        /// <summary>
        /// Write the NAL Unit bytes: the NAL header followed by the encoded RBSP (raw byte sequence payload) bytes.
        /// </summary>
        public void WriteBytes(Stream stream)
        {
            BitStream bitStream = new BitStream(stream, true);
            WriteHeader(bitStream);
            WriteEncodedPayloadBytes(stream);
        }

        /// <summary>
        /// Write the encoded RBSP (raw byte sequence payload) bytes
        /// </summary>
        public void WriteEncodedPayloadBytes(Stream stream)
        {
            MemoryStream rbspStream = new MemoryStream();
            WriteRawPayloadBytes(new RawBitStream(rbspStream));
            MemoryStream nalPayloadStream = NalUnitHelper.EncodeNalPayloadBytes(rbspStream.ToArray());
            ByteUtils.CopyStream(nalPayloadStream, stream);
        }

        /// <summary>
        /// Write the RBSP (raw byte sequence payload) bytes
        /// </summary>
        virtual public void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            // Implemented NALs should override and use their own storage mechanism
            ByteWriter.WriteBytes(bitStream.BaseStream, m_rawPayload);
        }

        public byte[] GetNalUnitBytes()
        {
            MemoryStream stream = new MemoryStream();
            WriteBytes(stream);
            return stream.ToArray();
        }

        public NalUnitType NalUnitType
        {
            get
            {
                return m_nalUnitType;
            }
        }

        virtual public int StreamPriority
        {
            get
            {
                switch ((NalUnitType)m_nalUnitType)
                {
                    case NalUnitType.AccessUnitDelimiter:
                        return 0;
                    case NalUnitType.SequenceParameterSet:
                        return 1;
                    case NalUnitType.PictureParameterSet:
                        return 2;
                    default:
                        return 6;
                }
            }
        }

        public bool IsSlice
        {
            get
            {
                return ((uint)m_nalUnitType >= 1 && (uint)m_nalUnitType <= 5);
            }
        }
    }
}
