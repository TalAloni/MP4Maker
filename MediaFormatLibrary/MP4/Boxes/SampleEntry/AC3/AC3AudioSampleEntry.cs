using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utilities;

namespace MediaFormatLibrary.MP4
{
    public class AC3AudioSampleEntry : AudioSampleEntry
    {
        public AC3AudioSampleEntry() : base(BoxType.AC3AudioSampleEntry)
        {
        }

        public AC3AudioSampleEntry(Stream stream) : base(stream)
        {
        }

        public override BoxContentType ContentType
        {
            get
            {
                return BoxContentType.Children;
            }
        }
    }
}
