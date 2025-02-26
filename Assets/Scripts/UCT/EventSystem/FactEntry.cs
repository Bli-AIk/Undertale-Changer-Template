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

        // 仅当 Scope 为 Area 时有效
        public Area area;

        // 仅当 Scope 为 Scene 时有效
        public string scene;

        public bool Equals(FactEntry other)
        {
            return name == other.name && value == other.value && area == other.area && scene == other.scene;
        }

        public override bool Equals(object obj)
        {
            return obj is FactEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, value, (int)area, scene);
        }
    }

    public enum Area
    {
        Ruins,
        SnowDin,
        Waterfall,
        HotLand,
        Core,
        NewHome
    }
}