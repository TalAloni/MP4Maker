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
    public class SampleToChunkEntry
    {
        public uint FirstChunk; // One-based
        public uint SamplesPerChunk;
        public uint SampleDescriptionIndex; // One-based

        public SampleToChunkEntry()
        {
        }

        public SampleToChunkEntry(uint firstChunk, uint samplesPerChunk, uint sampleDescriptionIndex)
        {
            FirstChunk = firstChunk;
            SamplesPerChunk = samplesPerChunk;
            SampleDescriptionIndex = sampleDescriptionIndex;
        }

        public SampleToChunkEntry(Stream stream)
        {
            FirstChunk = BigEndianReader.ReadUInt32(stream);
            SamplesPerChunk = BigEndianReader.ReadUInt32(stream);
            SampleDescriptionIndex = BigEndianReader.ReadUInt32(stream);
        }

        public void WriteBytes(Stream stream)
        {
            BigEndianWriter.WriteUInt32(stream, FirstChunk);
            BigEndianWriter.WriteUInt32(stream, SamplesPerChunk);
            BigEndianWriter.WriteUInt32(stream, SampleDescriptionIndex);
        }
    }

    public class SampleToChunkBox : FullBox
    {
        public List<SampleToChunkEntry> Entries = new List<SampleToChunkEntry>();

        public SampleToChunkBox() : base(BoxType.SampleToChunkBox)
        {}

        public SampleToChunkBox(Stream stream) : base(stream)
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
                    SampleToChunkEntry entry = new SampleToChunkEntry(stream);
                    Entries.Add(entry);
                }
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)Entries.Count);
            foreach (SampleToChunkEntry entry in Entries)
            {
                entry.WriteBytes(stream);
            }
        }
    }
}
