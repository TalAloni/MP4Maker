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
    public class PesPacket
    {
        public PesPacketHeader Header;
        public PesOptionalHeader OptionalHeader;
        public byte[] Data;

        public PesPacket()
        {
            Header = new PesPacketHeader();
            OptionalHeader = new PesOptionalHeader();
        }

        public PesPacket(byte[] buffer)
        {
            int offset = 0;
            Header = new PesPacketHeader(buffer, ref offset);
            if (HasOptionalHeader(Header.StreamID))
            {
                OptionalHeader = new PesOptionalHeader(buffer, ref offset);
                int packetTotalLength = (Header.PacketLength > 0) ? (PesPacketHeader.Length + Header.PacketLength) : buffer.Length;
                int dataLength = packetTotalLength - offset;
                Data = ByteReader.ReadBytes(buffer, ref offset, dataLength);
            }
            else
            {
                // Note: if Header.StreamID == ElementaryStreamID.PaddingStream we put the padding in the data buffer
                Data = ByteReader.ReadBytes(buffer, ref offset, Header.PacketLength);
            }
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[this.Length];
            int offset = 0;
            Header.WriteBytes(buffer, ref offset);
            if (HasOptionalHeader(Header.StreamID))
            {
                OptionalHeader.WriteBytes(buffer, ref offset);
                ByteWriter.WriteBytes(buffer, ref offset, Data);
            }
            else
            {
                ByteWriter.WriteBytes(buffer, ref offset, Data);
            }
            return buffer;
        }

        public int Length
        {
            get
            {
                int length = PesPacketHeader.Length + Data.Length;
                if (HasOptionalHeader(Header.StreamID))
                {
                    length += OptionalHeader.Length;
                }
                return length;
            }
        }

        public static bool HasOptionalHeader(ElementaryStreamID streamID)
        {
            if (streamID != ElementaryStreamID.ProgramStreamMap &&
                streamID != ElementaryStreamID.PrivateStream2 &&
                streamID != ElementaryStreamID.ECMStream &&
                streamID != ElementaryStreamID.EMMStream &&
                streamID != ElementaryStreamID.ProgramStreamDirectory &&
                streamID != ElementaryStreamID.DSMCCStream &&
                streamID != ElementaryStreamID.H222TypeE)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static PesPacket ReadPacket(byte[] buffer)
        {
            int offset = 0;
            if (buffer.Length >= PesPacketHeader.Length)
            {
                uint startCodePrefix = BigEndianReader.ReadUInt24(buffer, ref offset);
                if (startCodePrefix == PesPacketHeader.PacketStartCodePrefix)
                {
                    return new PesPacket(buffer);
                }
            }
            return null;
        }

        public static bool IsPesPacket(byte[] buffer)
        {
            int offset = 0;
            uint startCodePrefix = BigEndianReader.ReadUInt24(buffer, ref offset);
            return (startCodePrefix == PesPacketHeader.PacketStartCodePrefix);
        }

        public static ushort ReadPacketLength(byte[] buffer)
        {
            ushort packetLength = BigEndianConverter.ToUInt16(buffer, 4);
            return packetLength;
        }

        /// <summary>
        /// Including the header length
        /// </summary>
        public static int GetPacketTotalLength(byte[] buffer)
        {
            ushort packetLength = BigEndianConverter.ToUInt16(buffer, 4);
            if (packetLength == 0)
            {
                return 0;
            }
            else
            {
                return (PesPacketHeader.Length + packetLength);
            }
        }
    }
}
