using System;
using System.Collections.Generic;
using GlmNet;

namespace BundtCake
{
    //[StructLayout(LayoutKind.Sequential)]
    public struct UniformBufferObject
    {
        public mat4 Model;
        public mat4 View;
        public mat4 Projection;

        public List<byte> GetBytes()
        {
            var bytes = new List<byte>();

            AppendMat4BytesToBytes(bytes, Model);
            AppendMat4BytesToBytes(bytes, View);
            AppendMat4BytesToBytes(bytes, Projection);

            return bytes;
        }

        // void AppendMat4BytesToBytes(List<byte> bytes, mat4 mat)
        // {
        //     foreach (var f in mat.to_array())
        //     {
        //         AppendFloatBytesToBytes(bytes, f);
        //     }
        // }

        void AppendMat4BytesToBytes(List<byte> bytes,  mat4 mat)
        {
            AppendFloatBytesToBytes(bytes, mat[0, 0]);
            AppendFloatBytesToBytes(bytes, mat[0, 1]);
            AppendFloatBytesToBytes(bytes, mat[0, 2]);
            AppendFloatBytesToBytes(bytes, mat[0, 3]);

            AppendFloatBytesToBytes(bytes, mat[1, 0]);
            AppendFloatBytesToBytes(bytes, mat[1, 1]);
            AppendFloatBytesToBytes(bytes, mat[1, 2]);
            AppendFloatBytesToBytes(bytes, mat[1, 3]);

            AppendFloatBytesToBytes(bytes, mat[2, 0]);
            AppendFloatBytesToBytes(bytes, mat[2, 1]);
            AppendFloatBytesToBytes(bytes, mat[2, 2]);
            AppendFloatBytesToBytes(bytes, mat[2, 3]);
            
            AppendFloatBytesToBytes(bytes, mat[3, 0]);
            AppendFloatBytesToBytes(bytes, mat[3, 1]);
            AppendFloatBytesToBytes(bytes, mat[3, 2]);
            AppendFloatBytesToBytes(bytes, mat[3, 3]);
        }

        void AppendFloatBytesToBytes(List<byte> bytes, float appendFloat)
        {
            foreach (var item in BitConverter.GetBytes(appendFloat))
            {
                bytes.Add(item);
            }
        }
    }
}