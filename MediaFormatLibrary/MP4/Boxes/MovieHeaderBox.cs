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
    public class MovieHeaderBox : FullBox
    {
        public DateTime CreationTime;
        public DateTime ModificationTime;
        public uint Timescale;
        public ulong Duration;
        public double Rate; // Fixed point: 16(bits).16(bits)
        public double Volume; // Fixed point: 8(bits).8(bits)
        public ushort Reserved1;
        public ulong Reserved2;
        public byte[] Matrix; // 36 bytes
        public byte[] Predefined; // 24 bytes
        public uint NextTrackID;

        public MovieHeaderBox() : base(BoxType.MovieHeaderBox)
        {
            Version = 0;
            Rate = 1.0;
            Volume = 1.0;
            Matrix = new byte[36];
            Matrix[1] = 0x01;
            Matrix[17] = 0x01;
            Matrix[32] = 0x40;
            Predefined = new byte[24];
        }

        public MovieHeaderBox(Stream stream) : base(stream)
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
                    Timescale = BigEndianReader.ReadUInt32(stream);
                    Duration = BigEndianReader.ReadUInt32(stream);
                }
                else
                {
                    CreationTime = MP4Helper.ReadUInt64Time(stream);
                    ModificationTime = MP4Helper.ReadUInt64Time(stream);
                    Timescale = BigEndianReader.ReadUInt32(stream);
                    Duration = BigEndianReader.ReadUInt64(stream);
                }
                Rate = MP4Helper.ReadFixedPoint16_16(stream);
                Volume = MP4Helper.ReadFixedPoint8_8(stream);
                Reserved1 = BigEndianReader.ReadUInt16(stream);
                Reserved2 = BigEndianReader.ReadUInt64(stream);
                Matrix = ByteReader.ReadBytes(stream, 36);
                Predefined = ByteReader.ReadBytes(stream, 24);
                NextTrackID = BigEndianReader.ReadUInt32(stream);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            if (Version == 0)
            {
                MP4Helper.WriteUInt32Time(stream, CreationTime);
                MP4Helper.WriteUInt32Time(stream, ModificationTime);
                BigEndianWriter.WriteUInt32(stream, Timescale);
                BigEndianWriter.WriteUInt32(stream, (uint)Math.Min(Duration, UInt32.MaxValue));
            }
            else if (Version == 1)
            {
                MP4Helper.WriteUInt64Time(stream, CreationTime);
                MP4Helper.WriteUInt64Time(stream, ModificationTime);
                BigEndianWriter.WriteUInt32(stream, Timescale);
                BigEndianWriter.WriteUInt64(stream, Duration);
            }
            MP4Helper.WriteFixedPoint16_16(stream, Rate);
            MP4Helper.WriteFixedPoint8_8(stream, Volume);
            BigEndianWriter.WriteUInt16(stream, Reserved1);
            BigEndianWriter.WriteUInt64(stream, Reserved2);
            ByteWriter.WriteBytes(stream, Matrix);
            ByteWriter.WriteBytes(stream, Predefined);
            BigEndianWriter.WriteUInt32(stream, NextTrackID);
        }
    }
}
