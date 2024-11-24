using System;
using System.Collections.Generic;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Global.Scene
{
    /// <summary>
    ///     控制Menu场景
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        private const int SelectMax = 5;

        public int saveNumber;
        private OverworldControl _overworldControl;

        private int _select;
        private bool _setData;

        private TextMeshPro _textName,
            _textLv,
            _textTime,
            _textPosition,
            _textOptionsLeft,
            _textOptionsRight,
            _textMessage;

        private void Awake()
        {
            AssignReferences();
        }

        private void Start()
        {
            _setData = false;
            _overworldControl = MainControl.Instance.overworldControl;
            _overworldControl.playerScenePos = new Vector3(-0.5f, -1);
            if (MainControl.Instance.saveDataId < 0)
                MainControl.Instance.saveDataId = 0;
            MainControl.Instance.playerControl =
                DataHandlerService.SetPlayerControl(SaveController.LoadData("Data" + MainControl.Instance.saveDataId));
            saveNumber = MainControl.Instance.saveDataId;
            LoadLayer0();
        }

        private void Update()
        {
            if (!_textOptionsLeft.enabled)
                _textOptionsLeft.enabled = true;
            if (!_textOptionsRight.enabled)
                _textOptionsRight.enabled = true;


            if (_overworldControl.isSetting || _overworldControl.pause)
                return;

            if (InputService.GetKeyDown(KeyCode.LeftArrow))
                _select--;
            else if (InputService.GetKeyDown(KeyCode.UpArrow)) _select -= 2;
            if (InputService.GetKeyDown(KeyCode.RightArrow))
                _select++;
            else if (InputService.GetKeyDown(KeyCode.DownArrow)) _select += 2;
            if (_select < 0 + 2 * Convert.ToInt32(_setData))
            {
                if (_select % 2 != 0)
                    _select = SelectMax;
                else
                    _select = SelectMax - 1;
            }

            if (_select > SelectMax)
            {
                if (_select % 2 == 0)
                    _select = 0 + 2 * Convert.ToInt32(_setData);
                else _select = 1 + 2 * Convert.ToInt32(_setData);
            }

            if (_setData && _select == 2 && saveNumber == 0) _select = _select % 2 == 0 ? 3 : 4;

            if (InputService.GetKeyDown(KeyCode.UpArrow) ||
                InputService.GetKeyDown(KeyCode.DownArrow) ||
                InputService.GetKeyDown(KeyCode.LeftArrow) ||
                InputService.GetKeyDown(KeyCode.RightArrow))
                Flash();

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                if (!_setData)
                    switch (_select)
                    {
                        case 0:
                            GameUtilityService.FadeOutAndSwitchScene(MainControl.Instance.playerControl.saveScene,
                                Color.black, true);
                            break;

                        case 1:
                            GameUtilityService.FadeOutAndSwitchScene("Rename", Color.black, true);
                            break;

                        case 2:
                            SettingsController.Instance.OpenSetting();
                            break;

                        case 3:
                            SettingsController.Instance.SetSettingsLayer(SettingsLayerEnum
                                .LanguagePacksConfigLayer);
                            goto case 2;
                        case 4:
                            _setData = true;
                            saveNumber = MainControl.Instance.saveDataId;
                            if (0 != SaveController.GetDataNumber() - 1)
                                _select = 5;
                            Flash();
                            _textOptionsLeft.enabled = false;
                            _textOptionsRight.enabled = false;
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
                            if (saveNumber == SaveController.GetDataNumber() - 1) //新建
                            {
                                saveNumber++;
                                MainControl.Instance.saveDataId = saveNumber;
                                SaveController.SaveData(MainControl.Instance.playerControl,
                                    "Data" + MainControl.Instance.saveDataId);
                                MainControl.Instance.playerControl =
                                    DataHandlerService.SetPlayerControl(
                                        ScriptableObject.CreateInstance<PlayerControl>());
                                MainControl.Instance.playerControl.playerName = "";
                                GameUtilityService.FadeOutAndSwitchScene("Rename", Color.black);
                            }
                            else //下页
                            {
                                saveNumber++;
                                LoadLayer0();
                            }

                            break;

                        case 4:
                            if (SaveController.GetDataNumber() - 1 <= 0)
                                goto case 5;
                            SaveController.DeleteData("Data" + saveNumber);
                            if (saveNumber > SaveController.GetDataNumber() - 1)
                                saveNumber = SaveController.GetDataNumber() - 1;
                            LoadLayer0();
                            break;

                        case 5:
                            if (MainControl.Instance.saveDataId == saveNumber)
                            {
                                _setData = false;
                                Flash();
                                _textOptionsLeft.enabled = false;
                                _textOptionsRight.enabled = false;
                                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                break;
                            }

                            MainControl.Instance.saveDataId = saveNumber;
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                            break;
                    }
            }
            else if (InputService.GetKeyDown(KeyCode.X) && _setData)
            {
                _setData = false;
                Flash();
                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
            }
        }

        private void FixedUpdate()
        {
            _textTime.text = TextProcessingService.GetRealTime((int)MainControl.Instance.playerControl.gameTime);
        }

        private void AssignReferences()
        {
            _textName = transform.Find("TextName").GetComponent<TextMeshPro>();
            _textLv = transform.Find("TextLV").GetComponent<TextMeshPro>();
            _textTime = transform.Find("TextTime").GetComponent<TextMeshPro>();
            _textPosition = transform.Find("TextPosition").GetComponent<TextMeshPro>();
            _textOptionsLeft = transform.Find("TextOptionsLeft").GetComponent<TextMeshPro>();
            _textOptionsRight = transform.Find("TextOptionsRight").GetComponent<TextMeshPro>();
            _textMessage = transform.Find("TextMessage").GetComponent<TextMeshPro>();
        }

        private void LoadLayer0()
        {
            var playerControl = SaveController.LoadData("Data" + saveNumber);
            _textName.text = playerControl.playerName;
            _textLv.text = "LV " + playerControl.lv;
            _textPosition.text =
                TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                    playerControl.saveScene);
            Flash();
            _textMessage.text =
                TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave, "MenuUnder") +
                Application.version;
        }

        private void Flash()
        {
            var list = new List<string>();
            for (var i = 0; i < 6; i++)
                if (_setData && i == 2 && saveNumber == 0)
                    list.Add("<color=grey>");
                else if (i != _select)
                    list.Add("<color=white>");
                else
                    list.Add("<color=yellow>");

            if (!_setData)
            {
                _textOptionsLeft.text =
                    list[0] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu0") + "</color>\n" +
                    list[2] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu2") + "</color>\n" +
                    list[4] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu4") + "</color>";

                _textOptionsRight.text =
                    list[1] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu1") + "</color>\n" +
                    list[3] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu3") + "</color>\n" +
                    list[5] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu5") + "</color>";
            }
            else
            {
                _textOptionsLeft.text =
                    list[0] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu12") + "</color>\n" +
                    list[2] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu6") + "</color>\n" +
                    list[4] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        0 == SaveController.GetDataNumber() - 1 ? "Menu8" : "Menu11") + "</color>";

                _textOptionsRight.text =
                    list[1] + "Data" + saveNumber + "</color>\n" +
                    list[3] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        saveNumber == SaveController.GetDataNumber() - 1 ? "Menu10" : "Menu7") + "</color>\n" +
                    list[5] + TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.sceneTextsSave,
                        "Menu9") + "</color>";
            }
        }
    }
}