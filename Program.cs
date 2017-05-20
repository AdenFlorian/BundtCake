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
        [DllImport("Kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        static MyLogger _logger = new MyLogger(nameof(Program));

        static int Main(string[] args)
        {
            VulkanPrinter.PrintAvailableLayers();
            VulkanPrinter.PrintAvailableInstanceExtensions();

            var windowPtr = SDL.SDL_CreateWindow("help, im stuck in a title bar factory", 100, 100, 500, 500, 0);

            var requiredExtensionNames = new string[] { "VK_EXT_debug_report", "VK_KHR_win32_surface" };

            var instance = new Instance(new InstanceCreateInfo
            {
                EnabledLayerNames = new string[] { "VK_LAYER_LUNARG_standard_validation" },
                EnabledExtensionNames = requiredExtensionNames
            });

            SetupDebugCallback(instance);

            var info = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetWindowWMInfo(windowPtr, ref info);

            var hinstance = GetModuleHandle(null);

            var surface = instance.CreateWin32SurfaceKHR(new Win32SurfaceCreateInfoKhr{
                Hinstance = hinstance,
                Hwnd = info.info.win.window
            });

            var physicalDevices = instance.EnumeratePhysicalDevices();

            _logger.LogInfo("Physical devices:");

            foreach (var physicalDevice in physicalDevices)
            {
                _logger.LogInfo("\t" + physicalDevice.GetProperties().DeviceName);
            }

            var selectedPhysicalDevice = physicalDevices[0];

            VulkanPrinter.PrintAvailablePhysicalDeviceExtensions(selectedPhysicalDevice);
            VulkanPrinter.PrintAvailablePhysicalDeviceFeatures(selectedPhysicalDevice);

            var queueFamilyProperties = selectedPhysicalDevice.GetQueueFamilyProperties();

            // selectedPhysicalDevice.CreateDevice(new DeviceCreateInfo{
            //     QueueCreateInfos = new DeviceQueueCreateInfo{
            //         QueueFamilyIndex = ,
            //         QueueCount = ,
            //         QueuePriorities = 
            //     },
            //     EnabledExtensionNames = requiredExtensionNames,
            //     EnabledFeatures = 
            // });




            // SDL.SDL_Event sdlEvent;

            // while (true)
            // {
            //     SDL.SDL_PollEvent(out sdlEvent);

            //     if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
            //     {
            //         break;
            //     }
            // }

            return 0;
        }

        static void SetupDebugCallback(Instance instance)
        {
            instance.EnableDebug((DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData) =>
            {
                _logger.LogInfo("Valdiation layer " + flags + ": " + Marshal.PtrToStringUTF8(message));
                return false;
            }, DebugReportFlagsExt.Error | DebugReportFlagsExt.Warning | DebugReportFlagsExt.PerformanceWarning/* | DebugReportFlagsExt.Information/* | DebugReportFlagsExt.Debug*/);
        }
    }
}
