using System;
using System.Collections.Generic;
using GlmNet;

namespace BundtCake
{
    public struct UniformBufferObject
    {
        public mat4 Model;
        public mat4 View;
        public mat4 Projection;

        public List<byte> GetBytes()
        {
            var bytes = new List<byte>();

            foreach (var f in Model.to_array())
            {
                foreach (var b in BitConverter.GetBytes(f))
                {
                    bytes.Add(b);
                }
            }

            foreach (var f in View.to_array())
            {
                foreach (var b in BitConverter.GetBytes(f))
                {
                    bytes.Add(b);
                }
            }

            foreach (var f in Projection.to_array())
            {
                foreach (var b in BitConverter.GetBytes(f))
                {
                    bytes.Add(b);
                }
            }

            return bytes;
        }
    }
}