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
    /// .ts - MPEG2 Transport stream
    /// .m2ts - Blu-ray Disc Audio-Video MPEG2 transport stream
    /// </summary>
    public class Mpeg2TransportStream
    {
        public const int StreamBufferSize = 4194304;

        public const ushort ProgramAssociationTablePID = 0x00;
        public const ushort NullPacketPID = 0x1FFF;

        private Stream m_stream;
        private bool m_isBluRayTransportStream;
        private int m_packetIndex;

        public Mpeg2TransportStream(Stream stream, bool isBluRayTransportStream)
        {
            m_stream = stream;
            m_isBluRayTransportStream = isBluRayTransportStream;
        }

        public TransportPacket ReadPacket()
        {
            TransportPacket result = TransportPacket.ReadPacket(m_stream, m_isBluRayTransportStream);
            if (result != null)
            {
                result.PacketIndex = m_packetIndex;
                m_packetIndex++;
            }
            return result;
        }

        public void WritePacket(TransportPacket packet)
        {
            ByteWriter.WriteBytes(m_stream, packet.GetBytes());
        }

        public Stream BaseStream
        {
            get
            {
                return m_stream;
            }
        }

        public long Position
        {
            get
            {
                return m_stream.Position;
            }
            set
            {
                m_stream.Position = value;
            }
        }

        public long Length
        {
            get
            {
                return m_stream.Length;
            }
        }

        public bool IsBluRayTransportStream
        {
            get
            {
                return m_isBluRayTransportStream;
            }
        }

        public static Mpeg2TransportStream OpenTS(string path, FileMode fileMode, FileAccess access)
        {
            FileStream stream = new FileStream(path, fileMode, access, FileShare.Read, StreamBufferSize);
            return new Mpeg2TransportStream(stream, false);
        }

        public static Mpeg2TransportStream OpenM2TS(string path, FileMode fileMode, FileAccess access)
        {
            FileStream stream = new FileStream(path, fileMode, access, FileShare.Read, StreamBufferSize);
            return new Mpeg2TransportStream(stream, true);
        }
    }
}
