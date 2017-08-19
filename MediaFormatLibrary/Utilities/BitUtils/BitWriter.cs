using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class BitWriter
    {
        public static void WriteBitsLSB(byte[] bytes, int bitOffset, ulong value, int bitCount)
        {
            for (int index = bitOffset; index < bitOffset + bitCount; index++)
            {
                int position = index - bitOffset;
                bool bitValue = ((value >> position) & 1) > 0;
                int byteOffset = index / 8;
                int bitIndex = index % 8;
                if (bitValue)
                {
                    bytes[byteOffset] |= (byte)(1 << bitIndex);
                }
                else
                {
                    bytes[byteOffset] &= (byte)~(1 << bitIndex);
                }
            }
        }

        public static void WriteBitsLSB(byte[] bytes, ref int bitOffset, ulong value, int bitCount)
        {
            WriteBitsLSB(bytes, bitOffset, value, bitCount);
            bitOffset += bitCount;
        }

        public static void WriteBooleanLSB(byte[] bytes, int bitOffset, bool value)
        {
            WriteBitsLSB(bytes, bitOffset, Convert.ToByte(value), 1);
        }

        public static void WriteBooleanLSB(byte[] bytes, ref int bitOffset, bool value)
        {
            WriteBitsLSB(bytes, ref bitOffset, Convert.ToByte(value), 1);
        }

        public static void WriteBitsMSB(byte[] bytes, int bitOffset, ulong value, int bitCount)
        {
            for (int index = bitOffset; index < bitOffset + bitCount; index++)
            {
                int position = (bitCount - 1) - (index - bitOffset);
                bool bitValue = ((value >> position) & 1) > 0;
                int byteOffset = index / 8;
                int bitIndex = 7 - (index % 8);
                if (bitValue)
                {
                    bytes[byteOffset] |= (byte)(1 << bitIndex);
                }
                else
                {
                    bytes[byteOffset] &= (byte)~(1 << bitIndex);
                }
            }
        }

        public static void WriteBitsMSB(byte[] bytes, ref int bitOffset, ulong value, int bitCount)
        {
            WriteBitsMSB(bytes, bitOffset, value, bitCount);
            bitOffset += bitCount;
        }

        public static void WriteBooleanMSB(byte[] bytes, int bitOffset, bool value)
        {
            WriteBitsMSB(bytes, bitOffset, Convert.ToByte(value), 1);
        }

        public static void WriteBooleanMSB(byte[] bytes, ref int bitOffset, bool value)
        {
            WriteBitsMSB(bytes, ref bitOffset, Convert.ToByte(value), 1);
        }
    }
}
