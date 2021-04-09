﻿using System;
using System.Runtime.InteropServices;

namespace InspectFileUsingPeCoff.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct IMAGE_NT_HEADER64
    {
        public uint Signature;
        public IMAGE_FILE_HEADER FileHeader;
        public IMAGE_OPTIONAL_HEADER64 OptionalHeader;
    }
}
