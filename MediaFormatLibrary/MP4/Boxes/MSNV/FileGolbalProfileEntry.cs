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
    /// <summary>
    /// [IEC/TS 62592] FileGolbalProfileEntry
    /// </summary>
    public class FileGolbalProfileEntry : FullBox
    {
        public uint FunctionFlags; // function_flags
        public uint Reserved;

        public FileGolbalProfileEntry() : base(BoxType.FileGlobalProfileEntry)
        {
        }

        public FileGolbalProfileEntry(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            FunctionFlags = BigEndianReader.ReadUInt32(stream);
            Reserved = BigEndianReader.ReadUInt32(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, FunctionFlags);
            BigEndianWriter.WriteUInt32(stream, Reserved);
        }
    }
}
