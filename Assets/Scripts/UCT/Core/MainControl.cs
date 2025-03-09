using System;
using System.Collections.Generic;
using System.IO;
using Alchemy.Inspector;
using Debug;
using DG.Tweening;
using UCT.Audio;
using UCT.Battle;
using UCT.Battle.BattleConfigs;
using UCT.Control;
using UCT.EventSystem;
using UCT.Overworld;
using UCT.Service;
using UCT.Settings;
using UCT.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UCT.Core
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
            Battle
        }

        [Title("=== 状态 ===")] [ReadOnly] public SceneState sceneState;

        [ReadOnly] public int languagePackId;

        [ReadOnly] public int saveDataId;

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
        public static OverworldPlayerBehaviour OverworldPlayerBehaviour { get; private set; }

        public static MainControl Instance { get; private set; }

        public static int LanguagePackageExternalNumber { get; private set; }

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
                    if (OverworldPlayerBehaviour)
                    {
                        Destroy(OverworldPlayerBehaviour.gameObject);
                        OverworldPlayerBehaviour = null;
                    }

                    break;
                case SceneState.Overworld:
                {
                    if (!eventController)
                    {
                        eventController = GetComponent<EventController>();
                    }

                    GetOverworldPlayerBehaviour();

                    OverworldPlayerBehaviour.transform.position = playerControl.playerLastPos;
                    break;
                }
                case SceneState.Battle:
                    InitializationBattle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected sceneState value: {sceneState}");
            }

            if (isFadeTransitionEnabled)
            {
                sceneSwitchingFadeImage = GameObject.Find("Canvas/InOutBlack").GetComponent<Image>();
                sceneSwitchingFadeImage.color = Color.black;
                SettingsStorage.Pause = !isFadeInUnpaused;
                if (!isFadeInDisabled)
                {
                    sceneSwitchingFadeImage.DOColor(Color.clear, 0.5f).SetEase(Ease.Linear)
                        .OnKill(() => SettingsStorage.Pause = false);
                    SettingsController.Instance.Frame.color = SettingsStorage.IsUsingHdFrame
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

            GameUtilityService.ToggleAllSfx(SettingsStorage.IsSimplifySfx);

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

            if (DataHandlerService.GetItemFormDataName(playerControl.wearWeapon) is EquipmentItem weapon)
            {
                weapon.OnUpdate(0);
            }

            if (DataHandlerService.GetItemFormDataName(playerControl.wearArmor) is EquipmentItem armor)
            {
                armor.OnUpdate(0);
            }
        }

        public static void SetLanguagePackageExternalNumber(int value)
        {
            LanguagePackageExternalNumber = value;
        }

        public static void GetOverworldPlayerBehaviour()
        {
            if (OverworldPlayerBehaviour)
            {
                return;
            }

            var owPlayer = GameObject.Find("Player");

            if (owPlayer)
            {
                OverworldPlayerBehaviour = owPlayer.GetComponent<OverworldPlayerBehaviour>();
            }
        }

        private void InitializeVolume()
        {
            AudioListener.volume = SettingsStorage.MainVolume;

            var bgmVolume = MathUtilityService.NormalizedValueToDb(SettingsStorage.BGMVolume);
            AudioControl.globalAudioMixer.SetFloat("BgmVolume", bgmVolume);


            var fxVolume = MathUtilityService.NormalizedValueToDb(SettingsStorage.FXVolume);
            AudioControl.globalAudioMixer.SetFloat("FxVolume", fxVolume);
        }


        private void InitializationLoad()
        {
            AudioControl = Resources.Load<AudioControl>("AudioControl");
            BattleControl = Resources.Load<BattleControl>("BattleControl");
            playerControl = Resources.Load<PlayerControl>("PlayerControl");

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

        public void InitializationScene()
        {
            languagePackId = DataHandlerService.LanguagePackDetection(languagePackId);

            if (!overworldControl)
            {
                overworldControl = Resources.Load<OverworldControl>("OverworldControl");
                KeyBindings.ResetDictionary();
            }


            var settingAsset = DataHandlerService.LoadLanguageData("UI\\Setting", languagePackId);
            LanguagePackControl.settingTexts = DataHandlerService.LoadItemData(settingAsset);

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

            SettingsStorage.IsUsingHdFrame = Convert.ToBoolean(PlayerPrefs.GetInt("hdResolution", 0));
            SettingsStorage.IsSimplifySfx = Convert.ToBoolean(PlayerPrefs.GetInt("noSFX", 0));
            SettingsStorage.VsyncMode = (VSyncMode)PlayerPrefs.GetInt("vsyncMode", 0);
        }

        private void InitializationBattle()
        {
            if (BattleControl.BattleConfig == null)
            {
                Debug.LogWarning("战斗场景配置类尚未加载，已使用默认的配置类 DemoBattle");
                BattleControl.BattleConfig = new DemoBattle();
                AudioController.Instance.audioSource.clip = BattleControl.BattleConfig.bgmClip;
                AudioController.Instance.audioSource.volume = BattleControl.BattleConfig.volume;
                AudioController.Instance.audioSource.pitch = BattleControl.BattleConfig.pitch;
                AudioController.Instance.audioSource.Play();
            }

            //BattleControl加载
            //--------------------------------------------------------------------------------
            BattleControl.turnDialogAsset = new List<string>();

            var uiText = DataHandlerService.LoadLanguageData("Battle\\UIBattleText", languagePackId);

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

            var uiTextSave = DataHandlerService.LoadItemData(uiText);
            BattleControl.actSave =
                TextProcessingService.BatchGetFirstChildStringByPrefix(uiTextSave,
                    "Act\\");
            BattleControl.mercySave = TextProcessingService.BatchGetFirstChildStringByPrefix(uiTextSave,
                "Mercy\\");
            BattleControl.turnTextSave = TextProcessingService.BatchGetFirstChildStringByPrefix(uiTextSave,
                "Turn\\");
            BattleControl.enemiesNameSave =
                TextProcessingService.BatchGetFirstChildStringByPrefix(uiTextSave,
                    "Enemy\\");
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
                Debug.Log($"Debug: 无敌模式已{(playerControl.keepInvincible ? "开启" : "关闭")}！（Ctrl+I）", "#FFFF00");
            }

            if (playerControl.keepInvincible)
            {
                playerControl.hp = playerControl.hpMax;
            }

            if (_debugStringGradient == null)
            {
                Debug.Log("NO");
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
                SettingsStorage.ResolutionLevel =
                    GameUtilityService.UpdateResolutionSettings(SettingsStorage.IsUsingHdFrame,
                        SettingsStorage.ResolutionLevel);
            }

            if (InputService.GetKeyDown(KeyCode.Semicolon))
            {
                SettingsStorage.IsSimplifySfx = !SettingsStorage.IsSimplifySfx;
                GameUtilityService.ToggleAllSfx(SettingsStorage.IsSimplifySfx);
            }

            // ReSharper disable once InvertIf
            if (InputService.GetKeyDown(KeyCode.F4))
            {
                SettingsStorage.FullScreen = !SettingsStorage.FullScreen;
                GameUtilityService.SetResolution(SettingsStorage.ResolutionLevel);
            }
        }

        public static void HitPlayer(int damage)
        {
            Instance.playerControl.hp -= damage;
            Instance.playerControl.missTime = Instance.playerControl.missTimeMax;


            if (DataHandlerService.GetItemFormDataName(Instance.playerControl.wearArmor) is ArmorItem armor)
            {
                armor.OnDamageTaken(0);
            }
        }
    }
}