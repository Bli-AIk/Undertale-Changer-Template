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
    }
}