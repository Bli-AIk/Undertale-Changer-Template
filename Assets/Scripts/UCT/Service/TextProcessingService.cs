using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UCT.Core;
using UnityEngine;
using Random = System.Random;

namespace UCT.Service
{
    /// <summary>
    ///     字符串、文本处理相关函数
    /// </summary>
    public static class TextProcessingService
    {
        /// <summary>
        ///     检测 '\'字符然后分割文本到子List
        ///     批量处理List string
        /// </summary>
        public static void SplitStringToListWithDelimiter(List<string> parentList, List<string> sonList)
        {
            SplitStringToListWithDelimiter(parentList, sonList, '\\');
        }

        /// <summary>
        ///     检测 '\'字符然后分割文本到子List
        ///     批量处理List string
        /// </summary>
        private static void SplitStringToListWithDelimiter(List<string> parentList,
            List<string> sonList,
            char delimiter)
        {
            sonList.Clear();
            var text = new StringBuilder();
            foreach (var t1 in parentList.SelectMany(t => t))
            {
                if (t1 == delimiter || t1 == ';')
                {
                    sonList.Add(text.ToString());
                    text.Clear();
                }
                else
                {
                    text.Append(t1);
                }
            }
        }


        /// <summary>
        ///     检测 '\'字符然后分割文本到子List
        ///     传入一个string
        /// </summary>
        public static List<string> SplitStringToListWithDelimiter(string parentString, char delimiter = '\\')
        {
            var result = new List<string>();
            var text = new StringBuilder();

            foreach (var t in parentString)
            {
                if (t == delimiter || t == ';')
                {
                    result.Add(text.ToString());
                    text.Clear();
                }
                else
                {
                    text.Append(t);
                }
            }

            return result;
        }

        /// <summary>
        ///     检测输入字符串，返回第一个分隔符之前的部分。
        /// </summary>
        private static string SplitFirstStringWithDelimiter(string input, char delimiter = '\\')
        {
            var result = new StringBuilder();
            foreach (var t in input)
            {
                if (t != delimiter)
                {
                    result.Append(t);
                }
                else
                {
                    break;
                }
            }

            return result.ToString();
        }


        /// <summary>
        ///     检测list的前几个字符是否与传入的string screen相同。
        ///     若相同则分割文本到子List
        /// </summary>
        public static List<string> BatchGetFirstChildStringByPrefix(List<string> parentList,
            string screen)
        {
            var result = new List<string>();
            result.AddRange(from t in parentList where t[..screen.Length] == screen select t[screen.Length..]);
            return result;
        }

        /// <summary>
        ///     传入数据名称返回文本包文本
        /// </summary>
        public static string GetFirstChildStringByPrefix(List<string> parentList,
            string screen,
            bool isIgnoreMiss = false)
        {
            // 收集所有以"screen\"开头的匹配项，并做相应的截取处理
            var matches = (from t in parentList
                where t.Length > screen.Length && SplitFirstStringWithDelimiter(t) == screen
                select t[(screen.Length + 1)..]
                into str
                select str[..^1]).ToList();

            if (matches.Count > 0)
            {
                var random = new Random();
                var randomIndex = random.Next(matches.Count);
                return matches[randomIndex];
            }

            if (isIgnoreMiss)
            {
                return null;
            }

            var nullText = $"<color=yellow><color=#FF6666>{screen}</color> is null!</color>";
            Debug.LogError(nullText);
            return nullText;
        }

        /// <summary>
        ///     用于游戏内文本读取
        ///     传入数据名称返回所有同名的文本包文本
        /// </summary>
        public static List<string> GetAllChildStringsByPrefix(List<string> parentList, string screen)
        {
            var result = (from t in parentList
                where t.Length > screen.Length && SplitFirstStringWithDelimiter(t) == screen
                select t[(screen.Length + 1)..]
                into str
                select str[..^1]).ToList();
            return result;
        }


        /// <summary>
        ///     再分配文本包
        /// </summary>
        public static List<string>[] ClassifyStringsByPrefix(List<string> sourceStrings, string[] prefixes)
        {
            var classifiedSubstrings = new List<string>[prefixes.Length];

            for (var i = 0; i < prefixes.Length; i++)
            {
                classifiedSubstrings[i] = new List<string>();
            }

            foreach (var sourceString in sourceStrings)
            {
                for (var prefixIndex = 0; prefixIndex < prefixes.Length; prefixIndex++)
                {
                    var prefix = prefixes[prefixIndex];

                    if (sourceString.Length <= prefix.Length)
                    {
                        continue;
                    }

                    if (!sourceString.StartsWith(prefix))
                    {
                        continue;
                    }

                    var substring = sourceString[(prefix.Length + 1)..];
                    classifiedSubstrings[prefixIndex].Add(substring);
                }
            }

            return classifiedSubstrings;
        }


        /// <summary>
        ///     换算时间
        /// </summary>
        public static string GetRealTime(int totalSeconds)
        {
            if (totalSeconds < 0)
            {
                totalSeconds = 0;
            }

            totalSeconds /= 60;
            var minutes = totalSeconds % 60;
            var hours = totalSeconds / 60;

            var hoursString = hours < 10 ? $"0{hours}" : $"{hours}";
            var minutesString = minutes < 10 ? $"0{minutes}" : $"{minutes}";

            var result = $"{hoursString}:{minutesString}";
            return result;
        }

        /// <summary>
        ///     从字符串中移除指定位置的子字符串。
        /// </summary>
        public static string RemoveSubstring(string inputString, int startIndex, int endIndex, string add = "")
        {
            if (startIndex < 0 || endIndex >= inputString.Length || startIndex > endIndex)
            {
                Debug.Log("无效的起始和结束位置");
                return inputString;
            }

            var part1 = inputString[..startIndex]; // 从开头到A之前的部分
            var part2 = inputString[(endIndex + 1)..]; // 从B之后到字符串末尾的部分
            var result = part1 + add + part2; // 合并两部分
            return result;
        }

        /// <summary>
        ///     随机生成一串英文字符串。
        /// </summary>
        public static string RandomString(int length = 6,
            string alphabet = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM")
        {
            var text = new StringBuilder();

            for (var i = 0; i < length; i++)
            {
                text.Append(alphabet[UnityEngine.Random.Range(0, alphabet.Length)]);
            }

            return text.ToString();
        }

        /// <summary>
        ///     生成指定Color的字符串形式，并包含原始文本。
        /// </summary>
        /// <param name="color">目标颜色。</param>
        /// <param name="inputString">原始文本。</param>
        /// <returns>带有指定颜色的字符串格式</returns>
        public static string StringColor(Color color, string inputString)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}FF>{inputString}</color>";
        }

        /// <summary>
        ///     生成字符串形式的随机颜色。
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static string RandomStringColor()
        {
            var builder = new StringBuilder("<color=#");
            for (var i = 0; i < 6; i++)
            {
                builder.AppendFormat("{0:X}", UnityEngine.Random.Range(0, 16));
            }

            builder.Append("FF>");
            return builder.ToString();
        }

        /// <summary>
        ///     生成字符串形式的随机颜色。
        /// </summary>
        public static string RandomStringColor(string inputString)
        {
            return RandomStringColor() + inputString + "</color>";
        }

        /// <summary>
        ///     将输入文本中的字母转换为指定的大小写。
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="toLowercase">是否转换为小写</param>
        /// <returns>转换后的字符串</returns>
        public static string ConvertLettersCase(string input, bool toLowercase)
        {
            return toLowercase
                ? input.ToLower(CultureInfo.InvariantCulture)
                : input.ToUpper(CultureInfo.InvariantCulture);
        }


        /// <summary>
        ///     解析输入字符串以获取浮点数范围或特定值，并返回一个随机浮点数。
        ///     - 如果输入字符串包含 "r" 或 "R"，则在指定范围内生成随机值。
        ///     - 如果输入字符串包含 "P" 或 "p"，则返回玩家的位置（基于 `isY` 标志返回 X 或 Y 轴位置）。
        ///     - 如果输入字符串包含 "O" 或 "o" 且带有 '+'，则使用原始浮点数进行调整。
        /// </summary>
        /// <param name="text">表示浮点数或范围的输入字符串（例如 "1R5" 表示范围为 1 到 5）。</param>
        /// <param name="origin">在某些情况下使用的默认浮点值（例如遇到 "O" 或 "o" 时）。</param>
        /// <param name="isY">确定在使用 "P" 或 "p" 时是否返回 Y 轴位置。</param>
        /// <param name="plusSave">对最终结果的可选调整，默认为 0。</param>
        /// <returns>基于输入字符串的随机浮点数，如果未找到随机范围则返回原始值。</returns>
        private static float ParseFloatWithSpecialCharacters(string text,
            float origin,
            bool isY = false,
            float plusSave = 0)
        {
            var plus = plusSave;
            while (true)
            {
                var save = new StringBuilder();
                if (text[0] != 'O' && text[0] != 'o' && text[0] != 'P' && text[0] != 'p')
                {
                    float x1 = 0;
                    var isHaveR = IsParseFloatHaveR(text, save, ref x1, ref plus);

                    if (!isHaveR)
                    {
                        return plus + float.Parse(text);
                    }

                    var x2 = float.Parse(save.ToString());
                    return plus + UnityEngine.Random.Range(x1, x2);
                }

                if (IsParseFloatHaveOther(text, origin, isY, out var floatWithSpecialCharacters))
                {
                    return floatWithSpecialCharacters;
                }

                text = text[2..];
                plus = origin;
            }
        }

        private static bool IsParseFloatHaveOther(string text,
            float origin,
            bool isY,
            out float floatWithSpecialCharacters)
        {
            floatWithSpecialCharacters = 0;
            if (text is "P" or "p")
            {
                floatWithSpecialCharacters = isY
                    ? MainControl.Instance.battlePlayerController.transform.position.y
                    : MainControl.Instance.battlePlayerController.transform.position.x;
                return true;
            }

            if (text.Length > 1 && (text[0] == 'O' || text[0] == 'o') && text[1] == '+')
            {
                return false;
            }

            floatWithSpecialCharacters = origin;
            return true;
        }

        private static bool IsParseFloatHaveR(string text, StringBuilder save, ref float x1, ref float plus)
        {
            var isHaveR = false;
            foreach (var t in text)
            {
                switch (t)
                {
                    case 'r' or 'R' when !isHaveR:
                        x1 = float.Parse(save.ToString());
                        save.Clear();
                        isHaveR = true;
                        break;
                    case '+':
                        plus = float.Parse(save.ToString());
                        save.Clear();
                        break;
                    default:
                        save.Append(t);
                        break;
                }
            }

            return isHaveR;
        }

        /// <summary>
        ///     将 Vector2 转换为形如 (x,y) 的字符串表示
        /// </summary>
        public static string RealVector2ToStringVector2(Vector2 vector2)
        {
            return $"({vector2.x},{vector2.y})";
        }

        /// <summary>
        ///     输入形如(x,y)的字符串向量，返回Vector2
        /// </summary>
        public static Vector2 StringVector2ToRealVector2(string stringVector2)
        {
            stringVector2 = stringVector2.Trim('(', ')');

            var components = stringVector2.Split(',');

            if (components.Length == 2 && float.TryParse(components[0], out var x) &&
                float.TryParse(components[1], out var y))
            {
                return new Vector2(x, y);
            }

            Debug.LogWarning($"输入的字符串 \"{stringVector2}\" 格式不正确，应形如 (x,y)。");
            return new Vector2();
        }

        /// <summary>
        ///     输入形如(x,y)的字符串向量，返回Vector2
        ///     使用ParseFloatWithSpecialCharacters进行转换。
        /// </summary>
        public static Vector2 StringVector2ToRealVector2(string stringVector2, Vector3 origin)
        {
            stringVector2 = stringVector2.Substring(1, stringVector2.Length - 2) + ",";
            var realVector2 = Vector2.zero;
            var save = new StringBuilder();
            var isSetX = false;
            foreach (var t in stringVector2)
            {
                if (t == ',')
                {
                    if (!isSetX)
                    {
                        realVector2.x = ParseFloatWithSpecialCharacters(save.ToString(), origin.x);
                        isSetX = true;
                        save.Clear();
                    }
                    else
                    {
                        realVector2.y = ParseFloatWithSpecialCharacters(save.ToString(), origin.y, true);
                        break;
                    }
                }
                else
                {
                    save.Append(t);
                }
            }

            return realVector2;
        }

        public static string ToFirstLetterUpperCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }

            return char.ToUpper(input[0]) + input[1..];
        }
    }
}