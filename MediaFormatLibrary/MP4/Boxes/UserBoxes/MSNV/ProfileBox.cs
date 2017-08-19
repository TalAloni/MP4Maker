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
    /// [IEC/TS 62592] ProfileBox
    /// </summary>
    public class ProfileBox : UserBox
    {
        public static readonly Guid UserTypeGuid = new Guid("50524f46-21d2-4fce-bb88-695cfac9c740"); // the first 4 bytes equals to 'PROF'

        public byte Version;
        public uint Flags; // 3 bytes

        public ProfileBox() : base(UserTypeGuid)
        {
        }

        public ProfileBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            Version = (byte)stream.ReadByte();
            Flags = MP4Helper.ReadUInt24(stream);
            uint entryCount = BigEndianReader.ReadUInt32(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            stream.WriteByte(Version);
            MP4Helper.WriteUInt24(stream, Flags);
            BigEndianWriter.WriteUInt32(stream, (uint)Children.Count);
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
