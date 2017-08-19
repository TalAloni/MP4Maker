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
    class ES_Descriptor extends BaseDescriptor : bit(8) tag=ES_DescrTag {
    bit(16) ES_ID;
    bit(1) streamDependenceFlag;
    bit(1) URL_Flag;
    bit(1) OCRstreamFlag;
    bit(5) streamPriority;
    if (streamDependenceFlag)
    bit(16) dependsOn_ES_ID;
    if (URL_Flag) {
    bit(8) URLlength;
    bit(8) URLstring[URLlength];
    }
    if (OCRstreamFlag)
    bit(16) OCR_ES_Id;
    DecoderConfigDescriptor decConfigDescr;
    if (ODProfileLevelIndication==0x01) //no SL extension.
    {
    SLConfigDescriptor slConfigDescr;
    }
    else // SL extension is possible.
    {
    SLConfigDescriptor slConfigDescr;
    }
    IPI_DescrPointer ipiPtr[0 .. 1];
    IP_IdentificationDataSet ipIDS[0 .. 255];
    IPMP_DescriptorPointer ipmpDescrPtr[0 .. 255];
    LanguageDescriptor langDescr[0 .. 255];
    QoS_Descriptor qosDescr[0 .. 1];
    RegistrationDescriptor regDescr[0 .. 1];
    ExtensionDescriptor extDescr[0 .. 255];
    }
    */
    /// <summary>
    /// [ISO/IEC 14496-1] ES_Descriptor
    /// See also: http://mp4parser.googlecode.com/svn/site/1.0-RC-4/xref/com/googlecode/mp4parser/boxes/mp4/objectdescriptors/ESDescriptor.html
    /// </summary>
    public class ESDescriptor
    {
        public const byte DescriptorType = 0x03;
        public const int BaseLength = 3; // excluding the descriptor type and the length fields

        public ushort ESID; // ES_ID
        // bool StreamDependencyFlag;
        // bool UrlFlag; // URL_Flag
        // bool OCRStreamFlag;
        public byte StreamPriority; // 5 bits
        public ushort? DependsOnESID; // if StreamDependencyFlag
        public ushort? OcrESID; // OCR_ES_Id, if OCRStreamFlag
        public string Url; // if UrlFlag

        public DecoderConfigDescriptor DecoderConfigDescriptor;
        public SLConfigDescriptor SLConfigDescriptor;

        public ESDescriptor()
        {
        }

        public ESDescriptor(Stream stream)
        {
            byte descriptorType = (byte)stream.ReadByte();
            if (descriptorType != DescriptorType)
            {
                throw new Exception("Invalid descriptor type");
            }
            int length = ReadLength(stream);
            ESID = BigEndianReader.ReadUInt16(stream);
            byte temp = (byte)stream.ReadByte();
            bool streamDependencyFlag = (temp & 0x80) > 0;
            bool urlFlag = (temp & 0x40) > 0;
            bool ocrStreamFlag = (temp & 0x20) > 0;
            StreamPriority = (byte)(temp & 0x1F);
            if (streamDependencyFlag)
            {
                DependsOnESID = BigEndianReader.ReadUInt16(stream);
            }
            if (urlFlag)
            {
                int urlLength = (byte)stream.ReadByte();
                Url = ByteReader.ReadAnsiString(stream, urlLength);
            }
            if (ocrStreamFlag)
            {
                OcrESID = BigEndianReader.ReadUInt16(stream);
            }

            long endPosition = stream.Position + (length - BaseLength);
            if (stream.Position < endPosition)
            {
                DecoderConfigDescriptor = new DecoderConfigDescriptor(stream);
            }
            if (stream.Position < endPosition)
            {
                SLConfigDescriptor = new SLConfigDescriptor(stream);
            }
        }

        public void WriteBytes(Stream stream)
        {
            stream.WriteByte(DescriptorType);
            WriteLength(stream, Length - 2);
            BigEndianWriter.WriteUInt16(stream, ESID);
            byte temp = (byte)(StreamPriority << 3 | Convert.ToByte(DependsOnESID.HasValue) << 2 | Convert.ToByte(Url != null) << 1 | Convert.ToByte(OcrESID.HasValue));
            stream.WriteByte(temp);
            if (DependsOnESID.HasValue)
            {
                BigEndianWriter.WriteUInt16(stream, DependsOnESID.Value);
            }
            if (Url != null)
            {
                stream.WriteByte((byte)Url.Length);
                ByteWriter.WriteAnsiString(stream, Url);
            }
            if (OcrESID.HasValue)
            {
                BigEndianWriter.WriteUInt16(stream, OcrESID.Value);
            }
            if (DecoderConfigDescriptor != null)
            {
                DecoderConfigDescriptor.WriteBytes(stream);
            }
            if (SLConfigDescriptor != null)
            {
                SLConfigDescriptor.WriteBytes(stream);
            }
        }

        public int Length
        {
            get
            {
                int length = 5;
                if (DependsOnESID.HasValue)
                {
                    length += 2;
                }

                if (Url != null)
                {
                    length += Url.Length + 1;
                }

                if (OcrESID.HasValue)
                {
                    length += 2;
                }

                if (DecoderConfigDescriptor != null)
                {
                    length += DecoderConfigDescriptor.Length;
                }

                if (SLConfigDescriptor != null)
                {
                    length += SLConfigDescriptor.Length;
                }
                return length;
            }
        }

        public static int ReadLength(Stream stream)
        {
            int length = 0;
            int numBytes = 0;
            byte b;
            do
            {
                b = (byte)stream.ReadByte();
                numBytes++;
                length = (length << 7) | (b & 0x7F);
            }
            while ((b & 0x80) > 0 && numBytes < 4);
            return length;
        }

        public static void WriteLength(Stream stream, int length)
        {
            WriteLength(stream, length, true);
        }

        public static void WriteLength(Stream stream, int length, bool compact)
        {
            int numBytes;
            if (compact)
            {
                if (length <= 0x7F)
                {
                    numBytes = 1;
                }
                else if (length <= 0x3FFF)
                {
                    numBytes = 2;
                }
                else if (length <= 0x1FFFFF)
                {
                    numBytes = 3;
                }
                else
                {
                    numBytes = 4;
                }
            }
            else
            {
                numBytes = 4;
            }

            for (int i = numBytes - 1; i >= 0; i--)
            {
                byte b = (byte)((length >> (i * 7)) & 0x7F);
                if (i != 0)
                {
                    b |= 0x80;
                }
                stream.WriteByte(b);
            }
        }
    }
}
