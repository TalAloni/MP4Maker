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
    public class ProgramAssociationSection : ProgramSpecificInformationSection
    {
        public ushort TransportStreamID;
        public byte Reserved2; // 2 bits
        public byte VersionNumber; // 5 bits
        public bool CurrentNextIndicator; // Indicates that data is to be used at the present moment.
        public byte SectionNumber;
        public byte LastSectionNumber;
        public KeyValuePairList<ushort, ushort> Programs = new KeyValuePairList<ushort,ushort>();
        // uint CRC32;

        public ProgramAssociationSection()
        {
            Reserved2 = 0x03;
        }

        public ProgramAssociationSection(byte[] buffer, int offset)
        {
            int startOffset = offset;
            ReadSectionHeader(buffer, ref offset);
            TransportStreamID = BigEndianReader.ReadUInt16(buffer, ref offset);
            byte temp = ByteReader.ReadByte(buffer, ref offset);
            Reserved2 = (byte)((temp & 0xC0) >> 6);
            VersionNumber = (byte)((temp & 0x3E) >> 1);
            CurrentNextIndicator = (temp & 0x01) > 0;
            SectionNumber = ByteReader.ReadByte(buffer, ref offset);
            LastSectionNumber = ByteReader.ReadByte(buffer, ref offset);

            int programCount = (SectionLength - 9) / 4;
            for (int index = 0; index < programCount; index++)
            {
                ushort programNumber = BigEndianReader.ReadUInt16(buffer, ref offset);
                ushort temp2 = BigEndianReader.ReadUInt16(buffer, ref offset);
                ushort reserved3 = (ushort)((temp2 & 0xE000) >> 13);
                ushort pid = (ushort)(temp2 & 0x1FFF);
                Programs.Add(programNumber, pid);
            }
            uint expectedCRC32 = CRC32Mpeg.Compute(buffer, startOffset, offset - startOffset + 1);
            uint crc32 = BigEndianReader.ReadUInt32(buffer, ref offset); ;
            if (crc32 != expectedCRC32)
            {
                throw new InvalidDataException("CRC32 is invalid");
            }
        }

        public override void WriteBytes(byte[] buffer, int offset)
        {
            int startOffset = offset;
            WriteSectionHeader(buffer, ref offset);
            BigEndianWriter.WriteUInt16(buffer, ref offset, TransportStreamID);
            byte temp = (byte)(((Reserved2 & 0x03) << 6) |
                        ((VersionNumber & 0x1F) << 1) |
                        Convert.ToByte(CurrentNextIndicator));
            ByteWriter.WriteByte(buffer, ref offset, temp);
            ByteWriter.WriteByte(buffer, ref offset, SectionNumber);
            ByteWriter.WriteByte(buffer, ref offset, LastSectionNumber);
            foreach (KeyValuePair<ushort, ushort> entry in Programs)
            {
                BigEndianWriter.WriteUInt16(buffer, ref offset, entry.Key);
                int reserved = 0x07;
                ushort temp2 = (ushort)((reserved << 13) | (entry.Value & 0x1FFF));
                BigEndianWriter.WriteUInt16(buffer, ref offset, temp2);
            }

            uint crc32 = CRC32Mpeg.Compute(buffer, startOffset, offset - startOffset + 1);
            BigEndianWriter.WriteUInt32(buffer, ref offset, crc32);
        }

        public override int Length
        {
            get
            {
                return HeaderLength + 5 + Programs.Count * 4 + 4;
            }
        }

        public override byte TableID
        {
            get
            {
                return (byte)TableName.ProgramAssociation;
            }
        }
    }
}
