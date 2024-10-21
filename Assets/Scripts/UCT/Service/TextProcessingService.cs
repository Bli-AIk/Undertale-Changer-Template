using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UCT.Service
{
    /// <summary>
    /// 字符串、文本处理相关函数
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
        /// 检测输入字符串，返回第一个分隔符之前的部分。
        /// </summary>
        public static string SplitFirstStringWithDelimiter(string input, char delimiter = '\\')
        {
            var result = "";
            foreach (var t in input)
            {
                if (t != delimiter)
                    result += t;
                else break;
            }
            return result;
        }

        /// <summary>
        /// 从输入字符串中获取最后一个分隔符前的浮点数。
        /// 默认忽略最后一个分号，反向搜索直到遇到指定的分隔符。
        /// 如果无法解析浮点数，返回一个默认的极大值。
        /// </summary>
        /// <param name="input">原字符串</param>
        /// <param name="isIgnoreSemicolon">是否忽略分号</param>
        /// <param name="delimiter">分隔符</param>
        /// <returns>填充后的字符串</returns>
        public static float GetLastFloatBeforeDelimiter(string input, bool isIgnoreSemicolon = true, char delimiter = '\\')
        {
            if (isIgnoreSemicolon && input[^1..] == ";")
                input = input[..^1];
            var changed = "";
            for (var i = 0; i < input.Length; i++)
            {
                changed += input[input.Length - i - 1];
            }
            changed = SplitFirstStringWithDelimiter(changed, delimiter);
            input = "";
            for (var i = 0; i < changed.Length; i++)
            {
                input += changed[changed.Length - i - 1];
            }
            return float.TryParse(input, out var y) ? y : Mathf.Infinity;
        }

        /// <summary>
        /// 用于游戏内文本读取
        /// 传入数据名称返回文本包文本
        /// 给第一个 返第二个
        /// </summary>
        public static string GetFirstChildStringByPrefix(List<string> parentList, string screen)
        {
            foreach (var result in from t in parentList where t.Length > screen.Length && SplitFirstStringWithDelimiter(t) == screen select t[(screen.Length + 1)..] into str select str[..^1])
            {
                return result;
            }

            return "null";
        }

        /// <summary>
        /// 用于游戏内文本读取
        /// 传入数据名称返回所有同名的文本包文本
        /// </summary>
        public static List<string> GetAllChildStringsByPrefix(List<string> parentList, string screen)
        {
            var result = (from t in parentList where t.Length > screen.Length && SplitFirstStringWithDelimiter(t) == screen select t[(screen.Length + 1)..] into str select str[..^1]).ToList();
            return result;
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
            var result = origin;
            
            for (var i = 0; i < length - result.Length; i++)
            {
                result += " ";
            }
            return result;
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

            var result = $"{hoursString}:{minutesString}";
            return result;
        }

        /// <summary>
        /// 快捷输入富文本标记
        /// </summary>
        public static string RichText(string richText)
        {
            var result = $"<{richText}>";
            return result;
        }

        /// <summary>
        /// 快捷输入含参富文本标记
        /// </summary>
        public static string RichText(string richText, int number)
        {
            var result = $"<{richText}={number}>";
            return result;
        }

        /// <summary>
        /// 快捷输入富文本标记，包含结尾
        /// </summary>
        public static string RichTextWithEnd(string richText, string internalString = default)
        {
            var result = $"<{richText}>{internalString}</{richText}>";
            return result;
        }

        /// <summary>
        /// 快捷输入含参富文本标记，包含结尾
        /// </summary>
        public static string RichTextWithEnd(string richText, int number, string internalString = default)
        {
            var result = $"<{richText}={number}>{internalString}</{richText}>";
            return result;
        }
        
        /// <summary>
        /// 从字符串中移除指定位置的子字符串。
        /// </summary>
        public static string RemoveSubstring(string inputString, int startIndex, int endIndex, string add = "")
        {
            if (startIndex < 0 || endIndex >= inputString.Length || startIndex > endIndex)
            {
                Global.Other.Debug.Log("无效的起始和结束位置");
                return inputString;
            }

            var part1 = inputString[..startIndex]; // 从开头到A之前的部分
            var part2 = inputString[(endIndex + 1)..]; // 从B之后到字符串末尾的部分
            //Other.Debug.Log(inputString.Substring(startIndex + 1));
            var result = part1 + add + part2; // 合并两部分
            return result;
        }

        /// <summary>
        /// 随机生成一个六位长的英文
        /// </summary>
        public static string RandomString(int length = 6, string alphabet = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM")
        {
            var text = "";

            for (var i = 0; i < length; i++)
            {
                text += alphabet[Random.Range(0, alphabet.Length)];
            }
            return text;
        }

        /// <summary>
        /// 生成字符串形式的随机颜色。
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static string RandomStringColor()
        {
            var text = "<color=#";
            for (var i = 0; i < 6; i++)
            {
                text += $"{Random.Range(0, 16):X}";
            }
            text += "FF>";
            return text;
        }

        /// <summary>
        /// 生成字符串形式的随机颜色。
        /// </summary>
        public static string RandomStringColor(string origin)
        {
            return RandomStringColor() + origin + "</color>";
        }
    }
}