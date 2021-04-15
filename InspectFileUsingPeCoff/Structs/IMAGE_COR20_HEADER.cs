using System;
using System.Runtime.InteropServices;

namespace InspectFileUsingPeCoff.Structs
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct IMAGE_COR20_HEADER
    {
        public uint cb;

        public ushort MajorRuntimeVersion;

        public ushort MinorRuntimeVersion;

        public IMAGE_DATA_DIRECTORY MetaData;

        public ComImageFlags Flags;

        public uint EntryPointTokenOrRVA;

        public IMAGE_DATA_DIRECTORY Resources;

        public IMAGE_DATA_DIRECTORY StrongNameSignature;

        public IMAGE_DATA_DIRECTORY CodeManagerTable;

        public IMAGE_DATA_DIRECTORY VTableFixups;

        public IMAGE_DATA_DIRECTORY ExportAddressTableJumps;

        public IMAGE_DATA_DIRECTORY ManagedNativeHeader;
    }
}
