using System;
using System.Collections.Generic;

namespace BundtCake
{
    public class GameObject
    {
        static int nextId;

        public readonly Transform Transform = new Transform();

        public int Id = nextId++;
        public Mesh Mesh = new Mesh();
    }
}