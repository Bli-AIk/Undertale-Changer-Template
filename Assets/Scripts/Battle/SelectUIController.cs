using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Log;
/// <summary>
/// UI Controller in Battle Scene
/// Also responsible for player turn control.
/// </summary>
public class SelectUIController : MonoBehaviour
{
    private TextMeshPro nameUI, hpUI, textUI, textUIBack;
    private SpriteRenderer hpSpr;

    [Header("HP Article Color Scheme")]
    public Color hpColorUnder;

    public Color hpColorOn;
    public Color hpColorHit;

    [Header("Number of dialog bubbles loaded")]
    //Load actSave
    public int numDialog;

    public bool isDialog;

    [Header("Storing unused Sprite")]
    public List<Sprite> spriteUI;

    public List<SpriteRenderer> buttons;
    public List<Vector2> playerUIPos;

    [Header("Selection of four buttons UI 0 on")]
    public int selectUI;

    [Header("Layer")]
    public int selectLayer;
    //0 Select Button 1 Select Name 2 Act Option/Backpack Layer 3 Execute Layer Zeroed after entering enemy turn

    [Header("Subselect")]
    public int selectSon;

    public int selectGrandSon;
    //Item&Mercy:1 2 3 three positions ACT:four positions
    private ItemSelectController itemSelectController;
    private TypeWritter typeWritter;
    private GameObject enemiesHpLine;

    [Header("Save ACT option for invocation")]
    public List<string> actSave;

    [Header("Automatically find monsters for battle masters. Make sure they have the same name.")]
    public List<EnemiesController> enemiesControllers;

    private TargetController target;
    private DialogBubbleBehaviour dialog;

    private int saveTurn = -1;
    private string saveTurnText = "";

    [Header("Play customized turn text the first time you enter a turn")]
    public bool firstIn = false;

    public int firstInDiy = -1;

    private void Start()
    {
        target = transform.Find("Target").GetComponent<TargetController>();
        target.gameObject.SetActive(false);
        nameUI = transform.Find("Name UI").GetComponent<TextMeshPro>();
        hpUI = transform.Find("HP UI").GetComponent<TextMeshPro>();
        textUI = transform.Find("Text UI").GetComponent<TextMeshPro>();
        textUIBack = transform.Find("Text UI Back").GetComponent<TextMeshPro>();
        hpSpr = transform.Find("HP").GetComponent<SpriteRenderer>();
        itemSelectController = transform.Find("ItemSelect").GetComponent<ItemSelectController>();
        enemiesHpLine = transform.Find("EnemiesHpLine").gameObject;
        dialog = GameObject.Find("DialogBubble").GetComponent<DialogBubbleBehaviour>();
        dialog.gameObject.SetActive(false);
        typeWritter = GetComponent<TypeWritter>();
        string[] loadButton = new string[] {
            "FIGHT",
            "ACT",
            "ITEM",
            "MERCY"};
        for (int i = 0; i < loadButton.Length; i++)
        {
            buttons.Add(transform.Find(loadButton[i]).GetComponent<SpriteRenderer>());
        }

        for (int i = 0; i < MainControl.instance.BattleControl.enemies.Count; i++)
        {
            EnemiesController enemies = GameObject.Find(MainControl.instance.BattleControl.enemies[i].name).GetComponent<EnemiesController>();
            if (enemies != null)
            {
                enemiesControllers.Add(enemies);
                enemiesControllers[i].atk = MainControl.instance.BattleControl.enemiesATK[i];
                enemiesControllers[i].def = MainControl.instance.BattleControl.enemiesDEF[i];
            }
        }
        selectUI = 1;
        TurnTextLoad(true, 0);
        enemiesHpLine.SetActive(false);

        UITextUpdate(0);

        hpFood = MainControl.instance.PlayerControl.hp;

        InTurn();
    }

    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;

        if (TurnController.instance.isMyTurn)
            MyTurn();

        dialog.gameObject.SetActive(isDialog);

        if (isDialog)
        {
            if ((!dialog.typeWritter.isTyping && (MainControl.instance.KeyArrowToControl(KeyCode.Z))) || ((selectUI == 1 || textUI.text == "") && numDialog == 0))
            {
                if (numDialog < actSave.Count)
                    KeepDialogBubble();
                else
                //Enemy turn: open!
                {
                    isDialog = false;

                    TurnController.instance.OutYourTurn();

                    itemSelectController.gameObject.SetActive(false);
                    actSave = new List<string>();
                    selectLayer = 4;
                }
            }
        }
    }

    /// <summary>
    //// UI typing No forced text control after typing is complete
    /// </summary>
    private void Type(string text)
    {
        typeWritter.TypeOpen(text, false, 0, 0, textUI);
    }

    /// <summary>
    /// Tactical exchange
    /// </summary>
    private void SpriteChange()
    {
        Sprite sprSave = buttons[selectUI - 1].sprite;
        buttons[selectUI - 1].sprite = spriteUI[selectUI - 1];
        spriteUI[selectUI - 1] = sprSave;
    }

    /// <summary>
    /// Setting when selectUI=1
    /// Mainly for selected monsters.
    /// </summary>
    private void LayerOneSet()
    {
        MainControl.instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - selectSon * 0.66f, MainControl.instance.battlePlayerController.transform.position.z);
        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selectSon < MainControl.instance.BattleControl.enemies.Count - 1)
        {
            selectSon++;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
        }
        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selectSon > 0)
        {
            selectSon--;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
        }
    }

    /// <summary>
    //// Into our turn.
    /// </summary>
    public void InTurn()
    {
        TurnController.instance.isMyTurn = true;
        selectLayer = 0;
        selectUI = 1;
        selectSon = 0;
        selectGrandSon = 0;
        SpriteChange();
        TurnTextLoad();
        itemSelectController.gameObject.SetActive(true);

        MainControl.instance.forceJumpLoadTurn = false;

        MainControl.instance.battlePlayerController.collideCollider.enabled = false;
    }

    /// <summary>
    /// My turn! Draw Cards)
    /// </summary>
    private void MyTurn()
    {
        switch (selectLayer)
        {
            case 0:

                MainControl.instance.battlePlayerController.transform.position = (Vector3)playerUIPos[selectUI - 1] + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
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
                else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                {
                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
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
                if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                {
                    selectLayer = 1;
                    selectGrandSon = 1;
                    if (!(MainControl.instance.PlayerControl.myItems[0] == 0 && selectUI == 3))
                    {
                        AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                        typeWritter.TypeStop();
                        textUI.text = "";
                    }
                    if (selectUI != 3)
                        for (int i = 0; i < MainControl.instance.BattleControl.enemies.Count; i++)
                        {
                            textUI.text += "<color=#00000000>aa*</color>* " + MainControl.instance.BattleControl.enemies[i].name + "\n";
                        }
                    else
                    {
                        MainControl.instance.PlayerControl.myItems = MainControl.instance.ListOrderChanger(MainControl.instance.PlayerControl.myItems);

                        textUIBack.rectTransform.anchoredPosition = new Vector2(-5, -3.3f);
                        textUIBack.alignment = TextAlignmentOptions.TopRight;
                        hpSpr.material.SetFloat("_IsFlashing", 1);
                        hpSpr.material.SetColor("_ColorFlash", hpColorOn);

                        hpFood = MainControl.instance.PlayerControl.hp;

                    }

                    if (MainControl.instance.PlayerControl.myItems[0] == 0 && selectUI == 3)
                        selectLayer = 0;
                }

                //if (hpFood ! = MainControl.instance.PlayerControl.hp)
                hpUI.text = UIHPVoid(hpFood) + " / " + UIHPVoid(MainControl.instance.PlayerControl.hpMax);
                break;

            case 1:
                if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                {
                    selectLayer = 0;
                    selectSon = 0;
                    if (!firstIn)
                        TurnTextLoad();
                    else
                        TurnTextLoad(true, firstInDiy);
                    enemiesHpLine.SetActive(false);
                    break;
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.Z) && selectUI != 3)
                {
                    if (selectUI != 1)
                        selectLayer = 2;
                    else
                    {
                        selectLayer = 3;
                        SpriteChange();
                    }

                    selectGrandSon = 1;
                    textUI.text = "";
                    AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                }
                switch (selectUI)
                {
                    case 1:
                    //FIGHT: Select the enemy.
                        enemiesHpLine.SetActive(true);
                        LayerOneSet();
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            enemiesHpLine.SetActive(false);
                            target.gameObject.SetActive(true);
                            target.select = selectSon;
                            target.transform.Find("Move").transform.position = new Vector3(MainControl.instance.BattleControl.enemies[selectSon].transform.position.x, target.transform.Find("Move").transform.position.y);
                            target.hitMonster = enemiesControllers[selectSon];
                            MainControl.instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                        }
                        break;

                    case 2:
                    //ACT: Selecting Enemies
                        LayerOneSet();
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            List<string> save = new List<string>();
                            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.BattleControl.actSave, save, MainControl.instance.BattleControl.enemies[selectSon].name + "\\");
                            MainControl.instance.MaxToOneSon(save, actSave);

                            textUI.text = "<color=#00000000>aa</color> * " + actSave[0];
                            textUIBack.text = "";
                            if (actSave.Count > MainControl.instance.BattleControl.enemies.Count)
                                textUIBack.text += "* " + actSave[2];
                            if (actSave.Count > 2 * MainControl.instance.BattleControl.enemies.Count)
                                textUI.text += "\n<color=#00000000>aa</color> * " + actSave[4];
                            if (actSave.Count > 3 * MainControl.instance.BattleControl.enemies.Count)
                                textUIBack.text += "\n* " + actSave[6];
                            for (int i = 0; i < actSave.Count; i++)
                            {
                                actSave[i] += ';';
                            }

                            actSave = MainControl.instance.ChangeItemData(actSave, false, new List<string> { enemiesControllers[selectSon].name, enemiesControllers[selectSon].atk.ToString(), enemiesControllers[selectSon].def.ToString() });

                            for (int i = 0; i < actSave.Count; i++)
                            {
                                actSave[i] = actSave[i].Substring(0, actSave[i].Length - 1);
                            }

                            textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                            textUIBack.alignment = TextAlignmentOptions.TopLeft;
                        }
                        break;

                    case 3:
                    //ITEM: Jump 2
                        itemSelectController.myItemMax = MainControl.instance.FindMax(MainControl.instance.PlayerControl.myItems);
                        itemSelectController.Open();
                        selectLayer = 2;

                        if (MainControl.instance.PlayerControl.myItems[selectSon] < 10000)
                            UITextUpdate(UITextMode.Food, int.Parse(MainControl.instance.ItemIdGetData(MainControl.instance.PlayerControl.myItems[selectSon], "Auto")));
                        else
                            UITextUpdate(UITextMode.Food);
                        break;

                    case 4:
                    //MERCY: Selection of the enemy
                        LayerOneSet();
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            List<string> save = new List<string>();
                            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.BattleControl.mercySave, save, MainControl.instance.BattleControl.enemies[selectSon].name + "\\");
                            MainControl.instance.MaxToOneSon(save, actSave);

                            textUI.text = "<color=#00000000>aa</color> * " + actSave[0];
                            if (actSave.Count > MainControl.instance.BattleControl.enemies.Count)
                                textUI.text += "\n<color=#00000000>aa</color> * " + actSave[2];
                            if (actSave.Count > 4 * MainControl.instance.BattleControl.enemies.Count)
                                textUI.text += "\n<color=#00000000>aa</color> * " + actSave[4];
                        }
                        break;
                }
                break;

            case 2:
                switch (selectUI)
                {
                    case 2:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selectGrandSon - 2 >= 1)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selectGrandSon -= 2;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selectGrandSon + 2 <= actSave.Count / 2)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selectGrandSon += 2;
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) && selectGrandSon - 1 >= 1)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selectGrandSon--;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow) && selectGrandSon + 1 <= actSave.Count / 2)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
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
                        MainControl.instance.battlePlayerController.transform.position = new Vector3(playerPosX, playerPosY, MainControl.instance.battlePlayerController.transform.position.z);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                        {
                            selectLayer = 1;
                            selectGrandSon = 1;
                            textUI.text = "";
                            textUIBack.text = "";
                            for (int i = 0; i < MainControl.instance.BattleControl.enemies.Count; i++)
                            {
                                textUI.text += "<color=#00000000>aa*</color>* " + MainControl.instance.BattleControl.enemies[i].name + "\n";
                            }
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            switch (selectSon)
                            // Write the ACT trigger code here.
                            {
                                case 0:
                                //Monster 0
                                    switch (selectGrandSon)
                                    //Option
                                    {
                                        case 1:

                                            break;

                                        case 2:

                                            DebugLogger.Log(1);
                                            AudioController.instance.GetFx(3, MainControl.instance.AudioControl.fxClipBattle);

                                            break;

                                        case 3:

                                            break;

                                        case 4:

                                            break;
                                    }
                                    break;

                                case 1:
                                //Monster 1
                                    switch (selectGrandSon)
                                    //Options
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

                                case 2:
                                //Monster 2
                                    switch (selectGrandSon)
                                    //Options
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

                            textUIBack.text = "";
                            selectLayer = 3;
                            MainControl.instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                            Type(actSave[2 * (selectGrandSon + 1) - 3]);
                            SpriteChange();
                            itemSelectController.Close();
                        }

                        break;

                    case 3:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                        {
                            selectLayer = 0;
                            selectSon = 0;
                            if (!firstIn)
                                TurnTextLoad();
                            else
                                TurnTextLoad(true, firstInDiy);
                            itemSelectController.Close();

                            textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                            textUIBack.alignment = TextAlignmentOptions.TopLeft;
                            textUIBack.text = "";

                            UITextUpdate(UITextMode.Food);
                            break;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            selectLayer = 3;
                            MainControl.instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                            MainControl.instance.UseItem(typeWritter, textUI, selectSon + 1);
                            SpriteChange();
                            itemSelectController.Close();

                            textUIBack.rectTransform.anchoredPosition = new Vector2(10.75f, -3.3f);
                            textUIBack.alignment = TextAlignmentOptions.TopLeft;
                            textUIBack.text = "";

                            UITextUpdate(UITextMode.Food);
                            break;
                        }

                        //hpSpr.material.SetFloat("_Crop", 1);

                        string textUITextChanger1 = "";
                        string textUITextChanger2 = "";

                        string textUIDataChanger1 = "";
                        string textUIDataChanger2 = "";

                        int myItemMax = MainControl.instance.FindMax(MainControl.instance.PlayerControl.myItems);

                        if (myItemMax > 1)
                        {
                            textUITextChanger1 = "<color=#00000000>aa*</color>* " + MainControl.instance.ItemIdGetName(MainControl.instance.PlayerControl.myItems[selectSon + 1 - (selectGrandSon - 1)], "Auto", 0) + "\n";
                            textUIDataChanger1 = MainControl.instance.ItemIdGetData(MainControl.instance.PlayerControl.myItems[selectSon + 1 - (selectGrandSon - 1)], "Auto", true) + "\n";
                        }
                        if (myItemMax > 2)
                        {
                            textUITextChanger2 = "<color=#00000000>aa*</color>* " + MainControl.instance.ItemIdGetName(MainControl.instance.PlayerControl.myItems[selectSon + 2 - (selectGrandSon - 1)], "Auto", 0) + "\n";
                            textUIDataChanger2 = MainControl.instance.ItemIdGetData(MainControl.instance.PlayerControl.myItems[selectSon + 2 - (selectGrandSon - 1)], "Auto", true) + "\n";
                        }
                        int num = 8;

                        if (myItemMax >= 8)
                        {
                            itemSelectController.myItemSelect = selectSon;
                        }
                        else
                        //if (myItemMax < num)
                        {
                            if (myItemMax >= 6)
                            {
                                num = 8;
                            }
                            else if (myItemMax >= 4)
                            {
                                num = 7;
                            }
                            else if (myItemMax >= 2)
                            {
                                num = 6;
                            }
                            else if (myItemMax >= 1)
                            {
                                num = 5;
                            }
                            if (myItemMax % 2 == 0)
                            {
                                itemSelectController.myItemSelect = selectSon + (num - 1 - myItemMax);
                            }
                            else
                                itemSelectController.myItemSelect = selectSon + (num - myItemMax);
                        }
                        itemSelectController.myItemRealSelect = selectSon;
                        MainControl.instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - (selectGrandSon - 1) * 0.66f, MainControl.instance.battlePlayerController.transform.position.z);

                        textUI.text = "<color=#00000000>aa*</color>* " + MainControl.instance.ItemIdGetName(MainControl.instance.PlayerControl.myItems[selectSon - (selectGrandSon - 1)], "Auto", 0) + "\n" +
                                       textUITextChanger1 + textUITextChanger2;
                        textUIBack.text = MainControl.instance.ItemIdGetData(MainControl.instance.PlayerControl.myItems[selectSon - (selectGrandSon - 1)], "Auto", true) + "\n" + textUIDataChanger1 + textUIDataChanger2;

                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selectSon > 0)
                        {
                            if (selectGrandSon > 1)
                                selectGrandSon--;
                            itemSelectController.PressDown(true);
                            selectSon--;
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);

                            if (MainControl.instance.PlayerControl.myItems[selectSon] < 10000)
                                UITextUpdate(UITextMode.Food, int.Parse(MainControl.instance.ItemIdGetData(MainControl.instance.PlayerControl.myItems[selectSon], "Auto")));
                            else
                                UITextUpdate(UITextMode.Food);
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selectSon < myItemMax - 1)
                        {
                            if (selectGrandSon < 3)
                                selectGrandSon++;
                            itemSelectController.PressDown(false);
                            selectSon++;
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);

                            if (MainControl.instance.PlayerControl.myItems[selectSon] < 10000)
                                UITextUpdate(UITextMode.Food, int.Parse(MainControl.instance.ItemIdGetData(MainControl.instance.PlayerControl.myItems[selectSon], "Auto")));
                            else
                                UITextUpdate(UITextMode.Food);
                        }

                        hpUI.text = UIHPVoid(hpFood) + " / " + UIHPVoid(MainControl.instance.PlayerControl.hpMax);
                        break;

                    case 4:
                        MainControl.instance.battlePlayerController.transform.position = new Vector3(-5.175f, -0.96f - (selectGrandSon - 1) * 0.66f, MainControl.instance.battlePlayerController.transform.position.z);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                        {
                            selectLayer = 1;
                            selectGrandSon = 1;
                            textUI.text = "";
                            for (int i = 0; i < MainControl.instance.BattleControl.enemies.Count; i++)
                            {
                                textUI.text += "<color=#00000000>aa*</color>* " + MainControl.instance.BattleControl.enemies[i].name + "\n";
                            }
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            selectLayer = 3;
                            MainControl.instance.battlePlayerController.transform.position = (Vector3)(Vector2.one * 10000) + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                            if (actSave[2 * (selectGrandSon + 1) - 3] != "Null")
                                Type(actSave[2 * (selectGrandSon + 1) - 3]);
                            else
                            {
                                textUI.text = "";
                                MainControl.instance.battlePlayerController.transform.position = (Vector3)MainControl.instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                                OpenDialogBubble(MainControl.instance.BattleControl.turnDialogAsset[TurnController.instance.turn]);
                            }
                            SpriteChange();
                            itemSelectController.Close();
                        }

                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selectGrandSon - 1 >= 1)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selectGrandSon--;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selectGrandSon + 1 <= actSave.Count / 2)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selectGrandSon++;
                        }

                        break;
                }
                break;

            case 3:
                MainControl.instance.forceJumpLoadTurn = true;
                firstIn = false;

                if (((selectUI == 1) && !target.gameObject.activeSelf))
                {
                    if (MainControl.instance.battlePlayerController.transform.position != (Vector3)MainControl.instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z))
                    {
                        textUI.text = "";
                        MainControl.instance.battlePlayerController.transform.position = (Vector3)MainControl.instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                        OpenDialogBubble(MainControl.instance.BattleControl.turnDialogAsset[TurnController.instance.turn]);
                    }
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                {
                    MainControl.instance.battlePlayerController.collideCollider.enabled = true;
                    if (!isDialog)
                    {
                        if (selectUI != 1 && textUI.text == "")
                        {
                            OpenDialogBubble(MainControl.instance.BattleControl.turnDialogAsset[TurnController.instance.turn]);
                            MainControl.instance.battlePlayerController.transform.position = (Vector3)MainControl.instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                            break;
                        }
                        if (selectUI != 1 && !typeWritter.isTyping)
                        {
                            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                            {
                                textUI.text = "";
                                MainControl.instance.battlePlayerController.transform.position = (Vector3)MainControl.instance.battlePlayerController.sceneDrift + new Vector3(0, 0, MainControl.instance.battlePlayerController.transform.position.z);
                                OpenDialogBubble(MainControl.instance.BattleControl.turnDialogAsset[TurnController.instance.turn]);
                            }
                        }
                    }
                }

                break;
        }
    }

    private void OpenDialogBubble(string textAsset)
    {
        MainControl.instance.BattleControl.randomTurnDir = MainControl.instance.Get1Or_1();
        MainControl.instance.LoadItemData(actSave, textAsset);
        actSave = MainControl.instance.ChangeItemData(actSave, true, new List<string>());
        isDialog = true;
        numDialog = 0;
        //if (selectUI == 1)
        // KeepDialogBubble(); // KeepDialogBubble().
    }

    private void KeepDialogBubble()
    {
        List<string> save = new List<string>();
        MainControl.instance.MaxToOneSon(actSave[numDialog], save);
        for (int i = 0; i < enemiesControllers.Count; i++)
        {
            if (save[2] == enemiesControllers[i].name)
            {
                dialog.transform.SetParent(enemiesControllers[i].transform);
                break;
            }
        }

        dialog.size = MainControl.instance.StringVector2ToRealVector2(save[0], dialog.size);
        dialog.position = MainControl.instance.StringVector2ToRealVector2(save[1], dialog.position);

        dialog.isBackRight = Convert.ToBoolean(save[3]);
        dialog.backY = float.Parse(save[4]);
        dialog.typeWritter.TypeOpen(save[5], false, 0, 1, dialog.tmp);
        numDialog++;
        dialog.tmp.text = "";
        dialog.PositionChange();
    }

    private void TurnTextLoad(bool isDiy = false, int diy = 0)
    {
        if (TurnController.instance.turn != saveTurn || saveTurnText == "")
        {
            List<string> load = new List<string>();
            saveTurn = TurnController.instance.turn;
            if (isDiy)
            {
                load = TurnTextLoad(MainControl.instance.BattleControl.turnTextSave, diy);
                firstIn = false;
            }
            else
            {
                load = TurnTextLoad(MainControl.instance.BattleControl.turnTextSave, saveTurn);
            }

            saveTurnText = load[UnityEngine.Random.Range(0, load.Count)];
        }
        Type(saveTurnText);
    }

    private List<string> TurnTextLoad(List<string> TurnTextSave, int turn)
    {
        List<string> TurnTextSaveChanged = new List<string>();
        for (int i = 0; i < TurnTextSave.Count; i++)
        {
            if (TurnTextSave[i].Substring(0, turn.ToString().Length) == turn.ToString())
                TurnTextSaveChanged.Add(TurnTextSave[i].Substring(turn.ToString().Length + 1));
        }
        List<string> saves = new List<string>();
        MainControl.instance.MaxToOneSon(TurnTextSaveChanged, saves);
        return saves;
    }

    public enum UITextMode
    {
        None,
        Hit,
        Food
    }

    private Tween hpFoodTween;
    private int hpFood;

    /// <summary>
    /// Updated UI text and blood bars.
    /// </summary>
    public void UITextUpdate(UITextMode uiTextMode = 0, int foodNum = 0)
    {
        hpSpr.transform.localScale = new Vector3(0.525f * MainControl.instance.PlayerControl.hpMax, 8.5f);
        hpSpr.material.SetColor("_ColorUnder", hpColorUnder);
        hpSpr.material.SetColor("_ColorOn", hpColorOn);

        switch (uiTextMode)
        {
            case UITextMode.None:
                goto default;
            default:
                hpSpr.material.SetFloat("_Crop", (float)MainControl.instance.PlayerControl.hp / MainControl.instance.PlayerControl.hpMax);
                hpSpr.material.SetFloat("_Flash", (float)MainControl.instance.PlayerControl.hp / MainControl.instance.PlayerControl.hpMax);
                break;

            case UITextMode.Hit:
                hpSpr.material.DOKill();

                hpSpr.material.SetFloat("_IsFlashing", 0);
                hpSpr.material.SetColor("_ColorFlash", hpColorHit);
                hpSpr.material.SetFloat("_Crop", (float)MainControl.instance.PlayerControl.hp / MainControl.instance.PlayerControl.hpMax);
                hpSpr.material.DOFloat((float)MainControl.instance.PlayerControl.hp / MainControl.instance.PlayerControl.hpMax, "_Flash", 0.5f).SetEase(Ease.OutCirc);

                break;

            case UITextMode.Food:
                hpSpr.material.DOKill();

                hpSpr.material.SetFloat("_IsFlashing", 1);
                hpSpr.material.SetFloat("_Crop", (float)MainControl.instance.PlayerControl.hp / MainControl.instance.PlayerControl.hpMax);
                float addNum = MainControl.instance.PlayerControl.hp + foodNum;
                if (addNum > MainControl.instance.PlayerControl.hpMax)
                    addNum = MainControl.instance.PlayerControl.hpMax;
                hpSpr.material.DOFloat(addNum / MainControl.instance.PlayerControl.hpMax, "_Flash", 0.5f).SetEase(Ease.OutCirc);
                break;
        }

        hpUI.transform.localPosition = new Vector3(9.85f + 0.0265f * (MainControl.instance.PlayerControl.hpMax - 20), -5.825f);
        nameUI.text = MainControl.instance.PlayerControl.playerName + " lv<size=3><color=#00000000>0</size></color>" + MainControl.instance.PlayerControl.lv;

        if (uiTextMode != UITextMode.Food)
            hpUI.text = UIHPVoid(MainControl.instance.PlayerControl.hp) + " / " + UIHPVoid(MainControl.instance.PlayerControl.hpMax);
        else
        {
            hpFoodTween.Kill();
            int addNum = MainControl.instance.PlayerControl.hp + foodNum;
            if (addNum > MainControl.instance.PlayerControl.hpMax)
                addNum = MainControl.instance.PlayerControl.hpMax;
            hpFoodTween = DOTween.To(() => hpFood, x => hpFood = x, addNum, 0.5f);
        }
    }

    /// <summary>
    /// Fix hpUI showing 01 as 1)
    /// </summary>
    private string UIHPVoid(int i)
    {
        if (0 <= i && i < 10)
            return "0" + i;
        else
            return i.ToString();
    }
}
