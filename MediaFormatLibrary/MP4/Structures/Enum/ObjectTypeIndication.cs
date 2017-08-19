
namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// [ISO/IEC 14496-1] objectTypeIndication
    /// </summary>
    public enum ObjectTypeIndication : byte
    {
        ITUH264 = 0x21, // Audio ISO/IEC 14496-3
        Mpeg4AAC = 0x40, // Audio ISO/IEC 14496-3
        Mpeg2AACMain = 0x66, // Audio ISO/IEC 13818-7 Main Profile
        Mpeg2AACLC = 0x67,   // Audio ISO/IEC 13818-7 Low Complexity Profile
        Mpeg2AACSSR = 0x68,  // Audio ISO/IEC 13818-7 Scaleable Sampling Rate Profile
    }
}
