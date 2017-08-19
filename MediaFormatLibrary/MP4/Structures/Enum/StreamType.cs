
namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// [ISO/IEC 14496-1] streamType
    /// </summary>
    public enum StreamType : byte // 6 bits
    {
        VisualStream = 0x04,
        AudioStream = 0x05,
    }
}
