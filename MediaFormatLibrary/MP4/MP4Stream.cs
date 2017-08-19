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

namespace MediaFormatLibrary.MP4
{
    public class MP4Stream
    {
        public const int StreamBufferSize = 4194304;

        private Stream m_stream;

        public MP4Stream(Stream stream)
        {
            m_stream = stream;
        }

        public List<Box> ReadRootBoxes()
        {
            List<Box> rootBoxes = new List<Box>();

            Box box = ReadBox();
            while (box != null)
            {
                rootBoxes.Add(box);
                box = ReadBox();
            }

            return rootBoxes;
        }

        public Box ReadBox()
        {
            return BoxHelper.ReadBox(m_stream);
        }

        public Stream BaseStream
        {
            get
            {
                return m_stream;
            }
        }

        public long Position
        {
            get
            {
                return m_stream.Position;
            }
            set
            {
                m_stream.Position = value;
            }
        }

        public long Length
        {
            get
            {
                return m_stream.Length;
            }
        }

        public static MP4Stream Open(string path, FileMode fileMode, FileAccess access)
        {
            FileStream stream = new FileStream(path, fileMode, access, FileShare.Read, StreamBufferSize);
            return new MP4Stream(stream);
        }
    }
}
