using System;
using System.Runtime.InteropServices;

namespace InspectFileUsingPeCoff.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct IMAGE_DATA_DIRECTORY
    {
        public uint VirtualAddress;
        public uint Size;
    }
}
