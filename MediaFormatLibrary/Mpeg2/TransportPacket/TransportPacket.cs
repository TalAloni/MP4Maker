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
    public class TransportPacket
    {
        public const int PacketLength = 188; // excluding extra header

        private bool m_isBluRayTransportStream;
        public TransportPacketExtraHeader ExtraHeader; // TP_extra_header, BDAV M2TS
        public TransportPacketHeader Header; // 4 bytes
        public AdaptationField AdaptationField;
        public byte[] Payload; // 184 bytes maximum

        public int PacketIndex; // for analyzing purposes

        public TransportPacket(bool isBluRayTransportStream)
        {
            m_isBluRayTransportStream = isBluRayTransportStream;
            if (isBluRayTransportStream)
            {
                ExtraHeader = new TransportPacketExtraHeader();
            }
            Header = new TransportPacketHeader();
            AdaptationField = new AdaptationField();
        }

        public TransportPacket(byte[] buffer, bool isBluRayTransportStream)
        {
            m_isBluRayTransportStream = isBluRayTransportStream;
            int offset = 0;
            int packetLength = m_isBluRayTransportStream ? PacketLength + TransportPacketExtraHeader.Length : PacketLength;
            if (isBluRayTransportStream)
            {
                ExtraHeader = new TransportPacketExtraHeader(buffer, ref offset);
            }
            Header = new TransportPacketHeader(buffer, ref offset);
            if (Header.AdaptationFieldExist)
            {
                AdaptationField = new AdaptationField(buffer, ref offset);
            }
            if (Header.PayloadExist)
            {
                int payloadLength = packetLength - offset;
                Payload = ByteReader.ReadBytes(buffer, offset, payloadLength);
            }
            else
            {
                Payload = new byte[0];
            }
        }

        public byte[] GetBytes()
        {
            byte[] buffer = new byte[this.Length];
            int offset = 0;
            if (m_isBluRayTransportStream)
            {
                ExtraHeader.WriteBytes(buffer, ref offset);
            }
            Header.WriteBytes(buffer, ref offset);
            if (Header.AdaptationFieldExist)
            {
                AdaptationField.WriteBytes(buffer, ref offset);
            }
            if (Header.PayloadExist)
            {
                ByteWriter.WriteBytes(buffer, ref offset, Payload);
            }

            for (int index = offset; index < buffer.Length; index++)
            {
                buffer[index] = 0xFF;
            }

            return buffer;
        }

        public int Length
        {
            get
            {
                return m_isBluRayTransportStream ? PacketLength + TransportPacketExtraHeader.Length : PacketLength;
            }
        }

        public static TransportPacket ReadPacket(Stream stream, bool isBluRayTransportStream)
        {
            int packetLength = isBluRayTransportStream ? PacketLength + TransportPacketExtraHeader.Length : PacketLength;
            byte[] buffer = new byte[packetLength];
            int bytesRead = stream.Read(buffer, 0, packetLength);
            if (bytesRead < packetLength)
            {
                return null;
            }

            int offset = isBluRayTransportStream ? TransportPacketExtraHeader.Length : 0;
            if (HasSyncByte(buffer, offset))
            {
                return new TransportPacket(buffer, isBluRayTransportStream);
            }
            else
            {
                return null;
            }
        }

        public static bool HasSyncByte(byte[] buffer, int offset)
        {
            return TransportPacketHeader.HasSyncByte(buffer, offset);
        }

        public static byte[] GetStuffingBytes(int length)
        {
            byte[] stuffing = new byte[length];
            for (int index = 0; index < length; index++)
            {
                stuffing[index] = 0xFF;
            }
            return stuffing;
        }
    }
}
