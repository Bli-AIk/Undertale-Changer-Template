using System.Collections.Generic;
using UnityEngine;

namespace Debug
{
    /// <summary>
    /// 生成一系列脑梗加密文本
    /// </summary>
    public class DebugSpTextSummon : MonoBehaviour
    {
        public List<string> puter;

        private void Start()
        {
            puter.Add(LetterToNumber(puter[0]));
            puter.Add(NumberToNumberLetter(puter[1]));
            puter.Add(NineToNumber(puter[2]));
            puter.Add(BackString(puter[3]));
        }

        /// <summary>
        /// 原字母文本转化为数字
        /// </summary>
        private string LetterToNumber(string str)
        {
            var returnText = "";
            var text = " abcdefghijklmnopqrstuvwxyz";
            var textCap = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            for (var i = 0; i < str.Length; i++)
            {
                for (var j = 0; j < text.Length; j++)
                {
                    if (str[i] == text[j] || str[i] == textCap[j])
                    {
                        if (j > 9)
                            returnText += j.ToString();
                        else
                            returnText += "0" + j;
                        break;
                    }
                }
            }
            return returnText;
        }

        /// <summary>
        /// 数字的首字母表示数字
        /// 数字换算为字母
        /// </summary>
        private string NumberToNumberLetter(string str)
        {
            var returnText = "";
            var text = "0123456789 ";
            var textB = "ZOTtFfSsEN ";

            for (var i = 0; i < str.Length; i++)
            {
                for (var j = 0; j < text.Length; j++)
                {
                    if (str[i] == text[j])
                    {
                        returnText += textB[j];
                        break;
                    }
                }
            }
            return returnText;
        }

        /// <summary>
        /// 26字母换算为九键数字
        /// </summary>
        private string NineToNumber(string str)
        {
            var returnText = "";
            var text = " abcdefghijklmnopqrstuvwxyz";
            var textCap = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var back = "112122233132334142435152536162637172737481828391929394";
            for (var i = 0; i < str.Length; i++)
            {
                for (var j = 0; j < text.Length; j++)
                {
                    if (str[i] == text[j] || str[i] == textCap[j])
                    {
                        returnText += back[2 * j] + back[2 * j + 1].ToString();
                        break;
                    }
                }
            }
            return returnText;
        }

        /// <summary>
        /// 倒车请注意
        /// </summary>
        private string BackString(string str)
        {
            var returnText = "";

            for (var i = str.Length - 1; i >= 0; i--)
            {
                returnText += str[i];
            }
            return returnText;
        }
    }
}