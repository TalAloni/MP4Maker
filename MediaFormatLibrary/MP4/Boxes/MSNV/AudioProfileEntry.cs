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

namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// [IEC/TS 62592] AudioProfileEntry
    /// </summary>
    public class AudioProfileEntry : FullBox
    {
        public uint TrackID; // track_ID
        public AudioCodecType CodecType; // codec_type
        public byte[] CodecSpecificInformation; // 4 bytes, codec_specific_information
        public uint AudioAttributeFlags; // audio_attribute_flags
        public uint AvgBitRateKbps; // average_bitrate
        public uint MaxBitRateKbps; // max_bitrate
        public uint SamplingRate; // sampling_rate
        public uint NumberOfChannels; // audio_channel_number

        public AudioProfileEntry() : base(BoxType.AudioProfileEntry)
        {
            CodecSpecificInformation = new byte[4];
        }

        public AudioProfileEntry(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            TrackID = BigEndianReader.ReadUInt32(stream);
            CodecType = (AudioCodecType)BigEndianReader.ReadUInt32(stream);
            CodecSpecificInformation = ByteReader.ReadBytes(stream, 4);
            AudioAttributeFlags = BigEndianReader.ReadUInt32(stream);
            AvgBitRateKbps = BigEndianReader.ReadUInt32(stream);
            MaxBitRateKbps = BigEndianReader.ReadUInt32(stream);
            SamplingRate = BigEndianReader.ReadUInt32(stream);
            NumberOfChannels = BigEndianReader.ReadUInt32(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, TrackID);
            BigEndianWriter.WriteUInt32(stream, (uint)CodecType);
            ByteWriter.WriteBytes(stream, CodecSpecificInformation, 4);
            BigEndianWriter.WriteUInt32(stream, AudioAttributeFlags);
            BigEndianWriter.WriteUInt32(stream, AvgBitRateKbps);
            BigEndianWriter.WriteUInt32(stream, MaxBitRateKbps);
            BigEndianWriter.WriteUInt32(stream, SamplingRate);
            BigEndianWriter.WriteUInt32(stream, NumberOfChannels);
        }

        /// <summary>
        /// 0x00 - ISO/IEC 14496-3 (MPEG-4 audio stream)
        /// </summary>
        public byte AACDataFormType
        {
            get
            {
                return CodecSpecificInformation[0];
            }
            set
            {
                CodecSpecificInformation[0] = value;
            }
        }

        public byte AACReserved
        {
            get
            {
                return CodecSpecificInformation[1];
            }
            set
            {
                CodecSpecificInformation[1] = value;
            }
        }

        /// <summary>
        /// MSNV notes:
        /// IEC/TS 62592 specify that we must use AAC LC (0x02)
        /// </summary>
        public AudioObjectType AACObjectType
        {
            get
            {
                return (AudioObjectType)(CodecSpecificInformation[2] & 0x1F);
            }
            set
            {
                CodecSpecificInformation[2] = (byte)value;
            }
        }

        /// <summary>
        /// Defined in ISO/IEC 14496-3
        /// MSNV notes:
        /// IEC/TS 62592 specify that we must use an AAC Profile, and defined classes use L2(0x29) or L4 (0x2B)
        /// For successful 3D playback this must NOT be set to 0
        /// Seen: 0x0F (PSP), 0x29
        /// </summary>
        public byte AACProfileLevel
        {
            get
            {
                return CodecSpecificInformation[3];
            }
            set
            {
                CodecSpecificInformation[3] = value;
            }
        }
    }
}
