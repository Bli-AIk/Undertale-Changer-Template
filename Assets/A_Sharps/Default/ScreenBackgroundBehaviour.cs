using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���ƹ���ȫ��/��Ļ�߿�Ĺ�����(�ݷ�
/// </summary>
public class ScreenBackgroundBehaviour : MonoBehaviour
{
    GameObject backBlack;
    // Start is called before the first frame update
    void Start()
    {
        backBlack = transform.Find("Back").gameObject;

    }

    // Update is called once per frame
    void Update()
    {
        
        if (Screen.fullScreen != MainControl.instance.OverwroldControl.fullScreen)
        {
            Screen.SetResolution(Screen.width, Screen.height, MainControl.instance.OverwroldControl.fullScreen);
        }
        backBlack.SetActive(MainControl.instance.OverwroldControl.backGround);
    }
}
