using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UCT.Battle
{
    /// <summary>
    /// Battle场景中的UI控制器
    /// 也负责玩家回合的控制
    /// </summary>
    public class SelectUIController : MonoBehaviour
    {
        private static readonly int IsFlashing = Shader.PropertyToID("_IsFlashing");
        private static readonly int ColorFlash = Shader.PropertyToID("_ColorFlash");
        private static readonly int ColorOn = Shader.PropertyToID("_ColorOn");
        private static readonly int ColorUnder = Shader.PropertyToID("_ColorUnder");
        private static readonly int Crop = Shader.PropertyToID("_Crop");
        private static readonly int Flash = Shader.PropertyToID("_Flash");
        
        private TextMeshPro _nameUI, _hpUI, _textUI, _textUIBack;
        private SpriteRenderer _hpSpr;

        [Header("HP条配色")]
        public Color hpColorUnder;

        public Color hpColorOn;
        public Color hpColorHit;

        [Header("对话气泡载入数")]//载入actSave
        public int numberDialog;

        public bool isDialog;

        [Header("暂存未使用的Sprite")]
        public List<Sprite> spriteUI;

        public List<SpriteRenderer> buttons;
        public List<Vector2> playerUIPos;

        public enum SelectedButton
        {
            Fight,
            Act,
            Item,
            Mercy
        }
        [Header("选择的按钮")]
        public SelectedButton selectedButton;

        public enum SelectedLayer
        {
            ButtonLayer,//按钮层，仅能选择按钮
            NameLayer,//名称层，用于显示、选择怪物名称
            OptionLayer,//选项层，用于选择选项
            NarratorLayer,//对话层，用于显示对话
            TurnLayer//回合层，在此层时进入敌方回合
        }
        
        [Header("目前的UI层级")]
        public SelectedLayer selectedLayer;

        [Header("选择的名称编号")] 
        public int selectedName;
        
        [Header("选择的选项")]
        public int selectedOption;
        
        private ItemSelectController _itemSelectController;
        private TypeWritter _typeWritter;
        private GameObject _enemiesHpLine;

        [Header("暂存ACT选项以便调用")]
        public List<string> actSave;

        [Header("自动寻找战斗总控的怪物 需保证名称一致")]
        public List<EnemiesController> enemiesControllers;

        private TargetController _target;
        private DialogBubbleBehaviour _dialog;

        private int _saveTurn = -1;
        private string _saveTurnText = "";

        [Header("首次进入回合的时候播放自定义的回合文本")]
        public bool firstIn;

        public int firstInDiy = -1;

        private void Start()
        {
            _target = transform.Find("Target").GetComponent<TargetController>();
            _target.gameObject.SetActive(false);
            _nameUI = transform.Find("Name UI").GetComponent<TextMeshPro>();
            _hpUI = transform.Find("HP UI").GetComponent<TextMeshPro>();
            _textUI = transform.Find("Text UI").GetComponent<TextMeshPro>();
            _textUIBack = transform.Find("Text UI Back").GetComponent<TextMeshPro>();
            _hpSpr = transform.Find("HP").GetComponent<SpriteRenderer>();
            _itemSelectController = transform.Find("ItemSelect").GetComponent<ItemSelectController>();
            _enemiesHpLine = transform.Find("EnemiesHpLine").gameObject;
            _dialog = GameObject.Find("DialogBubble").GetComponent<DialogBubbleBehaviour>();
            _dialog.gameObject.SetActive(false);
            _typeWritter = GetComponent<TypeWritter>();
            string[] loadButton = {
                "FIGHT",
                "ACT",
                "ITEM",
                "MERCY"};
            foreach (var t in loadButton)
            {
                buttons.Add(transform.Find(t).GetComponent<SpriteRenderer>());
            }

            for (var i = 0; i < MainControl.Instance.BattleControl.enemies.Count; i++)
            {
                var enemies = GameObject.Find(MainControl.Instance.BattleControl.enemies[i].name).GetComponent<EnemiesController>();
                if (enemies == null) continue;
                enemiesControllers.Add(enemies);
                enemiesControllers[i].atk = MainControl.Instance.BattleControl.enemiesAtk[i];
                enemiesControllers[i].def = MainControl.Instance.BattleControl.enemiesDef[i];
            }
            selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
            TurnTextLoad(true);
            _enemiesHpLine.SetActive(false);

            UITextUpdate();

            _hpFood = MainControl.Instance.playerControl.hp;

            InTurn();
        }

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;
            
            if (MainControl.Instance.playerControl.isDebug)
                NameUIUpdate();

            if (TurnController.Instance.isMyTurn)
                MyTurn();

            _dialog.gameObject.SetActive(isDialog);

            if (!isDialog) return;
            if ((_dialog.typeWritter.isTyping || !GameUtilityService.KeyArrowToControl(KeyCode.Z)) &&
                (selectedButton != SelectedButton.Fight && _textUI.text != "" || numberDialog != 0)) return;
            if (numberDialog < actSave.Count)
                KeepDialogBubble();
            else//敌方回合：开！
            {
                isDialog = false;

                TurnController.Instance.OutYourTurn();

                _itemSelectController.gameObject.SetActive(false);
                actSave = new List<string>();
                selectedLayer = SelectedLayer.TurnLayer;
            }
        }

        /// <summary>
        /// UI打字 打字完成后不会强制控死文本
        /// </summary>
        private void Type(string text)
        {
            _typeWritter.TypeOpen(text, false, 0, 0, _textUI);
        }

        /// <summary>
        /// 战术互换
        /// </summary>
        private void SpriteChange()
        {
            (buttons[(int)selectedButton].sprite, spriteUI[(int)selectedButton]) = (
                spriteUI[(int)selectedButton], buttons[(int)selectedButton].sprite);
        }

        /// <summary>
        /// selectUI=1时的设定
        /// 主要为选定怪物
        /// </summary>
        private void LayerOneSet()
        {
            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - selectedName * 0.66f, MainControl.Instance.battlePlayerController.transform.position.z);
            if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow) && selectedName < MainControl.Instance.BattleControl.enemies.Count - 1)
            {
                selectedName++;
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
            }

            if (!GameUtilityService.KeyArrowToControl(KeyCode.UpArrow) || selectedName <= 0) return;
            selectedName--;
            AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
        }

        /// <summary>
        ///进我方回合
        /// </summary>
        public void InTurn()
        {
            TurnController.Instance.isMyTurn = true;
            selectedLayer = SelectedLayer.ButtonLayer;
            selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
            selectedName = 0;
            selectedOption = 0;
            SpriteChange();
            TurnTextLoad();
            _itemSelectController.gameObject.SetActive(true);

            MainControl.Instance.battlePlayerController.collideCollider.enabled = false;
        }

        /// <summary>
        /// 我的回合！抽卡)
        /// </summary>
        private void MyTurn()
        {
            switch (selectedLayer)
            {
                case SelectedLayer.ButtonLayer:

                    MainControl.Instance.battlePlayerController.transform.position =
                        (Vector3)playerUIPos[(int)selectedButton] + new Vector3(0, 0,
                            MainControl.Instance.battlePlayerController.transform.position.z);
                    if (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        if (selectedButton >= EnumService.GetMinEnumValue<SelectedButton>())
                        {
                            SpriteChange();
                            if (selectedButton == EnumService.GetMinEnumValue<SelectedButton>())
                                selectedButton = EnumService.GetMaxEnumValue<SelectedButton>();
                            else
                                selectedButton -= 1;
                            SpriteChange();
                        }
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.RightArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        if (selectedButton <= EnumService.GetMaxEnumValue<SelectedButton>()) 
                        {
                            SpriteChange();
                            if (selectedButton == EnumService.GetMaxEnumValue<SelectedButton>())
                                selectedButton = EnumService.GetMinEnumValue<SelectedButton>();
                            else
                                selectedButton += 1;
                            SpriteChange();
                        }
                    }
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                    {
                        selectedLayer = SelectedLayer.NameLayer;
                        selectedOption = 0;
                        if (!(MainControl.Instance.playerControl.myItems[0] == 0 && selectedButton == SelectedButton.Item))
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                            _typeWritter.TypeStop();
                            _textUI.text = "";
                        }
                        if (selectedButton != SelectedButton.Item)
                            foreach (var t in MainControl.Instance.BattleControl.enemies)
                            {
                                _textUI.text += "<color=#00000000>aa*</color>* " + t.name + "\n";
                            }
                        else
                        {
                            MainControl.Instance.playerControl.myItems = ListManipulationService.MoveZerosToEnd(MainControl.Instance.playerControl.myItems);

                            _textUIBack.rectTransform.anchoredPosition = new Vector2(-5, -3.3f);
                            _textUIBack.alignment = TextAlignmentOptions.TopRight;
                            _hpSpr.material.SetFloat(IsFlashing, 1);
                            _hpSpr.material.SetColor(ColorFlash, hpColorOn);

                            _hpFood = MainControl.Instance.playerControl.hp;

                        }

                        if (MainControl.Instance.playerControl.myItems[0] == 0 && selectedButton == SelectedButton.Item)
                            selectedLayer = SelectedLayer.ButtonLayer;
                    }

                    //if (hpFood != MainControl.instance.PlayerControl.hp)
                    _hpUI.text = FormatWithLeadingZero(_hpFood) + " / " + FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
                    break;

                case SelectedLayer.NameLayer:
                    if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                    {
                        selectedLayer = SelectedLayer.ButtonLayer;
                        selectedName = 0;
                        if (!firstIn)
                            TurnTextLoad();
                        else
                            TurnTextLoad(true, firstInDiy);
                        _enemiesHpLine.SetActive(false);
                        break;
                    }

                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z) && selectedButton != SelectedButton.Item)
                    {
                        if (selectedButton != SelectedButton.Fight)
                            selectedLayer = SelectedLayer.OptionLayer;
                        else
                        {
                            selectedLayer = SelectedLayer.NarratorLayer;
                            SpriteChange();
                        }

                        selectedOption = 0;
                        _textUI.text = "";
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                    }
                    switch (selectedButton)
                    {
                        case SelectedButton.Fight://FIGHT：选择敌人
                            _enemiesHpLine.SetActive(true);
                            LayerOneSet();
                            if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                            {
                                _enemiesHpLine.SetActive(false);
                                _target.gameObject.SetActive(true);
                                _target.select = selectedName;
                                _target.transform.Find("Move").transform.position = new Vector3(MainControl.Instance.BattleControl.enemies[selectedName].transform.position.x, _target.transform.Find("Move").transform.position.y);
                                _target.hitMonster = enemiesControllers[selectedName];
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z); 
                            }
                            break;

                        case SelectedButton.Act://ACT：选择敌人
                            LayerOneSet();
                            if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                            {
                                var save = new List<string>();
                                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.BattleControl.actSave, save, MainControl.Instance.BattleControl.enemies[selectedName].name + "\\");
                                TextProcessingService.SplitStringToListWithDelimiter(save, actSave);

                                _textUI.text = "<color=#00000000>aa</color> * " + actSave[0];
                                _textUIBack.text = "";
                                if (actSave.Count > MainControl.Instance.BattleControl.enemies.Count)
                                    _textUIBack.text += "* " + actSave[2];
                                if (actSave.Count > 2 * MainControl.Instance.BattleControl.enemies.Count)
                                    _textUI.text += "\n<color=#00000000>aa</color> * " + actSave[4];
                                if (actSave.Count > 3 * MainControl.Instance.BattleControl.enemies.Count)
                                    _textUIBack.text += "\n* " + actSave[6];
                                for (var i = 0; i < actSave.Count; i++)
                                {
                                    actSave[i] += ';';
                                }

                                actSave = DataHandlerService.ChangeItemData(actSave, false, new List<string> { enemiesControllers[selectedName].name, enemiesControllers[selectedName].atk.ToString(), enemiesControllers[selectedName].def.ToString() });

                                for (var i = 0; i < actSave.Count; i++)
                                {
                                    actSave[i] = actSave[i][..(actSave[i].Length - 1)];
                                }

                                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                            }
                            break;

                        case SelectedButton.Item://ITEM：跳2
                            _itemSelectController.myItemMax = ListManipulationService.FindFirstZeroIndex(MainControl.Instance.playerControl.myItems);
                            _itemSelectController.Open();
                            selectedLayer = SelectedLayer.OptionLayer;

                            if (MainControl.Instance.playerControl.myItems[selectedName] < 10000)
                                UITextUpdate(UITextMode.Food, int.Parse(DataHandlerService.ItemIdGetData(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName], "Auto")));
                            else
                                UITextUpdate(UITextMode.Food);
                            break;

                        case SelectedButton.Mercy://MERCY：选择敌人
                            LayerOneSet();
                            if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                            {
                                var save = new List<string>();
                                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.BattleControl.mercySave, save, MainControl.Instance.BattleControl.enemies[selectedName].name + "\\");
                                TextProcessingService.SplitStringToListWithDelimiter(save, actSave);

                                _textUI.text = "<color=#00000000>aa</color> * " + actSave[0];
                                if (actSave.Count > MainControl.Instance.BattleControl.enemies.Count)
                                    _textUI.text += "\n<color=#00000000>aa</color> * " + actSave[2];
                                if (actSave.Count > 4 * MainControl.Instance.BattleControl.enemies.Count)
                                    _textUI.text += "\n<color=#00000000>aa</color> * " + actSave[4];
                            }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case SelectedLayer.OptionLayer:
                    switch (selectedButton)
                    {
                        
                        case SelectedButton.Fight:
                            break;
                        case SelectedButton.Act:
                            if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow) && selectedOption - 2 >= 0)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectedOption -= 2;
                            }
                            else if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow) && selectedOption + 2 <= actSave.Count / 2 - 1)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectedOption += 2;
                            }
                            if (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow) && selectedOption - 1 >= 0)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectedOption--;
                            }
                            else if (GameUtilityService.KeyArrowToControl(KeyCode.RightArrow) && selectedOption + 1 <= actSave.Count / 2 - 1)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectedOption++;
                            }

                            float playerPosX, playerPosY;
                            if ((selectedOption - 1) % 2 == 0)
                            {
                                playerPosX = 0.25f;
                            }
                            else
                            {
                                playerPosX = -5.175f;
                            }
                            if (selectedOption < 2)
                            {
                                playerPosY = -0.96f - 0 * 0.66f;
                            }
                            else
                            {
                                playerPosY = -0.96f - 1 * 0.66f;
                            }
                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(playerPosX, playerPosY, MainControl.Instance.battlePlayerController.transform.position.z);
                            if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                            {
                                selectedLayer = SelectedLayer.NameLayer;
                                selectedOption = 0;
                                _textUI.text = "";
                                _textUIBack.text = "";
                                foreach (var t in MainControl.Instance.BattleControl.enemies)
                                {
                                    _textUI.text += "<color=#00000000>aa*</color>* " + t.name + "\n";
                                }
                            }
                            else if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                            {
                                switch (selectedName)//在这里写ACT的相关触发代码
                                {
                                    case 0://怪物0
                                        switch (selectedOption)//选项
                                        {
                                            case 0:

                                                break;

                                            case 1:

                                                Global.Other.Debug.Log(1);
                                                AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipBattle);

                                                break;

                                            case 2:

                                                break;

                                            case 3:

                                                break;
                                        }
                                        break;

                                    case 1://怪物1
                                        switch (selectedOption)//选项
                                        {
                                            case 0:

                                                break;

                                            case 1:

                                                break;

                                            case 2:

                                                break;

                                            case 3:

                                                break;
                                        }
                                        break;

                                    case 2://怪物2
                                        switch (selectedOption)//选项
                                        {
                                            case 0:

                                                break;

                                            case 1:

                                                break;

                                            case 2:

                                                break;

                                            case 3:

                                                break;
                                        }
                                        break;
                                }

                                _textUIBack.text = "";
                                selectedLayer = SelectedLayer.NarratorLayer;
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                Type(actSave[2 * selectedOption - 3]);
                                SpriteChange();
                                _itemSelectController.Close();
                            }

                            break;

                        case SelectedButton.Item:
                            if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                            {
                                selectedLayer = SelectedLayer.ButtonLayer;
                                selectedName = 0;
                                if (!firstIn)
                                    TurnTextLoad();
                                else
                                    TurnTextLoad(true, firstInDiy);
                                _itemSelectController.Close();

                                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                                _textUIBack.text = "";

                                UITextUpdate(UITextMode.Food);
                                break;
                            }

                            if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                            {
                                selectedLayer = SelectedLayer.NarratorLayer; 
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                GameUtilityService.UseItem(_typeWritter, _textUI, selectedName + 1);
                                SpriteChange();
                                _itemSelectController.Close();

                                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                                _textUIBack.text = "";

                                UITextUpdate(UITextMode.Food);
                                break;
                            }

                            var textUITextChanger1 = "";
                            var textUITextChanger2 = "";

                            var textUIDataChanger1 = "";
                            var textUIDataChanger2 = "";

                            var myItemMax = ListManipulationService.FindFirstZeroIndex(MainControl.Instance.playerControl.myItems);

                            if (myItemMax > 1)
                            {
                                textUITextChanger1 = "<color=#00000000>aa*</color>* " + DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName + 1 - selectedOption], "Auto", 0) + "\n";
                                textUIDataChanger1 = DataHandlerService.ItemIdGetData(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName + 1 - selectedOption], "Auto", true) + "\n";
                            }
                            if (myItemMax > 2)
                            {
                                textUITextChanger2 = "<color=#00000000>aa*</color>* " + DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName + 2 - selectedOption], "Auto", 0) + "\n";
                                textUIDataChanger2 = DataHandlerService.ItemIdGetData(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName + 2 - selectedOption], "Auto", true) + "\n";
                            }
                            var number = 8;

                            if (myItemMax >= 8)
                            {
                                _itemSelectController.myItemSelect = selectedName;
                            }
                            else //if (myItemMax < number)
                            {
                                number = myItemMax switch
                                {
                                    >= 6 => 8,
                                    >= 4 => 7,
                                    >= 2 => 6,
                                    >= 1 => 5,
                                    _ => number
                                };
                                if (myItemMax % 2 == 0)
                                {
                                    _itemSelectController.myItemSelect = selectedName + (number - 1 - myItemMax);
                                }
                                else
                                    _itemSelectController.myItemSelect = selectedName + (number - myItemMax);
                            }
                            _itemSelectController.myItemRealSelect = selectedName;
                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - selectedOption * 0.66f, MainControl.Instance.battlePlayerController.transform.position.z);

                            _textUI.text = "<color=#00000000>aa*</color>* " + DataHandlerService.ItemIdGetName(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName - selectedOption], "Auto", 0) + "\n" +
                                          textUITextChanger1 + textUITextChanger2;
                            _textUIBack.text = DataHandlerService.ItemIdGetData(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName - selectedOption], "Auto", true) + "\n" + textUIDataChanger1 + textUIDataChanger2;

                            if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow) && selectedName > 0)
                            {
                                if (selectedOption > 0)
                                    selectedOption--;
                                _itemSelectController.PressDown(true);
                                selectedName--;
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);

                                if (MainControl.Instance.playerControl.myItems[selectedName] < 10000)
                                    UITextUpdate(UITextMode.Food, int.Parse(DataHandlerService.ItemIdGetData(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName], "Auto")));
                                else
                                    UITextUpdate(UITextMode.Food);
                            }
                            if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow) && selectedName < myItemMax - 1)
                            {
                                if (selectedOption < 2)
                                    selectedOption++;
                                _itemSelectController.PressDown(false);
                                selectedName++;
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);

                                if (MainControl.Instance.playerControl.myItems[selectedName] < 10000)
                                    UITextUpdate(UITextMode.Food, int.Parse(DataHandlerService.ItemIdGetData(MainControl.Instance.ItemControl, MainControl.Instance.playerControl.myItems[selectedName], "Auto")));
                                else
                                    UITextUpdate(UITextMode.Food);
                            }

                            _hpUI.text = FormatWithLeadingZero(_hpFood) + " / " + FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
                            break;

                        case SelectedButton.Mercy:
                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - selectedOption * 0.66f, MainControl.Instance.battlePlayerController.transform.position.z);
                            if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                            {
                                selectedLayer = SelectedLayer.NameLayer;
                                selectedOption = 0;
                                _textUI.text = "";
                                foreach (var t in MainControl.Instance.BattleControl.enemies)
                                {
                                    _textUI.text += "<color=#00000000>aa*</color>* " + t.name + "\n";
                                }
                            }
                            else if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                            {
                                selectedLayer = SelectedLayer.NarratorLayer;
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                if (actSave[2 * selectedOption - 3] != "Null")
                                    Type(actSave[2 * selectedOption - 3]);
                                else
                                {
                                    _textUI.text = "";
                                    MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                    OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                                }
                                SpriteChange();
                                _itemSelectController.Close();
                            }

                            if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow) && selectedOption - 1 >= 0)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectedOption--;
                            }
                            else if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow) && selectedOption + 1 <= actSave.Count / 2 - 1)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectedOption++;
                            }

                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    break;

                case SelectedLayer.NarratorLayer:
                    //DataHandlerService.ForceJumpLoadTurn  = true;
                    firstIn = false;

                    if (selectedButton == SelectedButton.Fight && !_target.gameObject.activeSelf)
                    {
                        if (MainControl.Instance.battlePlayerController.transform.position != (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z))
                        {
                            _textUI.text = "";
                            MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                            OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                        }
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                    {
                        MainControl.Instance.battlePlayerController.collideCollider.enabled = true;
                        if (!isDialog)
                        {
                            if (selectedButton != SelectedButton.Fight && _textUI.text == "")
                            {
                                OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                break;
                            }
                            if (selectedButton != SelectedButton.Fight && !_typeWritter.isTyping)
                            {
                                if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                                {
                                    _textUI.text = "";
                                    MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                    OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                                }
                            }
                        }
                    }

                    break;
                case SelectedLayer.TurnLayer:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OpenDialogBubble(string textAsset)
        {
            MainControl.Instance.BattleControl.randomTurnDir = MathUtilityService.Get1Or_1();
            DataHandlerService.LoadItemData(actSave, textAsset);
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
            _dialog.typeWritter.TypeOpen(save[5], false, 0, 1, _dialog.tmp);
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
            var turnTextSaveChanged = (from t in turnTextSave where t[..turn.ToString().Length] == turn.ToString() select t[(turn.ToString().Length + 1)..]).ToList();
            var saves = new List<string>();
            TextProcessingService.SplitStringToListWithDelimiter(turnTextSaveChanged, saves);
            return saves;
        }

        public enum UITextMode
        {
            None,
            Hit,
            Food
        }

        private Tween _hpFoodTween;
        private int _hpFood;

        /// <summary>
        /// 更新UI文字与血条
        /// </summary>
        public void UITextUpdate(UITextMode uiTextMode = 0, int foodNumber = 0)
        {
            _hpSpr.transform.localScale = new Vector3(0.525f * MainControl.Instance.playerControl.hpMax, 8.5f);
            _hpSpr.material.SetColor(ColorUnder, hpColorUnder);
            _hpSpr.material.SetColor(ColorOn, hpColorOn);

            switch (uiTextMode)
            {
                case UITextMode.None:
                    goto default;
                default:
                    _hpSpr.material.SetFloat(Crop, (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    _hpSpr.material.SetFloat(Flash, (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    break;

                case UITextMode.Hit:
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat(IsFlashing, 0);
                    _hpSpr.material.SetColor(ColorFlash, hpColorHit);
                    _hpSpr.material.SetFloat(Crop, (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    _hpSpr.material.DOFloat((float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax, "_Flash", 0.5f).SetEase(Ease.OutCirc);

                    break;

                case UITextMode.Food:
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat(IsFlashing, 1);
                    _hpSpr.material.SetFloat(Crop, (float)MainControl.Instance.playerControl.hp / MainControl.Instance.playerControl.hpMax);
                    float addNumber = MainControl.Instance.playerControl.hp + foodNumber;
                    if (addNumber > MainControl.Instance.playerControl.hpMax)
                        addNumber = MainControl.Instance.playerControl.hpMax;
                    _hpSpr.material.DOFloat(addNumber / MainControl.Instance.playerControl.hpMax, "_Flash", 0.5f).SetEase(Ease.OutCirc);
                    break;
            }

            _hpUI.transform.localPosition = new Vector3(9.85f + 0.0265f * (MainControl.Instance.playerControl.hpMax - 20), -5.825f);
            NameUIUpdate();

            if (uiTextMode != UITextMode.Food)
                _hpUI.text = FormatWithLeadingZero(MainControl.Instance.playerControl.hp) + " / " + FormatWithLeadingZero(MainControl.Instance.playerControl.hpMax);
            else
            {
                _hpFoodTween.Kill();
                var addNumber = MainControl.Instance.playerControl.hp + foodNumber;
                if (addNumber > MainControl.Instance.playerControl.hpMax)
                    addNumber = MainControl.Instance.playerControl.hpMax;
                _hpFoodTween = DOTween.To(() => _hpFood, x => _hpFood = x, addNumber, 0.5f);
            }
        }

        private void NameUIUpdate()
        {
            _nameUI.text = MainControl.Instance.playerControl.playerName + " lv<size=3><color=#00000000>0</size></color>" + MainControl.Instance.playerControl.lv;
        }

        /// <summary>
        /// 将数字格式化为两位数（前导零）显示，例如将 1 显示为 01。
        /// </summary>
        private static string FormatWithLeadingZero(int i)
        {
            return i.ToString("D2");
        }
    }
}