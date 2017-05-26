using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BundtCommon;
using Newtonsoft.Json;
using Vulkan;
using Vulkan.Windows;
using VkBuffer = Vulkan.Buffer;

namespace BundtCake
{
    class Vulkan
    {
        const uint VK_SUBPASS_EXTERNAL = ~0U;
        const uint VK_QUEUE_FAMILY_IGNORED = ~0U;
        const string TEXTURE_PATH = "textures/a-button.png";

        static readonly MyLogger _logger = new MyLogger(nameof(Vulkan));
        static readonly string[] _requiredInstanceExtensions = new string[] { "VK_EXT_debug_report", "VK_KHR_win32_surface", "VK_KHR_surface" };
        static readonly string[] _requiredPhysicalDeviceExtensions = new string[] { "VK_KHR_swapchain" };

        Instance _instance;
        SurfaceKhr _surface;
        PhysicalDevice _physicalDevice;
        Device _device;
        Queue _graphicsQueue;
        Queue _presentQueue;
        Format _swapChainImageFormat;
        SwapchainKhr _swapChain;
        Extent2D _swapChainExtent;
        List<ImageView> _swapChainImageViews;
        RenderPass _renderPass;
        DescriptorSetLayout _descriptorSetLayout;
        PipelineLayout _graphicsPipelineLayout;
        Pipeline _graphicsPipeline;
        CommandPool _commandPool;
        CommandPool _tempCommandPool;
        Image _textureImage;
        DeviceMemory _textureImageMemory;
        ImageView _textureImageView;
        Sampler _textureSampler;
        ImageView _depthImageView;
        Image _depthImage;
        DeviceMemory _depthImageMemory;
        List<Framebuffer> _swapChainFramebuffers;
        //VkBuffer _vertexBuffer;
        Dictionary<int, VkBuffer> _vertexBuffers = new Dictionary<int, VkBuffer>();
        //DeviceMemory _vertexBufferMemory;
        Dictionary<int, DeviceMemory> _vertexBufferMemories = new Dictionary<int, DeviceMemory>();
        //VkBuffer _indexBuffer;
        Dictionary<int, VkBuffer> _indexBuffers = new Dictionary<int, VkBuffer>();
        //DeviceMemory _indexBufferMemory;
        Dictionary<int, DeviceMemory> _indexBufferMemories = new Dictionary<int, DeviceMemory>();
        Dictionary<int, VkBuffer> _uniformBuffers = new Dictionary<int, VkBuffer>();
        Dictionary<int, DeviceMemory> _uniformBufferMemories = new Dictionary<int, DeviceMemory>();
        DescriptorPool _descriptorPool;
        Dictionary<int, DescriptorSet> _descriptorSets = new Dictionary<int, DescriptorSet>();
        CommandBuffer[] _drawCommandBuffers;
        Semaphore _imageAvailableSemaphore;
        Semaphore _renderFinishedSemaphore;
        Image[] _swapChainImages;

        Window _window;

        uint _graphicsQueueFamilyIndex;
        uint _presentQueueFamilyIndex;

        bool _isDisposed;

        //GameObject _gameObject;
        Dictionary<int, GameObject> _gameObjects = new Dictionary<int, GameObject>();
        Camera _mainCamera;

        public void Initialize(Window window, IEnumerable<GameObject> gameObjects, Camera mainCamera)
        {
            _window = window;
            foreach (var gameObject in gameObjects)
            {
                _gameObjects[gameObject.Id] = gameObject;
            }
            _mainCamera = mainCamera;

            _logger.LogInfo("Initializing Vulkan...");

            VulkanPrinter.PrintAvailableInstanceLayers();
            VulkanPrinter.PrintAvailableInstanceExtensions();

            _instance = CreateInstance();

            SetupDebugCallback();

            _surface = CreateWin32Surface();

            var physicalDevices = GetPhysicalDevices();

            var requiredPhysicalDeviceFeatures = GetRequiredFeatures();

            var suitablePhysicalDevices = _instance.GetSuitablePhysicalDevices(_requiredPhysicalDeviceExtensions, requiredPhysicalDeviceFeatures, QueueFlags.Graphics, _surface);

            _physicalDevice = ChooseBestPhysicalDevice(physicalDevices);

            _physicalDevice.PrintQueueFamilies();

            _graphicsQueueFamilyIndex = _physicalDevice.GetIndexOfFirstAvailableGraphicsQueueFamily();
            _presentQueueFamilyIndex = _physicalDevice.GetIndexOfFirstAvailablePresentQueueFamily(_surface);

            _device = CreateLogicalDevice(requiredPhysicalDeviceFeatures);

            CreateSwapchain();
            CreateSwapChainImageViews();

            CreateRenderPass();

            CreateDescriptorSetLayout();

            CreateGraphicsPipeline();

            CreateCommandPool();
            CreateTempCommandPool();

            CreateDepthResources();

            CreateFramebuffers();

            CreateTextureImage();
            CreateTextureImageView();
            CreateTextureSampler();

            // // loadModel();

            CreateVertexBuffers();
            CreateIndexBuffers();
            CreateUniformBuffer();

            CreateDescriptorPool();
            CreateDescriptorSets();

            CreateCommandBuffers();

            CreateSemaphores();

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
            else if (flags.HasFlag(DebugReportFlagsExt.PerformanceWarning))
            {
                _logger.LogWarning("Validation layer " + flags + ": " + Marshal.PtrToStringUTF8(message));
            }
            else
            {
                _logger.LogInfo("Validation layer " + flags + ": " + Marshal.PtrToStringUTF8(message));
            }
            return false;
        }

        SurfaceKhr CreateWin32Surface()
        {
            _logger.LogDebug("Creating Win32 surface...");

            var win32SurfaceCreateInfoKhr = new Win32SurfaceCreateInfoKhr
            {
                Hinstance = _window.Win32hInstance,
                Hwnd = _window.Win32hWnd
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

            _graphicsQueue = device.GetQueue(_graphicsQueueFamilyIndex, 0);
            _presentQueue = device.GetQueue(_presentQueueFamilyIndex, 0);

            return device;
        }

        void CreateSwapchain()
        {
            _logger.LogInfo("Creating swap chain...");

            var surfaceCapabilities = _physicalDevice.GetSurfaceCapabilitiesKHR(_surface);

            _logger.LogInfo("surfaceCapabilities:");
            _logger.LogInfo("\tMinImageCount: " + surfaceCapabilities.MinImageCount);
            _logger.LogInfo("\tMaxImageCount: " + surfaceCapabilities.MaxImageCount);

            _swapChainExtent = ChooseSwapExtent(surfaceCapabilities, _window);

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
                swapchainCreateInfo.QueueFamilyIndices = new uint[] { _graphicsQueueFamilyIndex, _presentQueueFamilyIndex };
            }
            else
            {
                _logger.LogInfo("Swap chain image sharing mode: Exclusive");
                swapchainCreateInfo.ImageSharingMode = SharingMode.Exclusive;
                swapchainCreateInfo.QueueFamilyIndexCount = 0;
                swapchainCreateInfo.QueueFamilyIndices = null;
            }

            _swapChain = _device.CreateSwapchainKHR(swapchainCreateInfo);

            _swapChainImages = _device.GetSwapchainImagesKHR(_swapChain);
        }

        void RecreateSwapchain()
        {
            _device.WaitIdle();

            CleanupSwapchain();

            CreateSwapchain();
            CreateSwapChainImageViews();
            CreateRenderPass();
            CreateGraphicsPipeline();
            CreateDepthResources();
            CreateFramebuffers();
            CreateCommandBuffers();
        }

        static SurfaceFormatKhr ChooseSwapSurfaceFormat(IEnumerable<SurfaceFormatKhr> availableFormats)
        {
            if (availableFormats.Count() == 1 && availableFormats.First().Format == Format.Undefined)
            {
                return new SurfaceFormatKhr { Format = Format.B8G8R8A8Unorm, ColorSpace = ColorSpaceKhr.SrgbNonlinear };
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

        void CreateSwapChainImageViews()
        {
            _logger.LogInfo("Creating swap chain image views...");

            _swapChainImageViews = new List<ImageView>();

            for (var i = 0; i < _swapChainImages.Count(); i++)
            {
                _swapChainImageViews.Add(CreateImageView(_swapChainImages[i], _swapChainImageFormat, ImageAspectFlags.Color));
            }
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

        void CreateRenderPass()
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
                        //InputAttachments = ,
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

            _renderPass = _device.CreateRenderPass(renderPassInfo);
        }

        Format FindDepthFormat()
        {
            return FindSupportedFormat(new Format[] { Format.D32Sfloat, Format.D32SfloatS8Uint, Format.D24UnormS8Uint },
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
                    },
                    new DescriptorSetLayoutBinding
                    {
                        Binding = 1,
                        DescriptorType = DescriptorType.CombinedImageSampler,
                        DescriptorCount = 1,
                        StageFlags = ShaderStageFlags.Fragment,
                    }
                }
            };

            _descriptorSetLayout = _device.CreateDescriptorSetLayout(descriptorSetLayoutCreateInfo);
        }

        void CreateGraphicsPipeline()
        {
            _logger.LogInfo("Creating graphics pipeline...");

            var vertexShaderBytes = File.ReadAllBytes("shaders/vert.spv");
            var vertexShaderModule = _device.CreateShaderModule(vertexShaderBytes);

            var fragmentShaderBytes = File.ReadAllBytes("shaders/frag.spv");
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
                VertexBindingDescriptions = new VertexInputBindingDescription[] { vertexBindingDescription },
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
                CullMode = CullModeFlags.None,
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
            _graphicsPipeline = _device.CreateGraphicsPipelines(null, new GraphicsPipelineCreateInfo[] { graphicsPipelineCreateInfo })[0];
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

        void CreateDepthResources()
        {
            var depthFormat = FindDepthFormat();
            CreateImage(_swapChainExtent.Width, _swapChainExtent.Height, depthFormat, ImageTiling.Optimal, ImageUsageFlags.DepthStencilAttachment, MemoryPropertyFlags.DeviceLocal, out _depthImage, out _depthImageMemory);

            _depthImageView = createImageView(_depthImage, depthFormat, ImageAspectFlags.Depth);

            TransitionImageLayout(_depthImage, depthFormat, ImageLayout.Undefined, ImageLayout.DepthStencilAttachmentOptimal);
        }

        static bool HasStencilComponent(Format format)
        {
            return format == Format.D32SfloatS8Uint || format == Format.D24UnormS8Uint;
        }

        void CreateImage(uint width, uint height, Format format, ImageTiling tiling, ImageUsageFlags usage, MemoryPropertyFlags properties, out Image image, out DeviceMemory imageMemory)
        {
            var imageCreateInfo = new ImageCreateInfo
            {
                ImageType = ImageType.Image2D,
                Format = format,
                Extent = new Extent3D
                {
                    Width = width,
                    Height = height,
                    Depth = 1
                },
                MipLevels = 1,
                ArrayLayers = 1,
                Samples = SampleCountFlags.Count1,
                Tiling = tiling,
                Usage = usage,
                SharingMode = SharingMode.Exclusive,
                //QueueFamilyIndexCount = ,
                //QueueFamilyIndices = ,
                InitialLayout = ImageLayout.Preinitialized
            };

            image = _device.CreateImage(imageCreateInfo);

            var imageMemoryRequirements = _device.GetImageMemoryRequirements(image);

            var memoryAllocateInfo = new MemoryAllocateInfo
            {
                AllocationSize = imageMemoryRequirements.Size,
                MemoryTypeIndex = FindMemoryType(imageMemoryRequirements.MemoryTypeBits, properties)
            };

            imageMemory = _device.AllocateMemory(memoryAllocateInfo);

            _device.BindImageMemory(image, imageMemory, 0);
        }

        uint FindMemoryType(uint typeFilter, MemoryPropertyFlags properties)
        {
            var memoryProperties = _physicalDevice.GetMemoryProperties();

            for (int i = 0; i < memoryProperties.MemoryTypeCount; i++)
            {
                if ((typeFilter & (1 << i)) != 0 && memoryProperties.MemoryTypes[i].PropertyFlags.HasFlag(properties))
                {
                    return (uint)i;
                }
            }

            throw new VulkanException("failed to find suitable memory type!");
        }

        ImageView createImageView(Image image, Format format, ImageAspectFlags aspectFlags)
        {
            var imageViewCreateInfo = new ImageViewCreateInfo
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

            return _device.CreateImageView(imageViewCreateInfo);
        }

        void TransitionImageLayout(Image image, Format format, ImageLayout oldLayout, ImageLayout newLayout)
        {
            CommandBuffer commandBuffer = BeginSingleTimeCommands();

            var imageMemoryBarrier = new ImageMemoryBarrier
            {
                //SrcAccessMask = ,
                //DstAccessMask = ,
                OldLayout = oldLayout,
                NewLayout = newLayout,
                SrcQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                DstQueueFamilyIndex = VK_QUEUE_FAMILY_IGNORED,
                Image = image,
                SubresourceRange = new ImageSubresourceRange
                {
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            if (newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                if (HasStencilComponent(format))
                {
                    imageMemoryBarrier.SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.Depth | ImageAspectFlags.Stencil,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1
                    };
                }
                else
                {
                    imageMemoryBarrier.SubresourceRange = new ImageSubresourceRange
                    {
                        AspectMask = ImageAspectFlags.Depth,
                        BaseMipLevel = 0,
                        LevelCount = 1,
                        BaseArrayLayer = 0,
                        LayerCount = 1
                    };
                }
            }
            else
            {
                imageMemoryBarrier.SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.Color,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                };
            }

            if (oldLayout == ImageLayout.Preinitialized && newLayout == ImageLayout.TransferSrcOptimal)
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.HostWrite;
                imageMemoryBarrier.DstAccessMask = AccessFlags.TransferRead;
            }
            else if (oldLayout == ImageLayout.Preinitialized && newLayout == ImageLayout.TransferDstOptimal)
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.HostWrite;
                imageMemoryBarrier.DstAccessMask = AccessFlags.TransferWrite;
            }
            else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
            {
                imageMemoryBarrier.SrcAccessMask = AccessFlags.TransferWrite;
                imageMemoryBarrier.DstAccessMask = AccessFlags.ShaderRead;
            }
            else if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.DepthStencilAttachmentOptimal)
            {
                imageMemoryBarrier.SrcAccessMask = 0;
                imageMemoryBarrier.DstAccessMask = AccessFlags.DepthStencilAttachmentRead | AccessFlags.DepthStencilAttachmentWrite;
            }
            else
            {
                throw new VulkanException("unsupported layout transition!");
            }

            commandBuffer.CmdPipelineBarrier(
                PipelineStageFlags.TopOfPipe, PipelineStageFlags.TopOfPipe,
                0,
                null,
                null,
                imageMemoryBarrier
            );

            EndSingleTimeCommands(commandBuffer);
        }

        void CreateTextureImage()
        {
            //var pixels = stbi_load(TEXTURE_PATH, &texWidth, &texHeight, &texChannels, STBI_rgb_alpha);
            var imagedata = ImageSharp.Image.Load(TEXTURE_PATH);
            DeviceSize imageSize = imagedata.Width * imagedata.Height * 4;

            _logger.LogInfo("loaded texWidth: " + imagedata.Width);
            _logger.LogInfo("loaded texHeight: " + imagedata.Height);
            //_logger.LogInfo("loaded texChannels: " + imagedata.);
            _logger.LogInfo("loaded imagedata.Pixels.Length: " + imagedata.Pixels.Length);

            if (imagedata == null)
            {
                throw new VulkanException("failed to load texture image!");
            }

            VkBuffer stagingBuffer;
            DeviceMemory stagingBufferMemory;

            CreateBuffer(imageSize, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingBuffer, out stagingBufferMemory);

            var pixelsBytes = new List<byte>();

            foreach (var pixel in imagedata.Pixels)
            {
                pixelsBytes.Add(pixel.R);
                pixelsBytes.Add(pixel.G);
                pixelsBytes.Add(pixel.B);
                pixelsBytes.Add(pixel.A);
            }

            _device.CopyToBufferMemory(pixelsBytes.ToArray(), stagingBufferMemory, 0, imageSize, 0);

            CreateImage((uint)imagedata.Width, (uint)imagedata.Height, Format.R8G8B8A8Unorm, ImageTiling.Optimal, ImageUsageFlags.TransferDst | ImageUsageFlags.Sampled, MemoryPropertyFlags.DeviceLocal, out _textureImage, out _textureImageMemory);

            TransitionImageLayout(_textureImage, Format.R8G8B8A8Unorm, ImageLayout.Preinitialized, ImageLayout.TransferDstOptimal);
            CopyBufferToImage(stagingBuffer, _textureImage, (uint)imagedata.Width, (uint)imagedata.Height);
            TransitionImageLayout(_textureImage, Format.R8G8B8A8Unorm, ImageLayout.TransferDstOptimal, ImageLayout.ShaderReadOnlyOptimal);

            imagedata.Dispose();
            _device.DestroyBuffer(stagingBuffer);
            _device.FreeMemory(stagingBufferMemory);
        }

        void CreateTextureImageView()
        {
            _textureImageView = createImageView(_textureImage, Format.R8G8B8A8Unorm, ImageAspectFlags.Color);
        }

        void CreateTextureSampler()
        {
            var samplerCreateInfo = new SamplerCreateInfo
            {
                MipLodBias = 0.0f,
                MaxLod = 0.0f,
                MinLod = 0.0f,
                CompareOp = CompareOp.Always,
                CompareEnable = false,
                MaxAnisotropy = 16,
                AnisotropyEnable = true,
                BorderColor = BorderColor.IntOpaqueBlack,
                AddressModeU = SamplerAddressMode.Repeat,
                AddressModeV = SamplerAddressMode.Repeat,
                AddressModeW = SamplerAddressMode.Repeat,
                MipmapMode = SamplerMipmapMode.Linear,
                MinFilter = Filter.Linear,
                MagFilter = Filter.Linear,
                UnnormalizedCoordinates = false
            };

            _textureSampler = _device.CreateSampler(samplerCreateInfo);
        }

        CommandBuffer BeginSingleTimeCommands()
        {
            var commandBufferAllocateInfo = new CommandBufferAllocateInfo
            {
                CommandPool = _tempCommandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = 1
            };

            var commandBuffer = _device.AllocateCommandBuffers(commandBufferAllocateInfo)[0];

            commandBuffer.Begin(new CommandBufferBeginInfo
            {
                Flags = CommandBufferUsageFlags.OneTimeSubmit,
                //InheritanceInfo = 
            });

            return commandBuffer;
        }

        void EndSingleTimeCommands(CommandBuffer commandBuffer)
        {
            commandBuffer.End();

            _graphicsQueue.Submit(new SubmitInfo
            {
                CommandBuffers = new CommandBuffer[] { commandBuffer },
            });

            _graphicsQueue.WaitIdle();

            _device.FreeCommandBuffer(_tempCommandPool, commandBuffer);
        }

        void CreateBuffer(DeviceSize size, BufferUsageFlags usage, MemoryPropertyFlags properties, out VkBuffer buffer, out DeviceMemory bufferMemory)
        {
            var bufferCreateInfo = new BufferCreateInfo
            {
                Size = size,
                Usage = usage,
                SharingMode = SharingMode.Exclusive,
                //QueueFamilyIndexCount = ,
                //QueueFamilyIndices = 
            };

            buffer = _device.CreateBuffer(bufferCreateInfo);

            var memRequirements = _device.GetBufferMemoryRequirements(buffer);

            var memoryAllocateInfo = new MemoryAllocateInfo
            {
                AllocationSize = memRequirements.Size,
                MemoryTypeIndex = FindMemoryType(memRequirements.MemoryTypeBits, properties)
            };

            bufferMemory = _device.AllocateMemory(memoryAllocateInfo);

            _device.BindBufferMemory(buffer, bufferMemory, 0);
        }

        void CopyBufferToImage(VkBuffer buffer, Image image, uint width, uint height)
        {
            var commandBuffer = BeginSingleTimeCommands();

            var region = new BufferImageCopy
            {
                BufferOffset = 0,
                BufferRowLength = 0,
                BufferImageHeight = 0,
                ImageSubresource = new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.Color,
                    MipLevel = 0,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                },
                ImageOffset = new Offset3D
                {
                    X = 0,
                    Y = 0,
                    Z = 0
                },
                ImageExtent = new Extent3D
                {
                    Width = width,
                    Height = height,
                    Depth = 1
                }
            };

            commandBuffer.CmdCopyBufferToImage(
                buffer,
                image,
                ImageLayout.TransferDstOptimal,
                new BufferImageCopy[] { region }
            );

            EndSingleTimeCommands(commandBuffer);
        }

        void CreateFramebuffers()
        {
            _logger.LogInfo("Creating frame buffers...");

            _swapChainFramebuffers = new List<Framebuffer>();

            for (var i = 0; i < _swapChainImageViews.Count; i++)
            {
                var attachments = new ImageView[]
                {
                    _swapChainImageViews[i], _depthImageView
                };

                var framebufferCreateInfo = new FramebufferCreateInfo
                {
                    RenderPass = _renderPass,
                    Attachments = attachments,
                    Width = _swapChainExtent.Width,
                    Height = _swapChainExtent.Height,
                    Layers = 1
                };

                _swapChainFramebuffers.Add(_device.CreateFramebuffer(framebufferCreateInfo));
            }
        }

        void CreateVertexBuffers()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                var bufferSize = Marshal.SizeOf(gameObject.Mesh.Vertices[0]) * gameObject.Mesh.Vertices.Count;
                VkBuffer stagingBuffer;
                DeviceMemory stagingBufferMemory;
                CreateBuffer(bufferSize, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingBuffer, out stagingBufferMemory);

                _device.CopyToBufferMemory(Vertex.VertexArrayToByteArray(gameObject.Mesh.Vertices).ToArray(), stagingBufferMemory, 0, bufferSize, 0);

                VkBuffer vertexBuffer;
                DeviceMemory vertexBufferMemory;

                CreateBuffer(bufferSize, BufferUsageFlags.TransferDst | BufferUsageFlags.VertexBuffer, MemoryPropertyFlags.DeviceLocal, out vertexBuffer, out vertexBufferMemory);

                CopyBuffer(stagingBuffer, vertexBuffer, bufferSize);

                _vertexBuffers[gameObject.Id] = vertexBuffer;
                _vertexBufferMemories[gameObject.Id] = vertexBufferMemory;

                _device.DestroyBuffer(stagingBuffer);
                _device.FreeMemory(stagingBufferMemory);
            }
        }

        void CreateIndexBuffers()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                var bufferSize = Marshal.SizeOf(gameObject.Mesh.Indices[0]) * gameObject.Mesh.Indices.Length;

                VkBuffer stagingBuffer;
                DeviceMemory stagingBufferMemory;
                CreateBuffer(bufferSize, BufferUsageFlags.TransferSrc, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out stagingBuffer, out stagingBufferMemory);

                var indicesBytes = new List<byte>();

                foreach (var index in gameObject.Mesh.Indices)
                {
                    foreach (var b in BitConverter.GetBytes(index))
                    {
                        indicesBytes.Add(b);
                    }
                }

                _device.CopyToBufferMemory(indicesBytes.ToArray(), stagingBufferMemory, 0, bufferSize, 0);

                VkBuffer indexBuffer;
                DeviceMemory indexBufferMemory;

                CreateBuffer(bufferSize, BufferUsageFlags.TransferDst | BufferUsageFlags.IndexBuffer, MemoryPropertyFlags.DeviceLocal, out indexBuffer, out indexBufferMemory);

                CopyBuffer(stagingBuffer, indexBuffer, bufferSize);

                _indexBuffers[gameObject.Id] = indexBuffer;
                _indexBufferMemories[gameObject.Id] = indexBufferMemory;

                _device.DestroyBuffer(stagingBuffer);
                _device.FreeMemory(stagingBufferMemory);
            }
        }

        void CreateUniformBuffer()
        {
            var bufferSize = UniformBufferObject.GetSizeInBytes();
            foreach (var index in _gameObjects.Keys)
            {
                VkBuffer uniformBuffer;
                DeviceMemory uniformBufferMemory;

                CreateBuffer(bufferSize, BufferUsageFlags.UniformBuffer, MemoryPropertyFlags.HostVisible | MemoryPropertyFlags.HostCoherent, out uniformBuffer, out uniformBufferMemory);

                _uniformBuffers[index] = uniformBuffer;
                _uniformBufferMemories[index] = uniformBufferMemory;
            }
        }

        void CopyBuffer(VkBuffer srcBuffer, VkBuffer dstBuffer, DeviceSize size)
        {
            var commandBuffer = BeginSingleTimeCommands();

            var copyRegion = new BufferCopy
            {
                Size = size
            };

            commandBuffer.CmdCopyBuffer(srcBuffer, dstBuffer, new BufferCopy[] { copyRegion });

            EndSingleTimeCommands(commandBuffer);
        }

        void CreateDescriptorPool()
        {
            var poolSizes = new DescriptorPoolSize[2];

            poolSizes[0].Type = DescriptorType.UniformBuffer;
            poolSizes[0].DescriptorCount = (uint)_gameObjects.Count;

            poolSizes[1].Type = DescriptorType.CombinedImageSampler;
            poolSizes[1].DescriptorCount = (uint)_gameObjects.Count;

            var poolInfo = new DescriptorPoolCreateInfo
            {
                Flags = 0,
                MaxSets = (uint)_gameObjects.Count,
                PoolSizes = poolSizes
            };

            _descriptorPool = _device.CreateDescriptorPool(poolInfo);
        }

        void CreateDescriptorSets()
        {
            foreach (var index in _gameObjects.Keys)
            {
                _descriptorSets[index] = CreateDescriptorSet(_descriptorSetLayout,
                    _descriptorPool,
                    _uniformBuffers[index],
                    _textureSampler,
                    _textureImageView);
            }
        }

        DescriptorSet CreateDescriptorSet(DescriptorSetLayout descriptorSetLayout,
            DescriptorPool descriptorPool,
            VkBuffer uniformBuffer,
            Sampler textureSampler,
            ImageView textureImageView)
        {
            var allocInfo = new DescriptorSetAllocateInfo
            {
                DescriptorPool = descriptorPool,
                SetLayouts = new DescriptorSetLayout[] { descriptorSetLayout }
            };

            var descriptorSet = _device.AllocateDescriptorSets(allocInfo)[0];

            var bufferInfo = new DescriptorBufferInfo
            {
                Buffer = uniformBuffer,
                Offset = 0,
                Range = UniformBufferObject.GetSizeInBytes()
            };

            var imageInfo = new DescriptorImageInfo
            {
                Sampler = textureSampler,
                ImageView = textureImageView,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal
            };

            var descriptorWrites = new WriteDescriptorSet[]
            {
                new WriteDescriptorSet
                {
                    DstSet = descriptorSet,
                    DstBinding = 0,
                    DstArrayElement = 0,
                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.UniformBuffer,
                    BufferInfo = new DescriptorBufferInfo[] {bufferInfo}
                },
                new WriteDescriptorSet
                {
                    DstSet = descriptorSet,
                    DstBinding = 1,
                    DstArrayElement = 0,
                    DescriptorCount = 1,
                    DescriptorType = DescriptorType.CombinedImageSampler,
                    ImageInfo = new DescriptorImageInfo[] {imageInfo}
                }
            };

            _device.UpdateDescriptorSets(descriptorWrites, null);

            return descriptorSet;
        }

        void CreateCommandBuffers()
        {
            _logger.LogInfo("Creating command buffers...");

            var allocInfo = new CommandBufferAllocateInfo
            {
                CommandPool = _commandPool,
                Level = CommandBufferLevel.Primary,
                CommandBufferCount = (uint)_swapChainFramebuffers.Count
            };

            _drawCommandBuffers = _device.AllocateCommandBuffers(allocInfo);

            for (var i = 0; i < _drawCommandBuffers.Length; i++)
            {
                var beginInfo = new CommandBufferBeginInfo
                {
                    Flags = CommandBufferUsageFlags.SimultaneousUse
                };

                _drawCommandBuffers[i].Begin(beginInfo);

                var clearValues = new ClearValue[] { new ClearValue(), new ClearValue() };
                clearValues[0].Color = new ClearColorValue(new float[] { 0.0f, 0.0f, 0.0f, 1.0f });
                clearValues[1].DepthStencil = new ClearDepthStencilValue { Depth = 1.0f, Stencil = 0 };

                var renderPassInfo = new RenderPassBeginInfo
                {
                    RenderPass = _renderPass,
                    Framebuffer = _swapChainFramebuffers[i],
                    RenderArea = new Rect2D
                    {
                        Offset = new Offset2D { X = 0, Y = 0 },
                        Extent = _swapChainExtent
                    },
                    ClearValues = clearValues
                };

                _drawCommandBuffers[i].CmdBeginRenderPass(renderPassInfo, SubpassContents.Inline);

                _drawCommandBuffers[i].CmdBindPipeline(PipelineBindPoint.Graphics, _graphicsPipeline);

                foreach (var gameObject in _gameObjects.Values)
                {
                    _drawCommandBuffers[i].CmdBindVertexBuffers(0, new VkBuffer[] { _vertexBuffers[gameObject.Id] }, new DeviceSize[] { 0 });
                    _drawCommandBuffers[i].CmdBindIndexBuffer(_indexBuffers[gameObject.Id], 0, IndexType.Uint32);
                    // TODO Should have one descriptor set per object, each having its own uniformbuffer
                    _drawCommandBuffers[i].CmdBindDescriptorSets(PipelineBindPoint.Graphics, _graphicsPipelineLayout, 0, new DescriptorSet[] { _descriptorSets[gameObject.Id] }, null);
                    _drawCommandBuffers[i].CmdDrawIndexed((uint)gameObject.Mesh.Indices.Length, 1, 0, 0, 0);
                }

                _drawCommandBuffers[i].CmdEndRenderPass();

                _drawCommandBuffers[i].End();
            }
        }

        void CreateSemaphores()
        {
            var semaphoreInfo = new SemaphoreCreateInfo();

            _imageAvailableSemaphore = _device.CreateSemaphore(semaphoreInfo);
            _renderFinishedSemaphore = _device.CreateSemaphore(semaphoreInfo);
        }

        DateTime _startTime = DateTime.Now;

        void UpdateUniformBuffer(GameObject gameObject, Camera mainCamera, DeviceMemory uniformBufferMemory, uint screenWidth, uint screenHeight)
        {
            var uniformBufferObject = new UniformBufferObject
            {
                Model = gameObject.Transform.CreateModelMatrix(),
                View = mainCamera.CreateViewMatrix(),
                Projection = mainCamera.CreateProjectionMatrix(screenWidth, screenHeight)
            };

            _device.CopyToBufferMemory(uniformBufferObject.GetBytes().ToArray(), uniformBufferMemory, 0, uniformBufferObject.GetBytes().ToArray().Length, 0);
        }

        public void OnWindowResized()
        {
            if (_window.GetSize().Width == 0 || _window.GetSize().Height == 0) return;

            _device.WaitIdle();

            RecreateSwapchain();
        }

        public void DrawFrame()
        {
            UpdateUniformBuffersForEachGameObject();

            var swapChainImageIndex = TryToAcquireNextSwapChainImage();
            if (swapChainImageIndex == uint.MaxValue) return;

            var submitInfo = CreateSubmitInfo(_imageAvailableSemaphore,
                PipelineStageFlags.ColorAttachmentOutput,
                _drawCommandBuffers[swapChainImageIndex],
                _renderFinishedSemaphore);

            _graphicsQueue.Submit(submitInfo);

            var presentInfo = CreatePresentInfo(_renderFinishedSemaphore, _swapChain, swapChainImageIndex);

            TryToPresent(presentInfo);
        }

        void UpdateUniformBuffersForEachGameObject()
        {
            foreach (var gameObject in _gameObjects.Values)
            {
                UpdateUniformBuffer(gameObject, _mainCamera, _uniformBufferMemories[gameObject.Id], _swapChainExtent.Width, _swapChainExtent.Height);
            }
        }

        uint TryToAcquireNextSwapChainImage()
        {
            try
            {
                return _device.AcquireNextImageKHR(_swapChain, ulong.MaxValue, _imageAvailableSemaphore, null);
            }
            catch (ResultException re)
            {
                if (re.Result.HasFlag(Result.ErrorOutOfDateKhr))
                {
                    RecreateSwapchain();
                    return uint.MaxValue;
                }
                throw;
            }
        }

        SubmitInfo CreateSubmitInfo(Semaphore waitSemaphore, PipelineStageFlags waitDstStageMask, CommandBuffer commandbuffer, Semaphore signalSemaphore)
        {
            return new SubmitInfo
            {
                WaitSemaphores = new Semaphore[] { waitSemaphore },
                WaitDstStageMask = new PipelineStageFlags[] { waitDstStageMask },
                CommandBuffers = new CommandBuffer[] { commandbuffer },
                SignalSemaphores = new Semaphore[] { signalSemaphore }
            };
        }

        PresentInfoKhr CreatePresentInfo(Semaphore waitSemaphore, SwapchainKhr swapchain, uint imageIndex)
        {
            return new PresentInfoKhr
            {
                WaitSemaphores = new Semaphore[] { waitSemaphore },
                Swapchains = new SwapchainKhr[] { _swapChain },
                ImageIndices = new uint[] { imageIndex }
            };
        }

        void TryToPresent(PresentInfoKhr presentInfo)
        {
            try
            {
                _presentQueue.PresentKHR(presentInfo);
            }
            catch (ResultException re)
            {
                if (re.Result.HasFlag(Result.ErrorOutOfDateKhr) || re.Result.HasFlag(Result.SuboptimalKhr))
                {
                    RecreateSwapchain();
                }
                else
                {
                    throw;
                }
            }
        }

        ~Vulkan()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _device.WaitIdle();

            CleanupSwapchain();

            _device.DestroySampler(_textureSampler);
            _device.DestroyImageView(_textureImageView);

            _device.DestroyImage(_textureImage);
            _device.FreeMemory(_textureImageMemory);

            _device.DestroyDescriptorPool(_descriptorPool);

            _device.DestroyDescriptorSetLayout(_descriptorSetLayout);

            foreach (var index in _gameObjects.Keys)
            {
                _device.DestroyBuffer(_uniformBuffers[index]);
                _device.FreeMemory(_uniformBufferMemories[index]);
            }

            foreach (var gameObject in _gameObjects.Values)
            {
                _device.DestroyBuffer(_indexBuffers[gameObject.Id]);
                _device.FreeMemory(_indexBufferMemories[gameObject.Id]);

                _device.DestroyBuffer(_vertexBuffers[gameObject.Id]);
                _device.FreeMemory(_vertexBufferMemories[gameObject.Id]);
            }

            _device.DestroySemaphore(_renderFinishedSemaphore);
            _device.DestroySemaphore(_imageAvailableSemaphore);

            _device.DestroyCommandPool(_commandPool);
            _device.DestroyCommandPool(_tempCommandPool);

            _device.Destroy();
            //_instance.DestroyDebugReportCallbackEXT();
            _instance.DestroySurfaceKHR(_surface);
            _instance.Dispose();
        }

        void CleanupSwapchain()
        {
            _device.DestroyImageView(_depthImageView);
            _device.DestroyImage(_depthImage);
            _device.FreeMemory(_depthImageMemory);

            _swapChainFramebuffers.ForEach(x => _device.DestroyFramebuffer(x));

            _device.FreeCommandBuffers(_commandPool, _drawCommandBuffers);

            _device.DestroyPipeline(_graphicsPipeline);
            _device.DestroyPipelineLayout(_graphicsPipelineLayout);

            _device.DestroyRenderPass(_renderPass);

            _swapChainImageViews.ForEach(x => _device.DestroyImageView(x));

            _device.DestroySwapchainKHR(_swapChain);
        }
    }
}