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
    /*
    class DecoderConfigDescriptor extends BaseDescriptor : bit(8)
    tag=DecoderConfigDescrTag {
    bit(8) objectTypeIndication;
    bit(6) streamType;
    bit(1) upStream;
    const bit(1) reserved=1;
    bit(24) bufferSizeDB;
    bit(32) maxBitrate;
    bit(32) avgBitrate;
    DecoderSpecificInfo decSpecificInfo[0..1];
    profileLevelIndicationIndexDescriptor profileLevelIndicationIndexDescr[0..255];
    }
    */
    /// <summary>
    /// [ISO/IEC 14496-1] DecoderConfigDescriptor
    /// </summary>
    public class DecoderConfigDescriptor
    {
        public const byte DescriptorType = 0x04;
        public const int BaseLength = 13; // excluding the descriptor type and the length fields

        public ObjectTypeIndication ObjectTypeIndication;
        public StreamType StreamType; // 6 bits
        public bool Upstream;
        public bool Reserved;
        public uint BufferSizeDB; // 3 bytes
        public uint MaxBitRate;
        public uint AvgBitRate;

        public List<DecoderSpecificInfo> DecSpecificInfo = new List<DecoderSpecificInfo>();

        public DecoderConfigDescriptor()
        {
        }

        public DecoderConfigDescriptor(Stream stream)
        {
            byte descriptorType = (byte)stream.ReadByte();
            if (descriptorType != DescriptorType)
            {
                throw new Exception("Invalid descriptor type");
            }
            int length = ESDescriptor.ReadLength(stream);
            ObjectTypeIndication = (ObjectTypeIndication)stream.ReadByte();
            byte temp = (byte)stream.ReadByte();
            StreamType = (StreamType)((temp & 0xFC) >> 2);
            Upstream = (temp & 0x2) > 0;
            Reserved = (temp & 0x1) > 0;
            BufferSizeDB = BigEndianReader.ReadUInt24(stream);
            MaxBitRate = BigEndianReader.ReadUInt32(stream);
            AvgBitRate = BigEndianReader.ReadUInt32(stream);

            long endPosition = stream.Position + (length - BaseLength);
            while (stream.Position < endPosition)
            {
                // The existence and semantics of decoder specific information depends on the values of streamType and objectTypeIndication.
                DecoderSpecificInfo info = DecoderSpecificInfo.ReadFromStream(stream, StreamType, ObjectTypeIndication);
                DecSpecificInfo.Add(info);
            }
        }

        public void WriteBytes(Stream stream)
        {
            stream.WriteByte(DescriptorType);
            ESDescriptor.WriteLength(stream, Length - 2);
            stream.WriteByte((byte)ObjectTypeIndication);
            byte temp = (byte)((byte)StreamType << 2 | Convert.ToByte(Upstream) << 1 | Convert.ToByte(Reserved));
            stream.WriteByte(temp);
            BigEndianWriter.WriteUInt24(stream, BufferSizeDB);
            BigEndianWriter.WriteUInt32(stream, MaxBitRate);
            BigEndianWriter.WriteUInt32(stream, AvgBitRate);
            foreach (DecoderSpecificInfo info in DecSpecificInfo)
            {
                info.WriteBytes(stream);
            }
        }

        public int Length
        {
            get
            {
                int length = 15;
                foreach (DecoderSpecificInfo info in DecSpecificInfo)
                {
                    length += info.Length;
                }
                return length;
            }
        }
    }
}
