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
    public class MovieFragmentHeaderBox : FullBox
    {
        public uint SequenceNumber;

        public MovieFragmentHeaderBox() : base(BoxType.MovieFragmentHeaderBox)
        {
        }

        public MovieFragmentHeaderBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            SequenceNumber = BigEndianReader.ReadUInt32(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, SequenceNumber);
        }
    }
}
