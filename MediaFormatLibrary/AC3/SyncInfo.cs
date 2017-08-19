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

namespace MediaFormatLibrary.AC3
{
    public class SyncInfo
    {
        public const ushort SyncWord = 0x0B77;
        public const int Length = 5;

        // syncword - 16 bits - 0x0B77
        public ushort CRC1;
        public byte FSCod; // 2 bits
        public byte FrmSizeCod; // 6 bits

        public SyncInfo()
        {
        }

        public SyncInfo(BitStream stream)
        {
            ushort syncWord = (ushort)stream.ReadBits(16);
            if (syncWord != SyncWord)
            {
                throw new Exception("Invalid AC3 syncword");
            }
            CRC1 = (ushort)stream.ReadBits(16);
            FSCod = (byte)stream.ReadBits(2);
            FrmSizeCod = (byte)stream.ReadBits(6);
        }

        public void WriteBytes(BitStream stream)
        {
            stream.WriteBits(SyncWord, 16);
            stream.WriteBits(CRC1, 16);
            stream.WriteBits(FSCod, 2);
            stream.WriteBits(FrmSizeCod, 6);
        }

        public int SampleRate
        {
            get
            {
                return GetSampleRate(FSCod);
            }
            set
            {
                FSCod = GetSampleRateCode(value);
            }
        }

        public int BitRate
        {
            get
            {
                return GetBitRate(FrmSizeCod);
            }
        }

        public int FrameSize
        {
            get
            {
                return GetFrameSize(FSCod, FrmSizeCod);
            }
        }

        public static int GetSampleRate(byte sampleRateCode)
        {
            switch (sampleRateCode)
            {
                case 0:
                    return 48000;
                case 1:
                    return 44100;
                case 2:
                    return 32000;
                case 3:
                    throw new NotImplementedException("Unknown sample rate code");
                default:
                    throw new ArgumentException("Invalid sample rate code");
            }
        }

        public static byte GetSampleRateCode(int sampleRate)
        {
            switch (sampleRate)
            {
                case 48000:
                    return 0;
                case 44100:
                    return 1;
                case 32000:
                    return 2;
                default:
                    throw new ArgumentException("Invalid sample rate");
            }
        }

        /// <returns>Bitrate in kbit/s</returns>
        public static int GetBitRate(byte frameSizeCode)
        {
            switch (frameSizeCode / 2)
            {
                case 0:
                    return 32;
                case 1:
                    return 40;
                case 2:
                    return 48;
                case 3:
                    return 56;
                case 4:
                    return 64;
                case 5:
                    return 80;
                case 6:
                    return 96;
                case 7:
                    return 112;
                case 8:
                    return 128;
                case 9:
                    return 160;
                case 10:
                    return 192;
                case 11:
                    return 224;
                case 12:
                    return 256;
                case 13:
                    return 320;
                case 14:
                    return 384;
                case 15:
                    return 448;
                case 16:
                    return 512;
                case 17:
                    return 576;
                case 18:
                    return 640;
                default:
                    throw new ArgumentException("Invalid frame size code");
            }
        }

        public static int GetFrameSize(byte sampleRateCode, byte frameSizeCode)
        {
            // in 2-byte words:
            int[,] frameSizeCodeTable = new int[,]
            {
                {96, 69, 64},
                {96, 70, 64},
                {120, 87, 80},
                {120, 88, 80},
                {144, 104, 96},
                {144, 105, 96},
                {168, 121, 112},
                {168, 122, 112},
                {192, 139, 128},
                {192, 140, 128},
                {240, 174, 160},
                {240, 175, 160},
                {288, 208, 192},
                {288, 209, 192},
                {336, 243, 224},
                {336, 244, 224},
                {384, 278, 256},
                {384, 279, 256},
                {480, 348, 320},
                {480, 349, 320},
                {576, 417, 384},
                {576, 418, 384},
                {672, 487, 448},
                {672, 488, 448},
                {768, 557, 512},
                {768, 558, 512},
                {960, 696, 640},
                {960, 697, 640},
                {1152, 835, 768},
                {1152, 836, 768},
                {1344, 975, 896},
                {1344, 976, 896},
                {1536, 1114, 1024},
                {1536, 1115, 1024},
                {1728, 1253, 1152},
                {1728, 1254, 1152},
                {1920, 1393, 1280},
                {1920, 1394, 1280},
            };
            if (sampleRateCode == 3)
            {
                throw new NotImplementedException("Unknown sample rate code");
            }
            else if (sampleRateCode > 3)
            {
                throw new ArgumentException("Invalid sample rate code");
            }

            if (frameSizeCode > 37)
            {
                throw new ArgumentException("Invalid frame size code");
            }
            int sampleRateIndex = 2 - sampleRateCode;
            return frameSizeCodeTable[frameSizeCode, sampleRateIndex] * 2;
        }
    }
}
