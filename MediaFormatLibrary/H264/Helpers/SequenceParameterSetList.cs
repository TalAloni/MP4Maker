/* Copyright (C) 2014-2015 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;

namespace MediaFormatLibrary.H264
{
    // "A decoder must be capable of simultaneously storing the contents of the sequence parameter sets for all values of seq_parameter_set_id"
    public class SequenceParameterSetList : List<SequenceParameterSet>
    {
        public SequenceParameterSet GetSequenceParameterSet(uint seqParameterSetID)
        {
            int index = IndexOf(this, seqParameterSetID);
            if (index >= 0)
            {
                return this[index];
            }
            return null;
        }

        /// <summary>
        /// The most recently stored SPS will always be the last
        /// </summary>
        public void Store(SequenceParameterSet sps)
        {
            int index = IndexOf(this, sps.SeqParameterSetID);
            if (index >= 0)
            {
                this.RemoveAt(index);
            }
            this.Add(sps);
        }

        public static int IndexOf(List<SequenceParameterSet> spsList, uint seqParameterSetID)
        {
            for (int index = 0; index < spsList.Count; index++)
            {
                if (spsList[index].SeqParameterSetID == seqParameterSetID)
                {
                    return index;
                }
            }
            return -1;
        }
    }
}
