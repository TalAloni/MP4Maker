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
    public class AdaptationField
    {
        public byte FieldLength; // adaptation_field_length
        public bool DiscontinuityIndicator; // discontinuity_indicator
        public bool RandomAccessIndicator; // random_access_indicator
        public bool ElementaryStreamPriorityIndicator; // elementary_stream_priority_indicator
        public bool PCRFlag; // PCR_flag
        public bool OPCRFlag; // OPCR_flag
        public bool SplicingPointFlag; // splicing_point_flag
        public bool TransportPrivateDataFlag; // transport_private_data_flag
        public bool AdaptationFieldExtensionFlag; // adaptation_field_extension_flag

        /// <summary>
        /// [ISO/IEC 13818-1] The PCR indicates the intended time of arrival of the byte containing the
        /// last bit of the program_clock_reference_base at the input of the system target decoder.
        /// </summary>
        public ulong ProgramClockReferenceBase; // 33 bits, program_clock_reference_base
        public byte ProgramClockReferenceReserved; // 6 bits, reserved
        public ushort ProgramClockReferenceExtension; // 9 bits, program_clock_reference_extension

        public AdaptationField()
        {
            ProgramClockReferenceReserved = 0x3F;
        }

        public AdaptationField(byte[] buffer, ref int offset)
        {
            FieldLength = ByteReader.ReadByte(buffer, ref offset);
            if (FieldLength > 0)
            {
                int startOffset = offset;
                int bitOffset = offset * 8;
                DiscontinuityIndicator = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                RandomAccessIndicator = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                ElementaryStreamPriorityIndicator = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                PCRFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                OPCRFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                SplicingPointFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                TransportPrivateDataFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                AdaptationFieldExtensionFlag = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
                if (PCRFlag)
                {
                    ProgramClockReferenceBase = BitReader.ReadBitsMSB(buffer, ref bitOffset, 33);
                    ProgramClockReferenceReserved = (byte)BitReader.ReadBitsMSB(buffer, ref bitOffset, 6);
                    ProgramClockReferenceExtension = (ushort)BitReader.ReadBitsMSB(buffer, ref bitOffset, 9);
                }
                if (OPCRFlag)
                {
                    throw new NotImplementedException();
                }
                if (SplicingPointFlag)
                {
                    throw new NotImplementedException();
                }
                if (TransportPrivateDataFlag)
                {
                    throw new NotImplementedException();
                }
                if (AdaptationFieldExtensionFlag)
                {
                    throw new NotImplementedException();
                }
                offset = bitOffset / 8;
                int stuffingLength = FieldLength - (offset - startOffset);
                offset += stuffingLength;
            }
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            ByteWriter.WriteByte(buffer, ref offset, FieldLength);
            if (FieldLength > 0)
            {
                int startOffset = offset;
                int bitOffset = offset * 8;
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, DiscontinuityIndicator);
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, RandomAccessIndicator);
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, ElementaryStreamPriorityIndicator);
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, PCRFlag);
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, OPCRFlag);
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, SplicingPointFlag);
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, TransportPrivateDataFlag);
                BitWriter.WriteBooleanMSB(buffer, ref bitOffset, AdaptationFieldExtensionFlag);
                if (PCRFlag)
                {
                    BitWriter.WriteBitsMSB(buffer, ref bitOffset, ProgramClockReferenceBase, 33);
                    BitWriter.WriteBitsMSB(buffer, ref bitOffset, ProgramClockReferenceReserved, 6);
                    BitWriter.WriteBitsMSB(buffer, ref bitOffset, ProgramClockReferenceExtension, 9);
                }
                if (OPCRFlag)
                {
                    throw new NotImplementedException();
                }
                if (SplicingPointFlag)
                {
                    throw new NotImplementedException();
                }
                if (TransportPrivateDataFlag)
                {
                    throw new NotImplementedException();
                }
                if (AdaptationFieldExtensionFlag)
                {
                    throw new NotImplementedException();
                }
                offset = bitOffset / 8;
                int stuffingLength = FieldLength - (offset - startOffset);
                byte[] stuffing = TransportPacket.GetStuffingBytes(stuffingLength);
                ByteWriter.WriteBytes(buffer, ref offset, stuffing);
            }
        }
    }
}
