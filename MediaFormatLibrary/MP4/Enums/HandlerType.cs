
namespace MediaFormatLibrary.MP4
{
    public enum HandlerType : uint
    {
        Video = 0x76696465, // 'vide'
        Audio = 0x736F756E, // 'soun'
        Hint = 0x68696E74, // 'hint'

        /// <summary>
        /// Timed Metadata track
        /// </summary>
        Metadata = 0x6D657461, // 'meta'

        AuxiliaryVideo = 0x61757876, // 'auxv'
        
        /// <summary>
        /// The value ‘null’ can be used in the primary meta box to
        /// indicate that it is merely being used to hold resources.
        /// </summary>
        Null = 0x6E756C6C, // 'null'

        ODSM = 0x6F64736D, // 'odsm', scene description
        QuickTimeMetaData = 0x6D646972, // 'mdir'
        QuickTimeMetaDataTags = 0x6D647461, // 'mdta', part of [QuickTime File Format Specification]
    }
}
