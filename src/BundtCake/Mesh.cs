using System.Collections.Generic;
using GlmNet;

namespace BundtCake
{
    class Mesh
    {
        public vec3[] VertexPositions;
        public vec3[] Colors;
        public vec2[] TexCoords;
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