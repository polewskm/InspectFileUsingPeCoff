namespace InspectFileUsingPeCoff.Win32
{
    internal static class PeConstants
    {
        public const int IMAGE_DOS_SIGNATURE = 0x5A4D;
        public const int IMAGE_NT_SIGNATURE = 0x00004550;
        public const int IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x010B;
        public const int IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x020B;
        public const int IMAGE_SIZEOF_SHORT_NAME = 8;
        public const int IMAGE_DIRECTORY_ENTRY_EXPORT = 0;
    }
}
