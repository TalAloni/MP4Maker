
namespace MediaFormatLibrary.Mpeg2
{
    /// <summary>
    /// [ISO/IEC 13818-1] Program Map Table: stream_type
    /// </summary>
    public enum StreamType : byte
    {
        Reserved = 0x00,
        Mpeg2ADTSAAC = 0x0F,
        H264 = 0x1B,
        MVC1 = 0x20,
        // 0x80-0xFF are defined as user-private, descriptors should be used to find out about the contents of streams.
        DolbyDigital = 0x81,
        DTS = 0x82,
        DolbyTrueHD = 0x83,
        DolbyDigitalPlus = 0x84,
        DTSHDHighResolutionAudio = 0x85,
        DTSHDMasterAudio = 0x86,
        PGS = 0x90,
    }
}
