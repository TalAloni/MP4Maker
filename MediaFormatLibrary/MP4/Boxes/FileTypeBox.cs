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
    public class FileTypeBox : Box
    {
        public FileBrand MajorBrand; // major_brand
        public uint MinorVersion; // minor_version
        public List<FileBrand> CompatibleBrands = new List<FileBrand>(); // compatible_brands

        public FileTypeBox() : base(BoxType.FileTypeBox)
        {
        }

        public FileTypeBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            MajorBrand = (FileBrand)BigEndianReader.ReadUInt32(stream);
            MinorVersion = BigEndianReader.ReadUInt32(stream);
            int count = (int)((this.Size - 16) / 4);
            for (int index = 0; index < count; index++)
            {
                CompatibleBrands.Add((FileBrand)BigEndianReader.ReadUInt32(stream));
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, (uint)MajorBrand);
            BigEndianWriter.WriteUInt32(stream, MinorVersion);
            foreach (FileBrand CompatibleBrand in CompatibleBrands)
            {
                BigEndianWriter.WriteUInt32(stream, (uint)CompatibleBrand);
            }
        }
    }
}
