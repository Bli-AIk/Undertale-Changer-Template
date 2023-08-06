using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
/// <summary>
/// 用于在OW插入选项系统，与UT的选项一致，而不同于DR的选项。
/// 在含有选项的场景内会添加heart。
/// 在文本包内调用
/// </summary>
public class OverworldTalkSelect : MonoBehaviour
{
    public int select;
    Image heart;
    bool canSelect;
    TypeWritter typeWritter;
    public List<string> texts;
    public List<Sprite> sprites;
    float saveFloat;
    public string typeText;
    
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
                    case 0://选择了左侧选项“是”
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
                                MainControl.instance.OverworldControl.pause = true;
                                MainControl.instance.OutWhite("Menu");
                                AudioController.instance.audioSource.volume = 0;
                                break;
                            
                            default:
                                break;
                        }
                        break;
                    case 1://选择了右侧选项“否”
                        break;
                }
                heart.color = Color.clear;
                canSelect = false;
                typeText = "";
                return;
            }

            
        }
    }
}
