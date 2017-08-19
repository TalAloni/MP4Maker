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

namespace MediaFormatLibrary.H264
{
    /// <summary>
    /// ref_pic_list_modification
    /// </summary>
    public class RefPicListModification
    {
        public bool RefPicListModificationFlagL0; // ref_pic_list_modification_flag_l0
        public uint ModificationOfPicNumsIdc; // modification_of_pic_nums_idc
        public uint AbsDiffPicNumMinus1; // abs_diff_pic_num_minus1
        public uint LongTermPicNum; // long_term_pic_num
        public bool RefPicListModificationFlagL1; // ref_pic_list_modification_flag_l1
        /*private uint m_modification_of_pic_nums_idc;
        private uint m_abs_diff_pic_num_minus1;
        private uint m_long_term_pic_num;*/

        public RefPicListModification()
        {
        }

        public RefPicListModification(RawBitStream bitStream)
        {
            throw new NotImplementedException();
        }

        public void WriteBits(RawBitStream bitStream)
        {
            throw new NotImplementedException();
        }

        /*[Obsolete]
        public void RefPicListModificationRead(RawBitStream stream)
        {
            if ((SliceType % 5) != 2 && (SliceType % 5) != 4)
            {
                RefPicListModificationFlagL0 = stream.ReadBoolean();
                if (RefPicListModificationFlagL0)
                {
                    do
                    {
                        ModificationOfPicNumsIdc = stream.ReadExpGolombCodeUnsigned();
                        if (ModificationOfPicNumsIdc == 0 || ModificationOfPicNumsIdc == 1)
                        {
                            AbsDiffPicNumMinus1 = stream.ReadExpGolombCodeUnsigned();
                        }
                        else if (ModificationOfPicNumsIdc == 2)
                        {
                            LongTermPicNum = stream.ReadExpGolombCodeUnsigned();
                        }
                    }
                    while (ModificationOfPicNumsIdc != 3);
                }
            }
            if ((SliceType % 5) == 1)
            {
                RefPicListModificationFlagL1 = stream.ReadBoolean();
                if (RefPicListModificationFlagL1)
                {
                    do
                    {
                        ModificationOfPicNumsIdc = stream.ReadExpGolombCodeUnsigned();
                        if (ModificationOfPicNumsIdc == 0 || ModificationOfPicNumsIdc == 1)
                        {
                            AbsDiffPicNumMinus1 = stream.ReadExpGolombCodeUnsigned();
                        }
                        else if (ModificationOfPicNumsIdc == 2)
                        {
                            LongTermPicNum = stream.ReadExpGolombCodeUnsigned();
                        }
                    }
                    while (ModificationOfPicNumsIdc != 3);
                }
            }
        }*/
    }
}
