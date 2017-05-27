using System;
using System.Collections.Generic;
using System.Numerics;
using BundtCommon;
using Microsoft.Extensions.Logging;
using Troschuetz.Random;

namespace BundtCake
{
    class Program
    {
        static MyLogger _logger = new MyLogger(nameof(Program));

        static float _moveSpeed = 10f;
        static GameObject _playerGO;

        static int Main(string[] args)
        {
            MyLogger.SetLogLevelOverride(LogLevel.Information);

            var gameObjects = CreateGameObjects();

            var mainCamera = new Camera();
            mainCamera.Transform.Position = new Vector3(0, 2, 0);

            var engine = new Engine();

            if (engine.Initialize(gameObjects, mainCamera, "i am bundt", 100, 100, 1920, 1080) == false)
            {
                engine.Dispose();
                return 0;
            }

            engine.JustBeforeDraw += (deltaTime) => {
                var forward = 0f;
                var strafe = 0f;

                if (Input.LeftShift)
                {
                    _moveSpeed *= 3;
                }   
                if (Input.A)
                {
                    strafe += _moveSpeed * deltaTime;
                }
                if (Input.D)
                {
                    strafe -= _moveSpeed * deltaTime;
                }
                if (Input.W)
                {
                    forward += _moveSpeed * deltaTime;
                }
                if (Input.S)
                {
                    forward -= _moveSpeed * deltaTime;
                }

                _playerGO.Transform.Position += _playerGO.Transform.Forward * forward;
                _playerGO.Transform.Position += _playerGO.Transform.Right * strafe;

                _playerGO.Transform.Rotation.Y += Input.MouseX * 0.1f;

                mainCamera.Transform.Position = _playerGO.Transform.Position;
                mainCamera.Transform.Rotation.Y = _playerGO.Transform.Rotation.Y;

                RotateCameraWithMouse(mainCamera);

                _moveSpeed = 10f;
            };

            engine.Start();

            return 0;
        }

        static List<GameObject> CreateGameObjects()
        {
            var random = new TRandom();
            var gameObjects = new List<GameObject>();

            var floor = new GameObject();
            floor.Mesh = Primitives.CreatePlane();
            floor.Transform.Scale.X = 100;
            floor.Transform.Scale.Z = 100;
            gameObjects.Add(floor);

            _playerGO = new GameObject();
            _playerGO.Transform.Position.Y = 2;

            for (int i = 0; i < 1000; i++)
            {

                var newObstacleGO = new GameObject();
                newObstacleGO.Mesh = Primitives.CreateCube36();
                newObstacleGO.Transform.Position.X = (float)random.NextDouble(-100, 100);
                newObstacleGO.Transform.Position.Z = (float)random.NextDouble(-100, 100);
                newObstacleGO.Transform.Position.Y = (float)random.NextDouble(1, 10);
                newObstacleGO.Transform.Scale.X = (float)random.NextDouble(0.1f, 3);
                newObstacleGO.Transform.Scale.Y = (float)random.NextDouble(0.1f, 3);
                newObstacleGO.Transform.Scale.Z = (float)random.NextDouble(0.1f, 3);
                newObstacleGO.Transform.Rotation.X = (float)random.NextDouble(0, 360);
                newObstacleGO.Transform.Rotation.Y = (float)random.NextDouble(0, 360);
                newObstacleGO.Transform.Rotation.Z = (float)random.NextDouble(0, 360);
                gameObjects.Add(newObstacleGO);
            }
            

            return gameObjects;
        }

        static void RotateCameraWithMouse(Camera mainCamera)
        {
            mainCamera.Transform.Rotation.X -= Input.MouseY * 0.1f;
        }
    }
}
