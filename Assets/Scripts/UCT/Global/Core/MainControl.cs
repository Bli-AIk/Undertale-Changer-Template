using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using UCT.Battle;
using UCT.Control;
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
    /// 调用所有ScriptableObject 并负责对数据和语言包的导入
    /// 还包括大部分常用的函数
    /// </summary>

    public class MainControl : MonoBehaviour
    {
        public static MainControl Instance;
        
        [FormerlySerializedAs("languagePack")] public int languagePackId;
        public int dataNumber;

        public const int LanguagePackInsideNumber = 3; //内置语言包总数

        [FormerlySerializedAs("blacking")] public bool isSceneSwitching;

        [Header("-BGM BPM设置-")]
        [Space]
        [Header("BGM BPM")]
        public float bpm;

        [Header("BGM BPM偏移")]
        public float bpmDeviation;

        [Header("开启节拍器")]
        public bool isMetronome;

        [Header("-BGM BPM计算结果-")]
        [Space]
        public List<float> beatTimes;

        [Header("-MainControl设置-")]
        [Space]
        [Header("状态:正常,战斗内")]
        public SceneState sceneState;

        public bool haveInOutBlack, noInBlack;
        public bool notPauseIn;

        [FormerlySerializedAs("_inOutBlack")] public Image inOutBlack;

        public int currentBeatIndex;
        public float nextBeatTime;
        
        [Header("引用用的")]
        [Header("战斗外")]
        public PlayerBehaviour playerBehaviour;

        public Camera cameraMainInBattle;
        public Camera mainCamera;

        public BoxDrawer mainBox;

        public enum SceneState
        {
            Normal,
            InBattle,
        }

        public OverworldControl OverworldControl { get; private set; }
        public ItemControl ItemControl { get; private set; }
        public PlayerControl PlayerControl { get; private set; }
        public AudioControl AudioControl { get; private set; }
        public BattleControl BattleControl { get; private set; }

        public BattlePlayerController battlePlayerController;

        public SelectUIController selectUIController;

        public CameraShake cameraShake, cameraShake3D;

        public void SetPlayerControl(PlayerControl playerControl)
        {
            foreach (var field in typeof(PlayerControl).GetFields())
            {
                field.SetValue(null, field.GetValue(playerControl));
            }
        }

        private void InitializationLoad()
        {
            //调用ScriptableObject
            //--------------------------------------------------------------------------------
            PlayerControl = Resources.Load<PlayerControl>("PlayerControl");
            AudioControl = Resources.Load<AudioControl>("AudioControl");
            //InitializationOverworld内调用OverworldControl
            //Initialization内调用ItemControl
            //--------------------------------------------------------------------------------
        }

        /// <summary>
        /// 初始化加载一大堆数据
        /// </summary>
        public void Initialization(int languageId)
        {
            if (!ItemControl)
                ItemControl = Resources.Load<ItemControl>("ItemControl");

            if (!languageId.Equals(languagePackId))
                languagePackId = languageId;

            languagePackId = DataHandlerService.LanguagePackDetection(languagePackId);

            //ItemControl加载
            //--------------------------------------------------------------------------------
            ItemControl.itemText = DataHandlerService.LoadLanguageData("UI\\ItemText", languagePackId);

            DataHandlerService.LoadItemData(ItemControl.itemMax, ItemControl.itemData);
            DataHandlerService.LoadItemData(ItemControl.itemTextMax, ItemControl.itemText);

            TextProcessingService.ClassifyStringsByPrefix(ItemControl.itemTextMax, new[] { "Data", "Item" }, new[] { ItemControl.itemTextMaxData, ItemControl.itemTextMaxItem });
            DataHandlerService.ClassifyItemsData(ItemControl);

            ItemControl.itemTextMaxData = DataHandlerService.ChangeItemData(ItemControl.itemTextMaxData, true, new List<string>());
            ItemControl.itemTextMaxItem = DataHandlerService.ChangeItemData(ItemControl.itemTextMaxItem, true, new List<string>());
            ItemControl.itemTextMaxItem = DataHandlerService.ChangeItemData(ItemControl.itemTextMaxItem, false, new List<string>());

            TextProcessingService.SplitStringToListWithDelimiter(ItemControl.itemTextMaxItem, ItemControl.itemTextMaxItemSon);
            //--------------------------------------------------------------------------------
        }

        public void InitializationOverworld()
        {
            languagePackId = DataHandlerService.LanguagePackDetection(languagePackId);

            if (OverworldControl == null)
            {
                OverworldControl = Resources.Load<OverworldControl>("OverworldControl");
            }

            OverworldControl.settingAsset = DataHandlerService.LoadLanguageData("UI\\Setting", languagePackId);

            DataHandlerService.LoadItemData(OverworldControl.settingSave, OverworldControl.settingAsset);

            if (sceneState == SceneState.InBattle)
                return;
            //OverworldControl加载
            //--------------------------------------------------------------------------------

            OverworldControl.sceneTextsAsset = DataHandlerService.LoadLanguageData($"Overworld\\{SceneManager.GetActiveScene().name}", languagePackId);

            if (SceneManager.GetActiveScene().name == "Start")
                return;
            DataHandlerService.LoadItemData(OverworldControl.sceneTextsSave, OverworldControl.sceneTextsAsset);

            OverworldControl.settingSave = DataHandlerService.ChangeItemData(OverworldControl.settingSave, true, new List<string>());

            OverworldControl.sceneTextsSave = DataHandlerService.ChangeItemData(OverworldControl.sceneTextsSave, true, new List<string>());

            //--------------------------------------------------------------------------------

            OverworldControl.hdResolution = Convert.ToBoolean(PlayerPrefs.GetInt("hdResolution", 0));
            OverworldControl.noSfx = Convert.ToBoolean(PlayerPrefs.GetInt("noSFX", 0));
            OverworldControl.vsyncMode = (OverworldControl.VSyncMode)PlayerPrefs.GetInt("vsyncMode", 0);
        }

        private void InitializationBattle()
        {
            //BattleControl加载
            //--------------------------------------------------------------------------------
            if (BattleControl == null)
                BattleControl = Resources.Load<BattleControl>("BattleControl");

            BattleControl.turnDialogAsset = new List<string>();

            BattleControl.uiText = DataHandlerService.LoadLanguageData("Battle\\UIBattleText", languagePackId);

            string[] turnSave;
            if (languagePackId < LanguagePackInsideNumber)
            {
                var textAssets = Resources.LoadAll<TextAsset>($"TextAssets/LanguagePacks/{DataHandlerService.GetLanguageInsideId(languagePackId)}/Battle/Turn");

                turnSave = new string[textAssets.Length];
                for (var i = 0; i < textAssets.Length; i++)
                {
                    turnSave[i] = textAssets[i].text;
                }
            }
            else
                turnSave = Directory.GetFiles($"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePackId - LanguagePackInsideNumber]}\\Battle\\Turn");

            foreach (var t in turnSave)
            {
                if (languagePackId < LanguagePackInsideNumber)
                    BattleControl.turnDialogAsset.Add(t);
                else if (t[^3..] == "txt")
                    BattleControl.turnDialogAsset.Add(File.ReadAllText(t));
            }

            DataHandlerService.LoadItemData(BattleControl.uiTextSave, BattleControl.uiText);
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.actSave, "Act\\");
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.mercySave, "Mercy\\");
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.turnTextSave, "Turn\\");

            BattleControl.turnTextSave = DataHandlerService.ChangeItemData(BattleControl.turnTextSave, true, new List<string>());
            //--------------------------------------------------------------------------------
            //OldBoxController = GameObject.Find("MainFrame").GetComponent<OldBoxController>();
            battlePlayerController = GameObject.Find("Player").GetComponent<BattlePlayerController>();
            selectUIController = GameObject.Find("SelectUI").GetComponent<SelectUIController>();
            if (cameraShake == null)
                cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
            cameraShake3D = GameObject.Find("3D CameraP").GetComponent<CameraShake>();
            if (cameraMainInBattle == null)
                cameraMainInBattle = cameraShake.GetComponent<Camera>();
        }

        private void Awake()
        {
            mainCamera = Camera.main;
            
            languagePackId = PlayerPrefs.GetInt("languagePack", 2);
            
            if (PlayerPrefs.GetInt("dataNumber", 0) >= 0)
                dataNumber = PlayerPrefs.GetInt("dataNumber", 0);
            else
            {
                PlayerPrefs.SetInt("dataNumber", 0);
                dataNumber = 0;
            }
            if (dataNumber > (SaveController.GetDataNumber() - 1))
            {
                dataNumber = (SaveController.GetDataNumber() - 1);
            }

            Instance = this;
            InitializationLoad();
            Initialization(languagePackId);

            if (dataNumber == -1)
            {
                SetPlayerControl(ScriptableObject.CreateInstance<PlayerControl>());
            }
        }

        public void Start()
        {
            if (PlayerControl.isDebug && PlayerControl.invincible)
                PlayerControl.hp = PlayerControl.hpMax / 2;

            DataHandlerService.InitializationLanguagePackFullWidth();

            if (sceneState == SceneState.InBattle)
            {
                InitializationBattle();
            }
            else
            {
                var playerOw = GameObject.Find("Player");
                if (playerOw)
                    playerBehaviour = playerOw.GetComponent<PlayerBehaviour>();
            }

            if (haveInOutBlack)
            {
                inOutBlack = GameObject.Find("Canvas/InOutBlack").GetComponent<Image>();
                inOutBlack.color = Color.black;
                OverworldControl.pause = !notPauseIn;
                if (!noInBlack)
                {
                    inOutBlack.DOColor(Color.clear, 0.5f).SetEase(Ease.Linear).OnKill(() => OverworldControl.pause = false);
                    CanvasController.Instance.frame.color = OverworldControl.hdResolution ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
                }
                else
                {
                    inOutBlack.color = Color.clear;
                }
            }

            GameUtilityService.SetCanvasFrameSprite(CanvasController.Instance.framePic);

            AudioListener.volume = OverworldControl.mainVolume;
            OverworldControl.isSetting = false;

            GameUtilityService.ToggleAllSfx(OverworldControl.noSfx);

            beatTimes = MathUtilityService.MusicBpmCount(bpm, bpmDeviation);
        }

        private void Update()
        {
            PlayerControl.gameTime += Time.deltaTime;

            if (PlayerControl.isDebug)
            {
                if (Input.GetKeyDown(KeyCode.F5))
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);

                if (PlayerControl.keepInvincible)
                    PlayerControl.hp = PlayerControl.hpMax;

                PlayerControl.playerName = "Debug";
            }
            if (PlayerControl.hpMax < PlayerControl.hp)
                PlayerControl.hp = PlayerControl.hpMax;

            if (OverworldControl.isSetting)
                return;
            if (GameUtilityService.KeyArrowToControl(KeyCode.Tab))
            {
                GameUtilityService.UpdateResolutionSettings();
            }
            if (GameUtilityService.KeyArrowToControl(KeyCode.Semicolon))
            {
                OverworldControl.noSfx = !OverworldControl.noSfx;
                GameUtilityService.ToggleAllSfx(OverworldControl.noSfx);
            }
            if (GameUtilityService.KeyArrowToControl(KeyCode.F4))
            {
                OverworldControl.fullScreen = !OverworldControl.fullScreen;
                GameUtilityService.SetResolution(OverworldControl.resolutionLevel);
            }

            if (isMetronome) GameUtilityService.Metronome();
        }

    }
}