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

namespace MediaFormatLibrary.Mpeg2
{
    /// <summary>
    /// AVC_video_descriptor
    /// </summary>
    public class AVCVideoDescriptor : Descriptor
    {
        public byte ProfileIdc; // profile_idc
        public bool ConstraintSet0Flag; // constraint_set0_flag
        public bool ConstraintSet1Flag; // constraint_set1_flag
        public bool ConstraintSet2Flag; // constraint_set2_flag
        public byte AVCCompatibleFlags; // 5 bits, AVC_compatible_flags
        public byte LevelIdc; // level_idc
        public bool AVCStillPresent; // AVC_still_present
        public bool AVC24HourPictureFlag; // AVC_24_hour_picture_flag
        public byte Reserved; // 6 bits, reserved

        public AVCVideoDescriptor()
        {
            this.Tag = DescriptorTag.AVCVideoDescriptor;
            Reserved = 0x3F;
        }

        public AVCVideoDescriptor(byte[] buffer, ref int offset) : base(buffer, ref offset)
        {
            int bitOffset = 0;
            ProfileIdc = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 8);
            ConstraintSet0Flag = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            ConstraintSet1Flag = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            ConstraintSet2Flag = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            AVCCompatibleFlags = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 5);
            LevelIdc = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 8);
            AVCStillPresent = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            AVC24HourPictureFlag = BitReader.ReadBooleanMSB(this.Data, ref bitOffset);
            Reserved = (byte)BitReader.ReadBitsMSB(this.Data, ref bitOffset, 6);
        }

        public override void WriteBytes(byte[] buffer, ref int offset)
        {
            this.Data = new byte[4];
            int bitOffset = 0;
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, ProfileIdc, 8);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, ConstraintSet0Flag);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, ConstraintSet1Flag);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, ConstraintSet2Flag);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, AVCCompatibleFlags, 5);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, LevelIdc, 8);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, AVCStillPresent);
            BitWriter.WriteBooleanMSB(this.Data, ref bitOffset, AVC24HourPictureFlag);
            BitWriter.WriteBitsMSB(this.Data, ref bitOffset, Reserved, 6);

            base.WriteBytes(buffer, ref offset);
        }

        public override int Length
        {
            get
            {
                return HeaderLength + 4;
            }
        }
    }
}
