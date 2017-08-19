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

namespace MediaFormatLibrary.H264
{
    /// <summary>
    /// Hypothetical reference decoder parameters
    /// </summary>
    public class HRDParameters
    {
        public uint CpbCntMinus1; // cpb_cnt_minus1
        public byte BitRateScale; // bit_rate_scale
        public byte CpbSizeScale; // cpb_size_scale
        public uint[] BitRateValueMinus1; // bit_rate_value_minus1
        public uint[] CpbSizeValueMinus1; // cpb_size_value_minus1
        public bool[] CbrFlag; // cbr_flag
        public byte InitialCpbRemovalDelayLengthMinus1; // initial_cpb_removal_delay_length_minus1
        public byte CpbRemovalDelayLengthMinus1; // cpb_removal_delay_length_minus1
        public byte DpbOutputDelayLengthMinus1; // dpb_output_delay_length_minus1
        public byte TimeOffsetLength; // time_offset_length

        public HRDParameters()
        {
        }

        public HRDParameters(RawBitStream bitStream)
        {
            CpbCntMinus1 = bitStream.ReadExpGolombCodeUnsigned();
            BitRateScale = (byte)bitStream.ReadBits(4);
            CpbSizeScale = (byte)bitStream.ReadBits(4);
            BitRateValueMinus1 = new uint[CpbCntMinus1 + 1];
            CpbSizeValueMinus1 = new uint[CpbCntMinus1 + 1];
            CbrFlag = new bool[CpbCntMinus1 + 1];
            for (int index = 0; index < CpbCntMinus1 + 1; index++)
            {
                BitRateValueMinus1[index] = bitStream.ReadExpGolombCodeUnsigned();
                CpbSizeValueMinus1[index] = bitStream.ReadExpGolombCodeUnsigned();
                CbrFlag[index] = bitStream.ReadBoolean();
            }

            InitialCpbRemovalDelayLengthMinus1 = (byte)bitStream.ReadBits(5);
            CpbRemovalDelayLengthMinus1 = (byte)bitStream.ReadBits(5);
            DpbOutputDelayLengthMinus1 = (byte)bitStream.ReadBits(5);
            TimeOffsetLength = (byte)bitStream.ReadBits(5);
        }

        public void WriteBits(RawBitStream bitStream)
        {
            bitStream.WriteExpGolombCodeUnsigned(CpbCntMinus1);
            bitStream.WriteBits(BitRateScale, 4);
            bitStream.WriteBits(CpbSizeScale, 4);

            for (int index = 0; index < CpbCntMinus1 + 1; index++)
            {
                bitStream.WriteExpGolombCodeUnsigned(BitRateValueMinus1[index]);
                bitStream.WriteExpGolombCodeUnsigned(CpbSizeValueMinus1[index]);
                bitStream.WriteBoolean(CbrFlag[index]);
            }

            bitStream.WriteBits(InitialCpbRemovalDelayLengthMinus1, 5);
            bitStream.WriteBits(CpbRemovalDelayLengthMinus1, 5);
            bitStream.WriteBits(DpbOutputDelayLengthMinus1, 5);
            bitStream.WriteBits(TimeOffsetLength, 5);
        }

        /// <summary>
        /// Maximum Bitrate, Get Bitrate, Set Bitrate and scale
        /// </summary>
        [Obsolete]
        public uint[] BitRate
        {
            get
            {
                uint[] result = new uint[CpbCntMinus1 + 1];
                for (int index = 0; index < CpbCntMinus1 + 1; index++)
                {
                    result[index] = (BitRateValueMinus1[index] + 1) * (uint)Math.Pow(2, 6 + BitRateScale);
                }
                return result;
            }
            set
            {
                byte scale;
                BitRateValueMinus1 = CalculateCpbValuesMinusOne(value, 6, out scale);
                CpbSizeScale = scale;
            }
        }

        /// <summary>
        /// CpbSize, Get CpbSize, Set CpbSize and scale
        /// </summary>
        [Obsolete]
        public uint[] CpbSize
        {
            get
            {
                uint[] result = new uint[CpbCntMinus1 + 1];
                for (int index = 0; index < CpbCntMinus1 + 1; index++)
                {
                    result[index] = (CpbSizeValueMinus1[index] + 1) * (uint)Math.Pow(2, 4 + CpbSizeScale);
                }
                return result;
            }
            set
            {
                byte scale;
                CpbSizeValueMinus1 = CalculateCpbValuesMinusOne(value, 4, out scale);
                CpbSizeScale = scale;
            }
        }

        [Obsolete]
        private uint[] CalculateCpbValuesMinusOne(uint[] values, int scaleOffset, out byte scale)
        {
            uint minimum = uint.MaxValue;
            for (int index = 0; index < CpbCntMinus1 + 1; index++)
            {
                if (values[index] < minimum)
                {
                    minimum = values[index];
                }
            }

            scale = 0;
            minimum = minimum >> scaleOffset;
            while (((minimum >> 1) - 1) > 0
                && minimum % 2 == 0
                && scale < 15)
            {
                scale++;
                minimum = minimum >> 1;
            }

            uint[] result = new uint[CpbCntMinus1 + 1];
            for (int index = 0; index < CpbCntMinus1 + 1; index++)
            {
                result[index] = ((values[index]) >> (int)(scaleOffset + scale)) - 1;
            }

            return result;
        }
    }
}
