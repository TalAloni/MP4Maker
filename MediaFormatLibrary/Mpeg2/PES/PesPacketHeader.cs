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
    /// Packetized Elementary Stream Header
    /// </summary>
    public class PesPacketHeader
    {
        public const int Length = 6;
        public const uint PacketStartCodePrefix = 0x000001;

        //public uint PacketStartCodePrefix; // 3 bytes
        public ElementaryStreamID StreamID;
        /// <summary>
        /// A value of 0 indicates that the PES packet length is neither specified nor bounded and
        /// is allowed only in PES packets whose payload consists of bytes from a video
        /// elementary stream contained in Transport Stream packets.
        /// </summary>
        public ushort PacketLength; // Excluding header

        public PesPacketHeader()
        {
        }

        public PesPacketHeader(byte[] buffer, ref int offset)
        {
            uint startCodePrefix = ReadUInt24(buffer, ref offset);
            StreamID = (ElementaryStreamID)ByteReader.ReadByte(buffer, ref offset);
            PacketLength = BigEndianReader.ReadUInt16(buffer, ref offset);
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            WriteUInt24(buffer, ref offset, PacketStartCodePrefix);
            ByteWriter.WriteByte(buffer, ref offset, (byte)StreamID);
            BigEndianWriter.WriteUInt16(buffer, ref offset, PacketLength);
        }

        public static uint ReadUInt24(byte[] buffer, ref int offset)
        {
            byte[] temp = ByteReader.ReadBytes(buffer, ref offset, 3);
            return (uint)((buffer[0] << 16) | (buffer[1] << 8) | (buffer[2] << 0));
        }

        public static void WriteUInt24(byte[] buffer, ref int offset, uint value)
        {
            ByteWriter.WriteByte(buffer, ref offset, (byte)((value >> 16) & 0xFF));
            ByteWriter.WriteByte(buffer, ref offset, (byte)((value >> 8) & 0xFF));
            ByteWriter.WriteByte(buffer, ref offset, (byte)((value >> 0) & 0xFF));
        }
    }
}
