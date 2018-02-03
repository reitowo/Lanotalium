using System;

namespace Schwarzer.Chart.Bms.Bemusilization
{
    public enum ResourceType
    {
        Unknown,
        bmp,
        wav,
        bpm,
        stop,
        bga,
    }

    public struct BMSResourceData
    {
        public ResourceType type;
        public long resourceId;
        public string dataPath;
        public object additionalData;
    }

    public struct ResourceId : IEquatable<ResourceId>
    {
        public ResourceType type;
        public long resourceId;

        public ResourceId(ResourceType type, long resourceId)
        {
            this.type = type;
            this.resourceId = resourceId;
        }

        public bool Equals(ResourceId other)
        {
            return type == other.type && resourceId == other.resourceId;
        }

        public override bool Equals(object obj)
        {
            return obj is ResourceId && Equals((ResourceId)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * 23 + type.GetHashCode();
                hashCode = hashCode * 23 + resourceId.GetHashCode();
                return hashCode;
            }
        }
    }
}
