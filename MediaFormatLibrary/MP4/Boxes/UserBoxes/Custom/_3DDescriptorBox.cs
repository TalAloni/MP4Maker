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
    /// <summary>
    /// We assume 3DDS means 3D Description
    /// </summary>
    public class _3DDescriptorBox : UserBox
    {
        public static readonly Guid UserTypeGuid = new Guid("33444453-21d2-4fce-bb88-695cfac9c740"); // the first 4 bytes equals to '3DDS'

        public uint Flags; // Undocumented, probably a flags field (Seen: 0x82811002)

        public _3DDescriptorBox() : base(UserTypeGuid)
        {
        }

        public _3DDescriptorBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            Flags = BigEndianReader.ReadUInt32(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, Flags);
        }
    }
}
