using System;
using System.Security;

namespace Inflectra.SpiraTest.Installer.UI.UnsafeNativeMethods
{
    [SuppressUnmanagedCodeSecurity]
    internal delegate Int32 BrowseCallbackProc(IntPtr hwnd, Int32 msg, IntPtr lParam, IntPtr lpData);
}
