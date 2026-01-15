/* Copyright (C) 2026 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    public class SegmentIndexBox : FullBox
    {
        public uint ReferenceID;
        public uint TimeScale;
        public ulong EarliestPresentationTime; // UInt32 for version 0, UInt64 for version 1
        public ulong FirstOffet; // UInt32 for version 0, UInt64 for version 1
        public ushort Reserved;
        public List<SegmentReference> References = new List<SegmentReference>();

        public SegmentIndexBox() : base(BoxType.SegmentIndexBox)
        { }

        public SegmentIndexBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            ReferenceID = BigEndianReader.ReadUInt32(stream);
            TimeScale = BigEndianReader.ReadUInt32(stream);
            if (Version == 0)
            {
                EarliestPresentationTime = BigEndianReader.ReadUInt32(stream);
                FirstOffet = BigEndianReader.ReadUInt32(stream);
            }
            else
            {
                EarliestPresentationTime = BigEndianReader.ReadUInt64(stream);
                FirstOffet = BigEndianReader.ReadUInt64(stream);
            }
            Reserved = BigEndianReader.ReadUInt16(stream);
            ushort referenceCount = BigEndianReader.ReadUInt16(stream);
            for (int index = 0; index < referenceCount; index++)
            {
                References.Add(new SegmentReference(stream));
            }
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            throw new NotImplementedException();
        }

        public override BoxContentType ContentType
        {
            get
            {
                return BoxContentType.Data;
            }
        }
    }

    public class SegmentReference
    {
        public bool ReferenceType;
        public uint ReferenceSize; // 31 bits
        public uint SubsegmentDuration;
        public bool StartsWithSap;
        public byte SapType; // 3 bits
        public uint SapDeltaTime; // 28 bits

        public SegmentReference()
        {
        }

        public SegmentReference(Stream stream)
        {
            uint temp = BigEndianReader.ReadUInt32(stream);
            ReferenceType = (temp & 0x80000000) > 0;
            ReferenceSize = (uint)(temp & 0x7FFFFFFF);
            SubsegmentDuration = BigEndianReader.ReadUInt32(stream);
            temp = BigEndianReader.ReadUInt32(stream);
            StartsWithSap = (temp & 0x80000000) > 0;
            SapType = (byte)((temp & 0x70000000) >> 28);
            SapDeltaTime = (uint)(temp & 0x0FFFFFFF);
        }
    }
}
