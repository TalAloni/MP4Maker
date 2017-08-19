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
    public class SubsetSequenceParameterSet : SequenceParameterSet
    {
        private byte[] m_rawPayload; // FIXME: payload data, as a temporary measure

        public SubsetSequenceParameterSet() : base(NalUnitType.SubsetSequenceParameterSet)
        {
        }

        public SubsetSequenceParameterSet(MemoryStream stream) : base(stream)
        {
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            base.ReadDecodedPayloadBytes(bitStream);
            // handle subset data

            bitStream.Position = 0;
            m_rawPayload = ByteReader.ReadAllBytes(bitStream.BaseStream);
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            // WriteSequenceParameterSetData(bitStream);
            // handle subset data
            // bitStream.WriteRbspTrailingBits();

            ByteWriter.WriteBytes(bitStream.BaseStream, m_rawPayload);
        }
    }
}
