using Alchemy.Inspector;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;

namespace UCT.Overworld
{
    /// <summary>
    /// 管理OW背包系统
    /// </summary>
    public class BackpackBehaviour : MonoBehaviour
    {
        [ShowInInspector]
        private bool _isOpenBackPack;
        public static BackpackBehaviour Instance;
        public int select, sonSelect, sonUse;
        private int _sonSelectMax;
        private SpriteRenderer _heart;
        private float _clock;
        private BoxDrawer _overviewBox, _informationBox;
        private GameObject _player;
        private TextMeshPro _informationText, _overviewNameText, _overviewInfoText, _optionsText;

        [HideInInspector]
        public TextMeshPro talkText;
        [HideInInspector]
        public TextMeshPro saveText;
        [HideInInspector]
        public BoxDrawer optionsBox;
        [HideInInspector]
        public BoxDrawer saveBox;
        [HideInInspector]
        public Transform saveHeart;
        [HideInInspector]
        public TypeWritter typeWritter;

        public const int BoxZAxisVisible = 5;
        public const int BoxZAxisInvisible = -50;
        private const float ItemBoxY = -8.9f;
        private const float InfoBoxY = -10.3f;
        
        private void Awake()
        {
            Instance = this;
            talkText = transform.Find("BackpackCamera/TalkBox/TalkText").GetComponent<TextMeshPro>();
            typeWritter = GetComponent<TypeWritter>();
        }

        private void Start()
        {
            GetComponent();
            _isOpenBackPack = false;
            MainControl.Instance.playerControl.canMove = true;

        }

        private void GetComponent()
        {
            _informationText = transform.Find("BackpackCamera/BackpackBoxes/InformationBox/InformationText")
                .GetComponent<TextMeshPro>();
            _overviewNameText = transform.Find("BackpackCamera/BackpackBoxes/OverviewBox/OverviewNameText")
                .GetComponent<TextMeshPro>();
            _overviewInfoText = transform.Find("BackpackCamera/BackpackBoxes/OverviewBox/OverviewInfoText")
                .GetComponent<TextMeshPro>();
            _optionsText = transform.Find("BackpackCamera/BackpackBoxes/OptionsBox/OptionsText").GetComponent<TextMeshPro>();
            _heart = transform.Find("BackpackCamera/Heart").GetComponent<SpriteRenderer>();
            optionsBox = transform.Find("BackpackCamera/BackpackBoxes/OptionsBox").GetComponent<BoxDrawer>();
            optionsBox.localPosition.z = BoxZAxisInvisible;
            _overviewBox = transform.Find("BackpackCamera/BackpackBoxes/OverviewBox").GetComponent<BoxDrawer>();
            _overviewBox.localPosition.z = BoxZAxisInvisible;
            _informationBox = transform.Find("BackpackCamera/BackpackBoxes/InformationBox").GetComponent<BoxDrawer>();
            saveBox = transform.Find("BackpackCamera/SaveBox").GetComponent<BoxDrawer>();
            saveText = transform.Find("BackpackCamera/SaveBox/SaveText").GetComponent<TextMeshPro>();
            saveHeart = transform.Find("BackpackCamera/SaveBox/Heart");
            _player = GameObject.Find("Player");
        }

        private void Update()
        {
            if (MainControl.Instance.overworldControl.isSetting || MainControl.Instance.overworldControl.pause)
            {
                return;
            }

            TalkBoxPositionChanger.Instance.isUp =
                _player.transform.position.y < transform.position.y - 1.25f;


            if (_clock > 0)
            {
                _clock -= Time.deltaTime;
            }
            else if (select > 0)
            {
                _isOpenBackPack = true;
            }
            if (sonUse != 4)
            {
                _informationText.gameObject.SetActive(sonSelect != 0);
                _informationBox.localPosition.z = sonSelect != 0 ? BoxZAxisVisible : BoxZAxisInvisible;
            }
            else
            {
                _informationText.gameObject.SetActive(false);
                _informationBox.localPosition.z = BoxZAxisInvisible;
            }

            if ((GameUtilityService.ConvertKeyDownToControl(KeyCode.X) ||
                 GameUtilityService.ConvertKeyDownToControl(KeyCode.C)) &&
                sonSelect == 0) 
            {
                
                if (_isOpenBackPack)//关闭
                {
                    BackpackExit();
                }
                else if (GameUtilityService.ConvertKeyDownToControl(KeyCode.C))//开启
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    MainControl.Instance.playerControl.myItems =
                        ListManipulationService.MoveZerosToEnd(MainControl.Instance.playerControl.myItems);
                    var uiSelectPlusColor = "";
                    _sonSelectMax = 8;
                    for (var i = 0; i < MainControl.Instance.playerControl.myItems.Count; i++)
                    {
                        if (MainControl.Instance.playerControl.myItems[i] != 0) continue;
                        if (i == 0)
                            uiSelectPlusColor = "<color=grey>";
                        _sonSelectMax = i;
                        break;
                    }
                    sonUse = 0;
                    optionsBox.localPosition.z = BoxZAxisVisible;
                    _heart.transform.localPosition = new Vector3(_heart.transform.localPosition.x,
                        _heart.transform.localPosition.y, BoxZAxisVisible);
                    
                    if (_player.transform.position.y >= transform.position.y - 1.25f)
                    {
                        //_uiMessage.anchoredPosition = new Vector2(0, 270);
                        _overviewBox.localPosition.y = 3.325f;
                    }
                    else
                    {
                        //_uiMessage.anchoredPosition = Vector2.zero;
                        _overviewBox.localPosition.y = -3.425f;
                    }

                    _overviewBox.localPosition.z = BoxZAxisVisible;

                    _clock = 0.01f;
                    select = 1;
                    MainControl.Instance.playerControl.canMove = false;

                    _optionsText.text = uiSelectPlusColor +
                                     MainControl.Instance.ItemControl.itemTextMaxData[0][
                                         ..(MainControl.Instance.ItemControl.itemTextMaxData[0].Length - 1)];
                    _overviewNameText.text = MainControl.Instance.playerControl.playerName;
                    _overviewInfoText.text = $"LV {MainControl.Instance.playerControl.lv}\n" +
                                             $"HP {MainControl.Instance.playerControl.hp}/" +
                                             $"{MainControl.Instance.playerControl.hpMax}\n" +
                                             $"G<indent=9.25>{MainControl.Instance.playerControl.gold}";

                    FlashBackpackBoxRightPoint(select == 1 ? ItemBoxY : InfoBoxY);
                }
            }

            if (_isOpenBackPack && !typeWritter.isTyping)
            {
                if (GameUtilityService.ConvertKeyDownToControl(KeyCode.Z))
                {
                    MainControl.Instance.playerBehaviour.owTimer = 0.1f;

                    if (select == 1)
                    {
                        if (sonUse == 0 && MainControl.Instance.playerControl.myItems[0] != 0)
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        }

                        if (_sonSelectMax != 0)
                        {
                            if (sonSelect == 0)
                            {
                                sonSelect = 1;
                                _informationText.text = "";
                                foreach (var t in MainControl.Instance.playerControl.myItems)
                                {
                                    if (t != 0)
                                        _informationText.text +=
                                            $" {DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, t, "Auto", 0)}\n";
                                    else _informationText.text += "\n";
                                }

                                _informationText.text +=
                                    $"\n {MainControl.Instance.ItemControl.itemTextMaxData[8][..^1]}" +
                                    $"{MainControl.Instance.ItemControl.itemTextMaxData[9][..^1]}" +
                                    $"{MainControl.Instance.ItemControl.itemTextMaxData[10][..^1]}";

                            }
                            else
                            {
                                var plusText = MainControl.Instance.playerControl.myItems[sonSelect - 1] switch
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
                                        GameUtilityService.UseItem(typeWritter, talkText, sonSelect, plusText);
                                        _overviewInfoText.text = $"LV {MainControl.Instance.playerControl.lv}\n" +
                                                                 $"HP {MainControl.Instance.playerControl.hp}/" +
                                                                 $"{MainControl.Instance.playerControl.hpMax}\n" +
                                                                 $"G<indent=9.25>{MainControl.Instance.playerControl.gold}";
                                        goto default;
                                    case 2:
                                        sonUse = 4;
                                        typeWritter.TypeOpen(
                                            MainControl.Instance.ItemControl.itemTextMaxItemSon[
                                                (MainControl.Instance.playerControl.myItems[sonSelect - 1] + plusText) *
                                                5 - 2], false, 0, 1, talkText);

                                        goto default;
                                    case 3:
                                        sonUse = 4;
                                        typeWritter.TypeOpen(
                                            MainControl.Instance.ItemControl.itemTextMaxItemSon[
                                                (MainControl.Instance.playerControl.myItems[sonSelect - 1] + plusText) *
                                                5 - 1], false, 0, 1, talkText);

                                        MainControl.Instance.playerControl.myItems[sonSelect - 1] = 0;
                                        goto default;
                                    default:
                                        if (!typeWritter.isTyping)
                                        {
                                            BackpackExit();
                                            break;
                                        }

                                        if (TalkBoxPositionChanger.Instance.boxDrawer.localPosition.z < 0)
                                        {
                                            TalkBoxPositionChanger.Instance.Change();
                                            TalkBoxPositionChanger.Instance.boxDrawer.localPosition =
                                                new Vector3(TalkBoxPositionChanger.Instance.boxDrawer.localPosition.x,
                                                    TalkBoxPositionChanger.Instance.boxDrawer.localPosition.y,
                                                    BoxZAxisVisible);
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

                        _informationText.text = "";
                        _informationText.text = $"\"{MainControl.Instance.playerControl.playerName}\"\n\n"
                                                + $"LV {MainControl.Instance.playerControl.lv}\n"
                                                + $"HP {MainControl.Instance.playerControl.hp}{TextProcessingService.RichTextWithEnd("size", 12, " ")}/{TextProcessingService.RichTextWithEnd("size", 12, " ")}{MainControl.Instance.playerControl.hpMax}{TextProcessingService.RichTextWithEnd("size", 1, "\n")}\n"
                                                + $"{MainControl.Instance.ItemControl.itemTextMaxData[1][..^1]}\n"
                                                + $"{TextProcessingService.RichTextWithEnd("size", 12, " ")}{(MainControl.Instance.playerControl.atk - 10)}{TextProcessingService.RichTextWithEnd("size", 6, " ")}({MainControl.Instance.playerControl.wearAtk}){TextProcessingService.RichTextWithEnd("size", 15, " ")} EXP:{MainControl.Instance.playerControl.exp}\n"
                                                + $"{MainControl.Instance.ItemControl.itemTextMaxData[2][..^1]}\n"
                                                + $"{TextProcessingService.RichTextWithEnd("size", 12, " ")}{(MainControl.Instance.playerControl.def - 10)}{TextProcessingService.RichTextWithEnd("size", 6, " ")}({MainControl.Instance.playerControl.wearDef}){TextProcessingService.RichTextWithEnd("size", 15, " ")}\n"
                                                + $"{MainControl.Instance.ItemControl.itemTextMaxData[3][..^1]}:\n"
                                                + $"{MainControl.Instance.playerControl.nextExp}\n\n"
                                                + $"{MainControl.Instance.ItemControl.itemTextMaxData[4][..^1]}\n"
                                                + $"{DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.wearArm, "Auto", 0)}\n"
                                                + $"{MainControl.Instance.ItemControl.itemTextMaxData[5][..^1]}\n"
                                                + $"{DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.wearArmor, "Auto", 0)}\n"
                                                + $"{TextProcessingService.RichTextWithEnd("size", 13, "\n")}"
                                                + $"{MainControl.Instance.ItemControl.itemTextMaxData[6][..^1]}\n"
                                                + $"{MainControl.Instance.playerControl.gold}";


                    }
                }

                if ((GameUtilityService.ConvertKeyDownToControl(KeyCode.X)) && sonUse != 4)
                {
                    if (select == 2 || (select == 1 && sonUse == 0))
                    {
                        sonSelect = 0;
                        _heart.transform.localPosition = new Vector3(_heart.transform.localPosition.x,
                            _heart.transform.localPosition.y, BoxZAxisVisible);
                    }
                    else
                        sonUse = 0;
                }

                if (sonUse != 4)
                {
                    if (GameUtilityService.ConvertKeyDownToControl(KeyCode.UpArrow))
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
                    else if (GameUtilityService.ConvertKeyDownToControl(KeyCode.DownArrow))
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

                if (GameUtilityService.ConvertKeyDownToControl(KeyCode.UpArrow) ||
                    GameUtilityService.ConvertKeyDownToControl(KeyCode.DownArrow))
                {
                    FlashBackpackBoxRightPoint(select == 1 ? ItemBoxY : InfoBoxY);
                }

                if (sonUse != 0)
                {
                    if (GameUtilityService.ConvertKeyDownToControl(KeyCode.LeftArrow) && sonUse is > 1 and < 4)
                    {
                        sonUse -= 1;

                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    }
                    else if (GameUtilityService.ConvertKeyDownToControl(KeyCode.RightArrow) && sonUse < 3)
                    {
                        sonUse += 1;

                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    }
                }
            }
            else _heart.transform.localPosition = new Vector3(-6.385f, 0.9f, _heart.transform.localPosition.z);

            switch (sonUse)
            {
                case 0:
                    if (sonSelect == 0)
                        _heart.transform.localPosition = new Vector3(-6.385f, 0.9f - (select - 1) * 0.9f,
                            _heart.transform.localPosition.z);
                    else
                    {
                        switch (select)
                        {
                            case 1:
                            {
                                if (sonSelect is < 9 and > 0)
                                {
                                    _heart.transform.localPosition = new Vector3(-2.575f,
                                        3.575f - (sonSelect - 1) * 0.775f,
                                        _heart.transform.localPosition.z);
                                }

                                break;
                            }
                            case 2:
                                _heart.transform.localPosition = Vector3.one * 10000;
                                break;
                        }
                    }

                    break;

                case 1:
                    _heart.transform.localPosition = new Vector3(-2.575f, -3.4f, _heart.transform.localPosition.z);
                    break;

                case 2:
                    _heart.transform.localPosition = new Vector3(-0.16f, -3.4f, _heart.transform.localPosition.z);
                    break;

                case 3:
                    _heart.transform.localPosition = new Vector3(2.675f, -3.4f, _heart.transform.localPosition.z);
                    break;

                case 4:
                    _heart.transform.localPosition = Vector3.one * 10000;
                    break;
            }
        }

        private void FlashBackpackBoxRightPoint(float y)
        {
            const int vertexPointA = 2;
            _informationBox.vertexPoints[vertexPointA] = new Vector3(_informationBox.vertexPoints[vertexPointA].x, y);
            const int vertexPointB = 3;
            _informationBox.vertexPoints[vertexPointB] = new Vector3(_informationBox.vertexPoints[vertexPointB].x, y);
        }

        private void BackpackExit()
        {
            MainControl.Instance.playerControl.canMove = true;
            sonSelect = 0;
            select = 0;
            _isOpenBackPack = false;
            optionsBox.localPosition.z = BoxZAxisInvisible;
            _heart.transform.localPosition = new Vector3(_heart.transform.localPosition.x,
                _heart.transform.localPosition.y, BoxZAxisInvisible);
            _overviewBox.localPosition.z = BoxZAxisInvisible;
            typeWritter.TypeStop();
            TalkBoxPositionChanger.Instance.boxDrawer.localPosition.z = BoxZAxisInvisible;
            sonUse = 0;
            sonSelect = 0;
        }
    }
}