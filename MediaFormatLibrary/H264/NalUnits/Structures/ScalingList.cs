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

namespace MediaFormatLibrary.H264
{
    public class ScalingList
    {
        private int[] m_scalingList;

        public ScalingList(RawBitStream bitStream, int sizeOfScalingList)
        {
            m_scalingList = new int[sizeOfScalingList];

            int lastScale = 8;
            int nextScale = 8;
            for (int j = 0; j < sizeOfScalingList; j++)
            {
                if (nextScale != 0)
                {
                    int deltaScale = bitStream.ReadExpGolombCodeSigned();
                    nextScale = (lastScale + deltaScale + 256) % 256;
                }
                m_scalingList[j] = (nextScale == 0) ? lastScale : nextScale;
                lastScale = m_scalingList[j];
            }
        }

        public void WriteBits(RawBitStream bitStream)
        {
            int lastScale = 8;
            int nextScale = 8;
            for (int j = 0; j < m_scalingList.Length; j++)
            {
                if (nextScale != 0)
                {
                    // The value of delta_scale shall be in the range of -128 to +127, inclusive.
                    int deltaScale = m_scalingList[j] - lastScale;
                    bitStream.WriteExpGolombCodeSigned(deltaScale);
                }
                lastScale = m_scalingList[j];
            }
        }
    }
}
