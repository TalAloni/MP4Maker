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
    public class ElementaryStreamEntry
    {
        public StreamType StreamType;
        public byte Reserved1; // 3 bits
        public ushort PID; // 13 bits, elementary_PID
        public byte Reserved2; // 4 bits
        // ushort ESInfoLength; // 12 bits
        public List<Descriptor> Descriptors = new List<Descriptor>();

        public ElementaryStreamEntry()
        {
            Reserved1 = 0x07;
            Reserved2 = 0x0F;
        }

        public ElementaryStreamEntry(byte[] buffer, ref int offset)
        {
            StreamType = (StreamType)ByteReader.ReadByte(buffer, ref offset);
            ushort temp = BigEndianReader.ReadUInt16(buffer, ref offset);
            Reserved1 = (byte)((temp & 0xE000) >> 13);
            PID = (ushort)(temp & 0x1FFF);
            temp = BigEndianReader.ReadUInt16(buffer, ref offset);
            Reserved2 = (byte)((temp & 0xF000) >> 12);
            ushort esInfoLength = (ushort)(temp & 0x0FFF);
            if (esInfoLength > 0)
            {
                Descriptors = Descriptor.ReadDescriptorList(buffer, ref offset, esInfoLength);
            }
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            ByteWriter.WriteByte(buffer, ref offset, (byte)StreamType);
            ushort temp = (ushort)(((Reserved1 & 0x07) << 13) | (PID & 0x1FFF));
            BigEndianWriter.WriteUInt16(buffer, ref offset, temp);
            ushort esInfoLength = (ushort)Descriptor.GetDescriptorListLength(Descriptors);
            temp = (ushort)(((Reserved2 & 0x0F) << 12) | (esInfoLength & 0x0FFF));
            BigEndianWriter.WriteUInt16(buffer, ref offset, temp);
            Descriptor.WriteDescriptorList(buffer, ref offset, Descriptors);
        }

        public int Length
        {
            get
            {
                return 5 + Descriptor.GetDescriptorListLength(Descriptors);
            }
        }
    }
}
