using System;
using System.Collections.Generic;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Global.Scene
{
    /// <summary>
    /// 控制Menu，sodayo)
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        [Header("玩家名-LV-时间-位置-具体选项-底部字")]
        public List<TextMeshPro> tmps;

        private int _select, _selectMax = 5;
        public int layer;
        private bool _setData;

        public int saveNumber;

        private void Awake()
        {
            MainControl.Instance.OverworldControl.playerScenePos = new Vector3(-0.5f, -1);

            for (var i = 0; i < transform.childCount; i++)
            {
                tmps.Add(transform.GetChild(i).GetComponent<TextMeshPro>());
            }
        }

        private void Start()
        {
            _setData = false;
            layer = 0;
            if (MainControl.Instance.dataNumber < 0)
                MainControl.Instance.dataNumber = 0;
            MainControl.Instance.SetPlayerControl(SaveController.LoadData("Data" + MainControl.Instance.dataNumber));
            saveNumber = MainControl.Instance.dataNumber;
            LoadLayer0();
        }

        private void LoadLayer0()
        {
            var playerControl = SaveController.LoadData("Data" + saveNumber);
            tmps[0].text = playerControl.playerName;
            tmps[1].text = "LV " + playerControl.lv;
            //tmps[2]在update内设置
            tmps[3].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, playerControl.saveScene);

            Flash();

            tmps[5].text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "MenuUnder") + Application.version;
        }

        private void Update()
        {
            if (MainControl.Instance.OverworldControl.isSetting || MainControl.Instance.OverworldControl.pause)
                return;

            if (MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow))
            {
                _select--;
            }
            else if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow))
            {
                _select -= 2;
            }
            if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow))
            {
                _select++;
            }
            else if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
            {
                _select += 2;
            }
            if (_select < 0 + 2 * Convert.ToInt32(_setData))
            {
                if (_select % 2 != 0)
                    _select = _selectMax;
                else
                    _select = _selectMax - 1;
            }
            if (_select > _selectMax)
            {
                if (_select % 2 == 0)
                    _select = 0 + 2 * Convert.ToInt32(_setData);
                else _select = 1 + 2 * Convert.ToInt32(_setData);
            }
            if (_setData && _select == 2 && (saveNumber == 0))
            {
                if (_select % 2 == 0)
                    _select = 3;
                else _select = 4;
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
                    if (!_setData)
                        switch (_select)
                        {
                            case 0:
                                MainControl.Instance.OutBlack(MainControl.Instance.PlayerControl.saveScene, Color.black, true);
                                break;

                            case 1:
                                MainControl.Instance.OutBlack("Rename", Color.black, true);
                                break;

                            case 2:
                                CanvasController.Instance.InSetting();
                                break;

                            case 3:
                                CanvasController.Instance.settingLevel = 2;
                                goto case 2;
                            case 4:
                                _setData = true;
                                saveNumber = MainControl.Instance.dataNumber;
                                if (0 != (SaveController.GetDataNumber() - 1))
                                    _select = 5;
                                Flash();
                                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                break;

                            case 5:
                                Application.Quit();
                                break;

                            default:
                                goto case 5;
                        }
                    else
                        switch (_select)
                        {
                            case 2:
                                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                if (saveNumber > 0)
                                    saveNumber--;
                                _select = 3;
                                LoadLayer0();
                                break;

                            case 3:
                                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
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
                                    _setData = false;
                                    Flash();
                                    AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                    break;
                                }

                                MainControl.Instance.dataNumber = saveNumber;
                                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                                break;
                        }
                }
                else if (MainControl.Instance.KeyArrowToControl(KeyCode.X) && _setData)
                {
                    _setData = false;
                    Flash();
                    AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                }
            }
        }

        private void Flash()
        {
            var list = new List<string>();
            for (var i = 0; i < 6; i++)
            {
                if (_setData && i == 2 && (saveNumber == 0))
                    list.Add("<color=grey>");
                else if (i != _select)
                    list.Add("");
                else
                    list.Add("<color=yellow>");
            }
            if (!_setData)
                tmps[4].text = list[0] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu0") + "</color> "
                               + list[1] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu1") + "</color>\n"
                               + list[2] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu2") + "</color> "
                               + list[3] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu3") + "</color>\n"
                               + list[4] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu4") + "</color> "
                               + list[5] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu5") + "</color>";
            else
                tmps[4].text = list[0] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu12") + "</color> "
                               + list[1] + "Data" + saveNumber + "</color>\n"
                               + list[2] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu6") + "</color> "
                               + list[3] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, saveNumber == (SaveController.GetDataNumber() - 1) ? "Menu10" : "Menu7") + "</color>\n"
                               + list[4] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, 0 == (SaveController.GetDataNumber() - 1) ? "Menu8" : "Menu11") + "</color> "
                               + list[5] + TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.sceneTextsSave, "Menu9") + "</color>";
        }

        private void FixedUpdate()
        {
            tmps[2].text = TextProcessingService.GetRealTime((int)MainControl.Instance.PlayerControl.gameTime);
        }
    }
}