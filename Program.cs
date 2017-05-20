using System;
using Vulkan;
using Vulkan.Windows;
using SDL2;
using System.Threading;
using System.Runtime.InteropServices;
using BundtCommon;

namespace BundtCake
{
    class Program
    {
        static MyLogger _logger = new MyLogger(nameof(Program));

        static int Main(string[] args)
        {
            var vulkan = new Vulkan();

            vulkan.Start();

            return 0;
        }
    }
}
