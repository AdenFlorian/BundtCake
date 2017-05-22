using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GlmNet;
using Vulkan;

namespace BundtCake
{
    //[StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public vec3 pos;
        public vec3 color;
        public vec2 texCoord;

        public static VertexInputBindingDescription GetBindingDescription()
        {
            var bindingDescription = new VertexInputBindingDescription
            {
                Binding = 0,
                // vec3 + vec3 + vec2 = 8
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

            foo(byteList, pos.x);
            foo(byteList, pos.y);
            foo(byteList, pos.z);
            foo(byteList, color.x);
            foo(byteList, color.y);
            foo(byteList, color.z);
            foo(byteList, texCoord.x);
            foo(byteList, texCoord.y);

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