using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// Used to insert the option system in OW, which is consistent with the UT option but different from the DR option.
/// Heart will be added in scenes with options.
/// Calling within a text package
/// </summary>
public class OverworldTalkSelect : MonoBehaviour
{
    public static OverworldTalkSelect instance;
    public int select;
    Image heart;
    bool canSelect;
    TypeWritter typeWritter;
    public List<string> texts;
    public string typeText;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        typeWritter = GameObject.Find("BackpackCanvas").GetComponent<TypeWritter>();
        Transform heartTrans = transform.Find("RawImage/Talk/UITalk/Heart");
        GameObject heartObj;
        if (!heartTrans)
            heartObj = Instantiate(new GameObject(), transform.Find("RawImage/Talk/UITalk"));
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

    
    void Update()
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
                    case 0://Left option selected
                        switch (typeText)
                        {
                            /*
                            Typewriter Example
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
                    case 1://Right option selected
                        break;
                }
                heart.color = Color.clear;
                canSelect = false;
                return;
            }

            
        }
    }
}
