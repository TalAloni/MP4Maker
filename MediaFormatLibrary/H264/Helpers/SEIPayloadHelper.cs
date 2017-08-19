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
    public class SEIPayloadHelper
    {
        public static SEIPayload GetSEIPayload(SEIPayloadType payloadType, byte[] payloadData, SequenceParameterSetList spsList)
        {
            RawBitStream payloadStream = new RawBitStream();
            payloadStream.BaseStream.Write(payloadData, 0, payloadData.Length);
            payloadStream.Position = 0;

            switch (payloadType)
            {
                case SEIPayloadType.BufferingPeriod:
                    return new BufferingPeriodPayload(payloadStream, spsList);
                case SEIPayloadType.PicTiming:
                    return new PicTimingPayload(payloadStream, spsList[spsList.Count - 1]);
                case SEIPayloadType.UserDataUnregistered:
                    return new UserDataUnregisteredPayload(payloadStream);
                case SEIPayloadType.FramePackingArrangement:
                    return new FramePackingArrangement(payloadStream);
                default:
                    return new SEIPayload(payloadType, payloadStream);
            }
        }

    }
}
