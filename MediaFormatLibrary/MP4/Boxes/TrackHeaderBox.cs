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
    public enum TrackHeaderFlags : uint // 24 bits
    {
        TrackEnabled = 0x01, // Track_enabled
        TrackInMovie = 0x02, // Track_in_movie
        TrackInPreview = 0x04, // Track_in_preview
    }

    public class TrackHeaderBox : FullBox
    {
        public DateTime CreationTime;
        public DateTime ModificationTime;
        public uint TrackID;
        public uint Reserved1;
        public ulong Duration; // In MovieHeaderBox time units
        public ulong Reserved2;
        public ushort Layer;
        public ushort AlternateGroup;
        public double Volume; // Fixed point: 8(bits).8(bits)
        public ushort Reserved3;
        public byte[] Matrix; // 36 bytes
        public double Width; // Fixed point: 16(bits).16(bits)
        public double Height; // Fixed point: 16(bits).16(bits)

        public TrackHeaderBox() : base(BoxType.TrackHeaderBox)
        {
            Flags = (uint)(TrackHeaderFlags.TrackEnabled | TrackHeaderFlags.TrackInMovie | TrackHeaderFlags.TrackInPreview);
            Version = 0;
            Matrix = new byte[36];
            Matrix[1] = 0x01;
            Matrix[17] = 0x01;
            Matrix[32] = 0x40;
        }

        public TrackHeaderBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            if (Version == 0 || Version == 1)
            {
                if (Version == 0)
                {
                    CreationTime = MP4Helper.ReadUInt32Time(stream);
                    ModificationTime = MP4Helper.ReadUInt32Time(stream);
                    TrackID = BigEndianReader.ReadUInt32(stream);
                    Reserved1 = BigEndianReader.ReadUInt32(stream);
                    Duration = BigEndianReader.ReadUInt32(stream);
                }
                else
                {
                    CreationTime = MP4Helper.ReadUInt64Time(stream);
                    ModificationTime = MP4Helper.ReadUInt64Time(stream);
                    TrackID = BigEndianReader.ReadUInt32(stream);
                    Reserved1 = BigEndianReader.ReadUInt32(stream);
                    Duration = BigEndianReader.ReadUInt64(stream);
                }
                Reserved2 = BigEndianReader.ReadUInt64(stream);
                Layer = BigEndianReader.ReadUInt16(stream);
                AlternateGroup = BigEndianReader.ReadUInt16(stream);
                Volume = MP4Helper.ReadFixedPoint8_8(stream);
                Reserved3 = BigEndianReader.ReadUInt16(stream);
                Matrix = ByteReader.ReadBytes(stream, 36);
                Width = MP4Helper.ReadFixedPoint16_16(stream);
                Height = MP4Helper.ReadFixedPoint16_16(stream);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            if (Version == 0)
            {
                MP4Helper.WriteUInt32Time(stream, CreationTime);
                MP4Helper.WriteUInt32Time(stream, ModificationTime);
                BigEndianWriter.WriteUInt32(stream, TrackID);
                BigEndianWriter.WriteUInt32(stream, Reserved1);
                BigEndianWriter.WriteUInt32(stream, (uint)Math.Min(Duration, UInt32.MaxValue));
            }
            else if (Version == 1)
            {
                MP4Helper.WriteUInt64Time(stream, CreationTime);
                MP4Helper.WriteUInt64Time(stream, ModificationTime);
                BigEndianWriter.WriteUInt32(stream, TrackID);
                BigEndianWriter.WriteUInt32(stream, Reserved1);
                BigEndianWriter.WriteUInt64(stream, Duration);
            }
            BigEndianWriter.WriteUInt64(stream, Reserved2);
            BigEndianWriter.WriteUInt16(stream, Layer);
            BigEndianWriter.WriteUInt16(stream, AlternateGroup);
            MP4Helper.WriteFixedPoint8_8(stream, Volume);
            BigEndianWriter.WriteUInt16(stream, Reserved3);
            ByteWriter.WriteBytes(stream, Matrix);
            MP4Helper.WriteFixedPoint16_16(stream, Width);
            MP4Helper.WriteFixedPoint16_16(stream, Height);
        }
    }
}
