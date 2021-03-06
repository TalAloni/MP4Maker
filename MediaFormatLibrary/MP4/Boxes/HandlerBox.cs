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

namespace MediaFormatLibrary.MP4
{
    public class HandlerBox : FullBox
    {
        public uint Predefined;
        public HandlerType HandlerType;
        public byte[] Reserved; // 12 reserved bytes
        public string Name;

        public HandlerBox() : base(BoxType.HandlerReferenceBox)
        {
            Reserved = new byte[12];
            Name = String.Empty;
        }

        public HandlerBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            if (Version == 0)
            {
                Predefined = BigEndianReader.ReadUInt32(stream);
                HandlerType = (HandlerType)BigEndianReader.ReadUInt32(stream);
                Reserved = ByteReader.ReadBytes(stream, 12);
                Name = ByteReader.ReadNullTerminatedUTF8String(stream);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, Predefined);
            BigEndianWriter.WriteUInt32(stream, (uint)HandlerType);
            ByteWriter.WriteBytes(stream, Reserved);
            ByteWriter.WriteNullTerminatedUTF8String(stream, Name);
        }
    }
}
