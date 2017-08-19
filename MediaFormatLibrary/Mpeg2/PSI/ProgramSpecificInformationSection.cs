/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.Mpeg2
{
    public abstract class ProgramSpecificInformationSection
    {
        /// <summary>
        /// Excluding pointer and filler bytes
        /// </summary>
        public const int HeaderLength = 3;

        // byte TableID
        public bool SectionSyntaxIndicator; // signals whether the section conforms to the "long" form
        public bool TableSpecificIndicator;
        public byte Reserved1; // 2 bits
        protected ushort SectionLength; // 12 bits, the number of bytes that follow (including CRC)

        public ProgramSpecificInformationSection()
        {
            SectionSyntaxIndicator = true;
            Reserved1 = 0x03;
        }

        public void ReadSectionHeader(byte[] buffer, ref int offset)
        {
            byte tableID = ByteReader.ReadByte(buffer, ref offset);
            ushort temp = BigEndianReader.ReadUInt16(buffer, ref offset);
            SectionSyntaxIndicator = (temp & 0x8000) > 0;
            TableSpecificIndicator = (temp & 0x4000) > 0;
            Reserved1 = (byte)((temp & 0x3000) >> 12);
            SectionLength = (ushort)(temp & 0x0FFF);
        }

        public void WriteSectionHeader(byte[] buffer, ref int offset)
        {
            this.SectionLength = (ushort)(this.Length - HeaderLength);
            ByteWriter.WriteByte(buffer, ref offset, this.TableID);
            ushort temp = (ushort)((Convert.ToByte(SectionSyntaxIndicator) << 15 |
                        Convert.ToByte(TableSpecificIndicator) << 14) |
                        ((Reserved1 & 0x03) << 12) |
                        (SectionLength & 0x0FFF));
            BigEndianWriter.WriteUInt16(buffer, ref offset, temp);
        }

        public abstract void WriteBytes(byte[] buffer, int offset);

        /// <summary>
        /// Including pointer field
        /// </summary>
        public byte[] GetBytes()
        {
            byte[] buffer = new byte[1 + this.Length];
            WriteBytes(buffer, 1);
            return buffer;
        }

        /// <summary>
        /// Excluding pointer and filler bytes
        /// </summary>
        public abstract int Length
        {
            get;
        }

        public abstract byte TableID
        {
            get;
        }

        public static bool IsSectionComplete(byte[] buffer)
        {
            byte pointer = buffer[0];
            ushort temp = BigEndianConverter.ToUInt16(buffer, 1 + pointer + 1);
            ushort sectionLength = (ushort)(temp & 0x0FFF);
            return (buffer.Length >= 1 + pointer + 3 + sectionLength);
        }

        public static ProgramSpecificInformationSection ReadSection(byte[] buffer)
        {
            byte pointer = buffer[0];
            int offset = 1 + pointer;
            byte tableID = buffer[offset];
            switch ((TableName)tableID)
            {
                case TableName.ProgramAssociation:
                    return new ProgramAssociationSection(buffer, offset);
                case TableName.ProgramMap:
                    return new ProgramMapSection(buffer, offset);
                case TableName.SelectionInformationSection:
                    return new SelectionInformationSection(buffer, offset);
                default:
                    return null;
            }
        }

    }
}
