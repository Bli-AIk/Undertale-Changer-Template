using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 生成一系列脑梗加密文本
/// </summary>
public class DebugSpTextSummon : MonoBehaviour
{
    public List<string> puter;

    private void Start()
    {
        puter.Add(LetterTonumber(puter[0]));
        puter.Add(numberTonumberLetter(puter[1]));
        puter.Add(NineTonumber(puter[2]));
        puter.Add(backString(puter[3]));
    }

    /// <summary>
    /// 原字母文本转化为数字
    /// </summary>
    private string LetterTonumber(string str)
    {
        string returnText = "";
        string text = " abcdefghijklmnopqrstuvwxyz";
        string textCap = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < str.Length; i++)
        {
            for (int j = 0; j < text.Length; j++)
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
    private string numberTonumberLetter(string str)
    {
        string returnText = "";
        string text = "0123456789 ";
        string textB = "ZOTtFfSsEN ";

        for (int i = 0; i < str.Length; i++)
        {
            for (int j = 0; j < text.Length; j++)
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
    private string NineTonumber(string str)
    {
        string returnText = "";
        string text = " abcdefghijklmnopqrstuvwxyz";
        string textCap = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string back = "112122233132334142435152536162637172737481828391929394";
        for (int i = 0; i < str.Length; i++)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (str[i] == text[j] || str[i] == textCap[j])
                {
                    returnText += back[2 * j].ToString() + back[2 * j + 1].ToString();
                    break;
                }
            }
        }
        return returnText;
    }

    /// <summary>
    /// 倒车请注意
    /// </summary>
    private string backString(string str)
    {
        string returnText = "";

        for (int i = str.Length - 1; i >= 0; i--)
        {
            returnText += str[i];
        }
        return returnText;
    }
}