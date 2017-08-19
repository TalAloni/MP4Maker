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
    public class BSI // Bit Stream Information
    {
        public byte BSID; // 5 bits
        public byte BSMod; // 3 bits
        public byte ACMod; // 3 bits
        public byte CMixLev; // 2 bits, present if((acmod & 0x1) && (acmod != 0x1))
        public byte SurMixLev; // 2 bits, present if(acmod & 0x4)
        public byte DSurMod; // 2 bits, present if(acmod == 0x2)
        public bool LfeOn;
        public byte DialNorm; // 5 bits
        public bool CompreFlag; // compre
        public byte Compre;  // 8 bits - compr, present if CompreFlag
        public bool LangCodeFlag; // langcode
        public byte LangCode;// 8 bits - langcod, present if LangCodeFlag
        public bool AudioProductionInfoFlag; // audprodie
        public byte MixLevel; // 5 bits
        public byte RoomTyp; // 2 bits

        // if acmod == 0 (dual mono)
        public byte DialNorm2; // 5 bits
        public bool CompreFlag2; // compr2e
        public byte Compre2;  // 8 bits - compr2, present if CompreFlag
        public bool LangCodeFlag2; // langcod2e
        public byte LangCode2;// 8 bits - langcod2, present if LangCodeFlag
        public bool AudioProductionInfoFlag2; // audprodi2e
        public byte MixLevel2; // 5 bits
        public byte RoomTyp2; // 2 bits

        public bool CopyrightFlag; // copyrightb
        public bool OrigFlag; // origbs
        public bool TimeCode1Flag; // timecod1e
        public ushort TimeCode1; // 14 bits - timecod1, present if TimeCode1Flag
        public bool TimeCode2Flag; // timecod2e
        public ushort TimeCode2; // 14 bits - timecod2, present if TimeCode2Flag
        public bool AdditionalBitstreamInfoFlag; // addbsie
        public byte AdditionalBitstreamInfoLength; // addbsil - 6 bits, present if AdditionalBitstreamInfoFlag
        public byte[] AdditionalBitstreamInfo; // AdditionalBitstreamInfoLength + 1 bytes

        public BSI()
        {

        }

        public BSI(BitStream bitStream)
        {
            BSID = (byte)bitStream.ReadBits(5);
            BSMod = (byte)bitStream.ReadBits(3);
            ACMod = (byte)bitStream.ReadBits(3);
            if (((ACMod & 0x1) > 0) && (ACMod != 0x1))
            {
                CMixLev = (byte)bitStream.ReadBits(2);
            }
            if ((ACMod & 0x4) > 0)
            {
                SurMixLev = (byte)bitStream.ReadBits(2);
            }
            if (ACMod == 0x2)
            {
                DSurMod = (byte)bitStream.ReadBits(2);
            }
            LfeOn = bitStream.ReadBoolean();
            DialNorm = (byte)bitStream.ReadBits(5);
            CompreFlag = bitStream.ReadBoolean();
            if (CompreFlag)
            {
                Compre = bitStream.ReadByte();
            }
            LangCodeFlag = bitStream.ReadBoolean();
            if (LangCodeFlag)
            {
                LangCode = bitStream.ReadByte();
            }
            AudioProductionInfoFlag = bitStream.ReadBoolean();
            if (AudioProductionInfoFlag)
            {
                MixLevel = (byte)bitStream.ReadBits(5);
                RoomTyp = (byte)bitStream.ReadBits(2);
            }

            if (ACMod == 0)
            {
                DialNorm2 = (byte)bitStream.ReadBits(5);
                CompreFlag2 = bitStream.ReadBoolean();
                if (CompreFlag2)
                {
                    Compre = bitStream.ReadByte();
                }
                LangCodeFlag2 = bitStream.ReadBoolean();
                if (LangCodeFlag2)
                {
                    LangCode2 = bitStream.ReadByte();
                }
                AudioProductionInfoFlag2 = bitStream.ReadBoolean();
                if (AudioProductionInfoFlag2)
                {
                    MixLevel2 = (byte)bitStream.ReadBits(5);
                    RoomTyp2 = (byte)bitStream.ReadBits(2);
                }
            }

            CopyrightFlag = bitStream.ReadBoolean();
            OrigFlag = bitStream.ReadBoolean();
            TimeCode1Flag = bitStream.ReadBoolean();
            if (TimeCode1Flag)
            {
                TimeCode1 = (ushort)bitStream.ReadBits(14);
            }
            TimeCode2Flag = bitStream.ReadBoolean();
            if (TimeCode2Flag)
            {
                TimeCode2 = (ushort)bitStream.ReadBits(14);
            }

            AdditionalBitstreamInfoFlag = bitStream.ReadBoolean();
            if (AdditionalBitstreamInfoFlag)
            {
                AdditionalBitstreamInfoLength = (byte)bitStream.ReadBits(6);
                AdditionalBitstreamInfo = new byte[AdditionalBitstreamInfoLength + 1];
                for(int index = 0; index < AdditionalBitstreamInfoLength + 1; index++)
                {
                    AdditionalBitstreamInfo[index] = bitStream.ReadByte();
                }
            }
        }

        public int NumberOfChannels
        {
            get
            {
                return GetNumberOfChannels(ACMod, LfeOn);
            }
        }

        public static int GetNumberOfChannels(byte acMod, bool lfeOn)
        {
            int result = GetNumberOfFullRangeChannels(acMod);
            if (lfeOn)
            {
                result++;
            }
            return result;
        }

        /// <summary>
        /// Return number of channels excluding LFE
        /// </summary>
        public static int GetNumberOfFullRangeChannels(byte acMod)
        {
            switch (acMod)
            {
                case 0:
                    return 2;
                case 1:
                    return 1;
                case 2:
                    return 2;
                case 3:
                    return 3;
                case 4:
                    return 3;
                case 5:
                    return 4;
                case 6:
                    return 4;
                case 7:
                    return 5;
                default:
                    throw new ArgumentException("Invalid audio coding mode");
            }
        }
    }
}
