using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    /// <summary>
    ///  See ETSI TS 102 366 V1.2.1 Annex F (for how to put AC3 in MP4)
    /// </summary>
    public class AC3SpecificBox : Box
    {
        public byte FSCod; // 2 bits
        public byte BSID; // 5 bits
        public byte BSMod; // 3 bits
        public byte ACMod; // 3 bits
        public bool LfeOn; // 1 bits
        public byte BitRateCode; //  5 bits, bit_rate_code
        public byte Reserved; // 5 bits

        public AC3SpecificBox() : base(BoxType.AC3SpecificBox)
        {
        }

        public AC3SpecificBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            BitStream bitStream = new BitStream(stream, true);

            FSCod = (byte)bitStream.ReadBits(2);
            BSID = (byte)bitStream.ReadBits(5);
            BSMod = (byte)bitStream.ReadBits(3);
            ACMod = (byte)bitStream.ReadBits(3);
            LfeOn = bitStream.ReadBoolean();
            BitRateCode = (byte)bitStream.ReadBits(5);
            Reserved = (byte)bitStream.ReadBits(5);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BitStream bitStream = new BitStream(stream, true);
            bitStream.WriteBits(FSCod, 2);
            bitStream.WriteBits(BSID, 5);
            bitStream.WriteBits(BSMod, 3);
            bitStream.WriteBits(ACMod, 3);
            bitStream.WriteBoolean(LfeOn);
            bitStream.WriteBits(BitRateCode, 5);
            bitStream.WriteBits(Reserved, 5);
        }

        public int BitRate
        {
            get
            {
                return GetBitrateFromCode(this.BitRateCode);
            }
        }

        public static int GetBitrateFromCode(byte bitRateCode)
        {
            switch (bitRateCode)
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
                    throw new Exception("Invalid AC3 bit rate code");
            }
        }
    }
}
