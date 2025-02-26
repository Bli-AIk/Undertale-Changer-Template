using System;
using System.Collections.Generic;
using System.IO;
using Alchemy.Inspector;
using Debug;
using DG.Tweening;
using UCT.Battle;
using UCT.Control;
using UCT.EventSystem;
using UCT.Extensions;
using UCT.Global.Settings;
using UCT.Global.UI;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UCT.Global.Core
{
    /// <summary>
    ///     调用所有ScriptableObject，对场景进行初始化
    /// </summary>
    public class MainControl : MonoBehaviour
    {
        public enum SceneState
        {
            Normal,
            Overworld,
            InBattle
        }

        public static MainControl Instance { get; private set; }

        [ReadOnly] [Header("外置语言包总数")] public static int LanguagePackageExternalNumber;

        [Space] [Title("=== 角色与行为控制 ===")] public static OverworldPlayerBehaviour overworldPlayerBehaviour;

        [Title("=== 场景状态设置 ===")] public SceneState sceneState;

        [Space]
        [Title("=== 语言包相关设置 ===")]
        [Header("语言包ID")]
        [FormerlySerializedAs("languagePack")]
        public int languagePackId;

        [Space]
        [Title("=== 存档相关设置 ===")]
        [Header("存档id")]
        [FormerlySerializedAs("dataNumber")]
        public int saveDataId;

        [FormerlySerializedAs("isSceneSwitchingFadeTransitionEnabled")]
        [Space]
        [Title("=== 场景切换相关设置 ===")]
        [Header("当前场景是否启用渐入渐出")]
        [FormerlySerializedAs("haveInOutBlack")]
        public bool isFadeTransitionEnabled;

        [FormerlySerializedAs("isSceneSwitchingFadeInDisabled")]
        [Header("当前场景是否关闭渐入")]
        [FormerlySerializedAs("noInBlack")]
        public bool isFadeInDisabled;

        [FormerlySerializedAs("isSceneSwitchingFadeInUnpaused")]
        [Header("当前场景是否在渐入时不暂停")]
        [FormerlySerializedAs("notPauseIn")]
        public bool isFadeInUnpaused;

        [Header("场景切换使用的Image")]
        [FormerlySerializedAs("_inOutBlack")] public Image sceneSwitchingFadeImage;

        [Header("场景是否在切换")]
        [FormerlySerializedAs("blacking")] public bool isSceneSwitching;

        [Space] [Title("=== UI与画面相关 ===")] public Camera cameraMainInBattle;

        public Camera mainCamera;
        public BoxDrawer mainBox;

        [FormerlySerializedAs("PlayerControl")]
        public PlayerControl playerControl;

        [FormerlySerializedAs("OverworldControl")]
        public OverworldControl overworldControl;

        public BattlePlayerController battlePlayerController;

        public SelectUIController selectUIController;

        public CameraShake cameraShake, cameraShake3D;


        public EventController eventController;

        public readonly ItemController ItemController = new();

        private DebugStringGradient _debugStringGradient = new("Debug");

        public static int LanguagePackageInternalNumber => 3;
        public LanguagePackControl LanguagePackControl { get; private set; }
        public AudioControl AudioControl { get; private set; }
        public BattleControl BattleControl { get; private set; }
        public CharacterSpriteManager[] CharacterSpriteManagers { get; private set; }

        private void Awake()
        {
            ItemController.InitializeItems();

            languagePackId = PlayerPrefs.GetInt("languagePack", 2);

            if (PlayerPrefs.GetInt("dataNumber", 0) >= 0)
            {
                saveDataId = PlayerPrefs.GetInt("dataNumber", 0);
            }
            else
            {
                PlayerPrefs.SetInt("dataNumber", 0);
                saveDataId = 0;
            }

            if (saveDataId > SaveController.GetDataNumber() - 1)
            {
                saveDataId = SaveController.GetDataNumber() - 1;
            }

            Instance = this;
            InitializationLoad();
            Initialization(languagePackId);

            CharacterSpriteManagers = Resources.LoadAll<CharacterSpriteManager>("CharacterSprites");

            if (saveDataId == -1)
            {
                playerControl = DataHandlerService.SetPlayerControl(ScriptableObject.CreateInstance<PlayerControl>());
            }
        }

        public void Start()
        {
            if (playerControl.isDebug && playerControl.invincible)
            {
                playerControl.hp = playerControl.hpMax / 2;
            }

            DataHandlerService.InitializationLanguagePackFullWidth();

            switch (sceneState)
            {
                case SceneState.Normal:
                    if (overworldPlayerBehaviour)
                    {
                        Destroy(overworldPlayerBehaviour.gameObject);
                        overworldPlayerBehaviour = null;
                    }

                    break;
                case SceneState.Overworld:
                {
                    if (!eventController)
                    {
                        eventController = GetComponent<EventController>();
                    }

                    GetOverworldPlayerBehaviour();

                    overworldPlayerBehaviour.transform.position = playerControl.playerLastPos;
                    break;
                }
                case SceneState.InBattle:
                    InitializationBattle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (isFadeTransitionEnabled)
            {
                sceneSwitchingFadeImage = GameObject.Find("Canvas/InOutBlack").GetComponent<Image>();
                sceneSwitchingFadeImage.color = Color.black;
                SettingsStorage.pause = !isFadeInUnpaused;
                if (!isFadeInDisabled)
                {
                    sceneSwitchingFadeImage.DOColor(Color.clear, 0.5f).SetEase(Ease.Linear)
                        .OnKill(() => SettingsStorage.pause = false);
                    SettingsController.Instance.Frame.color = SettingsStorage.isUsingHdFrame
                        ? Color.white
                        : ColorEx.WhiteClear;
                }
                else
                {
                    sceneSwitchingFadeImage.color = Color.clear;
                }
            }

            GameUtilityService.SetCanvasFrameSprite(SettingsController.Instance.frameSpriteIndex);

            InitializeVolume();
            overworldControl.isSetting = false;

            GameUtilityService.ToggleAllSfx(SettingsStorage.isSimplifySfx);

            _debugStringGradient = new DebugStringGradient("Debug");

            EventController.LoadTables();
        }

        private void Update()
        {
            playerControl.gameTime += Time.deltaTime;

            if (playerControl.isDebug)
            {
                DebugUpdate();
            }

            if (playerControl.hpMax < playerControl.hp)
            {
                playerControl.hp = playerControl.hpMax;
            }

            if (overworldControl.isSetting)
            {
                return;
            }

            SettingsShortcuts();
        }

        public static void GetOverworldPlayerBehaviour()
        {
            if (overworldPlayerBehaviour)
            {
                return;
            }

            var owPlayer = GameObject.Find("Player");

            if (owPlayer)
            {
                overworldPlayerBehaviour = owPlayer.GetComponent<OverworldPlayerBehaviour>();
            }
        }

        private void InitializeVolume()
        {
            AudioListener.volume = SettingsStorage.mainVolume;

            var bgmVolume = MathUtilityService.NormalizedValueToDb(SettingsStorage.bgmVolume);
            AudioControl.globalAudioMixer.SetFloat("BgmVolume", bgmVolume);


            var fxVolume = MathUtilityService.NormalizedValueToDb(SettingsStorage.fxVolume);
            AudioControl.globalAudioMixer.SetFloat("FxVolume", fxVolume);
        }


        private void InitializationLoad()
        {
            playerControl = Resources.Load<PlayerControl>("PlayerControl");
            AudioControl = Resources.Load<AudioControl>("AudioControl");
            //InitializationOverworld内调用OverworldControl
            //Initialization内调用ItemControl

            if (playerControl.items.Count < 8)
            {
                playerControl.items = new List<string> { "", "", "", "", "", "", "", "" };
            }
        }

        /// <summary>
        ///     初始化加载一大堆数据
        /// </summary>
        public void Initialization(int languageId)
        {
            if (!LanguagePackControl)
            {
                LanguagePackControl = Resources.Load<LanguagePackControl>("LanguagePackControl");
            }

            if (!languageId.Equals(languagePackId))
            {
                languagePackId = languageId;
            }

            languagePackId = DataHandlerService.LanguagePackDetection(languagePackId);

            //ItemControl加载
            //--------------------------------------------------------------------------------

            var itemTextMax =
                DataHandlerService.LoadItemData(DataHandlerService.LoadLanguageData("UI\\ItemText", languagePackId));

            var stringsByPrefix = TextProcessingService.ClassifyStringsByPrefix(itemTextMax, new[] { "Data", "Item" });
            LanguagePackControl.dataTexts = stringsByPrefix[0];
            var itemTextMaxItem = stringsByPrefix[1];

            LanguagePackControl.dataTexts =
                DataHandlerService.ChangeItemData(LanguagePackControl.dataTexts, true, new List<string>());
            itemTextMaxItem =
                DataHandlerService.ChangeItemData(itemTextMaxItem, true, new List<string>());
            itemTextMaxItem =
                DataHandlerService.ChangeItemData(itemTextMaxItem, false, new List<string>());

            TextProcessingService.SplitStringToListWithDelimiter(itemTextMaxItem,
                LanguagePackControl.itemTexts);
            //--------------------------------------------------------------------------------
        }

        public void InitializationOverworld()
        {
            languagePackId = DataHandlerService.LanguagePackDetection(languagePackId);

            if (!overworldControl)
            {
                overworldControl = Resources.Load<OverworldControl>("OverworldControl");
                KeyBindings.ResetDictionary();
            }


            var settingAsset = DataHandlerService.LoadLanguageData("UI\\Setting", languagePackId);
            LanguagePackControl.settingTexts = DataHandlerService.LoadItemData(settingAsset);

            if (sceneState == SceneState.InBattle)
            {
                return;
            }

            if (SceneManager.GetActiveScene().name != "Start")
            {
                var sceneTextsAsset =
                    DataHandlerService.LoadLanguageData($"Overworld\\{SceneManager.GetActiveScene().name}",
                        languagePackId);
                LanguagePackControl.sceneTexts = DataHandlerService.LoadItemData(sceneTextsAsset);
                LanguagePackControl.sceneTexts =
                    DataHandlerService.ChangeItemData(LanguagePackControl.sceneTexts, true, new List<string>());
            }

            LanguagePackControl.settingTexts =
                DataHandlerService.ChangeItemData(LanguagePackControl.settingTexts, true, new List<string>());

            SettingsStorage.isUsingHdFrame = Convert.ToBoolean(PlayerPrefs.GetInt("hdResolution", 0));
            SettingsStorage.isSimplifySfx = Convert.ToBoolean(PlayerPrefs.GetInt("noSFX", 0));
            SettingsStorage.vsyncMode = (VSyncMode)PlayerPrefs.GetInt("vsyncMode", 0);
        }

        private void InitializationBattle()
        {
            //BattleControl加载
            //--------------------------------------------------------------------------------
            if (!BattleControl)
            {
                BattleControl = Resources.Load<BattleControl>("BattleControl");
            }

            BattleControl.turnDialogAsset = new List<string>();

            BattleControl.uiText = DataHandlerService.LoadLanguageData("Battle\\UIBattleText", languagePackId);

            string[] turnSave;
            if (languagePackId < LanguagePackageInternalNumber)
            {
                var textAssets = Resources.LoadAll<TextAsset>(
                    $"TextAssets/LanguagePacks/{DataHandlerService.GetLanguageInsideId(languagePackId)}/Battle/Turn");

                turnSave = new string[textAssets.Length];
                for (var i = 0; i < textAssets.Length; i++)
                {
                    turnSave[i] = textAssets[i].text;
                }
            }
            else
            {
                turnSave = Directory.GetFiles(
                    $@"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePackId - LanguagePackageInternalNumber]}\Battle\Turn");
            }

            foreach (var t in turnSave)
            {
                if (languagePackId < LanguagePackageInternalNumber)
                {
                    BattleControl.turnDialogAsset.Add(t);
                }
                else if (t[^3..] == "txt")
                {
                    BattleControl.turnDialogAsset.Add(File.ReadAllText(t));
                }
            }

            BattleControl.uiTextSave = DataHandlerService.LoadItemData(BattleControl.uiText);
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.actSave, "Act\\");
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.mercySave,
                "Mercy\\");
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.turnTextSave,
                "Turn\\");

            BattleControl.turnTextSave =
                DataHandlerService.ChangeItemData(BattleControl.turnTextSave, true, new List<string>());
            //--------------------------------------------------------------------------------
            battlePlayerController = GameObject.Find("BattlePlayer").GetComponent<BattlePlayerController>();
            selectUIController = GameObject.Find("SelectUI").GetComponent<SelectUIController>();
            if (!cameraShake)
            {
                cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
            }

            cameraShake3D = GameObject.Find("3D CameraP").GetComponent<CameraShake>();
            if (!cameraMainInBattle)
            {
                cameraMainInBattle = cameraShake.GetComponent<Camera>();
            }
        }

        /// <summary>
        ///     开启Debug后，在每帧执行
        /// </summary>
        private void DebugUpdate()
        {
            // F5刷新场景
            if (Input.GetKeyDown(KeyCode.F5))
            {
                GameUtilityService.RefreshTheScene();
            }

            // 无敌模式 Ctrl+i开启
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
                Input.GetKeyDown(KeyCode.I))
            {
                playerControl.keepInvincible = !playerControl.keepInvincible;
                Other.Debug.Log($"Debug: 无敌模式已{(playerControl.keepInvincible ? "开启" : "关闭")}！（Ctrl+I）", "#FFFF00");
            }

            if (playerControl.keepInvincible)
            {
                playerControl.hp = playerControl.hpMax;
            }

            if (_debugStringGradient == null)
            {
                Other.Debug.Log("NO");
                return;
            }

            // 强制重命名playerName为Debug
            playerControl.playerName = _debugStringGradient.UpdateStringGradient();
        }

        /// <summary>
        ///     通过快捷键设置分辨率、切换音效和全屏模式。
        /// </summary>
        private static void SettingsShortcuts()
        {
            if (InputService.GetKeyDown(KeyCode.Tab))
            {
                SettingsStorage.resolutionLevel =
                    GameUtilityService.UpdateResolutionSettings(SettingsStorage.isUsingHdFrame,
                        SettingsStorage.resolutionLevel);
            }

            if (InputService.GetKeyDown(KeyCode.Semicolon))
            {
                SettingsStorage.isSimplifySfx = !SettingsStorage.isSimplifySfx;
                GameUtilityService.ToggleAllSfx(SettingsStorage.isSimplifySfx);
            }

            // ReSharper disable once InvertIf
            if (InputService.GetKeyDown(KeyCode.F4))
            {
                SettingsStorage.fullScreen = !SettingsStorage.fullScreen;
                GameUtilityService.SetResolution(SettingsStorage.resolutionLevel);
            }
        }
    }
}