/* Copyright (C) 2026 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System.IO;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    public class TrackFragmentHeaderBox : FullBox
    {
        public uint TrackID;
        public ulong BaseDataOffset;
        public uint SampleDescriptionIndex;
        public uint DefaultSampleDuration;
        public uint DefaultSampleSize;
        public uint DefaultSampleFlags;

        public TrackFragmentHeaderBox() : base(BoxType.TrackFragmentHeaderBox)
        {
        }

        public TrackFragmentHeaderBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            TrackID = BigEndianReader.ReadUInt32(stream);
            if (BaseDataOffsetPresent)
            {
                BaseDataOffset = BigEndianReader.ReadUInt64(stream);
            }

            if (SampleDescriptionIndexPresent)
            {
                SampleDescriptionIndex = BigEndianReader.ReadUInt32(stream);
            }

            if (DefaultSampleDurationPresent)
            {
                DefaultSampleDuration = BigEndianReader.ReadUInt32(stream);
            }

            if (DefaultSampleSizePresent)
            {
                DefaultSampleSize = BigEndianReader.ReadUInt32(stream);
            }

            if (DefaultSampleFlagsPresent)
            {
                DefaultSampleFlags = BigEndianReader.ReadUInt32(stream);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, TrackID);
            if (BaseDataOffsetPresent)
            {
                BigEndianWriter.WriteUInt64(stream, BaseDataOffset);
            }

            if (SampleDescriptionIndexPresent)
            {
                BigEndianWriter.WriteUInt32(stream, SampleDescriptionIndex);
            }

            if (DefaultSampleDurationPresent)
            {
                BigEndianWriter.WriteUInt32(stream, DefaultSampleDuration);
            }

            if (DefaultSampleSizePresent)
            {
                BigEndianWriter.WriteUInt32(stream, DefaultSampleSize);
            }

            if (DefaultSampleFlagsPresent)
            {
                BigEndianWriter.WriteUInt32(stream, DefaultSampleFlags);
            }
        }

        public bool BaseDataOffsetPresent
        {
            get
            {
                return (Flags & 0x00000001) != 0;
            }
        }

        public bool SampleDescriptionIndexPresent
        {
            get
            {
                return (Flags & 0x00000002) != 0;
            }
        }

        public bool DefaultSampleDurationPresent
        {
            get
            {
                return (Flags & 0x00000008) != 0;
            }
        }

        public bool DefaultSampleSizePresent
        {
            get
            {
                return (Flags & 0x00000010) != 0;
            }
        }

        public bool DefaultSampleFlagsPresent
        {
            get
            {
                return (Flags & 0x00000020) != 0;
            }
        }

        public bool DefaultBaseIsMoof
        {
            get
            {
                return (Flags & 0x00020000) != 0;
            }
        }
    }
}
