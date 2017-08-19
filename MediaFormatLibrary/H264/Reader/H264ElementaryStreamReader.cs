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
    public partial class H264ElementaryStreamReader
    {
        private H264ElementaryStream m_stream;
        private SequenceParameterSetList m_spsList = new SequenceParameterSetList(); // // "A decoder must be capable of simultaneously storing the contents of the sequence parameter sets for all values of seq_parameter_set_id"
        private PictureParameterSetList m_ppsList = new PictureParameterSetList(); // "A decoder must be capable of simultaneously storing the contents of the picture parameter sets for all values of pic_parameter_set_id".
        private bool m_hasAccessUnitDelimiters;
        private NalUnit m_previousCodedSlice;

        private int m_decodingIndex;

        private MemoryStream m_pendingNalUnit;
        
        public H264ElementaryStreamReader(H264ElementaryStream stream)
        {
            m_stream = stream;

            m_pendingNalUnit = stream.ReadNalUnitStream();
            if (m_pendingNalUnit != null)
            {
                NalUnitType unitType = NalUnitHelper.GetNalUnitType(m_pendingNalUnit);
                // When an access unit delimiter NAL unit is present, it shall be the first NAL unit
                m_hasAccessUnitDelimiters = (unitType == NalUnitType.AccessUnitDelimiter || unitType == NalUnitType.DependencyRepresentationDelimiter);
            }
        }

        public H264AccessUnit ReadAccessUnit()
        {
            List<MemoryStream> accessUnitNALs = ReadAccessUnitNALs();
            if (accessUnitNALs != null)
            {
                H264AccessUnit accessUnit = new H264AccessUnit();
                accessUnit.AddRange(accessUnitNALs);
                foreach (MemoryStream nalUnit in accessUnit)
                {
                    NalUnitType nalUnitType = NalUnitHelper.GetNalUnitType(nalUnit);
                    
                    if (nalUnitType == NalUnitType.CodedSliceIDR || nalUnitType == NalUnitType.CodedSliceNonIDR)
                    {
                        if (nalUnitType == NalUnitType.CodedSliceIDR)
                        {
                            accessUnit.IsIDRPicture = true;
                        }
                        CodedSlice codedSlice = new CodedSlice(nalUnit, m_spsList, m_ppsList);
                        accessUnit.PicOrderCntLsb = codedSlice.PicOrderCntLsb;
                    }
                    else if (nalUnitType == NalUnitType.CodedSliceExtension)
                    {
                        CodedSliceExtension codedSliceExtension = new CodedSliceExtension(nalUnit, m_spsList, m_ppsList);
                        if (!codedSliceExtension.MvcExtension.NonIdrFlag)
                        {
                            accessUnit.IsIDRPicture = true;
                        }
                        accessUnit.PicOrderCntLsb = codedSliceExtension.PicOrderCntLsb;
                    }
                    nalUnit.Position = 0;
                }
                accessUnit.DecodingOrder = m_decodingIndex;
                m_decodingIndex++;
                return accessUnit;
            }
            return null;
        }

        /// <summary>
        /// Access Unit:
        /// A set of NAL units that are consecutive in decoding order and contain exactly one primary
        /// coded picture. In addition to the primary coded picture, an access unit may also contain
        /// one or more redundant coded pictures, one auxiliary coded picture, or other NAL units not
        /// containing slices or slice data partitions of a coded picture.
        /// The decoding of an access unit always results in a decoded picture.
        /// </summary>
        public List<MemoryStream> ReadAccessUnitNALs()
        {
            MemoryStream nalUnitStream;
            if (m_pendingNalUnit != null)
            {
                nalUnitStream = m_pendingNalUnit;
                m_pendingNalUnit = null;
            }
            else
            {
                nalUnitStream = m_stream.ReadNalUnitStream();
            }

            List<MemoryStream> accessUnit = new List<MemoryStream>();
            bool hasCodedSlice = false;
            while (nalUnitStream != null)
            {
                NalUnitType nalUnitType;
                bool startOfNewAccessUnit;
                if (m_hasAccessUnitDelimiters == true)
                {
                    nalUnitType = NalUnitHelper.GetNalUnitType(nalUnitStream);
                    startOfNewAccessUnit = (accessUnit.Count > 0) && (nalUnitType == NalUnitType.AccessUnitDelimiter || nalUnitType == NalUnitType.DependencyRepresentationDelimiter);

                    if (nalUnitType == NalUnitType.SequenceParameterSet)
                    {
                        m_spsList.Store(new SequenceParameterSet(nalUnitStream));
                    }
                    else if (nalUnitType == NalUnitType.SubsetSequenceParameterSet)
                    {
                        m_spsList.Store(new SubsetSequenceParameterSet(nalUnitStream));
                    }
                    else if (nalUnitType == NalUnitType.PictureParameterSet)
                    {
                        m_ppsList.Store(new PictureParameterSet(nalUnitStream, m_spsList));
                    }
                }
                else
                {
                    NalUnit nalUnit = NalUnitHelper.GetNalUnit(nalUnitStream, m_spsList, m_ppsList);
                    nalUnitType = nalUnit.NalUnitType;
                    startOfNewAccessUnit = hasCodedSlice &&
                                            (nalUnit is AccessUnitDelimiter || nalUnit is SequenceParameterSet || nalUnit is PictureParameterSet ||
                                             nalUnit is SEI || ((byte)nalUnit.NalUnitType >= 14 && (byte)nalUnit.NalUnitType <= 18) ||
                                            IsFirstVclInPrimaryCodedPicture(nalUnit));
                    if (nalUnit is CodedSlice || nalUnit is CodedSliceExtension)
                    {
                        hasCodedSlice = true;
                        m_previousCodedSlice = nalUnit;
                    }
                }

                nalUnitStream.Position = 0;
                if (!startOfNewAccessUnit)
                {
                    accessUnit.Add(nalUnitStream);
                }
                else
                {
                    m_pendingNalUnit = nalUnitStream;
                    break;
                }
                nalUnitStream = m_stream.ReadNalUnitStream();
            }

            if (accessUnit.Count > 0)
            {
                return accessUnit;
            }
            return null;
        }

        // See: [ITU-T H.264] 7.4.1.2.4 Detection of the first VCL NAL unit of a primary coded picture
        private bool IsFirstVclInPrimaryCodedPicture(NalUnit nalUnit)
        {
            return IsFirstVclInPrimaryCodedPicture(nalUnit, m_previousCodedSlice, m_spsList, m_ppsList);
        }

        // See: [ITU-T H.264] 7.4.1.2.4 Detection of the first VCL NAL unit of a primary coded picture
        public static bool IsFirstVclInPrimaryCodedPicture(NalUnit nalUnit, NalUnit previousCodedSlice, SequenceParameterSetList spsList, PictureParameterSetList ppsList)
        {
            SliceHeader sliceHeader = SliceHeader.FromCodedSlice(nalUnit);
            if (sliceHeader == null)
            {
                return false;
            }

            if (previousCodedSlice == null)
            {
                return true;
            }

            SliceHeader previousSliceHeader = SliceHeader.FromCodedSlice(previousCodedSlice);

            if (sliceHeader.FrameNum != previousSliceHeader.FrameNum ||
                sliceHeader.PicParameterSetID != previousSliceHeader.PicParameterSetID ||
                sliceHeader.FieldPicFlag != previousSliceHeader.FieldPicFlag ||
                sliceHeader.BottomFieldFlag != previousSliceHeader.BottomFieldFlag ||
                sliceHeader.NalRefIdc != previousSliceHeader.NalRefIdc)
            {
                return true;
            }
            
            PictureParameterSet pps = ppsList.GetPictureParameterSet(sliceHeader.PicParameterSetID);
            SequenceParameterSet sps = spsList.GetSequenceParameterSet(pps.SeqParameterSetID);
            if (sps.PicOrderCntType == 0 && (sliceHeader.PicOrderCntLsb != previousSliceHeader.PicOrderCntLsb || sliceHeader.DeltaPicOrderCntBottom != previousSliceHeader.DeltaPicOrderCntBottom))
            {
                return true;
            }
            
            if (sps.PicOrderCntType == 1 && (sliceHeader.DeltaPicOrderCnt[0] != previousSliceHeader.DeltaPicOrderCnt[0] || sliceHeader.DeltaPicOrderCnt[1] != previousSliceHeader.DeltaPicOrderCnt[1]))
            {
                return true;
            }

            if (sliceHeader.IdrPicFlag != previousSliceHeader.IdrPicFlag)
            {
                return true;
            }

            if (sliceHeader.IdrPicFlag && previousSliceHeader.IdrPicFlag && (sliceHeader.IdrPicID != previousSliceHeader.IdrPicID))
            {
                return true;
            }

            return false;
        }

        public NalUnit ReadDecodedNalUnit()
        {
            MemoryStream nalUnitStream;
            if (m_pendingNalUnit != null)
            {
                nalUnitStream = m_pendingNalUnit;
                m_pendingNalUnit = null;
            }
            else
            {
                nalUnitStream = m_stream.ReadNalUnitStream();
            }

            if (nalUnitStream != null)
            {
                NalUnit nalUnit = NalUnitHelper.GetNalUnit(nalUnitStream, m_spsList, m_ppsList);
                return nalUnit;
            }

            return null;
        }

        public List<NalUnit> ReadDecodedAccessUnit()
        {
            List<NalUnit> result = new List<NalUnit>();
            List<MemoryStream> accessUnit = ReadAccessUnitNALs();
            foreach (MemoryStream nalUnitStream in accessUnit)
            {
                NalUnit nalUnit = NalUnitHelper.GetNalUnit(nalUnitStream, m_spsList, m_ppsList);
                result.Add(nalUnit);
            }
            return result;
        }
    }
}
