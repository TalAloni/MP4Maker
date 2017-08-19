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

namespace MediaFormatLibrary.MP4
{
    /*
    class SLConfigDescriptor extends BaseDescriptor : bit(8) tag=SLConfigDescrTag {
    bit(8) predefined;
    if (predefined==0) {
    bit(1) useAccessUnitStartFlag;
    bit(1) useAccessUnitEndFlag;
    bit(1) useRandomAccessPointFlag;
    bit(1) hasRandomAccessUnitsOnlyFlag;
    bit(1) usePaddingFlag;
    bit(1) useTimeStampsFlag;
    bit(1) useIdleFlag;
    bit(1) durationFlag;
    bit(32) timeStampResolution;
    bit(32) OCRResolution;
    bit(8) timeStampLength; // must be <= 64
    bit(8) OCRLength; // must be <= 64
    bit(8) AU_Length; // must be <= 32
    bit(8) instantBitrateLength;
    bit(4) degradationPriorityLength;
    bit(5) AU_seqNumLength; // must be <= 16
    bit(5) packetSeqNumLength; // must be <= 16
    bit(2) reserved=0b11;
    }
    if (durationFlag) {
    bit(32) timeScale;
    bit(16) accessUnitDuration;
    bit(16) compositionUnitDuration;
    }
    if (!useTimeStampsFlag) {
    bit(timeStampLength) startDecodingTimeStamp;
    bit(timeStampLength) startCompositionTimeStamp;
    }
    }
    */
    /// <summary>
    /// [ISO/IEC 14496-1] SLConfigDescriptor
    /// </summary>
    public class SLConfigDescriptor
    {
        public const byte DescriptorType = 0x06;

        public byte[] SLValue; // Set to 0x02, see ISO/IEC 14496-14

        public SLConfigDescriptor()
        {
        }

        public SLConfigDescriptor(Stream stream)
        {
            byte descriptorType = (byte)stream.ReadByte();
            if (descriptorType != DescriptorType)
            {
                throw new Exception("Invalid descriptor type");
            }
            int length = ESDescriptor.ReadLength(stream);
            SLValue = ByteReader.ReadBytes(stream, length);
        }

        public void WriteBytes(Stream stream)
        {
            stream.WriteByte(DescriptorType);
            ESDescriptor.WriteLength(stream, 1);
            ByteWriter.WriteBytes(stream, SLValue);
        }

        public int Length
        {
            get
            {
                return 2 + SLValue.Length;
            }
        }
    }
}
