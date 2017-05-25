using System.Collections.Generic;
using System.Numerics;

namespace BundtCake
{
    class Mesh
    {
        public Vector3[] VertexPositions;
        public Vector3[] Colors;
        public Vector2[] TexCoords;
        public uint[] Indices;
        public List<Vertex> Vertices
        {
            get
            {
                var vertices = new List<Vertex>();

                for (int i = 0; i < VertexPositions.Length; i++)
                {
                    vertices.Add(new Vertex(VertexPositions[i], Colors[i], TexCoords[i]));
                }

                return vertices;
            }
        }
    }
}