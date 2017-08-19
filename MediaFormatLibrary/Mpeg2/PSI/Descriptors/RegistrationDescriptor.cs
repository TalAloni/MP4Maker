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

namespace MediaFormatLibrary.Mpeg2
{
    public class RegistrationDescriptor : Descriptor
    {
        public FormatIdentifier FormatIdentifier;
        public byte[] AdditionalIdentificationInfo;

        public RegistrationDescriptor()
        {
            Tag = DescriptorTag.RegistrationDescriptor;
            AdditionalIdentificationInfo = new byte[0];
        }

        public RegistrationDescriptor(byte[] buffer, ref int offset) : base(buffer, ref offset)
        {
            FormatIdentifier = (FormatIdentifier)BigEndianConverter.ToUInt32(this.Data, 0);
            AdditionalIdentificationInfo = ByteReader.ReadBytes(this.Data, 4, this.Data.Length - 4);
        }

        public override void WriteBytes(byte[] buffer, ref int offset)
        {
            this.Data = new byte[4 + AdditionalIdentificationInfo.Length];
            BigEndianWriter.WriteUInt32(this.Data, 0, (uint)FormatIdentifier);
            ByteWriter.WriteBytes(this.Data, 4, AdditionalIdentificationInfo);
            
            base.WriteBytes(buffer, ref offset);
        }

        public override int Length
        {
            get
            {
                return HeaderLength + 4 + AdditionalIdentificationInfo.Length;
            }
        }
    }
}
