using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;

namespace InspectFileUsingPeCoff.Win32
{
    [SecurityCritical]
    internal static class NativeMethods
    {
        [DllImport(DllImports.OleAut32, CharSet = CharSet.Unicode)]
        public static extern int LoadTypeLib(
            [In] string file,
            [Out] out ITypeLib? typeLib);
    }
}
