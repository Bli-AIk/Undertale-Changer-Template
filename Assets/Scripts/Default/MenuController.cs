using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Control Menu, sodayo)
/// </summary>
public class MenuController : MonoBehaviour
{
    [Header("Player Name - LV - Time - Location - Specific Options - Bottom Word")]
    public List<TextMeshPro> tmps;

    private int select, selectMax = 5;
    public int layer;
    private bool setData;

    public int saveNum;

    private void Awake()
    {
        MainControl.instance.OverworldControl.playerScenePos = new Vector3(-0.5f, -1);

        for (int i = 0; i < transform.childCount; i++)
        {
            tmps.Add(transform.GetChild(i).GetComponent<TextMeshPro>());
        }
    }

    private void Start()
    {
        setData = false;
        layer = 0;
        if (MainControl.instance.dataNum < 0)
            MainControl.instance.dataNum = 0;
        MainControl.instance.SetPlayerControl(SaveController.LoadData("Data" + MainControl.instance.dataNum));
        saveNum = MainControl.instance.dataNum;
        LoadLayer0();
    }

    private void LoadLayer0()
    {
        PlayerControl playerControl = SaveController.LoadData("Data" + saveNum);
        tmps[0].text = playerControl.playerName;
        tmps[1].text = "LV " + playerControl.lv;
        //tmps[2] set in update
        tmps[3].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, playerControl.saveScene);

        Flash();

        tmps[5].text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "MenuUnder") + Application.version;
    }

    private void Update()
    {
        if (MainControl.instance.OverworldControl.isSetting || MainControl.instance.OverworldControl.pause)
            return;

        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow))
        {
            select--;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
        {
            select -= 2;
        }
        if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
        {
            select++;
        }
        else if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
        {
            select += 2;
        }
        if (select < 0 + 2 * Convert.ToInt32(setData))
        {
            if (select % 2 != 0)
                select = selectMax;
            else
                select = selectMax - 1;
        }
        if (select > selectMax)
        {
            if (select % 2 == 0)
                select = 0 + 2 * Convert.ToInt32(setData);
            else select = 1 + 2 * Convert.ToInt32(setData);
        }
        if (setData && select == 2 && (saveNum == 0))
        {
            if (select % 2 == 0)
                select = 3;
            else select = 4;
        }
        if (layer == 0)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow)
            || MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow) || MainControl.instance.KeyArrowToControl(KeyCode.RightArrow))
            {
                Flash();
            }
            if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
            {
                if (!setData)
                    switch (select)
                    {
                        case 0:
                            MainControl.instance.OutBlack(MainControl.instance.PlayerControl.saveScene, Color.black, true);
                            break;

                        case 1:
                            MainControl.instance.OutBlack("Rename", Color.black, true);
                            break;

                        case 2:
                            CanvasController.instance.InSetting();
                            break;

                        case 3:
                            CanvasController.instance.settingLevel = 2;
                            goto case 2;
                        case 4:
                            setData = true;
                            saveNum = MainControl.instance.dataNum;
                            if (0 != (SaveController.GetDataNum() - 1))
                                select = 5;
                            Flash();
                            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                            break;

                        case 5:
                            Application.Quit();
                            break;

                        default:
                            goto case 5;
                    }
                else
                    switch (select)
                    {
                        case 2:
                            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                            if (saveNum > 0)
                                saveNum--;
                            select = 3;
                            LoadLayer0();
                            break;

                        case 3:
                            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                            if (saveNum == (SaveController.GetDataNum() - 1))
                            //New
                            {
                                saveNum++;
                                MainControl.instance.dataNum = saveNum;
                                SaveController.SaveData(MainControl.instance.PlayerControl, "Data" + MainControl.instance.dataNum);
                                MainControl.instance.SetPlayerControl(ScriptableObject.CreateInstance<PlayerControl>());
                                MainControl.instance.PlayerControl.playerName = "";
                                MainControl.instance.OutBlack("Rename", Color.black);
                            }
                            else
                            //Next page
                            {
                                saveNum++;
                                LoadLayer0();
                            }
                            break;

                        case 4:
                            if (SaveController.GetDataNum() - 1 <= 0)
                                goto case 5;
                            SaveController.DeleteData("Data" + saveNum);
                            if (saveNum > (SaveController.GetDataNum() - 1))
                                saveNum = SaveController.GetDataNum() - 1;
                            LoadLayer0();
                            break;

                        case 5:
                            if (MainControl.instance.dataNum == saveNum)
                            {
                                setData = false;
                                Flash();
                                AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                                break;
                            }
                            else
                            {
                                MainControl.instance.dataNum = saveNum;
                                AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                                break;
                            }
                    }
            }
            else if (MainControl.instance.KeyArrowToControl(KeyCode.X) && setData)
            {
                setData = false;
                Flash();
                AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
            }
        }
    }

    private void Flash()
    {
        List<string> list = new List<string>();
        for (int i = 0; i < 6; i++)
        {
            if (setData && i == 2 && (saveNum == 0))
                list.Add("<color=grey>");
            else if (i != select)
                list.Add("");
            else
                list.Add("<color=yellow>");
        }
        if (!setData)
            tmps[4].text = list[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu0") + "</color> "
                     + list[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu1") + "</color>\n"
                     + list[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu2") + "</color> "
                     + list[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu3") + "</color>\n"
                     + list[4] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu4") + "</color> "
                     + list[5] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu5") + "</color>";
        else
            tmps[4].text = list[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu12") + "</color> "
                     + list[1] + "Data" + saveNum + "</color>\n"
                     + list[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu6") + "</color> "
                     + list[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, saveNum == (SaveController.GetDataNum() - 1) ? "Menu10" : "Menu7") + "</color>\n"
                     + list[4] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, 0 == (SaveController.GetDataNum() - 1) ? "Menu8" : "Menu11") + "</color> "
                     + list[5] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.sceneTextsSave, "Menu9") + "</color>";
    }

    private void FixedUpdate()
    {
        tmps[2].text = MainControl.instance.GetRealTime((int)MainControl.instance.PlayerControl.gameTime);
    }
}
