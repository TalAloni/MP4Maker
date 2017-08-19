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
    public class SEIPayload
    {
        private SEIPayloadType m_payloadType; // payload_type

        private byte[] m_payloadBytes; // payload bytes, only used for unrecognized payloads

        public SEIPayload(SEIPayloadType type)
        {
            m_payloadType = type;
        }

        public SEIPayload(SEIPayloadType type, RawBitStream bitStream)
        {
            m_payloadType = type;
            // For implemented payloads we wish to delay the decoding until all variables have been set
            if (this.GetType() == typeof(SEIPayload)) // We check if the current class is SEIPayload and not a class that inherits from SEIPayload
            {
                ReadBits(bitStream);
            }
        }

        virtual public void ReadBits(RawBitStream bitStream)
        {
            // Implemented SEI payloads should override and use their own storage mechanism
            m_payloadBytes = ByteReader.ReadAllBytes(bitStream.BaseStream);
        }

        virtual public void WriteBits(RawBitStream bitStream)
        {
            // Implemented SEI payloads should override and use their own storage mechanism
            ByteWriter.WriteBytes(bitStream.BaseStream, m_payloadBytes);
        }

        public void WriteBytes(RawBitStream bitStream)
        {
            WriteBits(bitStream);
            WriteTrailingBits(bitStream);
        }

        public void WriteTrailingBits(RawBitStream bitStream)
        {
            if (bitStream.BitOffset != 0)
            {
                bitStream.WriteRbspTrailingBits();
            }
        }
        
        public SEIPayloadType PayloadType
        {
            get
            {
                return m_payloadType;
            }
        }
    }
}
