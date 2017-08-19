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
    public class Descriptor
    {
        public const int HeaderLength = 2;

        public DescriptorTag Tag;
        // byte Length; // number of bytes of the descriptor immediately following this field
        public byte[] Data;

        public Descriptor()
        {
            Data = new byte[0];
        }

        public Descriptor(byte[] buffer, ref int offset)
        {
            Tag = (DescriptorTag)ByteReader.ReadByte(buffer, ref offset);
            byte length = ByteReader.ReadByte(buffer, ref offset);
            Data = ByteReader.ReadBytes(buffer, ref offset, length);
        }

        public virtual void WriteBytes(byte[] buffer, ref int offset)
        {
            ByteWriter.WriteByte(buffer, ref offset, (byte)Tag);
            ByteWriter.WriteByte(buffer, ref offset, (byte)Data.Length);
            ByteWriter.WriteBytes(buffer, ref offset, Data);
        }

        public virtual int Length
        {
            get
            {
                return HeaderLength + Data.Length;
            }
        }

        public static Descriptor ReadDescriptor(byte[] buffer, ref int offset)
        {
            byte tag = buffer[offset];
            switch ((DescriptorTag)tag)
            {
                case DescriptorTag.RegistrationDescriptor:
                    return new RegistrationDescriptor(buffer, ref offset);
                case DescriptorTag.AVCVideoDescriptor:
                    return new AVCVideoDescriptor(buffer, ref offset);
                case DescriptorTag.PartialTransportStreamDescriptor:
                    return new PartialTransportStreamDescriptor(buffer, ref offset);
                case DescriptorTag.AC3AudioDescriptor:
                    return new AC3AudioDescriptor(buffer, ref offset);
                case DescriptorTag.DigitalCopyProtectionDescriptor:
                    return new DigitalCopyProtectionDescriptor(buffer, ref offset);
                default:
                    return new Descriptor(buffer, ref offset);
            }
        }

        public static List<Descriptor> ReadDescriptorList(byte[] buffer, ref int offset, int descriptorListLength)
        {
            List<Descriptor> result = new List<Descriptor>();
            int startOffset = offset;
            while (offset < startOffset + descriptorListLength)
            {
                Descriptor entry = ReadDescriptor(buffer, ref offset);
                result.Add(entry);
            }

            if (offset != startOffset + descriptorListLength)
            {
                throw new InvalidDataException("The descriptor list did not match expected length");
            }
            return result;
        }

        public static void WriteDescriptorList(byte[] buffer, ref int offset, List<Descriptor> descriptorList)
        {
            foreach (Descriptor entry in descriptorList)
            {
                entry.WriteBytes(buffer, ref offset);
            }
        }

        public static int GetDescriptorListLength(List<Descriptor> descriptorList)
        {
            int result = 0;
            foreach (Descriptor entry in descriptorList)
            {
                result += entry.Length;
            }
            return result;
        }
    }
}
