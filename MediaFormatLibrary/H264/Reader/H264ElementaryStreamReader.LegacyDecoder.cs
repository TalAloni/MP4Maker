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
    public partial class H264ElementaryStreamReader
    {
        private H264AccessUnit m_pendingAccessUnit;

        /// <summary>
        /// Coded Video Sequence - A sequence of access units that consists, in decoding order, of an IDR access unit
        /// followed by zero or more non-IDR access units including all subsequent access units up to but not including
        /// any subsequent IDR access unit.
        /// </summary>
        public List<H264AccessUnit> ReadCodedVideoSequence()
        {
            List<H264AccessUnit> buffer = new List<H264AccessUnit>();
            if (m_pendingAccessUnit != null)
            {
                buffer.Add(m_pendingAccessUnit);
                m_pendingAccessUnit = null;
            }

            // Read all pictures until the next IDR or EOF
            while (true)
            {
                H264AccessUnit accessUnit = ReadAccessUnit();
                if (accessUnit == null)
                {
                    break;
                }
                if (buffer.Count > 0 && accessUnit.IsIDRPicture)
                {
                    m_pendingAccessUnit = accessUnit;
                    break;
                }
                buffer.Add(accessUnit);
            }

            if (buffer.Count > 0)
            {
                return buffer;
            }
            else
            {
                return null;
            }
        }
    }
}
