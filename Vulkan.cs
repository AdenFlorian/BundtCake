using System;
using System.Runtime.InteropServices;
using BundtCommon;
using SDL2;
using Vulkan;
using Vulkan.Windows;

namespace BundtCake
{
    class Vulkan
    {
        static readonly string[] _requiredInstanceExtensions = new string[] { "VK_EXT_debug_report", "VK_KHR_win32_surface" };
        static readonly string[] _requiredDeviceExtensions = new string[] { "VK_KHR_swapchain" };

        readonly MyLogger _logger = new MyLogger(nameof(Vulkan));

        public void Start()
        {
            VulkanPrinter.PrintAvailableInstanceLayers();
            VulkanPrinter.PrintAvailableInstanceExtensions();

            var window = CreateWindow();

            var instance = CreateInstance();

            SetupDebugCallback(instance);

            var surface = CreateSurface(instance, window);

            var physicalDevices = GetPhysicalDevices(instance);

            var physicalDevice = ChoosePhysicalDevice(physicalDevices);

            var queueFamilyProperties = physicalDevice.GetQueueFamilyProperties();

            var device = CreateLogicalDevice(physicalDevice);
            




            // SDL.SDL_Event sdlEvent;

            // while (true)
            // {
            //     SDL.SDL_PollEvent(out sdlEvent);

            //     if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
            //     {
            //         break;
            //     }
            // }
        }

        Device CreateLogicalDevice(PhysicalDevice physicalDevice)
        {
            var requiredPhysicalDeviceFeatures = new PhysicalDeviceFeatures();
            requiredPhysicalDeviceFeatures.SamplerAnisotropy = true;

            return physicalDevice.CreateDevice(new DeviceCreateInfo
            {
                QueueCreateInfos = new DeviceQueueCreateInfo[]
                {
                    new DeviceQueueCreateInfo
                    {
                        QueueFamilyIndex = 0,
                        QueueCount = 1,
                        QueuePriorities = new float[] {1f}
                    }
                },
                EnabledExtensionNames = _requiredDeviceExtensions,
                EnabledFeatures = requiredPhysicalDeviceFeatures
            });
        }

        PhysicalDevice ChoosePhysicalDevice(PhysicalDevice[] physicalDevices)
        {
            var physicalDevice = physicalDevices[0];
            _logger.LogInfo("Choosing first available physical device: " + physicalDevice.GetProperties().DeviceName);
            _logger.LogInfo(physicalDevice.ExtensionsToString());
            _logger.LogInfo(physicalDevice.FeaturesToString());
            return physicalDevice;
        }

        PhysicalDevice[] GetPhysicalDevices(Instance instance)
        {
            var physicalDevices = instance.EnumeratePhysicalDevices();
            _logger.LogInfo("Physical devices:");

            foreach (var physicalDevice in physicalDevices)
            {
                _logger.LogInfo("\t" + physicalDevice.GetProperties().DeviceName);
            }

            return physicalDevices;
        }

        static SurfaceKhr CreateSurface(Instance instance, IntPtr window)
        {
            var info = new SDL.SDL_SysWMinfo();
            SDL.SDL_GetWindowWMInfo(window, ref info);

            var hinstance = Win32Api.GetModuleHandle(null);
            return instance.CreateWin32SurfaceKHR(new Win32SurfaceCreateInfoKhr
            {
                Hinstance = hinstance,
                Hwnd = info.info.win.window
            });
        }

        static IntPtr CreateWindow()
        {
            return SDL.SDL_CreateWindow("help, im stuck in a title bar factory", 100, 100, 500, 500, 0);
        }

        static Instance CreateInstance()
        {
            return new Instance(new InstanceCreateInfo
            {
                EnabledLayerNames = new string[] { "VK_LAYER_LUNARG_standard_validation" },
                EnabledExtensionNames = _requiredInstanceExtensions
            });
        }

        void SetupDebugCallback(Instance instance)
        {
            instance.EnableDebug((DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData) =>
            {
                _logger.LogInfo("Validation layer " + flags + ": " + Marshal.PtrToStringUTF8(message));
                return false;
            }, DebugReportFlagsExt.Error | DebugReportFlagsExt.Warning | DebugReportFlagsExt.PerformanceWarning/* | DebugReportFlagsExt.Information/* | DebugReportFlagsExt.Debug*/);
        }
    }
}