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
    public class DigitalCopyProtectionDescriptor : Descriptor
    {
        public ushort CASystemID;
        public bool Reserved1;
        public bool RetentionMoveMode;
        public byte RetentionState; // 3 bits
        public bool EPN;
        public byte DTCPCCI; // 2 bits, 0 means copy-free
        public byte Reserved2; // 5 bits
        public bool ImageConstraintToken;
        public byte APS; // 2 bits, 0 means copy-free

        public DigitalCopyProtectionDescriptor()
        {
            this.Tag = DescriptorTag.DigitalCopyProtectionDescriptor;
            Reserved1 = true;
            Reserved2 = 0x1F;
        }

        public DigitalCopyProtectionDescriptor(byte[] buffer, ref int offset) : base(buffer, ref offset)
        {
            CASystemID = BigEndianConverter.ToUInt16(this.Data, 0);
            int bitOffset = 16;
            Reserved1 = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            RetentionMoveMode = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            RetentionState = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 3);
            EPN = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            DTCPCCI = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 2);
            Reserved2 = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 5);
            ImageConstraintToken = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            APS = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 2);
        }

        public override void WriteBytes(byte[] buffer, ref int offset)
        {
            this.Data = new byte[4];
            BigEndianWriter.WriteUInt16(this.Data, 0, CASystemID);
            int bitOffset = 16;
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, Reserved1);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, RetentionMoveMode);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, RetentionState, 3);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, EPN);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, DTCPCCI, 2);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, Reserved2, 5);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, ImageConstraintToken);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, APS, 2);
            base.WriteBytes(buffer, ref offset);
        }

        public override int Length
        {
            get
            {
                return HeaderLength + 4;
            }
        }
    }
}
