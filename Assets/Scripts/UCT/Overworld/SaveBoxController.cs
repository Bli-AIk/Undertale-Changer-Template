using System;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Overworld
{
    public class SaveBoxController : MonoBehaviour
    {
        public static SaveBoxController Instance;

        private void Awake()
        {
            Instance = this;
        }

        private bool _saveOpen;

        private enum SaveSelect
        {
            Confirm,
            Cancel,
            Confirmed
        }
        
        private SaveSelect _saveSelect;

        private void Update()
        {
            if (_saveOpen) UpdateSave();
        }
        
        public void OpenSaveBox()
        {
            MainControl.Instance.playerControl.canMove = false;
            SettingsStorage.pause = true;
            
            _saveOpen = true;
            _saveSelect = SaveSelect.Confirm;

            BackpackBehaviour.Instance.saveBox.localPosition = new Vector3(
                BackpackBehaviour.Instance.saveBox.localPosition.x, BackpackBehaviour.Instance.saveBox.localPosition.y,
                5);
            var playerName = MainControl.Instance.playerControl.playerName;
            BackpackBehaviour.Instance.saveText.text =
                SetFirstHalfSaveText(playerName) +
                "<indent=7.5></indent>" +
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.settingSave,
                    "Save") +
                "<indent=52.5></indent>" +
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.settingSave,
                    "Back");

            BackpackBehaviour.Instance.saveHeart.localPosition = new Vector3(-4.225f, -1.25f, 0);
        }
        
        private static string SetFirstHalfSaveText(string playerName)
        {
            return playerName +
                   "<indent=35></indent>" +
                   "LV" +
                   MainControl.Instance.playerControl.lv +
                   "<indent=72></indent>" +
                   TextProcessingService.GetRealTime((int)MainControl.Instance.playerControl.gameTime) + "\n" +
                   TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.overworldControl.settingSave,
                       SceneManager.GetActiveScene().name) + "\n" +
                   "<line-height=1.5>\n</line-height>";
        }

        private void UpdateSave()
        {
            if (_saveSelect != SaveSelect.Confirmed)
            {
                if (InputService.GetKeyDown(KeyCode.LeftArrow) ||
                    InputService.GetKeyDown(KeyCode.RightArrow))
                {
                    _saveSelect = _saveSelect switch
                    {
                        SaveSelect.Confirm => SaveSelect.Cancel,
                        SaveSelect.Cancel => SaveSelect.Confirm,
                        _ => _saveSelect
                    };

                    BackpackBehaviour.Instance.saveHeart.localPosition =
                        new Vector3(-4.225f + (int)_saveSelect * 4.5f, -1.25f, 0);
                }
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                switch (_saveSelect)
                {
                    case SaveSelect.Confirm:
                        _saveSelect = SaveSelect.Confirmed;
                        SaveGame();
                        AudioController.Instance.GetFx(12, MainControl.Instance.AudioControl.fxClipUI);
                        BackpackBehaviour.Instance.saveHeart.position = new Vector2(10000, 10000);
                        BackpackBehaviour.Instance.saveText.text =
                            "<color=yellow>" +
                            SetFirstHalfSaveText(MainControl.Instance.playerControl.playerName) +
                            "<indent=7.5></indent>" +
                            TextProcessingService.GetFirstChildStringByPrefix(
                                MainControl.Instance.overworldControl.settingSave, "Saved");
                        break;

                    case SaveSelect.Cancel:
                    case SaveSelect.Confirmed:
                        goto default;
                    default:
                        BackpackBehaviour.Instance.saveHeart.position = new Vector2(10000, 10000);
                        BackpackBehaviour.Instance.saveBox.localPosition = new Vector3(
                            BackpackBehaviour.Instance.saveBox.localPosition.x,
                            BackpackBehaviour.Instance.saveBox.localPosition.y, -50);
                        BackpackBehaviour.Instance.saveText.text = "";
                        MainControl.Instance.playerControl.canMove = true;
                        SettingsStorage.pause = false;
                        _saveOpen = false;
                        break;

                }
            }
            else if (InputService.GetKeyDown(KeyCode.X))
            {
                BackpackBehaviour.Instance.saveHeart.position = new Vector2(10000, 10000);
                BackpackBehaviour.Instance.saveBox.localPosition = new Vector3(
                    BackpackBehaviour.Instance.saveBox.localPosition.x,
                    BackpackBehaviour.Instance.saveBox.localPosition.y, -50);
                BackpackBehaviour.Instance.saveText.text = "";
                MainControl.Instance.playerControl.canMove = true;
                SettingsStorage.pause = false;
                _saveOpen = false;
            }
        }

        private static void SaveGame()
        {
            SaveController.SaveData(MainControl.Instance.playerControl,
                "Data" + MainControl.Instance.saveDataId);
            MainControl.Instance.playerControl.saveScene = SceneManager.GetActiveScene().name;
            PlayerPrefs.SetInt("languagePack", MainControl.Instance.languagePackId);
            PlayerPrefs.SetInt("dataNumber", MainControl.Instance.saveDataId);
            PlayerPrefs.SetInt("hdResolution",
                Convert.ToInt32(SettingsStorage.isUsingHdFrame));
            PlayerPrefs.SetInt("noSFX", Convert.ToInt32(SettingsStorage.isSimplifySfx));
            PlayerPrefs.SetInt("vsyncMode",
                Convert.ToInt32(SettingsStorage.vsyncMode));
        }
    }
}