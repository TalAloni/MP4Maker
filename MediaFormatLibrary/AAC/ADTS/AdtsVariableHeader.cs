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

namespace MediaFormatLibrary.AAC
{
    /// <summary>
    /// [ISO/IEC 14496-3] adts_variable_header
    /// </summary>
    public class AdtsVariableHeader
    {
        public bool CopyrightIdentificationBit;   // copyright_identification_bit
        public bool CopyrightIdentificationStart; // copyright_identification_start
        public ushort AacFrameLength;             // 13 bits, aac_frame_length, including the headers and protection
        public ushort AdtsBufferFullness;         // 11 bits, adts_buffer_fullness
        public byte NumberOfRawDataBlocksInFrameMinus1; //  2 bits, number_of_raw_data_blocks_in_frame

        public AdtsVariableHeader()
        {
            AdtsBufferFullness = 0x7FF;
        }

        public AdtsVariableHeader(BitStream stream)
        {
            CopyrightIdentificationBit = stream.ReadBoolean();
            CopyrightIdentificationStart = stream.ReadBoolean();
            AacFrameLength = (ushort)stream.ReadBits(13);
            AdtsBufferFullness = (ushort)stream.ReadBits(11);
            NumberOfRawDataBlocksInFrameMinus1 = (byte)stream.ReadBits(2);
        }

        public void WriteBytes(BitStream stream)
        {
            stream.WriteBoolean(CopyrightIdentificationBit);
            stream.WriteBoolean(CopyrightIdentificationStart);
            stream.WriteBits(AacFrameLength, 13);
            stream.WriteBits(AdtsBufferFullness, 11);
            stream.WriteBits(NumberOfRawDataBlocksInFrameMinus1, 2);
        }
    }
}
