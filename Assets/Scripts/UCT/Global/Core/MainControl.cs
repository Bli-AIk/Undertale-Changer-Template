using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using DG.Tweening;
using TMPro;
using UCT.Battle;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.UI;
using UCT.Overworld;
using UCT.Service;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
        /// <summary>
        /// 获取内置语言包ID
        /// </summary>
        public static string GetLanguageInsideId(int id) =>
            id switch
            {
                0 => "CN",
                1 => "TCN",
                _ => "US"
            };

        /// <summary>
        /// 加载对应语言包的数据
        /// </summary>
        private string LoadLanguageData(string path)
        {
            return languagePackId < LanguagePackInsideNumber 
                ? Resources.Load<TextAsset>($"TextAssets/LanguagePacks/{GetLanguageInsideId(languagePackId)}/{path}").text 
                : File.ReadAllText($"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePackId - LanguagePackInsideNumber]}\\{path}.txt");
        }

        private void LanguagePackDetection()
        {
            if (languagePackId < 0 ||
                languagePackId >=
                Directory.GetDirectories(
                Application.dataPath +
                "\\LanguagePacks").Length +
                LanguagePackInsideNumber)
            {
                languagePackId = 2;
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

            LanguagePackDetection();

            //ItemControl加载
            //--------------------------------------------------------------------------------
            ItemControl.itemText = LoadLanguageData("UI\\ItemText");

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
            LanguagePackDetection();

            if (OverworldControl == null)
            {
                OverworldControl = Resources.Load<OverworldControl>("OverworldControl");
            }

            OverworldControl.settingAsset = LoadLanguageData("UI\\Setting");

            DataHandlerService.LoadItemData(OverworldControl.settingSave, OverworldControl.settingAsset);

            if (sceneState == SceneState.InBattle)
                return;
            //OverworldControl加载
            //--------------------------------------------------------------------------------

            OverworldControl.sceneTextsAsset = LoadLanguageData($"Overworld\\{SceneManager.GetActiveScene().name}");

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

        private void InitializationLanguagePackFullWidth()
        {
            //检测语言包全半角
            if (OverworldControl.textWidth != bool.Parse(TextProcessingService.GetFirstChildStringByPrefix(OverworldControl.settingSave, "LanguagePackFullWidth")))
            {
                OverworldControl.textWidth = bool.Parse(TextProcessingService.GetFirstChildStringByPrefix(OverworldControl.settingSave, "LanguagePackFullWidth"));
                foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(TextChanger)))
                {
                    var textChanger = (TextChanger)obj;
                    textChanger.width = OverworldControl.textWidth;
                    textChanger.Set();
                    textChanger.Change();
                }
            }

            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(TextProcessingService.GetFirstChildStringByPrefix(OverworldControl.settingSave, "CultureInfo"));
        }

        private void InitializationBattle()
        {
            //BattleControl加载
            //--------------------------------------------------------------------------------
            if (BattleControl == null)
                BattleControl = Resources.Load<BattleControl>("BattleControl");

            BattleControl.turnDialogAsset = new List<string>();

            BattleControl.uiText = LoadLanguageData("Battle\\UIBattleText");

            string[] turnSave;
            if (languagePackId < LanguagePackInsideNumber)
            {
                var textAssets = Resources.LoadAll<TextAsset>($"TextAssets/LanguagePacks/{GetLanguageInsideId(languagePackId)}/Battle/Turn");

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

            InitializationLanguagePackFullWidth();

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
        public Color GetRandomColor()
        {
            return new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1);
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
            if (KeyArrowToControl(KeyCode.Tab))
            {
                GameUtilityService.UpdateResolutionSettings();
            }
            if (KeyArrowToControl(KeyCode.Semicolon))
            {
                OverworldControl.noSfx = !OverworldControl.noSfx;
                GameUtilityService.ToggleAllSfx(OverworldControl.noSfx);
            }
            if (KeyArrowToControl(KeyCode.F4))
            {
                OverworldControl.fullScreen = !OverworldControl.fullScreen;
                GameUtilityService.SetResolution(OverworldControl.resolutionLevel);
            }

            if (isMetronome)
                Metronome();
        }

        public int currentBeatIndex;
        public float nextBeatTime;
        /// <summary>
        /// 控制节拍器
        /// </summary>
        private void Metronome()
        {
            if (beatTimes.Count <= 0) return;

            var firstIn = true;
            while (currentBeatIndex < beatTimes.Count && AudioController.Instance.audioSource.time >= nextBeatTime)
            {
                if (firstIn)
                {
                    AudioController.Instance.GetFx(currentBeatIndex % 4 == 0 ? 13 : 14, AudioControl.fxClipUI);
                }
                currentBeatIndex++;

                if (currentBeatIndex < beatTimes.Count)
                {
                    nextBeatTime = beatTimes[currentBeatIndex];
                }
                firstIn = false;
            }

            if (currentBeatIndex < beatTimes.Count) return;
            nextBeatTime = beatTimes[0];
            currentBeatIndex = 0;
        }

        /// <summary>
        /// 应用默认键位
        /// </summary>
        public void ApplyDefaultControl()
        {
            OverworldControl.keyCodes = new List<KeyCode>
            {
                KeyCode.DownArrow,
                KeyCode.RightArrow,
                KeyCode.UpArrow,
                KeyCode.LeftArrow,
                KeyCode.Z,
                KeyCode.X,
                KeyCode.C,
                KeyCode.V,
                KeyCode.F4,
                KeyCode.None,
                KeyCode.None,
                KeyCode.Escape
            };

            OverworldControl.keyCodesBack1 = new List<KeyCode>
            {
                KeyCode.S,
                KeyCode.D,
                KeyCode.W,
                KeyCode.A,
                KeyCode.Return,
                KeyCode.RightShift,
                KeyCode.RightControl,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None
            };
            OverworldControl.keyCodesBack2 = new List<KeyCode>
            {
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.LeftShift,
                KeyCode.LeftControl,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None,
                KeyCode.None
            };
        }

        /// <summary>
        /// 传入默认KeyCode并转换为游戏内键位。
        /// mode:0按下 1持续 2抬起
        /// </summary>
        public bool KeyArrowToControl(KeyCode key, int mode = 0)
        {

            return mode switch
            {
                0 => key switch
                {
                    KeyCode.DownArrow => Input.GetKeyDown(OverworldControl.keyCodes[0]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[0]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[0]),
                    KeyCode.RightArrow => Input.GetKeyDown(OverworldControl.keyCodes[1]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[1]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[1]),
                    KeyCode.UpArrow => Input.GetKeyDown(OverworldControl.keyCodes[2]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[2]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[2]),
                    KeyCode.LeftArrow => Input.GetKeyDown(OverworldControl.keyCodes[3]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[3]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[3]),
                    KeyCode.Z => Input.GetKeyDown(OverworldControl.keyCodes[4]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[4]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[4]),
                    KeyCode.X => Input.GetKeyDown(OverworldControl.keyCodes[5]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[5]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[5]),
                    KeyCode.C => Input.GetKeyDown(OverworldControl.keyCodes[6]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[6]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[6]),
                    KeyCode.V => Input.GetKeyDown(OverworldControl.keyCodes[7]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[7]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[7]),
                    KeyCode.F4 => Input.GetKeyDown(OverworldControl.keyCodes[8]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[8]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[8]),
                    KeyCode.Tab => Input.GetKeyDown(OverworldControl.keyCodes[9]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[9]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[9]),
                    KeyCode.Semicolon => Input.GetKeyDown(OverworldControl.keyCodes[10]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[10]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[10]),
                    KeyCode.Escape => Input.GetKeyDown(OverworldControl.keyCodes[11]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[11]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[11]),
                    _ => false,
                },
                1 => key switch
                {
                    KeyCode.DownArrow => Input.GetKey(OverworldControl.keyCodes[0]) || Input.GetKey(OverworldControl.keyCodesBack1[0]) || Input.GetKey(OverworldControl.keyCodesBack2[0]),
                    KeyCode.RightArrow => Input.GetKey(OverworldControl.keyCodes[1]) || Input.GetKey(OverworldControl.keyCodesBack1[1]) || Input.GetKey(OverworldControl.keyCodesBack2[1]),
                    KeyCode.UpArrow => Input.GetKey(OverworldControl.keyCodes[2]) || Input.GetKey(OverworldControl.keyCodesBack1[2]) || Input.GetKey(OverworldControl.keyCodesBack2[2]),
                    KeyCode.LeftArrow => Input.GetKey(OverworldControl.keyCodes[3]) || Input.GetKey(OverworldControl.keyCodesBack1[3]) || Input.GetKey(OverworldControl.keyCodesBack2[3]),
                    KeyCode.Z => Input.GetKey(OverworldControl.keyCodes[4]) || Input.GetKey(OverworldControl.keyCodesBack1[4]) || Input.GetKey(OverworldControl.keyCodesBack2[4]),
                    KeyCode.X => Input.GetKey(OverworldControl.keyCodes[5]) || Input.GetKey(OverworldControl.keyCodesBack1[5]) || Input.GetKey(OverworldControl.keyCodesBack2[5]),
                    KeyCode.C => Input.GetKey(OverworldControl.keyCodes[6]) || Input.GetKey(OverworldControl.keyCodesBack1[6]) || Input.GetKey(OverworldControl.keyCodesBack2[6]),
                    KeyCode.V => Input.GetKey(OverworldControl.keyCodes[7]) || Input.GetKey(OverworldControl.keyCodesBack1[7]) || Input.GetKey(OverworldControl.keyCodesBack2[7]),
                    KeyCode.F4 => Input.GetKey(OverworldControl.keyCodes[8]) || Input.GetKey(OverworldControl.keyCodesBack1[8]) || Input.GetKey(OverworldControl.keyCodesBack2[8]),
                    KeyCode.Tab => Input.GetKey(OverworldControl.keyCodes[9]) || Input.GetKey(OverworldControl.keyCodesBack1[9]) || Input.GetKey(OverworldControl.keyCodesBack2[9]),
                    KeyCode.Semicolon => Input.GetKey(OverworldControl.keyCodes[10]) || Input.GetKey(OverworldControl.keyCodesBack1[10]) || Input.GetKey(OverworldControl.keyCodesBack2[10]),
                    KeyCode.Escape => Input.GetKey(OverworldControl.keyCodes[11]) || Input.GetKey(OverworldControl.keyCodesBack1[11]) || Input.GetKey(OverworldControl.keyCodesBack2[11]),
                    _ => false,
                },
                2 => key switch
                {
                    KeyCode.DownArrow => Input.GetKeyUp(OverworldControl.keyCodes[0]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[0]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[0]),
                    KeyCode.RightArrow => Input.GetKeyUp(OverworldControl.keyCodes[1]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[1]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[1]),
                    KeyCode.UpArrow => Input.GetKeyUp(OverworldControl.keyCodes[2]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[2]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[2]),
                    KeyCode.LeftArrow => Input.GetKeyUp(OverworldControl.keyCodes[3]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[3]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[3]),
                    KeyCode.Z => Input.GetKeyUp(OverworldControl.keyCodes[4]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[4]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[4]),
                    KeyCode.X => Input.GetKeyUp(OverworldControl.keyCodes[5]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[5]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[5]),
                    KeyCode.C => Input.GetKeyUp(OverworldControl.keyCodes[6]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[6]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[6]),
                    KeyCode.V => Input.GetKeyUp(OverworldControl.keyCodes[7]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[7]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[7]),
                    KeyCode.F4 => Input.GetKeyUp(OverworldControl.keyCodes[8]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[8]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[8]),
                    KeyCode.Tab => Input.GetKeyUp(OverworldControl.keyCodes[9]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[9]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[9]),
                    KeyCode.Semicolon => Input.GetKeyUp(OverworldControl.keyCodes[10]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[10]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[10]),
                    KeyCode.Escape => Input.GetKeyUp(OverworldControl.keyCodes[11]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[11]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[11]),
                    _ => false,
                },
                _ => false,
            };
        }

        /// <summary>
        /// 传入使用背包的哪个物体
        /// 然后就使用 打true会顺带把背包顺序整理下
        /// 然后再让打字机打个字
        /// plusText填0就自己计算
        /// </summary>
        public void UseItem(TypeWritter typeWritter, TMP_Text tmpText, int sonSelect, int plusText = 0)
        {
            if (plusText == 0)
            {
                plusText = PlayerControl.myItems[sonSelect - 1] switch
                {
                    >= 20000 => -20000 + ItemControl.itemFoods.Count / 3 + ItemControl.itemArms.Count / 2,
                    >= 10000 => -10000 + ItemControl.itemFoods.Count / 3,
                    _ => 0
                };
            }

            switch (PlayerControl.myItems[sonSelect - 1])
            {
                case >= 20000:// 防具
                    typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
                    PlayerControl.wearDef = int.Parse(DataHandlerService.ItemIdGetName(ItemControl, PlayerControl.myItems[sonSelect - 1], "Auto", 1));
                    (PlayerControl.wearArmor, PlayerControl.myItems[sonSelect - 1]) = (PlayerControl.myItems[sonSelect - 1], PlayerControl.wearArmor);

                    AudioController.Instance.GetFx(3, AudioControl.fxClipUI);
                    break;
                case >= 10000:// 武器
                    typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
                    PlayerControl.wearAtk = int.Parse(DataHandlerService.ItemIdGetName(ItemControl, PlayerControl.myItems[sonSelect - 1], "Auto", 1));
                    (PlayerControl.wearArm, PlayerControl.myItems[sonSelect - 1]) = (PlayerControl.myItems[sonSelect - 1], PlayerControl.wearArm);

                    AudioController.Instance.GetFx(3, AudioControl.fxClipUI);
                    break;
                // 食物
                default:
                {
                    var plusHp = int.Parse(DataHandlerService.ItemIdGetName(ItemControl, PlayerControl.myItems[sonSelect - 1], "Auto", 2));
                    if (PlayerControl.wearArm == 10001)
                        plusHp += 4;

                    typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[PlayerControl.myItems[sonSelect - 1] * 5 - 3], false,
                        plusHp, 0, tmpText);

                    PlayerControl.hp += plusHp;

                    if (PlayerControl.hp > PlayerControl.hpMax)
                        PlayerControl.hp = PlayerControl.hpMax;
                    for (var i = 0; i < ItemControl.itemFoods.Count; i++)
                    {
                        if (ItemControl.itemTextMaxItemSon[PlayerControl.myItems[sonSelect - 1] * 5 - 5] !=
                            ItemControl.itemFoods[i]) continue;
                        var text = ItemControl.itemFoods[i + 1];
                        text = text.Substring(1, text.Length - 1);
                        PlayerControl.myItems[sonSelect - 1] = DataHandlerService.ItemNameGetId(ItemControl, text, "Foods");
                        break;
                    }
                    AudioController.Instance.GetFx(2, AudioControl.fxClipUI);
                    break;
                }
            }
        }
    }
}