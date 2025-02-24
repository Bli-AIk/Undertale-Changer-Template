using System;
using Plugins.Timer.Source;
using TMPro;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.Settings;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UCT.Overworld
{
    public class SaveBoxController : MonoBehaviour
    {
        private SaveSelectionState _currentSelection = SaveSelectionState.Confirm;

        private bool _isSaveOpen;
        private static Transform SaveHeart => BackpackBehaviour.Instance.saveHeart;
        private static TMP_Text SaveText => BackpackBehaviour.Instance.saveText;
        private static BoxDrawer SaveBox => BackpackBehaviour.Instance.saveBox;
        public static SaveBoxController Instance { get; private set; }

        private static PlayerControl PlayerControl => MainControl.Instance.playerControl;
        private static OverworldControl OverworldControl => MainControl.Instance.overworldControl;
        private static AudioControl AudioControl => MainControl.Instance.AudioControl;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (!_isSaveOpen)
            {
                return;
            }

            HandleSelectionInput();
            HandleConfirmationInput();
            HandleCancellationInput();
        }

        public void OpenSaveBox()
        {
            SetGamePaused(true);
            _isSaveOpen = true;
            _currentSelection = SaveSelectionState.Confirm;

            InitializeSaveBoxPosition();
            UpdateSaveText();
            UpdateSelectionIndicator();
        }

        private static void SetGamePaused(bool isPaused)
        {
            PlayerControl.canMove = !isPaused;
            SettingsStorage.pause = isPaused;
        }

        private static void InitializeSaveBoxPosition()
        {
            SaveBox.localPosition = new Vector3(
                SaveBox.localPosition.x,
                SaveBox.localPosition.y,
                5);
        }

        private static void UpdateSaveText()
        {
            var saveContent = BuildSaveTextContent();
            SaveText.text = FormatSaveText(saveContent);
        }

        private static (string summary, string saveLabel, string backLabel) BuildSaveTextContent()
        {
            return (
                BuildSaveSummaryText(PlayerControl.playerName),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.settingTexts,
                    "Save"),
                TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.settingTexts,
                    "Back")
            );
        }

        private static string FormatSaveText((string summary, string saveLabel, string backLabel) content)
        {
            return $"{content.summary}<indent=7.5></indent>{content.saveLabel}"
                   + $"<indent=52.5></indent>{content.backLabel}";
        }

        private static string BuildSaveSummaryText(string playerName)
        {
            return $"{playerName}<indent=35></indent>LV{PlayerControl.lv}"
                   + $"<indent=72></indent>{TextProcessingService.GetRealTime((int)PlayerControl.gameTime)}\n"
                   + $"{TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.settingTexts, SceneManager.GetActiveScene().name)}\n"
                   + "<line-height=1.5>\n</line-height>";
        }

        private void HandleSelectionInput()
        {
            if (_currentSelection == SaveSelectionState.Confirmed)
            {
                return;
            }

            if (!InputService.GetKeyDown(KeyCode.LeftArrow) && !InputService.GetKeyDown(KeyCode.RightArrow))
            {
                return;
            }

            ToggleSelection();
            UpdateSelectionIndicator();
        }

        private void ToggleSelection()
        {
            _currentSelection = _currentSelection switch
            {
                SaveSelectionState.Confirm => SaveSelectionState.Cancel,
                SaveSelectionState.Cancel => SaveSelectionState.Confirm,
                _ => _currentSelection
            };
        }

        private void UpdateSelectionIndicator()
        {
            var xPosition = -4.225f + (int)_currentSelection * 4.5f;
            SaveHeart.localPosition = new Vector3(xPosition, -1.25f, 0);
        }

        private void HandleConfirmationInput()
        {
            if (!InputService.GetKeyDown(KeyCode.Z))
            {
                return;
            }

            switch (_currentSelection)
            {
                case SaveSelectionState.Confirm:
                    ProcessSaveConfirmation();
                    break;
                case SaveSelectionState.Cancel:
                case SaveSelectionState.Confirmed:
                default:
                    CloseSaveBox();
                    break;
            }
        }

        private void ProcessSaveConfirmation()
        {
            _currentSelection = SaveSelectionState.Confirmed;
            SaveService.SaveGame();

            AudioController.Instance.PlayFx(12, AudioControl.fxClipUI);
            UpdateAfterSaveUI();
        }

        private static void UpdateAfterSaveUI()
        {
            SaveHeart.position = new Vector2(10000, 10000);
            SaveText.text = $"<color=yellow>{BuildSaveSummaryText(PlayerControl.playerName)}"
                            + $"<indent=7.5></indent>{TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.LanguagePackControl.settingTexts, "Saved")}";
        }

        private void HandleCancellationInput()
        {
            if (!InputService.GetKeyDown(KeyCode.X))
            {
                return;
            }

            CloseSaveBox();
        }

        private void CloseSaveBox()
        {
            ResetUIElements();
            Timer.Register(0.1f, () => SetGamePaused(false));

            _isSaveOpen = false;
        }

        private static void ResetUIElements()
        {
            SaveHeart.position = new Vector2(10000, 10000);
            SaveBox.localPosition = new Vector3(
                SaveBox.localPosition.x,
                SaveBox.localPosition.y,
                -50);
            SaveText.text = "";
        }

        private enum SaveSelectionState
        {
            Confirm,
            Cancel,
            Confirmed
        }
    }

    public static class SaveService
    {
        public static void SaveGame()
        {
            SaveController.SaveData(MainControl.Instance.playerControl,
                $"Data{MainControl.Instance.saveDataId}");

            var player = MainControl.Instance.playerControl;
            player.saveScene = SceneManager.GetActiveScene().name;

            SavePlayerPreferences();
        }

        private static void SavePlayerPreferences()
        {
            PlayerPrefs.SetInt("languagePack", MainControl.Instance.languagePackId);
            PlayerPrefs.SetInt("dataNumber", MainControl.Instance.saveDataId);
            PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(SettingsStorage.isUsingHdFrame));
            PlayerPrefs.SetInt("noSFX", Convert.ToInt32(SettingsStorage.isSimplifySfx));
            PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(SettingsStorage.vsyncMode));
        }
    }
}