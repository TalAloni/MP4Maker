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
    public class PictureParameterSet : NalUnit
    {
        public uint PicParameterSetID; // pic_parameter_set_id
        public uint SeqParameterSetID; // seq_parameter_set_id
        public bool EntropyCodingModeFlag; // entropy_coding_mode_flag
        public bool BottomFieldPicOrderInFramePresentFlag; // bottom_field_pic_order_in_frame_present_flag
        public uint NumSliceGroupsMinus1; // num_slice_groups_minus1
        public uint SliceGroupMapType; // slice_group_map_type
        public uint[] RunLengthMinus1; // run_length_minus1
        public uint[] TopLeft; // top_left
        public uint[] BottomRight; // bottom_right
        public bool SliceGroupChangeDirectionFlag; // slice_group_change_direction_flag
        public uint SliceGroupChangeRateMinus1; // slice_group_change_rate_minus1
        public uint PicSizeInMapUnitsMinus1; // pic_size_in_map_units_minus1
        public uint[] SliceGroupID; // slice_group_id
        public uint NumRefIdxL0DefaultActiveMinus1; // num_ref_idx_l0_default_active_minus1
        public uint NumRefIdxL1DefaultActiveMinus1; // num_ref_idx_l1_default_active_minus1
        public bool WeightedPredFlag; // weighted_pred_flag
        public byte WeightedBipredIdc; // weighted_bipred_idc
        public int PicInitQpMinus26; // pic_init_qp_minus26
        public int PicInitQsMinus26; // pic_init_qs_minus26
        public int ChromaQpIndexOffset; // chroma_qp_index_offset
        public bool DeblockingFilterControlPresentFlag; // deblocking_filter_control_present_flag
        public bool ConstrainedIntraPredFlag; // constrained_intra_pred_flag
        public bool RedundantPicCntPresentFlag; // redundant_pic_cnt_present_flag

        public bool MoreData;
        public bool Transform8x8ModeFlag; // transform_8x8_mode_flag
        public bool PicScalingMatrixPresentFlag; // pic_scaling_matrix_present_flag
        public List<bool> PicScalingListPresentFlagList = new List<bool>(); // pic_scaling_list_present_flag
        public List<ScalingList> ScalingLists = new List<ScalingList>();
        public int SecondChromaQpIndexOffset; // second_chroma_qp_index_offset

        private SequenceParameterSetList m_spsList;

        public PictureParameterSet(SequenceParameterSetList spsList) : base(NalUnitType.PictureParameterSet)
        {
            m_spsList = spsList;
        }

        public PictureParameterSet(MemoryStream stream, SequenceParameterSet sps) : base(stream)
        {
            m_spsList = new SequenceParameterSetList();
            m_spsList.Store(sps);
            ReadEncodedPayloadBytes(stream);
        }

        public PictureParameterSet(MemoryStream stream, SequenceParameterSetList spsList) : base(stream)
        {
            m_spsList = spsList;
            ReadEncodedPayloadBytes(stream);
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            PicParameterSetID = bitStream.ReadExpGolombCodeUnsigned();
            SeqParameterSetID = bitStream.ReadExpGolombCodeUnsigned();
            SequenceParameterSet sps = m_spsList.GetSequenceParameterSet(SeqParameterSetID);
            if (sps == null)
            {
                throw new InvalidDataException("Invalid SeqParameterSetID");
            }
            EntropyCodingModeFlag = bitStream.ReadBoolean();
            BottomFieldPicOrderInFramePresentFlag = bitStream.ReadBoolean();
            NumSliceGroupsMinus1 = bitStream.ReadExpGolombCodeUnsigned();
            if (NumSliceGroupsMinus1 > 0)
            {
                SliceGroupMapType = bitStream.ReadExpGolombCodeUnsigned();
                if (SliceGroupMapType == 0)
                {
                    RunLengthMinus1 = new uint[NumSliceGroupsMinus1];
                    for (int index = 0; index < NumSliceGroupsMinus1; index++)
                    {
                        RunLengthMinus1[index] = bitStream.ReadExpGolombCodeUnsigned();
                    }
                }
                else if (SliceGroupMapType == 2)
                {
                    TopLeft = new uint[NumSliceGroupsMinus1];
                    BottomRight = new uint[NumSliceGroupsMinus1];
                    for (int index = 0; index < NumSliceGroupsMinus1; index++)
                    {
                        TopLeft[index] = bitStream.ReadExpGolombCodeUnsigned();
                        BottomRight[index] = bitStream.ReadExpGolombCodeUnsigned();
                    }
                }
                else if (SliceGroupMapType == 3 || SliceGroupMapType == 4 || SliceGroupMapType == 5)
                {
                    SliceGroupChangeDirectionFlag = bitStream.ReadBoolean();
                    SliceGroupChangeRateMinus1 = bitStream.ReadExpGolombCodeUnsigned(); ;
                }
                else if (SliceGroupMapType == 6)
                {
                    PicSizeInMapUnitsMinus1 = bitStream.ReadExpGolombCodeUnsigned();
                    SliceGroupID = new uint[PicSizeInMapUnitsMinus1];
                    for (int index = 0; index < NumSliceGroupsMinus1; index++)
                    {
                        SliceGroupID[index] = bitStream.ReadExpGolombCodeUnsigned();
                    }
                }
            }

            NumRefIdxL0DefaultActiveMinus1 = bitStream.ReadExpGolombCodeUnsigned();
            NumRefIdxL1DefaultActiveMinus1 = bitStream.ReadExpGolombCodeUnsigned();
            WeightedPredFlag = bitStream.ReadBoolean();
            WeightedBipredIdc = (byte)bitStream.ReadBits(2);
            PicInitQpMinus26 = bitStream.ReadExpGolombCodeSigned();
            PicInitQsMinus26 = bitStream.ReadExpGolombCodeSigned();
            ChromaQpIndexOffset = bitStream.ReadExpGolombCodeSigned();

            DeblockingFilterControlPresentFlag = bitStream.ReadBoolean();
            ConstrainedIntraPredFlag = bitStream.ReadBoolean();
            RedundantPicCntPresentFlag = bitStream.ReadBoolean();

            MoreData = bitStream.MoreRbspData();
            if (MoreData)
            {
                Transform8x8ModeFlag = bitStream.ReadBoolean();
                PicScalingMatrixPresentFlag = bitStream.ReadBoolean();
                if (PicScalingMatrixPresentFlag)
                {
                    int count = 6 + ( ( sps.ChromaFormatIdc != 3 ) ? 2 : 6 ) * Convert.ToInt32(Transform8x8ModeFlag);
                    for (int i = 0; i < count; i++)
                    {
                        bool picScalingListPresentFlag = bitStream.ReadBoolean();
                        PicScalingListPresentFlagList.Add(picScalingListPresentFlag);
                        if (picScalingListPresentFlag)
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
                SecondChromaQpIndexOffset = bitStream.ReadExpGolombCodeSigned();
            }
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            SequenceParameterSet sps = m_spsList.GetSequenceParameterSet(SeqParameterSetID);
            
            bitStream.WriteExpGolombCodeUnsigned(PicParameterSetID);
            bitStream.WriteExpGolombCodeUnsigned(SeqParameterSetID);
            bitStream.WriteBoolean(EntropyCodingModeFlag);
            bitStream.WriteBoolean(BottomFieldPicOrderInFramePresentFlag);
            bitStream.WriteExpGolombCodeUnsigned(NumSliceGroupsMinus1);
            if (NumSliceGroupsMinus1 > 0)
            {
                bitStream.WriteExpGolombCodeUnsigned(SliceGroupMapType);
                if (SliceGroupMapType == 0)
                {
                    for (int index = 0; index < NumSliceGroupsMinus1; index++)
                    {
                        bitStream.WriteExpGolombCodeUnsigned(RunLengthMinus1[index]);
                    }
                }
                else if (SliceGroupMapType == 2)
                {
                    for (int index = 0; index < NumSliceGroupsMinus1; index++)
                    {
                        bitStream.WriteExpGolombCodeUnsigned(TopLeft[index]);
                        bitStream.WriteExpGolombCodeUnsigned(BottomRight[index]);
                    }
                }
                else if (SliceGroupMapType == 3 || SliceGroupMapType == 4 || SliceGroupMapType == 5)
                {
                    bitStream.WriteBoolean(SliceGroupChangeDirectionFlag);
                    bitStream.WriteExpGolombCodeUnsigned(SliceGroupChangeRateMinus1);
                }
                else if (SliceGroupMapType == 6)
                {
                    bitStream.WriteExpGolombCodeUnsigned(PicSizeInMapUnitsMinus1);
                    for (int index = 0; index < NumSliceGroupsMinus1; index++)
                    {
                        bitStream.WriteExpGolombCodeUnsigned(SliceGroupID[index]);
                    }
                }
            }

            bitStream.WriteExpGolombCodeUnsigned(NumRefIdxL0DefaultActiveMinus1);
            bitStream.WriteExpGolombCodeUnsigned(NumRefIdxL1DefaultActiveMinus1);
            bitStream.WriteBoolean(WeightedPredFlag);
            bitStream.WriteBits(WeightedBipredIdc, 2);
            bitStream.WriteExpGolombCodeSigned(PicInitQpMinus26);
            bitStream.WriteExpGolombCodeSigned(PicInitQsMinus26);
            bitStream.WriteExpGolombCodeSigned(ChromaQpIndexOffset);

            bitStream.WriteBoolean(DeblockingFilterControlPresentFlag);
            bitStream.WriteBoolean(ConstrainedIntraPredFlag);
            bitStream.WriteBoolean(RedundantPicCntPresentFlag);

            if (MoreData)
            {
                bitStream.WriteBoolean(Transform8x8ModeFlag);
                bitStream.WriteBoolean(PicScalingMatrixPresentFlag);
                if (PicScalingMatrixPresentFlag)
                {
                    int count = 6 + ((sps.ChromaFormatIdc != 3) ? 2 : 6) * Convert.ToInt32(Transform8x8ModeFlag);
                    for (int i = 0; i < count; i++)
                    {
                        bool picScalingListPresentFlag = PicScalingListPresentFlagList[i];
                        bitStream.WriteBoolean(picScalingListPresentFlag);
                        if (picScalingListPresentFlag)
                        {
                            ScalingLists[i].WriteBits(bitStream);
                        }
                    }
                }
                bitStream.WriteExpGolombCodeSigned(SecondChromaQpIndexOffset);
            }

            bitStream.WriteRbspTrailingBits();
        }
    }
}
