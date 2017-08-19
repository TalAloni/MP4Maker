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

namespace MediaFormatLibrary.AAC
{
    /// <summary>
    /// [ISO/IEC 14496-3] adts_fixed_header
    /// </summary>
    public class AdtsFixedHeader
    {
        public const ushort SyncWord = 0xFFF;

        // syncword - 12 bits - 0xFFF
        public bool ID;
        public byte Layer; // 2 bits
        public bool ProtectionAbsent; // protection_absent;
        public byte ProfileObjectType; // profile_ObjectType - 2 bits
        public byte SamplingFrequencyIndex; // sampling_frequency_index - 4 bits, the escape value is not permitted
        public bool PrivateBit; // private_bit
        public byte ChannelConfiguration; // channel_configuration - 3 bits
        public bool OriginalCopy; // original_copy
        public bool Home; // private_bit

        public AdtsFixedHeader()
        {
        }

        public AdtsFixedHeader(BitStream stream)
        {
            ushort syncword = (ushort)stream.ReadBits(12);
            if (syncword != SyncWord)
            {
                throw new Exception("Invalid ADTS syncword");
            }
            ID = stream.ReadBoolean();
            Layer = (byte)stream.ReadBits(2);
            ProtectionAbsent = stream.ReadBoolean();
            ProfileObjectType = (byte)stream.ReadBits(2);
            SamplingFrequencyIndex = (byte)stream.ReadBits(4);
            PrivateBit = stream.ReadBoolean();
            ChannelConfiguration = (byte)stream.ReadBits(3);
            OriginalCopy = stream.ReadBoolean();
            Home = stream.ReadBoolean();
        }

        public void WriteBytes(BitStream stream)
        {
            stream.WriteBits(SyncWord, 12);
            stream.WriteBoolean(ID);
            stream.WriteBits(Layer, 2);
            stream.WriteBoolean(ProtectionAbsent);
            stream.WriteBits(ProfileObjectType, 2);
            stream.WriteBits(SamplingFrequencyIndex, 4);
            stream.WriteBoolean(PrivateBit);
            stream.WriteBits(ChannelConfiguration, 3);
            stream.WriteBoolean(OriginalCopy);
            stream.WriteBoolean(Home);
        }

        public int Length
        {
            get
            {
                return (ProtectionAbsent == true) ? 7 : 9;
            }
        }

        public ADTSProfile AudioProfile
        {
            get
            {
                if (ID)
                {
                    return (ADTSProfile)(ProfileObjectType + 4);
                }
                else
                {
                    return (ADTSProfile)(ProfileObjectType);
                }
            }
            set
            {
                ID = (byte)value >= 4;
                ProfileObjectType = (byte)((byte)value % 4);
            }
        }

        /// <summary>
        /// Sample rate
        /// </summary>
        public int SamplingFrequency
        {
            get
            {
                return GetSamplingFrequency(SamplingFrequencyIndex);
            }
        }

        public int NumberOfChannels
        {
            get
            {
                return GetNumberOfChannels(ChannelConfiguration);
            }
        }

        public static int GetSamplingFrequency(byte samplingFrequencyIndex)
        {
            switch (samplingFrequencyIndex)
            {
                case 0x00:
                    return 96000;
                case 0x01:
                    return 88200;
                case 0x02:
                    return 64000;
                case 0x03:
                    return 48000;
                case 0x04:
                    return 44100;
                case 0x05:
                    return 32000;
                case 0x06:
                    return 24000;
                case 0x07:
                    return 22500;
                case 0x08:
                    return 16000;
                case 0x09:
                    return 12000;
                case 0x0A:
                    return 11025;
                case 0x0B:
                    return 8000;
                case 0x0C:
                    return 7350;
                case 0x0D:
                case 0x0E:
                    throw new Exception("Unsupported sampling_frequency_index");
                default:
                    throw new Exception("Invalid sampling_frequency_index");
            }
        }

        public static byte? GetSamplingFrequencyIndex(int samplingFrequency)
        {
            switch (samplingFrequency)
            {
                case 96000:
                    return 0x00;
                case 88200:
                    return 0x01;
                case 64000:
                    return 0x02;
                case 48000:
                    return 0x03;
                case 44100:
                    return 0x04;
                case 32000:
                    return 0x05;
                case 24000:
                    return 0x06;
                case 22500:
                    return 0x07;
                case 16000:
                    return 0x08;
                case 12000:
                    return 0x09;
                case 11025:
                    return 0x0A;
                case 8000:
                    return 0x0B;
                case 7350:
                    return 0x0C;
                case 0x0D:
                default:
                    return null;
            }
        }

        public static int GetNumberOfChannels(byte channelConfiguration)
        {
            if (channelConfiguration == 0)
            {
                // A single program_config_element() following as first syntactic element in the first
                // raw_data_block() after the header specifies the channel configuration.
                throw new NotImplementedException("Unsupported channel_configuration");
            }
            else if (channelConfiguration >= 1 && channelConfiguration <= 6)
            {
                return channelConfiguration; 
            }
            else if (channelConfiguration == 7)
            {
                return 8;
            }
            else
            {
                throw new Exception("Unsupported channel_configuration");
            }
        }
    }
}
