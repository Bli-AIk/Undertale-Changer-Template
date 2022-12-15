 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
/// <summary>
/// 用于字体匹配及双语字体数据修正
/// </summary>
public class TextChanger : MonoBehaviour
{
    //public TMP_FontAsset assetback;
    TMP_Text tmp;
    public bool notUGUI;
    public bool width;//若中英混搭 则true就完事了
    [Header("US/CN")]
    public Vector4[] Options;
    //public float[] sizes;
    
    //public bool no = true;
    private void Start()
    {
        Set();
        Change();
    }
    public void Set()
    {
        if (notUGUI)
        {
            tmp = GetComponent<TextMeshPro>();
        }
        else
        {
            tmp = GetComponent<TextMeshProUGUI>();
        }
        /*
       while (tmp.font != assetback)
        {
            tmp.font = assetback;
        }
        */
        width = MainControl.instance.OverwroldControl.textWidth;
        /*
        if (tmp.font != assetback)
            tmp.font = assetback;
        */
        }

    
    public void Change()
    {
        if (tmp != null)
        {
            if (notUGUI)
            {
                tmp.characterSpacing = Options[Convert.ToInt32(width)].x;
                tmp.wordSpacing = Options[Convert.ToInt32(width)].y;
                tmp.lineSpacing = Options[Convert.ToInt32(width)].z;
                tmp.paragraphSpacing = Options[Convert.ToInt32(width)].w;
                //textMeshPro.fontSize = sizes[Convert.ToInt32(width)];
            }
            else
            {
                tmp.characterSpacing = Options[Convert.ToInt32(width)].x;
                tmp.wordSpacing = Options[Convert.ToInt32(width)].y;
                tmp.lineSpacing = Options[Convert.ToInt32(width)].z;
                tmp.paragraphSpacing = Options[Convert.ToInt32(width)].w;
                //textMeshProUGUI.fontSize = sizes[Convert.ToInt32(width)];
            }
        }
        else
        {
            Set();
            Change();
        }
    }

}
