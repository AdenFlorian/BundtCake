using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BundtCommon;
using GlmNet;
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

            var myGameObject = new GameObject();
            myGameObject.Mesh.VertexPositions = new vec3[]
            {
                new vec3(-0.5f, -0.5f, 0.0f), 
                new vec3(0.5f, -0.5f, 0.0f),  
                new vec3(0.5f, 0.5f, 0.0f),   
                new vec3(-0.5f, 0.5f, 0.0f),  
                new vec3(-1.5f, -0.5f, -0.5f),
                new vec3(1.5f, -0.5f, -0.5f), 
                new vec3(1.5f, 0.5f, -0.5f),  
                new vec3(-1.5f, 0.5f, -0.5f), 
            };
            myGameObject.Mesh.Colors = new vec3[]
            {
                new vec3(1.0f, 0.3f, 0.0f),
                new vec3(0.9f, 0.7f, 0.6f),
                new vec3(0.7f, 0.0f, 1.0f),
                new vec3(1.0f, 0.3f, 0.7f),
                new vec3(1.0f, 0.3f, 0.0f),
                new vec3(0.9f, 0.7f, 0.6f),
                new vec3(0.7f, 0.0f, 1.0f),
                new vec3(1.0f, 0.3f, 0.7f),
            };
            myGameObject.Mesh.TexCoords = new vec2[]
            {
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f),
                new vec2(0.0f, 0.0f),
                new vec2(1.0f, 0.0f),
                new vec2(1.0f, 1.0f),
                new vec2(0.0f, 1.0f)
            };
            myGameObject.Mesh.Indices = new uint[]
            {
                0, 1, 2, 2, 3, 0,
                4, 5, 6, 6, 7, 4
            };
            myGameObject.Transform.Position.x = 1;

            var myGameObject2 = new GameObject();
            myGameObject2.Mesh = Primitives.CreateCube8();
            myGameObject2.Transform.Scale *= .5f;

            var myGameObject3 = new GameObject();
            myGameObject3.Mesh = Primitives.CreateCube36();
            myGameObject3.Transform.Scale *= .6f;
            myGameObject3.Transform.Position.x -= 1f;

            var gameObjects = new List<GameObject>();
            gameObjects.Add(myGameObject);
            gameObjects.Add(myGameObject2);
            gameObjects.Add(myGameObject3);

            DoFoo(100, 100, gameObjects);

            return 0;
        }

        static void DoFoo(int x, int y, List<GameObject> gameObjects)
        {
            var vulkan = new Vulkan();

            var window = new Window("help, im stuck in a title bar factory", x, y, 500, 500, SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            vulkan.Initialize(window, gameObjects);

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
                if (sdlEvent.type == SDL.SDL_EventType.SDL_WINDOWEVENT && sdlEvent.window.windowID == window.Id)
                {
                    if (sdlEvent.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_CLOSE)
                    {
                        break;
                    }
                    if (sdlEvent.window.windowEvent == SDL.SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED)
                    {
                        vulkan.OnWindowResized();
                        continue;
                    }
                }
                //gameObject.Transform.Position.x -= 0.0001f;
                //gameObject.Transform.Rotation.x -= 0.001f;
                //gameObject.Transform.Rotation.y -= 0.002f;
                gameObjects[0].Transform.Rotation.z -= 0.003f;
                gameObjects[1].Transform.Rotation.x -= 0.006f;
                //gameObject.Transform.Scale.x -= 0.0001f;
                //gameObject.Transform.Scale.y += 0.0002f;
                //gameObject.Transform.Scale.z -= 0.0003f;
                vulkan.DrawFrame();
            }

            vulkan.Dispose();

            window.Dispose();
        }
    }
}
