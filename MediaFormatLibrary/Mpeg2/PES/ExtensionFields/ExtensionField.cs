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
    public class ExtensionField
    {
        public bool PrivateDataFlag; // PES_private_data_flag
        public bool PackHeaderFieldFlag; // pack_header_field_flag
        public bool ProgramPacketSequenceCounterFlag; // program_packet_sequence_counter_flag
        public bool PSTDBufferFlag; // P-STD_buffer_flag
        public byte Reserved; // 3 bits
        public bool ExtensionFlag2; // PES_extension_flag_2

        public byte[] PrivateData; // 16 bytes, PES_private_data
        // bool MarkerBits; // 2 bits
        public bool BufferScale; // P-STD_buffer_scale
        public ushort BufferSize; // 13 bits, P-STD_buffer_size
        // bool MarkerBit1;
        public byte ProgramPacketSequenceCounter; // 7 bits, program_packet_sequence_counter
        // bool MarkerBit2;
        public bool Mpeg1Mpeg2Identifier; // MPEG1_MPEG2_identifier
        public byte OriginalStuffLength; // 6 bits, original_stuff_length
        // bool MarkerBit3;
        public byte ExtensionFieldLength; // 7 bits, PES_extension_field_length
        public byte[] ExtensionReserved;

        public ExtensionField()
        {
        }

        public ExtensionField(byte[] buffer, ref int offset)
        {
            byte temp = ByteReader.ReadByte(buffer, ref offset);
            PrivateDataFlag = (temp & 0x80) > 0;
            PackHeaderFieldFlag = (temp & 0x40) > 0;
            ProgramPacketSequenceCounterFlag = (temp & 0x20) > 0;
            PSTDBufferFlag = (temp & 0x10) > 0;
            Reserved = (byte)((temp & 0x0E) >> 1);
            ExtensionFlag2 = (temp & 0x01) > 0;

            if (PrivateDataFlag)
            {
                PrivateData = ByteReader.ReadBytes(buffer, ref offset, 16);
            }
            if (PackHeaderFieldFlag)
            {
                throw new NotImplementedException();
            }
            if (ProgramPacketSequenceCounterFlag)
            {
                temp = ByteReader.ReadByte(buffer, ref offset);
                bool markerBit1 = (temp & 0x80) > 0;
                if (!markerBit1)
                {
                    throw new InvalidDataException("Invalid marker bit");
                }
                ProgramPacketSequenceCounter = (byte)(temp & 0xE0);
                temp = ByteReader.ReadByte(buffer, ref offset);
                bool markerBit2 = (temp & 0x80) > 0;
                if (!markerBit2)
                {
                    throw new InvalidDataException("Invalid marker bit");
                }
                Mpeg1Mpeg2Identifier = (temp & 0x40) > 0;
                OriginalStuffLength = (byte)(temp & 0x3F);
            }
            if (PSTDBufferFlag)
            {
                ushort pstdField = BigEndianReader.ReadUInt16(buffer, ref offset);
                byte markerBits = (byte)((pstdField & 0xC000) >> 14);
                if (markerBits != 0x01)
                {
                    throw new InvalidDataException("Invalid marker bits");
                }
                BufferScale = (pstdField & 0x2000) > 0;
                BufferSize = (ushort)(pstdField & 0x1FFF);
            }
            if (ExtensionFlag2)
            {
                temp = ByteReader.ReadByte(buffer, ref offset);
                bool markerBit3 = (temp & 0x80) > 0;
                if (!markerBit3)
                {
                    throw new InvalidDataException("Invalid marker bit");
                }
                ExtensionFieldLength = (byte)(temp & 0x7F);
                ExtensionReserved = ByteReader.ReadBytes(buffer, ref offset, ExtensionFieldLength);
            }
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            int bitOffset = offset * 8;
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, PrivateDataFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, PackHeaderFieldFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, ProgramPacketSequenceCounterFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, PSTDBufferFlag);
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, Reserved, 3);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, ExtensionFlag2);
            offset++;

            if (PrivateDataFlag)
            {
                ByteWriter.WriteBytes(buffer, ref offset, PrivateData, 16);
            }
            if (PackHeaderFieldFlag)
            {
                throw new NotImplementedException();
            }
            if (ProgramPacketSequenceCounterFlag)
            {
                throw new NotImplementedException();
            }
            if (PSTDBufferFlag)
            {
                throw new NotImplementedException();
            }
            if (ExtensionFlag2)
            {
                BitWriter.WriteBooleanMSB(buffer, offset * 8, true); // marker bit
                BitWriter.WriteBitsMSB(buffer, offset * 8 + 1, (byte)ExtensionReserved.Length, 7);
                offset++;
                ByteWriter.WriteBytes(buffer, ref offset, ExtensionReserved);
            }
        }

        public int Length
        {
            get
            {
                int result = 1;
                if (PrivateDataFlag)
                {
                    result += 16;
                }
                if (PackHeaderFieldFlag)
                {
                    throw new NotImplementedException();
                }

                if (ProgramPacketSequenceCounterFlag)
                {
                    result += 2;
                }

                if (PSTDBufferFlag)
                {
                    result += 2;
                }

                if (ExtensionFlag2)
                {
                    result += 1 + ExtensionReserved.Length;
                }
                return result;
            }
        }
    }
}
