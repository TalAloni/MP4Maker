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
    public class H264ElementaryStreamWriter
    {
        public const int StreamBufferSize = 4194304;

        private Stream m_stream;

        private bool m_writtenFirstNalInAccessUnit;
        private NalUnit m_previousCodedSlice;
        private SequenceParameterSetList m_spsList = new SequenceParameterSetList();
        private PictureParameterSetList m_ppsList = new PictureParameterSetList();

        public H264ElementaryStreamWriter(Stream stream)
        {
            m_stream = stream;
        }

        public H264ElementaryStreamWriter(string path, FileMode fileMode, FileAccess access)
        {
            m_stream = new FileStream(path, fileMode, access, FileShare.Read, StreamBufferSize);
        }

        // start code prefix: A unique sequence of three bytes equal to 0x000001 embedded in the byte stream as a prefix
        // to each NAL unit. The location of a start code prefix can be used by a decoder to identify the beginning of a
        // new NAL unit and the end of a previous NAL unit. Emulation of start code prefixes is prevented within NAL
        // units by the inclusion of emulation prevention bytes.
        //
        // When any of the following conditions are true, the zero_byte syntax element shall be present:
        // – the nal_unit_type within the nal_unit( ) is equal to 7 (sequence parameter set) or 8 (picture parameter set).
        // – the byte stream NAL unit syntax structure contains the first NAL unit of an access unit in decoding order.
        //
        // Note: zero_byte is a single byte equal to 0x00.
        public void WriteNalUnit(NalUnit nalUnit)
        {
            if (nalUnit is AccessUnitDelimiter ||
                nalUnit is DependencyRepresentationDelimiter)
            {
                m_stream.WriteByte(0);
                m_writtenFirstNalInAccessUnit = true;
            }
            if (nalUnit is SubsetSequenceParameterSet && !m_writtenFirstNalInAccessUnit)
            {
                // Subset SPS does not get zero_byte prefix unless it's the first NAL in the access unit
                m_stream.WriteByte(0);
                m_spsList.Store((SubsetSequenceParameterSet)nalUnit);
                m_writtenFirstNalInAccessUnit = true;
            }
            else if (nalUnit is SequenceParameterSet)
            {
                m_stream.WriteByte(0);
                m_spsList.Store((SequenceParameterSet)nalUnit);
                m_writtenFirstNalInAccessUnit = true;
            }
            else if (nalUnit is PictureParameterSet)
            {
                m_stream.WriteByte(0);
                m_ppsList.Store((PictureParameterSet)nalUnit);
                m_writtenFirstNalInAccessUnit = true;
            }
            else if (nalUnit is CodedSlice ||
                     nalUnit is CodedSliceExtension)
            {
                if (!m_writtenFirstNalInAccessUnit)
                {
                    bool isFirstVcl = H264ElementaryStreamReader.IsFirstVclInPrimaryCodedPicture(nalUnit, m_previousCodedSlice, m_spsList, m_ppsList);
                    if (isFirstVcl)
                    {
                        m_stream.WriteByte(0);
                    }
                }
                m_previousCodedSlice = nalUnit;
                m_writtenFirstNalInAccessUnit = false;
            }

            m_stream.WriteByte(0);
            m_stream.WriteByte(0);
            m_stream.WriteByte(1);
            nalUnit.WriteBytes(m_stream);
        }

        public void Close()
        {
            m_stream.Close();
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
    }
}
