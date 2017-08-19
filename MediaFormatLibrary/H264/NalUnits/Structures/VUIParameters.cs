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

namespace MediaFormatLibrary.H264
{
    /// <summary>
    /// Video usability information parameters
    /// </summary>
    public class VUIParameters
    {
        public bool AspectRatioInfoPresentFlag; // aspect_ratio_info_present_flag
        public byte AspectRatioIdc; // aspect_ratio_idc
        public ushort SarWidth; // sar_width
        public ushort SarHeight; // sar_height
        public bool OverscanInfoPresentFlag; // overscan_info_present_flag
        public bool OverscanAppropriateFlag; // overscan_appropriate_flag
        public bool VideoSignalTypePresentFlag; // video_signal_type_present_flag
        public byte VideoFormat; // video_format
        public bool VideoFullRangeFlag; // video_full_range_flag
        public bool ColourDescriptionPresentFlag; // colour_description_present_flag
        public byte ColourPrimaries; // colour_primaries
        public byte TransferCharacteristics; // transfer_characteristics
        public byte MatrixCoefficients; // matrix_coefficients
        public bool ChromaLocInfoPresentFlag; // chroma_loc_info_present_flag
        public uint ChromaSampleLocTypeTopField; // chroma_sample_loc_type_top_field
        public uint ChromaSampleLocYypeBottomField; // chroma_sample_loc_type_bottom_field
        public bool TimingInfoPresentFlag; // timing_info_present_flag
        public uint NumUnitsInTick; // num_units_in_tick, "the number of time units of a clock operating at the frequency time_scale"
        public uint TimeScale; // time_scale
        public bool FixedFrameRateFlag; // fixed_frame_rate_flag
        // nal_hrd_parameters_present_flag
        // vcl_hrd_parameters_present_flag
        public bool LowDelayHrdFlag; // low_delay_hrd_flag
        public bool PicStructPresentFlag; // pic_struct_present_flag
        public bool BitstreamRestrictionFlag; // bitstream_restriction_flag
        public bool MotionVectorsOverPicBoundariesFlag; // motion_vectors_over_pic_boundaries_flag
        public uint MaxBytesPerPicDenom; // max_bytes_per_pic_denom
        public uint MaxBitsPerMbDenom; // max_bits_per_mb_denom
        public uint Log2MaxMvLengthHorizontal; // log2_max_mv_length_horizontal
        public uint Log2MaxMvLengthVertical; // log2_max_mv_length_vertical
        public uint MaxNumReorderFrames; // max_num_reorder_frames
        public uint MaxDecFrameBuffering; // max_dec_frame_buffering

        // HRD parameters
        public HRDParameters NalHrdParameters;
        public HRDParameters VclHrdParameters;

        public VUIParameters()
        {
        }

        public VUIParameters(RawBitStream bitStream)
        {
            AspectRatioInfoPresentFlag = bitStream.ReadBoolean();
            if (AspectRatioInfoPresentFlag)
            {
                AspectRatioIdc = bitStream.ReadByte();
                if (AspectRatioIdc == 255) //Extended_SAR
                {
                    SarWidth = bitStream.ReadUInt16();
                    SarHeight = bitStream.ReadUInt16();
                }
            }
            OverscanInfoPresentFlag = bitStream.ReadBoolean();
            if (OverscanInfoPresentFlag)
            {
                OverscanAppropriateFlag = bitStream.ReadBoolean();
            }
            VideoSignalTypePresentFlag = bitStream.ReadBoolean();
            if (VideoSignalTypePresentFlag)
            {
                VideoFormat = (byte)bitStream.ReadBits(3);
                VideoFullRangeFlag = bitStream.ReadBoolean();
                ColourDescriptionPresentFlag = bitStream.ReadBoolean();
                if (ColourDescriptionPresentFlag)
                {
                    ColourPrimaries = bitStream.ReadByte();
                    TransferCharacteristics = bitStream.ReadByte();
                    MatrixCoefficients = bitStream.ReadByte();
                }
            }
            ChromaLocInfoPresentFlag = bitStream.ReadBoolean();
            if (ChromaLocInfoPresentFlag)
            {
                ChromaSampleLocTypeTopField = bitStream.ReadExpGolombCodeUnsigned();
                ChromaSampleLocYypeBottomField = bitStream.ReadExpGolombCodeUnsigned();
            }
            TimingInfoPresentFlag = bitStream.ReadBoolean();
            if (TimingInfoPresentFlag)
            {
                NumUnitsInTick = bitStream.ReadUInt32();
                TimeScale = bitStream.ReadUInt32();
                FixedFrameRateFlag = bitStream.ReadBoolean();
            }

            bool nalHrdParametersPresentFlag = bitStream.ReadBoolean();
            if (nalHrdParametersPresentFlag)
            {
                NalHrdParameters = new HRDParameters(bitStream);
            }
            bool vclHrdParametersPresentFlag = bitStream.ReadBoolean();
            if (vclHrdParametersPresentFlag)
            {
                VclHrdParameters = new HRDParameters(bitStream);
            }
            if (nalHrdParametersPresentFlag || vclHrdParametersPresentFlag)
            {
                LowDelayHrdFlag = bitStream.ReadBoolean();
            }
            PicStructPresentFlag = bitStream.ReadBoolean();
            BitstreamRestrictionFlag = bitStream.ReadBoolean();
            if (BitstreamRestrictionFlag)
            {
                MotionVectorsOverPicBoundariesFlag = bitStream.ReadBoolean();
                MaxBytesPerPicDenom = bitStream.ReadExpGolombCodeUnsigned();
                MaxBitsPerMbDenom = bitStream.ReadExpGolombCodeUnsigned();
                Log2MaxMvLengthHorizontal = bitStream.ReadExpGolombCodeUnsigned();
                Log2MaxMvLengthVertical = bitStream.ReadExpGolombCodeUnsigned();
                MaxNumReorderFrames = bitStream.ReadExpGolombCodeUnsigned();
                MaxDecFrameBuffering = bitStream.ReadExpGolombCodeUnsigned();
            }
        }

        public void WriteBits(RawBitStream bitStream)
        {
            bitStream.WriteBoolean(AspectRatioInfoPresentFlag);

            if (AspectRatioInfoPresentFlag)
            {
                bitStream.WriteBits(AspectRatioIdc, 8);

                if (AspectRatioIdc == 255) //Extended_SAR
                {
                    bitStream.WriteBits(SarWidth, 16);
                    bitStream.WriteBits(SarHeight, 16);
                }
            }
            bitStream.WriteBoolean(OverscanInfoPresentFlag);

            if (OverscanInfoPresentFlag)
            {
                bitStream.WriteBoolean(OverscanAppropriateFlag);
            }
            bitStream.WriteBoolean(VideoSignalTypePresentFlag);

            if (VideoSignalTypePresentFlag)
            {
                bitStream.WriteBits(VideoFormat, 3);
                bitStream.WriteBoolean(VideoFullRangeFlag);
                bitStream.WriteBoolean(ColourDescriptionPresentFlag);
                if (ColourDescriptionPresentFlag)
                {
                    bitStream.WriteBits(ColourPrimaries, 8);
                    bitStream.WriteBits(TransferCharacteristics, 8);
                    bitStream.WriteBits(MatrixCoefficients, 8);
                }
            }
            bitStream.WriteBoolean(ChromaLocInfoPresentFlag);

            if (ChromaLocInfoPresentFlag)
            {
                bitStream.WriteExpGolombCodeUnsigned(ChromaSampleLocTypeTopField);
                bitStream.WriteExpGolombCodeUnsigned(ChromaSampleLocYypeBottomField);
            }
            bitStream.WriteBoolean(TimingInfoPresentFlag);

            if (TimingInfoPresentFlag)
            {
                bitStream.WriteBits(NumUnitsInTick, 32);
                bitStream.WriteBits(TimeScale, 32);
                bitStream.WriteBoolean(FixedFrameRateFlag);
            }

            bitStream.WriteBoolean(NalHrdParameters != null);
            if (NalHrdParameters != null)
            {
                NalHrdParameters.WriteBits(bitStream);
            }

            bitStream.WriteBoolean(VclHrdParameters != null);
            if (VclHrdParameters != null)
            {
                VclHrdParameters.WriteBits(bitStream);
            }

            if (NalHrdParameters != null || VclHrdParameters != null)
            {
                bitStream.WriteBoolean(LowDelayHrdFlag);
            }
            bitStream.WriteBoolean(PicStructPresentFlag);
            bitStream.WriteBoolean(BitstreamRestrictionFlag);

            if (BitstreamRestrictionFlag)
            {
                bitStream.WriteBoolean(MotionVectorsOverPicBoundariesFlag);
                bitStream.WriteExpGolombCodeUnsigned(MaxBytesPerPicDenom);
                bitStream.WriteExpGolombCodeUnsigned(MaxBitsPerMbDenom);
                bitStream.WriteExpGolombCodeUnsigned(Log2MaxMvLengthHorizontal);
                bitStream.WriteExpGolombCodeUnsigned(Log2MaxMvLengthVertical);
                bitStream.WriteExpGolombCodeUnsigned(MaxNumReorderFrames);
                bitStream.WriteExpGolombCodeUnsigned(MaxDecFrameBuffering);
            }
        }

        // "A clock tick is the minimum interval of time that can be represented in the coded data"
        public uint? MinimumFrameDurationInTimeScale
        {
            get
            {
                if (this.TimingInfoPresentFlag)
                {
                    // nuit_field_based_flag shall be always 1
                    // See: http://codesequoia.wordpress.com/2011/01/30/understand-h-264-time-code/
                    const uint nuit_field_based_flag = 1;
                    return this.NumUnitsInTick * (1 + nuit_field_based_flag);
                }
                return null;
            }
        }

        public double? FrameRate
        {
            get
            {
                if (this.TimingInfoPresentFlag && this.FixedFrameRateFlag)
                {
                    double frameRate = (double)this.TimeScale / this.MinimumFrameDurationInTimeScale.Value;
                    return frameRate;
                }
                return null;
            }
        }
    }
}
