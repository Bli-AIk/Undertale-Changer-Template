using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Used to insert a system of options in the OW, consistent with those of the UT and different from those of the DR.
/// Heart will be added in scenes containing options.
/// Called from within a text package
/// </summary>
public class OverworldTalkSelect : MonoBehaviour
{
    public static OverworldTalkSelect instance;
    public int select;
    private Image heart;
    private bool canSelect;
    private TypeWritter typeWritter;
    public List<string> texts;
    public string typeText;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        typeWritter = GameObject.Find("BackpackCanvas").GetComponent<TypeWritter>();
        Transform heartTrans = BackpackBehaviour.instance.typeMessage.transform.Find("Heart");
        GameObject heartObj;
        if (!heartTrans)
            heartObj = Instantiate(new GameObject(), BackpackBehaviour.instance.typeMessage.transform);
        else heartObj = heartTrans.gameObject;

        heart = heartObj.AddComponent<Image>() ?? heartObj.GetComponent<Image>();
        heart.rectTransform.sizeDelta = 16 * Vector3.one;
        heart.sprite = Resources.Load<Sprite>("Sprites/Soul");
        heart.color = Color.clear;
    }

    public void Open()
    {
        select = 0;
        heart.rectTransform.anchoredPosition = new Vector2(-143.3f + Convert.ToInt32(select) * 192.5f, -18.8f);
        heart.color = Color.red;
        canSelect = true;
    }

    private void Update()
    {
        if (canSelect)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
            {
                if (select > 0)
                    select--;
                else select = 1;
                heart.rectTransform.anchoredPosition = new Vector2(-143.3f + Convert.ToInt32(select) * 192.5f, -18.8f);
                AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
            }
            else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
            {
                if (select < 1)
                    select++;
                else select = 0;
                heart.rectTransform.anchoredPosition = new Vector2(-143.3f + Convert.ToInt32(select) * 192.5f, -18.8f);
                AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
            }

            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
            {
                typeWritter.TypeStop();
                switch (select)
                {
                    case 0:
                    //left option selected
                        switch (typeText)
                        {
                            /*
                            打字机示例
                            case "XXX":

                        typeWritter.TypeOpen(MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.owTextsSave, texts[0]), false, 0, 1);
                        break;
                            */

                            case "BackMenu":
                                typeWritter.forceReturn = true;
                                MainControl.instance.OutBlack("Menu", Color.black, true, 0f);
                                AudioController.instance.audioSource.volume = 0;
                                break;

                            case "Select":
                                AudioController.instance.GetFx(2, MainControl.instance.AudioControl.fxClipBattle);
                                break;

                            default:
                                break;
                        }
                        break;

                    case 1:
                    //Right option selected
                        break;
                }
                heart.color = Color.clear;
                canSelect = false;
                return;
            }
        }
    }
}
