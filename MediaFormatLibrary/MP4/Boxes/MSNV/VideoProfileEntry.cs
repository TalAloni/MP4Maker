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
    /// [IEC/TS 62592] VideoProfileEntry
    /// </summary>
    public class VideoProfileEntry : FullBox
    {
        public uint TrackID; // track_ID
        public VideoCodecType CodecType; // codec_type
        public byte[] CodecSpecificInformation; // 4 bytes, codec_specific_information, AVC: these 4 bytes are identical to the first 4 bytes in AVCDecoderConfigurationRecord
        public uint VideoAttributeFlags; // video_attribute_flags, AVC: 0x00000000 for 2D content, Sony 3D demo clips has this set to 0x000B0002, some values (i.e. 0x00060000, 0x00070000) will disable 3D, the first flag 0x00000001 will also disable 3D
        public uint AvgBitRateKbps; // average_bitrate
        public uint MaxBitRateKbps; // max_bitrate
        public double AvgFramerate; // average_frame_rate, Fixed point: 16(bits).16(bits)
        public double MaxFramerate; // max_frame_rate, Fixed point: 16(bits).16(bits)
        public ushort Width; // first half of visual_size (4 bytes)
        public ushort Height; // second half of visual_size (4 bytes)
        public ushort SARNominator; // SAR - Storage aspect ratio, first half of pixel_aspect_ratio (2 bytes)
        public ushort SARDenominator; // second half of pixel_aspect_ratio (2 bytes)

        public VideoProfileEntry() : base(BoxType.VideoProfileEntry)
        {
            CodecSpecificInformation = new byte[4];
        }

        public VideoProfileEntry(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            TrackID = BigEndianReader.ReadUInt32(stream);
            CodecType = (VideoCodecType)BigEndianReader.ReadUInt32(stream);
            CodecSpecificInformation = ByteReader.ReadBytes(stream, 4);
            VideoAttributeFlags = BigEndianReader.ReadUInt32(stream);
            AvgBitRateKbps = BigEndianReader.ReadUInt32(stream);
            MaxBitRateKbps = BigEndianReader.ReadUInt32(stream);
            AvgFramerate = MP4Helper.ReadFixedPoint16_16(stream);
            MaxFramerate = MP4Helper.ReadFixedPoint16_16(stream);
            Width = BigEndianReader.ReadUInt16(stream);
            Height = BigEndianReader.ReadUInt16(stream);
            SARNominator = BigEndianReader.ReadUInt16(stream);
            SARDenominator = BigEndianReader.ReadUInt16(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt32(stream, TrackID);
            BigEndianWriter.WriteUInt32(stream, (uint)CodecType);
            ByteWriter.WriteBytes(stream, CodecSpecificInformation, 4);
            BigEndianWriter.WriteUInt32(stream, VideoAttributeFlags);
            BigEndianWriter.WriteUInt32(stream, AvgBitRateKbps);
            BigEndianWriter.WriteUInt32(stream, MaxBitRateKbps);
            MP4Helper.WriteFixedPoint16_16(stream, AvgFramerate);
            MP4Helper.WriteFixedPoint16_16(stream, MaxFramerate);
            BigEndianWriter.WriteUInt16(stream, Width);
            BigEndianWriter.WriteUInt16(stream, Height);
            BigEndianWriter.WriteUInt16(stream, SARNominator);
            BigEndianWriter.WriteUInt16(stream, SARDenominator);
        }

        public byte AVCConfigurationVersion
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

        public byte AVCProfileIndication
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

        public byte AVCProfileCompatibility
        {
            get
            {
                return CodecSpecificInformation[2]; 
            }
            set
            {
                CodecSpecificInformation[2] = value;
            }
        }

        public byte AVCLevelIndication
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
