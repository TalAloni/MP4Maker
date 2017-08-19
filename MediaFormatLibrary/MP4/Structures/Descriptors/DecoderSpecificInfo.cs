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
    /// <summary>
    /// [ISO/IEC 14496-1] DecoderSpecificInfo
    /// </summary>
    public class DecoderSpecificInfo
    {
        public const byte DescriptorType = 0x05;

        public byte[] Info;

        public DecoderSpecificInfo()
        {
        }

        public DecoderSpecificInfo(Stream stream)
        {
            byte descriptorType = (byte)stream.ReadByte();
            if (descriptorType != DescriptorType)
            {
                throw new Exception("Invalid descriptor type");
            }
            int length = ESDescriptor.ReadLength(stream);
            Info = ByteReader.ReadBytes(stream, length);
        }

        public virtual void WriteBytes(Stream stream)
        {
            stream.WriteByte(DescriptorType);
            ESDescriptor.WriteLength(stream, Info.Length);
            ByteWriter.WriteBytes(stream, Info);
        }

        public virtual int Length
        {
            get
            {
                return 2 + Info.Length;
            }
        }

        // [ISO/IEC 14496-1] "The existence and semantics of decoder specific information depends on the values of streamType and objectTypeIndication."
        public static DecoderSpecificInfo ReadFromStream(Stream stream, StreamType streamType, ObjectTypeIndication objectType)
        {
            if (streamType == StreamType.AudioStream && objectType == ObjectTypeIndication.Mpeg4AAC)
            {
                return new AudioSpecificConfig(stream);
            }
            else
            {
                return new DecoderSpecificInfo(stream);
            }
        }
    }
}
