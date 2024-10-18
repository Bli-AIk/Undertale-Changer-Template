using System.Collections.Generic;
using System.Linq;

namespace UCT.Service
{
    /// <summary>
    /// 文本处理相关函数
    /// </summary>
    public static class TextProcessingService
    {
        /// <summary>
        /// 检测 '\'字符然后分割文本到子List
        /// 批量处理List string 
        /// </summary>
        public static void SplitStringToListWithDelimiter(List<string> parentList, List<string> sonList, char delimiter = '\\')
        {
            sonList.Clear();
            var text = "";
            foreach (var t1 in parentList.SelectMany(t => t))
            {
                if (t1 == delimiter || t1 == ';')
                {
                    sonList.Add(text);
                    text = "";
                }
                else text += t1;
            }
        }

        /// <summary>
        /// 检测 '\'字符然后分割文本到子List
        /// 传入一个string
        /// </summary>
        public static void SplitStringToListWithDelimiter(string parentString, List<string> sonList, char delimiter = '\\')
        {
            sonList.Clear();
            var text = "";

            foreach (var t in parentString)
            {
                if (t == delimiter || t == ';')
                {
                    sonList.Add(text);
                    text = "";
                }
                else text += t;
            }
        }

        /// <summary>
        /// 检测到第一个'\'字符就传出
        /// </summary>
        public static string SplitFirstStringWithDelimiter(string original, char delimiter = '\\')
        {
            var final = "";
            foreach (var t in original)
            {
                if (t != delimiter)
                    final += t;
                else break;
            }
            return final;
        }

        /// <summary>
        /// 反向检测第一个'\'字符就传出，可选忽视掉最后的 ; 号。
        /// </summary>
        public static float GetLastFloatBeforeDelimiter(string original, bool ignoreSemicolon = true, char delimiter = '\\')
        {
            if (ignoreSemicolon && original[^1..] == ";")
                original = original[..^1];
            var changed = "";
            for (var i = 0; i < original.Length; i++)
            {
                changed += original[original.Length - i - 1];
            }
            changed = SplitFirstStringWithDelimiter(changed, delimiter);
            original = "";
            for (var i = 0; i < changed.Length; i++)
            {
                original += changed[changed.Length - i - 1];
            }
            if (float.TryParse(original, out var y))
                return y;
            return 99999999;
        }

        /// <summary>
        /// 用于游戏内文本读取
        /// 传入数据名称返回文本包文本
        /// 给第一个 返第二个
        /// </summary>
        public static string GetFirstChildStringByPrefix(List<string> parentList, string screen)
        {
            foreach (var str in from t in parentList where t.Length > screen.Length && SplitFirstStringWithDelimiter(t) == screen select t[(screen.Length + 1)..] into str select str[..^1])
            {
                return str;
            }

            return "null";
        }

        /// <summary>
        /// 用于游戏内文本读取
        /// 传入数据名称返回所有同名的文本包文本
        /// </summary>
        public static List<string> GetAllChildStringsByPrefix(List<string> parentList, string screen)
        {
            return (from t in parentList where t.Length > screen.Length && SplitFirstStringWithDelimiter(t) == screen select t[(screen.Length + 1)..] into str select str[..^1]).ToList();
        }

        /// <summary>
        /// 检测list的前几个字符是否与传入的string screen相同。
        /// 若相同则分割文本到子List
        /// </summary>
        public static void GetFirstChildStringByPrefix(List<string> parentList, List<string> sonList, string screen)
        {
            sonList.Clear();
            sonList.AddRange(from t in parentList where t[..screen.Length] == screen select t[screen.Length..]);
        }

        /// <summary>
        /// 再分配文本包
        /// </summary>
        public static void ClassifyStringsByPrefix(List<string> max, string[] text, List<string>[] son)
        {
            foreach (var t in son)
            {
                t.Clear();
            }

            foreach (var t in max)
            {
                for (var j = 0; j < text.Length; j++)
                {
                    if (t[..text[j].Length] == text[j])
                    {
                        son[j].Add(t[(text[j].Length + 1)..]);
                    }
                }
            }
        }

        /// <summary>
        /// 给一个指定长度，然后会用空格填充原字符串
        /// </summary>
        /// <param name="origin">原字符串</param>
        /// <param name="length">返回长度</param>
        /// <returns></returns>
        public static string PadStringToLength(string origin, int length)
        {
            for (var i = 0; i < length - origin.Length; i++)
            {
                origin += " ";
            }
            return origin;
        }

        /// <summary>
        /// 换算时间
        /// </summary>
        public static string GetRealTime(int totalSeconds)
        {
            if (totalSeconds < 0)
                totalSeconds = 0;

            //int seconds = totalSeconds % 60;
            totalSeconds /= 60;
            var minutes = totalSeconds % 60;
            var hours = totalSeconds / 60;

            var hoursString = hours < 10 ? $"0{hours}" : $"{hours}";
            var minutesString = minutes < 10 ? $"0{minutes}" : $"{minutes}";

            return $"{hoursString}:{minutesString}";
        }
    }
}