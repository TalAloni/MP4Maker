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

namespace MediaFormatLibrary
{
    public class SampleData
    {
        public byte[] RawSampleData;
        public uint DurationInTimeUnits;
        public double DurationInSeconds;
        public bool IsSyncSample;
        public int SampleDelayInTimeUnits;
        public double SampleDelayInSeconds;
    }
}
