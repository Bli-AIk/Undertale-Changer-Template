using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
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

        [Header("四个按钮UI的选择 0开")]
        public int selectUI;

        [Header("层")]
        public int selectLayer;//0选择按钮 1选择名称 2Act选项/背包层 3执行层 进入敌方回合后归零

        [Header("子选择")]
        public int selectSon;

        public int selectGrandSon;//Item&Mercy:1 2 3三个位置 ACT:四个位置
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
            for (int i = 0; i < loadButton.Length; i++)
            {
                buttons.Add(transform.Find(loadButton[i]).GetComponent<SpriteRenderer>());
            }

            for (int i = 0; i < MainControl.Instance.BattleControl.enemies.Count; i++)
            {
                EnemiesController enemies = GameObject.Find(MainControl.Instance.BattleControl.enemies[i].name).GetComponent<EnemiesController>();
                if (enemies != null)
                {
                    enemiesControllers.Add(enemies);
                    enemiesControllers[i].atk = MainControl.Instance.BattleControl.enemiesAtk[i];
                    enemiesControllers[i].def = MainControl.Instance.BattleControl.enemiesDef[i];
                }
            }
            selectUI = 1;
            TurnTextLoad(true);
            _enemiesHpLine.SetActive(false);

            UITextUpdate();

            _hpFood = MainControl.Instance.PlayerControl.hp;

            InTurn();
        }

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;

            if (TurnController.Instance.isMyTurn)
                MyTurn();

            _dialog.gameObject.SetActive(isDialog);

            if (isDialog)
            {
                if ((!_dialog.typeWritter.isTyping && (MainControl.Instance.KeyArrowToControl(KeyCode.Z))) || ((selectUI == 1 || _textUI.text == "") && numberDialog == 0))
                {
                    if (numberDialog < actSave.Count)
                        KeepDialogBubble();
                    else//敌方回合：开！
                    {
                        isDialog = false;

                        TurnController.Instance.OutYourTurn();

                        _itemSelectController.gameObject.SetActive(false);
                        actSave = new List<string>();
                        selectLayer = 4;
                    }
                }
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
            (buttons[selectUI - 1].sprite, spriteUI[selectUI - 1]) = (spriteUI[selectUI - 1], buttons[selectUI - 1].sprite);
        }

        /// <summary>
        /// selectUI=1时的设定
        /// 主要为选定怪物
        /// </summary>
        private void LayerOneSet()
        {
            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - selectSon * 0.66f, MainControl.Instance.battlePlayerController.transform.position.z);
            if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow) && selectSon < MainControl.Instance.BattleControl.enemies.Count - 1)
            {
                selectSon++;
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
            }
            if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow) && selectSon > 0)
            {
                selectSon--;
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
            }
        }

        /// <summary>
        ///进我方回合
        /// </summary>
        public void InTurn()
        {
            TurnController.Instance.isMyTurn = true;
            selectLayer = 0;
            selectUI = 1;
            selectSon = 0;
            selectGrandSon = 0;
            SpriteChange();
            TurnTextLoad();
            _itemSelectController.gameObject.SetActive(true);

            MainControl.Instance.forceJumpLoadTurn = false;

            MainControl.Instance.battlePlayerController.collideCollider.enabled = false;
        }

        /// <summary>
        /// 我的回合！抽卡)
        /// </summary>
        private void MyTurn()
        {
            switch (selectLayer)
            {
                case 0:

                    MainControl.Instance.battlePlayerController.transform.position = (Vector3)playerUIPos[selectUI - 1] + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                    if (MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        if (selectUI >= 1)
                        {
                            SpriteChange();
                            if (selectUI == 1)
                                selectUI = 4;
                            else
                                selectUI -= 1;
                            SpriteChange();
                        }
                    }
                    else if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        if (selectUI <= 4)
                        {
                            SpriteChange();
                            if (selectUI == 4)
                                selectUI = 1;
                            else
                                selectUI += 1;
                            SpriteChange();
                        }
                    }
                    if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                    {
                        selectLayer = 1;
                        selectGrandSon = 1;
                        if (!(MainControl.Instance.PlayerControl.myItems[0] == 0 && selectUI == 3))
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                            _typeWritter.TypeStop();
                            _textUI.text = "";
                        }
                        if (selectUI != 3)
                            for (int i = 0; i < MainControl.Instance.BattleControl.enemies.Count; i++)
                            {
                                _textUI.text += "<color=#00000000>aa*</color>* " + MainControl.Instance.BattleControl.enemies[i].name + "\n";
                            }
                        else
                        {
                            MainControl.Instance.PlayerControl.myItems = MainControl.Instance.ListOrderChanger(MainControl.Instance.PlayerControl.myItems);

                            _textUIBack.rectTransform.anchoredPosition = new Vector2(-5, -3.3f);
                            _textUIBack.alignment = TextAlignmentOptions.TopRight;
                            _hpSpr.material.SetFloat("_IsFlashing", 1);
                            _hpSpr.material.SetColor("_ColorFlash", hpColorOn);

                            _hpFood = MainControl.Instance.PlayerControl.hp;

                        }

                        if (MainControl.Instance.PlayerControl.myItems[0] == 0 && selectUI == 3)
                            selectLayer = 0;
                    }

                    //if (hpFood != MainControl.instance.PlayerControl.hp)
                    _hpUI.text = UihpVoid(_hpFood) + " / " + UihpVoid(MainControl.Instance.PlayerControl.hpMax);
                    break;

                case 1:
                    if (MainControl.Instance.KeyArrowToControl(KeyCode.X))
                    {
                        selectLayer = 0;
                        selectSon = 0;
                        if (!firstIn)
                            TurnTextLoad();
                        else
                            TurnTextLoad(true, firstInDiy);
                        _enemiesHpLine.SetActive(false);
                        break;
                    }

                    if (MainControl.Instance.KeyArrowToControl(KeyCode.Z) && selectUI != 3)
                    {
                        if (selectUI != 1)
                            selectLayer = 2;
                        else
                        {
                            selectLayer = 3;
                            SpriteChange();
                        }

                        selectGrandSon = 1;
                        _textUI.text = "";
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                    }
                    switch (selectUI)
                    {
                        case 1://FIGHT：选择敌人
                            _enemiesHpLine.SetActive(true);
                            LayerOneSet();
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                            {
                                _enemiesHpLine.SetActive(false);
                                _target.gameObject.SetActive(true);
                                _target.select = selectSon;
                                _target.transform.Find("Move").transform.position = new Vector3(MainControl.Instance.BattleControl.enemies[selectSon].transform.position.x, _target.transform.Find("Move").transform.position.y);
                                _target.hitMonster = enemiesControllers[selectSon];
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z); 
                            }
                            break;

                        case 2://ACT：选择敌人
                            LayerOneSet();
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                            {
                                List<string> save = new List<string>();
                                MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.BattleControl.actSave, save, MainControl.Instance.BattleControl.enemies[selectSon].name + "\\");
                                MainControl.Instance.MaxToOneSon(save, actSave);

                                _textUI.text = "<color=#00000000>aa</color> * " + actSave[0];
                                _textUIBack.text = "";
                                if (actSave.Count > MainControl.Instance.BattleControl.enemies.Count)
                                    _textUIBack.text += "* " + actSave[2];
                                if (actSave.Count > 2 * MainControl.Instance.BattleControl.enemies.Count)
                                    _textUI.text += "\n<color=#00000000>aa</color> * " + actSave[4];
                                if (actSave.Count > 3 * MainControl.Instance.BattleControl.enemies.Count)
                                    _textUIBack.text += "\n* " + actSave[6];
                                for (int i = 0; i < actSave.Count; i++)
                                {
                                    actSave[i] += ';';
                                }

                                actSave = MainControl.Instance.ChangeItemData(actSave, false, new List<string> { enemiesControllers[selectSon].name, enemiesControllers[selectSon].atk.ToString(), enemiesControllers[selectSon].def.ToString() });

                                for (int i = 0; i < actSave.Count; i++)
                                {
                                    actSave[i] = actSave[i].Substring(0, actSave[i].Length - 1);
                                }

                                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                            }
                            break;

                        case 3://ITEM：跳2
                            _itemSelectController.myItemMax = MainControl.Instance.FindMax(MainControl.Instance.PlayerControl.myItems);
                            _itemSelectController.Open();
                            selectLayer = 2;

                            if (MainControl.Instance.PlayerControl.myItems[selectSon] < 10000)
                                UITextUpdate(UITextMode.Food, int.Parse(MainControl.Instance.ItemIdGetData(MainControl.Instance.PlayerControl.myItems[selectSon], "Auto")));
                            else
                                UITextUpdate(UITextMode.Food);
                            break;

                        case 4://MERCY：选择敌人
                            LayerOneSet();
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                            {
                                List<string> save = new List<string>();
                                MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.BattleControl.mercySave, save, MainControl.Instance.BattleControl.enemies[selectSon].name + "\\");
                                MainControl.Instance.MaxToOneSon(save, actSave);

                                _textUI.text = "<color=#00000000>aa</color> * " + actSave[0];
                                if (actSave.Count > MainControl.Instance.BattleControl.enemies.Count)
                                    _textUI.text += "\n<color=#00000000>aa</color> * " + actSave[2];
                                if (actSave.Count > 4 * MainControl.Instance.BattleControl.enemies.Count)
                                    _textUI.text += "\n<color=#00000000>aa</color> * " + actSave[4];
                            }
                            break;
                    }
                    break;

                case 2:
                    switch (selectUI)
                    {
                        case 2:
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow) && selectGrandSon - 2 >= 1)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectGrandSon -= 2;
                            }
                            else if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow) && selectGrandSon + 2 <= actSave.Count / 2)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectGrandSon += 2;
                            }
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow) && selectGrandSon - 1 >= 1)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectGrandSon--;
                            }
                            else if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow) && selectGrandSon + 1 <= actSave.Count / 2)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectGrandSon++;
                            }

                            float playerPosX, playerPosY;
                            if (selectGrandSon % 2 == 0)
                            {
                                playerPosX = 0.25f;
                            }
                            else
                            {
                                playerPosX = -5.175f;
                            }
                            if (selectGrandSon < 3)
                            {
                                playerPosY = -0.96f - 0 * 0.66f;
                            }
                            else
                            {
                                playerPosY = -0.96f - 1 * 0.66f;
                            }
                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(playerPosX, playerPosY, MainControl.Instance.battlePlayerController.transform.position.z);
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.X))
                            {
                                selectLayer = 1;
                                selectGrandSon = 1;
                                _textUI.text = "";
                                _textUIBack.text = "";
                                for (int i = 0; i < MainControl.Instance.BattleControl.enemies.Count; i++)
                                {
                                    _textUI.text += "<color=#00000000>aa*</color>* " + MainControl.Instance.BattleControl.enemies[i].name + "\n";
                                }
                            }
                            else if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                            {
                                switch (selectSon)//在这里写ACT的相关触发代码
                                {
                                    case 0://怪物0
                                        switch (selectGrandSon)//选项
                                        {
                                            case 1:

                                                break;

                                            case 2:

                                                Global.Other.Debug.Log(1);
                                                AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipBattle);

                                                break;

                                            case 3:

                                                break;

                                            case 4:

                                                break;
                                        }
                                        break;

                                    case 1://怪物1
                                        switch (selectGrandSon)//选项
                                        {
                                            case 1:

                                                break;

                                            case 2:

                                                break;

                                            case 3:

                                                break;

                                            case 4:

                                                break;
                                        }
                                        break;

                                    case 2://怪物2
                                        switch (selectGrandSon)//选项
                                        {
                                            case 1:

                                                break;

                                            case 2:

                                                break;

                                            case 3:

                                                break;

                                            case 4:

                                                break;
                                        }
                                        break;
                                }

                                _textUIBack.text = "";
                                selectLayer = 3;
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                Type(actSave[2 * (selectGrandSon + 1) - 3]);
                                SpriteChange();
                                _itemSelectController.Close();
                            }

                            break;

                        case 3:
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.X))
                            {
                                selectLayer = 0;
                                selectSon = 0;
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

                            if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                            {
                                selectLayer = 3; 
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                MainControl.Instance.UseItem(_typeWritter, _textUI, selectSon + 1);
                                SpriteChange();
                                _itemSelectController.Close();

                                _textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                                _textUIBack.alignment = TextAlignmentOptions.TopLeft;
                                _textUIBack.text = "";

                                UITextUpdate(UITextMode.Food);
                                break;
                            }

                            //hpSpr.material.SetFloat("_Crop", 1);

                            string textUITextChanger1 = "";
                            string textUITextChanger2 = "";

                            string textUIDataChanger1 = "";
                            string textUIDataChanger2 = "";

                            int myItemMax = MainControl.Instance.FindMax(MainControl.Instance.PlayerControl.myItems);

                            if (myItemMax > 1)
                            {
                                textUITextChanger1 = "<color=#00000000>aa*</color>* " + MainControl.Instance.ItemIdGetName(MainControl.Instance.PlayerControl.myItems[selectSon + 1 - (selectGrandSon - 1)], "Auto", 0) + "\n";
                                textUIDataChanger1 = MainControl.Instance.ItemIdGetData(MainControl.Instance.PlayerControl.myItems[selectSon + 1 - (selectGrandSon - 1)], "Auto", true) + "\n";
                            }
                            if (myItemMax > 2)
                            {
                                textUITextChanger2 = "<color=#00000000>aa*</color>* " + MainControl.Instance.ItemIdGetName(MainControl.Instance.PlayerControl.myItems[selectSon + 2 - (selectGrandSon - 1)], "Auto", 0) + "\n";
                                textUIDataChanger2 = MainControl.Instance.ItemIdGetData(MainControl.Instance.PlayerControl.myItems[selectSon + 2 - (selectGrandSon - 1)], "Auto", true) + "\n";
                            }
                            int number = 8;

                            if (myItemMax >= 8)
                            {
                                _itemSelectController.myItemSelect = selectSon;
                            }
                            else //if (myItemMax < number)
                            {
                                if (myItemMax >= 6)
                                {
                                    number = 8;
                                }
                                else if (myItemMax >= 4)
                                {
                                    number = 7;
                                }
                                else if (myItemMax >= 2)
                                {
                                    number = 6;
                                }
                                else if (myItemMax >= 1)
                                {
                                    number = 5;
                                }
                                if (myItemMax % 2 == 0)
                                {
                                    _itemSelectController.myItemSelect = selectSon + (number - 1 - myItemMax);
                                }
                                else
                                    _itemSelectController.myItemSelect = selectSon + (number - myItemMax);
                            }
                            _itemSelectController.myItemRealSelect = selectSon;
                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - (selectGrandSon - 1) * 0.66f, MainControl.Instance.battlePlayerController.transform.position.z);

                            _textUI.text = "<color=#00000000>aa*</color>* " + MainControl.Instance.ItemIdGetName(MainControl.Instance.PlayerControl.myItems[selectSon - (selectGrandSon - 1)], "Auto", 0) + "\n" +
                                          textUITextChanger1 + textUITextChanger2;
                            _textUIBack.text = MainControl.Instance.ItemIdGetData(MainControl.Instance.PlayerControl.myItems[selectSon - (selectGrandSon - 1)], "Auto", true) + "\n" + textUIDataChanger1 + textUIDataChanger2;

                            if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow) && selectSon > 0)
                            {
                                if (selectGrandSon > 1)
                                    selectGrandSon--;
                                _itemSelectController.PressDown(true);
                                selectSon--;
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);

                                if (MainControl.Instance.PlayerControl.myItems[selectSon] < 10000)
                                    UITextUpdate(UITextMode.Food, int.Parse(MainControl.Instance.ItemIdGetData(MainControl.Instance.PlayerControl.myItems[selectSon], "Auto")));
                                else
                                    UITextUpdate(UITextMode.Food);
                            }
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow) && selectSon < myItemMax - 1)
                            {
                                if (selectGrandSon < 3)
                                    selectGrandSon++;
                                _itemSelectController.PressDown(false);
                                selectSon++;
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);

                                if (MainControl.Instance.PlayerControl.myItems[selectSon] < 10000)
                                    UITextUpdate(UITextMode.Food, int.Parse(MainControl.Instance.ItemIdGetData(MainControl.Instance.PlayerControl.myItems[selectSon], "Auto")));
                                else
                                    UITextUpdate(UITextMode.Food);
                            }

                            _hpUI.text = UihpVoid(_hpFood) + " / " + UihpVoid(MainControl.Instance.PlayerControl.hpMax);
                            break;

                        case 4:
                            MainControl.Instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - (selectGrandSon - 1) * 0.66f, MainControl.Instance.battlePlayerController.transform.position.z);
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.X))
                            {
                                selectLayer = 1;
                                selectGrandSon = 1;
                                _textUI.text = "";
                                for (int i = 0; i < MainControl.Instance.BattleControl.enemies.Count; i++)
                                {
                                    _textUI.text += "<color=#00000000>aa*</color>* " + MainControl.Instance.BattleControl.enemies[i].name + "\n";
                                }
                            }
                            else if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                            {
                                selectLayer = 3;
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                if (actSave[2 * (selectGrandSon + 1) - 3] != "Null")
                                    Type(actSave[2 * (selectGrandSon + 1) - 3]);
                                else
                                {
                                    _textUI.text = "";
                                    MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                    OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                                }
                                SpriteChange();
                                _itemSelectController.Close();
                            }

                            if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow) && selectGrandSon - 1 >= 1)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectGrandSon--;
                            }
                            else if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow) && selectGrandSon + 1 <= actSave.Count / 2)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                selectGrandSon++;
                            }

                            break;
                    }
                    break;

                case 3:
                    MainControl.Instance.forceJumpLoadTurn = true;
                    firstIn = false;

                    if (((selectUI == 1) && !_target.gameObject.activeSelf))
                    {
                        if (MainControl.Instance.battlePlayerController.transform.position != (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z))
                        {
                            _textUI.text = "";
                            MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                            OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                        }
                    }
                    else if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                    {
                        MainControl.Instance.battlePlayerController.collideCollider.enabled = true;
                        if (!isDialog)
                        {
                            if (selectUI != 1 && _textUI.text == "")
                            {
                                OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                                MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                break;
                            }
                            if (selectUI != 1 && !_typeWritter.isTyping)
                            {
                                if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                                {
                                    _textUI.text = "";
                                    MainControl.Instance.battlePlayerController.transform.position = (Vector3)MainControl.Instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.Instance.battlePlayerController.transform.position.z);
                                    OpenDialogBubble(MainControl.Instance.BattleControl.turnDialogAsset[TurnController.Instance.turn]);
                                }
                            }
                        }
                    }

                    break;
            }
        }

        private void OpenDialogBubble(string textAsset)
        {
            MainControl.Instance.BattleControl.randomTurnDir = MainControl.Instance.Get1Or_1();
            MainControl.Instance.LoadItemData(actSave, textAsset);
            actSave = MainControl.Instance.ChangeItemData(actSave, true, new List<string>());
            isDialog = true;
            numberDialog = 0;
            //if (selectUI == 1)
            //    KeepDialogBubble();
        }

        private void KeepDialogBubble()
        {
            List<string> save = new List<string>();
            MainControl.Instance.MaxToOneSon(actSave[numberDialog], save);
            for (int i = 0; i < enemiesControllers.Count; i++)
            {
                if (save[2] == enemiesControllers[i].name)
                {
                    _dialog.transform.SetParent(enemiesControllers[i].transform);
                    break;
                }
            }

            _dialog.size = MainControl.Instance.StringVector2ToRealVector2(save[0], _dialog.size);
            _dialog.position = MainControl.Instance.StringVector2ToRealVector2(save[1], _dialog.position);

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
                List<string> load = new List<string>();
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

        private List<string> TurnTextLoad(List<string> turnTextSave, int turn)
        {
            List<string> turnTextSaveChanged = new List<string>();
            for (int i = 0; i < turnTextSave.Count; i++)
            {
                if (turnTextSave[i].Substring(0, turn.ToString().Length) == turn.ToString())
                    turnTextSaveChanged.Add(turnTextSave[i].Substring(turn.ToString().Length + 1));
            }
            List<string> saves = new List<string>();
            MainControl.Instance.MaxToOneSon(turnTextSaveChanged, saves);
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
            _hpSpr.transform.localScale = new Vector3(0.525f * MainControl.Instance.PlayerControl.hpMax, 8.5f);
            _hpSpr.material.SetColor("_ColorUnder", hpColorUnder);
            _hpSpr.material.SetColor("_ColorOn", hpColorOn);

            switch (uiTextMode)
            {
                case UITextMode.None:
                    goto default;
                default:
                    _hpSpr.material.SetFloat("_Crop", (float)MainControl.Instance.PlayerControl.hp / MainControl.Instance.PlayerControl.hpMax);
                    _hpSpr.material.SetFloat("_Flash", (float)MainControl.Instance.PlayerControl.hp / MainControl.Instance.PlayerControl.hpMax);
                    break;

                case UITextMode.Hit:
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat("_IsFlashing", 0);
                    _hpSpr.material.SetColor("_ColorFlash", hpColorHit);
                    _hpSpr.material.SetFloat("_Crop", (float)MainControl.Instance.PlayerControl.hp / MainControl.Instance.PlayerControl.hpMax);
                    _hpSpr.material.DOFloat((float)MainControl.Instance.PlayerControl.hp / MainControl.Instance.PlayerControl.hpMax, "_Flash", 0.5f).SetEase(Ease.OutCirc);

                    break;

                case UITextMode.Food:
                    _hpSpr.material.DOKill();

                    _hpSpr.material.SetFloat("_IsFlashing", 1);
                    _hpSpr.material.SetFloat("_Crop", (float)MainControl.Instance.PlayerControl.hp / MainControl.Instance.PlayerControl.hpMax);
                    float addNumber = MainControl.Instance.PlayerControl.hp + foodNumber;
                    if (addNumber > MainControl.Instance.PlayerControl.hpMax)
                        addNumber = MainControl.Instance.PlayerControl.hpMax;
                    _hpSpr.material.DOFloat(addNumber / MainControl.Instance.PlayerControl.hpMax, "_Flash", 0.5f).SetEase(Ease.OutCirc);
                    break;
            }

            _hpUI.transform.localPosition = new Vector3(9.85f + 0.0265f * (MainControl.Instance.PlayerControl.hpMax - 20), -5.825f);
            _nameUI.text = MainControl.Instance.PlayerControl.playerName + " lv<size=3><color=#00000000>0</size></color>" + MainControl.Instance.PlayerControl.lv;

            if (uiTextMode != UITextMode.Food)
                _hpUI.text = UihpVoid(MainControl.Instance.PlayerControl.hp) + " / " + UihpVoid(MainControl.Instance.PlayerControl.hpMax);
            else
            {
                _hpFoodTween.Kill();
                int addNumber = MainControl.Instance.PlayerControl.hp + foodNumber;
                if (addNumber > MainControl.Instance.PlayerControl.hpMax)
                    addNumber = MainControl.Instance.PlayerControl.hpMax;
                _hpFoodTween = DOTween.To(() => _hpFood, x => _hpFood = x, addNumber, 0.5f);
            }
        }

        /// <summary>
        /// 解决hpUI把01显示成1的问题)
        /// </summary>
        private string UihpVoid(int i)
        {
            if (0 <= i && i < 10)
                return "0" + i;
            return i.ToString();
        }
    }
}