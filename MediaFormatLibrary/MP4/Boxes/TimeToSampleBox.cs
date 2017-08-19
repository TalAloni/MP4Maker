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
    public class TimeToSampleEntry
    {
        public uint SampleCount; // "counts the number of consecutive samples that have the given duration"
        public uint SampleDelta; // "gives the delta of these samples in the time-scale of the media"

        public TimeToSampleEntry()
        {
        }

        public TimeToSampleEntry(uint sampleCount, uint sampleDelta)
        {
            SampleCount = sampleCount;
            SampleDelta = sampleDelta;
        }
    }

    public class TimeToSampleBox : FullBox
    {
        public List<TimeToSampleEntry> Entries = new List<TimeToSampleEntry>();

        public TimeToSampleBox() : base(BoxType.DecodingTimeToSampleBox)
        {
        }

        public TimeToSampleBox(Stream stream) : base(stream)
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
                    TimeToSampleEntry entry = new TimeToSampleEntry();
                    entry.SampleCount = BigEndianReader.ReadUInt32(stream);
                    entry.SampleDelta = BigEndianReader.ReadUInt32(stream);
                    Entries.Add(entry);
                }
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)Entries.Count);
            foreach (TimeToSampleEntry entry in Entries)
            {
                BigEndianWriter.WriteUInt32(stream, entry.SampleCount);
                BigEndianWriter.WriteUInt32(stream, entry.SampleDelta);
            }
        }
    }
}
