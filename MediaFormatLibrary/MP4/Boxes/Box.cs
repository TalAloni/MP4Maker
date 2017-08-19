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
    public class Box
    {
        // structure in stream:
        // size
        // type
        // largesize (present if size == 1)
        public ulong Size;
        private BoxType m_type;
        public List<Box> Children;

        private long m_startOffset;
        private byte[] m_data; // box data, only used for unrecognized boxes

        public Box(BoxType type)
        {
            m_type = type;
            if (ContentType == BoxContentType.Children ||
                ContentType == BoxContentType.DataAndChildren)
            {
                Children = new List<Box>();
            }
        }

        public Box(Stream stream)
        {
            m_startOffset = stream.Position;
            Size = BigEndianReader.ReadUInt32(stream);
            m_type = (BoxType)BigEndianReader.ReadUInt32(stream);
            if (Size == 1)
            {
                Size = BigEndianReader.ReadUInt64(stream);
            }

            ReadData(stream);

            if (ContentType == BoxContentType.Children ||
                ContentType == BoxContentType.DataAndChildren)
            {
                ReadChildren(stream);
            }
        }

        public virtual void ReadData(Stream stream)
        {
            if (this.GetType() == typeof(Box)) // We check if the current class is Box and not a class that inherits from Box
            {
                if (m_type != BoxType.MediaDataBox)
                {
                    int headerLength = (int)(stream.Position - m_startOffset);
                    long bytesToRead = (long)this.Size - headerLength;
                    if (bytesToRead > Int32.MaxValue)
                    {
                        throw new NotImplementedException("Cannot read box larger than 2GB");
                    }
                    m_data = ByteReader.ReadBytes(stream, (int)bytesToRead);
                }
            }
        }

        private void ReadChildren(Stream stream)
        {
            Children = new List<Box>();
            long endOfContainer = m_startOffset + (long)this.Size;
            Box child = BoxHelper.ReadBox(stream, endOfContainer);
            while (child != null)
            {
                Children.Add(child);
                child = BoxHelper.ReadBox(stream, endOfContainer);
            }
        }

        public void WriteBytes(Stream stream)
        {
            m_startOffset = stream.Position;
            BigEndianWriter.WriteUInt32(stream, 0); // We will write the box size later
            BigEndianWriter.WriteUInt32(stream, (uint)m_type);
            WriteData(stream);
            FinalizeWrite(stream);
        }

        public virtual void WriteData(Stream stream)
        {
            if (this.GetType() == typeof(Box)) // We check if the current class is Box and not a class that inherits from Box
            {
                if (m_type == BoxType.MediaDataBox)
                {
                    throw new InvalidOperationException("MediaDataBox cannot use the default WriteBytes() implementation");
                }

                ByteWriter.WriteBytes(stream, m_data);
            }
        }

        private void FinalizeWrite(Stream stream)
        {
            if (ContentType == BoxContentType.Children ||
                ContentType == BoxContentType.DataAndChildren)
            {
                foreach (Box child in Children)
                {
                    child.WriteBytes(stream);
                }
            }

            long endOffset = stream.Position;
            stream.Seek(m_startOffset, SeekOrigin.Begin);
            this.Size = (ulong)(endOffset - m_startOffset);
            if (this.Size > UInt32.MaxValue)
            {
                throw new NotImplementedException("Box size is too big");
            }
            BigEndianWriter.WriteUInt32(stream, (uint)this.Size);
            stream.Seek(endOffset, SeekOrigin.Begin);
        }

        public BoxType Type
        {
            get
            {
                return m_type;
            }
        }

        public virtual BoxContentType ContentType
        {
            get
            {
                return BoxContentType.Data;
            }
        }
    }
}
