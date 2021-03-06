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
    public enum DataEntryFlags : uint // 24 bits
    {
        SelfContained = 0x000001,
    }

    public class DataEntryUrlBox : FullBox
    {
        public string Location;

        public DataEntryUrlBox() : base(BoxType.DataEntryUrlBox)
        {}

        public DataEntryUrlBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            if ((DataEntryFlags)Flags != DataEntryFlags.SelfContained)
            {
                Location = ByteReader.ReadNullTerminatedUTF8String(stream);
            }
        }

        public override void WriteData(Stream stream)
        {
            if (Location == null)
            {
                Flags = (uint)DataEntryFlags.SelfContained;
            }
            base.WriteData(stream);
            if (Location != null)
            {
                ByteWriter.WriteNullTerminatedUTF8String(stream, Location);
            }
        }
    }
}
