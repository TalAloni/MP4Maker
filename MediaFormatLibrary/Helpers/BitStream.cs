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

namespace MediaFormatLibrary
{
    public class BitStream
    {
        private Stream m_stream;
        private int m_bitOffset;
        private bool m_isMsbFirst = false;

        public BitStream(Stream stream, bool isMsbFirst) : this(stream, isMsbFirst, 0)
        {
        }

        public BitStream(Stream stream, bool isMsbFirst, int bitOffset)
        {
            m_stream = stream;
            m_isMsbFirst = isMsbFirst;
            m_bitOffset = bitOffset;
        }

        public BitStream(byte[] buffer, bool isMsbFirst)
        {
            m_stream = new MemoryStream(buffer);
            m_isMsbFirst = isMsbFirst;
            m_bitOffset = 0;
        }

        public BitStream(bool isMsbFirst)
        {
            m_stream = new MemoryStream();
            m_isMsbFirst = isMsbFirst;
            m_bitOffset = 0;
        }

        public ulong ReadBits(int bitCount)
        {
            int byteCount = (int)Math.Ceiling((double)(m_bitOffset + bitCount) / 8);
            byte[] buffer = new byte[byteCount];
            int bytesRead = m_stream.Read(buffer, 0, byteCount);
            ulong result;
            if (m_isMsbFirst)
            {
                result = BitReader.ReadBitsMSB(buffer, m_bitOffset, bitCount);
            }
            else
            {
                result = BitReader.ReadBitsLSB(buffer, m_bitOffset, bitCount);
            }
            m_bitOffset = (m_bitOffset + bitCount) % 8;
            if (m_bitOffset > 0)
            {
                // If we reached the end of the stream, we should not return back
                if (bytesRead == byteCount)
                {
                    // Still bits to read from current byte
                    m_stream.Position--;
                }
            }

            return result;
        }

        public bool ReadBoolean()
        {
            return ((ReadBits(1) & 0x01) > 0);
        }

        public byte ReadByte()
        {
            return (byte)ReadBits(8);
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadBits(16);
        }

        public uint ReadUInt32()
        {
            return (uint)ReadBits(32);
        }

        /// <summary>
        /// Will write bits unobtrusively, any bits preceding / following will be untouched.
        /// </summary>
        public void WriteBits(ulong value, int bitCount)
        {
            // Calculate the number of bytes affected by this write
            int byteCount = (int)Math.Ceiling((double)(m_bitOffset + bitCount) / 8);
            byte[] buffer = new byte[byteCount];
            int bytesRead = m_stream.Read(buffer, 0, byteCount);
            m_stream.Position -= bytesRead;
            if (m_isMsbFirst)
            {
                BitWriter.WriteBitsMSB(buffer, m_bitOffset, value, bitCount);
            }
            else
            {
                BitWriter.WriteBitsLSB(buffer, m_bitOffset, value, bitCount);
            }
            m_stream.Write(buffer, 0, buffer.Length);
            m_bitOffset = (m_bitOffset + bitCount) % 8;
            if (m_bitOffset > 0)
            {
                // Still bits to write ar current byte
                m_stream.Seek(-1, SeekOrigin.Current);
            }
        }

        public void WriteBoolean(bool value)
        {
            WriteBits(Convert.ToByte(value), 1);
        }

        public void WriteByte(byte value)
        {
            WriteBits(value, 8);
        }

        public void WriteUInt16(ushort value)
        {
            WriteBits(value, 16);
        }

        public void WriteUInt32(uint value)
        {
            WriteBits(value, 32);
        }

        public void WriteStream(BitStream stream)
        {
            long bytesToCopy = (stream.LengthInBits - stream.PositionInBits) / 8;
            long bitsToCopy = (stream.LengthInBits - stream.PositionInBits) % 8;
            for (long index = 0; index < bytesToCopy; index++)
            {
                WriteByte(stream.ReadByte());
            }

            for (long index = 0; index < bitsToCopy; index++)
            {
                WriteBoolean(stream.ReadBoolean());
            }
        }

        public void Truncate()
        {
            if (m_bitOffset != 0)
            {
                WriteBits(0x00, 8 - m_bitOffset);
            }
            m_stream.SetLength(m_stream.Position + 1);
        }

        public Stream BaseStream
        {
            get
            {
                return m_stream;
            }
        }

        public int BitOffset
        {
            get
            {
                return m_bitOffset;
            }
            set
            {
                m_bitOffset = value;
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
                m_bitOffset = 0;
                m_stream.Position = value;
            }
        }

        public long PositionInBits
        {
            get
            {
                return m_stream.Position * 8 + m_bitOffset;
            }
            set
            {
                m_stream.Position = value / 8;
                m_bitOffset = (int)(value % 8);
            }
        }

        public long Length
        {
            get
            {
                return m_stream.Length;
            }
        }

        public long LengthInBits
        {
            get
            {
                return m_stream.Length * 8;
            }
        }
    }
}
