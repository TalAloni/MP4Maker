
namespace MediaFormatLibrary.Mpeg2
{
    /// <summary>
    /// copy_permission_indicator, 2 bits.
    /// </summary>
    public enum CopyPermissionIndicator : byte
    {
        CopyFree = 0x00,
        NoMoreCopy = 0x01,
        CopyOnce = 0x02,
        CopyProhibited = 0x03,
    }
}
