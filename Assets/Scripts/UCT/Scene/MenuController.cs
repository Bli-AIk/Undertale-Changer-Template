using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UCT.Audio;
using UCT.Control;
using UCT.Core;
using UCT.Service;
using UCT.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Scene
{
    /// <summary>
    ///     控制Menu场景
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        private const int SelectMax = 5;

        public int saveNumber;

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

            if (MainControl.Instance.saveDataId < 0)
            {
                MainControl.Instance.saveDataId = 0;
            }

            MainControl.Instance.playerControl =
                DataHandlerService.SetPlayerControl(SaveController.LoadData("Data" + MainControl.Instance.saveDataId));
            saveNumber = MainControl.Instance.saveDataId;
            LoadLayer0();
        }

        private void Update()
        {
            EnableTextOptions();

            if (GameUtilityService.IsGamePausedOrSetting())
            {
                return;
            }

            UpdateInput();

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                if (!_setData)
                {
                    ExecuteMenuPage();
                }
                else
                {
                    ExecuteDataPage();
                }
            }
            else if (InputService.GetKeyDown(KeyCode.X))
            {
                CancelSetData();
            }
        }

        private void FixedUpdate()
        {
            _textTime.text = TextProcessingService.GetRealTime((int)MainControl.Instance.playerControl.gameTime);
        }

        private void CancelSetData()
        {
            if (!_setData)
            {
                return;
            }

            _setData = false;
            Flash();
            AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
        }

        private void ExecuteDataPage()
        {
            switch (_select)
            {
                case 2:
                {
                    AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
                    DataPageUp();
                    break;
                }

                case 3:
                {
                    AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
                    if (saveNumber == SaveController.GetDataNumber() - 1) //新建
                    {
                        DataCreate();
                    }
                    else //下页
                    {
                        DataPageDown();
                    }

                    break;
                }

                case 4:
                {
                    if (SaveController.GetDataNumber() - 1 <= 0)
                    {
                        DataConfirm();
                        break;
                    }

                    DataCancel();
                    break;
                }

                case 5:
                {
                    DataConfirm();
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException($"Unexpected Select value: {_select}");
                }
            }
        }

        private void ExecuteMenuPage()
        {
            switch (_select)
            {
                case 0:
                {
                    LoadSaveScene();
                    break;
                }

                case 1:
                {
                    LoadRenameScene();
                    break;
                }

                case 2:
                {
                    SettingsController.Instance.OpenSetting();
                    break;
                }

                case 3:
                {
                    SettingsController.Instance.OpenSetting("SettingLanguagePackageLayer");
                    break;
                }
                case 4:
                {
                    OpenDataPage();
                    break;
                }

                case 5:
                {
                    Application.Quit();
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException($"Unexpected Select value: {_select}");
                }
            }
        }

        private void DataConfirm()
        {
            if (MainControl.Instance.saveDataId == saveNumber)
            {
                _setData = false;
                Flash();
                _textOptionsLeft.enabled = false;
                _textOptionsRight.enabled = false;
                AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
                return;
            }

            MainControl.Instance.saveDataId = saveNumber;
            AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void DataCancel()
        {
            SaveController.DeleteData("Data" + saveNumber);
            if (saveNumber > SaveController.GetDataNumber() - 1)
            {
                saveNumber = SaveController.GetDataNumber() - 1;
            }

            LoadLayer0();
        }

        private void DataCreate()
        {
            saveNumber++;
            MainControl.Instance.saveDataId = saveNumber;
            SaveController.SaveData(MainControl.Instance.playerControl,
                "Data" + MainControl.Instance.saveDataId);
            MainControl.Instance.playerControl =
                DataHandlerService.SetPlayerControl(ScriptableObject.CreateInstance<PlayerControl>());
            MainControl.Instance.playerControl.playerName = "";
            GameUtilityService.FadeOutAndSwitchScene("Rename", Color.black);
        }

        private void DataPageDown()
        {
            saveNumber++;
            LoadLayer0();
        }

        private void DataPageUp()
        {
            if (saveNumber > 0)
            {
                saveNumber--;
            }

            _select = 3;
            LoadLayer0();
        }

        private void OpenDataPage()
        {
            _setData = true;
            saveNumber = MainControl.Instance.saveDataId;
            if (0 != SaveController.GetDataNumber() - 1)
            {
                _select = 5;
            }

            Flash();
            _textOptionsLeft.enabled = false;
            _textOptionsRight.enabled = false;
            AudioController.Instance.PlayFx(1, MainControl.Instance.AudioControl.fxClipUI);
        }

        private static void LoadRenameScene()
        {
            GameUtilityService.FadeOutAndSwitchScene("Rename", Color.black, null, true);
        }

        private static void LoadSaveScene()
        {
            GameUtilityService.FadeOutAndSwitchScene(MainControl.Instance.playerControl.saveScene,
                Color.black, null, true);
        }

        private void UpdateInput()
        {
            if (InputService.GetKeyDown(KeyCode.LeftArrow))
            {
                _select--;
            }
            else if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                _select -= 2;
            }

            if (InputService.GetKeyDown(KeyCode.RightArrow))
            {
                _select++;
            }
            else if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                _select += 2;
            }

            LimitSelectRange();

            if (InputService.GetKeyDown(KeyCode.UpArrow) ||
                InputService.GetKeyDown(KeyCode.DownArrow) ||
                InputService.GetKeyDown(KeyCode.LeftArrow) ||
                InputService.GetKeyDown(KeyCode.RightArrow))
            {
                Flash();
            }
        }

        private void LimitSelectRange()
        {
            if (_select < 0 + 2 * Convert.ToInt32(_setData))
            {
                if (_select % 2 != 0)
                {
                    _select = SelectMax;
                }
                else
                {
                    _select = SelectMax - 1;
                }
            }

            if (_select > SelectMax)
            {
                if (_select % 2 == 0)
                {
                    _select = 0 + 2 * Convert.ToInt32(_setData);
                }
                else
                {
                    _select = 1 + 2 * Convert.ToInt32(_setData);
                }
            }

            if (_setData && _select == 2 && saveNumber == 0)
            {
                _select = _select % 2 == 0 ? 3 : 4;
            }
        }

        private void EnableTextOptions()
        {
            if (!_textOptionsLeft.enabled)
            {
                _textOptionsLeft.enabled = true;
            }

            if (!_textOptionsRight.enabled)
            {
                _textOptionsRight.enabled = true;
            }
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
            _textLv.text = $"LV {playerControl.lv}";
            _textPosition.text =
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.settingTexts,
                    playerControl.saveScene);
            Flash();
            _textMessage.text =
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.sceneTexts,
                    "MenuUnder") +
                Application.version;
        }

        private void Flash()
        {
            var list = new List<string>();
            for (var i = 0; i < 6; i++)
            {
                if (_setData && i == 2 && saveNumber == 0)
                {
                    list.Add("<color=grey>");
                }
                else if (i != _select)
                {
                    list.Add("<color=white>");
                }
                else
                {
                    list.Add("<color=yellow>");
                }
            }

            const string endColor = "</color>";
            if (!_setData)
            {
                _textOptionsLeft.text =
                    new StringBuilder().Append((object)list[0])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu0"))
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[2])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu2"))
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[4])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu4"))
                        .Append(endColor)
                        .ToString();

                _textOptionsRight.text =
                    new StringBuilder().Append((object)list[1])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu1"))
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[3])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu3"))
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[5])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu5"))
                        .Append(endColor)
                        .ToString();
            }
            else
            {
                _textOptionsLeft.text =
                    new StringBuilder().Append((object)list[0])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu12"))
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[2])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu6"))
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[4])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            0 == SaveController.GetDataNumber() - 1 ? "Menu8" : "Menu11"))
                        .Append(endColor)
                        .ToString();

                _textOptionsRight.text =
                    new StringBuilder().Append((object)list[1])
                        .Append("Data")
                        .Append(saveNumber)
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[3])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            saveNumber == SaveController.GetDataNumber() - 1 ? "Menu10" : "Menu7"))
                        .Append(endColor)
                        .Append("\n")
                        .Append((object)list[5])
                        .Append(TextProcessingService.GetFirstChildStringByPrefix(
                            MainControl.Instance.LanguagePackControl.sceneTexts,
                            "Menu9"))
                        .Append(endColor)
                        .ToString();
            }
        }
    }
}