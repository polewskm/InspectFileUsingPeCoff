using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace InspectFileUsingPeCoff.Win32
{
    [SecurityCritical]
    internal static class NativeMethods
    {
        [DllImport(DllImports.Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(
            [In] IntPtr handle);

        [DllImport(DllImports.Kernel32, SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern SafeFileMappingHandle CreateFileMapping(
            [In] SafeFileHandle hFile,
            [In] IntPtr lpFileMappingAttributes,
            [In] FileMapProtection flProtect,
            [In] int dwMaximumSizeHigh,
            [In] int dwMaximumSizeLow,
            [In] string lpName);

        [DllImport(DllImports.Kernel32, SetLastError = true)]
        public static extern SafeViewOfFileBuffer MapViewOfFile(
            [In] SafeFileMappingHandle hFileMapping,
            [In] FileMapAccess dwDesiredAccess,
            [In] int dwFileOffsetHigh,
            [In] int dwFileOffsetLow,
            [In] IntPtr dwNumberOfBytesToMap);

        [DllImport(DllImports.Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnmapViewOfFile(
            [In] IntPtr lpBaseAddress);
    }
}
