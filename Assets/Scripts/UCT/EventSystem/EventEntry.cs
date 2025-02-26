using System;

namespace UCT.EventSystem
{
    /// <summary>
    ///     事件系统的Fact条目，用于定义事件
    /// </summary>
    [Serializable]
    public struct EventEntry : IEquatable<EventEntry>
    {
        public string name;

        public bool isTriggering;

        public float closeTime;

        public bool Equals(EventEntry other)
        {
            return name == other.name && isTriggering == other.isTriggering && closeTime.Equals(other.closeTime);
        }

        public override bool Equals(object obj)
        {
            return obj is EventEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(name, isTriggering, closeTime);
        }
    }

    //TODO:可能的触发事件的方式: OWTrigger, 特定时间触发, 角色状态（数据检测）触发, 对话中触发, 使用/检查/丢弃特定物品触发, etc.
}