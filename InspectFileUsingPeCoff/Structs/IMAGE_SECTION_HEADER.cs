using System;
using System.Runtime.InteropServices;
using InspectFileUsingPeCoff.Win32;

namespace InspectFileUsingPeCoff.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct IMAGE_SECTION_HEADER
    {
        public fixed byte Name[PeConstants.IMAGE_SIZEOF_SHORT_NAME];

        public uint VirtualSize;

        public uint VirtualAddress;

        public uint SizeOfRawData;

        public uint PointerToRawData;

        public uint PointerToRelocations;

        public uint PointerToLinenumbers;

        public ushort NumberOfRelocations;

        public ushort NumberOfLinenumbers;

        public uint Characteristics;

        public override string ToString()
        {
            fixed (byte* pName = Name)
            {
                return Marshal.PtrToStringAnsi(new IntPtr(pName));
            }
        }

        public uint ToFileOffset(uint relativeVirtualAddress)
        {
            // https://stackoverflow.com/questions/45212489/image-section-headers-virtualaddress-and-pointertorawdata-difference
            return relativeVirtualAddress + PointerToRawData - VirtualAddress;
        }
    }
}
