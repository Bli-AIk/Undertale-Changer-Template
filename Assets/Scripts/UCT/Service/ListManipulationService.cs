using System.Collections.Generic;
using System.Linq;
using UCT.Global.Core;

namespace UCT.Service
{
    /// <summary>
    ///     数组，列表相关函数
    /// </summary>
    public static class ListManipulationService
    {
        /// <summary>
        ///     找到列表中第一个 null 或空字符串的索引。如果列表中没有，则返回列表的长度。
        /// </summary>
        /// <param name="list">字符串列表</param>
        /// <returns>第一个 null 或空字符串的索引，或列表的长度</returns>
        public static int FindFirstNullOrEmptyIndex(List<string> list)
        {
            var result = list.Count;
            for (var i = 0; i < list.Count; i++)
            {
                if (!string.IsNullOrEmpty(list[i]))
                {
                    continue;
                }

                result = i;
                break;
            }

            return result;
        }

        /// <summary>
        ///     重排列表，将所有Null或空字符串排在最后。
        /// </summary>
        public static List<string> MoveNullOrEmptyToEnd(List<string> inputList)
        {
            var nonEmptyItems = new List<string>();
            var emptyOrNullItems = new List<string>();

            foreach (var item in inputList)
            {
                if (string.IsNullOrEmpty(item))
                {
                    emptyOrNullItems.Add(item);
                }
                else
                {
                    nonEmptyItems.Add(item);
                }
            }

            nonEmptyItems.AddRange(emptyOrNullItems);
            return nonEmptyItems;
        }

        /// <summary>
        ///     检查列表内的项是否都是item字典内注册过的dataName。如果不是，对应项会设为null。
        /// </summary>
        public static List<string> CheckAllDataNamesInItemList(List<string> inputList)
        {
            return inputList.Select(item =>
                    MainControl.Instance.ItemController.ItemDictionary.Keys.Any(
                        dictionaryItem => item == dictionaryItem)
                        ? item
                        : null)
                .ToList();
        }
    }
}