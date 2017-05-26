using System;
using System.Collections.Generic;
using System.Numerics;

namespace BundtCake
{
    //[StructLayout(LayoutKind.Sequential)]
    public struct UniformBufferObject
    {
        public Matrix4x4 Model;
        public Matrix4x4 View;
        public Matrix4x4 Projection;

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

        void AppendMat4BytesToBytes(List<byte> bytes,  Matrix4x4 mat)
        {
            AppendFloatBytesToBytes(bytes, mat.M11);
            AppendFloatBytesToBytes(bytes, mat.M12);
            AppendFloatBytesToBytes(bytes, mat.M13);
            AppendFloatBytesToBytes(bytes, mat.M14);

            AppendFloatBytesToBytes(bytes, mat.M21);
            AppendFloatBytesToBytes(bytes, mat.M22);
            AppendFloatBytesToBytes(bytes, mat.M23);
            AppendFloatBytesToBytes(bytes, mat.M24);

            AppendFloatBytesToBytes(bytes, mat.M31);
            AppendFloatBytesToBytes(bytes, mat.M32);
            AppendFloatBytesToBytes(bytes, mat.M33);
            AppendFloatBytesToBytes(bytes, mat.M34);

            AppendFloatBytesToBytes(bytes, mat.M41);
            AppendFloatBytesToBytes(bytes, mat.M42);
            AppendFloatBytesToBytes(bytes, mat.M43);
            AppendFloatBytesToBytes(bytes, mat.M44);
        }

        void AppendFloatBytesToBytes(List<byte> bytes, float appendFloat)
        {
            foreach (var item in BitConverter.GetBytes(appendFloat))
            {
                bytes.Add(item);
            }
        }

        public static int GetSizeInBytes()
        {
            var floatsInMat4 = 16;
            var numOfMat4s = 3;
            var totalFloats = floatsInMat4 * numOfMat4s;
            var bitsPerFloat = 32;
            var bitInAByte = 8;
            return (totalFloats * bitsPerFloat) / bitInAByte;
        }
    }
}