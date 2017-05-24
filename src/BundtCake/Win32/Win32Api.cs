using System;
using System.Runtime.InteropServices;

namespace BundtCake
{
    static class Win32Api
    {
        [DllImport("Kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
        
        [DllImport("Kernel32.dll", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);
    }
}