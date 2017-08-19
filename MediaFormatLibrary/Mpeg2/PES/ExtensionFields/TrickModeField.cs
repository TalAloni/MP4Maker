/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.Mpeg2
{
    public class TrickModeField
    {
        public byte TrickModeControl; // 3 bits, trick_mode_control
        public byte FieldID; // 2 bits, field_id
        public bool IntraSliceRefresh; // intra_slice_refresh
        public byte FrequencyTruncation; // 2 bits, frequency_truncation
        public byte RepeatControl; // 3 bits, rep_cntrl

        public TrickModeField()
        {
        }

        public TrickModeField(byte[] buffer, ref int offset)
        {
            throw new NotImplementedException();
        }

        public void WriteBytes(byte[] buffer, ref int offset)
        {
            throw new NotImplementedException();
        }
    }
}
