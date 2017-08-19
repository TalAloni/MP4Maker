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

namespace MediaFormatLibrary.H264
{
    public class SequenceParameterSet : NalUnit
    {
        public byte ProfileIdc; // profile_idc
        public bool ConstraintSet0Flag; // constraint_set0_flag
        public bool ConstraintSet1Flag; // constraint_set1_flag
        public bool ConstraintSet2Flag; // constraint_set2_flag
        public bool ConstraintSet3Flag; // constraint_set3_flag
        public bool ConstraintSet4Flag; // constraint_set4_flag
        public bool ConstraintSet5Flag; // constraint_set4_flag
        public byte ReservedZero2Bits; // reserved_zero_4bits
        public byte LevelIdc; // level_idc
        public uint SeqParameterSetID; // seq_parameter_set_id
        public uint ChromaFormatIdc; // chroma_format_idc
        public bool SeparateColourPlaneFlag; // separate_colour_plane_flag
        public uint BitDepthLumaMinus8; // bit_depth_luma_minus8
        public uint BitDepthChromaMinus8; // bit_depth_chroma_minus8
        public bool QpprimeYZeroTransformBypassFlag; // qpprime_y_zero_transform_bypass_flag
        public bool SeqScalingMatrixPresentFlag; // seq_scaling_matrix_present_flag
        public List<bool> SeqScalingListPresentFlagList = new List<bool>(); // seq_scaling_list_present_flag
        public List<ScalingList> ScalingLists = new List<ScalingList>(); // scaling_list
        public uint Log2MaxFrameNumMinus4; // log2_max_frame_num_minus4
        public uint PicOrderCntType; // pic_order_cnt_type
        public uint Log2MaxPicOrderCntLsbMinus4; // log2_max_pic_order_cnt_lsb_minus4
        public bool DeltaPicOrderAlwaysZeroFlag; // delta_pic_order_always_zero_flag
        public int OffsetForNonRefPic; // offset_for_non_ref_pic
        public int OffsetForTopToBottomField; // offset_for_top_to_bottom_field
        public uint NumRefFramesInPicOrderCntCycle; // num_ref_frames_in_pic_order_cnt_cycle
        public int[] OffsetForRefFrame; // offset_for_ref_frame
        public uint NumRefFrames; // num_ref_frames
        public bool GapsInFrameNumValueAllowedFlag; // gaps_in_frame_num_value_allowed_flag
        public uint PicWidthInMbsMinus1; // pic_width_in_mbs_minus1
        public uint PicHeightInMapUnitsMinus1; // pic_height_in_map_units_minus1
        public bool FrameMbsOnlyFlag; // frame_mbs_only_flag
        public bool MbAdaptiveFrameFieldFlag; // mb_adaptive_frame_field_flag
        public bool Direct8x8InferenceFlag; // direct_8x8_inference_flag
        public bool FrameCroppingFlag; // frame_cropping_flag
        public uint FrameCropLeftOffset; // frame_crop_left_offset
        public uint FrameCropRightOffset; // frame_crop_right_offset
        public uint FrameCropTopOffset; // frame_crop_top_offset
        public uint FrameCropBottomOffset; // frame_crop_bottom_offset
        // bool vui_parameters_present_flag
        public VUIParameters VUIParameters;

        public SequenceParameterSet() : base(NalUnitType.SequenceParameterSet)
        {
        }

        public SequenceParameterSet(NalUnitType nalUnitType) : base(nalUnitType)
        {
        }

        public SequenceParameterSet(MemoryStream stream) : base(stream)
        {
            ReadEncodedPayloadBytes(stream);
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            ReadSequenceParameterSetData(bitStream);
        }

        public void ReadSequenceParameterSetData(RawBitStream bitStream)
        {
            ProfileIdc = bitStream.ReadByte();
            ConstraintSet0Flag = bitStream.ReadBoolean();
            ConstraintSet1Flag = bitStream.ReadBoolean();
            ConstraintSet2Flag = bitStream.ReadBoolean();
            ConstraintSet3Flag = bitStream.ReadBoolean();
            ConstraintSet4Flag = bitStream.ReadBoolean();
            ConstraintSet5Flag = bitStream.ReadBoolean();
            ReservedZero2Bits = (byte)bitStream.ReadBits(2);

            LevelIdc = bitStream.ReadByte();
            SeqParameterSetID = bitStream.ReadExpGolombCodeUnsigned();
            if (ProfileIdc == 100 || ProfileIdc == 110 || ProfileIdc == 122 || ProfileIdc == 244 || ProfileIdc == 44 ||
                ProfileIdc == 83 || ProfileIdc == 86 || ProfileIdc == 118 || ProfileIdc == 128 || ProfileIdc == 138 || ProfileIdc == 139 || ProfileIdc == 134)
            {
                ChromaFormatIdc = bitStream.ReadExpGolombCodeUnsigned();
                if (ChromaFormatIdc == 3)
                {
                    SeparateColourPlaneFlag = bitStream.ReadBoolean();
                }
                BitDepthLumaMinus8 = bitStream.ReadExpGolombCodeUnsigned();
                BitDepthChromaMinus8 = bitStream.ReadExpGolombCodeUnsigned();
                QpprimeYZeroTransformBypassFlag = bitStream.ReadBoolean();
                SeqScalingMatrixPresentFlag = bitStream.ReadBoolean();
                if (SeqScalingMatrixPresentFlag)
                {
                    int count = (ChromaFormatIdc != 3) ? 8 : 12;
                    for (int i = 0; i < count; i++)
                    {
                        bool seqScalingListPresentFlag = bitStream.ReadBoolean();
                        SeqScalingListPresentFlagList.Add(seqScalingListPresentFlag);
                        if (seqScalingListPresentFlag)
                        {
                            ScalingList scalingList;
                            if (i < 6)
                            {
                                scalingList = new ScalingList(bitStream, 16);
                            }
                            else
                            {
                                scalingList = new ScalingList(bitStream, 64);
                            }
                            ScalingLists.Add(scalingList);
                        }
                    }
                }
            }

            Log2MaxFrameNumMinus4 = bitStream.ReadExpGolombCodeUnsigned();
            PicOrderCntType = bitStream.ReadExpGolombCodeUnsigned();
            if (PicOrderCntType == 0)
            {
                Log2MaxPicOrderCntLsbMinus4 = bitStream.ReadExpGolombCodeUnsigned();
            }
            else if (PicOrderCntType == 1)
            {
                DeltaPicOrderAlwaysZeroFlag = bitStream.ReadBoolean();
                OffsetForNonRefPic = bitStream.ReadExpGolombCodeSigned();
                OffsetForTopToBottomField = bitStream.ReadExpGolombCodeSigned();
                NumRefFramesInPicOrderCntCycle = bitStream.ReadExpGolombCodeUnsigned();
                
                OffsetForRefFrame = new int[NumRefFramesInPicOrderCntCycle];
                for (int i = 0; i < NumRefFramesInPicOrderCntCycle; i++)
                {
                    OffsetForRefFrame[i] = bitStream.ReadExpGolombCodeSigned();
                }
            }
            NumRefFrames = bitStream.ReadExpGolombCodeUnsigned();
            GapsInFrameNumValueAllowedFlag = bitStream.ReadBoolean();
            PicWidthInMbsMinus1 = bitStream.ReadExpGolombCodeUnsigned();
            PicHeightInMapUnitsMinus1 = bitStream.ReadExpGolombCodeUnsigned();
            FrameMbsOnlyFlag = bitStream.ReadBoolean();
            if (!FrameMbsOnlyFlag)
            {
                MbAdaptiveFrameFieldFlag = bitStream.ReadBoolean();
            }
            Direct8x8InferenceFlag = bitStream.ReadBoolean();
            FrameCroppingFlag = bitStream.ReadBoolean();
            if (FrameCroppingFlag)
            {
                FrameCropLeftOffset = bitStream.ReadExpGolombCodeUnsigned();
                FrameCropRightOffset = bitStream.ReadExpGolombCodeUnsigned();
                FrameCropTopOffset = bitStream.ReadExpGolombCodeUnsigned();
                FrameCropBottomOffset = bitStream.ReadExpGolombCodeUnsigned();
            }
            bool vuiParametersPresentFlag = bitStream.ReadBoolean();
            if (vuiParametersPresentFlag)
            {
                VUIParameters = new VUIParameters(bitStream);
            }
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            WriteSequenceParameterSetData(bitStream);
            bitStream.WriteRbspTrailingBits();
        }

        public void WriteSequenceParameterSetData(RawBitStream bitStream)
        {
            bitStream.WriteByte(ProfileIdc);
            bitStream.WriteBoolean(ConstraintSet0Flag);
            bitStream.WriteBoolean(ConstraintSet1Flag);
            bitStream.WriteBoolean(ConstraintSet2Flag);
            bitStream.WriteBoolean(ConstraintSet3Flag);
            bitStream.WriteBoolean(ConstraintSet4Flag);
            bitStream.WriteBoolean(ConstraintSet5Flag);
            bitStream.WriteBits(ReservedZero2Bits, 2);

            bitStream.WriteByte(LevelIdc);

            bitStream.WriteExpGolombCodeUnsigned(SeqParameterSetID);

            if (ProfileIdc == 100 || ProfileIdc == 110 || ProfileIdc == 122 || ProfileIdc == 244 || ProfileIdc == 44 ||
                ProfileIdc == 83 || ProfileIdc == 86 || ProfileIdc == 118 || ProfileIdc == 128 || ProfileIdc == 138 || ProfileIdc == 139 || ProfileIdc == 134)
            {
                bitStream.WriteExpGolombCodeUnsigned(ChromaFormatIdc);
                if (ChromaFormatIdc == 3)
                {
                    bitStream.WriteBoolean(SeparateColourPlaneFlag);
                }
                bitStream.WriteExpGolombCodeUnsigned(BitDepthLumaMinus8);
                bitStream.WriteExpGolombCodeUnsigned(BitDepthChromaMinus8);
                bitStream.WriteBoolean(QpprimeYZeroTransformBypassFlag);
                bitStream.WriteBoolean(SeqScalingMatrixPresentFlag);

                if (SeqScalingMatrixPresentFlag)
                {
                    int count = (ChromaFormatIdc != 3) ? 8 : 12;

                    for (int i = 0; i < count; i++)
                    {
                        bitStream.WriteBoolean(SeqScalingListPresentFlagList[i]);
                        if (SeqScalingListPresentFlagList[i])
                        {
                            ScalingLists[i].WriteBits(bitStream);
                        }
                    }
                }
            }

            bitStream.WriteExpGolombCodeUnsigned(Log2MaxFrameNumMinus4);
            bitStream.WriteExpGolombCodeUnsigned(PicOrderCntType);

            if (PicOrderCntType == 0)
            {
                bitStream.WriteExpGolombCodeUnsigned(Log2MaxPicOrderCntLsbMinus4);
            }
            else if (PicOrderCntType == 1)
            {
                bitStream.WriteBoolean(DeltaPicOrderAlwaysZeroFlag);
                bitStream.WriteExpGolombCodeSigned(OffsetForNonRefPic);
                bitStream.WriteExpGolombCodeSigned(OffsetForTopToBottomField);

                bitStream.WriteExpGolombCodeUnsigned(NumRefFramesInPicOrderCntCycle);

                for (int i = 0; i < NumRefFramesInPicOrderCntCycle; i++)
                {
                    bitStream.WriteExpGolombCodeSigned(OffsetForRefFrame[i]);
                }
            }
            bitStream.WriteExpGolombCodeUnsigned(NumRefFrames);
            bitStream.WriteBoolean(GapsInFrameNumValueAllowedFlag);
            bitStream.WriteExpGolombCodeUnsigned(PicWidthInMbsMinus1);
            bitStream.WriteExpGolombCodeUnsigned(PicHeightInMapUnitsMinus1);
            bitStream.WriteBoolean(FrameMbsOnlyFlag);

            if (!FrameMbsOnlyFlag)
            {
                bitStream.WriteBoolean(MbAdaptiveFrameFieldFlag);
            }
            bitStream.WriteBoolean(Direct8x8InferenceFlag);
            bitStream.WriteBoolean(FrameCroppingFlag);

            if (FrameCroppingFlag)
            {
                bitStream.WriteExpGolombCodeUnsigned(FrameCropLeftOffset);
                bitStream.WriteExpGolombCodeUnsigned(FrameCropRightOffset);
                bitStream.WriteExpGolombCodeUnsigned(FrameCropTopOffset);
                bitStream.WriteExpGolombCodeUnsigned(FrameCropBottomOffset);
            }

            bitStream.WriteBoolean(VUIParameters != null);
            if (VUIParameters != null)
            {
                VUIParameters.WriteBits(bitStream);
            }
        }

        public HRDParameters NalHrdParameters
        {
            get
            {
                if (VUIParameters != null)
                {
                    return VUIParameters.NalHrdParameters;
                }
                return null;
            }
            set
            {
                VUIParameters.NalHrdParameters = value;
            }
        }

        public HRDParameters VclHrdParameters
        {
            get
            {
                if (VUIParameters != null)
                {
                    return VUIParameters.VclHrdParameters;
                }
                return null;
            }
            set
            {
                VUIParameters.VclHrdParameters = value;
            }
        }

        public uint Width
        {
            get
            {
                uint width = ((PicWidthInMbsMinus1 + 1) * 16);
                if (FrameCroppingFlag)
                {
                    width -= (FrameCropLeftOffset * 2 + FrameCropRightOffset * 2);
                }
                return  width;
            }
        }

        public uint Height
        {
            get
            {
                uint height = ((2 - Convert.ToUInt32(FrameMbsOnlyFlag)) * (PicHeightInMapUnitsMinus1 + 1) * 16);
                if (FrameCroppingFlag)
                {
                    height -= (FrameCropTopOffset * 2 + FrameCropBottomOffset * 2);
                }
                return height;
            }
        }

        public uint MaxPicOrderCntLsb
        {
            get
            {
                return (uint)Math.Pow(2, Log2MaxPicOrderCntLsbMinus4 + 4);
            }
        }

        public uint NumReorderFrames
        {
            get
            {
                uint numReorderFrames = 16;
                if (VUIParameters != null && VUIParameters.BitstreamRestrictionFlag)
                {
                    numReorderFrames = VUIParameters.MaxNumReorderFrames;
                }
                return numReorderFrames;
            }
        }
    }
}
