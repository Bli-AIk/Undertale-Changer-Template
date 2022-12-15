using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
/// <summary>
/// Battle场景中的UI控制器
/// 也负责玩家回合的控制
/// </summary>
public class SelentUIController : MonoBehaviour
{
    TextMeshPro nameUI, hpUI, textUI, textUIBack;
    SpriteRenderer hpSpr, hpOnSpr;

    [Header("对话气泡载入数")]//载入actSave
    public int numDialog;
    public bool isDialog;

    [Header("暂存未使用的Sprite")]
    public List<Sprite> spriteUI;
    public List<SpriteRenderer> buttons;
    public List<Vector2> playerUIPos;

    [Header("四个按钮UI的选择 0开")]
    public int selentUI;
    [Header("层")]
    public int selentLayer;//0选择按钮 1选择名称 2Act选项/背包层 3执行层 进入敌方回合后归零
    [Header("子选择")]
    public int selentSon;
    public int selentGrandSon;//Item&Mercy:1 2 3三个位置 ACT:四个位置
    ItemSelentController itemSelentController;
    RoundController roundController;
    BattlePlayerController player;
    TypeWritter typeWritter;
    GameObject enemiesHpLine;
    [Header("暂存ACT选项以便调用")]
    public List<string> actSave;

    [Header("自动寻找战斗总控的怪物 需保证名称一致")]
    public List<EnemiesController> enemiesControllers;
    TargetController target;
    DialogBubbleBehaviour dialog;

    int saveRound = -1;
    string saveRoundText= "";
    // Start is called before the first frame update
    void Start()
    {
        target = transform.Find("Target").GetComponent<TargetController>();
        target.gameObject.SetActive(false);
        nameUI = transform.Find("Name UI").GetComponent<TextMeshPro>();
        hpUI = transform.Find("HP UI").GetComponent<TextMeshPro>();
        textUI = transform.Find("Text UI").GetComponent<TextMeshPro>();
        textUIBack = transform.Find("Text UI Back").GetComponent<TextMeshPro>();
        hpSpr = transform.Find("HP").GetComponent<SpriteRenderer>();
        hpOnSpr = transform.Find("HPOn").GetComponent<SpriteRenderer>();
        roundController = GameObject.Find("MainControl").GetComponent<RoundController>();
        player = GameObject.Find("Player").GetComponent<BattlePlayerController>();
        itemSelentController = transform.Find("ItemSelent").GetComponent<ItemSelentController>();
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
        selentUI = 1;
        RoundTextLoad();
        enemiesHpLine.SetActive(false);
    }
   
    // Update is called once per frame
    void Update()
    {
        if (MainControl.instance.OverwroldControl.isSetting || MainControl.instance.OverwroldControl.pause)
            return;

        UIText();
        if (roundController.isMyRound)
            MyRound();

        dialog.gameObject.SetActive(isDialog);

        if (isDialog)
        {
            if ((!dialog.typeWritter.isTyping && (MainControl.instance.KeyArrowToControl(KeyCode.Z))) || ((selentUI == 1 || typeWritter.endString == "") && numDialog == 0)) 
            {
                if (numDialog < actSave.Count)
                    KeepDialogBubble();
                else//敌方回合：开！
                {
                    isDialog = false;

                    roundController.OutYourRound();

                    itemSelentController.gameObject.SetActive(false);
                    actSave = new List<string>();
                    selentLayer = 4;
                }
            }
        }
    }
    /// <summary>
    /// UI打字 打字完成后不会强制控死文本
    /// </summary>
    void Type(string text)
    {
        typeWritter.TypeOpen(text, false, 0, 0);
    }
    /// <summary>
    /// 战术互换
    /// </summary>
    void SpriteChange()
    {
        Sprite sprSave = buttons[selentUI - 1].sprite;
        buttons[selentUI - 1].sprite = spriteUI[selentUI - 1];
        spriteUI[selentUI - 1] = sprSave;
    }
    
    /// <summary>
    /// selentUI=1时的设定
    /// 主要为选定怪物
    /// </summary>
    void LayerOneSet()
    {
        player.transform.position = new Vector3(-5.175f, -0.96f - selentSon * 0.66f);
        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selentSon < MainControl.instance.BattleControl.enemies.Count - 1)
        {
            selentSon++;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
        }
        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selentSon > 0)
        {
            selentSon--;
            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
        }
    }
    /// <summary>
    ///进我方回合
    /// </summary>
    public void InRound(int round)
    {
        selentLayer = 0;
        selentUI = 1;
        selentSon = 0;
        selentGrandSon = 0;
        SpriteChange();
        RoundTextLoad();
        itemSelentController.gameObject.SetActive(true);
        if (round >= 0)
            MainControl.instance.LoadRound(round);
    }

    /// <summary>
    /// 我的回合！抽卡)
    /// </summary>
    void MyRound()
    {
        switch (selentLayer)
        {
            case 0:
                textUI.text = typeWritter.endString;
                player.transform.position = playerUIPos[selentUI - 1];
                if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
                {
                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                    if (selentUI >= 1)
                    {
                        SpriteChange();
                        if (selentUI == 1)
                            selentUI = 4;
                        else
                            selentUI -= 1;
                        SpriteChange();
                    }
                }
                else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
                {
                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                    if (selentUI <= 4)
                    {
                        SpriteChange();
                        if (selentUI == 4)
                            selentUI = 1;
                        else
                            selentUI += 1;
                        SpriteChange();
                    }
                }
                if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                {
                    selentLayer = 1;
                    selentGrandSon = 1;
                    if (!(MainControl.instance.PlayerControl.myItems[0] == 0 && selentUI == 3))
                    {
                        AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                        typeWritter.TypeStop();
                        textUI.text = "";
                    }
                    if (selentUI != 3)
                        for (int i = 0; i < MainControl.instance.BattleControl.enemies.Count; i++)
                        {
                            textUI.text += "<color=#00000000>aa*</color>* " + MainControl.instance.BattleControl.enemies[i].name + "\n";
                        }
                    else                        
                        MainControl.instance.PlayerControl.myItems = MainControl.instance.ListOrderChanger(MainControl.instance.PlayerControl.myItems);
                    
                    if (MainControl.instance.PlayerControl.myItems[0] == 0 && selentUI == 3)
                        selentLayer = 0;
                    
                }
                    
                break;
            case 1:
                if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                {
                    selentLayer = 0;
                    selentSon = 0;
                    RoundTextLoad();
                    enemiesHpLine.SetActive(false);
                    break;
                }
                else if ((MainControl.instance.KeyArrowToControl(KeyCode.Z)) && selentUI != 3)
                {
                    if (selentUI != 1)
                        selentLayer = 2;
                    else
                    {
                        selentLayer = 3;
                        SpriteChange();
                    }


                    selentGrandSon = 1;
                    textUI.text = "";
                    AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                }
                switch (selentUI)
                {
                    case 1://FIGHT：选择敌人
                        enemiesHpLine.SetActive(true);
                        LayerOneSet();
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            enemiesHpLine.SetActive(false);
                            target.gameObject.SetActive(true);
                            target.selent = selentSon;
                            target.transform.Find("Move").transform.position = new Vector3(MainControl.instance.BattleControl.enemies[selentSon].transform.position.x, target.transform.Find("Move").transform.position.y);
                            target.hitMonster = enemiesControllers[selentSon];
                        }
                        break;
                    case 2://ACT：选择敌人
                        LayerOneSet();
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            List<string> save = new List<string>();
                            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.BattleControl.actSave, save, MainControl.instance.BattleControl.enemies[selentSon].name + "\\");
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

                            actSave = MainControl.instance.ChangeItemData(actSave, false, new List<string> { enemiesControllers[selentSon].name , enemiesControllers[selentSon].atk.ToString(), enemiesControllers[selentSon].def.ToString()});

                            for (int i = 0; i < actSave.Count; i++)
                            {
                                actSave[i] = actSave[i].Substring(0, actSave[i].Length - 1);
                            }
                        }
                        break;
                    case 3://ITEM：跳2
                        itemSelentController.myItemMax = MainControl.instance.FindMax(MainControl.instance.PlayerControl.myItems); ;
                        itemSelentController.Open();
                        selentLayer = 2;
                        break;
                    case 4://MERCY：选择敌人
                        LayerOneSet();
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            List<string> save = new List<string>();
                            MainControl.instance.ScreenMaxToOneSon(MainControl.instance.BattleControl.mercySave, save, MainControl.instance.BattleControl.enemies[selentSon].name + "\\");
                            MainControl.instance.MaxToOneSon(save, actSave);

                            textUI.text = "<color=#00000000>aa</color> * " + actSave[0];
                            if (actSave.Count > MainControl.instance.BattleControl.enemies.Count)
                                textUI.text += "\n<color=#00000000>aa</color> * " + actSave[2];
                            if (actSave.Count > 2 * MainControl.instance.BattleControl.enemies.Count)
                                textUI.text += "\n<color=#00000000>aa</color> * " + actSave[4];
                        }
                        break;
                }
                break;

            case 2:
                switch (selentUI)
                {
                    case 2:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                        {
                            selentLayer = 1;
                            selentGrandSon = 1;
                            textUI.text = "";
                            textUIBack.text = "";
                            for (int i = 0; i < MainControl.instance.BattleControl.enemies.Count; i++)
                            {
                                textUI.text += "<color=#00000000>aa*</color>* " + MainControl.instance.BattleControl.enemies[i].name + "\n";
                            }
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            textUIBack.text = "";
                            selentLayer = 3;
                            Type(actSave[2 * (selentGrandSon + 1) - 3]);
                            SpriteChange();
                            itemSelentController.Close();
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selentGrandSon - 2 >= 1)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selentGrandSon -= 2;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selentGrandSon + 2 <= actSave.Count / 2)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selentGrandSon += 2;
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) && selentGrandSon - 1 >= 1)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selentGrandSon--;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow) && selentGrandSon + 1 <= actSave.Count / 2)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selentGrandSon++;
                        }
                        
                        float playerPosX, playerPosY;
                        if (selentGrandSon % 2 == 0)
                        {
                            playerPosX = 0.25f;
                        }
                        else
                        {
                            playerPosX = -5.175f;
                        }
                        if (selentGrandSon < 3)
                        {
                            playerPosY = -0.96f - 0 * 0.66f;
                        }
                        else
                        {
                            playerPosY = -0.96f - 1 * 0.66f;
                        }
                        player.transform.position = new Vector3(playerPosX, playerPosY);


                        break;
                    case 3:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                        {
                            selentLayer = 0;
                            selentSon = 0;
                            RoundTextLoad();
                            itemSelentController.Close();
                            break;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            selentLayer = 3;
                            MainControl.instance.UseItem(typeWritter, selentSon + 1);
                            SpriteChange();
                            itemSelentController.Close();
                            break;
                        }

                        string textUITextChanger1 = "";
                        string textUITextChanger2 = "";
                        int myItemMax = MainControl.instance.FindMax(MainControl.instance.PlayerControl.myItems);
                    
                        if (myItemMax > 1)
                        {
                            textUITextChanger1 = "<color=#00000000>aa*</color>* " + MainControl.instance.ItemIdGet(MainControl.instance.PlayerControl.myItems[selentSon + 1 - (selentGrandSon - 1)], "Auto", 0) + "\n";
                        }
                        if (myItemMax > 2)
                        {
                            textUITextChanger2 = "<color=#00000000>aa*</color>* " + MainControl.instance.ItemIdGet(MainControl.instance.PlayerControl.myItems[selentSon + 2 - (selentGrandSon - 1)], "Auto", 0) + "\n";
                        }
                        int num = 8;

                        if (myItemMax >= 8)
                        {
                            itemSelentController.myItemSelent = selentSon;
                        }
                        else //if (myItemMax < num)
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
                                itemSelentController.myItemSelent = selentSon + (num - 1 - myItemMax);
                            }
                            else
                                itemSelentController.myItemSelent = selentSon + (num - myItemMax);
                        }
                        itemSelentController.myItemRealSelent = selentSon;
                        player.transform.position = new Vector3(-5.175f, -0.96f - (selentGrandSon - 1) * 0.66f);

                        textUI.text = "<color=#00000000>aa*</color>* " + MainControl.instance.ItemIdGet(MainControl.instance.PlayerControl.myItems[selentSon - (selentGrandSon - 1)], "Auto", 0) + "\n" +
                                       textUITextChanger1 + textUITextChanger2;

                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selentSon > 0)
                        {
                            if (selentGrandSon > 1)
                                selentGrandSon--;
                            itemSelentController.PressDown(true);
                            selentSon--;
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selentSon < myItemMax - 1)
                        {
                            if (selentGrandSon <3)
                                selentGrandSon++;
                            itemSelentController.PressDown(false);
                            selentSon++;
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        }
                        break;

                    case 4:
                        player.transform.position = new Vector3(-5.175f, -0.96f - (selentGrandSon - 1) * 0.66f);
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                        {
                            selentLayer = 1;
                            selentGrandSon = 1;
                            textUI.text = "";
                            for (int i = 0; i < MainControl.instance.BattleControl.enemies.Count; i++)
                            {
                                textUI.text += "<color=#00000000>aa*</color>* " + MainControl.instance.BattleControl.enemies[i].name + "\n";
                            }
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            selentLayer = 3;
                            if (actSave[2 * (selentGrandSon + 1) - 3] != "Null")
                                Type(actSave[2 * (selentGrandSon + 1) - 3]);
                            
                                SpriteChange();
                            itemSelentController.Close();
                        }


                        if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) && selentGrandSon - 1 >= 1)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selentGrandSon--;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow) && selentGrandSon + 1 <= actSave.Count / 2)
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            selentGrandSon++;
                        }

                        break;
                }
                break;
            case 3:
                if (!isDialog)
                {
                    if (selentUI != 1 && typeWritter.endString == "")
                    {
                        OpenDialogBubble(MainControl.instance.BattleControl.roundDialogAsset[roundController.round]);
                        player.transform.position = new Vector2(0, -1.5f);
                        break;
                    }
                    if (selentUI != 1 && !typeWritter.isTyping)
                    {
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            typeWritter.endString = "";
                            player.transform.position = new Vector2(0, -1.5f);
                            OpenDialogBubble(MainControl.instance.BattleControl.roundDialogAsset[roundController.round]);

                        }
                    }
                    else if (selentUI == 1 && !target.gameObject.activeSelf)
                    {
                        if (player.transform.position != new Vector3(0, -1.5f))
                        {
                            player.transform.position = new Vector3(0, -1.5f);
                            OpenDialogBubble(MainControl.instance.BattleControl.roundDialogAsset[roundController.round]);
                        }
                    }
                    else
                    {
                        player.transform.position = Vector2.one * 10000;
                    }

                }



                textUI.text = typeWritter.endString;
                break;
        }
        
    }
    void OpenDialogBubble(string textAsset)
    {
        MainControl.instance.LoadItemData(actSave, textAsset);
        actSave = MainControl.instance.ChangeItemData(actSave, true, new List<string>());
        isDialog = true;
        numDialog = 0;
        //if (selentUI == 1)
        //    KeepDialogBubble();
    }
    void KeepDialogBubble()
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
        dialog.typeWritter.TypeOpen(save[5], false, 0, 1);
        numDialog++;
        dialog.PositionChange();
    }
    void RoundTextLoad()
    {
        if (roundController.round != saveRound || saveRoundText == "")
        {
            saveRound = roundController.round;
            List<string> load = RoundTextLoad(MainControl.instance.BattleControl.roundTextSave, saveRound);
            saveRoundText = load[UnityEngine.Random.Range(0, load.Count)];
        }
        Type(saveRoundText);
    }

    List<string> RoundTextLoad(List<string> RoundTextSave, int round)
    {
        List<string> RoundTextSaveChanged = new List<string>();
        for (int i = 0; i < RoundTextSave.Count; i++)
        {
            if (RoundTextSave[i].Substring(0, round.ToString().Length) == round.ToString())
                RoundTextSaveChanged.Add(RoundTextSave[i].Substring(round.ToString().Length + 1));
        }
        List<string> saves = new List<string>();
        MainControl.instance.MaxToOneSon(RoundTextSaveChanged, saves);
        return saves;
    }



    void UIText()
    {
        hpSpr.transform.localScale = new Vector3(0.525f * MainControl.instance.PlayerControl.hpMax, 8.5f);
        hpOnSpr.transform.localScale = new Vector3(0.525f * MainControl.instance.PlayerControl.hp, 8.5f);
        hpUI.transform.localPosition = new Vector3(9.85f + 0.0265f * (MainControl.instance.PlayerControl.hpMax - 20), -5.825f);
        nameUI.text = MainControl.instance.PlayerControl.playerName + " lv<size=3><color=#00000000>0</size></color>" + MainControl.instance.PlayerControl.lv;
        hpUI.text = UIHPVoid(MainControl.instance.PlayerControl.hp) + " / " + UIHPVoid(MainControl.instance.PlayerControl.hpMax);
    }
    /// <summary>
    /// 解决hpUI把01显示成1的问题)
    /// </summary>
    string UIHPVoid(int i)
    {
        if (0 <= i && i < 10)
            return "0" + i;
        else
            return i.ToString();
    }
}

