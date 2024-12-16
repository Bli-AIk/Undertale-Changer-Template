using System;
using Alchemy.Inspector;

namespace UCT.EventSystem
{
    /// <summary>
    /// 事件系统的Fact条目，用于定义事件
    /// </summary>
    [Serializable]
    public struct EventEntry
    {
        public string name;
        [ReadOnly]
        public bool isTriggering;
    }
}