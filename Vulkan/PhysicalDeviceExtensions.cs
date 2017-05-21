using System;
using System.Collections.Generic;
using System.Linq;
using BundtCommon;
using Newtonsoft.Json;
using Vulkan;

namespace BundtCake
{
    static class PhysicalDeviceExtensions
    {
        static readonly MyLogger _logger = new MyLogger(nameof(PhysicalDeviceExtensions).Substring(0, 16));

        public static string GetName(this PhysicalDevice @this)
        {
            return @this.GetProperties().DeviceName;
        }

        public static uint GetIndexOfFirstAvailableGraphicsQueueFamily(this PhysicalDevice @this)
        {
            var queueFamilies = @this.GetQueueFamilyProperties();

            for (uint i = 0; i < queueFamilies.Length; i++)
            {
                if (queueFamilies[i].QueueFlags.HasFlag(QueueFlags.Graphics))
                {
                    return i;
                }
            }

            throw new VulkanException("Could not find a graphics queue family for device " + @this.GetName());
        }

        public static uint GetIndexOfFirstAvailablePresentQueueFamily(this PhysicalDevice @this, SurfaceKhr surface)
        {
            var queueFamilies = @this.GetQueueFamilyProperties();

            for (uint i = 0; i < queueFamilies.Length; i++)
            {
                if (@this.GetSurfaceSupportKHR(i, surface))
                {
                    return i;
                }
            }

            throw new VulkanException($"Could not find a queue family on device {@this.GetName()} that can present to given surface");
        }

        public static bool SupportsExtensions(this PhysicalDevice @this, IEnumerable<string> desiredExtensionNames)
        {
            var deviceExtensions = @this.EnumerateDeviceExtensionProperties();

            foreach (var desiredExtensionName in desiredExtensionNames)
            {
                if (deviceExtensions.Any(x => x.ExtensionName == desiredExtensionName) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool SupportsFeatures(this PhysicalDevice @this, PhysicalDeviceFeatures desiredFeatures)
        {
            var deviceFeatures = @this.GetFeatures().GetEnabledFeatures();

            foreach (var desiredFeature in desiredFeatures.GetEnabledFeatures())
            {
                if (deviceFeatures.Contains(desiredFeature) == false)
                {
                    return false;
                }
            }

            return true;
        }

        public static void PrintQueueFamilies(this PhysicalDevice @this)
        {
            var queueFamilyProperties = @this.GetQueueFamilyProperties();

            _logger.LogInfo($"Queue families from device {@this.GetName()}:");

            for (int i = 0; i < queueFamilyProperties.Length; i++)
            {
                var family = queueFamilyProperties[i];
                _logger.LogInfo("\tFamily " + i + ":");
                _logger.LogInfo("\t\tCount: " + family.QueueCount);
                _logger.LogInfo("\t\tFlags: " + family.QueueFlags);
                _logger.LogInfo("\t\tTimestampValidBits: " + family.TimestampValidBits);
                _logger.LogInfo("\t\tMinImageTransferGranularity:");
                _logger.LogInfo($"\t\t\tWidth: {family.MinImageTransferGranularity.Width}");
                _logger.LogInfo($"\t\t\tHeight: {family.MinImageTransferGranularity.Height}");
                _logger.LogInfo($"\t\t\tDepth: {family.MinImageTransferGranularity.Depth}");
            }
        }
    }
}