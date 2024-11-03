using System;

namespace UCT.Service
{
    /// <summary>
    /// 提供与枚举相关的函数。
    /// </summary>
    public static class EnumService
    {
        /// <summary>
        /// 获取枚举的最大项
        /// </summary>
        public static T GetMaxEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            dynamic maxValue = values.GetValue(0); // 初始化为第一个值

            foreach (var value in values)
            {
                if ((dynamic)value > maxValue)
                {
                    maxValue = value; // 更新最大值
                }
            }

            return (T)maxValue;
        }
        /// <summary>
        /// 获取枚举的最小项
        /// </summary>
        public static T GetMinEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            dynamic minValue = values.GetValue(0); // 初始化为第一个值

            foreach (var value in values)
            {
                if ((dynamic)value < minValue)
                {
                    minValue = value; // 更新最小值
                }
            }

            return (T)minValue;
        }
    }
}