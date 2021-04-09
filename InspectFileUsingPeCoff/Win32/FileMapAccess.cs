using System;

namespace InspectFileUsingPeCoff.Win32
{
    [Flags]
    internal enum FileMapAccess : uint
    {
        FileMapCopy = 0x0001,
        FileMapWrite = 0x0002,
        FileMapRead = 0x0004,
        FileMapAllAccess = 0x001F,
        FileMapExecute = 0x0020,
    }
}
