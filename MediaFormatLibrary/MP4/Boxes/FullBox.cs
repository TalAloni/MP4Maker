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
    public class FullBox : Box
    {
        public byte Version;
        public uint Flags; // 3 bytes

        public FullBox(BoxType type) : base(type)
        {
        }

        public FullBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            Version = (byte)stream.ReadByte();
            Flags = MP4Helper.ReadUInt24(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            stream.WriteByte(Version);
            MP4Helper.WriteUInt24(stream, Flags);
        }
    }
}
