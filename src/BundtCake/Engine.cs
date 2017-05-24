using System;
using System.Collections.Generic;
using GlmNet;
using static SDL2.SDL;

namespace BundtCake
{
    class Engine
    {
        public Action<float> JustBeforeDraw;

        Vulkan _vulkan;
        Window _window;
        List<GameObject> _gameObjects;
        
        public void Initialize(List<GameObject> gameObjects, Camera mainCamera, string windowTitle, int windowPositionX, int windowPositionY, int windowWidth, int windowHeight)
        {
            _gameObjects = gameObjects;
            
            _vulkan = new Vulkan();

            _window = new Window(windowTitle, windowPositionX, windowPositionY, windowWidth, windowHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            _vulkan.Initialize(_window, gameObjects, mainCamera);
        }

        public void Start()
        {
            var startTime = DateTime.Now;

            var deltaTime = 0f;
            var lastFrameTime = startTime;

            // If doing multiple windows in different threads
            // should poll events from main thread and pass them out from there

            while (true)
            {
                var sdlEvent = SdlWrapper.PollEvent();

                var eventResult = HandleSdlEvent(sdlEvent);

                if (eventResult == LoopDo.Continue) continue;
                if (eventResult == LoopDo.Break) break;

                deltaTime = (float)(DateTime.Now - lastFrameTime).TotalSeconds;
                lastFrameTime = DateTime.Now;

                JustBeforeDraw?.Invoke(deltaTime);

                _vulkan.DrawFrame();
            }

            _vulkan.Dispose();
            _window.Dispose();
        }

        LoopDo HandleSdlEvent(SDL_Event sdlEvent)
        {
            if (sdlEvent.type == SDL_EventType.SDL_QUIT)
            {
                return LoopDo.Break;
            }
            if (sdlEvent.type == SDL_EventType.SDL_WINDOWEVENT && sdlEvent.window.windowID == _window.Id)
            {
                if (sdlEvent.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                {
                    return LoopDo.Break;
                }
                if (sdlEvent.window.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                {
                    _vulkan.OnWindowResized();
                    return LoopDo.Continue;
                }
            }
            return LoopDo.Nothing;
        }

        enum LoopDo
        {
            Nothing,
            Continue,
            Break
        }
    }
}