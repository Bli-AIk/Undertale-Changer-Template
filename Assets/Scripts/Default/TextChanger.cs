using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
/// <summary>
/// Used for font matching and bilingual font data correction
/// </summary>
public class TextChanger : MonoBehaviour
{
    //public TMP_FontAsset assetback;
    TMP_Text tmp;
    public bool width;//If you mix and match Chinese and English, true will be done
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
        tmp = GetComponent<TMP_Text>();

        /*
       while (tmp.font != assetback)
        {
            tmp.font = assetback;
        }
        */
        width = MainControl.instance.OverworldControl.textWidth;
        /*
        if (tmp.font != assetback)
            tmp.font = assetback;
        */
    }


    public void Change()
    {
        if (tmp != null)
        {
            tmp.characterSpacing = Options[Convert.ToInt32(width)].x;
            tmp.wordSpacing = Options[Convert.ToInt32(width)].y;
            tmp.lineSpacing = Options[Convert.ToInt32(width)].z;
            tmp.paragraphSpacing = Options[Convert.ToInt32(width)].w;

        }
        else
        {
            Set();
            Change();
        }
    }

}
