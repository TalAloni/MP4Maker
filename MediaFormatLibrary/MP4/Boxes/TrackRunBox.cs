/* Copyright (C) 2026 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    public class TrackRunBox : FullBox
    {
        // uint SampleCount;
        public int DataOffset;
        public uint FirstSampleFlags;
        public List<TrackSampleEntry> Samples = new List<TrackSampleEntry>();

        public TrackRunBox() : base(BoxType.TrackRunBox)
        {
        }

        public TrackRunBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            uint sampleCount = BigEndianReader.ReadUInt32(stream);
            if (DataOffsetPresent)
            {
                DataOffset = BigEndianReader.ReadInt32(stream);
            }

            if (FirstSampleFlagsPresent)
            {
                FirstSampleFlags = BigEndianReader.ReadUInt32(stream);
            }

            for (int index = 0; index < sampleCount; index++)
            {
                TrackSampleEntry entry = new TrackSampleEntry();
                if (SampleDurationPresent)
                {
                    entry.SampleDuration = BigEndianReader.ReadUInt32(stream);
                }

                if (SampleSizePresent)
                {
                    entry.SampleSize = BigEndianReader.ReadUInt32(stream);
                }

                if (SampleFlagsPresent)
                {
                    entry.SampleFlags = BigEndianReader.ReadUInt32(stream);
                }

                if (SampleCompositionTimeOffsetsPresent)
                {
                    if (Version == 0)
                    {
                        entry.SampleCompositionTimeOffset = BigEndianReader.ReadUInt32(stream);
                    }
                    else
                    {
                        entry.SampleCompositionTimeOffset = BigEndianReader.ReadInt32(stream);
                    }
                }
                Samples.Add(entry);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)Samples.Count);
            if (DataOffsetPresent)
            {
                BigEndianWriter.WriteInt32(stream, DataOffset);
            }

            if (FirstSampleFlagsPresent)
            {
                BigEndianWriter.WriteUInt32(stream, FirstSampleFlags);
            }

            foreach (TrackSampleEntry entry in Samples)
            {
                if (SampleDurationPresent)
                {
                    BigEndianWriter.WriteUInt32(stream, entry.SampleDuration);
                }

                if (SampleSizePresent)
                {
                    BigEndianWriter.WriteUInt32(stream, entry.SampleSize);
                }

                if (SampleFlagsPresent)
                {
                    BigEndianWriter.WriteUInt32(stream, entry.SampleFlags);
                }

                if (SampleCompositionTimeOffsetsPresent)
                {
                    if (Version == 0)
                    {
                        BigEndianWriter.WriteUInt32(stream, (uint)entry.SampleCompositionTimeOffset);
                    }
                    else
                    {
                        BigEndianWriter.WriteInt32(stream, (int)entry.SampleCompositionTimeOffset);
                    }
                }
            }
        }

        public bool DataOffsetPresent
        {
            get
            {
                return (Flags & 0x00000001) != 0;
            }
        }

        public bool FirstSampleFlagsPresent
        {
            get
            {
                return (Flags & 0x00000004) != 0;
            }
        }

        public bool SampleDurationPresent
        {
            get
            {
                return (Flags & 0x00000100) != 0;
            }
        }

        public bool SampleSizePresent
        {
            get
            {
                return (Flags & 0x00000200) != 0;
            }
        }

        public bool SampleFlagsPresent
        {
            get
            {
                return (Flags & 0x00000400) != 0;
            }
        }

        public bool SampleCompositionTimeOffsetsPresent
        {
            get
            {
                return (Flags & 0x00000800) != 0;
            }
        }
    }

    public class TrackSampleEntry
    {
        public uint SampleDuration;
        public uint SampleSize;
        public uint SampleFlags;
        public long SampleCompositionTimeOffset; // UInt32 or Int32

        public TrackSampleEntry()
        {
        }

        public TrackSampleEntry(Stream stream)
        {
        }
    }
}
