using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

/// <summary>
/// 修改Overworld中对话框的位置
/// </summary>
public class TalkUIPositionChanger : MonoBehaviour
{
    public bool isUp;
    public bool haveHead;
    TextMeshProUGUI talk;
    private void Awake()
    {
        talk = GameObject.Find("BackpackCanvas/RawImage/Talk/UITalk").GetComponent<TextMeshProUGUI>();
    }
    
    void Start()
    {
        //gameObject.SetActive(false);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -50);
    }

    
    public void Change(bool forceFlash = true, bool haveHeader = false, bool cleaner = true, TypeWritter typeWritter = null)
    {
        if (forceFlash)
            haveHead = haveHeader;
        if (cleaner)
        {
            talk.text = "";
            if (typeWritter != null)
                typeWritter.endString = "";
        }

        if (isUp)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 7.77f, transform.localPosition.z);
            talk.rectTransform.anchoredPosition = new Vector2(10 + 115f * Convert.ToInt32(haveHead), 139);
        }
        else
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            talk.rectTransform.anchoredPosition = new Vector2(10 + 115f * Convert.ToInt32(haveHead), -170);
        }
    }
    private void OnEnable()
    {
        if (talk != null)
            talk.gameObject.SetActive(true);
    }
    private void OnDisable()
    {
        if (talk != null)
            talk.gameObject.SetActive(false);
    }
}
