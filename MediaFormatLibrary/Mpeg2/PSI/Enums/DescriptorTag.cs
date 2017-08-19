
namespace MediaFormatLibrary.Mpeg2
{
    public enum DescriptorTag : byte
    {
        RegistrationDescriptor = 0x05,
        AVCVideoDescriptor = 0x28,
        PartialTransportStreamDescriptor = 0x63, // ETSI EN 300 468
        AC3AudioDescriptor = 0x81, // See: Digital Audio Compression Standard (AC-3)
        DigitalCopyProtectionDescriptor = 0x88, // DTCP_descriptor, Digital Transmission Content Protection Specification 
    }
}
