using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// [ISO/IEC 14496-3] AudioSpecificConfig
    /// </summary>
    public class AudioSpecificConfig : DecoderSpecificInfo
    {
        public AudioObjectType AudioObjectType; // 5 bits
        public byte SamplingFrequencyIndex; // 4 bits
        public byte ChannelConfiguration; // 4 bits

        public AudioSpecificConfig() : base()
        {
        }

        public AudioSpecificConfig(Stream stream) : base(stream)
        {
            int bitOffset = 0;
            AudioObjectType = (AudioObjectType)BitReader.ReadBitsMSB(this.Info, ref bitOffset, 5);
            SamplingFrequencyIndex = (byte)BitReader.ReadBitsMSB(this.Info, ref bitOffset, 4);
            ChannelConfiguration = (byte)BitReader.ReadBitsMSB(this.Info, ref bitOffset, 4);
        }

        public override void WriteBytes(Stream stream)
        {
            this.Info = new byte[2];
            int bitOffset = 0;
            BitWriter.WriteBitsMSB(this.Info, ref bitOffset, (byte)AudioObjectType, 5);
            BitWriter.WriteBitsMSB(this.Info, ref bitOffset, SamplingFrequencyIndex, 4);
            BitWriter.WriteBitsMSB(this.Info, ref bitOffset, ChannelConfiguration, 4);
            base.WriteBytes(stream);
        }

        public override int Length
        {
            get
            {
                return 4;
            }
        }
    }
}
