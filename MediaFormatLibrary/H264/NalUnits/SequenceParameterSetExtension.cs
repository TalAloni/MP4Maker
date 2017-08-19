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
    public class SequenceParameterSetExtension : NalUnit
    {
        public uint SeqParameterSetID; // seq_parameter_set_id
        public uint AuxFormatIdc; // aux_format_idc
        public uint BitDepthAuxMinus8; // bit_depth_aux_minus8
        public bool AlphaIncrFlag; // alpha_incr_flag
        public uint AlphaOpaqueValue; // alpha_opaque_value
        public uint AlphaTransparentValue; // alpha_transparent_value
        public bool AdditionalExtensionFlag; // additional_extension_flag

        public SequenceParameterSetExtension() : base(NalUnitType.SequenceParameterSetExtension)
        {
        }

        public SequenceParameterSetExtension(MemoryStream stream) : base(stream)
        {
            ReadEncodedPayloadBytes(stream);
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
            SeqParameterSetID = bitStream.ReadExpGolombCodeUnsigned();
            AuxFormatIdc = bitStream.ReadExpGolombCodeUnsigned();
            BitDepthAuxMinus8 = bitStream.ReadExpGolombCodeUnsigned();
            AlphaIncrFlag = bitStream.ReadBoolean();
            AlphaOpaqueValue = bitStream.ReadExpGolombCodeUnsigned();
            AlphaTransparentValue = bitStream.ReadExpGolombCodeUnsigned();
            AdditionalExtensionFlag = bitStream.ReadBoolean();
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            base.WriteRawPayloadBytes(bitStream);
            bitStream.WriteExpGolombCodeUnsigned(SeqParameterSetID);
            bitStream.WriteExpGolombCodeUnsigned(AuxFormatIdc);
            bitStream.WriteExpGolombCodeUnsigned(BitDepthAuxMinus8);
            bitStream.WriteBoolean(AlphaIncrFlag);
            bitStream.WriteExpGolombCodeUnsigned(AlphaOpaqueValue);
            bitStream.WriteExpGolombCodeUnsigned(AlphaTransparentValue);
            bitStream.WriteBoolean(AdditionalExtensionFlag);
            bitStream.WriteRbspTrailingBits();
        }
    }
}
