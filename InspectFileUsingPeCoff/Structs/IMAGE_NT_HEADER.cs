using System;
using System.Runtime.InteropServices;

namespace InspectFileUsingPeCoff.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct IMAGE_NT_HEADER
    {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
    }
}
