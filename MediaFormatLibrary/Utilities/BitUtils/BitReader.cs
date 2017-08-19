using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public class BitReader
    {
        public static ulong ReadBitsLSB(byte[] bytes, int bitOffset, int bitCount)
        {
            ulong result = 0;
            for (int index = bitOffset; index < bitOffset + bitCount; index++)
            {
                int byteOffset = index / 8;
                int bitIndex = index % 8;
                int position = index - bitOffset;
                result |= ((ulong)((bytes[byteOffset] >> bitIndex) & 0x1) << position);
            }
            return result;
        }

        public static ulong ReadBitsLSB(byte[] bytes, ref int bitOffset, int bitCount)
        {
            ulong result = ReadBitsLSB(bytes, bitOffset, bitCount);
            bitOffset += bitCount;
            return result;
        }

        public static bool ReadBooleanLSB(byte[] bytes, int bitOffset)
        {
            return ReadBitsLSB(bytes, bitOffset, 1) == 1;
        }

        public static bool ReadBooleanLSB(byte[] bytes, ref int bitOffset)
        {
            return ReadBitsLSB(bytes, ref bitOffset, 1) == 1;
        }

        public static ulong ReadBitsMSB(byte[] bytes, int bitOffset, int bitCount)
        {
            ulong result = 0;
            for (int index = bitOffset; index < bitOffset + bitCount; index++)
            {
                int byteOffset = index / 8;
                int bitIndex = 7 - (index % 8);
                int position = (bitCount - 1) - (index - bitOffset);
                result |= ((ulong)((bytes[byteOffset] >> bitIndex) & 0x1) << position);
            }
            return result;
        }

        public static ulong ReadBitsMSB(byte[] bytes, ref int bitOffset, int bitCount)
        {
            ulong result = ReadBitsMSB(bytes, bitOffset, bitCount);
            bitOffset += bitCount;
            return result;
        }

        public static bool ReadBooleanMSB(byte[] bytes, int bitOffset)
        {
            return ReadBitsMSB(bytes, bitOffset, 1) == 1;
        }

        public static bool ReadBooleanMSB(byte[] bytes, ref int bitOffset)
        {
            return ReadBitsMSB(bytes, ref bitOffset, 1) == 1;
        }
    }
}
