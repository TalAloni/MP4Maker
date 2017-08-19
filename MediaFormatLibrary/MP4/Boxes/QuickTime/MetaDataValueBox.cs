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
    /// <summary>
    /// [QuickTime File Format Specification] (Metadata) Value Atom
    /// </summary>
    public class MetaDataValueBox : Box
    {
        public MetaDataValueType DataType;
        public uint Locale; // 0 - Default, all speakers in all countries
        public byte[] Data;

        public MetaDataValueBox() : base(BoxType.MetaDataValueBox)
        {
        }

        public MetaDataValueBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            DataType = (MetaDataValueType)BigEndianReader.ReadUInt32(stream);
            Locale = BigEndianReader.ReadUInt32(stream);
            int bytesToRead = (int)this.Size - 16;
            Data = ByteReader.ReadBytes(stream, bytesToRead);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)DataType);
            BigEndianWriter.WriteUInt32(stream, Locale);
            ByteWriter.WriteBytes(stream, Data);
        }
    }
}
