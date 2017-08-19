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
    /// [ISO/IEC 14496-3] adts_frame
    /// </summary>
    public class AdtsFrame
    {
        // FIXME:
        // MPEG-2 AAC (ISO/IEC 13818-7): "raw_data_block() always contains data representing 1024 output samples".
        // MPEG-4 AAC: It's an acceptable to assume an ADTS frame will have 1024 samples per frame, but 960 is possible too.
        public const int SampleCount = 1024;

        public AdtsFixedHeader FixedHeader;
        public AdtsVariableHeader VariableHeader;
        public List<byte[]> RawDataBlocks = new List<byte[]>();

        public AdtsFrame()
        {
            FixedHeader = new AdtsFixedHeader();
            VariableHeader = new AdtsVariableHeader();
        }

        public AdtsFrame(Stream stream)
        {
            BitStream bitStream = new BitStream(stream, true);
            FixedHeader = new AdtsFixedHeader(bitStream);
            VariableHeader = new AdtsVariableHeader(bitStream);
            if (VariableHeader.NumberOfRawDataBlocksInFrameMinus1 == 0)
            {
                // We are now byte aligned, we can use stream
                if (FixedHeader.ProtectionAbsent == false)
                {
                    ushort crc = BigEndianReader.ReadUInt16(stream);
                }
                int rawDataBlockLength = VariableHeader.AacFrameLength - FixedHeader.Length;
                byte[] rawDataBlock = ByteReader.ReadBytes(stream, rawDataBlockLength);
                RawDataBlocks.Add(rawDataBlock);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void WriteBytes(Stream stream)
        {
            if (VariableHeader.NumberOfRawDataBlocksInFrameMinus1 != 0)
            {
                throw new NotImplementedException();
            }

            VariableHeader.AacFrameLength = (ushort)(RawDataBlocks[0].Length + (FixedHeader.ProtectionAbsent == true ? 7 : 9));
            BitStream bitStream = new BitStream(stream, true);
            FixedHeader.WriteBytes(bitStream);
            VariableHeader.WriteBytes(bitStream);
            // We are now byte aligned, we can use stream
            if (FixedHeader.ProtectionAbsent == false)
            {
                ushort crc = 0; // FIXME, calculate CRC
                BigEndianWriter.WriteUInt16(stream, crc);
            }
            stream.Write(RawDataBlocks[0], 0, RawDataBlocks[0].Length);
        }

        public byte[] GetBytes()
        {
            MemoryStream stream = new MemoryStream();
            WriteBytes(stream);
            return stream.ToArray();
        }
    }
}
