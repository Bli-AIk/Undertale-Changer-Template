using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Generate a series of foolish encrypted text
/// </summary>
public class DebugSpTextSummon : MonoBehaviour
{
    public List<string> puter;
    void Start()
    {
        puter.Add(LetterToNum(puter[0]));
        puter.Add(NumToNumLetter(puter[1]));
        puter.Add(NineToNum(puter[2]));
        puter.Add(backString(puter[3]));
    }
    /// <summary>
    /// 原字母文本转化为数字
    /// </summary>
    string LetterToNum(string str)
    {
        string returnText = "";
        string text = " abcdefghijklmnopqrstuvwxyz";
        string textCap = " ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        for (int i = 0; i < str.Length; i++)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (str[i] == text[j]|| str[i] == textCap[j])
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
    string NumToNumLetter(string str)
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
    string NineToNum(string str)
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
    string backString(string str)
    {
        string returnText = "";

        for (int i = str.Length - 1; i >= 0; i--)
        {
            returnText += str[i];
        }
        return returnText;
    }
}
