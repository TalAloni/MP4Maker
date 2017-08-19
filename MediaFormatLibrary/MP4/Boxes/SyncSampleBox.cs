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
    public class SyncSampleBox : FullBox
    {
        public List<uint> Entries = new List<uint>(); // sample_number, one-based

        public SyncSampleBox() : base(BoxType.SyncSampleBox)
        {}

        public SyncSampleBox(Stream stream) : base(stream)
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
                    uint sampleNumber = BigEndianReader.ReadUInt32(stream);
                    Entries.Add(sampleNumber);
                }
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)Entries.Count);
            foreach (uint sampleNumber in Entries)
            {
                BigEndianWriter.WriteUInt32(stream, sampleNumber);
            }
        }
    }
}
