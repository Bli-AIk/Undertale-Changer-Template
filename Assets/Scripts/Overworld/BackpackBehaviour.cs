using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Managing the OW Backpack System
/// </summary>
public class BackpackBehaviour : MonoBehaviour
{
    public static BackpackBehaviour instance;

    public int select, sonSelect, sonUse;
    private int sonSelectMax;
    public RawImage rawImage;
    private RectTransform backpack, UIMessage;
    private TextMeshProUGUI uiItems, uiName, uiTexts, uiSelect;
    public TextMeshProUGUI typeMessage;
    private Image heart;
    private float clock;
    private GameObject BackpackUILeft, BackpackUIRight, player, mainCamera;
    private GameObject BackpackUIRightPoint2, BackpackUIRightPoint3;

    public GameObject saveBack;
    public TextMeshProUGUI saveUI;
    public RectTransform saveUIHeart;

    public TypeWritter typeWritter;

    private void Awake()
    {
        instance = this;
        rawImage = GameObject.Find("BackpackCanvas/RawImage").GetComponent<RawImage>();
        typeMessage = GameObject.Find("BackpackCanvas/RawImage/Talk/UITalk").GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        typeWritter = GetComponent<TypeWritter>();
        backpack = transform.Find("RawImage/Backpack").GetComponent<RectTransform>();
        uiItems = backpack.Find("UIItems").GetComponent<TextMeshProUGUI>();
        UIMessage = backpack.Find("UIMessage").GetComponent<RectTransform>();
        uiName = backpack.Find("UIMessage/UIName").GetComponent<TextMeshProUGUI>();
        uiTexts = backpack.Find("UIMessage/UITexts").GetComponent<TextMeshProUGUI>();
        uiSelect = backpack.Find("UISelect").GetComponent<TextMeshProUGUI>();
        heart = backpack.Find("Heart").GetComponent<Image>();
        BackpackUILeft = GameObject.Find("Main Camera/BackpackUI/Left2");
        BackpackUIRight = GameObject.Find("Main Camera/BackpackUI/Right");
        saveBack = GameObject.Find("Main Camera/Save");
        saveUI = GameObject.Find("BackpackCanvas/RawImage/Talk/UISave").GetComponent<TextMeshProUGUI>();
        saveUIHeart = GameObject.Find("BackpackCanvas/RawImage/Talk/UISave/Heart").GetComponent<RectTransform>();
        player = GameObject.Find("Player");
        mainCamera = GameObject.Find("Main Camera");
        BackpackUIRightPoint2 = BackpackUIRight.transform.Find("Point2").gameObject;
        BackpackUIRightPoint3 = BackpackUIRight.transform.Find("Point3").gameObject;
        BackpackUILeft.transform.parent.localPosition = new Vector3(BackpackUILeft.transform.parent.localPosition.x, BackpackUILeft.transform.parent.localPosition.y, -50);
        backpack.gameObject.SetActive(false);
        MainControl.instance.PlayerControl.canMove = true;

        SuitResolution();
    }

    public void SuitResolution()
    {
        if (!MainControl.instance.OverworldControl.hdResolution)
        {
            RectTransform rectTransform = rawImage.rectTransform;

            rectTransform.offsetMin = new Vector2(0, 0);

            rectTransform.offsetMax = new Vector2(0, 0);

            rectTransform.localScale = Vector3.one;
        }
        else
        {
            RectTransform rectTransform = rawImage.rectTransform;

            rectTransform.offsetMin = new Vector2(107, 0);

            rectTransform.offsetMax = new Vector2(-107, 0);

            rectTransform.localScale = Vector3.one * 0.89f;
        }
    }

    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
        {
            return;
        }

        TalkUIPositionChanger.instance.isUp = player.transform.position.y < mainCamera.transform.position.y - 1.25f;

        if (player.transform.position.y >= mainCamera.transform.position.y - 1.25f)
        {
            UIMessage.anchoredPosition = new Vector2(0, 270);
            BackpackUILeft.transform.localPosition = new Vector3(0, 6.75f, 5);
        }
        else
        {
            UIMessage.anchoredPosition = Vector2.zero;
            BackpackUILeft.transform.localPosition = new Vector3(0, 0, 5);
        }
        if (clock > 0)
        {
            clock -= Time.deltaTime;
        }
        else if (select > 0)
        {
            backpack.gameObject.SetActive(true);
        }
        if (sonUse != 4)
        {
            uiItems.gameObject.SetActive(sonSelect != 0);
            if (sonSelect != 0)
            {
                BackpackUIRight.transform.localPosition = new Vector3(BackpackUIRight.transform.localPosition.x, BackpackUIRight.transform.localPosition.y, 5);
            }
            else
            {
                BackpackUIRight.transform.localPosition = new Vector3(BackpackUIRight.transform.localPosition.x, BackpackUIRight.transform.localPosition.y, -50);
            }
        }
        else
        {
            uiItems.gameObject.SetActive(false);
            BackpackUIRight.transform.localPosition = new Vector3(BackpackUIRight.transform.localPosition.x, BackpackUIRight.transform.localPosition.y, -50);
        }
        if ((MainControl.instance.KeyArrowToControl(KeyCode.X) || MainControl.instance.KeyArrowToControl(KeyCode.C)) && sonSelect == 0)
        {
            if (backpack.gameObject.activeSelf)
            //Close
            {
                BackpackExit();
            }
            else if (MainControl.instance.KeyArrowToControl(KeyCode.C))
            //Open
            {
                AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                MainControl.instance.PlayerControl.myItems = MainControl.instance.ListOrderChanger(MainControl.instance.PlayerControl.myItems);
                string uiSelectPlusColor = "";
                sonSelectMax = 8;
                for (int i = 0; i < MainControl.instance.PlayerControl.myItems.Count; i++)
                {
                    if (MainControl.instance.PlayerControl.myItems[i] == 0)
                    {
                        if (i == 0)
                            uiSelectPlusColor = "<color=grey>";
                        sonSelectMax = i;
                        break;
                    }
                }
                sonUse = 0;
                BackpackUILeft.transform.parent.localPosition = new Vector3(BackpackUILeft.transform.parent.localPosition.x, BackpackUILeft.transform.parent.localPosition.y, 5);
                clock = 0.01f;
                select = 1;
                MainControl.instance.PlayerControl.canMove = false;

                uiSelect.text = uiSelectPlusColor + MainControl.instance.ItemControl.itemTextMaxData[0].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[0].Length - 1);
                uiName.text = MainControl.instance.PlayerControl.playerName;
                uiTexts.text = "LV " + MainControl.instance.PlayerControl.lv + "\nHP " + MainControl.instance.PlayerControl.hp + "/" + MainControl.instance.PlayerControl.hpMax + "\nG " + MainControl.instance.PlayerControl.gold;

                if (select == 1)
                    FlashBackpackUIRightPoint(-8.55f * 0.5f);
                else
                    FlashBackpackUIRightPoint(-11.3f * 0.5f);
            }
        }
        if (backpack.gameObject.activeSelf && !typeWritter.isTyping)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
            {
                MainControl.instance.playerBehaviour.owTimer = 0.1f;

                if (select == 1)
                {
                    if (sonUse == 0 && MainControl.instance.PlayerControl.myItems[0] != 0)
                    {
                        AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                    }

                    if (sonSelectMax != 0)
                    {
                        if (sonSelect == 0)
                        {
                            sonSelect = 1;
                            uiItems.text = "";
                            /*
                            BackpackUIRightPoint2.transform.localPosition = new Vector3(BackpackUIRightPoint2.transform.localPosition.x, -8.55f);
                            BackpackUIRightPoint3.transform.localPosition = new Vector3(BackpackUIRightPoint3.transform.localPosition.x, -8.55f);
                            */
                            for (int i = 0; i < MainControl.instance.PlayerControl.myItems.Count; i++)
                            {
                                if (MainControl.instance.PlayerControl.myItems[i] != 0)
                                    uiItems.text += " " + MainControl.instance.ItemIdGetName(MainControl.instance.PlayerControl.myItems[i], "Auto", 0) + "\n";
                                else uiItems.text += "\n";
                            }

                            uiItems.text += "\n " + MainControl.instance.ItemControl.itemTextMaxData[8].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[8].Length - 1)
                                + MainControl.instance.ItemControl.itemTextMaxData[9].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[9].Length - 1) +
                                MainControl.instance.ItemControl.itemTextMaxData[10].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[10].Length - 1);
                        }
                        else
                        {
                            int plusText;
                            if (MainControl.instance.PlayerControl.myItems[sonSelect - 1] >= 20000)
                                plusText = -20000 + MainControl.instance.ItemControl.itemFoods.Count / 3 + MainControl.instance.ItemControl.itemArms.Count / 2;
                            else if (MainControl.instance.PlayerControl.myItems[sonSelect - 1] >= 10000)
                                plusText = -10000 + MainControl.instance.ItemControl.itemFoods.Count / 3;
                            else plusText = 0;

                            switch (sonUse)
                            {
                                case 0:
                                    sonUse = 1;
                                    break;

                                case 1:
                                    sonUse = 4;
                                    MainControl.instance.UseItem(typeWritter, typeMessage, sonSelect, plusText);
                                    uiTexts.text = "LV " + MainControl.instance.PlayerControl.lv + "\nHP " + MainControl.instance.PlayerControl.hp + "/" + MainControl.instance.PlayerControl.hpMax + "\nG " + MainControl.instance.PlayerControl.gold;

                                    goto default;
                                case 2:
                                    sonUse = 4;
                                    typeWritter.TypeOpen(MainControl.instance.ItemControl.itemTextMaxItemSon[(MainControl.instance.PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 2], false, 0, 1, typeMessage);

                                    goto default;
                                case 3:
                                    sonUse = 4;
                                    typeWritter.TypeOpen(MainControl.instance.ItemControl.itemTextMaxItemSon[(MainControl.instance.PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 1], false, 0, 1, typeMessage);

                                    MainControl.instance.PlayerControl.myItems[sonSelect - 1] = 0;
                                    goto default;
                                default:
                                    if (!typeWritter.isTyping)
                                    {
                                        BackpackExit();
                                        break;
                                    }
                                    if (TalkUIPositionChanger.instance.transform.localPosition.z < 0)
                                    {
                                        TalkUIPositionChanger.instance.Change();
                                        TalkUIPositionChanger.instance.transform.localPosition = new Vector3(TalkUIPositionChanger.instance.transform.localPosition.x, TalkUIPositionChanger.instance.transform.localPosition.y, 5);
                                        //DebugLogger.LogWarning(talkUI.transform.localPosition.z);
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (select == 2)
                {
                    if (sonSelect == 0)
                    {
                        sonSelect = 1;
                        AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                    }
                    uiItems.text = "";
                    /*
                    BackpackUIRightPoint2.transform.localPosition = new Vector3(BackpackUIRightPoint2.transform.localPosition.x, -11.3f);
                    BackpackUIRightPoint3.transform.localPosition = new Vector3(BackpackUIRightPoint3.transform.localPosition.x, -11.3f);
                    */
                    uiItems.text = "\"" + MainControl.instance.PlayerControl.playerName + "\"\n\nLV " + MainControl.instance.PlayerControl.lv + "\nHP " + MainControl.instance.PlayerControl.hp +
                           "<size=12> </size>/<size=12> </size>" + MainControl.instance.PlayerControl.hpMax + "<size=1>\n</size>\n" + MainControl.instance.ItemControl.itemTextMaxData[1].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[1].Length - 1)
                           + "<size=12> </size>" + (MainControl.instance.PlayerControl.atk - 10) + "<size=6> </size>(" + MainControl.instance.PlayerControl.wearAtk + ")<size=15> </size> EXP:" + MainControl.instance.PlayerControl.exp +
                           "\n" + MainControl.instance.ItemControl.itemTextMaxData[2].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[2].Length - 1) + "<size=12> </size>" + (MainControl.instance.PlayerControl.def - 10) +
                           "<size=6> </size>(" + MainControl.instance.PlayerControl.wearDef + ")<size=15> </size>" + MainControl.instance.ItemControl.itemTextMaxData[3].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[3].Length - 1)
                           + ":" + MainControl.instance.PlayerControl.nextExp + "\n\n" + MainControl.instance.ItemControl.itemTextMaxData[4].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[4].Length - 1) + MainControl.instance.ItemIdGetName(MainControl.instance.PlayerControl.wearArm, "Auto", 0)
                           + "\n" + MainControl.instance.ItemControl.itemTextMaxData[5].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[5].Length - 1) + MainControl.instance.ItemIdGetName(MainControl.instance.PlayerControl.wearArmor, "Auto", 0)
                           + "\n<size=13>\n</size>" + MainControl.instance.ItemControl.itemTextMaxData[6].Substring(0, MainControl.instance.ItemControl.itemTextMaxData[6].Length - 1) + MainControl.instance.PlayerControl.gold;
                }
            }
            if ((MainControl.instance.KeyArrowToControl(KeyCode.X)) && sonUse != 4)
            {
                if (select == 2 || (select == 1 && sonUse == 0))
                    sonSelect = 0;
                else if (sonUse != 0)
                    sonUse = 0;
            }

            /*
            if (MainControl.instance.OverworldControl.background)
            //Border Adaptation
                backpack.localScale = Vector3.one * 0.8888f;
            else
            */
            backpack.localScale = Vector3.one;

            if (sonUse != 4)
            {
                if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                {
                    if (select > 1 && sonSelect == 0)
                    {
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        select -= 1;
                    }
                    if (select == 1 && sonSelect > 1 && sonSelect != 0)
                    {
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        sonSelect -= 1;
                    }
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                {
                    if (select < 2 && sonSelect == 0)
                    {
                        select += 1;
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                    }
                    if (select == 1 && sonSelect < sonSelectMax && sonSelect != 0)
                    {
                        sonSelect += 1;
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                    }
                }
            }
            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
            {
                if (select == 1)
                    FlashBackpackUIRightPoint(-8.55f * 0.5f);
                else
                    FlashBackpackUIRightPoint(-11.3f * 0.5f);
            }

            if (sonUse != 0)
            {
                if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) && sonUse > 1 && sonUse < 4)
                {
                    sonUse -= 1;

                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow) && sonUse < 3)
                {
                    sonUse += 1;

                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                }
            }
        }
        else heart.rectTransform.anchoredPosition = new Vector2(-255, 35);

        switch (sonUse)
        {
            case 0:
                if (sonSelect == 0)
                    heart.rectTransform.anchoredPosition = new Vector2(-255, 35 - (select - 1) * 36);
                else
                {
                    if (select == 2)
                        heart.transform.position = Vector2.one * 10000;
                    else if (select == 1)
                    {
                        if (sonSelect < 9 && sonSelect > 0)
                        {
                            heart.rectTransform.anchoredPosition = new Vector2(-103, 143 - (sonSelect - 1) * 31);
                        }
                    }
                }

                break;

            case 1:
                heart.rectTransform.anchoredPosition = new Vector2(-103, -137);
                break;

            case 2:
                heart.rectTransform.anchoredPosition = new Vector2(-11.5f, -137);
                break;

            case 3:
                heart.rectTransform.anchoredPosition = new Vector2(102.5f, -137);
                break;

            case 4:
                heart.rectTransform.anchoredPosition = Vector2.one * 10000;
                break;

            default:
                heart.rectTransform.anchoredPosition = new Vector2(-255, 35);
                break;
        }
    }

    private void FlashBackpackUIRightPoint(float y)
    {
        BackpackUIRightPoint2.transform.localPosition = new Vector3(BackpackUIRightPoint2.transform.localPosition.x, y);
        BackpackUIRightPoint3.transform.localPosition = new Vector3(BackpackUIRightPoint3.transform.localPosition.x, y);
    }

    private void BackpackExit()
    {
        MainControl.instance.PlayerControl.canMove = true;
        sonSelect = 0;
        select = 0;
        backpack.gameObject.SetActive(false);
        BackpackUILeft.transform.parent.localPosition = new Vector3(BackpackUILeft.transform.parent.localPosition.x, BackpackUILeft.transform.parent.localPosition.y, -50);
        typeWritter.TypeStop();
        TalkUIPositionChanger.instance.transform.localPosition = new Vector3(TalkUIPositionChanger.instance.transform.localPosition.x, TalkUIPositionChanger.instance.transform.localPosition.y, -50);
        //DebugLogger.Log(talkUI.transform.localPosition.z);
        sonUse = 0;
        sonSelect = 0;
    }
}
