using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace BundtCake
{
    //[StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Color;
        public Vector2 TexCoord;

        public Vertex(Vector3 pos, Vector3 color, Vector2 texCoord)
        {
            Position = pos;
            Color = color;
            TexCoord = texCoord;
        }

        public static IEnumerable<byte> VertexArrayToByteArray(IEnumerable<Vertex> vertices)
        {
            var verticesBytes = new List<byte>();

            foreach (var vertex in vertices)
            {
                foreach (var b in vertex.GetBytes())
                {
                    verticesBytes.Add(b);
                }
            }

            return verticesBytes;
        }

        public IEnumerable<byte> GetBytes()
        {
            var byteList = new List<byte>();

            foo(byteList, Position.X);
            foo(byteList, Position.Y);
            foo(byteList, Position.Z);
            foo(byteList, Color.X);
            foo(byteList, Color.Y);
            foo(byteList, Color.Z);
            foo(byteList, TexCoord.X);
            foo(byteList, TexCoord.Y);

            return byteList;
        }

        void foo(List<byte> bytes, float appendFloat)
        {
            foreach (var item in BitConverter.GetBytes(appendFloat))
            {
                bytes.Add(item);
            }
        }
    }
}