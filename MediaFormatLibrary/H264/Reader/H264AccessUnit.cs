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

namespace MediaFormatLibrary.H264
{
    public class H264AccessUnit : List<MemoryStream>
    {
        public bool IsIDRPicture;
        public uint PicOrderCntLsb;
        public uint PicOrderCount; // calculated from PicOrderCntLsb
        public int DecodingOrder; // this is the decoding order
        public int? DisplayOrder; // this is the display order

        /// <summary>
        /// DisplayOrder - DecodingOrder
        /// </summary>
        public int? Delay
        {
            get
            {
                if (DisplayOrder.HasValue)
                {
                    return DisplayOrder.Value - DecodingOrder;
                }
                return null;
            }
        }
    }
}
