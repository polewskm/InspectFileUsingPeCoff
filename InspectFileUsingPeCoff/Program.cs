using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using InspectFileUsingPeCoff.Structs;
using InspectFileUsingPeCoff.Win32;

namespace InspectFileUsingPeCoff
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            const string path1 = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\mscoreei.dll";
            InspectFile(path1);

            const string path2 = @"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\mscoreei.dll";
            InspectFile(path2);
        }

        private static FileInspectionResponse InspectFile(string filePath)
        {
            // http://bytepointer.com/resources/index.htm

            var response = new FileInspectionResponse();

            using var fileStream = File.OpenRead(filePath);

            using var fileMapping = NativeMethods.CreateFileMapping(
                fileStream.SafeFileHandle,
                IntPtr.Zero,
                FileMapProtection.PageReadonly,
                0,
                0,
                null);

            if (fileMapping.IsInvalid)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            var viewOfFile = NativeMethods.MapViewOfFile(
                fileMapping,
                FileMapAccess.FileMapRead,
                0,
                0,
                IntPtr.Zero);

            if (viewOfFile.IsInvalid)
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());

            viewOfFile.Initialize((ulong)fileStream.Length);

            var dosHeader = viewOfFile.Read<IMAGE_DOS_HEADER>(0);
            if (dosHeader.e_magic != PeConstants.IMAGE_DOS_SIGNATURE)
                return response;

            var peHeader = viewOfFile.Read<IMAGE_NT_HEADER>(dosHeader.e_lfanew);
            if (peHeader.Signature != PeConstants.IMAGE_NT_SIGNATURE)
                return response;

            var optionalHeaderOffset = (ulong)(dosHeader.e_lfanew + Marshal.SizeOf<IMAGE_NT_HEADER>());

            var magic = viewOfFile.Read<ushort>(optionalHeaderOffset);
            if (!Enum.IsDefined(typeof(BitnessType), (int)magic))
                return response;

            response.Bitness = (BitnessType)magic;

            ulong optionalHeaderSize;
            uint numberOfRvaAndSizes;

            if (response.Bitness == BitnessType.Bitness32)
            {
                optionalHeaderSize = (ulong)Marshal.SizeOf<IMAGE_OPTIONAL_HEADER32>();
                var optionalHeader = viewOfFile.Read<IMAGE_OPTIONAL_HEADER32>(optionalHeaderOffset);
                numberOfRvaAndSizes = optionalHeader.NumberOfRvaAndSizes;
            }
            else
            {
                optionalHeaderSize = (ulong)Marshal.SizeOf<IMAGE_OPTIONAL_HEADER64>();
                var optionalHeader = viewOfFile.Read<IMAGE_OPTIONAL_HEADER64>(optionalHeaderOffset);
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

            var exportDirectoryOffset = exportSectionHeader.ToRva(exportDataDirectory.VirtualAddress);
            var exportDirectory = viewOfFile.Read<IMAGE_EXPORT_DIRECTORY>(exportDirectoryOffset);

            if (exportDirectory.NumberOfNames == 0)
                return response;

            var hasDllRegisterServer = false;
            var hasDllUnregisterServer = false;

            var nameTableOffset = exportSectionHeader.ToRva(exportDirectory.AddressOfNames);
            for (ulong i = 0; i < exportDirectory.NumberOfNames; ++i)
            {
                const ulong sizeOfInt16 = 4;
                var nameEntryOffset = viewOfFile.Read<uint>(nameTableOffset + i * sizeOfInt16);
                var namePtrOffset = exportSectionHeader.ToRva(nameEntryOffset);
                var namePtr = viewOfFile.DangerousGetHandle() + (int)namePtrOffset;
                var exportName = Marshal.PtrToStringAnsi(namePtr);

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
