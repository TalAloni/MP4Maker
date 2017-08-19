using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFormatLibrary.H264
{
    public enum SliceType : uint
    {
        SLICE_TYPE_P = 0,
        SLICE_TYPE_B = 1,
        SLICE_TYPE_I = 2,
        SLICE_TYPE_SP = 3,
        SLICE_TYPE_SI = 4,
        SLICE_TYPE_P_2 = 5,
        SLICE_TYPE_B_2 = 6,
        SLICE_TYPE_I_2 = 7,
        SLICE_TYPE_SP_2 = 8,
        SLICE_TYPE_SI_2 = 9,
    }
}
