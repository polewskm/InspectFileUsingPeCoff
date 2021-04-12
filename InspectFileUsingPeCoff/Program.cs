using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using InspectFileUsingPeCoff.Structs;
using InspectFileUsingPeCoff.Win32;

namespace InspectFileUsingPeCoff
{
    internal static class Program
    {
        private static void Main()
        {
            Console.WriteLine("Hello World!");

            LoadTypeLibs();
            InspectFiles();
        }

        private static void LoadTypeLibs()
        {
            const string path1 = @"C:\Windows\System32\MSCOMCT2.OCX";
            var hr1 = NativeMethods.LoadTypeLib(path1, out var typeLib1);
            var exception1 = Marshal.GetExceptionForHR(hr1);
            if (exception1 != null)
                Console.WriteLine(exception1.ToString());
            if (typeLib1 != null)
                Marshal.FinalReleaseComObject(typeLib1);

            const string path2 = @"C:\Windows\SysWOW64\MSCOMCT2.OCX";
            var hr2 = NativeMethods.LoadTypeLib(path2, out var typeLib2);
            var exception2 = Marshal.GetExceptionForHR(hr2);
            if (exception2 != null)
                Console.WriteLine(exception2.ToString());
            if (typeLib2 != null)
                Marshal.FinalReleaseComObject(typeLib2);
        }

        private static void InspectFiles()
        {
            const string path1 = @"C:\Windows\System32\MSCOMCT2.OCX";
            InspectFile(path1);

            const string path2 = @"C:\Windows\SysWOW64\MSCOMCT2.OCX";
            InspectFile(path2);
        }

        private static FileInspectionResponse InspectFile(string filePath)
        {
            // http://bytepointer.com/resources/index.htm

            var response = new FileInspectionResponse();

            using var fileMapping = MemoryMappedFile.CreateFromFile(filePath, FileMode.Open, null, 0L, MemoryMappedFileAccess.Read);

            using var viewOfFile = fileMapping.CreateViewAccessor(0L, 0L, MemoryMappedFileAccess.Read);

            viewOfFile.Read(0, out IMAGE_DOS_HEADER dosHeader);
            if (dosHeader.e_magic != PeConstants.IMAGE_DOS_SIGNATURE)
                return response;

            viewOfFile.Read(dosHeader.e_lfanew, out IMAGE_NT_HEADER peHeader);
            if (peHeader.Signature != PeConstants.IMAGE_NT_SIGNATURE)
                return response;

            var optionalHeaderOffset = dosHeader.e_lfanew + Marshal.SizeOf<IMAGE_NT_HEADER>();

            var magic = viewOfFile.ReadUInt16(optionalHeaderOffset);
            if (!Enum.IsDefined(typeof(BitnessType), (int)magic))
                return response;

            response.Bitness = (BitnessType)magic;

            int optionalHeaderSize;
            uint numberOfRvaAndSizes;

            if (response.Bitness == BitnessType.Bitness32)
            {
                optionalHeaderSize = Marshal.SizeOf<IMAGE_OPTIONAL_HEADER32>();
                viewOfFile.Read(optionalHeaderOffset, out IMAGE_OPTIONAL_HEADER32 optionalHeader);
                numberOfRvaAndSizes = optionalHeader.NumberOfRvaAndSizes;
            }
            else
            {
                optionalHeaderSize = Marshal.SizeOf<IMAGE_OPTIONAL_HEADER64>();
                viewOfFile.Read(optionalHeaderOffset, out IMAGE_OPTIONAL_HEADER64 optionalHeader);
                numberOfRvaAndSizes = optionalHeader.NumberOfRvaAndSizes;
            }

            var firstDataDirectoryOffset = optionalHeaderOffset + optionalHeaderSize;
            var dataDirectories = new IMAGE_DATA_DIRECTORY[numberOfRvaAndSizes];
            viewOfFile.ReadArray(firstDataDirectoryOffset, dataDirectories, 0, dataDirectories.Length);

            var exportDataDirectory = dataDirectories[PeConstants.IMAGE_DIRECTORY_ENTRY_EXPORT];
            if (exportDataDirectory.VirtualAddress == 0 || exportDataDirectory.Size == 0)
                return response;

            var firstSectionHeaderOffset = optionalHeaderOffset + peHeader.FileHeader.SizeOfOptionalHeader;
            var sectionHeaders = new IMAGE_SECTION_HEADER[peHeader.FileHeader.NumberOfSections];
            viewOfFile.ReadArray(firstSectionHeaderOffset, sectionHeaders, 0, sectionHeaders.Length);

            var exportSectionHeader = sectionHeaders.First(section =>
                exportDataDirectory.VirtualAddress >= section.VirtualAddress &&
                exportDataDirectory.VirtualAddress < section.VirtualAddress + section.VirtualSize);

            var exportDirectoryOffset = exportSectionHeader.ToFileOffset(exportDataDirectory.VirtualAddress);
            viewOfFile.Read(exportDirectoryOffset, out IMAGE_EXPORT_DIRECTORY exportDirectory);

            if (exportDirectory.NumberOfNames == 0)
                return response;

            var hasDllRegisterServer = false;
            var hasDllUnregisterServer = false;

            var nameTableOffset = exportSectionHeader.ToFileOffset(exportDirectory.AddressOfNames);
            for (uint i = 0; i < exportDirectory.NumberOfNames; ++i)
            {
                const uint sizeOfInt32 = 4;
                var exportNameVirtualAddress = viewOfFile.ReadUInt32(nameTableOffset + i * sizeOfInt32);
                var exportNameOffset = exportSectionHeader.ToFileOffset(exportNameVirtualAddress);
                var exportNamePtr = viewOfFile.SafeMemoryMappedViewHandle.DangerousGetHandle() + (int)exportNameOffset;
                var exportName = Marshal.PtrToStringAnsi(exportNamePtr);

                Console.WriteLine(exportName);

                hasDllRegisterServer = hasDllRegisterServer ||
                    string.Equals(
                        exportName,
                        DllExports.DllRegisterServer,
                        StringComparison.OrdinalIgnoreCase);

                hasDllUnregisterServer = hasDllUnregisterServer ||
                    string.Equals(
                        exportName,
                        DllExports.DllUnregisterServer,
                        StringComparison.OrdinalIgnoreCase);

                if (!hasDllRegisterServer || !hasDllUnregisterServer) continue;

                response.HasComServerExports = true;
                return response;
            }

            return response;
        }
    }
}
