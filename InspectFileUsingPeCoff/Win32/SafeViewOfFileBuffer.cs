using System;
using System.Runtime.InteropServices;
using System.Security;

namespace InspectFileUsingPeCoff.Win32
{
    internal sealed class SafeViewOfFileBuffer : SafeBuffer
    {
        [SecurityCritical]
        public SafeViewOfFileBuffer()
            : base(true)
        {
            // nothing
        }

        public SafeViewOfFileBuffer(IntPtr handle, bool ownsHandle)
            : base(ownsHandle)
        {
            SetHandle(handle);
        }

        protected override bool ReleaseHandle() =>
            NativeMethods.UnmapViewOfFile(handle);
    }
}
