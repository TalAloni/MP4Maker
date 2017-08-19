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

namespace MediaFormatLibrary.H264
{
    public class AccessUnitDelimiter : NalUnit
    {
        public byte PrimaryPicType; // primary_pic_type

        public AccessUnitDelimiter() : base(NalUnitType.AccessUnitDelimiter)
        {
        }

        public AccessUnitDelimiter(MemoryStream stream) : base(stream)
        {
            ReadEncodedPayloadBytes(stream);
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            PrimaryPicType = (byte)bitStream.ReadBits(3);
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            bitStream.WriteBits(PrimaryPicType, 3);
            bitStream.WriteRbspTrailingBits();
        }

        public static byte GetPrimaryPicType(SliceType sliceType)
        {
            byte primary_pic_type;
            switch (sliceType)
            {
                case SliceType.SLICE_TYPE_I:
                case SliceType.SLICE_TYPE_I_2:
                    {
                        primary_pic_type = 0;
                        break;
                    }
                case SliceType.SLICE_TYPE_P:
                case SliceType.SLICE_TYPE_P_2:
                    {
                        primary_pic_type = 1;
                        break;
                    }
                case SliceType.SLICE_TYPE_B:
                case SliceType.SLICE_TYPE_B_2:
                    {
                        primary_pic_type = 2;
                        break;
                    }
                default:
                    {
                        primary_pic_type = 7;
                        break;
                    }
            }
            return primary_pic_type;
        }
    }
}
