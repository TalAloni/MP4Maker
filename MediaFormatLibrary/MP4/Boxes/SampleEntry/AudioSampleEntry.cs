using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    public class AudioSampleEntry : SampleEntry
    {
        public ulong Reserved1;
        public ushort ChannelCount;
        public ushort SampleSize;
        public ushort Predefined; // pre_defined
        public ushort Reserved2;
        public double SampleRate; // Fixed point: 16(bits).16(bits)

        public AudioSampleEntry(BoxType type) : base(type)
        {
        }

        public AudioSampleEntry(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            Reserved1 = BigEndianReader.ReadUInt64(stream);
            ChannelCount = BigEndianReader.ReadUInt16(stream);
            SampleSize = BigEndianReader.ReadUInt16(stream);
            Predefined = BigEndianReader.ReadUInt16(stream);
            Reserved2 = BigEndianReader.ReadUInt16(stream);
            SampleRate = MP4Helper.ReadFixedPoint16_16(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            BigEndianWriter.WriteUInt64(stream, Reserved1);
            BigEndianWriter.WriteUInt16(stream, ChannelCount);
            BigEndianWriter.WriteUInt16(stream, SampleSize);
            BigEndianWriter.WriteUInt16(stream, Predefined);
            BigEndianWriter.WriteUInt16(stream, Reserved2);
            MP4Helper.WriteFixedPoint16_16(stream, SampleRate);
        }
    }
}
