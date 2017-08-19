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
    public class VisualSampleEntry : SampleEntry
    {
        public byte[] Reserved1; // 16 bytes
        public ushort Width;
        public ushort Height;
        public uint HorizResolution; // 72 DPI
        public uint VertResolution; // 72 DPI
        public uint Reserved2;
        public ushort FrameCount; // frame_count, "indicates how many frames of compressed video are stored in each sample"
        public string CompressorName; // 32 bytes
        public byte[] Reserved4; // 4 bytes
        public ushort Depth;
        public short PreDefined3;

        public VisualSampleEntry(BoxType type) : base(type)
        {
            Reserved1 = new byte[16];
            HorizResolution = 0x00480000;
            VertResolution = 0x00480000;
            FrameCount = 1;
            Depth = 0x0018;
            PreDefined3 = -1;
        }

        public VisualSampleEntry(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            Reserved1 = ByteReader.ReadBytes(stream, 16);
            Width = BigEndianReader.ReadUInt16(stream);
            Height = BigEndianReader.ReadUInt16(stream);
            HorizResolution = BigEndianReader.ReadUInt32(stream);
            VertResolution = BigEndianReader.ReadUInt32(stream);
            Reserved2 = BigEndianReader.ReadUInt32(stream);
            FrameCount = BigEndianReader.ReadUInt16(stream);
            CompressorName = ByteReader.ReadAnsiString(stream, 32);
            Depth = BigEndianReader.ReadUInt16(stream);
            PreDefined3 = BigEndianReader.ReadInt16(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            ByteWriter.WriteBytes(stream, Reserved1);
            BigEndianWriter.WriteUInt16(stream, Width);
            BigEndianWriter.WriteUInt16(stream, Height);
            BigEndianWriter.WriteUInt32(stream, HorizResolution);
            BigEndianWriter.WriteUInt32(stream, VertResolution);
            BigEndianWriter.WriteUInt32(stream, Reserved2);
            BigEndianWriter.WriteUInt16(stream, FrameCount);
            ByteWriter.WriteAnsiString(stream, CompressorName, 32);
            BigEndianWriter.WriteUInt16(stream, Depth);
            BigEndianWriter.WriteInt16(stream, PreDefined3);
        }
    }
}
