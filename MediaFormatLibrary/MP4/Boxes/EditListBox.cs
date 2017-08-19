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
    public class EditListEntry
    {
        public ulong SegmentDuration; // The duration of this edit segment in units of the timescale in the Movie Header Box
        public long MediaTime; // the starting time within the media of this edit segment (in media time scale units)
        public double MediaRate; // Fixed point: 16(bits).16(bits)

        public EditListEntry()
        {
        }

        public EditListEntry(ulong segmentDuration, long mediaTime, double mediaRate)
        {
            SegmentDuration = segmentDuration;
            MediaTime = mediaTime;
            MediaRate = mediaRate;
        }
    }

    public class EditListBox : FullBox
    {
        public List<EditListEntry> Entries = new List<EditListEntry>();
        
        public EditListBox() : base(BoxType.EditListBox)
        {
        }

        public EditListBox(Stream stream) : base(stream)
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
                    EditListEntry entry = new EditListEntry();
                    if (Version == 0)
                    {
                        entry.SegmentDuration = BigEndianReader.ReadUInt32(stream);
                        entry.MediaTime = BigEndianReader.ReadInt32(stream);
                    }
                    else
                    {
                        entry.SegmentDuration = BigEndianReader.ReadUInt64(stream);
                        entry.MediaTime = BigEndianReader.ReadInt64(stream);
                    }
                    entry.MediaRate = MP4Helper.ReadFixedPoint16_16(stream);
                    Entries.Add(entry);
                }
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)Entries.Count);
            foreach(EditListEntry entry in Entries)
            {
                if (Version == 0)
                {
                    BigEndianWriter.WriteUInt32(stream, (uint)Math.Min(entry.SegmentDuration, UInt32.MaxValue));
                    BigEndianWriter.WriteInt32(stream, (int)Math.Min(entry.MediaTime, UInt32.MaxValue));
                }
                else
                {
                    BigEndianWriter.WriteUInt64(stream, entry.SegmentDuration);
                    BigEndianWriter.WriteInt64(stream, entry.MediaTime);
                }
                MP4Helper.WriteFixedPoint16_16(stream, entry.MediaRate);
            }
        }
    }
}
