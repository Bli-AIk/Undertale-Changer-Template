using System;

namespace UCT.EventSystem
{
    /// <summary>
    ///     事件系统的Fact条目，用于存储世界信息
    /// </summary>
    [Serializable]
    public struct FactEntry
    {
        public string name;
        public Scope scope; //TODO: 通过不同的ScriptableObject来区分Scope
        public int value;

        // 仅当 Scope 为 Area 时有效
        public Area area;

        // 仅当 Scope 为 Scene 时有效
        public string scene;
    }

    public enum Scope
    {
        Global
        // Area,
        // Scene,
        // Temp
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