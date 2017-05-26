using System.Runtime.InteropServices;
using Vulkan;

namespace BundtCake
{
    public static class DeviceExtensions
    {
        public static void CopyToBufferMemory(this Device @this, byte[] source, DeviceMemory destinationBufferMemory, DeviceSize offset, DeviceSize size, uint mapFlags)
        {
            var mappedMemoryPointer = @this.MapMemory(destinationBufferMemory, offset, size, mapFlags);

            Marshal.Copy(source, 0, mappedMemoryPointer, (int)(uint)size);

            @this.UnmapMemory(destinationBufferMemory);
        }
    }
}