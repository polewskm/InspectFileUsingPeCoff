using InspectFileUsingPeCoff.Win32;

namespace InspectFileUsingPeCoff
{
    public enum BitnessType
    {
        Unknown = 0,
        Bitness32 = PeConstants.IMAGE_NT_OPTIONAL_HDR32_MAGIC,
        Bitness64 = PeConstants.IMAGE_NT_OPTIONAL_HDR64_MAGIC
    }
}
