using Log;
using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Change the position of the dialog box in Overworld.
/// </summary>
public class TalkUIPositionChanger : MonoBehaviour
{
    public static TalkUIPositionChanger instance;
    public bool isUp;
    public bool haveHead;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        //gameObject.SetActive(false);
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -50);
    }

    public void Change(bool updateHeader = true, bool haveHeader = false, bool cleaner = true, TypeWritter typeWritter = null)
    {
        if (cleaner)
        {
            BackpackBehaviour.instance.typeMessage.text = "";
            if (typeWritter != null)
                typeWritter.endString = "";
        }

        if (!updateHeader)
            return;
        haveHead = haveHeader;

        if (isUp)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 7.77f, transform.localPosition.z);
            BackpackBehaviour.instance.typeMessage.rectTransform.anchoredPosition = new Vector2(10 + 115f * Convert.ToInt32(haveHead), 139);
        }
        else
        {
            transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
            BackpackBehaviour.instance.typeMessage.rectTransform.anchoredPosition = new Vector2(10 + 115f * Convert.ToInt32(haveHead), -170);
        }
    }




    private void OnEnable()
    {
        if (BackpackBehaviour.instance.typeMessage != null)
            BackpackBehaviour.instance.typeMessage.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (BackpackBehaviour.instance.typeMessage != null)
            BackpackBehaviour.instance.typeMessage.gameObject.SetActive(false);
    }
}
