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
    public class ChunkOffsetBox : FullBox
    {
        public List<uint> Entries = new List<uint>();

        public ChunkOffsetBox() : base(BoxType.ChunkOffsetBox)
        {}

        public ChunkOffsetBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            if (Version == 0)
            {
                uint entryCount = BigEndianReader.ReadUInt32(stream);
                for (int index = 0; index < entryCount; index++)
                {
                    // Offsets are file offsets, not the offset into any box within the file (e.g. Media Data Box).
                    // This permits referring to media data in files without any box structure.
                    uint chunkOffset = BigEndianReader.ReadUInt32(stream);
                    Entries.Add(chunkOffset);
                }
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)Entries.Count);
            foreach (uint chunkOffset in Entries)
            {
                BigEndianWriter.WriteUInt32(stream, chunkOffset);
            }
        }
    }
}
