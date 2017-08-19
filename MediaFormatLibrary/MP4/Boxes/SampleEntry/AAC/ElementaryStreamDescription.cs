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
using Utilities;

namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// See: ISO/IEC 14496-14
    /// </summary>
    public class ElementaryStreamDescriptorBox : FullBox
    {
        public ESDescriptor ESDescriptor;

        public ElementaryStreamDescriptorBox() : base(BoxType.ElementaryStreamDescriptorBox)
        {
            ESDescriptor = new ESDescriptor();
        }

        public ElementaryStreamDescriptorBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
            ESDescriptor = new ESDescriptor(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
            ESDescriptor.WriteBytes(stream);
        }

        public AudioObjectType AudioObjetcType
        {
            get
            {
                DecoderConfigDescriptor decoderConfig = ESDescriptor.DecoderConfigDescriptor;
                DecoderSpecificInfo info = decoderConfig.DecSpecificInfo.Count > 0 ? decoderConfig.DecSpecificInfo[0] : null;
                if (info is AudioSpecificConfig)
                {
                    return ((AudioSpecificConfig)info).AudioObjectType;
                }
                return AudioObjectType.Null;
            }
        }

        public uint AvgBitrate
        {
            get
            {
                return ESDescriptor.DecoderConfigDescriptor.AvgBitRate;
            }
        }

        public uint MaxBitrate
        {
            get
            {
                return ESDescriptor.DecoderConfigDescriptor.MaxBitRate;
            }
        }
    }
}
