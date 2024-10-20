using System.Collections.Generic;

namespace UCT.Service
{
    /// <summary>
    /// 数组，列表相关函数
    /// </summary>
    public static class ListManipulationService
    {
        /// <summary>
        /// 找到列表中第一个零的索引。如果列表中没有零，则返回列表的长度。
        /// </summary>
        /// <param name="list">整数列表</param>
        /// <returns>第一个零的索引或列表的长度</returns>
        public static int FindFirstZeroIndex(List<int> list)
        {
            var result = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] != 0) continue;
                result = i;
                break;
            }
            return result;
        }
    }
}