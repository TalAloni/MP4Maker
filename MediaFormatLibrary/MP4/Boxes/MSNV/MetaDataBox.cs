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
    public enum EntryType : uint
    {
        Title = 0x01,
        ProductionDate = 0x03,
        Software = 0x04,
        Product = 0x05,
        TrackProperty = 0x0A,
        TimeZoneOffset = 0x0B,
        ModificationDate = 0x0C,
    }

    public enum PayloadType : ushort
    {
        BinaryData = 0x00,
        UnicodeString = 0x01, // UTF-16 BE
    }

    public class MetaDataEntry
    {
        public EntryType EntryType;
        public bool ReadOnlyFlag;
        public LanguageCode LanguageCode; // There is 1 extra bit, which is sometimes set, not sure if it means anything
        public object Payload;

        public MetaDataEntry()
        {
        }

        public MetaDataEntry(EntryType entryType, LanguageCode languageCode, object payload)
        {
            EntryType = entryType;
            LanguageCode = languageCode;
            Payload = payload;
        }
    }

    /// <summary>
    /// [IEC/TS 62592] UserSpecificMetadataBox
    /// </summary>
    public class MetaDataBox : Box
    {
        public List<MetaDataEntry> Entries = new List<MetaDataEntry>();

        public MetaDataBox() : base(BoxType.MetaDataBox)
        {}

        public MetaDataBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            ushort entryCount = BigEndianReader.ReadUInt16(stream);
            for (int index = 0; index < entryCount; index++)
            {
                ushort entrySize = BigEndianReader.ReadUInt16(stream);
                int payloadSize = entrySize - 10;
                MetaDataEntry entry = new MetaDataEntry();
                entry.EntryType = (EntryType)BigEndianReader.ReadUInt32(stream);
                ushort temp = BigEndianReader.ReadUInt16(stream);
                entry.ReadOnlyFlag = (temp & 0x8000) > 0;
                entry.LanguageCode = (LanguageCode)(temp & 0x7FFF);
                PayloadType payloadType = (PayloadType)BigEndianReader.ReadUInt16(stream);
                if (payloadType == PayloadType.UnicodeString)
                {
                    string payloadString = ByteReader.ReadNullTerminatedUTF16BEString(stream);
                    if (payloadString.Length * 2 + 2 != payloadSize)
                    {
                        throw new Exception("Invalid payload length");
                    }
                    entry.Payload = payloadString;
                }
                else if (payloadType == PayloadType.BinaryData)
                {
                    byte[] array = ByteReader.ReadBytes(stream, payloadSize);
                    entry.Payload = array;
                }
                else
                {
                    throw new NotImplementedException("Unknown payload type");
                }
                Entries.Add(entry);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt16(stream, (ushort)Entries.Count);
            foreach (MetaDataEntry entry in Entries)
            {
                PayloadType payloadType;
                ushort payloadSize;
                if (entry.Payload is string)
                {
                    payloadType = PayloadType.UnicodeString;
                    payloadSize = (ushort)(((string)entry.Payload).Length * 2 + 2);
                }
                else if (entry.Payload is byte[])
                {
                    payloadType = PayloadType.BinaryData;
                    payloadSize = (ushort)((byte[])entry.Payload).Length;
                }
                else
                {
                    throw new NotImplementedException("Payload not supported");
                }
                ushort entrySize = (ushort)(payloadSize + 10);
                BigEndianWriter.WriteUInt16(stream, entrySize);
                BigEndianWriter.WriteUInt32(stream, (uint)entry.EntryType);
                BigEndianWriter.WriteUInt16(stream, (ushort)(((ushort)entry.LanguageCode | (Convert.ToByte(entry.ReadOnlyFlag) << 15))));
                BigEndianWriter.WriteUInt16(stream, (ushort)payloadType);
                if (entry.Payload is string)
                {
                    ByteWriter.WriteNullTerminatedUTF16BEString(stream, (string)entry.Payload);
                }
                else
                {
                    ByteWriter.WriteBytes(stream, (byte[])entry.Payload);
                }
            }
        }
    }
}
