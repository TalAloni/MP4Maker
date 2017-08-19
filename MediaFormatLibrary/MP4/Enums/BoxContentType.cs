
namespace MediaFormatLibrary.MP4
{
    public enum BoxContentType
    {
        Data,
        Children,

        /// <summary>
        // See: http://atomicparsley.sourceforge.net/mpeg-4files.html
        /// </summary>
        DataAndChildren,
    }
}
