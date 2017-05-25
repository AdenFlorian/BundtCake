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

            var floor = new GameObject();
            floor.Mesh = Primitives.CreateCube8();
            floor.Transform.Position.y = -20;
            floor.Transform.Scale *= 15;
            gameObjects.Add(floor);

            for (int i = 0; i < 100; i++)
            {
                var myGameObject = new GameObject();
                myGameObject.Mesh = Primitives.CreateCube36();
                myGameObject.Transform.Position.x -= i * 3;
                myGameObject.Transform.Position.z -= 3;
                gameObjects.Add(myGameObject);
                
            }

            var mainCamera = new Camera();
            mainCamera.Transform.Position = new vec3(0, 0, -25);

            engine.Initialize(gameObjects, mainCamera, "i am bundt", 100, 100, 1920, 1080);


            engine.JustBeforeDraw += (deltaTime) => {
                var moveSpeed = 5f;
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Transform.Rotation.z += 10 * i * deltaTime;
                    if (Input.W)
                    {
                        //gameObjects[i].Transform.Rotation.y += 100 * deltaTime;
                    }
                }
                if (Input.LeftShift)
                {
                    moveSpeed *= 10;
                }
                if (Input.A)
                {
                    mainCamera.Transform.Position.x += moveSpeed * deltaTime;
                    mainCamera.Transform.Rotation.z += moveSpeed * deltaTime;
                }
                if (Input.D)
                {
                    mainCamera.Transform.Position.x -= moveSpeed * deltaTime;
                    mainCamera.Transform.Rotation.z -= moveSpeed * deltaTime;
                }
                if (Input.W)
                {
                    mainCamera.Transform.Position.z += moveSpeed * deltaTime;
                }
                if (Input.S)
                {
                    mainCamera.Transform.Position.z -= moveSpeed * deltaTime;
                }
                //mainCamera.Transform.Position.x += engine.MouseX / 100f;
                //mainCamera.Transform.Position.y -= engine.MouseY / 100f;
                //mainCamera.Transform.Rotation.x += engine.MouseX / 1f;
                //mainCamera.Transform.Rotation.x -= Input.MouseY * 0.1f;
                //mainCamera.Transform.Rotation.y += Input.MouseX * 0.1f;
            };

            engine.Start();

            return 0;
        }
    }
}
