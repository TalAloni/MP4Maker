/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MediaFormatLibrary.H264;
using Utilities;

namespace MP4Maker
{
    public class H264Helper
    {
        public static void PrintH264Info(H264ElementaryStreamReader h264StreamReader)
        {
            NalUnit nalUnit = h264StreamReader.ReadDecodedNalUnit();
            while (nalUnit != null)
            {
                string description = "NAL unit type: " + nalUnit.NalUnitType.ToString();
                if (nalUnit is AccessUnitDelimiter)
                {
                    description += ", Primary Pic Type: " + ((AccessUnitDelimiter)nalUnit).PrimaryPicType.ToString();
                }
                else if (nalUnit is CodedSlice)
                {
                    description += ", PicOrderCntLsb: " + ((CodedSlice)nalUnit).PicOrderCntLsb;
                    description += ", Frame num: " + ((CodedSlice)nalUnit).FrameNum;
                }
                else if (nalUnit is CodedSliceExtension)
                {
                    description += ", PicOrderCntLsb: " + ((CodedSliceExtension)nalUnit).PicOrderCntLsb;
                    description += ", Frame num: " + ((CodedSliceExtension)nalUnit).FrameNum;
                }
                else if (nalUnit is SequenceParameterSet)
                {
                    description += ", VUIParameters: " + (((SequenceParameterSet)nalUnit).VUIParameters != null);
                    description += ", MaxNumReorderFrames: " + ((SequenceParameterSet)nalUnit).VUIParameters.MaxNumReorderFrames;
                    if (((SequenceParameterSet)nalUnit).VUIParameters != null)
                    {
                        description += ", NalHrdParameters: " + (((SequenceParameterSet)nalUnit).NalHrdParameters != null);
                        description += ", VclHrdParameters: " + (((SequenceParameterSet)nalUnit).VclHrdParameters != null);
                    }
                }
                else if (nalUnit is SEI)
                {
                    List<SEIPayload> payloads = ((SEI)nalUnit).Payloads;
                    foreach (SEIPayload payload in payloads)
                    {
                        description += ", Payload type: " + payload.PayloadType;
                        if (payload is FramePackingArrangement)
                        {
                            description += ", CurrentFrameIsFrame0Flag: " + ((FramePackingArrangement)payload).CurrentFrameIsFrame0Flag;
                            description += ", Repetition Period: " + ((FramePackingArrangement)payload).FramePackingArrangementRepetitionPeriod;
                        }
                    }
                }
                Console.WriteLine(description);
                nalUnit = h264StreamReader.ReadDecodedNalUnit();
            }
        }
    }
}
