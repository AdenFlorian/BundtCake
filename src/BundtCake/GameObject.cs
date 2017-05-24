using System;
using System.Collections.Generic;
using GlmNet;

namespace BundtCake
{
    class GameObject
    {
        static int nextId;

        public int Id = nextId++;
        public Transform Transform = new Transform();
        //public List<Vertex> Vertices = new List<Vertex>();
        public Mesh Mesh = new Mesh();
    }
}