using System;

namespace UCT.EventSystem
{
    /// <summary>
    ///     事件系统的Fact条目，用于存储世界信息
    /// </summary>
    [Serializable]
    public struct FactEntry : IEquatable<FactEntry>
    {
        public string name;
        public int value;


        public bool Equals(FactEntry other)
        {
            return name == other.name && value == other.value;
        }

        public override bool Equals(object obj)
        {
            return obj is FactEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, value);
        }
    }
}