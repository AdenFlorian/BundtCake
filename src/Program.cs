using System.Threading.Tasks;
using BundtCommon;
using Microsoft.Extensions.Logging;
using SDL2;

namespace BundtCake
{
    class Program
    {
        static MyLogger _logger = new MyLogger(nameof(Program));

        static int Main(string[] args)
        {
            MyLogger.SetLogLevelOverride(LogLevel.Debug);

            var vulkan = new Vulkan();

            var window = new Window("help, im stuck in a title bar factory", 100, 100, 500, 500, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            vulkan.Initialize(window);

            SDL.SDL_Event sdlEvent;

            while (true)
            {
                SDL.SDL_PollEvent(out sdlEvent);

                if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    break;
                }
                if (sdlEvent.type == SDL.SDL_EventType.SDL_WINDOWEVENT)
                {
                    if (sdlEvent.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                    {
                        vulkan.OnWindowResized();
                    }
                }

                vulkan.UpdateUniformBuffer();
                vulkan.DrawFrame();
            }

            vulkan.Dispose();

            // glfwDestroyWindow(window);

            return 0;
        }
    }
}
