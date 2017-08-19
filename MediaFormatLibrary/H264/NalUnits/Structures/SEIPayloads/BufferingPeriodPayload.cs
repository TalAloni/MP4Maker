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
    public class BufferingPeriodPayload : SEIPayload
    {
        public uint SeqParameterSetID; // seq_parameter_set_id
        public uint[] NalHrdInitialCpbRemovalDelay; // initial_cpb_removal_delay
        public uint[] NalHrdInitialCpbRemovalDelayOffset; // initial_cpb_removal_delay_offset
        public uint[] VclHrdInitialCpbRemovalDelay; // initial_cpb_removal_delay
        public uint[] VclHrdInitialCpbRemovalDelayOffset; // initial_cpb_removal_delay_offset

        private SequenceParameterSetList m_spsList;

        public BufferingPeriodPayload(SequenceParameterSetList spsList) : base(SEIPayloadType.BufferingPeriod)
        {
            m_spsList = spsList;
        }

        public BufferingPeriodPayload(RawBitStream bitStream, SequenceParameterSetList spsList) : base(SEIPayloadType.BufferingPeriod)
        {
            m_spsList = spsList;
            ReadBits(bitStream);
        }

        public override void ReadBits(RawBitStream bitStream)
        {
            SeqParameterSetID = bitStream.ReadExpGolombCodeUnsigned();
            SequenceParameterSet sps = m_spsList.GetSequenceParameterSet(SeqParameterSetID);

            if (sps.NalHrdParameters != null)
            {
                NalHrdInitialCpbRemovalDelay = new uint[sps.NalHrdParameters.CpbCntMinus1 + 1];
                NalHrdInitialCpbRemovalDelayOffset = new uint[sps.NalHrdParameters.CpbCntMinus1 + 1];
                for (int index = 0; index < sps.NalHrdParameters.CpbCntMinus1 + 1; index++)
                {
                    NalHrdInitialCpbRemovalDelay[index] = (uint)bitStream.ReadBits((int)sps.NalHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                    NalHrdInitialCpbRemovalDelayOffset[index] = (uint)bitStream.ReadBits((int)sps.NalHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                }
            }

            if (sps.VclHrdParameters != null)
            {
                VclHrdInitialCpbRemovalDelay = new uint[sps.NalHrdParameters.CpbCntMinus1 + 1];
                VclHrdInitialCpbRemovalDelayOffset = new uint[sps.NalHrdParameters.CpbCntMinus1 + 1];
                for (int index = 0; index < sps.VclHrdParameters.CpbCntMinus1 + 1; index++)
                {
                    VclHrdInitialCpbRemovalDelay[index] = (uint)bitStream.ReadBits((int)sps.VclHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                    VclHrdInitialCpbRemovalDelayOffset[index] = (uint)bitStream.ReadBits((int)sps.VclHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                }
            }
        }

        public override void WriteBits(RawBitStream bitStream)
        {
            SequenceParameterSet sps = m_spsList.GetSequenceParameterSet(SeqParameterSetID);

            bitStream.WriteExpGolombCodeUnsigned(SeqParameterSetID);

            if (sps.NalHrdParameters != null)
            {
                for (int index = 0; index < sps.NalHrdParameters.CpbCntMinus1 + 1; index++)
                {
                    bitStream.WriteBits(NalHrdInitialCpbRemovalDelay[index], (int)sps.NalHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                    bitStream.WriteBits(NalHrdInitialCpbRemovalDelayOffset[index], (int)sps.NalHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                }
            }

            if (sps.VclHrdParameters != null)
            {
                for (int index = 0; index < sps.VclHrdParameters.CpbCntMinus1 + 1; index++)
                {
                    bitStream.WriteBits(VclHrdInitialCpbRemovalDelay[index], (int)sps.VclHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                    bitStream.WriteBits(VclHrdInitialCpbRemovalDelayOffset[index], (int)sps.VclHrdParameters.InitialCpbRemovalDelayLengthMinus1 + 1);
                }
            }
        }
    }
}
