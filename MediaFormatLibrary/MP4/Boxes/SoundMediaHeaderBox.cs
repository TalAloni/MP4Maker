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
    public class SoundMediaHeaderBox : FullBox
    {
        public double Balance; // Signed Fixed point: 8(bits).8(bits)
        public ushort Reserved;

        public SoundMediaHeaderBox() : base(BoxType.SoundMediaHeaderBox)
        {
        }

        public SoundMediaHeaderBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            if (Version == 0)
            {
                Balance = MP4Helper.ReadSignedFixedPoint8_8(stream);
                Reserved = BigEndianReader.ReadUInt16(stream);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            MP4Helper.WriteSignedFixedPoint8_8(stream, Balance);
            BigEndianWriter.WriteUInt16(stream, Reserved);
        }
    }
}
