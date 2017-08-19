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
    public class DataReferenceBox : FullBox
    {
        public DataReferenceBox() : base(BoxType.DataReferenceBox)
        {}

        public DataReferenceBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            uint entryCount = BigEndianReader.ReadUInt32(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)this.Children.Count);
        }

        public override BoxContentType ContentType
        {
            get
            {
                return BoxContentType.DataAndChildren;
            }
        }
    }
}
