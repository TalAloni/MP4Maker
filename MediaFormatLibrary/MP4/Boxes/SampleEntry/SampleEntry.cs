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
    public class SampleEntry : Box
    {
        public byte[] Reserved; // 6 bytes
        public ushort DataReferenceIndex; // data_reference_index, "the index of the data reference to use to retrieve data associated with samples that use this sample description"

        public SampleEntry(BoxType type) : base(type)
        {
            Reserved = new byte[6];
        }

        public SampleEntry(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            Reserved = ByteReader.ReadBytes(stream, 6);
            DataReferenceIndex = BigEndianReader.ReadUInt16(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            ByteWriter.WriteBytes(stream, Reserved);
            BigEndianWriter.WriteUInt16(stream, DataReferenceIndex);
        }
    }
}
