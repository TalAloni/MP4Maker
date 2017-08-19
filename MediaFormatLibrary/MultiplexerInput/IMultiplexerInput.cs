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
using MediaFormatLibrary.Mpeg2;
using MediaFormatLibrary.MP4;

namespace MediaFormatLibrary
{
    public interface IMultiplexerInput
    {
        SampleData ReadSample();

        List<Descriptor> GetMpeg2Descriptors();

        Mpeg2.StreamType Mpeg2StreamType
        {
            get;
        }

        ElementaryStreamID Mpeg2StreamID
        {
            get;
        }

        byte? Mpeg2StreamIDExtention
        {
            get;
        }

        SampleEntry GetMP4SampleEntry();

        ContentType ContentType
        {
            get;
        }

        Stream BaseStream
        {
            get;
        }
    }
}
