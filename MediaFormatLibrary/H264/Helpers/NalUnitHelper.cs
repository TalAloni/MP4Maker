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
    public class NalUnitHelper
    {
        /// <param name="stream">MemoryStream containing a single NAL unit</param>
        /// <param name="spsList">Will be read from and updated</param>
        /// <param name="ppsList">Will be read from and updated</param>
        public static NalUnit GetNalUnit(MemoryStream stream, SequenceParameterSetList spsList, PictureParameterSetList ppsList)
        {
            NalUnitType nalUnitType = GetNalUnitType(stream);
            switch (nalUnitType)
            {
                case NalUnitType.AccessUnitDelimiter:
                    {
                        return new AccessUnitDelimiter(stream);
                    }
                case NalUnitType.CodedSliceExtension:
                    {
                        return new CodedSliceExtension(stream, spsList, ppsList);
                    }
                case NalUnitType.CodedSliceNonIDR:
                case NalUnitType.CodedSliceIDR:
                    {
                        return new CodedSlice(stream, spsList, ppsList);
                    }
                case NalUnitType.DependencyRepresentationDelimiter:
                    {
                        return new DependencyRepresentationDelimiter();
                    }
                case NalUnitType.PictureParameterSet:
                    {
                        PictureParameterSet pps = new PictureParameterSet(stream, spsList);
                        ppsList.Store(pps);
                        return pps;
                    }
                case NalUnitType.SEI:
                    {
                        return new SEI(stream, spsList);
                    }
                case NalUnitType.SequenceParameterSet:
                    {
                        SequenceParameterSet sps = new SequenceParameterSet(stream);
                        spsList.Store(sps);
                        return sps;
                    }
                case NalUnitType.SequenceParameterSetExtension:
                    {
                        return new SequenceParameterSetExtension(stream);
                    }
                case NalUnitType.SubsetSequenceParameterSet:
                    {
                        SubsetSequenceParameterSet subsetSPS = new SubsetSequenceParameterSet(stream);
                        spsList.Store(subsetSPS);
                        return subsetSPS;
                    }
                default:
                    {
                        return new NalUnit(stream);
                    }
            }
        }

        public static NalUnitType GetNalUnitType(MemoryStream stream)
        {
            BitStream bitStream = new BitStream(stream, true);
            bitStream.ReadBoolean();
            bitStream.ReadBits(2);
            NalUnitType nalUnitType = (NalUnitType)bitStream.ReadBits(5);
            bitStream.Position--;
            return nalUnitType;
        }

        public static AccessUnitDelimiter CreateNewAccessUnit(SliceType sliceTypeName)
        {
            AccessUnitDelimiter accessUnit = new AccessUnitDelimiter();
            accessUnit.NalRefIdc = 0;
            accessUnit.PrimaryPicType = AccessUnitDelimiter.GetPrimaryPicType(sliceTypeName);;
            return accessUnit;
        }

        public static MemoryStream DecodeNalPayloadBytes(byte[] bytes)
        {
            MemoryStream result = new MemoryStream();

            for (int index = 0; index < bytes.Length; index++)
            {
                if (index < bytes.Length - 4 && bytes[index] == 0 && bytes[index + 1] == 0 && bytes[index + 2] == 3 && bytes[index + 3] <= 3) // 00000011
                {
                    result.WriteByte(bytes[index]);
                    result.WriteByte(bytes[index + 1]);
                    index += 2;
                }
                else
                {
                    result.WriteByte(bytes[index]);
                }
            }
            result.Position = 0;
            return result;
        }

        public static MemoryStream EncodeNalPayloadBytes(byte[] bytes)
        {
            MemoryStream result = new MemoryStream();
            for (int index = 0; index < bytes.Length; index++)
            {
                if (index < bytes.Length - 3 && bytes[index] == 0 && bytes[index + 1] == 0 && bytes[index + 2] <= 3) // 00000011
                {
                    result.WriteByte(bytes[index]);
                    result.WriteByte(bytes[index + 1]);
                    result.WriteByte(3);
                    index += 1;
                }
                else
                {
                    result.WriteByte(bytes[index]);
                }
            }
            result.Position = 0;
            return result;
        }
    }
}
