/* Copyright (C) 2026 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System.IO;

namespace MediaFormatLibrary.MP4
{
    public class MovieFragmentBox : Box
    {
        public MovieFragmentBox() : base(BoxType.MovieFragmentBox)
        {
        }

        public MovieFragmentBox(Stream stream) : base(stream)
        {
        }

        public override void ReadData(Stream stream)
        {
            base.ReadData(stream);
        }

        public override void WriteData(Stream stream)
        {
            base.WriteData(stream);
        }

        public override BoxContentType ContentType
        {
            get
            {
                return BoxContentType.DataAndChildren;
            }
        }
    }
}
