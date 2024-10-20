using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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

        public bool blacking;

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

        private Image _inOutBlack;

        [Header("引用用的")]
        [Header("战斗外")]
        public PlayerBehaviour playerBehaviour;

        //[Header("战斗内")]
        //public OldBoxController OldBoxController;

        private Camera _cameraMainInBattle;
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

            LoadItemData(ItemControl.itemMax, ItemControl.itemData);
            LoadItemData(ItemControl.itemTextMax, ItemControl.itemText);

            TextProcessingService.ClassifyStringsByPrefix(ItemControl.itemTextMax, new[] { "Data", "Item" }, new[] { ItemControl.itemTextMaxData, ItemControl.itemTextMaxItem });
            ItemClassification();

            ItemControl.itemTextMaxData = ChangeItemData(ItemControl.itemTextMaxData, true, new List<string>());
            ItemControl.itemTextMaxItem = ChangeItemData(ItemControl.itemTextMaxItem, true, new List<string>());
            ItemControl.itemTextMaxItem = ChangeItemData(ItemControl.itemTextMaxItem, false, new List<string>());

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

            LoadItemData(OverworldControl.settingSave, OverworldControl.settingAsset);

            if (sceneState == SceneState.InBattle)
                return;
            //OverworldControl加载
            //--------------------------------------------------------------------------------

            OverworldControl.sceneTextsAsset = LoadLanguageData($"Overworld\\{SceneManager.GetActiveScene().name}");

            if (SceneManager.GetActiveScene().name == "Start")
                return;
            LoadItemData(OverworldControl.sceneTextsSave, OverworldControl.sceneTextsAsset);

            OverworldControl.settingSave = ChangeItemData(OverworldControl.settingSave, true, new List<string>());

            OverworldControl.sceneTextsSave = ChangeItemData(OverworldControl.sceneTextsSave, true, new List<string>());

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
                var file = t;

                if (languagePackId < LanguagePackInsideNumber)
                    BattleControl.turnDialogAsset.Add(file);
                else if (t[^3..] == "txt")
                    BattleControl.turnDialogAsset.Add(File.ReadAllText(file));
            }
            LoadItemData(BattleControl.uiTextSave, BattleControl.uiText);
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.actSave, "Act\\");
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.mercySave, "Mercy\\");
            TextProcessingService.GetFirstChildStringByPrefix(BattleControl.uiTextSave, BattleControl.turnTextSave, "Turn\\");

            BattleControl.turnTextSave = ChangeItemData(BattleControl.turnTextSave, true, new List<string>());
            //--------------------------------------------------------------------------------
            //OldBoxController = GameObject.Find("MainFrame").GetComponent<OldBoxController>();
            battlePlayerController = GameObject.Find("Player").GetComponent<BattlePlayerController>();
            selectUIController = GameObject.Find("SelectUI").GetComponent<SelectUIController>();
            if (cameraShake == null)
                cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
            cameraShake3D = GameObject.Find("3D CameraP").GetComponent<CameraShake>();
            if (_cameraMainInBattle == null)
                _cameraMainInBattle = cameraShake.GetComponent<Camera>();
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
                _inOutBlack = GameObject.Find("Canvas/InOutBlack").GetComponent<Image>();
                _inOutBlack.color = Color.black;
                OverworldControl.pause = !notPauseIn;
                if (!noInBlack)
                {
                    _inOutBlack.DOColor(Color.clear, 0.5f).SetEase(Ease.Linear).OnKill(EndBlack);
                    //CanvasController.instance.frame.DOKill();
                    //CanvasController.instance.frame.DOColor(Color.white, 0.5f);
                    CanvasController.Instance.frame.color = OverworldControl.hdResolution ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0);
                }
                else
                {
                    _inOutBlack.color = Color.clear;
                }
            }
            SetCanvasFrameSprite(CanvasController.Instance.framePic);

            AudioListener.volume = OverworldControl.mainVolume;
            OverworldControl.isSetting = false;

            FindAndChangeAllSfx(OverworldControl.noSfx);

            beatTimes = MusicBpmCount(bpm, bpmDeviation);
        }
        public Color RandomColor()
        {
            return new Color(Random.Range(0, 1f), Random.Range(0, 1f), Random.Range(0, 1f), 1);
        }

        private void EndBlack()
        {
            OverworldControl.pause = false;
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
                ChangeResolution();
            }
            if (KeyArrowToControl(KeyCode.Semicolon))
            {
                OverworldControl.noSfx = !OverworldControl.noSfx;
                FindAndChangeAllSfx(OverworldControl.noSfx);
            }
            if (KeyArrowToControl(KeyCode.F4))
            {
                OverworldControl.fullScreen = !OverworldControl.fullScreen;
                SetResolution(OverworldControl.resolutionLevel);
            }

            if (isMetronome)
                Metronome();
        }
        /// <summary>
        /// 计算BGM节拍
        /// </summary>
        private static List<float> MusicBpmCount(float inputBpm, float inputBpmDeviation, float musicDuration = 0)
        {
            if (musicDuration <= 0)
                musicDuration = AudioController.Instance.audioSource.clip.length;

            var beatInterval = 60f / inputBpm;
            var currentTime = inputBpmDeviation;
            List<float> beats = new();

            // 计算每个拍子的时间点，直到达到音乐时长
            while (currentTime < musicDuration)
            {
                beats.Add(currentTime);
                currentTime += beatInterval;
            }

            return beats;
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
        /// 开/关 SFX
        /// </summary>
        public void FindAndChangeAllSfx(bool isClose)
        {
            foreach (var obj in Resources.FindObjectsOfTypeAll(typeof(Light2D)))
            {
                var light2D = (Light2D)obj;
                light2D.enabled = !isClose;
            }
            
            mainCamera.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;

            if (sceneState != SceneState.InBattle) return;
            
            if (!_cameraMainInBattle)
            {
                if (!cameraShake)
                    cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                _cameraMainInBattle = cameraShake.GetComponent<Camera>();
            }
            _cameraMainInBattle.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;
        }

        /// <summary>
        /// 更改分辨率相关代码
        /// </summary>
        public void ChangeResolution()
        {
            if (!OverworldControl.hdResolution)
            {
                if (OverworldControl.resolutionLevel is >= 0 and < 4)
                    OverworldControl.resolutionLevel += 1;
                else
                    OverworldControl.resolutionLevel = 0;
            }
            else
            {
                if (OverworldControl.resolutionLevel is >= 5 and < 6)
                    OverworldControl.resolutionLevel += 1;
                else
                    OverworldControl.resolutionLevel = 5;
            }
            SetResolution(OverworldControl.resolutionLevel);
        }

        private void SetCanvasFrameSprite(int framePic = 2)//一般为CanvasController.instance.framePic
        {
            var frame = CanvasController.Instance.frame;
            frame.sprite = framePic < 0 ? null : OverworldControl.frames[framePic];
        }

        /// <summary>
        /// 分辨率设置
        /// </summary>
        public void SetResolution(int resolution)
        {
            if (!_cameraMainInBattle)
            {
                if (!cameraShake)
                    cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                else
                    _cameraMainInBattle = cameraShake.GetComponent<Camera>();
            }

            if (!OverworldControl.hdResolution)
            {
                if (OverworldControl.resolutionLevel > 4)
                    OverworldControl.resolutionLevel = 0;
            }
            else
            {
                if (OverworldControl.resolutionLevel < 5)
                    OverworldControl.resolutionLevel = 5;
            }

            if (!OverworldControl.hdResolution)
            {
                mainCamera.rect = new Rect(0, 0, 1, 1);
                if (sceneState == SceneState.InBattle)
                {
                    if (_cameraMainInBattle) _cameraMainInBattle.rect = new Rect(0, 0, 1, 1);
                }
                
                if (BackpackBehaviour.Instance)
                    BackpackBehaviour.Instance.SuitResolution();

                CanvasController.Instance.DOKill();
                CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2();
                CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                CanvasController.Instance.setting.transform.localScale = Vector3.one;
                CanvasController.Instance.setting.rectTransform.anchoredPosition = new Vector2(0, CanvasController.Instance.setting.rectTransform.anchoredPosition.y);
            }
            else
            {
                if (mainCamera)
                    mainCamera.rect = new Rect(0, 0.056f, 1, 0.888f);
                
                if (sceneState == SceneState.InBattle)
                    if (_cameraMainInBattle)
                        _cameraMainInBattle.rect = new Rect(0, 0.056f, 1, 0.888f);

                if (BackpackBehaviour.Instance)
                    BackpackBehaviour.Instance.SuitResolution();

                CanvasController.Instance.DOKill();

                if (CanvasController.Instance.framePic < 0)
                {
                    CanvasController.Instance.frame.color = Color.black;
                    CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2(0, -30f);
                }
                else 
                    CanvasController.Instance.fps.rectTransform.anchoredPosition = new Vector2();

                CanvasController.Instance.frame.DOColor(new Color(1, 1, 1, 1) * Convert.ToInt32(CanvasController.Instance.framePic >= 0), 1f);
                CanvasController.Instance.setting.transform.localScale = Vector3.one * 0.89f;
                CanvasController.Instance.setting.rectTransform.anchoredPosition = new Vector2(142.5f, CanvasController.Instance.setting.rectTransform.anchoredPosition.y);
            }

            
            var resolutionHeights = new List<int> { 480, 768, 864, 960, 1080, 540, 1080 };
            var resolutionWidths = GetWidthsWithHeights(resolutionHeights, 5);

            var currentResolutionWidth = resolutionWidths[resolution];
            var currentResolutionHeight = resolutionHeights[resolution];
            
            Screen.SetResolution(currentResolutionWidth, currentResolutionHeight, OverworldControl.fullScreen);
            OverworldControl.resolution = new Vector2(currentResolutionWidth, currentResolutionHeight);
        }

        private static List<int> GetWidthsWithHeights(List<int> heights,int resolutionCutPoint)
        {
            var widths = new List<int>();

            for (var i = 0; i < heights.Count; i++)
            {
                if (i < resolutionCutPoint)
                {
                    widths.Add(heights[i] / 3 * 4); // 4:3 aspect ratio
                }
                else
                {
                    widths.Add(heights[i] * 16 / 9); // 16:9 aspect ratio
                }
            }

            return widths;
        }

        /// <summary>
        /// 淡出 输入跳转场景名称
        /// banMusic是渐出
        /// time大于0有动画 等于0就直接切场景 小于0时会以time的绝对值
        /// </summary>
        public void OutBlack(string scene, Color color, bool banMusic = false, float time = 0.5f, bool async = true)
        {
            blacking = true;
            if (banMusic)
            {
                var bgm = AudioController.Instance.transform.GetComponent<AudioSource>();
                switch (time)
                {
                    case > 0:
                        DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, time).SetEase(Ease.Linear);
                        break;
                    case 0:
                        bgm.volume = 0;
                        break;
                    default:
                        DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, Mathf.Abs(time)).SetEase(Ease.Linear);
                        break;
                }
            }
            OverworldControl.pause = true;
            switch (time)
            {
                case > 0:
                {
                    _inOutBlack.DOColor(color, time).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
                    if (!OverworldControl.hdResolution)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
                case 0:
                    _inOutBlack.color = color;
                    SwitchScene(scene, async);
                    break;
                default:
                {
                    time = Mathf.Abs(time);
                    _inOutBlack.color = color;
                    _inOutBlack.DOColor(color, time).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
                    if (!OverworldControl.hdResolution)
                        CanvasController.Instance.frame.color = new Color(1, 1, 1, 0);
                    break;
                }
            }
        }

        public void OutWhite(string scene)
        {
            blacking = true;
            _inOutBlack.color = new Color(1, 1, 1, 0);
            AudioController.Instance.GetFx(6, AudioControl.fxClipUI);
            _inOutBlack.DOColor(Color.white, 5.5f).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
        }

        public void SwitchScene(string sceneName, bool async = true)
        {
            SetCanvasFrameSprite();
            if (SceneManager.GetActiveScene().name != "Menu" && SceneManager.GetActiveScene().name != "Rename" && SceneManager.GetActiveScene().name != "Story" && SceneManager.GetActiveScene().name != "Start" && SceneManager.GetActiveScene().name != "Gameover")
                PlayerControl.lastScene = SceneManager.GetActiveScene().name;
            if (async)
                SceneManager.LoadSceneAsync(sceneName);
            else SceneManager.LoadScene(sceneName);

            SetResolution(Instance.OverworldControl.resolutionLevel);
            blacking = false;
        }

        [Space]
        public bool forceJumpLoadTurn;

        public IEnumerator _LoadItemDataForTurn(List<string> list, TextAsset inputText)//保存的list 导入的text
        {
            list.Clear();
            var text = "";
            for (var i = 0; i < inputText.text.Length; i++)
            {
                if (inputText.text[i] == '/' && inputText.text[i + 1] == '*')
                {
                    i++;
                    while (!(inputText.text[i] == '/' && inputText.text[i - 1] == '*'))
                    {
                        i++;
                    }
                    i += 2;
                }

                if (inputText.text[i] != '\n' && inputText.text[i] != '\r' && inputText.text[i] != ';')
                    text += inputText.text[i];
                if (inputText.text[i] == ';')
                {
                    list.Add(text + ";");
                    text = "";
                }
                if ((i + 1) % 2 == 0 && !forceJumpLoadTurn)
                    yield return 0;
            }
        }

        /// <summary>
        /// 调入数据(传入TextAsset)
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public static void LoadItemData(List<string> list, TextAsset inputText)//保存的list 导入的text
        {
            list.Clear();
            var text = "";
            for (var i = 0; i < inputText.text.Length; i++)
            {
                if (inputText.text[i] == '/' && inputText.text[i + 1] == '*')
                {
                    i++;
                    while (!(inputText.text[i] == '/' && inputText.text[i - 1] == '*'))
                    {
                        i++;
                    }
                    i += 2;
                }

                if (inputText.text[i] != '\n' && inputText.text[i] != '\r' && inputText.text[i] != ';')
                    text += inputText.text[i];
                if (inputText.text[i] != ';') continue;
                list.Add(text + ";");
                text = "";
            }
        }

        /// <summary>
        /// 调入数据(传入string)
        /// </summary>
        public static void LoadItemData(List<string> list, string inputText)//保存的list 导入的text
        {
            list.Clear();
            var text = "";
            for (var i = 0; i < inputText.Length; i++)
            {
                if (inputText[i] == '/' && inputText[i + 1] == '*')
                {
                    i++;
                    while (!(inputText[i] == '/' && inputText[i - 1] == '*'))
                    {
                        i++;
                    }
                    i += 2;
                }
                if (inputText[i] != '\n' && inputText[i] != '\r' && inputText[i] != ';')
                    text += inputText[i];
                if (inputText[i] != ';') continue;
                list.Add(text + ";");
                text = "";
            }
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
                case >= 20000:
                    typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
                    PlayerControl.wearDef = int.Parse(ItemIdGetName(PlayerControl.myItems[sonSelect - 1], "Auto", 1));
                    (PlayerControl.wearArmor, PlayerControl.myItems[sonSelect - 1]) = (PlayerControl.myItems[sonSelect - 1], PlayerControl.wearArmor);

                    AudioController.Instance.GetFx(3, AudioControl.fxClipUI);
                    break;
                case >= 10000:
                    typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmpText);
                    PlayerControl.wearAtk = int.Parse(ItemIdGetName(PlayerControl.myItems[sonSelect - 1], "Auto", 1));
                    (PlayerControl.wearArm, PlayerControl.myItems[sonSelect - 1]) = (PlayerControl.myItems[sonSelect - 1], PlayerControl.wearArm);

                    AudioController.Instance.GetFx(3, AudioControl.fxClipUI);
                    break;
                //食物
                default:
                {
                    var plusHp = int.Parse(ItemIdGetName(PlayerControl.myItems[sonSelect - 1], "Auto", 2));
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
                        PlayerControl.myItems[sonSelect - 1] = ItemNameGetId(text, "Foods");
                        break;
                    }
                    AudioController.Instance.GetFx(2, AudioControl.fxClipUI);
                    break;
                }
            }
        }

        /// <summary>
        /// 转换特殊字符
        /// </summary>
        public List<string> ChangeItemData(List<string> list, bool isData, List<string> ex)
        {
            var newList = new List<string>();
            var text = "";
            var isXh = false;//检测是否有多个需要循环调用的特殊字符

            foreach (var t in list)
            {
                var empty = "";
                for (var j = 0; j < t.Length; j++)
                {
                    if (empty == "" && !isData)
                    {
                        var k = j;
                        while (t[j] != '\\')
                        {
                            empty += t[j];
                            j++;
                            if (j >= t.Length)
                                break;
                        }
                        j = k;
                        //Debug.Log(list[i] +"/"+ name);
                    }

                    while (t[j] == '<')
                    {
                        var inputText = "";
                        while ((j != 0 && t[j - 1] != '>' && !isXh) || isXh)
                        {
                            inputText += t[j];
                            j++;
                            if (j >= t.Length)
                            {
                                break;
                            }
                            isXh = false;
                        }
                        isXh = true;
                        text = ChangeItemDataSwitch(text, inputText, isData, empty, ex);
                    }
                    isXh = false;

                    if (t[j] == ';')
                    {
                        newList.Add(text + ";");
                        text = "";
                    }
                    else
                    {
                        text += t[j];
                    }
                }
            }
            return newList;
        }

        /// <summary>
        /// ReSharper disable once InvalidXmlDocComment
        /// ChangeItemData中检测'<''>'符号的Switch语句
        /// </summary>
        private string ChangeItemDataSwitch(string text, string inputText, bool isData, string inputName, List<string> ex)
        {
            switch (inputText)
            {
                case "<playerName>":
                    text += PlayerControl.playerName;
                    break;

                case "<enter>"://回车
                    text += "\n";
                    break;

                case "<stop...>":
                    text += ".<stop>.<stop>.<stop>";
                    break;

                case "<stop......>":
                    text += ".<stop>.<stop>.<stop>.<stop>.<stop>.<stop>";
                    break;

                case "<autoFoodFull>":
                    text += ItemControl.itemTextMaxData[11][..^1] + "\n";
                    text += "<autoFood>";
                    break;

                case "<autoCheckFood>":
                    inputText = "<data13>";
                    goto default;

                case "<autoArm>":
                    inputText = "<data14>";
                    goto default;
                case "<autoArmor>":
                    inputText = "<data15>";
                    goto default;

                case "<autoCheckArm>":
                    inputText = "<data16>";
                    goto default;

                case "<autoCheckArmor>":
                    inputText = "<data17>";
                    goto default;

                case "<autoLoseFood>":
                    inputText = "<data18>";
                    goto default;
                case "<autoLoseArm>":
                    inputText = "<data19>";
                    goto default;
                case "<autoLoseArmor>":
                    inputText = "<data20>";
                    goto default;
                case "<autoLoseOther>":
                    inputText = "<data21>";
                    goto default;

                case "<itemHp>":
                    if (inputName != "" && !isData)
                    {
                        text += ItemIdGetName(ItemNameGetId(inputName, "Foods"), "Auto", 2);
                        break;
                    }

                    goto default;

                case "<itemAtk>":
                    if (inputName != "" && !isData)
                    {
                        text += ItemIdGetName(ItemNameGetId(inputName, "Arms"), "Auto", 1);
                        break;
                    }

                    goto default;

                case "<itemDef>":
                    if (inputName != "" && !isData)
                    {
                        text += ItemIdGetName(ItemNameGetId(inputName, "Armors"), "Auto", 1);
                        break;
                    }

                    goto default;

                case "<getEnemiesName>":
                    if (inputName != "" && !isData)
                    {
                        text += ex[0];
                        break;
                    }

                    goto default;
                case "<getEnemiesATK>":
                    if (inputName != "" && !isData)
                    {
                        text += ex[1];
                        break;
                    }

                    goto default;
                case "<getEnemiesDEF>":
                    if (inputName != "" && !isData)
                    {
                        text += ex[2];
                        break;
                    }

                    goto default;

                default:
                    if (IsFrontCharactersMatch("<data", inputText))
                    {
                        text += ItemControl.itemTextMaxData[int.Parse(inputText.Substring(5, inputText.Length - 6))][..(ItemControl.itemTextMaxData[int.Parse(inputText.Substring(5, inputText.Length - 6))].Length - 1)];
                    }
                    else if (inputText.Length > 9)
                    {
                        switch (inputText[..9])
                        {
                            case "<itemName" when !isData:
                            {
                                if (inputName != "")
                                {
                                    if (ItemNameGetId(inputName, inputText.Substring(9, inputText.Length - 10) + 's') < 10000)
                                        text += ItemIdGetName(ItemNameGetId(inputName, inputText.Substring(9, inputText.Length - 10) + 's'), inputText.Substring(9, inputText.Length - 10) + 's', 0);
                                    else text += ItemIdGetName(ItemNameGetId(inputName, inputText.Substring(9, inputText.Length - 10) + 's'), "Auto", 0);
                                }

                                break;
                            }
                            case "<autoLose":
                                switch (inputText.Substring(9, inputText.Length - 10) + 's')
                                {
                                    case "Food":
                                        text += ItemControl.itemTextMaxData[18];
                                        break;

                                    case "Arm":
                                        text += ItemControl.itemTextMaxData[19];
                                        break;

                                    case "Armor":
                                        text += ItemControl.itemTextMaxData[20];
                                        break;

                                    case "Other":
                                        text += ItemControl.itemTextMaxData[21];
                                        break;
                                }

                                break;
                            default:
                                text += inputText;
                                break;
                        }
                    }
                    else
                    {
                        text += inputText;
                    }
                    break;
            }
            return text;
        }

        public static bool IsFrontCharactersMatch(string original, string inputText)
        {
            return inputText.Length > original.Length && inputText[..original.Length] == original;
        }
        /// <summary>
        /// 将输入文本中的字母转换为指定的大小写。（默认大写） 
        /// </summary>
        /// <param name="origin">输入字符串</param>
        /// <param name="toLowercase">是否转换为小写</param>
        /// <returns>转换后的字符串</returns>
        public static string ConvertLettersCase(string origin, bool toLowercase)
        {
            var result = "";
            const string UPPERCASE_LETTERS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string LOWERCASE_LETTERS = "abcdefghijklmnopqrstuvwxyz";
    
            foreach (var t in origin)
            {
                var isPlus = false;
                if (toLowercase)
                {
                    for (var j = 0; j < UPPERCASE_LETTERS.Length; j++)
                    {
                        if (t == UPPERCASE_LETTERS[j])
                        {
                            result += LOWERCASE_LETTERS[j];
                            break;
                        }

                        if (j == UPPERCASE_LETTERS.Length - 1)
                            isPlus = true;
                    }
                }
                else
                {
                    for (var j = 0; j < LOWERCASE_LETTERS.Length; j++)
                    {
                        if (t == LOWERCASE_LETTERS[j])
                        {
                            result += UPPERCASE_LETTERS[j];
                            break;
                        }

                        if (j == LOWERCASE_LETTERS.Length - 1)
                            isPlus = true;
                    }
                }
                if (isPlus)
                    result += t;
            }
            return result;
        }
       
        /// <summary>
        /// 输入形如(x,y)的向量
        /// 若向量形如(xRx，yRy)或(xrx，yry)，则在R左右取随机数
        /// </summary>
        public Vector2 StringVector2ToRealVector2(string stringVector2, Vector3 origin)
        {
            stringVector2 = stringVector2.Substring(1, stringVector2.Length - 2) + ",";
            var realVector2 = Vector2.zero;
            var save = "";
            var isSetX = false;
            foreach (var t in stringVector2)
            {
                if (t == ',')
                {
                    if (!isSetX)
                    {
                        realVector2.x = RandomFloatChange(save, origin.x);
                        isSetX = true;
                        save = "";
                    }
                    else
                    {
                        realVector2.y = RandomFloatChange(save, origin.y, true);
                        break;
                    }
                }
                else
                    save += t;
            }
            return realVector2;
        }

        /// <summary>
        /// 形如xRx / xrx / O   随机分开
        /// 如果没有r或R的话就会返回原本的，非常的实用
        ///
        /// 额外添加：P/p获取玩家位置 通过isY确定是X还是Y
        /// 通过xxx + xRx的形式实现一定程度上的固定。
        /// </summary>
        private float RandomFloatChange(string text, float origin, bool isY = false, float plusSave = 0)
        {
            var isHaveR = false;
            var save = "";
            if (text[0] != 'O' && text[0] != 'o' && text[0] != 'P' && text[0] != 'p')
            {
                float x1 = 0;
                foreach (var t in text)
                {
                    switch (t)
                    {
                        case 'r' or 'R' when !isHaveR:
                            x1 = float.Parse(save);
                            save = "";
                            isHaveR = true;
                            break;
                        case '+':
                            plusSave = float.Parse(save);
                            save = "";
                            break;
                        default:
                            save += t;
                            break;
                    }
                }

                if (!isHaveR) return plusSave + float.Parse(text);
                var x2 = float.Parse(save);
                return plusSave + Random.Range(x1, x2);

            }

            if (text == "P" || text == "p")
            {
                if (isY)
                {
                    return battlePlayerController.transform.position.y;
                }

                return battlePlayerController.transform.position.x;
            }

            if (text.Length > 1 && (text[0] == 'O' || text[0] == 'o') && text[1] == '+')
            {
                //Debug.LogWarning(text.Substring(2));
                //Debug.Log(RandomFloatChange(text.Substring(2), origin, isY, origin));
                return RandomFloatChange(text[2..], origin, isY, origin);
            }

            return origin;
        }

        /*之后回来翻才意识到这不就一个强制转换的事儿）
     *
    /// <summary>
    /// 输入形如(x,y)的向量
    ///
    /// </summary>
    public Vector3 StringVector2ToRealVector3(string stringVector2)
    {
        Vector2 endVector2 = StringVector2ToRealVector2(stringVector2);
        return new Vector3(endVector2.x, endVector2.y);
    }
    */

        /// <summary>
        /// 输入形如(r,g,b,a)的向量
        /// 同样支持随机数
        /// </summary>
        public Color StringVector4ToRealColor(string stringVector4, Color origin)
        {
            stringVector4 = stringVector4.Substring(1, stringVector4.Length - 2) + ",";
            var realVector4 = Color.white;
            var save = "";
            var isSet = 0;
            foreach (var t in stringVector4)
            {
                if (t == ',')
                {
                    switch (isSet)
                    {
                        case 0:
                            realVector4.r = RandomFloatChange(save, origin.r);
                            goto default;

                        case 1:
                            realVector4.g = RandomFloatChange(save, origin.g);
                            goto default;

                        case 2:
                            realVector4.b = RandomFloatChange(save, origin.b);
                            goto default;

                        case 3:
                            realVector4.a = RandomFloatChange(save, origin.a);
                            break;

                        default:
                            isSet++;
                            save = "";
                            break;
                    }
                }
                else
                    save += t;
            }
            return realVector4;
        }

        /// <summary>
        /// 在number1与number2之间判断 符合后返回number2.否则传回number1.
        /// </summary>
        public float JudgmentNumber(bool isGreater, float number1, float number2)
        {
            if (isGreater)
            {
                if (number1 > number2)
                    number1 = number2;
            }
            else
            {
                if (number1 < number2)
                    number1 = number2;
            }
            return number1;
        }

        /// <summary>
        /// 分配Item数据
        /// </summary>
        private void ItemClassification()
        {
            ItemControl.itemFoods.Clear();
            ItemControl.itemArms.Clear();
            ItemControl.itemArmors.Clear();
            ItemControl.itemOthers.Clear();
            foreach (var countText in ItemControl.itemMax)
            {
                var text = new string[4];
                var i = 0;
                foreach (var t in countText)
                {
                    if (t == '\\')
                        i++;
                    else if (t != ';')
                        text[i] += t.ToString();

                    if (i != 3 || t != ';') continue;
                    for (var j = 0; j < text.Length; j++)
                    {
                        if (j != 1)
                            ItemClassificationAdd(text[1], text[j]);
                    }
                    i = 0;
                }
            }
        }

        /// <summary>
        /// ItemClassification的一个子void
        /// </summary>
        private void ItemClassificationAdd(string i, string origin)
        {
            if (origin != "null")
                switch (i)
                {
                    case "Foods":
                        ItemControl.itemFoods.Add(origin);
                        break;

                    case "Arms":
                        ItemControl.itemArms.Add(origin);
                        break;

                    case "Armors":
                        ItemControl.itemArmors.Add(origin);
                        break;

                    case "Others":
                        ItemControl.itemOthers.Add(origin);
                        break;

                    default:
                        ItemControl.itemOthers.Add(origin);
                        break;
                }
        }

        /// <summary>
        /// 通过Id获取Item信息：
        /// type：Foods Arms Armors Others Auto
        /// number：0语言包名称
        ///     1/2：data1/2.
        ///     请勿多输.
        ///     Arm和Armor只有1
        /// </summary>
        public string ItemIdGetName(int id, string type, int number)
        {
            int realId;
            List<string> list;
            string idName;//获取编号名称
            switch (type)
            {
                case "Foods":
                    list = ItemControl.itemFoods;
                    realId = id * 3 - 3;
                    idName = list[realId];
                    break;

                case "Arms":
                    list = ItemControl.itemArms;
                    realId = id * 2 - 2;
                    idName = list[realId];
                    break;

                case "Armors":
                    list = ItemControl.itemArmors;
                    realId = id * 2 - 2;
                    idName = list[realId];
                    break;

                case "Others":
                    list = ItemControl.itemOthers;
                    realId = id * 3 - 3;
                    idName = list[realId];
                    break;

                case "Auto":
                    if (id <= 0)
                    {
                        list = null;
                        realId = 0;
                        idName = "null";
                    }
                    else if (id < 10000)
                    {
                        list = ItemControl.itemFoods;
                        realId = id * 3 - 3;
                        idName = list[realId];
                    }
                    else if (id < 20000)
                    {
                        list = ItemControl.itemArms;
                        realId = (id - 10000) * 2 - 2;
                        idName = list[realId];
                    }
                    else if (id < 30000)
                    {
                        list = ItemControl.itemArmors;
                        realId = (id - 20000) * 2 - 2;
                        idName = list[realId];
                    }
                    else
                    {
                        list = ItemControl.itemOthers;
                        realId = (id - 30000) * 3 - 3;
                        idName = list[realId];
                    }
                    break;

                default:
                    goto case "Others";
            }

            var subText = "";
            if (number == 0)//获取语言包内的名称
            {
                foreach (var t in ItemControl.itemTextMaxItem.Where(t => t[..idName.Length] == idName))
                {
                    idName = t[(idName.Length + 1)..];
                    break;
                }

                subText = idName.TakeWhile(t => t != '\\').Aggregate(subText, (current, t) => current + t);
            }
            else
            {
                if (list != null) 
                    subText = list[realId + number];
            }
            return subText;
        }

        /// <summary>
        /// 通过Id获取Item的数据（HP，ATK等）：
        /// type：Foods Arms Armors Others Auto
        /// justId:勾的话会加上 +xxHP/AT/DF等信息
        /// </summary>
        public string ItemIdGetData(int id, string type, bool notJustId = false)
        {
            int realId;
            List<string> list;
            string idData;//获取编号名称
            switch (type)
            {
                case "Foods":
                    list = ItemControl.itemFoods;
                    realId = id * 3 - 1;

                    if (notJustId)
                    {
                        idData = list[realId] + "HP";
                        if (int.Parse(list[realId]) > 0)
                            idData = $"+{idData}";
                    }
                    else
                        idData = list[realId];
                    break;

                case "Arms":
                    list = ItemControl.itemArms;
                    realId = id * 2 - 1;

                    if (notJustId)
                    {
                        idData = list[realId] + "AT";
                        if (int.Parse(list[realId]) > 0)
                            idData = $"+{idData}";
                    }
                    else
                        idData = list[realId];
                    break;

                case "Armors":
                    list = ItemControl.itemArmors;
                    realId = id * 2 - 1;

                    if (notJustId)
                    {
                        idData = list[realId] + "DF";
                        if (int.Parse(list[realId]) > 0)
                            idData = $"+{idData}";
                    }
                    else
                        idData = list[realId];
                    break;

                case "Others":
                    list = ItemControl.itemOthers;
                    realId = id * 3 - 1;
                    idData = list[realId];
                    break;

                case "Auto":
                    if (id < 10000)
                    {
                        list = ItemControl.itemFoods;
                        realId = id * 3 - 1;

                        if (notJustId)
                        {
                            idData = list[realId] + "HP";
                            if (int.Parse(list[realId]) > 0)
                                idData = $"+{idData}";
                        }
                        else
                            idData = list[realId];
                    }
                    else if (id < 20000)
                    {
                        list = ItemControl.itemArms;
                        realId = (id - 10000) * 2 - 1;

                        if (notJustId)
                        {
                            idData = list[realId] + "AT";
                            if (int.Parse(list[realId]) > 0)
                                idData = $"+{idData}";
                        }
                        else
                            idData = list[realId];
                    }
                    else if (id < 30000)
                    {
                        list = ItemControl.itemArmors;
                        realId = (id - 20000) * 2 - 1;

                        if (notJustId)
                        {
                            idData = list[realId] + "DF";
                            if (int.Parse(list[realId]) > 0)
                                idData = $"+{idData}";
                        }
                        else
                            idData = list[realId];
                    }
                    else
                    {
                        list = ItemControl.itemOthers;
                        realId = (id - 30000) * 3 - 1;
                        idData = list[realId];
                    }
                    break;

                default:
                    goto case "Others";
            }
            return idData;
        }

        /// <summary>
        /// 通过物品数据名称搞到它的id.
        /// type：Foods Arms Armors Others
        /// </summary>
        private int ItemNameGetId(string itemName, string type)
        {
            int id = 0, listInt;
            List<string> list;
            switch (type)
            {
                case "Foods":
                    list = ItemControl.itemFoods;
                    listInt = 3;
                    break;

                case "Arms":
                    list = ItemControl.itemArms;
                    listInt = 2;
                    id += 10000;
                    break;

                case "Armors":
                    list = ItemControl.itemArmors;
                    listInt = 2;
                    id += 20000;
                    break;

                case "Others":
                    list = ItemControl.itemOthers;
                    listInt = 3;
                    id += 30000;
                    break;

                default:
                    return 0;
            }

            if (list == null) return id;
            for (var i = 0; i < list.Count / listInt; i++)
            {
                if (list[i * listInt] != itemName) continue;
                id += i + 1;
                break;
            }
            return id;
        }
    }
}