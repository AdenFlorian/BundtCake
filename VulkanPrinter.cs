using BundtCommon;
using Vulkan;

namespace BundtCake
{
    public static class VulkanPrinter
    {
        static MyLogger _logger = new MyLogger(nameof(VulkanPrinter));
        
        public static void PrintAvailableInstanceLayers()
        {
            var availableLayers = Commands.EnumerateInstanceLayerProperties();

            _logger.LogInfo("Available instance layers:");
            foreach (var layer in availableLayers)
            {
                _logger.LogInfo("\t" + layer.LayerName);
            }
        }

        public static void PrintAvailableInstanceExtensions()
        {
            var availableExtensions = Commands.EnumerateInstanceExtensionProperties();

            _logger.LogInfo("Available instance extensions:");
            foreach (var extension in availableExtensions)
            {
                _logger.LogInfo("\t" + extension.ExtensionName);
            }
        }
    }
}