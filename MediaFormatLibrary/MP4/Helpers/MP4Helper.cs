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
    public class MP4Helper
    {
        public static DateTime ReadUInt32Time(Stream stream)
        {
            uint span = BigEndianReader.ReadUInt32(stream);
            DateTime result = new DateTime(1904, 1, 1);
            result = result.AddSeconds(span);
            return result;
        }

        public static DateTime ReadUInt64Time(Stream stream)
        {
            ulong span = BigEndianReader.ReadUInt64(stream);
            DateTime result = new DateTime(1904, 1, 1);
            result = result.AddSeconds(span);
            return result;
        }

        public static void WriteUInt32Time(Stream stream, DateTime value)
        {
            DateTime epoch = new DateTime(1904, 1, 1);
            TimeSpan span = value - epoch;
            BigEndianWriter.WriteUInt32(stream, (uint)span.TotalSeconds);
        }

        public static void WriteUInt64Time(Stream stream, DateTime value)
        {
            DateTime epoch = new DateTime(1904, 1, 1);
            TimeSpan span = value - epoch;
            BigEndianWriter.WriteUInt64(stream, (ulong)span.TotalSeconds);
        }

        public static double ReadFixedPoint8_8(Stream stream)
        {
            uint value = BigEndianReader.ReadUInt16(stream);
            return (double)value / 0x100;
        }

        public static double ReadSignedFixedPoint8_8(Stream stream)
        {
            double value = ReadFixedPoint8_8(stream);
            if (value > 127)
            {
                return -(value - 128);
            }
            return value;
        }

        public static double ReadFixedPoint16_16(Stream stream)
        {
            uint value = BigEndianReader.ReadUInt32(stream);
            return (double)value / 0x10000;
        }

        public static void WriteFixedPoint8_8(Stream stream, double value)
        {
            ushort valueToStore = (ushort)(value * 0x100);
            BigEndianWriter.WriteUInt16(stream, valueToStore);
        }

        public static void WriteSignedFixedPoint8_8(Stream stream, double value)
        {
            if (value < 0)
            {
                value = -value + 128;
            }
            WriteFixedPoint8_8(stream, value);
        }

        public static void WriteFixedPoint16_16(Stream stream, double value)
        {
            uint valueToStore = (uint)(value * 0x10000);
            BigEndianWriter.WriteUInt32(stream, valueToStore);
        }

        public static ushort EncodeLanguageCode(string language)
        {
            return (ushort)((((language[0] - 0x60) & 0x1F) << 10) + (((language[1] - 0x60) & 0x1F) << 5) + ((language[2] - 0x60) & 0x1F));
        }

        public static string DecodeLanguageCode(ushort languageCode)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append((char)(((languageCode >> 10) & 0x1F) + 0x60));
            builder.Append((char)(((languageCode >> 5) & 0x1F) + 0x60));
            builder.Append((char)((languageCode & 0x1F) + 0x60));
            return builder.ToString();
        }
    }
}
