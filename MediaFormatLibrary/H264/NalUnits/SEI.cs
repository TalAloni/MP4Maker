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
    /// Supplemental enhancement information
    /// </summary>
    public class SEI : NalUnit
    {
        public List<SEIPayload> Payloads = new List<SEIPayload>();

        public SequenceParameterSetList m_spsList;

        public SEI(SequenceParameterSetList spsList) : base(NalUnitType.SEI)
        {
            m_spsList = spsList;
        }

        public SEI(MemoryStream stream, SequenceParameterSetList spsList) : base(stream)
        {
            m_spsList = spsList;
            ReadEncodedPayloadBytes(stream);
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            do
            {
                // Read payload_type
                uint payload_type = 0;
                byte value = bitStream.ReadByte();
                while (value == 0xFF)
                {
                    payload_type += 0xFF;
                    value = bitStream.ReadByte();
                }
                payload_type += value;

                // Read payload_size
                uint payload_size = 0;
                value = bitStream.ReadByte();
                while (value == 0xFF)
                {
                    payload_size += 0xFF;
                    value = bitStream.ReadByte();
                }
                payload_size += value;

                // Read payload bytes
                byte[] payloadData = new byte[payload_size];
                for (int index = 0; index < payload_size; index++)
                {
                    payloadData[index] = bitStream.ReadByte();
                }

                SEIPayload payload = SEIPayloadHelper.GetSEIPayload((SEIPayloadType)payload_type, payloadData, m_spsList);
                Payloads.Add(payload);
            }
            while (bitStream.MoreRbspData());
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            foreach (SEIPayload payload in Payloads)
            {
                // Write payload_type
                uint payloadType = (uint)payload.PayloadType;
                while (payloadType >= 0xFF)
                {
                    bitStream.WriteBits(0xFF, 8);
                    payloadType -= 0xFF;
                }
                bitStream.WriteByte((byte)payloadType);

                RawBitStream payloadStream = new RawBitStream();
                payload.WriteBytes(payloadStream);
                payloadStream.Position = 0;
                // Write payload_size
                uint payloadSize = (uint)payloadStream.Length;
                while (payloadSize >= 0xFF)
                {
                    bitStream.WriteByte(0xFF);
                    payloadSize -= 0xFF;
                }
                bitStream.WriteByte((byte)payloadSize);
                
                // stream and payloadStream are byte-aligned so it's safe to use CopyStream
                // Write payload bytes:
                ByteUtils.CopyStream(payloadStream.BaseStream, bitStream.BaseStream);
            }
            bitStream.WriteRbspTrailingBits();
        }
        
        public override int StreamPriority
        {
            get
            {
                switch ((SEIPayloadType)Payloads[0].PayloadType)
                {
                    case SEIPayloadType.BufferingPeriod:
                        return 3;
                    case SEIPayloadType.PicTiming:
                        return 4;
                    default:
                        return 5;
                }
            }
        }
    }
}
