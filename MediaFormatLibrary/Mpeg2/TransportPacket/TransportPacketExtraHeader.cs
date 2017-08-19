/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.Mpeg2
{
    /// <summary>
    /// TP_extra_header (Blu-ray Disc Audio-Video extension)
    /// </summary>
    public class TransportPacketExtraHeader
    {
        public const int Length = 4;

        public CopyPermissionIndicator CopyPermissionIndicator; // 2 bits, copy_permission_indicator
        /// <summary>
        /// The arrival_time_stamp is equal to the lower 30 bits of the 27 MHz STC at the 0x47 byte of the Transport packet.
        /// In a packet that contains a PCR, the PCR will be a few ticks later than the arrival_time_stamp.
        /// The exact difference between the arrival_time_stamp and the PCR (and the number of bits between them)
        /// indicates the intended fixed bitrate of the variable rate Transport Stream.
        /// </summary>
        public uint ArrivalTimeStamp; // 30 bits, Arrival_Time_stamp

        public TransportPacketExtraHeader()
        {
        }

        public TransportPacketExtraHeader(byte[] buffer, ref int offset)
        {
            uint temp = BigEndianReader.ReadUInt32(buffer, ref offset);
            CopyPermissionIndicator = (CopyPermissionIndicator)((temp & 0xC0000000) >> 30);
            ArrivalTimeStamp = (temp & 0x3FFFFFFF);
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            uint temp = ((uint)((byte)CopyPermissionIndicator << 30) |
                        (uint)(ArrivalTimeStamp & 0x3FFFFFFF));
            BigEndianWriter.WriteUInt32(buffer, ref offset, temp);
        }
    }
}
