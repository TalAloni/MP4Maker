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
    public class UserBox : Box
    {
        private Guid m_userType;
        public byte[] Payload;

        public UserBox(Guid userType) : base(BoxType.UserBox)
        {
            m_userType = userType;
        }

        public UserBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            m_userType = BigEndianReader.ReadGuidBytes(stream);
            // We do not want to read the payload if the current class inherits UserBox, because it will have its own implementation
            if (this.GetType() == typeof(UserBox)) // We check if the current class is UserBox and not a class that inherits from UserBox
            {
                Payload = ByteReader.ReadBytes(stream, (int)this.Size - 24);
            }
        }

        public static string GetByteArrayString(byte[] array)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in array)
            {
                builder.Append(b.ToString("X2")); // 2 digit hex
                builder.Append(" ");
            }
            return builder.ToString();
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteGuidBytes(stream, m_userType);
            if (this.GetType() == typeof(UserBox)) // We check if the current class is UserBox and not a class that inherits from UserBox
            {
                ByteWriter.WriteBytes(stream, Payload);
            }
        }

        public virtual Guid UserType
        {
            get
            {
                return m_userType;
            }
        }
    }
}
