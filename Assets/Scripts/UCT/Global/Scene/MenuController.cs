using System;
using System.Collections.Generic;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UnityEngine;

namespace UCT.Global.Scene
{
    /// <summary>
    /// 控制Menu，sodayo)
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("玩家名-LV-时间-位置-具体选项-底部字")]
        public List<TextMeshPro> tmps;

        private int select, selectMax = 5;
        public int layer;
        private bool setData;

        public int saveNumber;

        private void Awake()
        {
            MainControl.Instance.OverworldControl.playerScenePos = new Vector3(-0.5f, -1);

            for (int i = 0; i < transform.childCount; i++)
            {
                tmps.Add(transform.GetChild(i).GetComponent<TextMeshPro>());
            }
        }

        private void Start()
        {
            setData = false;
            layer = 0;
            if (MainControl.Instance.dataNumber < 0)
                MainControl.Instance.dataNumber = 0;
            MainControl.Instance.SetPlayerControl(SaveController.LoadData("Data" + MainControl.Instance.dataNumber));
            saveNumber = MainControl.Instance.dataNumber;
            LoadLayer0();
        }

        private void LoadLayer0()
        {
            PlayerControl playerControl = SaveController.LoadData("Data" + saveNumber);
            tmps[0].text = playerControl.playerName;
            tmps[1].text = "LV " + playerControl.lv;
            //tmps[2]在update内设置
            tmps[3].text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, playerControl.saveScene);

            Flash();

            tmps[5].text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "MenuUnder") + Application.version;
        }

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;

            if (MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow))
            {
                select--;
            }
            else if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow))
            {
                select -= 2;
            }
            if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow))
            {
                select++;
            }
            else if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
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
            if (setData && select == 2 && (saveNumber == 0))
            {
                if (select % 2 == 0)
                    select = 3;
                else select = 4;
            }
            if (layer == 0)
            {
                if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow) || MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow)
                                                                            || MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow) || MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow))
                {
                    Flash();
                }
                if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                {
                    if (!setData)
                        switch (select)
                        {
                            case 0:
                                MainControl.Instance.OutBlack(MainControl.Instance.PlayerControl.saveScene, Color.black, true);
                                break;

                            case 1:
                                MainControl.Instance.OutBlack("Rename", Color.black, true);
                                break;

                            case 2:
                                CanvasController.instance.InSetting();
                                break;

                            case 3:
                                CanvasController.instance.settingLevel = 2;
                                goto case 2;
                            case 4:
                                setData = true;
                                saveNumber = MainControl.Instance.dataNumber;
                                if (0 != (SaveController.GetDataNumber() - 1))
                                    select = 5;
                                Flash();
                                AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
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
                                AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                if (saveNumber > 0)
                                    saveNumber--;
                                select = 3;
                                LoadLayer0();
                                break;

                            case 3:
                                AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                if (saveNumber == (SaveController.GetDataNumber() - 1))//新建
                                {
                                    saveNumber++;
                                    MainControl.Instance.dataNumber = saveNumber;
                                    SaveController.SaveData(MainControl.Instance.PlayerControl, "Data" + MainControl.Instance.dataNumber);
                                    MainControl.Instance.SetPlayerControl(ScriptableObject.CreateInstance<PlayerControl>());
                                    MainControl.Instance.PlayerControl.playerName = "";
                                    MainControl.Instance.OutBlack("Rename", Color.black);
                                }
                                else//下页
                                {
                                    saveNumber++;
                                    LoadLayer0();
                                }
                                break;

                            case 4:
                                if (SaveController.GetDataNumber() - 1 <= 0)
                                    goto case 5;
                                SaveController.DeleteData("Data" + saveNumber);
                                if (saveNumber > (SaveController.GetDataNumber() - 1))
                                    saveNumber = SaveController.GetDataNumber() - 1;
                                LoadLayer0();
                                break;

                            case 5:
                                if (MainControl.Instance.dataNumber == saveNumber)
                                {
                                    setData = false;
                                    Flash();
                                    AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                    break;
                                }
                                else
                                {
                                    MainControl.Instance.dataNumber = saveNumber;
                                    AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                    UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
                                    break;
                                }
                        }
                }
                else if (MainControl.Instance.KeyArrowToControl(KeyCode.X) && setData)
                {
                    setData = false;
                    Flash();
                    AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                }
            }
        }

        private void Flash()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < 6; i++)
            {
                if (setData && i == 2 && (saveNumber == 0))
                    list.Add("<color=grey>");
                else if (i != select)
                    list.Add("");
                else
                    list.Add("<color=yellow>");
            }
            if (!setData)
                tmps[4].text = list[0] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu0") + "</color> "
                               + list[1] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu1") + "</color>\n"
                               + list[2] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu2") + "</color> "
                               + list[3] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu3") + "</color>\n"
                               + list[4] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu4") + "</color> "
                               + list[5] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu5") + "</color>";
            else
                tmps[4].text = list[0] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu12") + "</color> "
                               + list[1] + "Data" + saveNumber + "</color>\n"
                               + list[2] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu6") + "</color> "
                               + list[3] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, saveNumber == (SaveController.GetDataNumber() - 1) ? "Menu10" : "Menu7") + "</color>\n"
                               + list[4] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, 0 == (SaveController.GetDataNumber() - 1) ? "Menu8" : "Menu11") + "</color> "
                               + list[5] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu9") + "</color>";
        }

        private void FixedUpdate()
        {
            tmps[2].text = MainControl.Instance.GetRealTime((int)MainControl.Instance.PlayerControl.gameTime);
        }
    }
}