using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using Vulkan;

namespace BundtCake
{
    //[StructLayout(LayoutKind.Sequential)]
    struct Vertex
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

        public static VertexInputBindingDescription GetBindingDescription()
        {
            var bindingDescription = new VertexInputBindingDescription
            {
                Binding = 0,
                // Vector3 + Vector3 + Vector2 = 8
                Stride = 8 * sizeof(float),
                InputRate = VertexInputRate.Vertex
            };

            return bindingDescription;
        }

        public static VertexInputAttributeDescription[] GetAttributeDescriptions()
        {
            var attributeDescriptions = new VertexInputAttributeDescription[3];

            attributeDescriptions[0].Binding = 0;
            attributeDescriptions[0].Location = 0;
            attributeDescriptions[0].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[0].Offset = 0;

            attributeDescriptions[1].Binding = 0;
            attributeDescriptions[1].Location = 1;
            attributeDescriptions[1].Format = Format.R32G32B32Sfloat;
            attributeDescriptions[1].Offset = 3 * sizeof(float);

            attributeDescriptions[2].Binding = 0;
            attributeDescriptions[2].Location = 2;
            attributeDescriptions[2].Format = Format.R32G32Sfloat;
            attributeDescriptions[2].Offset = 6 * sizeof(float);

            return attributeDescriptions;
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