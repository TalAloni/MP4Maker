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

namespace MediaFormatLibrary.H264
{
    /// <summary>
    /// Raw byte sequence payload (RBSP) bitstream
    /// See: http://codesequoia.wordpress.com/2009/10/18/h-264-stream-structure/
    /// </summary>
    public class RawBitStream : BitStream
    {
        public RawBitStream() : base(new MemoryStream(), true)
        {
        }

        public RawBitStream(Stream stream) : base(stream, true)
        {
        }

        public RawBitStream(BitStream stream) : base(stream.BaseStream, true, stream.BitOffset)
        {
        }

        public uint ReadExpGolombCodeUnsigned()
        {
            int leadingZeroBits = 0;
            while (!ReadBoolean())
            {
                leadingZeroBits++;
            }

            uint left = 0;
            if (leadingZeroBits > 0)
            {
                left = (uint)ReadBits(leadingZeroBits);
            }
            uint result = (uint)Math.Pow(2, leadingZeroBits) - 1 + left;
            return result;
        }

        public int ReadExpGolombCodeSigned()
        {
            uint result = ReadExpGolombCodeUnsigned();
            if ((result % 2) == 1)
            {
                return (int)((result + 1) / 2);
            }
            else
            {
                return -(int)(result / 2);
            }
        }

        public void WriteExpGolombCodeUnsigned(uint value)
        {
            int leadingZeroBits = H264ByteUtils.GetLeadingZeroBitsExpGolombCodeUnsigned(value);

            if (leadingZeroBits > 0)
            {
                WriteBits(0x00, leadingZeroBits);
            }
            WriteBits(0x01, 1);
            if (leadingZeroBits > 0)
            {
                uint left = Convert.ToUInt32(value - Math.Pow(2, leadingZeroBits) + 1);
                WriteBits(left, leadingZeroBits);
            }
        }

        public void WriteExpGolombCodeSigned(int value)
        {
            uint unsignedValue = H264ByteUtils.GetUnsignedCodeExpGolombCodeSigned(value);
            WriteExpGolombCodeUnsigned(unsignedValue);
        }

        public bool MoreRbspData()
        {
            long currentPositionInBits = this.PositionInBits;
            long currentPosition = this.Position;
            // search for rbsp_stop_one_bit
            // Back one byte at a time, find the last non-zero byte 
            long offset;
            byte value = 0;
            for(offset = this.Length - 1; offset >= currentPosition; offset--)
            {
                this.Position = offset;
                value = ReadByte();
                if (value != 0)
                {
                    break;
                }
            }

            bool moreRbspData;
            if (currentPosition == offset)
            {
                // The rbsp_stop_one_bit is in the same byte as our current position
                // We need to go to bit resolution
                // if there is no more RBDP data, the next bit should be the rbsp_stop_one_bit.
                int bitsToIgnore = (int)((currentPositionInBits + 1) % 8);
                moreRbspData = (value & (0xFF >> bitsToIgnore)) > 0;
            }
            else
            {
                moreRbspData = currentPosition < offset;
            }
            this.PositionInBits = currentPositionInBits;
            return moreRbspData;
        }

        public void WriteRbspTrailingBits()
        {
            int bitCount = 8 - this.BitOffset;
            WriteBoolean(true); // rbsp_stop_one_bit
            for (int index = 0; index < bitCount - 1; index++)
            {
                WriteBoolean(false); // rbsp_alignment_zero_bit
            }
        }
    }
}
