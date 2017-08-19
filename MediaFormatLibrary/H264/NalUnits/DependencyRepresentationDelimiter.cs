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
    /// <summary>
    /// Blu-ray specific NAL that is not part of the H.264 standard.
    /// MVC bitstream DRD is used to separate base view component and dependent
    /// view component within single access unit.
    /// Blu-ray spec requires every MVC access unit to start with AUD.
    /// Every MVC AU contains base view component and dependent view component.
    /// Coded representation of dependent view component within AU should be started with DRD.
    /// </summary>
    public class DependencyRepresentationDelimiter : NalUnit
    {
        public DependencyRepresentationDelimiter() : base(NalUnitType.DependencyRepresentationDelimiter)
        {
        }

        public DependencyRepresentationDelimiter(MemoryStream stream) : base(stream)
        {
            ReadEncodedPayloadBytes(stream);
        }

        public override void ReadDecodedPayloadBytes(RawBitStream bitStream)
        {
        }

        public override void WriteRawPayloadBytes(RawBitStream bitStream)
        {
            bitStream.WriteRbspTrailingBits();
        }
    }
}
