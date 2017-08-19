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

namespace MediaFormatLibrary.Mpeg2
{
    public class PartialTransportStreamDescriptor : Descriptor
    {
        public byte Reserved1; // 2 bits
        public uint PeakRate; // 22 bits, in units of 400 bit/s
        public byte Reserved2; // 2 bits
        public uint MinimumOverallSmoothingRate; // 22 bits, in units of 400 bit/s
        public byte Reserved3; // 2 bits
        public ushort MaximumOverallSmoothingBuffer; // 14 bits, in units of 1 byte

        public PartialTransportStreamDescriptor()
        {
            this.Tag = DescriptorTag.PartialTransportStreamDescriptor;
            Reserved1 = 0x03;
            Reserved2 = 0x03;
            Reserved3 = 0x03;
        }

        public PartialTransportStreamDescriptor(byte[] buffer, ref int offset): base(buffer, ref offset)
        {
            int bitOffset = 0;
            Reserved1 = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 2);
            PeakRate = (uint)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 22);
            Reserved2 = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 2);
            MinimumOverallSmoothingRate = (uint)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 22);
            Reserved3 = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 2);
            MaximumOverallSmoothingBuffer = (ushort)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 14);
        }

        public override void WriteBytes(byte[] buffer, ref int offset)
        {
            this.Data = new byte[8];
            int bitOffset = 0;
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, Reserved1, 2);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, PeakRate, 22);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, Reserved2, 2);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, MinimumOverallSmoothingRate, 22);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, Reserved3, 2);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, MaximumOverallSmoothingBuffer, 14);

            base.WriteBytes(buffer, ref offset);
        }

        public override int Length
        {
            get
            {
                return HeaderLength + 8;
            }
        }
    }
}
