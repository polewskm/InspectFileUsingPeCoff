using System;

namespace InspectFileUsingPeCoff
{
    [Flags]
    internal enum ComImageFlags : uint
    {
        None = 0x00000000,
        IlOnly = 0x00000001,
        Require32Bit = 0x00000002,
        IlLibrary = 0x00000004,
        StrongNameSigned = 0x00000008,
        NativeEntryPoint = 0x00000010,
        TrackDebugData = 0x00010000,
        Prefer32Bit = 0x00020000
    }
}
