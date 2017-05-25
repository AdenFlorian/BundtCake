using System;
using System.Collections.Generic;
using GlmNet;
using static SDL2.SDL;

namespace BundtCake
{
    class Engine
    {
        public Action<float> JustBeforeDraw;
        public float MouseX { get; private set; }
        public float MouseY { get; private set; }

        float lastMouseX;
        float lastMouseY;

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

            int mouseX, mouseY;
            SDL_GetMouseState(out mouseX, out mouseY);
            lastMouseX = mouseX;
            lastMouseY = mouseY;

            while (true)
            {
                var sdlEvent = SdlWrapper.PollEvent();

                var eventResult = HandleSdlEvent(sdlEvent);

                if (eventResult == LoopDo.Continue) continue;
                if (eventResult == LoopDo.Break) break;

                deltaTime = (float)(DateTime.Now - lastFrameTime).TotalSeconds;
                lastFrameTime = DateTime.Now;

                SDL_GetMouseState(out mouseX, out mouseY);
                MouseX = lastMouseX - mouseX;
                lastMouseX = mouseX;
                MouseY = lastMouseY - mouseY;
                lastMouseY = mouseY;

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
                case SDL_EventType.SDL_MOUSEMOTION: return HandleMouseMotionEvent(sdlEvent.motion);
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

        LoopDo HandleMouseMotionEvent(SDL_MouseMotionEvent mouseMotionEvent)
        {
            //MouseX = lastMouseX - mouseMotionEvent.x;
            //lastMouseX = mouseMotionEvent.x;
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