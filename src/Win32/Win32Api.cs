using System;
using System.Runtime.InteropServices;

namespace BundtCake
{
    static class Win32Api
    {
        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}