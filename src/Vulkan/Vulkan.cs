using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BundtCommon;
using Newtonsoft.Json;
using Vulkan;
using Vulkan.Windows;

namespace BundtCake
{
    class Vulkan
    {
        const uint VK_SUBPASS_EXTERNAL = (~0U);

        static readonly MyLogger _logger = new MyLogger(nameof(Vulkan));
        static readonly string[] _requiredInstanceExtensions = new string[] { "VK_EXT_debug_report", "VK_KHR_win32_surface", "VK_KHR_surface" };
        static readonly string[] _requiredPhysicalDeviceExtensions = new string[] { "VK_KHR_swapchain" };

        Instance _instance;
        SurfaceKhr _surface;
        PhysicalDevice _physicalDevice;
        Device _device;
        uint _graphicsQueueFamilyIndex;
        uint _presentQueueFamilyIndex;
        Format _swapChainImageFormat;
        SwapchainKhr _swapChain;
        Extent2D _swapChainExtent;
        IEnumerable<ImageView> _swapChainImageViews;
        RenderPass _renderPass;
        DescriptorSetLayout _descriptorSetLayout;
        PipelineLayout _graphicsPipelineLayout;
        Pipeline _graphicsPipeline;
        CommandPool _commandPool;
        CommandPool _tempCommandPool;

        public async Task InitializeAsync(Window window)
        {
            _logger.LogInfo("Initializing Vulkan...");

            VulkanPrinter.PrintAvailableInstanceLayers();
            VulkanPrinter.PrintAvailableInstanceExtensions();

            _instance = CreateInstance();

            SetupDebugCallback();

            _surface = CreateWin32Surface(window);

            var physicalDevices = GetPhysicalDevices();

            var requiredPhysicalDeviceFeatures = GetRequiredFeatures();

            var suitablePhysicalDevices = _instance.GetSuitablePhysicalDevices(_requiredPhysicalDeviceExtensions, requiredPhysicalDeviceFeatures, QueueFlags.Graphics, _surface);

            _physicalDevice = ChooseBestPhysicalDevice(physicalDevices);

            _physicalDevice.PrintQueueFamilies();

            _graphicsQueueFamilyIndex = _physicalDevice.GetIndexOfFirstAvailableGraphicsQueueFamily();
            _presentQueueFamilyIndex = _physicalDevice.GetIndexOfFirstAvailablePresentQueueFamily(_surface);

            _device = CreateLogicalDevice(requiredPhysicalDeviceFeatures);

            _swapChain = CreateSwapchain(window);

            var swapChainImages = _device.GetSwapchainImagesKHR(_swapChain);

            _swapChainImageViews = CreateSwapChainImageViews(swapChainImages);

            _renderPass = CreateRenderPass();

            CreateDescriptorSetLayout();

            await CreateGraphicsPipelineAsync();

            CreateCommandPool();
            CreateTempCommandPool();

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

        void SetupDebugCallback()
        {
            _logger.LogDebug("Setting up debug callback...");

            _instance.EnableDebug(DebugCallback, DebugReportFlagsExt.Error | DebugReportFlagsExt.Warning | DebugReportFlagsExt.PerformanceWarning/* | DebugReportFlagsExt.Information/* | DebugReportFlagsExt.Debug*/);
        }

        Bool32 DebugCallback(DebugReportFlagsExt flags, DebugReportObjectTypeExt objectType, ulong objectHandle, IntPtr location, int messageCode, IntPtr layerPrefix, IntPtr message, IntPtr userData)
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
        }

        SurfaceKhr CreateWin32Surface(Window window)
        {
            _logger.LogDebug("Creating Win32 surface...");

            var win32SurfaceCreateInfoKhr = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = window.Win32hInstance,
                Hwnd = window.Win32hWnd
            };

            _logger.LogDebug("Creating Win32 surface with info: \n" + JsonConvert.SerializeObject(win32SurfaceCreateInfoKhr).Prettify());

            return _instance.CreateWin32SurfaceKHR(win32SurfaceCreateInfoKhr);
        }

        // TODO Create surfaces for linux and osx

        IEnumerable<PhysicalDevice> GetPhysicalDevices()
        {
            var physicalDevices = _instance.EnumeratePhysicalDevices().ToList();
            
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

        Device CreateLogicalDevice(PhysicalDeviceFeatures requiredFeatures)
        {
            _logger.LogInfo("Creating logical device from physical device: " + _physicalDevice.GetProperties().DeviceName);

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

            var device = _physicalDevice.CreateDevice(deviceCreateInfo);

            var graphicsQueue = device.GetQueue(_graphicsQueueFamilyIndex, 0);
            var presentQueue = device.GetQueue(_presentQueueFamilyIndex, 0);

            return device;
        }

        SwapchainKhr CreateSwapchain(Window window)
        {
            _logger.LogInfo("Creating swap chain...");

            var surfaceCapabilities = _physicalDevice.GetSurfaceCapabilitiesKHR(_surface);

            _logger.LogInfo("surfaceCapabilities:");
            _logger.LogInfo("\tMinImageCount: " + surfaceCapabilities.MinImageCount);
            _logger.LogInfo("\tMaxImageCount: " + surfaceCapabilities.MaxImageCount);

            _swapChainExtent = ChooseSwapExtent(surfaceCapabilities, window);

            var surfacePresentModes = _physicalDevice.GetSurfacePresentModesKHR(_surface);
            var surfacePresentMode = ChooseSwapPresentMode(surfacePresentModes);
            
            var surfaceFormats = _physicalDevice.GetSurfaceFormatsKHR(_surface).ToList();
            var surfaceFormat = ChooseSwapSurfaceFormat(surfaceFormats);
            _swapChainImageFormat = surfaceFormat.Format;

            var swapchainCreateInfo = new SwapchainCreateInfoKhr
            {
                PresentMode = surfacePresentMode,
                CompositeAlpha = CompositeAlphaFlagsKhr.Opaque,
                PreTransform = surfaceCapabilities.CurrentTransform,
                ImageUsage = ImageUsageFlags.ColorAttachment,
                ImageArrayLayers = 1,
                ImageExtent = _swapChainExtent,
                ImageColorSpace = surfaceFormat.ColorSpace,
                ImageFormat = _swapChainImageFormat,
                MinImageCount = CalculateSwapChainImageCount(surfaceCapabilities),
                Surface = _surface,
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

            return _device.CreateSwapchainKHR(swapchainCreateInfo);
        }

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

        static uint CalculateSwapChainImageCount(SurfaceCapabilitiesKhr surfaceCapabilities)
        {
            var imageCount = surfaceCapabilities.MinImageCount + 1;
            
            if (surfaceCapabilities.MaxImageCount > 0 && imageCount > surfaceCapabilities.MaxImageCount)
            {
                imageCount = surfaceCapabilities.MaxImageCount;
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
        
        IEnumerable<ImageView> CreateSwapChainImageViews(Image[] swapChainImages)
        {
            _logger.LogInfo("Creating swap chain image views...");

            var swapChainImageViews = new List<ImageView>();

            for (var i = 0; i < swapChainImages.Count(); i++)
            {
                swapChainImageViews.Add(CreateImageView(swapChainImages[i], _swapChainImageFormat, ImageAspectFlags.Color));
            }

            return swapChainImageViews;
        }

        ImageView CreateImageView(Image image, Format format, ImageAspectFlags aspectFlags)
        {
            var viewInfo = new ImageViewCreateInfo
            {
                Image = image,
                ViewType = ImageViewType.View2D,
                Format = format,
                Components = new ComponentMapping
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                },
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = aspectFlags,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            return _device.CreateImageView(viewInfo);
        }

        RenderPass CreateRenderPass()
        {
            _logger.LogInfo("Creating render pass...");

            var renderPassInfo = new RenderPassCreateInfo
            {
                Attachments = new AttachmentDescription[]
                {
                    new AttachmentDescription
                    {
                        Format = _swapChainImageFormat,
                        Samples = SampleCountFlags.Count1,
                        LoadOp = AttachmentLoadOp.Clear,
                        StoreOp = AttachmentStoreOp.Store,
                        StencilLoadOp = AttachmentLoadOp.DontCare,
                        StencilStoreOp = AttachmentStoreOp.DontCare,
                        InitialLayout = ImageLayout.Undefined,
                        FinalLayout = ImageLayout.PresentSrcKhr
                    },
                    new AttachmentDescription
                    {
                        Format = FindDepthFormat(),
                        Samples = SampleCountFlags.Count1,
                        LoadOp = AttachmentLoadOp.Clear,
                        StoreOp = AttachmentStoreOp.DontCare,
                        StencilLoadOp = AttachmentLoadOp.DontCare,
                        StencilStoreOp = AttachmentStoreOp.DontCare,
                        InitialLayout = ImageLayout.Undefined,
                        FinalLayout = ImageLayout.DepthStencilAttachmentOptimal
                    }
                },
                Subpasses = new SubpassDescription[]
                {
                    new SubpassDescription
                    {
                        PipelineBindPoint = PipelineBindPoint.Graphics,
                        //InputAttachmentCount = ,
                        //InputAttachments = ,
                        //ColorAttachmentCount = 1,
                        ColorAttachments = new AttachmentReference[]
                        {
                            new AttachmentReference
                            {
                                Attachment = 0,
                                Layout = ImageLayout.ColorAttachmentOptimal
                            }
                        },
                        //ResolveAttachments = ,
                        DepthStencilAttachment = new AttachmentReference
                        {
                            Attachment = 1,
                            Layout = ImageLayout.DepthStencilAttachmentOptimal
                        },
                        //PreserveAttachmentCount = ,
                        //PreserveAttachments = 
                    }
                },
                Dependencies = new SubpassDependency[]
                {
                    new SubpassDependency
                    {
                        SrcSubpass = VK_SUBPASS_EXTERNAL,
                        DstSubpass = 0,
                        SrcStageMask = PipelineStageFlags.ColorAttachmentOutput,
                        DstStageMask = PipelineStageFlags.ColorAttachmentOutput,
                        SrcAccessMask = 0,
                        DstAccessMask = AccessFlags.ColorAttachmentRead | AccessFlags.ColorAttachmentWrite,
                        //DependencyFlags = 
                    }
                }
            };

            return _device.CreateRenderPass(renderPassInfo);
        }

        Format FindDepthFormat()
        {
            return FindSupportedFormat(new Format[] {Format.D32Sfloat, Format.D32SfloatS8Uint, Format.D24UnormS8Uint},
                ImageTiling.Optimal,
                FormatFeatureFlags.DepthStencilAttachment
        	);
        }

        Format FindSupportedFormat(IEnumerable<Format> candidates, ImageTiling tiling, FormatFeatureFlags features)
        {
            foreach (var format in candidates)
            {
                var props = _physicalDevice.GetFormatProperties(format);

                if (tiling == ImageTiling.Linear && props.LinearTilingFeatures.HasFlag(features))
                {
                    return format;
                }
                else if (tiling == ImageTiling.Optimal && props.OptimalTilingFeatures.HasFlag(features))
                {
                    return format;
                }
            }

            throw new VulkanException("failed to find supported format!");
        }

        void CreateDescriptorSetLayout()
        {
            _logger.LogInfo("Creating descriptor set layout...");

            var descriptorSetLayoutCreateInfo = new DescriptorSetLayoutCreateInfo
            {
                Bindings = new DescriptorSetLayoutBinding[]
                {
                    new DescriptorSetLayoutBinding
                    {
                        Binding = 0,
                        DescriptorType = DescriptorType.UniformBuffer,
                        DescriptorCount = 1,
                        StageFlags = ShaderStageFlags.Vertex,
                        ImmutableSamplers = null
                    },
                    new DescriptorSetLayoutBinding
                    {
                        Binding = 1,
                        DescriptorType = DescriptorType.CombinedImageSampler,
                        DescriptorCount = 1,
                        StageFlags = ShaderStageFlags.Fragment,
                        ImmutableSamplers = null
                    }
                }
            };

            _descriptorSetLayout = _device.CreateDescriptorSetLayout(descriptorSetLayoutCreateInfo);
        }

        async Task CreateGraphicsPipelineAsync()
        {
            _logger.LogInfo("Creating graphics pipeline...");

            var vertexShaderBytes = await File.ReadAllBytesAsync("shaders/vert.spv");
            var vertexShaderModule = _device.CreateShaderModule(vertexShaderBytes);

            var fragmentShaderBytes = await File.ReadAllBytesAsync("shaders/frag.spv");
            var fragmentShaderModule = _device.CreateShaderModule(fragmentShaderBytes);

            var pipelineShaderStateCreateInfos = new PipelineShaderStageCreateInfo[]
            {
                new PipelineShaderStageCreateInfo
                {
                    Stage = ShaderStageFlags.Vertex,
                    Module = vertexShaderModule,
                    Name = "main",
                    //SpecializationInfo = 
                },
                new PipelineShaderStageCreateInfo
                {
                    Stage = ShaderStageFlags.Fragment,
                    Module = fragmentShaderModule,
                    Name = "main",
                    //SpecializationInfo = 
                }
            };

            var vertexBindingDescription = Vertex.GetBindingDescription();
            var vertexAttributeDescriptions = Vertex.GetAttributeDescriptions();

            var pipelineVertexInputStateCreateInfo = new PipelineVertexInputStateCreateInfo
            {
                VertexBindingDescriptions = new VertexInputBindingDescription[] {vertexBindingDescription},
                VertexAttributeDescriptions = vertexAttributeDescriptions
            };

            var pipelineInputAssemblyStateCreateInfo = new PipelineInputAssemblyStateCreateInfo
            {
                Topology = PrimitiveTopology.TriangleList,
                PrimitiveRestartEnable = false
            };

            var pipelineViewportStateCreateInfo = new PipelineViewportStateCreateInfo
            {
                Viewports = new Viewport[]
                {
                    new Viewport
                    {
                        X = 0.0f,
                        Y = 0.0f,
                        Width = _swapChainExtent.Width,
                        Height = _swapChainExtent.Height,
                        MinDepth = 0.0f,
                        MaxDepth = 1.0f,
                    }
                },
                Scissors = new Rect2D[]
                {
                    new Rect2D
                    {
                        Offset = new Offset2D{ X = 0, Y = 0 },
                        Extent = _swapChainExtent
                    }
                }
            };

            var pipelineRasterizationStateCreateInfo = new PipelineRasterizationStateCreateInfo
            {
                DepthClampEnable = false,
                RasterizerDiscardEnable = false,
                PolygonMode = PolygonMode.Fill,
                CullMode = CullModeFlags.Back,
                FrontFace = FrontFace.CounterClockwise,
                DepthBiasEnable = false,
                //DepthBiasConstantFactor = ,
                //DepthBiasClamp = ,
                //DepthBiasSlopeFactor = ,
                LineWidth = 1.0f
            };

            var pipelineMultisampleStateCreateInfo = new PipelineMultisampleStateCreateInfo
            {
                RasterizationSamples = SampleCountFlags.Count1,
                SampleShadingEnable = false,
                // MinSampleShading = ,
                // SampleMask = ,
                // AlphaToCoverageEnable = ,
                // AlphaToOneEnable = 
            };

            var pipelineColorBlendStateCreateInfo = new PipelineColorBlendStateCreateInfo
            {
                LogicOpEnable = false,
                //LogicOp = ,
                Attachments = new PipelineColorBlendAttachmentState[]
                {
                    new PipelineColorBlendAttachmentState
                    {
                        BlendEnable = false,
                        // SrcColorBlendFactor = ,
                        // DstColorBlendFactor = ,
                        // ColorBlendOp = ,
                        // SrcAlphaBlendFactor = ,
                        // DstAlphaBlendFactor = ,
                        // AlphaBlendOp = ,
                        ColorWriteMask = ColorComponentFlags.R | ColorComponentFlags.G | ColorComponentFlags.B | ColorComponentFlags.A
                    }
                },
                //BlendConstants = 
            };

            var pipelineLayoutCreateInfo = new PipelineLayoutCreateInfo
            {
                SetLayouts = new DescriptorSetLayout[] { _descriptorSetLayout },
                PushConstantRanges = null
            };

            _logger.LogInfo("Creating graphics pipeline layout now");
            _graphicsPipelineLayout = _device.CreatePipelineLayout(pipelineLayoutCreateInfo);

            var pipelineDepthStencilStateCreateInfo = new PipelineDepthStencilStateCreateInfo
            {
                DepthTestEnable = true,
                DepthWriteEnable = true,
                DepthCompareOp = CompareOp.Less,
                DepthBoundsTestEnable = false,
                StencilTestEnable = false,
                //Front = ,
                //Back = ,
                MinDepthBounds = 0.0f,
                MaxDepthBounds = 1.0f
            };

            var graphicsPipelineCreateInfo = new GraphicsPipelineCreateInfo
            {
                Subpass = 0,
                RenderPass = _renderPass,
                Layout = _graphicsPipelineLayout,
                //DynamicState = ,
                ColorBlendState = pipelineColorBlendStateCreateInfo,
                DepthStencilState = pipelineDepthStencilStateCreateInfo,
                MultisampleState = pipelineMultisampleStateCreateInfo,
                BasePipelineHandle = null,
                RasterizationState = pipelineRasterizationStateCreateInfo,
                //TessellationState = ,
                InputAssemblyState = pipelineInputAssemblyStateCreateInfo,
                VertexInputState = pipelineVertexInputStateCreateInfo,
                Stages = pipelineShaderStateCreateInfos,
                ViewportState = pipelineViewportStateCreateInfo,
                BasePipelineIndex = -1
            };

            _logger.LogInfo("Creating graphics pipeline now");
            _graphicsPipeline = _device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[] {graphicsPipelineCreateInfo})[0];
            _logger.LogInfo("Graphics pipeline created");

            _device.DestroyShaderModule(fragmentShaderModule);
            _device.DestroyShaderModule(vertexShaderModule);
        }

        void CreateCommandPool()
        {
            _logger.LogInfo("Creating command pool...");

            var commandPoolCreateInfo = new CommandPoolCreateInfo
            {
                QueueFamilyIndex = _graphicsQueueFamilyIndex
            };

            _commandPool = _device.CreateCommandPool(commandPoolCreateInfo);
        }

        void CreateTempCommandPool()
        {
            _logger.LogInfo("Creating temp command pool...");

            var commandPoolCreateInfo = new CommandPoolCreateInfo
            {
                QueueFamilyIndex = _graphicsQueueFamilyIndex,
                Flags = CommandPoolCreateFlags.Transient
            };

            _tempCommandPool = _device.CreateCommandPool(commandPoolCreateInfo);
        }

        ~Vulkan()
        {
            Dispose();
        }

        public void Dispose()
        {
            CleanupSwapChain();

            // vkDestroySampler(device, textureSampler, nullptr);
            // vkDestroyImageView(device, textureImageView, nullptr);

            // vkDestroyImage(device, textureImage, nullptr);
            // vkFreeMemory(device, textureImageMemory, nullptr);

            // vkDestroyDescriptorPool(device, descriptorPool, nullptr);

            // vkDestroyDescriptorSetLayout(device, descriptorSetLayout, nullptr);
            // vkDestroyBuffer(device, uniformBuffer, nullptr);
            // vkFreeMemory(device, uniformBufferMemory, nullptr);

            // vkDestroyBuffer(device, indexBuffer, nullptr);
            // vkFreeMemory(device, indexBufferMemory, nullptr);

            // vkDestroyBuffer(device, vertexBuffer, nullptr);
            // vkFreeMemory(device, vertexBufferMemory, nullptr);

            // vkDestroySemaphore(device, renderFinishedSemaphore, nullptr);
            // vkDestroySemaphore(device, imageAvailableSemaphore, nullptr);


            _device.DestroyCommandPool(_commandPool);
            _device.DestroyCommandPool(_tempCommandPool);

            _device.DestroyDescriptorSetLayout(_descriptorSetLayout);
            _device.Destroy();
            //_instance.DestroyDebugReportCallbackEXT();
            _instance.DestroySurfaceKHR(_surface);
            _instance.Dispose();
        }

        void CleanupSwapChain()
        {
            //vkDestroyImageView(device, depthImageView, nullptr);
            //vkDestroyImage(device, depthImage, nullptr);
            //vkFreeMemory(device, depthImageMemory, nullptr);

            // for (size_t i = 0; i < swapChainFramebuffers.size(); i++)
            // {
            //     vkDestroyFramebuffer(device, swapChainFramebuffers[i], nullptr);
            // }

            // vkFreeCommandBuffers(device, commandPool, static_cast<uint32_t>(commandBuffers.size()), commandBuffers.data());

            _device.DestroyPipeline(_graphicsPipeline);
            _device.DestroyPipelineLayout(_graphicsPipelineLayout);

            _device.DestroyRenderPass(_renderPass);

            foreach (var swapChainImageView in _swapChainImageViews)
            {
                _device.DestroyImageView(swapChainImageView);
            }

            _device.DestroySwapchainKHR(_swapChain);
        }
    }
}