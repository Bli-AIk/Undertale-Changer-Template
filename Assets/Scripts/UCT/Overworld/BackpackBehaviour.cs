using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UCT.Overworld
{
    /// <summary>
    /// 管理OW背包系统
    /// </summary>
    public class BackpackBehaviour : MonoBehaviour
    {
        public static BackpackBehaviour Instance;

        public int select, sonSelect, sonUse;
        private int _sonSelectMax;
        public RawImage rawImage;
        private RectTransform _backpack, _uiMessage;
        private TextMeshProUGUI _uiItems, _uiName, _uiTexts, _uiSelect;
        public TextMeshProUGUI typeMessage;
        private Image _heart;
        private float _clock;
        private GameObject _backpackUILeft, _backpackUIRight, _player, _mainCamera;
        private GameObject _backpackUIRightPoint2, _backpackUIRightPoint3;

        public GameObject saveBack;
        public TextMeshProUGUI saveUI;
        public RectTransform saveUIHeart;

        public TypeWritter typeWritter;

        private void Awake()
        {
            Instance = this;
            rawImage = GameObject.Find("BackpackCanvas/RawImage").GetComponent<RawImage>();
            typeMessage = GameObject.Find("BackpackCanvas/RawImage/Talk/UITalk").GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            typeWritter = GetComponent<TypeWritter>();
            _backpack = transform.Find("RawImage/Backpack").GetComponent<RectTransform>();
            _uiItems = _backpack.Find("UIItems").GetComponent<TextMeshProUGUI>();
            _uiMessage = _backpack.Find("UIMessage").GetComponent<RectTransform>();
            _uiName = _backpack.Find("UIMessage/UIName").GetComponent<TextMeshProUGUI>();
            _uiTexts = _backpack.Find("UIMessage/UITexts").GetComponent<TextMeshProUGUI>();
            _uiSelect = _backpack.Find("UISelect").GetComponent<TextMeshProUGUI>();
            _heart = _backpack.Find("Heart").GetComponent<Image>();
            _backpackUILeft = GameObject.Find("Main Camera/BackpackUI/Left2");
            _backpackUIRight = GameObject.Find("Main Camera/BackpackUI/Right");
            saveBack = GameObject.Find("Main Camera/Save");
            saveUI = GameObject.Find("BackpackCanvas/RawImage/Talk/UISave").GetComponent<TextMeshProUGUI>();
            saveUIHeart = GameObject.Find("BackpackCanvas/RawImage/Talk/UISave/Heart").GetComponent<RectTransform>();
            _player = GameObject.Find("Player");
            _mainCamera = GameObject.Find("Main Camera");
            _backpackUIRightPoint2 = _backpackUIRight.transform.Find("Point2").gameObject;
            _backpackUIRightPoint3 = _backpackUIRight.transform.Find("Point3").gameObject;
            _backpackUILeft.transform.parent.localPosition = new Vector3(_backpackUILeft.transform.parent.localPosition.x, _backpackUILeft.transform.parent.localPosition.y, -50);
            _backpack.gameObject.SetActive(false);
            MainControl.Instance.PlayerControl.canMove = true;

            SuitResolution();
        }

        public void SuitResolution()
        {
            if (!MainControl.Instance.OverworldControl.hdResolution)
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
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
            {
                return;
            }

            TalkUIPositionChanger.Instance.isUp = _player.transform.position.y < _mainCamera.transform.position.y - 1.25f;

            if (_player.transform.position.y >= _mainCamera.transform.position.y - 1.25f)
            {
                _uiMessage.anchoredPosition = new Vector2(0, 270);
                _backpackUILeft.transform.localPosition = new Vector3(0, 6.75f, 5);
            }
            else
            {
                _uiMessage.anchoredPosition = Vector2.zero;
                _backpackUILeft.transform.localPosition = new Vector3(0, 0, 5);
            }
            if (_clock > 0)
            {
                _clock -= Time.deltaTime;
            }
            else if (select > 0)
            {
                _backpack.gameObject.SetActive(true);
            }
            if (sonUse != 4)
            {
                _uiItems.gameObject.SetActive(sonSelect != 0);
                _backpackUIRight.transform.localPosition = sonSelect != 0 ? new Vector3(_backpackUIRight.transform.localPosition.x, _backpackUIRight.transform.localPosition.y, 5) : new Vector3(_backpackUIRight.transform.localPosition.x, _backpackUIRight.transform.localPosition.y, -50);
            }
            else
            {
                _uiItems.gameObject.SetActive(false);
                _backpackUIRight.transform.localPosition = new Vector3(_backpackUIRight.transform.localPosition.x, _backpackUIRight.transform.localPosition.y, -50);
            }
            if ((MainControl.Instance.KeyArrowToControl(KeyCode.X) || MainControl.Instance.KeyArrowToControl(KeyCode.C)) && sonSelect == 0)
            {
                if (_backpack.gameObject.activeSelf)//关闭
                {
                    BackpackExit();
                }
                else if (MainControl.Instance.KeyArrowToControl(KeyCode.C))//开启
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    MainControl.Instance.PlayerControl.myItems = MainControl.Instance.ListOrderChanger(MainControl.Instance.PlayerControl.myItems);
                    string uiSelectPlusColor = "";
                    _sonSelectMax = 8;
                    for (int i = 0; i < MainControl.Instance.PlayerControl.myItems.Count; i++)
                    {
                        if (MainControl.Instance.PlayerControl.myItems[i] == 0)
                        {
                            if (i == 0)
                                uiSelectPlusColor = "<color=grey>";
                            _sonSelectMax = i;
                            break;
                        }
                    }
                    sonUse = 0;
                    _backpackUILeft.transform.parent.localPosition = new Vector3(_backpackUILeft.transform.parent.localPosition.x, _backpackUILeft.transform.parent.localPosition.y, 5);
                    _clock = 0.01f;
                    select = 1;
                    MainControl.Instance.PlayerControl.canMove = false;

                    _uiSelect.text = uiSelectPlusColor + MainControl.Instance.ItemControl.itemTextMaxData[0].Substring(0, MainControl.Instance.ItemControl.itemTextMaxData[0].Length - 1);
                    _uiName.text = MainControl.Instance.PlayerControl.playerName;
                    _uiTexts.text = $"LV {MainControl.Instance.PlayerControl.lv}\n" +
                                   $"HP {MainControl.Instance.PlayerControl.hp}/{MainControl.Instance.PlayerControl.hpMax}\n" +
                                   $"G {MainControl.Instance.PlayerControl.gold}";

                    if (select == 1)
                        FlashBackpackUIRightPoint(-8.55f * 0.5f);
                    else
                        FlashBackpackUIRightPoint(-11.3f * 0.5f);
                }
            }
            if (_backpack.gameObject.activeSelf && !typeWritter.isTyping)
            {
                if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                {
                    MainControl.Instance.playerBehaviour.owTimer = 0.1f;

                    if (select == 1)
                    {
                        if (sonUse == 0 && MainControl.Instance.PlayerControl.myItems[0] != 0)
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        }

                        if (_sonSelectMax != 0)
                        {
                            if (sonSelect == 0)
                            {
                                sonSelect = 1;
                                _uiItems.text = "";
                                foreach (var t in MainControl.Instance.PlayerControl.myItems)
                                {
                                    if (t != 0)
                                        _uiItems.text += $" {MainControl.Instance.ItemIdGetName(t, "Auto", 0)}\n";
                                    else _uiItems.text += "\n";
                                }

                                _uiItems.text += $"\n {MainControl.Instance.ItemControl.itemTextMaxData[8][..^1]}" +
                                                $"{MainControl.Instance.ItemControl.itemTextMaxData[9][..^1]}" +
                                                $"{MainControl.Instance.ItemControl.itemTextMaxData[10][..^1]}";

                            }
                            else
                            {
                                var plusText = MainControl.Instance.PlayerControl.myItems[sonSelect - 1] switch
                                {
                                    >= 20000 => -20000 + MainControl.Instance.ItemControl.itemFoods.Count / 3 +
                                                MainControl.Instance.ItemControl.itemArms.Count / 2,
                                    >= 10000 => -10000 + MainControl.Instance.ItemControl.itemFoods.Count / 3,
                                    _ => 0
                                };

                                switch (sonUse)
                                {
                                    case 0:
                                        sonUse = 1;
                                        break;

                                    case 1:
                                        sonUse = 4;
                                        MainControl.Instance.UseItem(typeWritter, typeMessage, sonSelect, plusText);
                                        _uiTexts.text = $"LV {MainControl.Instance.PlayerControl.lv}\n" +
                                                       $"HP {MainControl.Instance.PlayerControl.hp}/{MainControl.Instance.PlayerControl.hpMax}\n" +
                                                       $"G {MainControl.Instance.PlayerControl.gold}";

                                        goto default;
                                    case 2:
                                        sonUse = 4;
                                        typeWritter.TypeOpen(MainControl.Instance.ItemControl.itemTextMaxItemSon[(MainControl.Instance.PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 2], false, 0, 1, typeMessage);

                                        goto default;
                                    case 3:
                                        sonUse = 4;
                                        typeWritter.TypeOpen(MainControl.Instance.ItemControl.itemTextMaxItemSon[(MainControl.Instance.PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 1], false, 0, 1, typeMessage);

                                        MainControl.Instance.PlayerControl.myItems[sonSelect - 1] = 0;
                                        goto default;
                                    default:
                                        if (!typeWritter.isTyping)
                                        {
                                            BackpackExit();
                                            break;
                                        }
                                        if (TalkUIPositionChanger.Instance.transform.localPosition.z < 0)
                                        {
                                            TalkUIPositionChanger.Instance.Change();
                                            TalkUIPositionChanger.Instance.transform.localPosition = new Vector3(TalkUIPositionChanger.Instance.transform.localPosition.x, TalkUIPositionChanger.Instance.transform.localPosition.y, 5);
                                            //Debug.LogWarning(talkUI.transform.localPosition.z);
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
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        }
                        _uiItems.text = "";
                        /*
                    BackpackUIRightPoint2.transform.localPosition = new Vector3(BackpackUIRightPoint2.transform.localPosition.x, -11.3f);
                    BackpackUIRightPoint3.transform.localPosition = new Vector3(BackpackUIRightPoint3.transform.localPosition.x, -11.3f);
                    */
                        _uiItems.text = $"\"{MainControl.Instance.PlayerControl.playerName}\"\n\n"
                                       + $"LV {MainControl.Instance.PlayerControl.lv}\n"
                                       + $"HP {MainControl.Instance.PlayerControl.hp}{MainControl.Instance.RichTextWithEnd("size", 12, " ")}/{MainControl.Instance.RichTextWithEnd("size", 12, " ")}{MainControl.Instance.PlayerControl.hpMax}{MainControl.Instance.RichTextWithEnd("size", 1, "\n")}\n"
                                       + $"{MainControl.Instance.ItemControl.itemTextMaxData[1][..^1]}\n"
                                       + $"{MainControl.Instance.RichTextWithEnd("size", 12, " ")}{(MainControl.Instance.PlayerControl.atk - 10)}{MainControl.Instance.RichTextWithEnd("size", 6, " ")}({MainControl.Instance.PlayerControl.wearAtk}){MainControl.Instance.RichTextWithEnd("size", 15, " ")} EXP:{MainControl.Instance.PlayerControl.exp}\n"
                                       + $"{MainControl.Instance.ItemControl.itemTextMaxData[2][..^1]}\n"
                                       + $"{MainControl.Instance.RichTextWithEnd("size", 12, " ")}{(MainControl.Instance.PlayerControl.def - 10)}{MainControl.Instance.RichTextWithEnd("size", 6, " ")}({MainControl.Instance.PlayerControl.wearDef}){MainControl.Instance.RichTextWithEnd("size", 15, " ")}\n"
                                       + $"{MainControl.Instance.ItemControl.itemTextMaxData[3][..^1]}:\n"
                                       + $"{MainControl.Instance.PlayerControl.nextExp}\n\n"
                                       + $"{MainControl.Instance.ItemControl.itemTextMaxData[4][..^1]}\n"
                                       + $"{MainControl.Instance.ItemIdGetName(MainControl.Instance.PlayerControl.wearArm, "Auto", 0)}\n"
                                       + $"{MainControl.Instance.ItemControl.itemTextMaxData[5][..^1]}\n"
                                       + $"{MainControl.Instance.ItemIdGetName(MainControl.Instance.PlayerControl.wearArmor, "Auto", 0)}\n"
                                       + $"{MainControl.Instance.RichTextWithEnd("size", 13, "\n")}"
                                       + $"{MainControl.Instance.ItemControl.itemTextMaxData[6][..^1]}\n"
                                       + $"{MainControl.Instance.PlayerControl.gold}";


                    }
                }
                if ((MainControl.Instance.KeyArrowToControl(KeyCode.X)) && sonUse != 4)
                {
                    if (select == 2 || (select == 1 && sonUse == 0))
                        sonSelect = 0;
                    else
                        sonUse = 0;
                }

                _backpack.localScale = Vector3.one;

                if (sonUse != 4)
                {
                    if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow))
                    {
                        if (select > 1 && sonSelect == 0)
                        {
                            AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            select -= 1;
                        }
                        if (select == 1 && sonSelect > 1 && sonSelect != 0)
                        {
                            AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            sonSelect -= 1;
                        }
                    }
                    else if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
                    {
                        if (select < 2 && sonSelect == 0)
                        {
                            select += 1;
                            AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        }
                        if (select == 1 && sonSelect < _sonSelectMax && sonSelect != 0)
                        {
                            sonSelect += 1;
                            AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        }
                    }
                }
                if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
                {
                    if (select == 1)
                        FlashBackpackUIRightPoint(-8.55f * 0.5f);
                    else
                        FlashBackpackUIRightPoint(-11.3f * 0.5f);
                }

                if (sonUse != 0)
                {
                    if (MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow) && sonUse is > 1 and < 4)
                    {
                        sonUse -= 1;

                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    }
                    else if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow) && sonUse < 3)
                    {
                        sonUse += 1;

                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    }
                }
            }
            else _heart.rectTransform.anchoredPosition = new Vector2(-255, 35);

            switch (sonUse)
            {
                case 0:
                    if (sonSelect == 0)
                        _heart.rectTransform.anchoredPosition = new Vector2(-255, 35 - (select - 1) * 36);
                    else
                    {
                        if (select == 2)
                            _heart.transform.position = Vector2.one * 10000;
                        else if (select == 1)
                        {
                            if (sonSelect is < 9 and > 0)
                            {
                                _heart.rectTransform.anchoredPosition = new Vector2(-103, 143 - (sonSelect - 1) * 31);
                            }
                        }
                    }

                    break;

                case 1:
                    _heart.rectTransform.anchoredPosition = new Vector2(-103, -137);
                    break;

                case 2:
                    _heart.rectTransform.anchoredPosition = new Vector2(-11.5f, -137);
                    break;

                case 3:
                    _heart.rectTransform.anchoredPosition = new Vector2(102.5f, -137);
                    break;

                case 4:
                    _heart.rectTransform.anchoredPosition = Vector2.one * 10000;
                    break;

                default:
                    _heart.rectTransform.anchoredPosition = new Vector2(-255, 35);
                    break;
            }
        }

        private void FlashBackpackUIRightPoint(float y)
        {
            _backpackUIRightPoint2.transform.localPosition = new Vector3(_backpackUIRightPoint2.transform.localPosition.x, y);
            _backpackUIRightPoint3.transform.localPosition = new Vector3(_backpackUIRightPoint3.transform.localPosition.x, y);
        }

        private void BackpackExit()
        {
            MainControl.Instance.PlayerControl.canMove = true;
            sonSelect = 0;
            select = 0;
            _backpack.gameObject.SetActive(false);
            _backpackUILeft.transform.parent.localPosition = new Vector3(_backpackUILeft.transform.parent.localPosition.x, _backpackUILeft.transform.parent.localPosition.y, -50);
            typeWritter.TypeStop();
            TalkUIPositionChanger.Instance.transform.localPosition = new Vector3(TalkUIPositionChanger.Instance.transform.localPosition.x, TalkUIPositionChanger.Instance.transform.localPosition.y, -50);
            //Debug.Log(talkUI.transform.localPosition.z);
            sonUse = 0;
            sonSelect = 0;
        }
    }
}