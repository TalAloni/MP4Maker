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
    /// <summary>
    /// Packetized Elementary Stream Header
    /// </summary>
    public class PesOptionalHeader
    {
        // byte MarkerBits; // 2 bits, '10'
        public byte ScramblingControl; // 2 bits
        public bool Priority; // PES_priority
        public bool DataAlignmentIndicator; // data_alignment_indicator, Indicates that the PES packet header is immediately followed by the video start code or audio syncword.
        public bool Copyright;
        public bool OriginalOrCopy; // original_or_copy
        public byte PtsDtsFlag; // 2 bits, PTS_DTS_flags
        public bool ESCRFlag; // ESCR_flag
        public bool ESRateFlag; // ES_rate_flag
        public bool DsmTrickModeFlag; // DSM_trick_mode_flag
        public bool AdditionalCopyInfoFlag; // additional_copy_info_flag
        public bool CrcFlag; // PES_CRC_flag
        public bool ExtensionFlag; // PES_extension_flag
        // byte HeaderDataLength; // PES_header_data_length, optional field bytes + stuffing bytes
        
        public int StuffingLength;

        // Optional fields:
        public ulong PTS;
        public ulong DTS;
        public ulong EScrBase; // ESCR_base
        public ushort EScrExtension; // ESCR_extension
        public uint ESRate; // 22 bits, ES_rate
        public TrickModeField TrickModeField;
        public byte AdditionaCopyInfo; // 7 bits, additional_copy_info
        public ushort PreviousPESPacketCRC; // previous_PES_packet_CRC
        public ExtensionField ExtensionField;

        public PesOptionalHeader()
        {
            TrickModeField = new TrickModeField();
            ExtensionField = new ExtensionField();
        }

        public PesOptionalHeader(byte[] buffer, ref int offset)
        {
            int bitOffset = offset * 8;
            byte markerBits = (byte)BitReader.ReadBitsMSB(buffer, ref bitOffset, 2);
            if (markerBits != 0x02)
            {
                throw new InvalidDataException("Invalid marker bits");
            }
            ScramblingControl = (byte)BitReader.ReadBitsMSB(buffer, ref bitOffset, 2);
            Priority = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            DataAlignmentIndicator = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            Copyright = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            OriginalOrCopy = BitReader.ReadBooleanMSB(buffer, ref bitOffset);

            PtsDtsFlag = (byte)BitReader.ReadBitsMSB(buffer, ref bitOffset, 2);
            ESCRFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            ESRateFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            DsmTrickModeFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            AdditionalCopyInfoFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            CrcFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            ExtensionFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            
            offset = bitOffset / 8;
            byte headerDataLength = ByteReader.ReadByte(buffer, ref offset);
            int optionalFieldsStartOffset = offset;

            if (PtsDtsFlag == 0x02)
            {
                PTS = ReadPTSorDTS(buffer, ref offset, 0x02);
            }
            else if (PtsDtsFlag == 0x03)
            {
                PTS = ReadPTSorDTS(buffer, ref offset, 0x03);
                DTS = ReadPTSorDTS(buffer, ref offset, 0x01);
            }

            if (ESCRFlag)
            {
                ReadESCR(buffer, ref offset, out EScrBase, out EScrExtension);
            }

            if (ESRateFlag)
            {
                bool markerBit1 = BitReader.ReadBooleanMSB(buffer, offset * 8);
                bool markerBit2 = BitReader.ReadBooleanMSB(buffer, offset * 8 + 23);
                if (!markerBit1 || !markerBit2)
                {
                    throw new InvalidDataException("Invalid marker bit");
                }

                ESRate = (uint)BitReader.ReadBitsMSB(buffer, offset * 8 + 1, 22);
                offset += 3;
            }

            if (DsmTrickModeFlag)
            {
                TrickModeField = new TrickModeField(buffer, ref offset);
            }

            if (AdditionalCopyInfoFlag)
            {
                bool markerBit = BitReader.ReadBooleanMSB(buffer, offset * 8);
                if (!markerBit)
                {
                    throw new InvalidDataException("Invalid marker bit");
                }
                AdditionaCopyInfo = (byte)BitReader.ReadBitsMSB(buffer, offset * 8 + 1, 7);
                offset++;
            }

            if (CrcFlag)
            {
                PreviousPESPacketCRC = BigEndianReader.ReadUInt16(buffer, ref offset);
            }

            if (ExtensionFlag)
            {
                ExtensionField = new ExtensionField(buffer, ref offset);
            }

            StuffingLength = headerDataLength - (offset - optionalFieldsStartOffset);
            if (StuffingLength < 0)
            {
                throw new InvalidDataException();
            }
            offset += StuffingLength;
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            int bitOffset = offset * 8;
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, 0x02, 2); // marker bits
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, ScramblingControl, 2);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, Priority);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, DataAlignmentIndicator);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, Copyright);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, OriginalOrCopy);
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, PtsDtsFlag, 2);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, ESCRFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, ESRateFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, DsmTrickModeFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, AdditionalCopyInfoFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, CrcFlag);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, ExtensionFlag);
            offset = bitOffset / 8;
            ByteWriter.WriteByte(buffer, ref offset, (byte)(this.Length - 3));

            if (PtsDtsFlag == 0x02)
            {
                WritePTSorDTS(buffer, ref offset, PTS, 0x02);
            }
            else if (PtsDtsFlag == 0x03)
            {
                WritePTSorDTS(buffer, ref offset, PTS, 0x03);
                WritePTSorDTS(buffer, ref offset, DTS, 0x01);
            }

            if (ESCRFlag)
            {
                WriteESCR(buffer, ref offset, EScrBase, EScrExtension);
            }

            if (ESRateFlag)
            {
                BitWriter.WriteBooleanMSB(buffer, offset * 8, true);
                BitWriter.WriteBitsMSB(buffer, offset * 8 + 1, ESRate, 22);
                BitWriter.WriteBooleanMSB(buffer, offset * 8 + 23, true);
                offset += 3;
            }

            if (DsmTrickModeFlag)
            {
                TrickModeField.WriteBytes(buffer, ref offset);
            }

            if (AdditionalCopyInfoFlag)
            {
                BitWriter.WriteBooleanMSB(buffer, offset * 8, true);
                BitWriter.WriteBitsMSB(buffer, offset * 8 + 1, AdditionaCopyInfo, 7);
                offset++;
            }

            if (CrcFlag)
            {
                BigEndianWriter.WriteUInt16(buffer, ref offset, PreviousPESPacketCRC);
            }

            if (ExtensionFlag)
            {
                ExtensionField.WriteBytes(buffer, ref offset);
            }

            byte[] stuffingBytes = TransportPacket.GetStuffingBytes(StuffingLength);
            ByteWriter.WriteBytes(buffer, ref offset, stuffingBytes);
        }

        public int Length
        {
            get
            {
                int result = 3;
                if (PtsDtsFlag == 0x02)
                {
                    result += 5;
                }
                else if (PtsDtsFlag == 0x03)
                {
                    result += 10;
                }

                if (ESCRFlag)
                {
                    result += 6;
                }

                if (ESRateFlag)
                {
                    result += 3;
                }

                if (DsmTrickModeFlag)
                {
                    throw new NotImplementedException();
                }

                if (AdditionalCopyInfoFlag)
                {
                    result++;
                }

                if (CrcFlag)
                {
                    result += 2;
                }

                if (ExtensionFlag)
                {
                    result += ExtensionField.Length;
                }

                result += StuffingLength;

                return result;
            }
        }

        public bool HasPTS
        {
            get
            {
                return (PtsDtsFlag & 0x02) > 0;
            }
            set
            {
                PtsDtsFlag |= 0x02;
            }
        }

        public static ulong ReadPTSorDTS(byte[] buffer, ref int offset, byte expectedMarkerBits)
        {
            byte part1 = ByteReader.ReadByte(buffer, ref offset);
            ushort part2 = BigEndianReader.ReadUInt16(buffer, ref offset);
            ushort part3 = BigEndianReader.ReadUInt16(buffer, ref offset);
            byte markerBits = (byte)((part1 & 0xF0) >> 4);
            bool markerBit1 = (part1 & 0x01) > 0;
            bool markerBit2 = (part2 & 0x01) > 0;
            bool markerBit3 = (part3 & 0x01) > 0;

            if (markerBits != expectedMarkerBits)
            {
                throw new InvalidDataException("Invalid marker bits");
            }
            if (!markerBit1 || !markerBit2 || !markerBit3)
            {
                throw new InvalidDataException("Invalid marker bit");
            }

            uint low30 = (uint)((((part2 & 0xFFFE) >> 1) << 15) | ((part3 & 0xFFFE) >> 1));
            uint high3 = (uint)((part1 & 0x0E) >> 1);
            ulong result = (ulong)high3 << 30 | low30;
            return result;
        }

        public static void WritePTSorDTS(byte[] buffer, ref int offset, ulong value, byte markerBits)
        {
            byte part1 = (byte)((uint)((markerBits & 0x0F) << 4) | (((value >> 30) & 0x07) << 1) | 0x01);
            ushort part2 = (ushort)((((value >> 15) & 0x7FFF) << 1) | 0x01);
            ushort part3 = (ushort)(((value & 0x7FFF) << 1) | 0x01);
            ByteWriter.WriteByte(buffer, ref offset, part1);
            BigEndianWriter.WriteUInt16(buffer, ref offset, part2);
            BigEndianWriter.WriteUInt16(buffer, ref offset, part3);
        }

        public static void ReadESCR(byte[] buffer, ref int offset, out ulong escrBase, out ushort escrExtension)
        {
            // 6 bytes
            throw new NotImplementedException();
        }

        public static void WriteESCR(byte[] buffer, ref int offset, ulong escrBase, ushort escrExtension)
        {
            throw new NotImplementedException();
        }
    }
}