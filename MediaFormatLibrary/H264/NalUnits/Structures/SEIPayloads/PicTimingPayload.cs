/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFormatLibrary.H264
{
    public class PicTimingPayload : SEIPayload
    {
        public uint CpbRemovalDelay; // cpb_removal_delay
        public uint DpbOutputDelay; // dpb_output_delay
        public byte PicStruct; // pic_struct
        public bool[] ClockTimestampFlag; // clock_timestamp_flag
        public byte[] CtType; // ct_type
        public bool[] NuitFieldBasedFlag; // nuit_field_based_flag
        public byte[] CountingType; // counting_type
        public bool[] FullTimestampFlag; // full_timestamp_flag
        public bool[] DiscontinuityFlag; // discontinuity_flag
        public bool[] CntDroppedFlag; // cnt_dropped_flag
        public byte[] NFrames; // n_frames
        public byte[] SecondsValue; // seconds_value
        public byte[] MinutesValue; // minutes_value
        public byte[] HoursValue; // hours_value
        public bool[] SecondsFlag; // seconds_flag
        public bool[] MinutesFlag; // minutes_flag
        public bool[] HoursFlag; // hours_flag
        public int[] TimeOffset; // time_offset

        private SequenceParameterSet m_sps;

        public PicTimingPayload(SequenceParameterSet activeSPS) : base(SEIPayloadType.PicTiming)
        {
            m_sps = activeSPS;
        }

        /// <summary>
        /// The syntax of the picture timing SEI message is dependent on the content of the sequence
        /// parameter set that is active for the primary coded picture associated with the picture timing
        /// SEI message. However, unless the picture timing SEI message of an IDR access unit is preceded
        /// by a buffering period SEI message within the same access unit, the activation of the
        /// associated sequence parameter set (and, for IDR pictures that are not the first picture in
        /// the bitstream, the determination that the primary coded picture is an IDR picture) does not
        /// occur until the decoding of the first coded slice NAL unit of the primary coded picture.
        /// Since the coded slice NAL unit of the primary coded picture follows the picture timing SEI
        /// message in NAL unit order, there may be cases in which it is necessary for a decoder to store
        /// the RBSP containing the picture timing SEI message until determining the parameters of the
        /// sequence parameter that will be active for the primary coded picture, and then perform the
        /// parsing of the picture timing SEI message.
        /// </summary>
        public PicTimingPayload(RawBitStream bitStream, SequenceParameterSet activeSPS) : base(SEIPayloadType.PicTiming)
        {
            m_sps = activeSPS;
            ReadBits(bitStream);
        }

        public override void  ReadBits(RawBitStream bitStream)
        {
            if (m_sps.NalHrdParameters != null || m_sps.VclHrdParameters != null)
            {
                CpbRemovalDelay = (uint)bitStream.ReadBits((int)m_sps.NalHrdParameters.CpbRemovalDelayLengthMinus1 + 1);
                DpbOutputDelay = (uint)bitStream.ReadBits((int)m_sps.NalHrdParameters.DpbOutputDelayLengthMinus1 + 1);
            }

            if (m_sps.VUIParameters != null && m_sps.VUIParameters.PicStructPresentFlag)
            {
                PicStruct = (byte)bitStream.ReadBits(4);
                int numClockTS = GetNumClockTS(PicStruct);
                ClockTimestampFlag = new bool[numClockTS];

                CtType = new byte[numClockTS];
                NuitFieldBasedFlag = new bool[numClockTS];
                CountingType = new byte[numClockTS];
                FullTimestampFlag = new bool[numClockTS];
                DiscontinuityFlag = new bool[numClockTS];
                CntDroppedFlag = new bool[numClockTS];
                NFrames = new byte[numClockTS];
                SecondsValue = new byte[numClockTS];
                MinutesValue = new byte[numClockTS];
                HoursValue = new byte[numClockTS];
                SecondsFlag = new bool[numClockTS];
                MinutesFlag = new bool[numClockTS];
                HoursFlag = new bool[numClockTS];
                TimeOffset = new int[numClockTS];

                for (int index = 0; index < numClockTS; index++)
                {
                    ClockTimestampFlag[index] = bitStream.ReadBoolean();
                    if (ClockTimestampFlag[index])
                    {
                        CtType[index] = (byte)bitStream.ReadBits(2);
                        NuitFieldBasedFlag[index] = bitStream.ReadBoolean();
                        CountingType[index] = (byte)bitStream.ReadBits(5);
                        FullTimestampFlag[index] = bitStream.ReadBoolean();
                        DiscontinuityFlag[index] = bitStream.ReadBoolean();
                        CntDroppedFlag[index] = bitStream.ReadBoolean();
                        NFrames[index] = (byte)bitStream.ReadBits(8);
                        if (FullTimestampFlag[index])
                        {
                            SecondsValue[index] = (byte)bitStream.ReadBits(6);
                            MinutesValue[index] = (byte)bitStream.ReadBits(6);
                            HoursValue[index] = (byte)bitStream.ReadBits(5);
                        }
                        else
                        {
                            SecondsFlag[index] = bitStream.ReadBoolean();
                            if (SecondsFlag[index])
                            {
                                SecondsValue[index] = (byte)bitStream.ReadBits(6);
                                MinutesFlag[index] = bitStream.ReadBoolean();
                                if (MinutesFlag[index])
                                {
                                    MinutesValue[index] = (byte)bitStream.ReadBits(6);
                                    HoursFlag[index] = bitStream.ReadBoolean();
                                    if (HoursFlag[index])
                                    {
                                        HoursValue[index] = (byte)bitStream.ReadBits(5);
                                    }
                                }
                            }
                        }
                        if (m_sps.NalHrdParameters != null && m_sps.NalHrdParameters.TimeOffsetLength > 0)
                        {
                            //FIXME: the specs say i(v) (not mentioned), not u(v). I think one bit should represent the sign
                            TimeOffset[index] = (int)bitStream.ReadBits((int)m_sps.NalHrdParameters.TimeOffsetLength);
                        }
                    }
                }
            }
        }

        public override void WriteBits(RawBitStream bitStream)
        {
            if (m_sps.NalHrdParameters != null || m_sps.VclHrdParameters != null)
            {
                bitStream.WriteBits(CpbRemovalDelay, (int)m_sps.NalHrdParameters.CpbRemovalDelayLengthMinus1 + 1);
                bitStream.WriteBits(DpbOutputDelay, (int)m_sps.NalHrdParameters.DpbOutputDelayLengthMinus1 + 1);
            }

            if (m_sps.VUIParameters != null && m_sps.VUIParameters.PicStructPresentFlag)
            {
                bitStream.WriteBits(PicStruct, 4);
                int numClockTS = GetNumClockTS(PicStruct);
                
                for (int index = 0; index < numClockTS; index++)
                {
                    bitStream.WriteBoolean(ClockTimestampFlag[index]);
                    if (ClockTimestampFlag[index])
                    {
                        bitStream.WriteBits(CtType[index], 2);
                        bitStream.WriteBoolean(NuitFieldBasedFlag[index]);
                        bitStream.WriteBits(CountingType[index], 5);
                        bitStream.WriteBoolean(FullTimestampFlag[index]);
                        bitStream.WriteBoolean(DiscontinuityFlag[index]);
                        bitStream.WriteBoolean(CntDroppedFlag[index]);
                        bitStream.WriteBits(NFrames[index], 8);

                        if (FullTimestampFlag[index])
                        {
                            bitStream.WriteBits(SecondsValue[index], 6);
                            bitStream.WriteBits(MinutesValue[index], 6);
                            bitStream.WriteBits(HoursValue[index], 5);
                        }
                        else
                        {
                            bitStream.WriteBoolean(SecondsFlag[index]);
                            if (SecondsFlag[index])
                            {
                                bitStream.WriteBits(SecondsValue[index], 6);
                                bitStream.WriteBoolean(MinutesFlag[index]);
                                if (MinutesFlag[index])
                                {
                                    bitStream.WriteBits(MinutesValue[index], 6);
                                    bitStream.WriteBoolean(HoursFlag[index]);
                                    if (HoursFlag[index])
                                    {
                                        bitStream.WriteBits(HoursValue[index], 5);
                                    }
                                }
                            }
                        }
                        if (m_sps.NalHrdParameters != null && m_sps.NalHrdParameters.TimeOffsetLength > 0)
                        {
                            //FIXME: the specs say i(v) (not mentioned), not u(v). I think one bit should represent the sign
                            bitStream.WriteBits((uint)TimeOffset[index], (int)m_sps.NalHrdParameters.TimeOffsetLength);
                        }
                    }
                }
            }
        }

        private int GetNumClockTS(uint pic_struct)
        {
            switch (pic_struct)
            {
                case 0:
                case 1:
                case 2:
                    return 1;
                case 3:
                case 4:
                case 7:
                    return 2;
                case 5:
                case 6:
                case 8:
                    return 3;
                default:
                    throw new Exception("pic_struct value is illegal");
            }
        }
    }
}
