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
    public class ProgramMapSection : ProgramSpecificInformationSection
    {
        public ushort ProgramNumber;
        public byte Reserved2; // 2 bits
        public byte VersionNumber; // 5 bits
        public bool CurrentNextIndicator;
        public byte SectionNumber;
        public byte LastSectionNumber;
        public byte Reserved3; // 3 bits
        public ushort PCRPID; // PCR_PID, 13 bits
        public byte Reserved4; // 4 bits
        // ushort ProgramInfoLength; // 12 bits
        public List<Descriptor> ProgramDescriptors = new List<Descriptor>();
        public List<ElementaryStreamEntry> StreamEntries = new List<ElementaryStreamEntry>();
        // uint CRC32;

        public ProgramMapSection()
        {
            Reserved2 = 0x03;
            Reserved3 = 0x07;
            Reserved4 = 0x0F;
        }

        public ProgramMapSection(byte[] buffer, int offset)
        {
            int startOffset = offset;
            ReadSectionHeader(buffer, ref offset);
            ProgramNumber = BigEndianReader.ReadUInt16(buffer, ref offset);
            byte temp = ByteReader.ReadByte(buffer, ref offset);
            Reserved2 = (byte)(temp & 0xC0);
            VersionNumber = (byte)(temp & 0x3E);
            CurrentNextIndicator = (temp & 0x01) > 0;
            SectionNumber = ByteReader.ReadByte(buffer, ref offset);
            LastSectionNumber = ByteReader.ReadByte(buffer, ref offset);
            ushort temp2 = BigEndianReader.ReadUInt16(buffer, ref offset);
            Reserved3 = (byte)(temp2 & 0xE0);
            PCRPID = (ushort)(temp2 & 0x1FFF);
            temp2 = BigEndianReader.ReadUInt16(buffer, ref offset);
            Reserved4 = (byte)((temp2 & 0xF000) >> 12);
            ushort programInfoLength = (ushort)(temp2 & 0x0FFF);
            ProgramDescriptors = Descriptor.ReadDescriptorList(buffer, ref offset, programInfoLength);

            int entriesStartOffset = offset;
            int entriesListLength = this.SectionLength - (9 + programInfoLength + 4);
            while (offset < entriesStartOffset + entriesListLength)
            {
                ElementaryStreamEntry entry = new ElementaryStreamEntry(buffer, ref offset);
                StreamEntries.Add(entry);
            }

            if (offset != entriesStartOffset + entriesListLength)
            {
                throw new InvalidDataException("The entries list did not match expected length");
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
            BigEndianWriter.WriteUInt16(buffer, ref offset, ProgramNumber);
            byte temp = (byte)(((Reserved2 & 0x03) << 6) |
                        ((VersionNumber & 0x1F) << 1) |
                        Convert.ToByte(CurrentNextIndicator));
            ByteWriter.WriteByte(buffer, ref offset, temp);
            ByteWriter.WriteByte(buffer, ref offset, SectionNumber);
            ByteWriter.WriteByte(buffer, ref offset, LastSectionNumber);
            ushort temp2 = (ushort)(((Reserved3 & 0x07) << 13) |
                                    (PCRPID & 0x1FFF));
            BigEndianWriter.WriteUInt16(buffer, ref offset, temp2);
            ushort programInfoLength = (ushort)Descriptor.GetDescriptorListLength(ProgramDescriptors);
            temp2 = (ushort)(((Reserved4 & 0x0F) << 12) |
                              (programInfoLength & 0x1FFF));
            BigEndianWriter.WriteUInt16(buffer, ref offset, temp2);
            Descriptor.WriteDescriptorList(buffer, ref offset, ProgramDescriptors);

            foreach (ElementaryStreamEntry entry in StreamEntries)
            {
                entry.WriteBytes(buffer, ref offset);
            }

            uint crc32 = CRC32Mpeg.Compute(buffer, startOffset, offset - startOffset + 1);
            BigEndianWriter.WriteUInt32(buffer, ref offset, crc32);
        }

        public override int Length
        {
            get
            {
                int result = HeaderLength + 9 + Descriptor.GetDescriptorListLength(ProgramDescriptors);
                foreach(ElementaryStreamEntry entry in StreamEntries)
                {
                    result += entry.Length;
                }
                result += 4;
                return result;
            }
        }

        public override byte TableID
        {
            get
            {
                return (byte)TableName.ProgramMap;
            }
        }
    }
}
