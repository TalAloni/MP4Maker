using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utilities
{
    public class ByteReader
    {
        public static byte ReadByte(byte[] buffer, int offset)
        {
            return buffer[offset];
        }

        public static byte ReadByte(byte[] buffer, ref int offset)
        {
            offset++;
            return buffer[offset - 1];
        }

        public static byte[] ReadBytes(byte[] buffer, int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(buffer, offset, result, 0, length);
            return result;
        }

        public static byte[] ReadBytes(byte[] buffer, ref int offset, int length)
        {
            offset += length;
            return ReadBytes(buffer, offset - length, length);
        }

        /// <summary>
        /// Will return the ANSI string stored in the buffer
        /// </summary>
        public static string ReadAnsiString(byte[] buffer, int offset, int count)
        {
            // ASCIIEncoding.ASCII.GetString will convert some values to '?' (byte value of 63)
            // Any codepage will do, but the only one that Mono supports is 28591.
            return ASCIIEncoding.GetEncoding(28591).GetString(buffer, offset, count);
        }

        public static string ReadAnsiString(byte[] buffer, ref int offset, int count)
        {
            offset += count;
            return ReadAnsiString(buffer, offset - count, count);
        }

        public static string ReadUTF16String(byte[] buffer, int offset, int numberOfCharacters)
        {
            int numberOfBytes = numberOfCharacters * 2;
            return Encoding.Unicode.GetString(buffer, offset, numberOfBytes);
        }

        public static string ReadUTF16String(byte[] buffer, ref int offset, int numberOfCharacters)
        {
            int numberOfBytes = numberOfCharacters * 2;
            offset += numberOfBytes;
            return ReadUTF16String(buffer, offset - numberOfBytes, numberOfCharacters);
        }

        public static string ReadNullTerminatedAnsiString(byte[] buffer, int offset)
        {
            StringBuilder builder = new StringBuilder();
            char c = (char)ByteReader.ReadByte(buffer, offset);
            while (c != '\0')
            {
                builder.Append(c);
                offset++;
                c = (char)ByteReader.ReadByte(buffer, offset);
            }
            return builder.ToString();
        }

        public static string ReadNullTerminatedUTF16String(byte[] buffer, int offset)
        {
            StringBuilder builder = new StringBuilder();
            char c = (char)LittleEndianConverter.ToUInt16(buffer, offset);
            while (c != 0)
            {
                builder.Append(c);
                offset += 2;
                c = (char)LittleEndianConverter.ToUInt16(buffer, offset);
            }
            return builder.ToString();
        }

        public static string ReadNullTerminatedAnsiString(byte[] buffer, ref int offset)
        {
            string result = ReadNullTerminatedAnsiString(buffer, offset);
            offset += result.Length + 1;
            return result;
        }

        public static string ReadNullTerminatedUTF16String(byte[] buffer, ref int offset)
        {
            string result = ReadNullTerminatedUTF16String(buffer, offset);
            offset += result.Length * 2 + 2;
            return result;
        }

        public static byte[] ReadBytes(Stream stream, int count)
        {
            MemoryStream temp = new MemoryStream();
            ByteUtils.CopyStream(stream, temp, count);
            return temp.ToArray();
        }

        /// <summary>
        /// Return all bytes from current stream position to the end of the stream
        /// </summary>
        public static byte[] ReadAllBytes(Stream stream)
        {
            MemoryStream temp = new MemoryStream();
            ByteUtils.CopyStream(stream, temp);
            return temp.ToArray();
        }

        public static string ReadAnsiString(Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            return ASCIIEncoding.GetEncoding(28591).GetString(buffer, 0, count);
        }

        public static string ReadUTF16String(Stream stream, int numberOfCharacters)
        {
            int numberOfBytes = numberOfCharacters * 2;
            byte[] buffer = new byte[numberOfBytes];
            stream.Read(buffer, 0, numberOfBytes);
            return Encoding.Unicode.GetString(buffer, 0, numberOfBytes);
        }

        public static string ReadNullTerminatedUTF8String(Stream stream)
        {
            // UTF8: Code points larger than 127 are represented by multi-byte sequences, composed of a leading byte and one or more continuation bytes.
            // The leading byte has two or more high-order 1s followed by a 0, while continuation bytes all have '10' in the high-order position
            StringBuilder builder = new StringBuilder();
            List<byte> bytes = new List<byte>();
            byte b = (byte)stream.ReadByte();
            while (b != 0)
            {
                bytes.Add(b);
                b = (byte)stream.ReadByte();
            }
            return UnicodeEncoding.UTF8.GetString(bytes.ToArray());
        }

        public static string ReadNullTerminatedUTF16BEString(Stream stream)
        {
            StringBuilder builder = new StringBuilder();
            char c = (char)BigEndianReader.ReadUInt16(stream);
            while (c != 0)
            {
                builder.Append(c);
                c = (char)BigEndianReader.ReadUInt16(stream);
            }
            return builder.ToString();
        }
    }
}
