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
    public class UserDataUnregisteredPayload : SEIPayload
    {
        public byte[] uuidIsoIec11578; // uuid_iso_iec_11578
        public byte[] Data;

        public UserDataUnregisteredPayload() : base(SEIPayloadType.UserDataUnregistered)
        {
        }

        public UserDataUnregisteredPayload(RawBitStream bitStream) : base(SEIPayloadType.UserDataUnregistered)
        {
            ReadBits(bitStream);
        }

        public override void ReadBits(RawBitStream bitStream)
        {
            uuidIsoIec11578 = new byte[16];
            for (int index = 0; index < 16; index++)
            {
                uuidIsoIec11578[index] = bitStream.ReadByte();
            }

            int dataLength = (int)bitStream.Length - 16;
            Data = new byte[dataLength];
            for (int index = 0; index < dataLength; index++)
            {
                Data[index] = bitStream.ReadByte();
            }
        }

        public override void WriteBits(RawBitStream bitStream)
        {
            for (int index = 0; index < 16; index++)
            {
                bitStream.WriteByte(uuidIsoIec11578[index]);
            }

            for (int index = 0; index < Data.Length; index++)
            {
                bitStream.WriteByte(Data[index]);
            }
        }

        public string DataMessage
        {
            get
            {
                string message = Encoding.ASCII.GetString(Data);
                return message;
            }
        }
    }
}

