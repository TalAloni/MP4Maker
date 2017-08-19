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

namespace MediaFormatLibrary.H264
{
    public class H264ElementaryStream
    {
        public const int StreamBufferSize = 4194304;

        private Stream m_stream;

        public H264ElementaryStream(Stream stream)
        {
            m_stream = stream;
        }

        public H264ElementaryStream(string path, FileMode fileMode, FileAccess access)
        {
            m_stream = new FileStream(path, fileMode, access, FileShare.Read, StreamBufferSize);
        }

        /// <summary>
        /// Returns the NAL Unit bytes: the NAL header followed by the encoded RBSP (raw byte sequence payload) bytes.
        /// </summary>
        public MemoryStream ReadNalUnitStream()
        {
            if (m_stream.Position == 0)
            {
                byte[] buffer = new byte[4];
                m_stream.Read(buffer, 0, 4);
                if (buffer[0] == 0 && buffer[1] == 0 && buffer[2] == 0 && buffer[3] == 1)
                {
                    m_stream.Position = 4;
                }
                else
                {
                    return null;
                }
            }

            long streamLength = m_stream.Length;
            if (m_stream.Position <= streamLength - 4)
            {
                MemoryStream nalStream = new MemoryStream();
                byte[] buffer = new byte[4];
                m_stream.Read(buffer, 0, 4);

                while (!(buffer[1] == 0 && buffer[2] == 0 && buffer[3] == 1))
                {
                    if (m_stream.Position == streamLength)
                    {
                        nalStream.WriteByte(buffer[0]);
                        nalStream.WriteByte(buffer[1]);
                        nalStream.WriteByte(buffer[2]);
                        nalStream.WriteByte(buffer[3]);

                        buffer[0] = 0;
                        break;
                    }

                    nalStream.WriteByte(buffer[0]);
                    buffer[0] = buffer[1];
                    buffer[1] = buffer[2];
                    buffer[2] = buffer[3];
                    buffer[3] = (byte)m_stream.ReadByte();
                }

                if (buffer[0] != 0)
                {
                    nalStream.WriteByte(buffer[0]);
                }

                nalStream.Position = 0;
                return nalStream;
            }
            return null;
        }

        public void Close()
        {
            m_stream.Close();
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
    }
}
