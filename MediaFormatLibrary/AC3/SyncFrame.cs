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

namespace MediaFormatLibrary.AC3
{
    public class SyncFrame
    {
        // [ATSC Digital Audio Compression (AC-3) Standard]
        // "Each synchronization frame contains 6 coded audio blocks (AB), each of which represent 256 new audio samples per channel"
        public const uint SampleCount = 1536;

        public SyncInfo SyncInfo;
        public byte[] RawData;

        public BSI BSI; // Read only

        public SyncFrame()
        {
        }

        public SyncFrame(Stream stream)
        {
            BitStream bitStream = new BitStream(stream, true);
            SyncInfo = new SyncInfo(bitStream);
            // We are now byte aligned, we can use stream
            RawData = ByteReader.ReadBytes(stream, SyncInfo.FrameSize - SyncInfo.Length);

            BitStream bsiBitStream = new BitStream(new MemoryStream(RawData), true);
            BSI = new BSI(bsiBitStream);
        }

        public void WriteBytes(Stream stream)
        {
            BitStream bitStream = new BitStream(stream, true);
            SyncInfo.WriteBytes(bitStream);
            // We are now byte aligned, we can use stream
            ByteWriter.WriteBytes(stream, RawData);
        }

        public byte[] GetBytes()
        {
            MemoryStream stream = new MemoryStream();
            WriteBytes(stream);
            return stream.ToArray();
        }
    }
}
