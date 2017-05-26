using System;
using System.Collections.Generic;
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
            
            _window = new Window(windowTitle, windowPositionX, windowPositionY, windowWidth, windowHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            _vulkan = new Vulkan(_window, gameObjects, mainCamera);
            _vulkan.Initialize();
        }

        public void Start()
        {
            var startTime = DateTime.Now;
            var deltaTime = 0f;
            var lastFrameTime = startTime;

            Input.Start();

            while (true)
            {
                var sdlEvent = SdlWrapper.PollEvent();
                var eventResult = HandleSdlEvent(sdlEvent);

                if (eventResult == LoopDo.Continue) continue;
                if (eventResult == LoopDo.Break) break;

                deltaTime = (float)(DateTime.Now - lastFrameTime).TotalSeconds;
                lastFrameTime = DateTime.Now;

                Input.Update();
                JustBeforeDraw?.Invoke(deltaTime);
                Input.Clear();

                _vulkan.DrawFrame();
            }

            _vulkan.Dispose();
            _window.Dispose();
        }

        LoopDo HandleSdlEvent(SDL_Event sdlEvent)
        {
            switch (sdlEvent.type)
            {
                case SDL_EventType.SDL_QUIT: return LoopDo.Break;
                case SDL_EventType.SDL_WINDOWEVENT: return HandleWindowEvent(sdlEvent.window);
                case SDL_EventType.SDL_KEYDOWN:
                    Input.SetKeyDown(sdlEvent.key.keysym.sym);
                    return LoopDo.Continue;
                case SDL_EventType.SDL_KEYUP:
                    Input.SetKeyUp(sdlEvent.key.keysym.sym);
                    return LoopDo.Continue;
                default: break;
            }
            return LoopDo.Nothing;
        }

        LoopDo HandleWindowEvent(SDL_WindowEvent windowEvent)
        {
            if (windowEvent.windowID == _window.Id)
            {
                if (windowEvent.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                {
                    return LoopDo.Break;
                }
                if (windowEvent.windowEvent == SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
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