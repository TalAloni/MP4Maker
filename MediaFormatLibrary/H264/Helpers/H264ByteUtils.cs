/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFormatLibrary.H264
{
    public class H264ByteUtils
    {
        public static int GetLeadingZeroBitsExpGolombCodeUnsigned(uint value)
        {
            int leadingZeroBits = 0;

            // {2 * (Math.Pow(2, x) - 1)} is the maximum value that can be represented with x leading zero bits using Exp-Golomb.
            while (value > 2 * (Math.Pow(2, leadingZeroBits) - 1))
            {
                leadingZeroBits++;
            }

            return leadingZeroBits;
        }

        public static uint GetUnsignedCodeExpGolombCodeSigned(int value)
        {
            uint unsignedValue = (uint)Math.Abs(value * 2);
            if (value > 0)
            {
                unsignedValue--;
            }
            return unsignedValue;
        }
    }
}
