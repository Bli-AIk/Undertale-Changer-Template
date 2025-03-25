using System.Text.RegularExpressions;

namespace UCT.Service
{
    public static class TextMeshProRichTextChecker
    {
        private static readonly string[] RichTextTags =
        {
            "align", "allcaps", "alpha", "b", "br", "color", "cspace", "font", "font-weight",
            "gradient", "i", "indent", "line-height", "line-indent", "link", "lowercase",
            "margin", "mark", "mspace", "nobr", "noparse", "page", "pos", "rotate", "s",
            "size", "smallcaps", "space", "sprite", "strikethrough", "style", "sub", "sup",
            "u", "uppercase", "voffset", "width"
        };

        private static readonly Regex RichTextRegex = new(
            $@"</?({string.Join("|", RichTextTags)})\b(\s*=[^>]+)?>",
            RegexOptions.IgnoreCase | RegexOptions.Compiled
        );

        /// <summary>
        ///     检测字符串是否包含富文本标签
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>如果包含富文本标签，则返回 true，否则返回 false</returns>
        public static bool ContainsRichText(string input)
        {
            return !string.IsNullOrEmpty(input) && RichTextRegex.IsMatch(input);
        }
    }
}