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
    public class CompositionOffsetEntry
    {
        public uint SampleCount;
        public long SampleOffset; // UInt32 or Int32

        public CompositionOffsetEntry()
        {
        }

        public CompositionOffsetEntry(uint sampleCount, long sampleOffset)
        {
            SampleCount = sampleCount;
            SampleOffset = sampleOffset;
        }
    }

    public class CompositionOffsetBox : FullBox
    {
        public List<CompositionOffsetEntry> Entries = new List<CompositionOffsetEntry>();

        public CompositionOffsetBox() : base(BoxType.CompositionTimeToSampleBox)
        {
        }

        public CompositionOffsetBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            if (Version == 0 || Version == 1)
            {
                uint entryCount = BigEndianReader.ReadUInt32(stream);
                for (int index = 0; index < entryCount; index++)
                {
                    CompositionOffsetEntry entry = new CompositionOffsetEntry();
                    entry.SampleCount = BigEndianReader.ReadUInt32(stream);
                    if (Version == 0)
                    {

                        entry.SampleOffset = BigEndianReader.ReadUInt32(stream);
                    }
                    else
                    {
                        entry.SampleOffset = BigEndianReader.ReadInt32(stream);
                    }
                    Entries.Add(entry);
                }
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)Entries.Count);
            foreach (CompositionOffsetEntry entry in Entries)
            {
                BigEndianWriter.WriteUInt32(stream, entry.SampleCount);
                if (Version == 0)
                {
                    BigEndianWriter.WriteUInt32(stream, (uint)entry.SampleOffset);
                }
                else
                {
                    BigEndianWriter.WriteInt32(stream, (int)entry.SampleOffset);
                }
            }
        }

        /// <summary>
        /// Convert all SampleOffset values to positive
        /// </summary>
        /// <returns>The delay added to the track (to prevent negative SampleOffset)</returns>
        public int ConvertToNonNegative()
        {
            int minSampleOffset = 0;
            foreach (CompositionOffsetEntry entry in this.Entries)
            {
                if (entry.SampleOffset < minSampleOffset)
                {
                    minSampleOffset = (int)entry.SampleOffset;
                }
            }

            int trackDelay = -minSampleOffset;
            if (minSampleOffset < 0)
            {
                foreach (CompositionOffsetEntry entry in this.Entries)
                {
                    entry.SampleOffset += trackDelay;
                }
            }
            return trackDelay;
        }
    }
}
