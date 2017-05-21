using System;
using System.Runtime.Serialization;

namespace BundtCake
{
    [Serializable]
    class VulkanException : Exception
    {
        public VulkanException()
        {
        }

        public VulkanException(string message) : base(message)
        {
        }

        public VulkanException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected VulkanException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}