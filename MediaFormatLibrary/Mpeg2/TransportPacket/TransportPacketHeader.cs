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
    public class TransportPacketHeader
    {
        public const int Length = 4;
        public const byte SyncByte = 0x47;

        //public byte SyncByte;
        public bool TransportErrorIndicator;
        public bool PayloadUnitStartIndicator;
        public bool TransportPriority;
        public ushort PID; // 13 bits
        public byte TransportScramblingControl; // 2 bits
        public byte AdaptationFieldControl; // 2 bits
        public byte ContinuityCounter; // 4 bits

        public TransportPacketHeader()
        {
        }

        public TransportPacketHeader(byte[] buffer, ref int offset) : this(buffer, offset)
        {
            offset += 4;
        }

        public TransportPacketHeader(byte[] buffer, int offset)
        {
            byte syncByte = ByteReader.ReadByte(buffer, ref offset);
            int bitOffset = offset * 8;
            TransportErrorIndicator = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            PayloadUnitStartIndicator = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            TransportPriority = BitReader.ReadBooleanMSB(buffer, ref bitOffset);
            PID = (ushort)BitReader.ReadBitsMSB(buffer, ref bitOffset, 13);
            TransportScramblingControl = (byte)BitReader.ReadBitsMSB(buffer, ref bitOffset, 2);
            AdaptationFieldControl = (byte)BitReader.ReadBitsMSB(buffer, ref bitOffset, 2);
            ContinuityCounter = (byte)BitReader.ReadBitsMSB(buffer, ref bitOffset, 4);
            offset = bitOffset / 8;
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            ByteWriter.WriteByte(buffer, ref offset, SyncByte);
            int bitOffset = offset * 8;
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, TransportErrorIndicator);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, PayloadUnitStartIndicator);
            BitWriter.WriteBooleanMSB(buffer, ref bitOffset, TransportPriority);
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, PID, 13);
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, TransportScramblingControl, 2);
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, AdaptationFieldControl, 2);
            BitWriter.WriteBitsMSB(buffer, ref bitOffset, ContinuityCounter, 4);
            offset = bitOffset / 8;
        }

        public static TransportPacketHeader ReadHeader(Mpeg2TransportStream stream, bool peek)
        {
            int bytesToRead = stream.IsBluRayTransportStream ? 8 : 4;
            byte[] buffer = new byte[bytesToRead];
            int bytesRead = stream.BaseStream.Read(buffer, 0, bytesToRead);
            if (peek)
            {
                stream.BaseStream.Seek(-bytesRead, SeekOrigin.Current);
            }
            if (bytesRead == bytesToRead)
            {
                int offset = stream.IsBluRayTransportStream ? 4 : 0;
                if (HasSyncByte(buffer, offset))
                {
                    return new TransportPacketHeader(buffer, offset);
                }
                else
                {
                    return null;
                }
            }
            return null;
        }

        public bool AdaptationFieldExist
        {
            get
            {
                return (AdaptationFieldControl & 0x02) > 0;
            }
            set
            {
                AdaptationFieldControl |= 0x02;
            }
        }

        public bool PayloadExist
        {
            get
            {
                return (AdaptationFieldControl & 0x01) > 0;
            }
            set
            {
                AdaptationFieldControl |= 0x01;
            }
        }

        public static bool HasSyncByte(byte[] buffer, int offset)
        {
            byte syncByte = buffer[offset + 0];
            return (syncByte == TransportPacketHeader.SyncByte);
        }
    }
}
