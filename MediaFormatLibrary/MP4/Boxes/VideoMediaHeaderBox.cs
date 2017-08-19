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
    public class VideoMediaHeaderBox : FullBox
    {
        public ushort GraphicsMode;
        public ushort OpColor1;
        public ushort OpColor2;
        public ushort OpColor3;

        public VideoMediaHeaderBox() : base(BoxType.VideoMediaHeaderBox)
        {
            Flags = 1; // See ISO/IEC 14496-12
        }

        public VideoMediaHeaderBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            GraphicsMode = BigEndianReader.ReadUInt16(stream);
            OpColor1 = BigEndianReader.ReadUInt16(stream);
            OpColor2 = BigEndianReader.ReadUInt16(stream);
            OpColor3 = BigEndianReader.ReadUInt16(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt16(stream, GraphicsMode);
            BigEndianWriter.WriteUInt16(stream, OpColor1);
            BigEndianWriter.WriteUInt16(stream, OpColor2);
            BigEndianWriter.WriteUInt16(stream, OpColor3);
        }
    }
}
