using System.Threading.Tasks;
using BundtCommon;
using Microsoft.Extensions.Logging;

namespace BundtCake
{
    class Program
    {
        static MyLogger _logger = new MyLogger(nameof(Program));

        static int Main(string[] args)
        {
            MyLogger.SetLogLevelOverride(LogLevel.Debug);

            var vulkan = new Vulkan();

            var window = new Window("help, im stuck in a title bar factory", 100, 100, 500, 500);

            var initVulkanTask = Task.Run(async() => {
                await vulkan.InitializeAsync(window);
            });

            initVulkanTask.Wait();

            if (initVulkanTask.Exception != null)
            {
                throw initVulkanTask.Exception;
            }




            // SDL.SDL_Event sdlEvent;

            // while (true)
            // {
            //     SDL.SDL_PollEvent(out sdlEvent);

            //     if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
            //     {
            //         break;
            //     }
            // }


            vulkan.Dispose();


            // glfwDestroyWindow(window);

            return 0;
        }
    }
}
