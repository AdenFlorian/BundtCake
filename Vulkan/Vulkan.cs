using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BundtCommon;
using Newtonsoft.Json;
using Vulkan;
using Vulkan.Windows;

namespace BundtCake
{
    class Vulkan
    {
        static readonly MyLogger _logger = new MyLogger(nameof(Vulkan));
        static readonly string[] _requiredInstanceExtensions = new string[] { "VK_EXT_debug_report", "VK_KHR_win32_surface", "VK_KHR_surface" };
        static readonly string[] _requiredPhysicalDeviceExtensions = new string[] { "VK_KHR_swapchain" };

        uint _graphicsQueueFamilyIndex;
        uint _presentQueueFamilyIndex;

        public void Initialize(Window window)
        {
            _logger.LogInfo("Initializing Vulkan...");

            VulkanPrinter.PrintAvailableInstanceLayers();
            VulkanPrinter.PrintAvailableInstanceExtensions();

            var instance = CreateInstance();

            SetupDebugCallback(instance);

            var surface = CreateWin32Surface(instance, window);

            var physicalDevices = GetPhysicalDevices(instance);
            
            var requiredPhysicalDeviceFeatures = GetRequiredFeatures();

            var suitablePhysicalDevices = instance.GetSuitablePhysicalDevices(_requiredPhysicalDeviceExtensions, requiredPhysicalDeviceFeatures, QueueFlags.Graphics, surface);

            var physicalDevice = ChooseBestPhysicalDevice(physicalDevices);

            physicalDevice.PrintQueueFamilies();

            _graphicsQueueFamilyIndex = physicalDevice.GetIndexOfFirstAvailableGraphicsQueueFamily();
            _presentQueueFamilyIndex = physicalDevice.GetIndexOfFirstAvailablePresentQueueFamily(surface);

            var device = CreateLogicalDevice(physicalDevice, requiredPhysicalDeviceFeatures);

            var swapChain = CreateSwapchain(device, physicalDevice, surface, window);

            _logger.LogInfo("Vulkan initialized");
        }

        static Instance CreateInstance()
        {
            _logger.LogDebug("Creating Vulkan instance...");

            var instanceCreateInfo = new InstanceCreateInfo
            {
                EnabledLayerNames = new string[] { "VK_LAYER_LUNARG_standard_validation" },
                EnabledExtensionNames = _requiredInstanceExtensions
            };

            _logger.LogDebug("Creating Vulkan instance with info: \n" + JsonConvert.SerializeObject(instanceCreateInfo).Prettify());

            return new Instance(instanceCreateInfo);
        }

        static void SetupDebugCallback(Instance instance)
        {
            _logger.LogDebug("Setting up debug callback...");

            instance.EnableDebug((DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData) =>
            {
                if (flags.HasFlag(DebugReportFlagsExt.Error))
                {
                    _logger.LogError("Validation layer " + flags + ": " + Marshal.PtrToStringUTF8(message));
                }
                else
                {
                    _logger.LogInfo("Validation layer " + flags + ": " + Marshal.PtrToStringUTF8(message));
                }
                return false;
            }, DebugReportFlagsExt.Error | DebugReportFlagsExt.Warning | DebugReportFlagsExt.PerformanceWarning/* | DebugReportFlagsExt.Information/* | DebugReportFlagsExt.Debug*/);
        }

        static SurfaceKhr CreateWin32Surface(Instance instance, Window window)
        {
            _logger.LogDebug("Creating Win32 surface...");

            var win32SurfaceCreateInfoKhr = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = window.Win32hInstance,
                Hwnd = window.Win32hWnd
            };

            _logger.LogDebug("Creating Win32 surface with info: \n" + JsonConvert.SerializeObject(win32SurfaceCreateInfoKhr).Prettify());

            return instance.CreateWin32SurfaceKHR(win32SurfaceCreateInfoKhr);
        }

        // TODO Create surfaces for linux and osx

        static IEnumerable<PhysicalDevice> GetPhysicalDevices(Instance instance)
        {
            var physicalDevices = instance.EnumeratePhysicalDevices().ToList();
            
            _logger.LogInfo("Detected physical devices:");

            foreach (var physicalDevice in physicalDevices)
            {
                _logger.LogInfo("\t" + physicalDevice.GetProperties().DeviceName);
            }

            return physicalDevices;
        }

        static PhysicalDevice ChooseBestPhysicalDevice(IEnumerable<PhysicalDevice> suitablePhysicalDevices)
        {
            PhysicalDevice selectedDevice = null;

            foreach (var candidate in suitablePhysicalDevices)
            {
                if (candidate.GetProperties().DeviceType == PhysicalDeviceType.DiscreteGpu)
                {
                    selectedDevice = candidate;
                    _logger.LogDebug("Chose first candidate discrete GPU device: " + candidate.GetName());
                    break;
                }
            }

            if (selectedDevice == null)
            {
                selectedDevice = suitablePhysicalDevices.First();
                _logger.LogDebug("Chose first available candidate (couldn't decide which was best): " + selectedDevice.GetName());
            }

            _logger.LogInfo("Selected physical device: " + selectedDevice.GetName(), ConsoleColor.Green);

            _logger.LogDebug("Available extensions for physical device " + selectedDevice.GetName() + ":\n"
                + JsonConvert.SerializeObject(selectedDevice.EnumerateDeviceExtensionProperties().ToList().ConvertAll(x => x.ExtensionName)));

            _logger.LogDebug("Available features for physical device " + selectedDevice.GetName() + ":\n"
                + JsonConvert.SerializeObject(selectedDevice.GetFeatures().GetEnabledFeatures()));

            return selectedDevice;
        }

        static PhysicalDeviceFeatures GetRequiredFeatures()
        {
            var features = new PhysicalDeviceFeatures();
            features.SamplerAnisotropy = true;
            return features;
        }

        Device CreateLogicalDevice(PhysicalDevice physicalDevice, PhysicalDeviceFeatures requiredFeatures)
        {
            _logger.LogInfo("Creating logical device from physical device: " + physicalDevice.GetProperties().DeviceName);

            var queueCreateInfos = new List<DeviceQueueCreateInfo>();

            queueCreateInfos.Add(new DeviceQueueCreateInfo
            {
                QueueFamilyIndex = _graphicsQueueFamilyIndex,
                QueueCount = 1,
                QueuePriorities = new float[] { 1f }
            });

            if (_graphicsQueueFamilyIndex != _presentQueueFamilyIndex)
            {
                queueCreateInfos.Add(new DeviceQueueCreateInfo
                {
                    QueueFamilyIndex = _presentQueueFamilyIndex,
                    QueueCount = 1,
                    QueuePriorities = new float[] { 1f }
                });
            }

            var deviceCreateInfo = new DeviceCreateInfo
            {
                QueueCreateInfos = queueCreateInfos.ToArray(),
                EnabledExtensionNames = _requiredPhysicalDeviceExtensions,
                EnabledFeatures = requiredFeatures
            };

            _logger.LogInfo($"Creating logical device with QueueCreateInfos:\n{JsonConvert.SerializeObject(deviceCreateInfo.QueueCreateInfos).Prettify()}");
            _logger.LogInfo($"Creating logical device with extensions: {JsonConvert.SerializeObject(deviceCreateInfo.EnabledExtensionNames)}");
            _logger.LogInfo($"Creating logical device with features: {JsonConvert.SerializeObject(deviceCreateInfo.EnabledFeatures.GetEnabledFeatures())}");

            var device = physicalDevice.CreateDevice(deviceCreateInfo);


            var graphicsQueue = device.GetQueue(_graphicsQueueFamilyIndex, 0);
            var presentQueue = device.GetQueue(_presentQueueFamilyIndex, 0);

            return device;
        }

        SwapchainKhr CreateSwapchain(Device device, PhysicalDevice physicalDevice, SurfaceKhr surface, Window window)
        {
            _logger.LogInfo("Creating swap chain...");

            var swapChainSupport = QuerySwapChainSupport(physicalDevice, surface);

            _logger.LogInfo("surfaceCapabilities:");
            _logger.LogInfo("\tMinImageCount: " + swapChainSupport.SurfaceCapabilities.MinImageCount);
            _logger.LogInfo("\tMaxImageCount: " + swapChainSupport.SurfaceCapabilities.MaxImageCount);
            
            var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.SurfaceFormats);
            var swapChainExtent = ChooseSwapExtent(swapChainSupport.SurfaceCapabilities, window);

            var swapchainCreateInfo = new SwapchainCreateInfoKhr
            {
                PresentMode = ChooseSwapPresentMode(swapChainSupport.SurfacePresentModes),
                CompositeAlpha = CompositeAlphaFlagsKhr.Opaque,
                PreTransform = swapChainSupport.SurfaceCapabilities.CurrentTransform,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                ImageArrayLayers = 1,
                ImageExtent = swapChainExtent,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageFormat = surfaceFormat.Format,
                MinImageCount = CalculateSwapChainImageCount(swapChainSupport),
                Surface = surface,
                Clipped = true,
                OldSwapchain = null,
            };

            if (_graphicsQueueFamilyIndex != _presentQueueFamilyIndex)
            {
                _logger.LogInfo("Swap chain image sharing mode: Concurrent");
                swapchainCreateInfo.ImageSharingMode = SharingMode.Concurrent;
                swapchainCreateInfo.QueueFamilyIndexCount = 2;
                swapchainCreateInfo.QueueFamilyIndices = new uint[] {_graphicsQueueFamilyIndex, _presentQueueFamilyIndex};
            }
            else
            {
                _logger.LogInfo("Swap chain image sharing mode: Exclusive");
                swapchainCreateInfo.ImageSharingMode = SharingMode.Exclusive;
                swapchainCreateInfo.QueueFamilyIndexCount = 0;
                swapchainCreateInfo.QueueFamilyIndices = null;
            }

            return device.CreateSwapchainKHR(swapchainCreateInfo);
        }

        static SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice, SurfaceKhr surface)
        {
            return new SwapChainSupportDetails
            {
                SurfaceCapabilities = physicalDevice.GetSurfaceCapabilitiesKHR(surface),
                SurfaceFormats = physicalDevice.GetSurfaceFormatsKHR(surface).ToList(),
                SurfacePresentModes = physicalDevice.GetSurfacePresentModesKHR(surface).ToList()
            };
        }

        struct SwapChainSupportDetails
        {
            public SurfaceCapabilitiesKhr SurfaceCapabilities;
            public IEnumerable<SurfaceFormatKhr> SurfaceFormats;
            public IEnumerable<PresentModeKhr> SurfacePresentModes;
        };

        static SurfaceFormatKhr ChooseSwapSurfaceFormat(IEnumerable<SurfaceFormatKhr> availableFormats)
        {
            if (availableFormats.Count() == 1 && availableFormats.First().Format == Format.Undefined)
            {
                return new SurfaceFormatKhr{Format = Format.B8G8R8A8Unorm, ColorSpace = ColorSpaceKhr.SrgbNonlinear};
            }

            foreach (var availableFormat in availableFormats)
            {
                if (availableFormat.Format == Format.B8G8R8A8Unorm && availableFormat.ColorSpace == ColorSpaceKhr.SrgbNonlinear)
                {
                    return availableFormat;
                }
            }

            return availableFormats.First();
        }

        static PresentModeKhr ChooseSwapPresentMode(IEnumerable<PresentModeKhr> availablePresentModes)
        {
            var bestMode = PresentModeKhr.Fifo;

            foreach (var availablePresentMode in availablePresentModes)
            {
                if (availablePresentMode == PresentModeKhr.Mailbox)
                {
                    bestMode = availablePresentMode;
                    break;
                }
                else if (availablePresentMode == PresentModeKhr.Immediate)
                {
                    bestMode = availablePresentMode;
                }
            }

            return bestMode;
        }

        static uint CalculateSwapChainImageCount(SwapChainSupportDetails swapChainSupport)
        {
            var imageCount = swapChainSupport.SurfaceCapabilities.MinImageCount + 1;
            if (swapChainSupport.SurfaceCapabilities.MaxImageCount > 0 && imageCount > swapChainSupport.SurfaceCapabilities.MaxImageCount)
            {
                imageCount = swapChainSupport.SurfaceCapabilities.MaxImageCount;
            }
            _logger.LogInfo("Calculated swap chain image count: " + imageCount);
            return imageCount;
        }

        static Extent2D ChooseSwapExtent(SurfaceCapabilitiesKhr capabilities, Window window)
        {
            if (capabilities.CurrentExtent.Width != uint.MaxValue)
            {
                return capabilities.CurrentExtent;
            }
            else
            {
                var windowSize = window.GetSize();
                var actualExtent = new Extent2D { Width = (uint)windowSize.Width, Height = (uint)windowSize.Height };

                actualExtent.Width = Math.Max(capabilities.MinImageExtent.Width, Math.Min(capabilities.MaxImageExtent.Width, actualExtent.Width));
                actualExtent.Height = Math.Max(capabilities.MinImageExtent.Height, Math.Min(capabilities.MaxImageExtent.Height, actualExtent.Height));

                return actualExtent;
            }
        }
    }
}