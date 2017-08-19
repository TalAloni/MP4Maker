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
    public class CodedSliceExtension : NalUnit
    {
        public SliceHeader SliceHeader;

        private byte[] m_rawPayload; // FIXME: payload data, as a temporary measure

        private SequenceParameterSetList m_spsList;
        private PictureParameterSetList m_ppsList;

        public CodedSliceExtension(NalUnitType nalUnitTypeName, SequenceParameterSetList spsList, PictureParameterSetList ppsList) : base(nalUnitTypeName)
        {
            m_spsList = spsList;
            m_ppsList = ppsList;
        }

        public CodedSliceExtension(MemoryStream stream, SequenceParameterSetList spsList, PictureParameterSetList ppsList) : base(stream)
        {
            m_spsList = spsList;
            m_ppsList = ppsList;
            ReadEncodedPayloadBytes(stream);
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            if (SvcExtensionFlag)
            {
                throw new NotImplementedException();
            }
            else
            {
                // [ITU-T H.264] "For NAL units in which non_idr_flag is present, the variable IdrPicFlag derived in clause 7.4.1
                // is modified by setting it equal to 1 when non_idr_flag is equal to 0, and setting it equal to 0 when non_idr_flag is equal to 1"
                bool idrPicFlag = !MvcExtension.NonIdrFlag;
                SliceHeader = new SliceHeader(bitStream, m_spsList, m_ppsList, idrPicFlag, this.NalRefIdc);

                bitStream.Position = 0;
                m_rawPayload = ByteReader.ReadAllBytes(bitStream.BaseStream);
            }
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            if (SvcExtensionFlag)
            {
                throw new NotImplementedException();
            }
            else
            {
                ByteWriter.WriteBytes(bitStream.BaseStream, m_rawPayload);
            }
        }

        public SliceType SliceType
        {
            get
            {
                return SliceHeader.SliceType;
            }
        }

        public uint FrameNum
        {
            get
            {
                return SliceHeader.FrameNum;
            }
        }

        public uint PicOrderCntLsb
        {
            get
            {
                return SliceHeader.PicOrderCntLsb;
            }
        }
    }
}
