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
    // "A decoder must be capable of simultaneously storing the contents of the picture parameter sets for all values of pic_parameter_set_id".
    public class PictureParameterSetList : List<PictureParameterSet>
    {
        public PictureParameterSet GetPictureParameterSet(uint picParameterSetID)
        {
            int index = IndexOf(this, picParameterSetID);
            if (index >= 0)
            {
                return this[index];
            }
            return null;
        }

        /// <summary>
        /// The most recently stored PPS will always be the last
        /// </summary>
        public void Store(PictureParameterSet pps)
        {
            int index = IndexOf(this, pps.PicParameterSetID);
            if (index >= 0)
            {
                this.RemoveAt(index);
            }
            this.Add(pps);
        }

        public static int IndexOf(List<PictureParameterSet> ppsList, uint picParameterSetID)
        {
            for (int index = 0; index < ppsList.Count; index++)
            {
                if (ppsList[index].PicParameterSetID == picParameterSetID)
                {
                    return index;
                }
            }
            return -1;
        }
    }
}
