using Vulkan;

namespace BundtCake
{
    static class PhysicalDeviceExtensions
    {
        public static string ExtensionsToString(this PhysicalDevice @this)
        {
            var availablePhysicalDeviceExtensions = @this.EnumerateDeviceExtensionProperties();

            var str = "";

            str += "Available extensions for physical device " + @this.GetProperties().DeviceName + ":";

            foreach (var extension in availablePhysicalDeviceExtensions)
            {
               str += "\t" + extension.ExtensionName + "\n";
            }

            return str;
        }

        public static string FeaturesToString(this PhysicalDevice @this)
        {
            var str = "";

            var availablePhysicalDeviceFeatures = @this.GetFeatures();

            str += "Available features for physical device " + @this.GetProperties().DeviceName + ":\t";

            if (availablePhysicalDeviceFeatures.AlphaToOne) str += "AlphaToOne, ";
            if (availablePhysicalDeviceFeatures.DepthBiasClamp) str += "DepthBiasClamp, ";
            if (availablePhysicalDeviceFeatures.RobustBufferAccess) str += "RobustBufferAccess, ";
            if (availablePhysicalDeviceFeatures.ShaderStorageImageExtendedFormats) str += "ShaderStorageImageExtendedFormats, ";
            if (availablePhysicalDeviceFeatures.ShaderStorageImageMultisample) str += "ShaderStorageImageMultisample, ";
            if (availablePhysicalDeviceFeatures.ShaderStorageImageReadWithoutFormat) str += "ShaderStorageImageReadWithoutFormat, ";
            if (availablePhysicalDeviceFeatures.ShaderStorageImageWriteWithoutFormat) str += "ShaderStorageImageWriteWithoutFormat, ";
            if (availablePhysicalDeviceFeatures.ShaderUniformBufferArrayDynamicIndexing) str += "ShaderUniformBufferArrayDynamicIndexing, ";
            if (availablePhysicalDeviceFeatures.ShaderSampledImageArrayDynamicIndexing) str += "ShaderSampledImageArrayDynamicIndexing, ";
            if (availablePhysicalDeviceFeatures.ShaderStorageBufferArrayDynamicIndexing) str += "ShaderStorageBufferArrayDynamicIndexing, ";
            if (availablePhysicalDeviceFeatures.ShaderStorageImageArrayDynamicIndexing) str += "ShaderStorageImageArrayDynamicIndexing, ";
            if (availablePhysicalDeviceFeatures.ShaderClipDistance) str += "ShaderClipDistance, ";
            if (availablePhysicalDeviceFeatures.ShaderCullDistance) str += "ShaderCullDistance, ";
            if (availablePhysicalDeviceFeatures.ShaderFloat64) str += "ShaderFloat64, ";
            if (availablePhysicalDeviceFeatures.ShaderImageGatherExtended) str += "ShaderImageGatherExtended, ";
            if (availablePhysicalDeviceFeatures.ShaderInt64) str += "ShaderInt64, ";
            if (availablePhysicalDeviceFeatures.ShaderResourceResidency) str += "ShaderResourceResidency, ";
            if (availablePhysicalDeviceFeatures.ShaderResourceMinLod) str += "ShaderResourceMinLod, ";
            if (availablePhysicalDeviceFeatures.SparseBinding) str += "SparseBinding, ";
            if (availablePhysicalDeviceFeatures.SparseResidencyBuffer) str += "SparseResidencyBuffer, ";
            if (availablePhysicalDeviceFeatures.SparseResidencyImage2D) str += "SparseResidencyImage2D, ";
            if (availablePhysicalDeviceFeatures.SparseResidencyImage3D) str += "SparseResidencyImage3D, ";
            if (availablePhysicalDeviceFeatures.SparseResidency2Samples) str += "SparseResidency2Samples, ";
            if (availablePhysicalDeviceFeatures.SparseResidency4Samples) str += "SparseResidency4Samples, ";
            if (availablePhysicalDeviceFeatures.SparseResidency8Samples) str += "SparseResidency8Samples, ";
            if (availablePhysicalDeviceFeatures.SparseResidency16Samples) str += "SparseResidency16Samples, ";
            if (availablePhysicalDeviceFeatures.SparseResidencyAliased) str += "SparseResidencyAliased, ";
            if (availablePhysicalDeviceFeatures.ShaderInt16) str += "ShaderInt16, ";
            if (availablePhysicalDeviceFeatures.VariableMultisampleRate) str += "VariableMultisampleRate, ";
            if (availablePhysicalDeviceFeatures.ShaderTessellationAndGeometryPointSize) str += "ShaderTessellationAndGeometryPointSize, ";
            if (availablePhysicalDeviceFeatures.VertexPipelineStoresAndAtomics) str += "VertexPipelineStoresAndAtomics, ";
            if (availablePhysicalDeviceFeatures.FullDrawIndexUint32) str += "FullDrawIndexUint32, ";
            if (availablePhysicalDeviceFeatures.ImageCubeArray) str += "ImageCubeArray, ";
            if (availablePhysicalDeviceFeatures.IndependentBlend) str += "IndependentBlend, ";
            if (availablePhysicalDeviceFeatures.GeometryShader) str += "GeometryShader, ";
            if (availablePhysicalDeviceFeatures.TessellationShader) str += "TessellationShader, ";
            if (availablePhysicalDeviceFeatures.SampleRateShading) str += "SampleRateShading, ";
            if (availablePhysicalDeviceFeatures.DualSrcBlend) str += "DualSrcBlend, ";
            if (availablePhysicalDeviceFeatures.LogicOp) str += "LogicOp, ";
            if (availablePhysicalDeviceFeatures.MultiDrawIndirect) str += "MultiDrawIndirect, ";
            if (availablePhysicalDeviceFeatures.DrawIndirectFirstInstance) str += "DrawIndirectFirstInstance, ";
            if (availablePhysicalDeviceFeatures.DepthClamp) str += "DepthClamp, ";
            if (availablePhysicalDeviceFeatures.FragmentStoresAndAtomics) str += "FragmentStoresAndAtomics, ";
            if (availablePhysicalDeviceFeatures.DepthBiasClamp) str += "DepthBiasClamp, ";
            if (availablePhysicalDeviceFeatures.DepthBounds) str += "DepthBounds, ";
            if (availablePhysicalDeviceFeatures.WideLines) str += "WideLines, ";
            if (availablePhysicalDeviceFeatures.LargePoints) str += "LargePoints, ";
            if (availablePhysicalDeviceFeatures.AlphaToOne) str += "AlphaToOne, ";
            if (availablePhysicalDeviceFeatures.MultiViewport) str += "MultiViewport, ";
            if (availablePhysicalDeviceFeatures.SamplerAnisotropy) str += "SamplerAnisotropy, ";
            if (availablePhysicalDeviceFeatures.TextureCompressionEtc2) str += "TextureCompressionEtc2, ";
            if (availablePhysicalDeviceFeatures.TextureCompressionAstcLdr) str += "TextureCompressionAstcLdr, ";
            if (availablePhysicalDeviceFeatures.TextureCompressionBc) str += "TextureCompressionBc, ";
            if (availablePhysicalDeviceFeatures.OcclusionQueryPrecise) str += "OcclusionQueryPrecise, ";
            if (availablePhysicalDeviceFeatures.PipelineStatisticsQuery) str += "PipelineStatisticsQuery, ";
            if (availablePhysicalDeviceFeatures.FillModeNonSolid) str += "FillModeNonSolid, ";
            if (availablePhysicalDeviceFeatures.InheritedQueries) str += "InheritedQueries, ";

            return str;
        }
    }
}