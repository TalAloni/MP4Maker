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
using Utilities;

namespace MediaFormatLibrary.H264
{
    /// <summary>
    /// nal_unit_header_svc_extension
    /// </summary>
    public class NalUnitHeaderSvcExtension
    {
        public bool IdrFlag; // idr_flag
        public byte PriorityID; // 6 bits, priority_id
        public bool NoInterLayerPredFlag; // no_inter_layer_pred_flag
        public byte DependencyID; // 3 bits, dependency_id
        public byte QualityID; // 4 bits, quality_id
        public byte TemporalID; // 3 bits, temporal_id
        public bool UseRefBasePicFlag; // use_ref_base_pic_flag
        public bool DiscardableFlag; // discardable_flag
        public bool OutputFlag; // output_flag
        public byte ReservedThree2bits; // 2 bits, reserved_three_2bits

        public NalUnitHeaderSvcExtension()
        {
            ReservedThree2bits = 0x03;
        }

        public NalUnitHeaderSvcExtension(BitStream bitStream)
        {
            IdrFlag = bitStream.ReadBoolean();
            PriorityID = (byte)bitStream.ReadBits(6);
            NoInterLayerPredFlag = bitStream.ReadBoolean();
            DependencyID = (byte)bitStream.ReadBits(3);
            QualityID = (byte)bitStream.ReadBits(4);
            TemporalID = (byte)bitStream.ReadBits(3);
            UseRefBasePicFlag = bitStream.ReadBoolean();
            DiscardableFlag = bitStream.ReadBoolean();
            OutputFlag = bitStream.ReadBoolean();
            ReservedThree2bits = (byte)bitStream.ReadBits(2);
        }

        public void WriteBits(BitStream bitStream)
        {
            bitStream.WriteBoolean(IdrFlag);
            bitStream.WriteBits(PriorityID, 6);
            bitStream.WriteBoolean(NoInterLayerPredFlag);
            bitStream.WriteBits(DependencyID, 3);
            bitStream.WriteBits(QualityID, 4);
            bitStream.WriteBits(TemporalID, 3);
            bitStream.WriteBoolean(UseRefBasePicFlag);
            bitStream.WriteBoolean(DiscardableFlag);
            bitStream.WriteBoolean(OutputFlag);
            bitStream.WriteBits(ReservedThree2bits, 2);
        }
    }
}
