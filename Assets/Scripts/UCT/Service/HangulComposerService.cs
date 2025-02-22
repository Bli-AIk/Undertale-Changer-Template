using System;

namespace UCT.Service
{
    /// <summary>
    ///     韩语拼写相关函数
    /// </summary>
    public static class HangulComposerService
    {
        // 定义韩语的初声、中声、终声字母表
        private static readonly string[] Chosung =
            { "ㄱ", "ㄲ", "ㄴ", "ㄷ", "ㄸ", "ㄹ", "ㅁ", "ㅂ", "ㅃ", "ㅅ", "ㅆ", "ㅇ", "ㅈ", "ㅉ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ" };

        private static readonly string[] Jungsung =
            { "ㅏ", "ㅐ", "ㅑ", "ㅒ", "ㅓ", "ㅔ", "ㅕ", "ㅖ", "ㅗ", "ㅘ", "ㅙ", "ㅚ", "ㅛ", "ㅜ", "ㅝ", "ㅞ", "ㅟ", "ㅠ", "ㅡ", "ㅢ", "ㅣ" };

        private static readonly string[] Jongsung =
        {
            "", "ㄱ", "ㄲ", "ㄳ", "ㄴ", "ㄵ", "ㄶ", "ㄷ", "ㄹ", "ㄺ", "ㄻ", "ㄼ", "ㄽ", "ㄾ", "ㄿ", "ㅀ", "ㅁ", "ㅂ", "ㅄ", "ㅅ", "ㅆ", "ㅇ",
            "ㅈ", "ㅊ", "ㅋ", "ㅌ", "ㅍ", "ㅎ"
        };

        // 拼出一个完整的韩语音节
        // 拼出一个完整的韩语音节，输入格式为类似 "한" 的字符串
        public static char ComposeHangul(string syllable)
        {
            if (syllable.Length is < 2 or > 3)
            {
                throw new ArgumentException("输入字符串的长度应为 2 或 3");
            }

            if (syllable[0] == '√')
            {
                if (syllable[1] != '√')
                {
                    return syllable[1];
                }
            }

            if (syllable[1] == '√' && syllable[0] != '√')
            {
                return syllable[0];
            }

            if (syllable[1] == '√' && syllable[0] == '√')
            {
                return syllable[2];
            }

            if (syllable[2] == '√')
            {
                syllable = syllable[..2];
            }

            // 拆解输入的字符串
            var cho = syllable[0].ToString(); // 初声
            var jung = syllable[1].ToString(); // 中声
            var jong = syllable.Length == 3 ? syllable[2].ToString() : ""; // 终声（如果有的话）

            // 获取初声、中声和终声的索引
            var choIndex = Array.IndexOf(Chosung, cho);
            var jungIndex = Array.IndexOf(Jungsung, jung);
            var jongIndex = Array.IndexOf(Jongsung, jong);

            if (choIndex == -1 || jungIndex == -1 || jongIndex == -1)
            {
                throw new ArgumentException("无效的韩语字母组合");
            }

            // 计算音节的Unicode编码
            var code = 0xAC00 + (choIndex * 21 + jungIndex) * 28 + jongIndex;
            return (char)code;
        }
    }
}