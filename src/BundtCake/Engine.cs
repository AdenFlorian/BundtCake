using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BundtCommon;
using static SDL2.SDL;

namespace BundtCake
{
    class Engine
    {
        public Action<float> JustBeforeDraw;

        static MyLogger _logger = new MyLogger(nameof(Engine));

        VulkanRenderer _vulkan;
        Window _window;
        List<GameObject> _gameObjects;
        
        /// <summary>
        /// Returns false if user quit the application during initializtion
        /// </summary>
        public bool Initialize(List<GameObject> gameObjects, Camera mainCamera, string windowTitle, int windowPositionX, int windowPositionY, int windowWidth, int windowHeight)
        {
            _gameObjects = gameObjects;
            
            _window = new Window(windowTitle + " - Initializing...", windowPositionX, windowPositionY, windowWidth, windowHeight, SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            _vulkan = new VulkanRenderer(_window, gameObjects, mainCamera);

            var tokenSrc = new CancellationTokenSource();
            var initTask = _vulkan.InitializeAsync(tokenSrc.Token);

            while (initTask.IsCompleted == false)
            {
                var sdlEvent = SdlWrapper.PollEvent();
                if (sdlEvent.type == SDL_EventType.SDL_QUIT)
                {
                    _logger.LogInfo("Cancelling initialization...");
                    tokenSrc.Cancel();
                    initTask.Wait();
                    _logger.LogInfo("Initialization cancelled");
                    return false;
                }
            }

            _window.Title = windowTitle;

            return true;
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

            Dispose();
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

        public void Dispose()
        {
            _vulkan.Dispose();
            _window.Dispose();
        }
    }
}