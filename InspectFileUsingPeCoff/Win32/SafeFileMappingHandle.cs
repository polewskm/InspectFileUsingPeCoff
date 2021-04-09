using System;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace InspectFileUsingPeCoff.Win32
{
    [SecurityCritical]
    internal sealed class SafeFileMappingHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        [SecurityCritical]
        public SafeFileMappingHandle()
            : base(true)
        {
            // nothing
        }

        [SecurityCritical]
        public SafeFileMappingHandle(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        [SecurityCritical]
        protected override bool ReleaseHandle() =>
            NativeMethods.CloseHandle(handle);
    }
}
