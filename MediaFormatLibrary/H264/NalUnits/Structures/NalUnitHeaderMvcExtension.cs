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
    /// nal_unit_header_mvc_extension
    /// </summary>
    public class NalUnitHeaderMvcExtension
    {
        public bool NonIdrFlag; // non_idr_flag
        public byte PriorityID; // 6 bits, priority_id
        public ushort ViewID; // 10 bits, view_id
        public byte TemporalID; // 3 bits, temporal_id
        public bool AnchorPicFlag; // anchor_pic_flag
        public bool InterViewFlag; // inter_view_flag
        public bool ReservedOneBit; // reserved_one_bit

        public NalUnitHeaderMvcExtension()
        {
            ReservedOneBit = true;
        }

        public NalUnitHeaderMvcExtension(BitStream bitStream)
        {
            NonIdrFlag = bitStream.ReadBoolean();
            PriorityID = (byte)bitStream.ReadBits(6);
            ViewID = (ushort)bitStream.ReadBits(10);
            TemporalID = (byte)bitStream.ReadBits(3);
            AnchorPicFlag = bitStream.ReadBoolean();
            InterViewFlag = bitStream.ReadBoolean();
            ReservedOneBit = bitStream.ReadBoolean();
        }

        public void WriteBits(BitStream bitStream)
        {
            bitStream.WriteBoolean(NonIdrFlag);
            bitStream.WriteBits(PriorityID, 6);
            bitStream.WriteBits(ViewID, 10);
            bitStream.WriteBits(TemporalID, 3);
            bitStream.WriteBoolean(AnchorPicFlag);
            bitStream.WriteBoolean(InterViewFlag);
            bitStream.WriteBoolean(ReservedOneBit);
        }
    }
}
