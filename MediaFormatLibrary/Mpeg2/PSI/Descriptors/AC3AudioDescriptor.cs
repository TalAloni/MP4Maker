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
    // See: Digital Audio Compression Standard (AC-3)
    public class AC3AudioDescriptor : Descriptor
    {
        public byte SampleRateCode; // 3 bits
        public byte BSID; // 5 bits
        public byte BitRateCode; // 6 bits
        public byte SurroundMode; // 2 bits
        public byte BSMod; // 3 bits
        public byte NumChannels; // 4 bits
        public bool FullSvc;
        // Note: the descriptor contains allowable termination points, the fields below are not mandatory
        public byte? LangCode; // optional
        public byte? LangCode2; // optional
        public byte? MainID; // 3 bits, optional
        public byte Reserved; // 5 bits, optional
        public byte? AsvcFlags; // optional
        public bool IsAnsi;
        public string Text; // ANSI or Unicode
        // More optional fields

        public AC3AudioDescriptor()
        {
            this.Tag = DescriptorTag.AC3AudioDescriptor;
            Reserved = 0x1F;
        }

        public AC3AudioDescriptor(byte[] buffer, ref int offset) : base(buffer, ref offset)
        {
            BitStream bitStream = new BitStream(this.Data, true);
            SampleRateCode = (byte)bitStream.ReadBits(3);
            BSID = (byte)bitStream.ReadBits(5);
            BitRateCode = (byte)bitStream.ReadBits(6);
            SurroundMode = (byte)bitStream.ReadBits(2);
            BSMod = (byte)bitStream.ReadBits(3);
            NumChannels = (byte)bitStream.ReadBits(4);
            FullSvc = bitStream.ReadBoolean();

            if (bitStream.Position < bitStream.Length)
            {
                LangCode = bitStream.ReadByte();

                if (bitStream.Position < bitStream.Length)
                {
                    if (NumChannels == 0)
                    {
                        LangCode2 = bitStream.ReadByte();
                    }

                    if (bitStream.Position < bitStream.Length)
                    {
                        if (BSMod < 2)
                        {
                            MainID = (byte)bitStream.ReadBits(3);
                            Reserved = (byte)bitStream.ReadBits(5);
                        }
                        else
                        {
                            AsvcFlags = bitStream.ReadByte();
                        }


                        if (bitStream.Position < bitStream.Length)
                        {
                            byte textLength = (byte)bitStream.ReadBits(7);
                            IsAnsi = bitStream.ReadBoolean();
                            if (IsAnsi)
                            {
                                Text = ByteReader.ReadAnsiString(bitStream.BaseStream, textLength);
                            }
                            else
                            {
                                Text = ByteReader.ReadUTF16String(bitStream.BaseStream, textLength / 2);
                            }
                        }
                    }
                }
            }
        }

        public override void WriteBytes(byte[] buffer, ref int offset)
        {
            BitStream bitStream = new BitStream(true);
            bitStream.WriteBits(SampleRateCode, 3);
            bitStream.WriteBits(BSID, 5);
            bitStream.WriteBits(BitRateCode, 6);
            bitStream.WriteBits(SurroundMode, 2);
            bitStream.WriteBits(BSMod, 3);
            bitStream.WriteBits(NumChannels, 4);
            bitStream.WriteBoolean(FullSvc);

            if (LangCode.HasValue)
            {
                bitStream.WriteByte(LangCode.Value);

                if (NumChannels != 0 || (NumChannels == 0 && LangCode2.HasValue))
                {
                    if (NumChannels == 0)
                    {
                        bitStream.WriteByte(LangCode2.Value);
                    }

                    if (MainID.HasValue || AsvcFlags.HasValue)
                    {
                        if (BSMod < 2)
                        {
                            bitStream.WriteBits(MainID.Value, 3);
                            bitStream.WriteBits(Reserved, 5);
                        }
                        else
                        {
                            bitStream.WriteByte(AsvcFlags.Value);
                        }

                        if (Text != null)
                        {
                            if (IsAnsi)
                            {
                                bitStream.WriteBits((byte)(Text.Length), 7);
                                bitStream.WriteBoolean(IsAnsi);
                                ByteWriter.WriteAnsiString(bitStream.BaseStream, Text);
                            }
                            else
                            {
                                bitStream.WriteBits((byte)(Text.Length * 2), 7);
                                bitStream.WriteBoolean(IsAnsi);
                                ByteWriter.WriteUTF16String(bitStream.BaseStream, Text);
                            }
                        }
                    }
                }
            }

            this.Data = ((MemoryStream)bitStream.BaseStream).ToArray();
            base.WriteBytes(buffer, ref offset);
        }

        public override int Length
        {
            get
            {
                int count = Descriptor.HeaderLength + 3;

                if (LangCode.HasValue)
                {
                    count++;

                    if (NumChannels != 0 || (NumChannels == 0 && LangCode2.HasValue))
                    {
                        if (NumChannels == 0)
                        {
                            count++;
                        }

                        if (MainID.HasValue || AsvcFlags.HasValue)
                        {
                            count++;

                            if (Text != null)
                            {
                                count++;
                                if (IsAnsi)
                                {
                                    count += Text.Length;
                                }
                                else
                                {
                                    count += Text.Length * 2;
                                }
                            }
                        }
                    }
                }
                return count;
            }
        }

        /// <param name="bitrate">kbit/s</param>
        public static byte GetBitrateCode(int bitrate)
        {
            switch (bitrate)
            {
                case 32:
                    return 0;
                case 40:
                    return 1;
                case 48:
                    return 2;
                case 56:
                    return 3;
                case 64:
                    return 4;
                case 80:
                    return 5;
                case 96:
                    return 6;
                case 112:
                    return 7;
                case 128:
                    return 8;
                case 160:
                    return 9;
                case 192:
                    return 10;
                case 224:
                    return 11;
                case 256:
                    return 12;
                case 320:
                    return 13;
                case 384:
                    return 14;
                case 448:
                    return 15;
                case 512:
                    return 16;
                case 576:
                    return 17;
                case 640:
                    return 18;
                default:
                    throw new ArgumentException("Invalid bitrate");
            }
        }
    }
}
