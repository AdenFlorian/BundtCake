using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
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
            MyLogger.SetLogLevelOverride(LogLevel.Information);

            var engine = new Engine();

            var gameObjects = new List<GameObject>();

            var floor = new GameObject();
            floor.Mesh = Primitives.CreateCube8();
            floor.Transform.Position.Y = -20;
            floor.Transform.Scale *= 15;
            //gameObjects.Add(floor);

            for (int i = 0; i < 100; i++)
            {
                var myGameObject = new GameObject();
                myGameObject.Mesh = Primitives.CreateCube36();
                myGameObject.Transform.Position.X -= i * 3;
                myGameObject.Transform.Position.Z -= 3;
                //myGameObject.Transform.Scale *= 3;
                gameObjects.Add(myGameObject);
            }

            var mainCamera = new Camera();
            mainCamera.Transform.Position = new Vector3(0, 0, -200);

            if (engine.Initialize(gameObjects, mainCamera, "i am bundt", 100, 100, 1920, 1080) == false)
            {
                engine.Dispose();
                return 0;
            }

            engine.JustBeforeDraw += (deltaTime) => {
                var moveSpeed = 5f;
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    gameObjects[i].Transform.Rotation.Z += 10 * i * deltaTime;
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
                    mainCamera.Transform.Position.X += moveSpeed * deltaTime;
                    //mainCamera.Transform.Rotation.Z += moveSpeed * deltaTime;
                }
                if (Input.D)
                {
                    mainCamera.Transform.Position.X -= moveSpeed * deltaTime;
                    //mainCamera.Transform.Rotation.Z -= moveSpeed * deltaTime;
                }
                if (Input.W)
                {
                    mainCamera.Transform.Position.Z += moveSpeed * deltaTime;
                }
                if (Input.S)
                {
                    mainCamera.Transform.Position.Z -= moveSpeed * deltaTime;
                }
                mainCamera.Transform.Rotation.X -= Input.MouseY * 0.1f;
                mainCamera.Transform.Rotation.Y += Input.MouseX * 0.1f;
            };

            engine.Start();

            return 0;
        }
    }
}
