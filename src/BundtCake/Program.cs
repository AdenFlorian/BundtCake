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

            var engine = new Engine();

            var gameObjects = new List<GameObject>();

            for (int i = 0; i < 100; i++)
            {
                var myGameObject = new GameObject();
                myGameObject.Mesh = Primitives.CreateCube36();
                myGameObject.Transform.Position.x -= i * 3;
                myGameObject.Transform.Position.z -= 3;
                gameObjects.Add(myGameObject);
            }

            var mainCamera = new Camera();
            mainCamera.Transform.Position = new vec3(8, 5, 5);

            engine.Initialize(gameObjects, mainCamera, "i am bundt", 100, 100, 1920, 1080);

            engine.JustBeforeDraw += (deltaTime) => {
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Transform.Rotation.z += 10 * i * deltaTime;
                }
            };

            engine.Start();

            return 0;
        }
    }
}
