using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle
{
    /// <summary>
    ///     Battle场景中的UI控制器
    ///     也负责玩家回合的控制
    /// </summary>
    public class SelectUIController : MonoBehaviour
    {
        public enum SelectedButton
        {
            Fight,
            Act,
            Item,
            Mercy
        }

        /// <summary>
        ///     表示当前选择的界面层。
        /// </summary>
        public enum SelectedLayer
        {
            /// <summary>
            ///     按钮层，仅能选择按钮。
            /// </summary>
            ButtonLayer,

            /// <summary>
            ///     名称层，用于显示、选择怪物名称。
            /// </summary>
            NameLayer,

            /// <summary>
            ///     选项层，用于选择选项。
            /// </summary>
            OptionLayer,

            /// <summary>
            ///     对话层，用于显示对话。
            /// </summary>
            NarratorLayer,

            /// <summary>
            ///     回合层，在此层时进入敌方回合。
            /// </summary>
            TurnLayer
        }


        public enum UITextMode
        {
            None,
            Hit,
            Food
        }

        private static readonly int IsFlashing = Shader.PropertyToID("_IsFlashing");
        private static readonly int ColorFlash = Shader.PropertyToID("_ColorFlash");
        private static readonly int ColorOn = Shader.PropertyToID("_ColorOn");
        private static readonly int ColorUnder = Shader.PropertyToID("_ColorUnder");
        private static readonly int Crop = Shader.PropertyToID("_Crop");
        private static readonly int Flash = Shader.PropertyToID("_Flash");

        [Header("HP条配色")]
        public Color hpColorUnder;
        public Color hpColorOn;
        public Color hpColorHit;

        [Header("对话气泡载入数")]
        //载入actSave
        public int numberDialog;

        public bool isDialog;

        [Header("暂存未使用的Sprite")]
        public List<Sprite> spriteUI;

        public List<SpriteRenderer> buttons;
        public List<Vector2> playerUIPos;

        [Header("选择的按钮")]
        public SelectedButton selectedButton;

        [Header("目前的UI层级")]
        public SelectedLayer selectedLayer;

        [Header("选择的名称编号")]
        public int nameLayerIndex;

        [Header("选择的选项")]
        public int optionLayerIndex;

        [Header("暂存ACT选项以便调用")]
        public List<string> actSave;

        [Header("自动寻找战斗总控的怪物 需保证名称一致")]
        public List<EnemiesController> enemiesControllers;

        [Header("首次进入回合的时候播放自定义的回合文本")]
        public bool firstIn;

        public int firstInDiy = -1;
        private DialogBubbleBehaviour _dialog;
        private GameObject _enemiesHpLine;
        private int _hpFood;
        private Tween _hpFoodTween;
        private SpriteRenderer _hpSpr;

        private ItemScroller _itemScroller;

        private TextMeshPro _nameUI, _hpUI, _textUI, _textUIBack;

        private int _saveTurn = -1;
        private string _saveTurnText = "";

        private TargetController _target;
        private TypeWritter _typeWritter;

        private void Start()
        {
            GetComponent();
            TurnTextLoad(true);
            _enemiesHpLine.SetActive(false);
            UITextUpdate();
            _hpFood = MainControl.Instance.playerControl.hp;
            InTurn();
        }

        private void GetComponent()
        {
            _target = transform.Find("Target").GetComponent<TargetController>();
            _target.gameObject.SetActive(false);
            _nameUI = transform.Find("Name UI").GetComponent<TextMeshPro>();
            _hpUI = transform.Find("HP UI").GetComponent<TextMeshPro>();
            _textUI = transform.Find("Text UI").GetComponent<TextMeshPro>();
            _textUIBack = transform.Find("Text UI Back").GetComponent<TextMeshPro>();
            _hpSpr = transform.Find("HP").GetComponent<SpriteRenderer>();
            _itemScroller = transform.Find("ItemSelect").GetComponent<ItemScroller>();
            _enemiesHpLine = transform.Find("EnemiesHpLine").gameObject;
            _dialog = GameObject.Find("DialogBubble").GetComponent<DialogBubbleBehaviour>();
            _dialog.gameObject.SetActive(false);
            _typeWritter = GetComponent<TypeWritter>();
            foreach (var t in new[] { "FIGHT", "ACT", "ITEM", "MERCY" })
            {
                buttons.Add(transform.Find(t).GetComponent<SpriteRenderer>());
            }

            for (var i = 0; i < MainControl.Instance.BattleControl.enemies.Count; i++)
            {
                var enemies = GameObject.Find(MainControl.Instance.BattleControl.enemies[i].name)
                    .GetComponent<EnemiesController>();
                if (!enemies)
                {
                    continue;
                }

                enemiesControllers.Add(enemies);
                enemiesControllers[i].atk = MainControl.Instance.BattleControl.enemiesAtk[i];
                enemiesControllers[i].def = MainControl.Instance.BattleControl.enemiesDef[i];
            }
            selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
        }

        private void Update()
        {
            if (GameUtilityService.IsGamePausedOrSetting())
            {
                return;
            }

            if (MainControl.Instance.playerControl.isDebug)
            {
                NameUIUpdate();
            }

            if (TurnController.Instance.isMyTurn)
            {
                MyTurn();
            }

            _dialog.gameObject.SetActive(isDialog);

            if (!isDialog)
            {
                return;
            }

            if ((_dialog.typeWritter.isTyping || !InputService.GetKeyDown(KeyCode.Z)) &&
                ((selectedButton != SelectedButton.Fight && _textUI.text != "") || numberDialog != 0))
            {
                return;
            }

            if (numberDialog < actSave.Count)
            {
                KeepDialogBubble();
            }
            else //敌方回合：开！
            {
                isDialog = false;

                TurnController.Instance.OutYourTurn();

                _itemScroller.gameObject.SetActive(false);
                actSave = new List<string>();
                selectedLayer = SelectedLayer.TurnLayer;
            }
        }

        /// <summary>
        ///     UI打字 打字完成后不会强制控死文本
        /// </summary>
        private void Type(string text)
        {
            _typeWritter.StartTypeWritter(text, _textUI);
        }

        /// <summary>
        ///     战术互换
        /// </summary>
        private void SpriteChange()
        {
            (buttons[(int)selectedButton].sprite, spriteUI[(int)selectedButton]) = (
                spriteUI[(int)selectedButton], buttons[(int)selectedButton].sprite);
        }

        /// <summary>
        ///     selectUI=1时的设定
        ///     主要为选定怪物
        /// </summary>
        private void LayerOneSet()
        {
            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f,
                -0.96f - nameLayerIndex * 0.66f, MainControl.Instance.battlePlayerController.transform.position.z);
            if (InputService.GetKeyDown(KeyCode.DownArrow) &&
                nameLayerIndex < MainControl.Instance.BattleControl.enemies.Count - 1)
            {
                nameLayerIndex++;
                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
            }

            if (!InputService.GetKeyDown(KeyCode.UpArrow) || nameLayerIndex <= 0)
            {
                return;
            }

            nameLayerIndex--;
            AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
        }

        /// <summary>
        ///     进我方回合
        /// </summary>
        public void InTurn()
        {
            TurnController.Instance.isMyTurn = true;
            selectedLayer = SelectedLayer.ButtonLayer;
            selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
            nameLayerIndex = 0;
            optionLayerIndex = 0;
            SpriteChange();
            TurnTextLoad();
            _itemScroller.gameObject.SetActive(true);

            if (MainControl.Instance.battlePlayerController)
            {
                MainControl.Instance.battlePlayerController.collideCollider.enabled = false;
            }
        }

        /// <summary>
        ///     我的回合！抽卡)
        /// </summary>
        private void MyTurn()
        {
            switch (selectedLayer)
            {
                case SelectedLayer.ButtonLayer:
                {
                    MainControl.Instance.battlePlayerController.transform.position =
                        (Vector3)playerUIPos[(int)selectedButton] + new Vector3(0, 0,
                            MainControl.Instance.battlePlayerController.transform.position.z);
                    if (InputService.GetKeyDown(KeyCode.LeftArrow))
                    {
                        AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        if (selectedButton >= EnumService.GetMinEnumValue<SelectedButton>())
                        {
                            SpriteChange();
                            if (selectedButton == EnumService.GetMinEnumValue<SelectedButton>())
                            {
                                selectedButton = EnumService.GetMaxEnumValue<SelectedButton>();
                            }
                            else
                            {
                                selectedButton -= 1;
                            }

                            SpriteChange();
                        }
                    }
                    else if (InputService.GetKeyDown(KeyCode.RightArrow))
                    {
                        AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        if (selectedButton <= EnumService.GetMaxEnumValue<SelectedButton>())
                        {
                            SpriteChange();
                            if (selectedButton == EnumService.GetMaxEnumValue<SelectedButton>())
                            {
                                selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
                            }
                            else
                            {
                                selectedButton += 1;
                            }

                            SpriteChange();
                        }
                    }

                    if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        selectedLayer = SelectedLayer.NameLayer;
                        optionLayerIndex = 0;
                        AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        _typeWritter.TypeStop();
                        _textUI.text = "";

                        if (selectedButton != SelectedButton.Item)
                        {
                            foreach (var t in MainControl.Instance.BattleControl.enemies)
                            {
                                _textUI.text += "<indent=10></indent>* " + t.name + "\n";
                            }
                        }
                        else
                        {
                            MainControl.Instance.playerControl.items =
                                ListManipulationService.CheckAllDataNamesInItemList(MainControl.Instance.playerControl
                                    .items);
                            MainControl.Instance.playerControl.items =
                                ListManipulationService.MoveNullOrEmptyToEnd(MainControl.Instance.playerControl.items);

                            _textUIBack.rectTransform.anchoredPosition = new Vector2(-5, -3.3f);
                            _textUIBack.alignment = TextAlignmentOptions.TopRight;
                            _hpSpr.material.SetFloat(IsFlashing, 1);
                            _hpSpr.material.SetColor(ColorFlash, hpColorOn);

                            _hpFood = MainControl.Instance.playerControl.hp;
                        }

                        if (string.IsNullOrEmpty(MainControl.Instance.playerControl.items[0]) &&
                            selectedButton == SelectedButton.Item)
                        {
                            selectedLayer = SelectedLayer.ButtonLayer;
                        }
                    }

                    _hpUI.text = FormatWithLeadingZero(_hpFood) + " / " +
                                 FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
                    break;
                }

                case SelectedLayer.NameLayer:
                {
                    if (InputService.GetKeyDown(KeyCode.X))
                    {
                        selectedLayer = SelectedLayer.ButtonLayer;
                        nameLayerIndex = 0;
                        if (!firstIn)
                        {
                            TurnTextLoad();
                        }
                        else
                        {
                            TurnTextLoad(true, firstInDiy);
                        }

                        _enemiesHpLine.SetActive(false);
                        break;
                    }

                    if (InputService.GetKeyDown(KeyCode.Z) && selectedButton != SelectedButton.Item)
                    {
                        if (selectedButton != SelectedButton.Fight)
                        {
                            selectedLayer = SelectedLayer.OptionLayer;
                        }
                        else
                        {
                            selectedLayer = SelectedLayer.NarratorLayer;
                            SpriteChange();
                        }

                        optionLayerIndex = 0;
                        _textUI.text = "";
                        AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
                    }

                    switch (selectedButton)
                    {
                        case SelectedButton.Fight: //FIGHT：选择敌人
                        {
                            _enemiesHpLine.SetActive(true);
                            LayerOneSet();
                            if (InputService.GetKeyDown(KeyCode.Z))
                            {
                                _enemiesHpLine.SetActive(false);
                                _target.gameObject.SetActive(true);
                                _target.select = nameLayerIndex;
                                _target.transform.Find("Move").transform.position = new Vector3(
                                    MainControl.Instance.BattleControl.enemies[nameLayerIndex].transform.position.x,
                                    _target.transform.Find("Move").transform.position.y);
                                _target.hitMonster = enemiesControllers[nameLayerIndex];
                                MainControl.Instance.battlePlayerController.transform.position =
                                    (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                                        MainControl.Instance.battlePlayerController.transform.position.z);
                            }

                            break;
                        }

                        case SelectedButton.Act: //ACT：选择敌人
                        {
                            LayerOneSet();
                            if (InputService.GetKeyDown(KeyCode.Z))
                            {
                                var save = new List<string>();
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.BattleControl.actSave, save,
                                    MainControl.Instance.BattleControl.enemies[nameLayerIndex].name + "\\");
                                TextProcessingService.SplitStringToListWithDelimiter(save, actSave);

                                _textUI.text = "<indent=10></indent>* " + actSave[0];
                                _textUIBack.text = "";
                                if (actSave.Count > MainControl.Instance.BattleControl.enemies.Count)
                                {
                                    _textUIBack.text += "* " + actSave[2];
                                }

                                if (actSave.Count > 2 * MainControl.Instance.BattleControl.enemies.Count)
                                {
                                    _textUI.text += "\n<indent=10></indent>* " + actSave[4];
                                }

                                if (actSave.Count > 3 * MainControl.Instance.BattleControl.enemies.Count)
                                {
                                    _textUIBack.text += "\n* " + actSave[6];
                                }

                                for (var i = 0; i < actSave.Count; i++)
                                {
                                    actSave[i] += ';';
                                }

                                actSave = DataHandlerService.ChangeItemData(actSave, false,
                                    new List<string>
                                    {
                                        enemiesControllers[nameLayerIndex].name,
                                        enemiesControllers[nameLayerIndex].atk.ToString(),
                                        enemiesControllers[nameLayerIndex].def.ToString()
                                    });

                                for (var i = 0; i < actSave.Count; i++)
                                {
                                    actSave[i] = actSave[i][..(actSave[i].Length - 1)];
                                }

                                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                            }

                            break;
                        }

                        case SelectedButton.Item: //ITEM：跳2
                        {
                            _itemScroller.Open(ListManipulationService.FindFirstNullOrEmptyIndex(MainControl.Instance
                                .playerControl
                                .items), 0);
                            selectedLayer = SelectedLayer.OptionLayer;

                            var item = DataHandlerService.GetItemFormDataName(
                                MainControl.Instance.playerControl.items[nameLayerIndex]);
                            if (item is FoodItem or ParentFoodItem)
                            {
                                UITextUpdate(UITextMode.Food, item.Data.Value);
                            }
                            else
                            {
                                UITextUpdate(UITextMode.Food);
                            }

                            break;
                        }

                        case SelectedButton.Mercy: //MERCY：选择敌人
                        {
                            LayerOneSet();
                            if (InputService.GetKeyDown(KeyCode.Z))
                            {
                                var save = new List<string>();
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    MainControl.Instance.BattleControl.mercySave, save,
                                    MainControl.Instance.BattleControl.enemies[nameLayerIndex].name + "\\");
                                TextProcessingService.SplitStringToListWithDelimiter(save, actSave);

                                _textUI.text = "<indent=10></indent>* " + actSave[0];
                                if (actSave.Count > MainControl.Instance.BattleControl.enemies.Count)
                                {
                                    _textUI.text += "\n<indent=10></indent>* " + actSave[2];
                                }

                                if (actSave.Count > 4 * MainControl.Instance.BattleControl.enemies.Count)
                                {
                                    _textUI.text += "\n<indent=10></indent>* " + actSave[4];
                                }
                            }

                            break;
                        }
                        default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }

                    break;
                }

                case SelectedLayer.OptionLayer:
                {
                    switch (selectedButton)
                    {
                        case SelectedButton.Fight:
                        {
                            break;
                        }
                        case SelectedButton.Act:
                        {
                            if (InputService.GetKeyDown(KeyCode.UpArrow) && optionLayerIndex - 2 >= 0)
                            {
                                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                optionLayerIndex -= 2;
                            }
                            else if (InputService.GetKeyDown(KeyCode.DownArrow) &&
                                     optionLayerIndex + 2 <= actSave.Count / 2 - 1)
                            {
                                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                optionLayerIndex += 2;
                            }

                            if (InputService.GetKeyDown(KeyCode.LeftArrow) &&
                                optionLayerIndex - 1 >= 0)
                            {
                                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                optionLayerIndex--;
                            }
                            else if (InputService.GetKeyDown(KeyCode.RightArrow) &&
                                     optionLayerIndex + 1 <= actSave.Count / 2 - 1)
                            {
                                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                optionLayerIndex++;
                            }

                            float playerPosX, playerPosY;
                            if ((optionLayerIndex - 1) % 2 == 0)
                            {
                                playerPosX = 0.25f;
                            }
                            else
                            {
                                playerPosX = -5.175f;
                            }

                            if (optionLayerIndex < 2)
                            {
                                playerPosY = -0.96f - 0 * 0.66f;
                            }
                            else
                            {
                                playerPosY = -0.96f - 1 * 0.66f;
                            }

                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(playerPosX,
                                playerPosY, MainControl.Instance.battlePlayerController.transform.position.z);
                            if (InputService.GetKeyDown(KeyCode.X))
                            {
                                selectedLayer = SelectedLayer.NameLayer;
                                optionLayerIndex = 0;
                                _textUI.text = "";
                                _textUIBack.text = "";
                                foreach (var t in MainControl.Instance.BattleControl.enemies)
                                {
                                    _textUI.text += "<indent=10></indent>* " + t.name + "\n";
                                }
                            }
                            else if (InputService.GetKeyDown(KeyCode.Z))
                            {
                                switch (nameLayerIndex) //在这里写ACT的相关触发代码
                                {
                                    case 0: //怪物0
                                    {
                                        switch (optionLayerIndex) //选项
                                        {
                                            case 0:
                                            {
                                                break;
                                            }

                                            case 1:
                                            {
                                                Other.Debug.Log(1);
                                                AudioController.Instance.PlayFx(3,
                                                    MainControl.Instance.AudioControl.fxClipBattle);

                                                break;
                                            }

                                            case 2:
                                            {
                                                break;
                                            }

                                            case 3:
                                            {
                                                break;
                                            }
                                        }

                                        break;
                                    }

                                    case 1: //怪物1
                                    {
                                        switch (optionLayerIndex) //选项
                                        {
                                            case 0:
                                            {
                                                break;
                                            }

                                            case 1:
                                            {
                                                break;
                                            }

                                            case 2:
                                            {
                                                break;
                                            }

                                            case 3:
                                            {
                                                break;
                                            }
                                        }

                                        break;
                                    }

                                    case 2: //怪物2
                                    {
                                        switch (optionLayerIndex) //选项
                                        {
                                            case 0:
                                            {
                                                break;
                                            }

                                            case 1:
                                            {
                                                break;
                                            }

                                            case 2:
                                            {
                                                break;
                                            }

                                            case 3:
                                            {
                                                break;
                                            }
                                        }

                                        break;
                                    }
                                }

                                _textUIBack.text = "";
                                selectedLayer = SelectedLayer.NarratorLayer;
                                MainControl.Instance.battlePlayerController.transform.position =
                                    (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                                        MainControl.Instance.battlePlayerController.transform.position.z);
                                Type(actSave[2 * (optionLayerIndex + 1) - 1]);
                                SpriteChange();
                                _itemScroller.Close();
                            }

                            break;
                        }

                        case SelectedButton.Item:
                        {
                            ItemOptionLayer(ref nameLayerIndex, ref optionLayerIndex);
                            break;
                        }

                        case SelectedButton.Mercy:
                        {
                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f,
                                -0.96f - optionLayerIndex * 0.66f,
                                MainControl.Instance.battlePlayerController.transform.position.z);
                            if (InputService.GetKeyDown(KeyCode.X))
                            {
                                selectedLayer = SelectedLayer.NameLayer;
                                optionLayerIndex = 0;
                                _textUI.text = "";
                                foreach (var t in MainControl.Instance.BattleControl.enemies)
                                {
                                    _textUI.text += "<indent=10></indent>* " + t.name + "\n";
                                }
                            }
                            else if (InputService.GetKeyDown(KeyCode.Z))
                            {
                                selectedLayer = SelectedLayer.NarratorLayer;
                                MainControl.Instance.battlePlayerController.transform.position =
                                    (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                                        MainControl.Instance.battlePlayerController.transform.position.z);
                                if (actSave[2 * optionLayerIndex - 3] != "Null")
                                {
                                    Type(actSave[2 * optionLayerIndex - 3]);
                                }
                                else
                                {
                                    _textUI.text = "";
                                    MainControl.Instance.battlePlayerController.transform.position =
                                        (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0,
                                            0, MainControl.Instance.battlePlayerController.transform.position.z);
                                    OpenDialogBubble(
                                        MainControl.Instance.BattleControl.turnDialogAsset
                                            [TurnController.Instance.turn]);
                                }

                                SpriteChange();
                                _itemScroller.Close();
                            }

                            if (InputService.GetKeyDown(KeyCode.UpArrow) && optionLayerIndex - 1 >= 0)
                            {
                                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                optionLayerIndex--;
                            }
                            else if (InputService.GetKeyDown(KeyCode.DownArrow) &&
                                     optionLayerIndex + 1 <= actSave.Count / 2 - 1)
                            {
                                AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                optionLayerIndex++;
                            }

                            break;
                        }
                        default:
                        {
                            throw new ArgumentOutOfRangeException();
                        }
                    }

                    break;
                }

                case SelectedLayer.NarratorLayer:
                {
                    firstIn = false;

                    if (selectedButton == SelectedButton.Fight && !_target.gameObject.activeSelf)
                    {
                        if (!isDialog)
                        {
                            _textUI.text = "";
                            MainControl.Instance.battlePlayerController.transform.position =
                                (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0,
                                    MainControl.Instance.battlePlayerController.transform.position.z);
                            OpenDialogBubble(
                                MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                        }
                    }
                    else if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        MainControl.Instance.battlePlayerController.collideCollider.enabled = true;
                        if (!isDialog)
                        {
                            if (selectedButton != SelectedButton.Fight && _textUI.text == "")
                            {
                                OpenDialogBubble(
                                    MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                                MainControl.Instance.battlePlayerController.transform.position =
                                    (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0,
                                        MainControl.Instance.battlePlayerController.transform.position.z);
                                break;
                            }

                            if (selectedButton != SelectedButton.Fight && !_typeWritter.isTyping)
                            {
                                if (InputService.GetKeyDown(KeyCode.Z))
                                {
                                    _textUI.text = "";
                                    MainControl.Instance.battlePlayerController.transform.position =
                                        (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0,
                                            0, MainControl.Instance.battlePlayerController.transform.position.z);
                                    OpenDialogBubble(
                                        MainControl.Instance.BattleControl.turnDialogAsset
                                            [TurnController.Instance.turn]);
                                }
                            }
                        }
                    }

                    break;
                }
                case SelectedLayer.TurnLayer:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ItemOptionLayer(ref int globalItemIndex, ref int visibleItemIndex)
        {
            if (InputService.GetKeyDown(KeyCode.X))
            {
                selectedLayer = SelectedLayer.ButtonLayer;
                globalItemIndex = 0;
                if (!firstIn)
                {
                    TurnTextLoad();
                }
                else
                {
                    TurnTextLoad(true, firstInDiy);
                }

                _itemScroller.Close();

                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                _textUIBack.text = "";

                UITextUpdate(UITextMode.Food);
                return;
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                selectedLayer = SelectedLayer.NarratorLayer;
                MainControl.Instance.battlePlayerController.transform.position =
                    (Vector3)(Vector2.one * 10000) + new Vector3(0, 0,
                        MainControl.Instance.battlePlayerController.transform.position.z);
                var dataName = MainControl.Instance.playerControl.items[globalItemIndex + 1];

                TypeWritterTagProcessor.SetItemDataName(dataName);
                _typeWritter.StartTypeWritter(
                    DataHandlerService.ItemDataNameGetLanguagePackUseText(dataName), _textUI);
                DataHandlerService.GetItemFormDataName(dataName).OnUse(globalItemIndex + 1);

                SpriteChange();
                _itemScroller.Close();

                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                _textUIBack.text = "";

                UITextUpdate(UITextMode.Food);
                return;
            }

            var myItemMax = ListManipulationService.FindFirstNullOrEmptyIndex(
                MainControl.Instance.playerControl.items);

            var (itemLine0, itemDataText0) =
                GenerateItemDisplayText(globalItemIndex - visibleItemIndex);

            var (itemLine1, itemDataText1) = myItemMax > 1
                ? GenerateItemDisplayText(globalItemIndex - visibleItemIndex + 1)
                : ("", "");

            var (itemLine2, itemDataText2) = myItemMax > 2
                ? GenerateItemDisplayText(globalItemIndex - visibleItemIndex + 2)
                : ("", "");

            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f,
                -0.96f - visibleItemIndex * 0.66f,
                MainControl.Instance.battlePlayerController.transform.position.z);

            _textUI.text = itemLine0 + itemLine1 + itemLine2;

            _textUIBack.text = itemDataText0 + "\n" + itemDataText1 + "\n" + itemDataText2;

            var updateHandleItemInput = 
                _itemScroller.UpdateHandleItemInput(globalItemIndex, visibleItemIndex, myItemMax,
                    CommonItemNavigationLogic);
            globalItemIndex = updateHandleItemInput.globalItemIndex;
            visibleItemIndex = updateHandleItemInput.visibleItemIndex;

            _hpUI.text = FormatWithLeadingZero(_hpFood) + " / " +
                         FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
        }


        private void CommonItemNavigationLogic(int globalItemIndex)
        {
            AudioController.Instance.PlayFx(0, MainControl.Instance.AudioControl.fxClipUI);
            var dataName = MainControl.Instance.playerControl.items[globalItemIndex];
            var item = DataHandlerService.GetItemFormDataName(dataName);
            if (item is FoodItem or ParentFoodItem)
            {
                UITextUpdate(UITextMode.Food, item.Data.Value);
            }
            else
            {
                UITextUpdate(UITextMode.Food);
            }
        }

        private void OpenDialogBubble(string textAsset)
        {
            MainControl.Instance.BattleControl.randomTurnDir = MathUtilityService.Get1Or_1();
            actSave = DataHandlerService.LoadItemData(textAsset);
            actSave = DataHandlerService.ChangeItemData(actSave, true, new List<string>());
            isDialog = true;
            numberDialog = 0;
        }

        private void KeepDialogBubble()
        {
            var save = new List<string>();
            TextProcessingService.SplitStringToListWithDelimiter(actSave[numberDialog], save);
            foreach (var t in enemiesControllers.Where(t => save[2] == t.name))
            {
                _dialog.transform.SetParent(t.transform);
                break;
            }

            _dialog.size = TextProcessingService.StringVector2ToRealVector2(save[0], _dialog.size);
            _dialog.position = TextProcessingService.StringVector2ToRealVector2(save[1], _dialog.position);

            _dialog.isBackRight = Convert.ToBoolean(save[3]);
            _dialog.backY = float.Parse(save[4]);
            _dialog.typeWritter.StartTypeWritter(save[5], _dialog.tmp);
            numberDialog++;
            _dialog.tmp.text = "";
            _dialog.PositionChange();
        }

        private void TurnTextLoad(bool isDiy = false, int diy = 0)
        {
            if (TurnController.Instance.turn != _saveTurn || _saveTurnText == "")
            {
                List<string> load;
                _saveTurn = TurnController.Instance.turn;
                if (isDiy)
                {
                    load = TurnTextLoad(MainControl.Instance.BattleControl.turnTextSave, diy);
                    firstIn = false;
                }
                else
                {
                    load = TurnTextLoad(MainControl.Instance.BattleControl.turnTextSave, _saveTurn);
                }

                _saveTurnText = load[Random.Range(0, load.Count)];
            }

            Type(_saveTurnText);
        }

        private static List<string> TurnTextLoad(List<string> turnTextSave, int turn)
        {
            var turnTextSaveChanged =
                (from t in turnTextSave
                    where t[..turn.ToString().Length] == turn.ToString()
                    select t[(turn.ToString().Length + 1)..]).ToList();
            var saves = new List<string>();
            TextProcessingService.SplitStringToListWithDelimiter(turnTextSaveChanged, saves);
            return saves;
        }

        /// <summary>
        ///     更新UI文字与血条
        /// </summary>
        public void UITextUpdate(UITextMode uiTextMode = 0, int foodValue = 0)
        {
            _hpSpr.transform.localScale = new Vector3(0.525f * MainControl.Instance.playerControl.hpMax, 8.5f);
            _hpSpr.material.SetColor(ColorUnder, hpColorUnder);
            _hpSpr.material.SetColor(ColorOn, hpColorOn);

            switch (uiTextMode)
            {
                case UITextMode.None:
                {
                    goto default;
                }
                default:
                {
                    _hpSpr.material.SetFloat(Crop,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    _hpSpr.material.SetFloat(Flash,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    break;
                }

                case UITextMode.Hit:
                {
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat(IsFlashing, 0);
                    _hpSpr.material.SetColor(ColorFlash, hpColorHit);
                    _hpSpr.material.SetFloat(Crop,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    _hpSpr.material
                        .DOFloat(
                            (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax,
                            "_Flash", 0.5f).SetEase(Ease.OutCirc);

                    break;
                }

                case UITextMode.Food:
                {
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat(IsFlashing, 1);
                    _hpSpr.material.SetFloat(Crop,
                        (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    float addNumber = MainControl.Instance.playerControl.hp + foodValue;
                    if (addNumber > MainControl.Instance.playerControl.hpMax)
                    {
                        addNumber = MainControl.Instance.playerControl.hpMax;
                    }

                    _hpSpr.material.DOFloat(addNumber / MainControl.Instance.playerControl.hpMax, "_Flash", 0.5f)
                        .SetEase(Ease.OutCirc);
                    break;
                }
            }

            _hpUI.transform.localPosition =
                new Vector3(9.85f + 0.0265f * (MainControl.Instance.playerControl.hpMax - 20), -5.825f);
            NameUIUpdate();

            if (uiTextMode != UITextMode.Food)
            {
                _hpUI.text = FormatWithLeadingZero(MainControl.Instance.playerControl.hp) + " / " +
                             FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
            }
            else
            {
                _hpFoodTween.Kill();
                var addNumber = MainControl.Instance.playerControl.hp + foodValue;
                if (addNumber > MainControl.Instance.playerControl.hpMax)
                {
                    addNumber = MainControl.Instance.playerControl.hpMax;
                }

                _hpFoodTween = DOTween.To(() => _hpFood, x => _hpFood = x, addNumber, 0.5f);
            }
        }

        private void NameUIUpdate()
        {
            _nameUI.text = MainControl.Instance.playerControl.playerName +
                           " lv<indent=29.5>" +
                           MainControl.Instance.playerControl.lv;
        }

        /// <summary>
        ///     将数字格式化为两位数（前导零）显示，例如将 1 显示为 01。
        /// </summary>
        private static string FormatWithLeadingZero(int i)
        {
            return i.ToString("D2");
        }


        private static (string text, string data) GenerateItemDisplayText(int layerIndex)
        {
            var dataName = MainControl.Instance.playerControl.items[layerIndex];

            var text = "<indent=10></indent>* " +
                       DataHandlerService.ItemDataNameGetLanguagePackName(dataName) + "\n";

            var item = DataHandlerService.GetItemFormDataName(dataName);
            var data = GenerateItemStatText(item);

            return (text, data);
        }

        private static string GenerateItemStatText(GameItem item)
        {
            var result = item switch
            {
                FoodItem or ParentFoodItem => "HP " + (item.Data.Value > 0 ? "+" : ""),
                WeaponItem => "ATK ",
                ArmorItem => "DEF ",
                _ => "Value "
            };
            return result + item.Data.Value;
        }
    }
}