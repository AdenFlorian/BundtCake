using BundtCommon;
using Vulkan;

namespace BundtCake
{
    public static class VulkanPrinter
    {
        static MyLogger _logger = new MyLogger(nameof(VulkanPrinter));
        
        public static void PrintAvailableLayers()
        {
            var availableLayers = Commands.EnumerateInstanceLayerProperties();

            _logger.LogInfo("Available layers:");
            foreach (var layer in availableLayers)
            {
                _logger.LogInfo("\t" + layer.LayerName);
            }
        }

        public static void PrintAvailableInstanceExtensions()
        {
            var availableExtensions = Commands.EnumerateInstanceExtensionProperties();

            _logger.LogInfo("Available extensions:");
            foreach (var extension in availableExtensions)
            {
                _logger.LogInfo("\t" + extension.ExtensionName);
            }
        }

        public static void PrintAvailablePhysicalDeviceExtensions(PhysicalDevice physicalDevice)
        {
            var availablePhysicalDeviceExtensions = physicalDevice.EnumerateDeviceExtensionProperties();

            _logger.LogInfo("Available extensions for physical device " + physicalDevice.GetProperties().DeviceName + ":");
            foreach (var extension in availablePhysicalDeviceExtensions)
            {
                _logger.LogInfo("\t" + extension.ExtensionName);
            }
        }

        public static void PrintAvailablePhysicalDeviceFeatures(PhysicalDevice physicalDevice)
        {
            var availablePhysicalDeviceFeatures = physicalDevice.GetFeatures();

            _logger.LogInfo("Available features for physical device " + physicalDevice.GetProperties().DeviceName + ":");

            _logger.LogInfo("\tAlphaToOne: " + (availablePhysicalDeviceFeatures.AlphaToOne ? "true" : "false"));
            _logger.LogInfo("\tDepthBiasClamp: " + (availablePhysicalDeviceFeatures.DepthBiasClamp ? "true" : "false"));
            _logger.LogInfo("\tRobustBufferAccess: " + (availablePhysicalDeviceFeatures.RobustBufferAccess ? "true" : "false"));
            _logger.LogInfo("\tShaderStorageImageExtendedFormats: " + (availablePhysicalDeviceFeatures.ShaderStorageImageExtendedFormats ? "true" : "false"));
            _logger.LogInfo("\tShaderStorageImageMultisample: " + (availablePhysicalDeviceFeatures.ShaderStorageImageMultisample ? "true" : "false"));
            _logger.LogInfo("\tShaderStorageImageReadWithoutFormat: " + (availablePhysicalDeviceFeatures.ShaderStorageImageReadWithoutFormat ? "true" : "false"));
            _logger.LogInfo("\tShaderStorageImageWriteWithoutFormat: " + (availablePhysicalDeviceFeatures.ShaderStorageImageWriteWithoutFormat ? "true" : "false"));
            _logger.LogInfo("\tShaderUniformBufferArrayDynamicIndexing: " + (availablePhysicalDeviceFeatures.ShaderUniformBufferArrayDynamicIndexing ? "true" : "false"));
            _logger.LogInfo("\tShaderSampledImageArrayDynamicIndexing: " + (availablePhysicalDeviceFeatures.ShaderSampledImageArrayDynamicIndexing ? "true" : "false"));
            _logger.LogInfo("\tShaderStorageBufferArrayDynamicIndexing: " + (availablePhysicalDeviceFeatures.ShaderStorageBufferArrayDynamicIndexing ? "true" : "false"));
            _logger.LogInfo("\tShaderStorageImageArrayDynamicIndexing: " + (availablePhysicalDeviceFeatures.ShaderStorageImageArrayDynamicIndexing ? "true" : "false"));
            _logger.LogInfo("\tShaderClipDistance: " + (availablePhysicalDeviceFeatures.ShaderClipDistance ? "true" : "false"));
            _logger.LogInfo("\tShaderCullDistance: " + (availablePhysicalDeviceFeatures.ShaderCullDistance ? "true" : "false"));
            _logger.LogInfo("\tShaderFloat64: " + (availablePhysicalDeviceFeatures.ShaderFloat64 ? "true" : "false"));
            _logger.LogInfo("\tShaderImageGatherExtended: " + (availablePhysicalDeviceFeatures.ShaderImageGatherExtended ? "true" : "false"));
            _logger.LogInfo("\tShaderInt64: " + (availablePhysicalDeviceFeatures.ShaderInt64 ? "true" : "false"));
            _logger.LogInfo("\tShaderResourceResidency: " + (availablePhysicalDeviceFeatures.ShaderResourceResidency ? "true" : "false"));
            _logger.LogInfo("\tShaderResourceMinLod: " + (availablePhysicalDeviceFeatures.ShaderResourceMinLod ? "true" : "false"));
            _logger.LogInfo("\tSparseBinding: " + (availablePhysicalDeviceFeatures.SparseBinding ? "true" : "false"));
            _logger.LogInfo("\tSparseResidencyBuffer: " + (availablePhysicalDeviceFeatures.SparseResidencyBuffer ? "true" : "false"));
            _logger.LogInfo("\tSparseResidencyImage2D: " + (availablePhysicalDeviceFeatures.SparseResidencyImage2D ? "true" : "false"));
            _logger.LogInfo("\tSparseResidencyImage3D: " + (availablePhysicalDeviceFeatures.SparseResidencyImage3D ? "true" : "false"));
            _logger.LogInfo("\tSparseResidency2Samples: " + (availablePhysicalDeviceFeatures.SparseResidency2Samples ? "true" : "false"));
            _logger.LogInfo("\tSparseResidency4Samples: " + (availablePhysicalDeviceFeatures.SparseResidency4Samples ? "true" : "false"));
            _logger.LogInfo("\tSparseResidency8Samples: " + (availablePhysicalDeviceFeatures.SparseResidency8Samples ? "true" : "false"));
            _logger.LogInfo("\tSparseResidency16Samples: " + (availablePhysicalDeviceFeatures.SparseResidency16Samples ? "true" : "false"));
            _logger.LogInfo("\tSparseResidencyAliased: " + (availablePhysicalDeviceFeatures.SparseResidencyAliased ? "true" : "false"));
            _logger.LogInfo("\tShaderInt16: " + (availablePhysicalDeviceFeatures.ShaderInt16 ? "true" : "false"));
            _logger.LogInfo("\tVariableMultisampleRate: " + (availablePhysicalDeviceFeatures.VariableMultisampleRate ? "true" : "false"));
            _logger.LogInfo("\tShaderTessellationAndGeometryPointSize: " + (availablePhysicalDeviceFeatures.ShaderTessellationAndGeometryPointSize ? "true" : "false"));
            _logger.LogInfo("\tVertexPipelineStoresAndAtomics: " + (availablePhysicalDeviceFeatures.VertexPipelineStoresAndAtomics ? "true" : "false"));
            _logger.LogInfo("\tFullDrawIndexUint32: " + (availablePhysicalDeviceFeatures.FullDrawIndexUint32 ? "true" : "false"));
            _logger.LogInfo("\tImageCubeArray: " + (availablePhysicalDeviceFeatures.ImageCubeArray ? "true" : "false"));
            _logger.LogInfo("\tIndependentBlend: " + (availablePhysicalDeviceFeatures.IndependentBlend ? "true" : "false"));
            _logger.LogInfo("\tGeometryShader: " + (availablePhysicalDeviceFeatures.GeometryShader ? "true" : "false"));
            _logger.LogInfo("\tTessellationShader: " + (availablePhysicalDeviceFeatures.TessellationShader ? "true" : "false"));
            _logger.LogInfo("\tSampleRateShading: " + (availablePhysicalDeviceFeatures.SampleRateShading ? "true" : "false"));
            _logger.LogInfo("\tDualSrcBlend: " + (availablePhysicalDeviceFeatures.DualSrcBlend ? "true" : "false"));
            _logger.LogInfo("\tLogicOp: " + (availablePhysicalDeviceFeatures.LogicOp ? "true" : "false"));
            _logger.LogInfo("\tMultiDrawIndirect: " + (availablePhysicalDeviceFeatures.MultiDrawIndirect ? "true" : "false"));
            _logger.LogInfo("\tDrawIndirectFirstInstance: " + (availablePhysicalDeviceFeatures.DrawIndirectFirstInstance ? "true" : "false"));
            _logger.LogInfo("\tDepthClamp: " + (availablePhysicalDeviceFeatures.DepthClamp ? "true" : "false"));
            _logger.LogInfo("\tFragmentStoresAndAtomics: " + (availablePhysicalDeviceFeatures.FragmentStoresAndAtomics ? "true" : "false"));
            _logger.LogInfo("\tDepthBiasClamp: " + (availablePhysicalDeviceFeatures.DepthBiasClamp ? "true" : "false"));
            _logger.LogInfo("\tDepthBounds: " + (availablePhysicalDeviceFeatures.DepthBounds ? "true" : "false"));
            _logger.LogInfo("\tWideLines: " + (availablePhysicalDeviceFeatures.WideLines ? "true" : "false"));
            _logger.LogInfo("\tLargePoints: " + (availablePhysicalDeviceFeatures.LargePoints ? "true" : "false"));
            _logger.LogInfo("\tAlphaToOne: " + (availablePhysicalDeviceFeatures.AlphaToOne ? "true" : "false"));
            _logger.LogInfo("\tMultiViewport: " + (availablePhysicalDeviceFeatures.MultiViewport ? "true" : "false"));
            _logger.LogInfo("\tSamplerAnisotropy: " + (availablePhysicalDeviceFeatures.SamplerAnisotropy ? "true" : "false"));
            _logger.LogInfo("\tTextureCompressionEtc2: " + (availablePhysicalDeviceFeatures.TextureCompressionEtc2 ? "true" : "false"));
            _logger.LogInfo("\tTextureCompressionAstcLdr: " + (availablePhysicalDeviceFeatures.TextureCompressionAstcLdr ? "true" : "false"));
            _logger.LogInfo("\tTextureCompressionBc: " + (availablePhysicalDeviceFeatures.TextureCompressionBc ? "true" : "false"));
            _logger.LogInfo("\tOcclusionQueryPrecise: " + (availablePhysicalDeviceFeatures.OcclusionQueryPrecise ? "true" : "false"));
            _logger.LogInfo("\tPipelineStatisticsQuery: " + (availablePhysicalDeviceFeatures.PipelineStatisticsQuery ? "true" : "false"));
            _logger.LogInfo("\tFillModeNonSolid: " + (availablePhysicalDeviceFeatures.FillModeNonSolid ? "true" : "false"));
            _logger.LogInfo("\tInheritedQueries: " + (availablePhysicalDeviceFeatures.InheritedQueries ? "true" : "false"));
        }
    }
}