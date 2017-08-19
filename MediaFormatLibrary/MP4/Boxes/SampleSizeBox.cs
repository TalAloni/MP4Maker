/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    public class SampleSizeBox : FullBox
    {
        public uint SampleSize; // Default sample size
        public uint SampleCount;
        public List<uint> Entries = new List<uint>(); // Only present if SampleSize == 0

        public SampleSizeBox() : base(BoxType.SampleSizeBox)
        {}

        public SampleSizeBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            if (Version == 0)
            {
                SampleSize = BigEndianReader.ReadUInt32(stream);
                SampleCount = BigEndianReader.ReadUInt32(stream);
                if (SampleSize == 0)
                {
                    for (int index = 0; index < SampleCount; index++)
                    {
                        uint entrySize = BigEndianReader.ReadUInt32(stream);
                        Entries.Add(entrySize);
                    }
                }
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, SampleSize);
            BigEndianWriter.WriteUInt32(stream, SampleCount);
            if (Entries != null)
            {
                foreach (uint entrySize in Entries)
                {
                    BigEndianWriter.WriteUInt32(stream, entrySize);
                }
            }
        }
    }
}
