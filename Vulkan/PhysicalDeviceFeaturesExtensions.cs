using System.Collections.Generic;
using Vulkan;

namespace BundtCake
{
    public static class PhysicalDeviceFeaturesExtensions
    {
        public static IEnumerable<string> GetEnabledFeatures(this PhysicalDeviceFeatures @this)
        {
            var enabledFeatures = new List<string>();

            foreach (var field in @this.GetType().GetFields())
            {
                if ((Bool32)field.GetValue(@this))
                {
                    enabledFeatures.Add(field.Name);
                }
            }

            return enabledFeatures;
        }
    }
}