using System;

namespace UCT.Service
{
    /// <summary>
    ///     提供与枚举相关的函数。
    /// </summary>
    public static class EnumService
    {
        /// <summary>
        ///     获取枚举的最大项
        /// </summary>
        public static T GetMaxEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            var maxValue = (T)values.GetValue(0);

            foreach (T value in values)
                if (Convert.ToInt32(value) > Convert.ToInt32(maxValue))
                    maxValue = value;

            return maxValue;
        }

        /// <summary>
        ///     获取枚举的最小项
        /// </summary>
        public static T GetMinEnumValue<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            var minValue = (T)values.GetValue(0);

            foreach (T value in values)
                if (Convert.ToInt32(value) < Convert.ToInt32(minValue))
                    minValue = value;

            return minValue;
        }

        /// <summary>
        ///     将枚举值自增，若达到最大值，则回到最小值。
        /// </summary>
        public static T IncrementEnum<T>(T value) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            var currentIndex = Array.IndexOf(values, value);
            var nextIndex = (currentIndex + 1) % values.Length;
            return (T)values.GetValue(nextIndex);
        }

        /// <summary>
        ///     将枚举值自减，若达到最小值，则回到最大值。
        /// </summary>
        public static T DecrementEnum<T>(T value) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));
            var currentIndex = Array.IndexOf(values, value);
            var prevIndex = (currentIndex - 1 + values.Length) % values.Length;
            return (T)values.GetValue(prevIndex);
        }
    }
}