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
    public class FramePackingArrangement : SEIPayload
    {
        public uint FramePackingArrangementID; // frame_packing_arrangement_id
        public bool FramePackingArrangementCancelFlag; // frame_packing_arrangement_cancel_flag
        public FramePackingArrangementType FramePackingArrangementType; // frame_packing_arrangement_type
        public bool QuincunxSamplingFlag; // quincunx_sampling_flag
        public ContentInterpretationType ContentInterpretationType; // content_interpretation_type
        public bool SpatialFlippingFlag; // spatial_flipping_flag
        public bool Frame0FlippedFlag; // frame0_flipped_flag
        public bool FieldViewsFlag; // field_views_flag
        public bool CurrentFrameIsFrame0Flag; // current_frame_is_frame0_flag
        public bool Frame0SelfContainedFlag; // frame0_self_contained_flag
        public bool Frame1SelfContainedFlag; // frame1_self_contained_flag
        public byte Frame0GridPositionX; // frame0_grid_position_x
        public byte Frame0GridPositionY; // frame0_grid_position_y
        public byte Frame1GridPositionX; // frame1_grid_position_x
        public byte Frame1GridPositionY; // frame1_grid_position_y
        public byte FramePackingArrangementReservedByte; // frame_packing_arrangement_reserved_byte
        public uint FramePackingArrangementRepetitionPeriod; // frame_packing_arrangement_repetition_period
        public bool FramePackingArrangementExtensionFlag; // frame_packing_arrangement_extension_flag

        public FramePackingArrangement() : base(SEIPayloadType.FramePackingArrangement)
        { }

        public FramePackingArrangement(RawBitStream bitStream) : base(SEIPayloadType.FramePackingArrangement, bitStream)
        {
            ReadBits(bitStream);
        }

        public override void ReadBits(RawBitStream bitStream)
        {
            FramePackingArrangementID = bitStream.ReadExpGolombCodeUnsigned();
            FramePackingArrangementCancelFlag = bitStream.ReadBoolean();
            if (!FramePackingArrangementCancelFlag)
            {
                FramePackingArrangementType = (FramePackingArrangementType)bitStream.ReadBits(7);
                QuincunxSamplingFlag = bitStream.ReadBoolean();
                ContentInterpretationType = (ContentInterpretationType)bitStream.ReadBits(6);
                SpatialFlippingFlag = bitStream.ReadBoolean();
                Frame0FlippedFlag = bitStream.ReadBoolean();
                FieldViewsFlag = bitStream.ReadBoolean();
                CurrentFrameIsFrame0Flag = bitStream.ReadBoolean();
                Frame0SelfContainedFlag = bitStream.ReadBoolean();
                Frame1SelfContainedFlag = bitStream.ReadBoolean();

                if (!QuincunxSamplingFlag && FramePackingArrangementType != FramePackingArrangementType.FrameSequential)
                {
                    Frame0GridPositionX = (byte)bitStream.ReadBits(4);
                    Frame0GridPositionY = (byte)bitStream.ReadBits(4);
                    Frame1GridPositionX = (byte)bitStream.ReadBits(4);
                    Frame1GridPositionY = (byte)bitStream.ReadBits(4);
                }

                FramePackingArrangementReservedByte = bitStream.ReadByte();
                FramePackingArrangementRepetitionPeriod = bitStream.ReadExpGolombCodeUnsigned();
            }
            FramePackingArrangementExtensionFlag = bitStream.ReadBoolean();
        }

        public override void WriteBits(RawBitStream bitStream)
        {
            bitStream.WriteExpGolombCodeUnsigned(FramePackingArrangementID);
            bitStream.WriteBoolean(FramePackingArrangementCancelFlag);
            if (!FramePackingArrangementCancelFlag)
            {
                bitStream.WriteBits((byte)FramePackingArrangementType, 7);
                bitStream.WriteBoolean(QuincunxSamplingFlag);
                bitStream.WriteBits((byte)ContentInterpretationType, 6);
                bitStream.WriteBoolean(SpatialFlippingFlag);
                bitStream.WriteBoolean(Frame0FlippedFlag);
                bitStream.WriteBoolean(FieldViewsFlag);
                bitStream.WriteBoolean(CurrentFrameIsFrame0Flag);
                bitStream.WriteBoolean(Frame0SelfContainedFlag);
                bitStream.WriteBoolean(Frame1SelfContainedFlag);

                if (!QuincunxSamplingFlag && FramePackingArrangementType != FramePackingArrangementType.FrameSequential)
                {
                    bitStream.WriteBits(Frame0GridPositionX, 4);
                    bitStream.WriteBits(Frame0GridPositionY, 4);
                    bitStream.WriteBits(Frame1GridPositionX, 4);
                    bitStream.WriteBits(Frame1GridPositionY, 4);
                }

                bitStream.WriteByte(FramePackingArrangementReservedByte);
                bitStream.WriteExpGolombCodeUnsigned(FramePackingArrangementRepetitionPeriod);
            }
            bitStream.WriteBoolean(FramePackingArrangementExtensionFlag);
        }
    }
}
