
namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// [ISO/IEC 14496-3] MPEG-4 audio object types
    /// </summary>
    public enum AudioObjectType : byte // 5 bits
    {
        Null = 0,
        AACMain = 1,
        AACLC = 2,
        AACSSR = 3,
        AACLTP = 4,
        SBR = 5,
        AACScalable = 6,
        TwinVQ = 7,
        CELP = 8,
        HVXC = 9,
        // More
    }
}
