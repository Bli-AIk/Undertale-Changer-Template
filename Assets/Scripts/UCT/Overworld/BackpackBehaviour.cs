using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.Serialization;

namespace UCT.Overworld
{
    /// <summary>
    ///     管理OW背包系统
    /// </summary>
    public class BackpackBehaviour : MonoBehaviour
    {
        public const int BoxZAxisVisible = 5;
        public const int BoxZAxisInvisible = -50;
        private const float ItemBoxY = -8.9f;
        private const float InfoBoxY = -10.3f;
        public static BackpackBehaviour Instance;
        public int select, sonSelect, sonUse;

        [HideInInspector] public TextMeshPro talkText;

        [HideInInspector] public TextMeshPro saveText;

        [HideInInspector] public BoxDrawer optionsBox;

        [HideInInspector] public BoxDrawer saveBox;

        [HideInInspector] public Transform saveHeart;

        [HideInInspector] public TypeWritter typeWritter;

        private float _clock;
        public SpriteRenderer Heart { get; private set; }
        private TextMeshPro _informationText, _overviewNameText, _overviewInfoText, _optionsText;

        private BoxDrawer _overviewBox, _informationBox;
        private int _sonSelectMax;
        public bool IsOpenBackPack { get; private set; }

        private void Awake()
        {
            Instance = this;
            talkText = transform.Find("BackpackCamera/TalkBox/TalkText").GetComponent<TextMeshPro>();
            typeWritter = GetComponent<TypeWritter>();
        }

        private void Start()
        {
            GetComponent();
            IsOpenBackPack = false;
            MainControl.Instance.playerControl.canMove = true;
        }

        private void Update()
        {
            if (GameUtilityService.IsGamePausedOrSetting())
            {
                if (IsOpenBackPack)
                {
                    BackpackExit();
                }

                return;
            }

            if (TalkBoxController.Instance)
            {
                TalkBoxController.Instance.isUp = MainControl.overworldPlayerBehaviour.transform.position.y <
                                                  transform.position.y - 1.25f;
            }
            else
            {
                Other.Debug.LogWarning("TalkBoxPositionChanger instance is missing!");
            }


            if (_clock > 0)
            {
                _clock -= Time.deltaTime;
            }
            else if (select > 0)
            {
                IsOpenBackPack = true;
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

            if ((InputService.GetKeyDown(KeyCode.X) ||
                 InputService.GetKeyDown(KeyCode.C)) &&
                sonSelect == 0)
            {
                if (IsOpenBackPack) //关闭
                {
                    BackpackExit();
                }
                else if (InputService.GetKeyDown(KeyCode.C)) //开启
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    MainControl.Instance.playerControl.items =
                        ListManipulationService.CheckAllDataNamesInItemList(MainControl.Instance.playerControl.items);
                    MainControl.Instance.playerControl.items =
                        ListManipulationService.MoveNullOrEmptyToEnd(MainControl.Instance.playerControl.items);
                    var uiSelectPlusColor = "";
                    _sonSelectMax = 8;
                    for (var i = 0; i < MainControl.Instance.playerControl.items.Count; i++)
                    {
                        if (!string.IsNullOrEmpty(MainControl.Instance.playerControl.items[i]))
                        {
                            continue;
                        }

                        if (i == 0)
                        {
                            uiSelectPlusColor = "<color=grey>";
                        }

                        _sonSelectMax = i;
                        break;
                    }

                    sonUse = 0;
                    optionsBox.localPosition.z = BoxZAxisVisible;
                    Heart.transform.localPosition = new Vector3(Heart.transform.localPosition.x,
                        Heart.transform.localPosition.y, BoxZAxisVisible);

                    if (MainControl.overworldPlayerBehaviour.transform.position.y >= transform.position.y - 1.25f)
                    {
                        _overviewBox.localPosition.y = 3.325f;
                    }
                    else
                    {
                        _overviewBox.localPosition.y = -3.425f;
                    }

                    _overviewBox.localPosition.z = BoxZAxisVisible;

                    _clock = 0.01f;
                    select = 1;
                    MainControl.Instance.playerControl.canMove = false;

                    _optionsText.text = uiSelectPlusColor +
                                        MainControl.Instance.LanguagePackControl.dataTexts[0][
                                            ..(MainControl.Instance.LanguagePackControl.dataTexts[0].Length - 1)];
                    UpdateOverviewBoxText();
                    FlashBackpackBoxRightPoint(select == 1 ? ItemBoxY : InfoBoxY);
                }
            }

            var index = sonSelect - 1;
            if (IsOpenBackPack && !typeWritter.isTyping)
            {
                if (InputService.GetKeyDown(KeyCode.Z))
                {
                    MainControl.overworldPlayerBehaviour.owTimer = 0.1f;
                    _informationText.transform.localPosition = new Vector3(0.5f, -5.12f, 0);

                    if (select == 1)
                    {
                        if (sonUse == 0 && !string.IsNullOrEmpty(MainControl.Instance.playerControl.items[0]))
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        }

                        if (_sonSelectMax != 0)
                        {
                            if (sonSelect == 0)
                            {
                                sonSelect = 1;
                                _informationText.text = "<indent=4>";
                                foreach (var t in MainControl.Instance.playerControl.items)
                                {
                                    if (!string.IsNullOrEmpty(t))
                                    {
                                        _informationText.text +=
                                            DataHandlerService.ItemDataNameGetLanguagePackName(t) + "\n";
                                    }
                                    else
                                    {
                                        _informationText.text += "\n";
                                    }
                                }

                                _informationText.text +=
                                    $"\n{MainControl.Instance.LanguagePackControl.dataTexts[8][..^1]}" +
                                    $"{MainControl.Instance.LanguagePackControl.dataTexts[9][..^1]}" +
                                    $"{MainControl.Instance.LanguagePackControl.dataTexts[10][..^1]}";
                            }
                            else
                            {
                                var dataName = MainControl.Instance.playerControl.items[index];
                                if (string.IsNullOrEmpty(dataName))
                                {
                                    BackpackExit();
                                    return;
                                }

                                var item = DataHandlerService.GetItemFormDataName(dataName);
                                TypeWritterTagProcessor.SetItemDataName(dataName);
                                switch (sonUse)
                                {
                                    case 0:
                                        sonUse = 1;
                                        break;

                                    case 1:
                                        sonUse = 4;
                                        typeWritter.StartTypeWritter(
                                            DataHandlerService.ItemDataNameGetLanguagePackUseText(dataName), 1, talkText);

                                        _overviewInfoText.text = $"LV {MainControl.Instance.playerControl.lv}\n" +
                                                                 $"HP {MainControl.Instance.playerControl.hp}/" +
                                                                 $"{MainControl.Instance.playerControl.hpMax}\n" +
                                                                 $"G<indent=9.25>{MainControl.Instance.playerControl.gold}";
                                        item.OnUse(index);
                                        goto default;
                                    case 2:
                                        sonUse = 4;
                                        typeWritter.StartTypeWritter(
                                            DataHandlerService.ItemDataNameGetLanguagePackInfoText(dataName), 1, talkText);

                                        item.OnCheck(index);
                                        goto default;
                                    case 3:
                                        sonUse = 4;
                                        typeWritter.StartTypeWritter(
                                            DataHandlerService.ItemDataNameGetLanguagePackDropText(dataName), 1, talkText);
                                        item.OnDrop(index);
                                        goto default;
                                    default:
                                        UpdateOverviewBoxText();

                                        if (!typeWritter.isTyping)
                                        {
                                            BackpackExit();
                                            break;
                                        }

                                        if (TalkBoxController.Instance.boxDrawer.localPosition.z < 0)
                                        {
                                            TalkBoxController.Instance.Change();
                                            TalkBoxController.Instance.boxDrawer.localPosition =
                                                new Vector3(TalkBoxController.Instance.boxDrawer.localPosition.x,
                                                    TalkBoxController.Instance.boxDrawer.localPosition.y,
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

                        _informationText.transform.localPosition = new Vector3(0.5f, -5.2f, 0);
                        _informationText.text = "";
                        _informationText.text = $"\"{MainControl.Instance.playerControl.playerName}\"\n\n"
                                                + $"LV {MainControl.Instance.playerControl.lv}\n"
                                                + $"HP {MainControl.Instance.playerControl.hp}" +
                                                "<indent=18></indent>/<indent=23></indent>" +
                                                $"{MainControl.Instance.playerControl.hpMax}\n\n" +
                                                $"{MainControl.Instance.LanguagePackControl.dataTexts[1][..^1]}" +
                                                "<indent=9.75></indent>" +
                                                $"{MainControl.Instance.playerControl.atk - 10}" +
                                                $"({DataHandlerService.GetItemFormDataName(MainControl.Instance.playerControl.wearWeapon).Data.Value})" +
                                                "<indent=41.75></indent>" +
                                                $"EXP:<indent=55.25></indent>{MainControl.Instance.playerControl.exp}\n" +
                                                $"{MainControl.Instance.LanguagePackControl.dataTexts[2][..^1]}" +
                                                "<indent=9.75></indent>" +
                                                $"{MainControl.Instance.playerControl.def - 10}" +
                                                $"({DataHandlerService.GetItemFormDataName(MainControl.Instance.playerControl.wearArmor).Data.Value})" +
                                                "<indent=40></indent>" +
                                                $"{MainControl.Instance.LanguagePackControl.dataTexts[3][..^1]}:" +
                                                $"{MainControl.Instance.playerControl.nextExp}\n\n" +
                                                $"{MainControl.Instance.LanguagePackControl.dataTexts[4][..^1]}" +
                                                $"{DataHandlerService.ItemDataNameGetLanguagePackName(MainControl.Instance.playerControl.wearWeapon)}\n" +
                                                $"{MainControl.Instance.LanguagePackControl.dataTexts[5][..^1]}" +
                                                $"{DataHandlerService.ItemDataNameGetLanguagePackName(MainControl.Instance.playerControl.wearArmor)}<line-height=7.5>\n" +
                                                $"{MainControl.Instance.LanguagePackControl.dataTexts[6][..^1]}" +
                                                $"{MainControl.Instance.playerControl.gold}";
                    }
                }

                if (InputService.GetKeyDown(KeyCode.X) && sonUse != 4)
                {
                    if (select == 2 || (select == 1 && sonUse == 0))
                    {
                        sonSelect = 0;
                        Heart.transform.localPosition = new Vector3(Heart.transform.localPosition.x,
                            Heart.transform.localPosition.y, BoxZAxisVisible);
                    }
                    else
                    {
                        sonUse = 0;
                    }
                }

                if (sonUse != 4)
                {
                    if (InputService.GetKeyDown(KeyCode.UpArrow))
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
                    else if (InputService.GetKeyDown(KeyCode.DownArrow))
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

                if (InputService.GetKeyDown(KeyCode.UpArrow) ||
                    InputService.GetKeyDown(KeyCode.DownArrow))
                {
                    FlashBackpackBoxRightPoint(select == 1 ? ItemBoxY : InfoBoxY);
                }

                if (sonUse != 0)
                {
                    if (InputService.GetKeyDown(KeyCode.LeftArrow) && sonUse is > 1 and < 4)
                    {
                        sonUse -= 1;

                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    }
                    else if (InputService.GetKeyDown(KeyCode.RightArrow) && sonUse < 3)
                    {
                        sonUse += 1;

                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    }
                }
            }
            else
            {
                Heart.transform.localPosition = new Vector3(-6.385f, 0.9f, Heart.transform.localPosition.z);
            }

            switch (sonUse)
            {
                case 0:
                    if (sonSelect == 0)
                    {
                        Heart.transform.localPosition = new Vector3(-6.385f, 0.9f - (select - 1) * 0.9f,
                            Heart.transform.localPosition.z);
                    }
                    else
                    {
                        switch (select)
                        {
                            case 1:
                            {
                                if (sonSelect is < 9 and > 0)
                                {
                                    Heart.transform.localPosition = new Vector3(-2.575f,
                                        3.575f - index * 0.775f,
                                        Heart.transform.localPosition.z);
                                }

                                break;
                            }
                            case 2:
                                Heart.transform.localPosition = Vector3.one * 10000;
                                break;
                        }
                    }

                    break;

                case 1:
                    Heart.transform.localPosition = new Vector3(-2.575f, -3.4f, Heart.transform.localPosition.z);
                    break;

                case 2:
                    Heart.transform.localPosition = new Vector3(-0.16f, -3.4f, Heart.transform.localPosition.z);
                    break;

                case 3:
                    Heart.transform.localPosition = new Vector3(2.675f, -3.4f, Heart.transform.localPosition.z);
                    break;

                case 4:
                    Heart.transform.localPosition = Vector3.one * 10000;
                    break;
            }
        }

        private void UpdateOverviewBoxText()
        {
            _overviewNameText.text = MainControl.Instance.playerControl.playerName;
            _overviewInfoText.text = $"LV {MainControl.Instance.playerControl.lv}\n" +
                                     $"HP {MainControl.Instance.playerControl.hp}/" +
                                     $"{MainControl.Instance.playerControl.hpMax}\n" +
                                     $"G<indent=9.25>{MainControl.Instance.playerControl.gold}";
        }

        private void GetComponent()
        {
            _informationText = transform.Find("BackpackCamera/BackpackBoxes/InformationBox/InformationText")
                .GetComponent<TextMeshPro>();
            _overviewNameText = transform.Find("BackpackCamera/BackpackBoxes/OverviewBox/OverviewNameText")
                .GetComponent<TextMeshPro>();
            _overviewInfoText = transform.Find("BackpackCamera/BackpackBoxes/OverviewBox/OverviewInfoText")
                .GetComponent<TextMeshPro>();
            _optionsText = transform.Find("BackpackCamera/BackpackBoxes/OptionsBox/OptionsText")
                .GetComponent<TextMeshPro>();
            Heart = transform.Find("BackpackCamera/Heart").GetComponent<SpriteRenderer>();
            optionsBox = transform.Find("BackpackCamera/BackpackBoxes/OptionsBox").GetComponent<BoxDrawer>();
            optionsBox.localPosition.z = BoxZAxisInvisible;
            _overviewBox = transform.Find("BackpackCamera/BackpackBoxes/OverviewBox").GetComponent<BoxDrawer>();
            _overviewBox.localPosition.z = BoxZAxisInvisible;
            _informationBox = transform.Find("BackpackCamera/BackpackBoxes/InformationBox").GetComponent<BoxDrawer>();
            saveBox = transform.Find("BackpackCamera/SaveBox").GetComponent<BoxDrawer>();
            saveText = transform.Find("BackpackCamera/SaveBox/SaveText").GetComponent<TextMeshPro>();
            saveHeart = transform.Find("BackpackCamera/SaveBox/Heart");
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
            IsOpenBackPack = false;
            optionsBox.localPosition.z = BoxZAxisInvisible;
            Heart.transform.localPosition = new Vector3(Heart.transform.localPosition.x,
                Heart.transform.localPosition.y, BoxZAxisInvisible);
            _overviewBox.localPosition.z = BoxZAxisInvisible;
            typeWritter.TypeStop();
            TalkBoxController.Instance.boxDrawer.localPosition.z = BoxZAxisInvisible;
            sonUse = 0;
            sonSelect = 0;
        }
    }
}