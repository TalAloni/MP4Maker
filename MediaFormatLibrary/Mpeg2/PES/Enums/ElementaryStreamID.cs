
namespace MediaFormatLibrary.Mpeg2
{
    /// <summary>
    /// [ISO/IEC 13818-1] stream_id
    /// </summary>
    public enum ElementaryStreamID : byte
    {
        ProgramStreamMap = 0xBC,
        PrivateStream1 = 0xBD,
        PaddingStream = 0xBE,
        PrivateStream2 = 0xBF,
        Mpeg2AudioStream1 = 0xC0, // MPEG-1 or MPEG-2 audio stream number 1
        H264 = 0xE0,
        ECMStream = 0xF0,
        EMMStream = 0xF1,
        DSMCCStream = 0xF2,
        H222TypeE = 0xF8,
        AncillaryStream = 0xF9,
        Reserved1 = 0xFC,
        ExtendedStream = 0xFD,
        Reserved3 = 0xFE,
        ProgramStreamDirectory = 0xFF,
    }
}
