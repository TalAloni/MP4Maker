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

namespace MediaFormatLibrary.H264
{
    /// <summary>
    /// [ITU-T H.264] slice_header
    /// </summary>
    public class SliceHeader
    {
        public uint FirstMbInSlice; // first_mb_in_slice
        public SliceType SliceType; // slice_type
        public uint PicParameterSetID; // pic_parameter_set_id
        public byte ColourPlaneID; // colour_plane_id
        public uint FrameNum; // frame_num
        public bool FieldPicFlag; // field_pic_flag
        public bool BottomFieldFlag; // bottom_field_flag
        public uint IdrPicID; // idr_pic_id
        public uint PicOrderCntLsb; // pic_order_cnt_lsb
        public int DeltaPicOrderCntBottom; // delta_pic_order_cnt_bottom
        public int[] DeltaPicOrderCnt = new int[2]; // delta_pic_order_cnt
        public uint RedundantPicCnt; // redundant_pic_cnt
        public bool DirectSpatialMvPredFlag; // direct_spatial_mv_pred_flag
        public bool NumRefIdxActiveOverrideFlag; // num_ref_idx_active_override_flag
        public uint NumRefIdxL0ActiveMinus1; // num_ref_idx_l0_active_minus1
        public uint NumRefIdxL1ActiveMinus1; // num_ref_idx_l1_active_minus1
        public uint CabacInitIdc; // cabac_init_idc
        public int SliceQpDelta; // slice_qp_delta
        public bool SpForSwitchFlag; // sp_for_switch_flag
        public int SliceQsDelta; // slice_qs_delta
        public uint DisableDeblockingFilterIdc; // disable_deblocking_filter_idc
        public int SliceAlphaC0OffsetDiv2; // slice_alpha_c0_offset_div2
        public int SliceBetaOffsetDiv2; // slice_beta_offset_div2
        public uint SliceGroupChangeCycle; // slice_group_change_cycle

        public RefPicListModification RefPicListModification; // ref_pic_list_modification
        public PredWeightTable PredWeightTable; // pred_weight_table
        public DecRefPicMarking DecRefPicMarking; // dec_ref_pic_marking

        private SequenceParameterSetList m_spsList;
        private PictureParameterSetList m_ppsList;
        private bool m_idrPicFlag;
        private byte m_nalRefIdc; // nal_ref_idc

        public SliceHeader(SequenceParameterSetList spsList, PictureParameterSetList ppsList)
        {
            m_spsList = spsList;
            m_ppsList = ppsList;
        }

        public SliceHeader(RawBitStream bitStream, SequenceParameterSetList spsList, PictureParameterSetList ppsList, bool idrPicFlag, byte nalRefIdc)
        {
            m_spsList = spsList;
            m_ppsList = ppsList;
            m_idrPicFlag = idrPicFlag;
            m_nalRefIdc = nalRefIdc;

            FirstMbInSlice = bitStream.ReadExpGolombCodeUnsigned();
            SliceType = (SliceType)bitStream.ReadExpGolombCodeUnsigned();
            PicParameterSetID = bitStream.ReadExpGolombCodeUnsigned();
            PictureParameterSet pps = m_ppsList.GetPictureParameterSet(PicParameterSetID);
            SequenceParameterSet sps = m_spsList.GetSequenceParameterSet(pps.SeqParameterSetID);
            if (sps.SeparateColourPlaneFlag)
            {
                ColourPlaneID = (byte)bitStream.ReadBits(2);
            }
            FrameNum = (uint)bitStream.ReadBits((int)(sps.Log2MaxFrameNumMinus4 + 4));
            if (!sps.FrameMbsOnlyFlag)
            {
                FieldPicFlag = bitStream.ReadBoolean();
                if (FieldPicFlag)
                {
                    BottomFieldFlag = bitStream.ReadBoolean();
                }
            }

            if (idrPicFlag)
            {
                IdrPicID = bitStream.ReadExpGolombCodeUnsigned();
            }

            if (sps.PicOrderCntType == 0)
            {
                PicOrderCntLsb = (uint)bitStream.ReadBits((int)(sps.Log2MaxPicOrderCntLsbMinus4 + 4));
                if (pps.BottomFieldPicOrderInFramePresentFlag && !FieldPicFlag)
                {
                    DeltaPicOrderCntBottom = bitStream.ReadExpGolombCodeSigned();
                }
            }

            if (sps.PicOrderCntType == 1 && !sps.DeltaPicOrderAlwaysZeroFlag)
            {
                DeltaPicOrderCnt[0] = bitStream.ReadExpGolombCodeSigned();
                if (pps.BottomFieldPicOrderInFramePresentFlag && !FieldPicFlag)
                {
                    DeltaPicOrderCnt[1] = bitStream.ReadExpGolombCodeSigned();
                }
            }

            if (pps.RedundantPicCntPresentFlag)
            {
                RedundantPicCnt = bitStream.ReadExpGolombCodeUnsigned();
                if (SliceType == SliceType.SLICE_TYPE_B || SliceType == SliceType.SLICE_TYPE_B_2)
                {
                    DirectSpatialMvPredFlag = bitStream.ReadBoolean();
                }

                if (((uint)SliceType % 5) == (uint)SliceType.SLICE_TYPE_P
                    || ((uint)SliceType % 5) == (uint)SliceType.SLICE_TYPE_SP
                    || ((uint)SliceType % 5) == (uint)SliceType.SLICE_TYPE_B)
                {
                    NumRefIdxActiveOverrideFlag = bitStream.ReadBoolean();
                    if (NumRefIdxActiveOverrideFlag)
                    {
                        NumRefIdxL0ActiveMinus1 = bitStream.ReadExpGolombCodeUnsigned();
                        if (SliceType == SliceType.SLICE_TYPE_B || SliceType == SliceType.SLICE_TYPE_B_2)
                        {
                            NumRefIdxL1ActiveMinus1 = bitStream.ReadExpGolombCodeUnsigned();
                        }
                    }
                }

                RefPicListModification = new RefPicListModification(bitStream);
                if ((pps.WeightedPredFlag && (((uint)SliceType % 5) == (uint)SliceType.SLICE_TYPE_P || ((uint)SliceType % 5) == (uint)SliceType.SLICE_TYPE_SP))
                    || (pps.WeightedBipredIdc == 1 && ((uint)SliceType % 5) == (uint)SliceType.SLICE_TYPE_B))
                {
                    PredWeightTable = new PredWeightTable(bitStream);
                }

                if (m_nalRefIdc != 0)
                {
                    DecRefPicMarking = new DecRefPicMarking(bitStream);
                }
                if (pps.EntropyCodingModeFlag && ((uint)SliceType % 5) != (uint)SliceType.SLICE_TYPE_I
                    && ((uint)SliceType % 5) != (uint)SliceType.SLICE_TYPE_SI)
                {
                    CabacInitIdc = bitStream.ReadExpGolombCodeUnsigned();
                }
                SliceQpDelta = bitStream.ReadExpGolombCodeSigned();

                if (((uint)SliceType % 5) != (uint)SliceType.SLICE_TYPE_SP || ((uint)SliceType % 5) != (uint)SliceType.SLICE_TYPE_SI)
                {
                    if (((uint)SliceType % 5) != (uint)SliceType.SLICE_TYPE_SP)
                    {
                        SpForSwitchFlag = bitStream.ReadBoolean();
                    }
                    SliceQsDelta = bitStream.ReadExpGolombCodeSigned();
                }

                if (pps.DeblockingFilterControlPresentFlag)
                {
                    DisableDeblockingFilterIdc = bitStream.ReadExpGolombCodeUnsigned();
                    if (DisableDeblockingFilterIdc != 1)
                    {
                        SliceAlphaC0OffsetDiv2 = bitStream.ReadExpGolombCodeSigned();
                        SliceBetaOffsetDiv2 = bitStream.ReadExpGolombCodeSigned();
                    }
                }
                if (pps.NumSliceGroupsMinus1 > 0 &&
                    pps.SliceGroupMapType >= 3 && pps.SliceGroupMapType <= 5)
                {

                    int count = (int)Math.Ceiling(Math.Log((pps.PicSizeInMapUnitsMinus1 + 1) / (pps.SliceGroupChangeRateMinus1 + 1) + 1, 2));
                    SliceGroupChangeCycle = (uint)bitStream.ReadBits(count);
                }
            }
        }

        public void WriteBits(RawBitStream bitStream)
        {
            throw new NotImplementedException();
        }

        public bool IdrPicFlag
        {
            get
            {
                return m_idrPicFlag;
            }
        }

        public byte NalRefIdc
        {
            get
            {
                return m_nalRefIdc;
            }
        }

        public static SliceHeader FromCodedSlice(NalUnit codedSlice)
        {
            if (codedSlice is CodedSlice)
            {
                return ((CodedSlice)codedSlice).SliceHeader;
            }
            else if (codedSlice is CodedSliceExtension)
            {
                return ((CodedSliceExtension)codedSlice).SliceHeader;
            }
            return null;
        }
    }
}
