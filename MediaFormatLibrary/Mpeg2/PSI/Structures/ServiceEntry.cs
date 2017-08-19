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
    public class ServiceEntry
    {
        public ushort ServiceID;
        public bool Reserved;
        public byte RunningStatus; // 3 bits
        // ushort ServiceLoopLength; // 12 bits
        public List<Descriptor> Descriptors = new List<Descriptor>();

        public ServiceEntry()
        {
            Reserved = true;
        }

        public ServiceEntry(byte[] buffer, ref int offset)
        {
            ServiceID = BigEndianReader.ReadUInt16(buffer, ref offset);
            ushort temp = BigEndianReader.ReadUInt16(buffer, ref offset);
            Reserved = (temp & 0x8000) > 0;
            RunningStatus = (byte)((temp & 0xE000) >> 12);
            ushort serviceLoopLength = (ushort)(temp & 0x0FFF);
            Descriptors = Descriptor.ReadDescriptorList(buffer, ref offset, serviceLoopLength);
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            BigEndianWriter.WriteUInt16(buffer, ref offset, ServiceID);
            ushort serviceLoopLength = (ushort)Descriptor.GetDescriptorListLength(Descriptors);
            ushort temp = (ushort)(Convert.ToByte(Reserved) << 15 | ((RunningStatus & 0x07) << 12) | (serviceLoopLength & 0x0FFF));
            BigEndianWriter.WriteUInt16(buffer, ref offset, temp);
            Descriptor.WriteDescriptorList(buffer, ref offset, Descriptors);
        }

        public int Length
        {
            get
            {
                return 4 + Descriptor.GetDescriptorListLength(Descriptors);
            }
        }
    }
}
