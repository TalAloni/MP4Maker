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
    public class MediaHeaderBox : FullBox
    {
        public DateTime CreationTime;
        public DateTime ModificationTime;
        public uint Timescale;
        public ulong Duration;
        public LanguageCode LanguageCode;
        public ushort Predefined;

        public MediaHeaderBox() : base(BoxType.MediaHeaderBox)
        {
        }

        public MediaHeaderBox(Stream stream) : base(stream)
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
                LanguageCode = (LanguageCode)(BigEndianReader.ReadUInt16(stream) & 0x7FFF);
                Predefined = BigEndianReader.ReadUInt16(stream);
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

            BigEndianWriter.WriteUInt16(stream, (ushort)LanguageCode);
            BigEndianWriter.WriteUInt16(stream, Predefined);
        }
    }
}
