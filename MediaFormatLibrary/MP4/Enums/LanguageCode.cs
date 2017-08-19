
namespace MediaFormatLibrary.MP4
{
    /// <summary>
    /// 2 byte encoding of ISO 639 3-letters language code
    /// </summary>
    public enum LanguageCode : ushort
    {
        Undetermined = 0x55C4, // 'und'
        English = 0x15C7, // 'eng'
        Japanese = 0x2A0E, // 'jpn'
    }
}
