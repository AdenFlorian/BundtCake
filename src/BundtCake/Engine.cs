using System;
using System.Collections.Generic;
using GlmNet;
using SDL2;

namespace BundtCake
{
    class Engine
    {
        public Action<float> JustBeforeDraw;

        Vulkan _vulkan;
        Window _window;
        List<GameObject> _gameObjects;
        
        public void Initialize(List<GameObject> gameObjects, string windowTitle, int windowPositionX, int windowPositionY, int windowWidth, int windowHeight)
        {
            _gameObjects = gameObjects;
            
            _vulkan = new Vulkan();

            _window = new Window(windowTitle, windowPositionX, windowPositionY, windowWidth, windowHeight, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            _vulkan.CameraTransform.Position.x = 8;
            _vulkan.CameraTransform.Position.y = 5;
            _vulkan.CameraTransform.Position.z = 5;

            _vulkan.Initialize(_window, gameObjects);
        }

        public void Start()
        {
            var startTime = DateTime.Now;

            var deltaTime = 0f;
            var lastFrameTime = startTime;

            SDL.SDL_Event sdlEvent;

            // If doing multiple windows in different threads
            // should poll events from main thread and pass them out from there

            while (true)
            {
                SDL.SDL_PollEvent(out sdlEvent);

                if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                {
                    break;
                }
                if (sdlEvent.type == SDL.SDL_EventType.SDL_WINDOWEVENT && sdlEvent.window.windowID == _window.Id)
                {
                    if (sdlEvent.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                    {
                        break;
                    }
                    if (sdlEvent.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                    {
                        _vulkan.OnWindowResized();
                        continue;
                    }
                }

                deltaTime = (float)(DateTime.Now - lastFrameTime).TotalSeconds;
                lastFrameTime = DateTime.Now;

                JustBeforeDraw?.Invoke(deltaTime);

                _vulkan.DrawFrame();
            }

            _vulkan.Dispose();
            _window.Dispose();
        }
    }
}