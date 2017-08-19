using System;

namespace MediaFormatLibrary.MP4
{
    public enum FileBrand : uint
    {
        MSNV = 0x4D534E56, // 'MSNV'
        mp42 = 0x6D703432, // 'mp42'
        isom = 0x69736F6D, // 'isom'
    }
}
