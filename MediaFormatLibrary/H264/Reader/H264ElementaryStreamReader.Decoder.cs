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
    /// <summary>
    /// This partial class will calculate access unit display order using a hypothetical decoder.
    /// </summary>
    public partial class H264ElementaryStreamReader
    {
        private int m_nextDisplayOrder;
        private int m_decodedUnitsInBuffer;
        private H264AccessUnit m_pendingIDR;
        private List<H264AccessUnit> m_decoderBuffer = new List<H264AccessUnit>();
        private SequenceParameterSet m_activeSPS;
        private uint m_prevPicOrderCntMsb;
        private uint m_prevPicOrderCntLsb;

        // To get the pictures in display order:
        // Keep a buffer of size=(num_reorder_frames+1).
        // Put each newly decoded frame into the buffer.
        // Whenever the buffer is full, remove the frame with the lowest {idr_pic_id,poc}
        // (where idr_pic_id is a monotonically increasing value, not literally the variable idr_pic_id from the standard).
        public H264AccessUnit ReadAccessUnitWithTimingInformation()
        {
            if (m_pendingIDR == null)
            {
                // Note: We can only determine the display order of 1 frame when the buffer is of size=(numReorderFrames + 1)
                while (m_decoderBuffer.Count == 0 || m_decoderBuffer[0].DisplayOrder == null)
                {
                    H264AccessUnit accessUnit = ReadAccessUnit();
                    
                    if (accessUnit == null)
                    {
                        while (m_decodedUnitsInBuffer < m_decoderBuffer.Count)
                        {
                            MarkNextAccessUnitToBeDecoded();
                        }
                        break;
                    }
                    else if (accessUnit.IsIDRPicture && m_decoderBuffer.Count > 0)
                    {
                        m_pendingIDR = accessUnit;
                        while (m_decodedUnitsInBuffer < m_decoderBuffer.Count)
                        {
                            MarkNextAccessUnitToBeDecoded();
                        }
                        break;
                    }
                    AddToDecoderBuffer(accessUnit);

                    uint numReorderFrames = m_activeSPS.NumReorderFrames;
                    if ((m_decoderBuffer.Count - m_decodedUnitsInBuffer) >= numReorderFrames + 1)
                    {
                        MarkNextAccessUnitToBeDecoded();
                    }
                }
            }

            H264AccessUnit result = RemoveNextFromBuffer();
            if (m_decoderBuffer.Count == 0 && m_pendingIDR != null)
            {
                AddToDecoderBuffer(m_pendingIDR);
                m_pendingIDR = null;
            }
            return result;
        }

        private void AddToDecoderBuffer(H264AccessUnit accessUnit)
        {
            m_decoderBuffer.Add(accessUnit);
            if (accessUnit.IsIDRPicture)
            {
                // Note: Sequence parameter set applies to zero or more *entire* coded video sequences.
                // FIXME: We should use seq_parameter_set_id instead of using the latest SPS.
                m_activeSPS = m_spsList[m_spsList.Count - 1];
            }
            CalculatePicOrderCount(accessUnit);
        }

        private void CalculatePicOrderCount(H264AccessUnit accessUnit)
        {
            if (m_activeSPS.PicOrderCntType == 0)
            {
                if (accessUnit.IsIDRPicture)
                {
                    m_prevPicOrderCntMsb = 0;
                    m_prevPicOrderCntLsb = 0;
                }

                uint maxPicOrderCntLsb = m_activeSPS.MaxPicOrderCntLsb;
                uint picOrderCountLsb = accessUnit.PicOrderCntLsb;
                
                uint picOrderCntMsb;
                if (picOrderCountLsb < m_prevPicOrderCntLsb &&
                    (m_prevPicOrderCntLsb - picOrderCountLsb) >= (maxPicOrderCntLsb / 2))
                {
                    picOrderCntMsb = m_prevPicOrderCntMsb + maxPicOrderCntLsb;
                }
                else if (picOrderCountLsb > m_prevPicOrderCntLsb &&
                    (picOrderCountLsb - m_prevPicOrderCntLsb) > (maxPicOrderCntLsb / 2))
                {
                    picOrderCntMsb = m_prevPicOrderCntMsb - maxPicOrderCntLsb;
                }
                else
                {
                    picOrderCntMsb = m_prevPicOrderCntMsb;
                }

                accessUnit.PicOrderCount = picOrderCntMsb + picOrderCountLsb;
                m_prevPicOrderCntMsb = picOrderCntMsb;
                m_prevPicOrderCntLsb = accessUnit.PicOrderCntLsb;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private H264AccessUnit RemoveNextFromBuffer()
        {
            if (m_decoderBuffer.Count > 0)
            {
                H264AccessUnit result = m_decoderBuffer[0];
                m_decoderBuffer.RemoveAt(0);
                m_decodedUnitsInBuffer--;
                return result;
            }
            return null;
        }

        private void MarkNextAccessUnitToBeDecoded()
        {
            uint minPicOrderCount = UInt32.MaxValue;
            int minPicIndex = -1;
            for (int index = 0; index < m_decoderBuffer.Count; index++)
            {
                if (m_decoderBuffer[index].DisplayOrder == null)
                {
                    if (m_decoderBuffer[index].PicOrderCount < minPicOrderCount)
                    {
                        minPicOrderCount = m_decoderBuffer[index].PicOrderCount;
                        minPicIndex = index;
                    }
                }
            }
            m_decoderBuffer[minPicIndex].DisplayOrder = m_nextDisplayOrder;
            m_nextDisplayOrder++;
            m_decodedUnitsInBuffer++;
        }
    }
}
