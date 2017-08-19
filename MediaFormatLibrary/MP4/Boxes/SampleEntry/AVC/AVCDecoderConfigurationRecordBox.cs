/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Utilities;
using MediaFormatLibrary.H264;

namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// See: http://thompsonng.blogspot.co.il/2010/11/mp4-file-format-part-2.html
    /// </summary>
    public class AVCDecoderConfigurationRecordBox : Box
    {
        public byte ConfigurationVersion; // always 1
        public byte AVCProfileIndication;
        public byte AVCProfileCompatibility; // 8 bits int value that occurs between profile_IDC and level_IDC in the SPS
        public byte AVCLevelIndication;
        public byte Reserved1; // 6 bits, set to '111111'
        /// <summary>
        /// MP4 (ISO/IEC 14496-12) stream, "mdat" payload carries NAL units in length-data format (i.e. [LengthOfNalUnit1][NalUnit1][LengthOfNalUnit2][NalUnit2]).
        /// The size of the length field is signaled in LengthSizeMinusOne and it can be 1, 2, or 4 bytes.
        /// See ISO/IEC 14496-15 section 5.2.3 for the detail.
        /// </summary>
        public byte LengthSizeMinus1; // 2 bits, 
        public byte Reserved2; // 3 bits, set to '111'
        // numOfSequenceParameterSets; // 5 bits
        public List<SequenceParameterSet> SequenceParameterSetList = new List<SequenceParameterSet>();
        // numOfPictureParameterSets;
        public List<PictureParameterSet> PictureParameterSetList = new List<PictureParameterSet>();

        public AVCDecoderConfigurationRecordBox() : base(BoxType.AVCDecoderConfigurationRecordBox)
        {}

        public AVCDecoderConfigurationRecordBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            ConfigurationVersion = (byte)stream.ReadByte();
            AVCProfileIndication = (byte)stream.ReadByte();
            AVCProfileCompatibility = (byte)stream.ReadByte();
            AVCLevelIndication = (byte)stream.ReadByte();
            byte temp = (byte)stream.ReadByte();
            Reserved1 = (byte)(temp & 0xFC);
            LengthSizeMinus1 = (byte)(temp & 0x03);
            temp = (byte)stream.ReadByte();
            Reserved2 = (byte)(temp & 0xE0);
            byte NumOfSequenceParameterSets = (byte)(temp & 0x1F);
            for (int index = 0; index < NumOfSequenceParameterSets; index++)
            {
                ushort spsLength = BigEndianReader.ReadUInt16(stream);
                MemoryStream nalUnitStream = new MemoryStream();
                ByteUtils.CopyStream(stream, nalUnitStream, spsLength);
                nalUnitStream.Position = 0;
                SequenceParameterSet sps = new SequenceParameterSet(nalUnitStream);
                SequenceParameterSetList.Add(sps);
            }
            byte numOfPictureParameterSets = (byte)stream.ReadByte();
            for (int index = 0; index < numOfPictureParameterSets; index++)
            {
                ushort ppsLength = BigEndianReader.ReadUInt16(stream);
                MemoryStream nalUnitStream = new MemoryStream();
                ByteUtils.CopyStream(stream, nalUnitStream, ppsLength);
                nalUnitStream.Position = 0;
                SequenceParameterSet sps = SequenceParameterSetList[index];
                PictureParameterSet pps = new PictureParameterSet(nalUnitStream, sps);
                PictureParameterSetList.Add(pps);
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            stream.WriteByte(ConfigurationVersion);
            stream.WriteByte(AVCProfileIndication);
            stream.WriteByte(AVCProfileCompatibility);
            stream.WriteByte(AVCLevelIndication);
            stream.WriteByte((byte)(0xFC | (LengthSizeMinus1 & 0x03)));
            stream.WriteByte((byte)(0xE0 | (SequenceParameterSetList.Count & 0x1F)));
            foreach (SequenceParameterSet sps in SequenceParameterSetList)
            {
                long startPosition = stream.Position;
                BigEndianWriter.WriteUInt16(stream, 0); // We're going to write the sps length afterwards
                sps.WriteBytes(stream);
                long endPosition = stream.Position;
                stream.Position = startPosition;
                BigEndianWriter.WriteUInt16(stream, (ushort)(endPosition - (startPosition + 2)));
                stream.Position = endPosition;
            }
            stream.WriteByte((byte)PictureParameterSetList.Count);
            foreach (PictureParameterSet pps in PictureParameterSetList)
            {
                long startPosition = stream.Position;
                BigEndianWriter.WriteUInt16(stream, 0); // We're going to write the sps length afterwards
                pps.WriteBytes(stream);
                long endPosition = stream.Position;
                stream.Position = startPosition;
                BigEndianWriter.WriteUInt16(stream, (ushort)(endPosition - (startPosition + 2)));
                stream.Position = endPosition;
            }
        }
    }
}
