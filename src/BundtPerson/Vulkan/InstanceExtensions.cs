using System;
using System.Collections.Generic;
using System.Linq;
using BundtCommon;
using Newtonsoft.Json;
using Vulkan;

namespace BundtCake
{
    public static class InstanceExtensions
    {
        static readonly MyLogger _logger = new MyLogger(nameof(InstanceExtensions).Substring(0, 10));

        public static IEnumerable<PhysicalDevice> GetSuitablePhysicalDevices(this Instance @this, string[] requiredPhysicalDeviceExtensions, PhysicalDeviceFeatures requiredPhysicalDeviceFeatures, QueueFlags requiredQueueFlags, SurfaceKhr surface)
        {
            var suitablePhysicalDevices = new List<PhysicalDevice>();

            foreach (var physicalDevice in @this.EnumeratePhysicalDevices())
            {
                // Make sure device supports required extensions
                if (physicalDevice.SupportsExtensions(requiredPhysicalDeviceExtensions) == false)
                {
                    continue;
                }

                // Make sure device supports required features
                if (physicalDevice.SupportsFeatures(requiredPhysicalDeviceFeatures) == false)
                {
                    continue;
                }

                // Make sure device has a queue family with required flags
                if (physicalDevice.GetIndexOfFirstAvailableGraphicsQueueFamily() < 0)
                {
                    continue;
                }

                // Make sure device has a queue family with required flags
                if (physicalDevice.GetIndexOfFirstAvailablePresentQueueFamily(surface) < 0)
                {
                    continue;
                }

                suitablePhysicalDevices.Add(physicalDevice);
            }

            if (suitablePhysicalDevices.Count() == 0)
            {
                throw new VulkanException("No suitable physical device found");
            }

            return suitablePhysicalDevices;
        }
    }
}