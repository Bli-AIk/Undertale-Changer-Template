using DG.Tweening;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 调用所有ScriptableObject 并负责对数据和语言包的导入
/// 还包括大部分常用的函数
/// </summary>
public class MainControl : MonoBehaviour
{
    public static MainControl instance;
    public int languagePack;
    public int dataNumber;

    public readonly int languagePackInsideNumber = 3;//内置语言包总数

    public bool blacking = false;

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

    private Image inOutBlack;

    [Header("引用用的")]
    [Header("战斗外")]
    public PlayerBehaviour playerBehaviour;

    //[Header("战斗内")]
    //public OldBoxController OldBoxController;

    private Camera cameraMainInBattle;

    public BoxDrawer mainBox;

    public enum SceneState
    {
        Normal,
        InBattle,
    };

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
        PlayerControl.hp = playerControl.hp;
        PlayerControl.hpMax = playerControl.hpMax;
        PlayerControl.lv = playerControl.lv;
        PlayerControl.exp = playerControl.exp;
        PlayerControl.gold = playerControl.gold;
        PlayerControl.wearAtk = playerControl.wearAtk;
        PlayerControl.wearDef = playerControl.wearDef;
        PlayerControl.nextExp = playerControl.nextExp;
        PlayerControl.missTime = playerControl.missTime;
        PlayerControl.missTimeMax = playerControl.missTimeMax;
        PlayerControl.atk = playerControl.atk;
        PlayerControl.def = playerControl.def;
        PlayerControl.playerName = playerControl.playerName;
        PlayerControl.myItems = playerControl.myItems;
        PlayerControl.wearArm = playerControl.wearArm;
        PlayerControl.wearArmor = playerControl.wearArmor;
        PlayerControl.canMove = playerControl.canMove;
        PlayerControl.gameTime = playerControl.gameTime;
        PlayerControl.lastScene = playerControl.lastScene;
        PlayerControl.saveScene = playerControl.saveScene;
        PlayerControl.isDebug = playerControl.isDebug;
        PlayerControl.invincible = playerControl.invincible;
    }

    /// <summary>
    /// 获取内置语言包ID
    /// </summary>
    public string GetLanguageInsideId(int id)
    {
        switch (id)
        {
            case 0:
                return "CN";

            case 1:
                return "TCN";

            case 2:
                goto default;
            default:
                return "US";
        }
    }

    /// <summary>
    /// 加载对应语言包的数据
    /// </summary>
    private string LoadLanguageData(string path)
    {
        if (languagePack < languagePackInsideNumber)
        {
            return Resources.Load<TextAsset>($"TextAssets/LanguagePacks/{GetLanguageInsideId(languagePack)}/{path}").text;
        }
        else
        {
            return File.ReadAllText($"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePack - languagePackInsideNumber]}\\{path}.txt");
        }
    }

    private void LanguagePackDetection()
    {
        if ((languagePack < 0)
            || (languagePack >= Directory.GetDirectories(Application.dataPath + "\\LanguagePacks").Length + languagePackInsideNumber))
        {
            languagePack = 2;
        }
    }

    public void InitializationLoad()
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
    public void Initialization(int lan)
    {
        if (ItemControl == null)
            ItemControl = Resources.Load<ItemControl>("ItemControl");

        if (lan != languagePack)
            languagePack = lan;

        LanguagePackDetection();

        //ItemControl加载
        //--------------------------------------------------------------------------------
        ItemControl.itemText = LoadLanguageData("UI\\ItemText");

        LoadItemData(ItemControl.itemMax, ItemControl.itemData);
        LoadItemData(ItemControl.itemTextMax, ItemControl.itemText);

        MaxToSon(ItemControl.itemTextMax, new string[2] { "Data", "Item" }, new List<string>[2] { ItemControl.itemTextMaxData, ItemControl.itemTextMaxItem });
        ItemClassificatio();

        ItemControl.itemTextMaxData = ChangeItemData(ItemControl.itemTextMaxData, true, new List<string>());
        ItemControl.itemTextMaxItem = ChangeItemData(ItemControl.itemTextMaxItem, true, new List<string>());
        ItemControl.itemTextMaxItem = ChangeItemData(ItemControl.itemTextMaxItem, false, new List<string>());

        MaxToOneSon(ItemControl.itemTextMaxItem, ItemControl.itemTextMaxItemSon);
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
        OverworldControl.noSFX = Convert.ToBoolean(PlayerPrefs.GetInt("noSFX", 0));
        OverworldControl.vsyncMode = (OverworldControl.VSyncMode)PlayerPrefs.GetInt("vsyncMode", 0);
    }

    public void InitializationLanguagePackFullWidth()
    {
        //检测语言包全半角
        if (OverworldControl.textWidth != bool.Parse(ScreenMaxToOneSon(OverworldControl.settingSave, "LanguagePackFullWidth")))
        {
            OverworldControl.textWidth = bool.Parse(ScreenMaxToOneSon(OverworldControl.settingSave, "LanguagePackFullWidth"));
            foreach (TextChanger textChanger in Resources.FindObjectsOfTypeAll(typeof(TextChanger)))
            {
                textChanger.width = OverworldControl.textWidth;
                textChanger.Set();
                textChanger.Change();
            }
        }

        CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture(ScreenMaxToOneSon(OverworldControl.settingSave, "CultureInfo"));
    }

    public void InitializationBattle()
    {
        //BattleControl加载
        //--------------------------------------------------------------------------------
        if (BattleControl == null)
            BattleControl = Resources.Load<BattleControl>("BattleControl");

        BattleControl.turnDialogAsset = new List<string>();

        BattleControl.uiText = LoadLanguageData("Battle\\UIBattleText");

        string[] turnSave;
        TextAsset[] textAssets;
        if (languagePack < languagePackInsideNumber)
        {
            textAssets = Resources.LoadAll<TextAsset>($"TextAssets/LanguagePacks/{GetLanguageInsideId(languagePack)}/Battle/Turn");

            turnSave = new string[textAssets.Length];
            for (int i = 0; i < textAssets.Length; i++)
            {
                turnSave[i] = textAssets[i].text;
            }
        }
        else
            turnSave = Directory.GetFiles($"{Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePack - languagePackInsideNumber]}\\Battle\\Turn");

        for (int i = 0; i < turnSave.Length; i++)
        {
            string file = turnSave[i];

            if (languagePack < languagePackInsideNumber)
                BattleControl.turnDialogAsset.Add(file);
            else if (turnSave[i].Substring(turnSave[i].Length - 3) == "txt")
                BattleControl.turnDialogAsset.Add(File.ReadAllText(file));
        }
        LoadItemData(BattleControl.uiTextSave, BattleControl.uiText);
        ScreenMaxToOneSon(BattleControl.uiTextSave, BattleControl.actSave, "Act\\");
        ScreenMaxToOneSon(BattleControl.uiTextSave, BattleControl.mercySave, "Mercy\\");
        ScreenMaxToOneSon(BattleControl.uiTextSave, BattleControl.turnTextSave, "Turn\\");

        BattleControl.turnTextSave = ChangeItemData(BattleControl.turnTextSave, true, new List<string>());
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
        languagePack = PlayerPrefs.GetInt("languagePack", 2);
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

        instance = this;
        InitializationLoad();
        Initialization(languagePack);

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
            GameObject playerOw = GameObject.Find("Player");
            if (playerOw != null)
                playerBehaviour = playerOw.GetComponent<PlayerBehaviour>();
        }

        if (haveInOutBlack)
        {
            inOutBlack = GameObject.Find("Canvas/InOutBlack").GetComponent<Image>();
            inOutBlack.color = Color.black;
            OverworldControl.pause = !notPauseIn;
            if (!noInBlack)
            {
                inOutBlack.DOColor(Color.clear, 0.5f).SetEase(Ease.Linear).OnKill(EndBlack);
                if (OverworldControl.hdResolution)
                {
                    //CanvasController.instance.frame.DOKill();
                    //CanvasController.instance.frame.DOColor(Color.white, 0.5f);
                    CanvasController.instance.frame.color = new Color(1, 1, 1, 1);
                }
                else CanvasController.instance.frame.color = new Color(1, 1, 1, 0);
            }
            else
            {
                inOutBlack.color = Color.clear;
            }
        }
        SetCanvasFrameSprite(CanvasController.instance.framePic);

        AudioListener.volume = OverworldControl.mainVolume;
        OverworldControl.isSetting = false;

        FindAndChangeAllSFX(OverworldControl.noSFX);

        beatTimes = BGMBPMCount(bpm, bpmDeviation);
    }
    public Color RandomColor()
    {
        return new Color(UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), UnityEngine.Random.Range(0, 1f), 1);
    }

    /// <summary>
    /// 生成字符串形式的随机颜色。
    /// </summary>
    public string RandomStringColor()
    {
        string text = "<color=#";
        for (int i = 0; i < 6; i++)
        {
            text += string.Format("{0:X}", UnityEngine.Random.Range(0, 16));
        }
        text += "FF>";
        return text;
    }
    /// <summary>
    /// 生成字符串形式的随机颜色。
    /// </summary>
    public string RandomStringColor(string origin)
    {
        return RandomStringColor() + origin + "</color>";
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
            OverworldControl.noSFX = !OverworldControl.noSFX;
            FindAndChangeAllSFX(OverworldControl.noSFX);
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
    List<float> BGMBPMCount(float bpm, float bpmDeviation, float musicDuration = 0)
    {
        if (musicDuration <= 0)
            musicDuration = AudioController.instance.audioSource.clip.length;

        float beatInterval = 60f / bpm;
        float currentTime = bpmDeviation;
        List<float> beats = new();

        // 计算每个拍子的时间点，直到达到音乐时长
        while (currentTime < musicDuration)
        {
            beats.Add(currentTime);
            currentTime += beatInterval;
        }

        return beats;
    }
    public int currentBeatIndex = 0;
    public float nextBeatTime = 0f;
    /// <summary>
    /// 控制节拍器
    /// </summary>
    void Metronome()
    {
        if (beatTimes.Count <= 0)
            return;

        bool firstIn = true;
        while (currentBeatIndex < beatTimes.Count && AudioController.instance.audioSource.time >= nextBeatTime)
        {
            if (firstIn)
            {
                if (currentBeatIndex % 4 == 0)
                {
                    AudioController.instance.GetFx(13, AudioControl.fxClipUI);
                }
                else
                {
                    AudioController.instance.GetFx(14, AudioControl.fxClipUI);
                }
            }
            currentBeatIndex++;

            if (currentBeatIndex < beatTimes.Count)
            {
                nextBeatTime = beatTimes[currentBeatIndex];
            }
            firstIn = false;
        }

        if (currentBeatIndex >= beatTimes.Count)
        {
            nextBeatTime = beatTimes[0];
            currentBeatIndex = 0;
        }
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
    public void FindAndChangeAllSFX(bool isClose)
    {
        foreach (Light2D light in Resources.FindObjectsOfTypeAll(typeof(Light2D)))
        {
            light.enabled = !isClose;
        }
        /*
        foreach (Volume v in Resources.FindObjectsOfTypeAll(typeof(Volume)))
        {
            v.gameObject.SetActive(!isClose);
        }
        */
        Camera.main.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;

        if (sceneState == SceneState.InBattle)
        {
            if (cameraMainInBattle == null)
            {
                if (cameraShake == null)
                    cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
                cameraMainInBattle = cameraShake.GetComponent<Camera>();
            }
            cameraMainInBattle.GetUniversalAdditionalCameraData().renderPostProcessing = !isClose;
        }
    }

    /// <summary>
    /// 按按tab改改分辨率那样子))
    /// </summary>
    public void ChangeResolution()
    {
        if (!OverworldControl.hdResolution)
        {
            if (OverworldControl.resolutionLevel >= 0 && OverworldControl.resolutionLevel < 4)
                OverworldControl.resolutionLevel += 1;
            else
                OverworldControl.resolutionLevel = 0;
        }
        else
        {
            if (OverworldControl.resolutionLevel >= 5 && OverworldControl.resolutionLevel < 6)
                OverworldControl.resolutionLevel += 1;
            else
                OverworldControl.resolutionLevel = 5;
        }
        SetResolution(OverworldControl.resolutionLevel);
    }

    /// <summary>
    /// 和分辨率设置配套的换算
    /// </summary>
    private int ScreenSet(int y)
    {
        //if (OverworldControl.background)
        //    y = y / 9 * 16;
        //else
        y = y / 3 * 4;
        return y;
    }

    private void SetCanvasFrameSprite(int framePic = 2)//一般为CanvasController.instance.framePic
    {
        Image frame = CanvasController.instance.frame;
        if (framePic < 0)
        {
            frame.sprite = null;
        }
        else
        {
            frame.sprite = OverworldControl.frames[framePic];
        }
    }

    /// <summary>
    /// 分辨率设置
    /// </summary>
    public void SetResolution(int resolution)
    {
        if (cameraMainInBattle == null)
        {
            if (cameraShake == null)
                cameraShake = GameObject.Find("Main Camera").GetComponent<CameraShake>();
            if (cameraShake != null)
                cameraMainInBattle = cameraShake.GetComponent<Camera>();
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
            Camera.main.rect = new Rect(0, 0, 1, 1);
            if (sceneState == SceneState.InBattle)
            {
                
                cameraMainInBattle.rect = new Rect(0, 0, 1, 1);
            }
            // BackpackBehaviour rawImage在其脚本中控制
            /*
            RectTransform rectTransform = BackpackBehaviour.instance.rawImage.rectTransform;

            rectTransform.offsetMin = new Vector2(0, 0);

            rectTransform.offsetMax = new Vector2(0, 0);

            rectTransform.localScale = Vector3.one;
            */
            if (BackpackBehaviour.instance != null)
                BackpackBehaviour.instance.SuitResolution();

            CanvasController.instance.DOKill();
            CanvasController.instance.fps.rectTransform.anchoredPosition = new Vector2();
            CanvasController.instance.frame.color = new Color(1, 1, 1, 0);
            CanvasController.instance.setting.transform.localScale = Vector3.one;
            CanvasController.instance.setting.rectTransform.anchoredPosition = new Vector2(0, CanvasController.instance.setting.rectTransform.anchoredPosition.y);
        }
        else
        {
            Camera.main.rect = new Rect(0, 0.056f, 1, 0.888f);
            if (sceneState == SceneState.InBattle)
            {
                cameraMainInBattle.rect = new Rect(0, 0.056f, 1, 0.888f);
            }

            // BackpackBehaviour rawImage在其脚本中控制
            /*
            RectTransform rectTransform = BackpackBehaviour.instance.rawImage.rectTransform;

            rectTransform.offsetMin = new Vector2(107, 0);

            rectTransform.offsetMax = new Vector2(-107, 0);

            rectTransform.localScale = Vector3.one * 0.89f;
            */
            if (BackpackBehaviour.instance != null)
                BackpackBehaviour.instance.SuitResolution();

            //在SetCanvasFrameSprite内设定
            //CanvasController.instance.frame.sprite = OverworldControl.frames[CanvasController.instance.framePic];

            CanvasController.instance.DOKill();

            if (CanvasController.instance.framePic < 0)
            {
                CanvasController.instance.frame.color = Color.black;
                CanvasController.instance.fps.rectTransform.anchoredPosition = new Vector2(0, -30f);
            }
            else 
                CanvasController.instance.fps.rectTransform.anchoredPosition = new Vector2();

            CanvasController.instance.frame.DOColor(new Color(1, 1, 1, 1) * Convert.ToInt32(CanvasController.instance.framePic >= 0), 1f);
            CanvasController.instance.setting.transform.localScale = Vector3.one * 0.89f;
            CanvasController.instance.setting.rectTransform.anchoredPosition = new Vector2(142.5f, CanvasController.instance.setting.rectTransform.anchoredPosition.y);
        }

        switch (resolution)
        {
            case 0:
                Screen.SetResolution(ScreenSet(480), 480, OverworldControl.fullScreen);
                OverworldControl.resolution = new Vector2(ScreenSet(480), 480);
                break;

            case 1:
                Screen.SetResolution(ScreenSet(768), 768, OverworldControl.fullScreen);
                OverworldControl.resolution = new Vector2(ScreenSet(768), 768);
                break;

            case 2:
                Screen.SetResolution(ScreenSet(864), 864, OverworldControl.fullScreen);
                OverworldControl.resolution = new Vector2(ScreenSet(864), 864);
                break;

            case 3:
                Screen.SetResolution(ScreenSet(960), 960, OverworldControl.fullScreen);
                OverworldControl.resolution = new Vector2(ScreenSet(960), 960);
                break;

            case 4:
                Screen.SetResolution(ScreenSet(1080), 1080, OverworldControl.fullScreen);
                OverworldControl.resolution = new Vector2(ScreenSet(1080), 1080);
                break;

            case 5:
                Screen.SetResolution(1920 / 2, 1080 / 2, OverworldControl.fullScreen);
                OverworldControl.resolution = new Vector2(1920 / 2, 1080 / 2);
                break;

            case 6:
                Screen.SetResolution(1920, 1080, OverworldControl.fullScreen);
                OverworldControl.resolution = new Vector2(1920, 1080);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 淡出 输入跳转场景名称
    /// banMusic是渐出
    /// time>0有动画 =0就直接切场景 <0时会以time的绝对值
    /// </summary>
    public void OutBlack(string scene, Color color, bool banMusic = false, float time = 0.5f, bool Async = true)
    {
        blacking = true;
        if (banMusic)
        {
            AudioSource bgm = AudioController.instance.transform.GetComponent<AudioSource>();
            if (time > 0)
                DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, time).SetEase(Ease.Linear);
            else if (time == 0)
                bgm.volume = 0;
            else
                DOTween.To(() => bgm.volume, x => bgm.volume = x, 0, Mathf.Abs(time)).SetEase(Ease.Linear);
        }
        OverworldControl.pause = true;
        if (time > 0)
        {
            inOutBlack.DOColor(color, time).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
            if (!OverworldControl.hdResolution)
                CanvasController.instance.frame.color = new Color(1, 1, 1, 0);
        }
        else if (time == 0)
        {
            inOutBlack.color = color;
            SwitchScene(scene, Async);
        }
        else
        {
            time = Mathf.Abs(time);
            inOutBlack.color = color;
            inOutBlack.DOColor(color, time).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
            if (!OverworldControl.hdResolution)
                CanvasController.instance.frame.color = new Color(1, 1, 1, 0);
        }
    }

    public void OutWhite(string scene)
    {
        blacking = true;
        inOutBlack.color = new Color(1, 1, 1, 0);
        AudioController.instance.GetFx(6, AudioControl.fxClipUI);
        inOutBlack.DOColor(Color.white, 5.5f).SetEase(Ease.Linear).OnKill(() => SwitchScene(scene));
    }

    public void SwitchScene(string name, bool Async = true)
    {
        SetCanvasFrameSprite(2);
        if (SceneManager.GetActiveScene().name != "Menu" && SceneManager.GetActiveScene().name != "Rename" && SceneManager.GetActiveScene().name != "Story" && SceneManager.GetActiveScene().name != "Start" && SceneManager.GetActiveScene().name != "Gameover")
            PlayerControl.lastScene = SceneManager.GetActiveScene().name;
        if (Async)
            SceneManager.LoadSceneAsync(name);
        else SceneManager.LoadScene(name);

        SetResolution(instance.OverworldControl.resolutionLevel);
        blacking = false;
    }

    /// <summary>
    /// 传入string，返回删去末尾i个字符的string
    /// </summary>
    public string SubText(string str, int i = 1)
    {
        str = str.Substring(0, str.Length - i);
        return str;
    }

    /// <summary>
    /// 随机生成一个六位长的英文
    /// </summary>
    public string RandomName(int l = 6, string abc = "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM")
    {
        string text = "";

        for (int i = 0; i < l; i++)
        {
            text += abc[UnityEngine.Random.Range(0, abc.Length)];
        }
        return text;
    }

    [Space]
    public bool forceJumpLoadTurn;

    public IEnumerator _LoadItemDataForTurn(List<string> list, TextAsset texter)//保存的list 导入的text
    {
        list.Clear();
        string text = "";
        for (int i = 0; i < texter.text.Length; i++)
        {
            if (texter.text[i] == '/' && texter.text[i + 1] == '*')
            {
                i++;
                while (!(texter.text[i] == '/' && texter.text[i - 1] == '*'))
                {
                    i++;
                }
                i += 2;
            }

            if (texter.text[i] != '\n' && texter.text[i] != '\r' && texter.text[i] != ';')
                text += texter.text[i];
            if (texter.text[i] == ';')
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
    public void LoadItemData(List<string> list, TextAsset texter)//保存的list 导入的text
    {
        list.Clear();
        string text = "";
        for (int i = 0; i < texter.text.Length; i++)
        {
            if (texter.text[i] == '/' && texter.text[i + 1] == '*')
            {
                i++;
                while (!(texter.text[i] == '/' && texter.text[i - 1] == '*'))
                {
                    i++;
                }
                i += 2;
            }

            if (texter.text[i] != '\n' && texter.text[i] != '\r' && texter.text[i] != ';')
                text += texter.text[i];
            if (texter.text[i] == ';')
            {
                list.Add(text + ";");
                text = "";
            }
        }
    }

    /// <summary>
    /// 调入数据(传入string)
    /// </summary>
    public void LoadItemData(List<string> list, string texter)//保存的list 导入的text
    {
        list.Clear();
        string text = "";
        for (int i = 0; i < texter.Length; i++)
        {
            if (texter[i] == '/' && texter[i + 1] == '*')
            {
                i++;
                while (!(texter[i] == '/' && texter[i - 1] == '*'))
                {
                    i++;
                }
                i += 2;
            }
            if (texter[i] != '\n' && texter[i] != '\r' && texter[i] != ';')
                text += texter[i];
            if (texter[i] == ';')
            {
                list.Add(text + ";");
                text = "";
            }
        }
    }

    /// <summary>
    /// 传入使用背包的哪个物体
    /// 然后就使用 打true会顺带把背包顺序整理下
    /// 然后再让打字机打个字
    /// plusText填0就自己计算
    /// </summary>
    public void UseItem(TypeWritter typeWritter, TMPro.TMP_Text tmp_Text, int sonSelect, int plusText = 0)
    {
        if (plusText == 0)
        {
            if (PlayerControl.myItems[sonSelect - 1] >= 20000)
                plusText = -20000 + ItemControl.itemFoods.Count / 3 + ItemControl.itemArms.Count / 2;
            else if (PlayerControl.myItems[sonSelect - 1] >= 10000)
                plusText = -10000 + ItemControl.itemFoods.Count / 3;
            else plusText = 0;
        }

        if (PlayerControl.myItems[sonSelect - 1] >= 20000)
        {
            typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmp_Text);
            PlayerControl.wearDef = int.Parse(ItemIdGetName(PlayerControl.myItems[sonSelect - 1], "Auto", 1));
            int wearInt = PlayerControl.wearArmor;
            PlayerControl.wearArmor = PlayerControl.myItems[sonSelect - 1];
            PlayerControl.myItems[sonSelect - 1] = wearInt;

            AudioController.instance.GetFx(3, AudioControl.fxClipUI);
        }
        else if (PlayerControl.myItems[sonSelect - 1] >= 10000)
        {
            typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[(PlayerControl.myItems[sonSelect - 1] + plusText) * 5 - 3], false, 0, 0, tmp_Text);
            PlayerControl.wearAtk = int.Parse(ItemIdGetName(PlayerControl.myItems[sonSelect - 1], "Auto", 1));
            int wearInt = PlayerControl.wearArm;
            PlayerControl.wearArm = PlayerControl.myItems[sonSelect - 1];
            PlayerControl.myItems[sonSelect - 1] = wearInt;

            AudioController.instance.GetFx(3, AudioControl.fxClipUI);
        }
        else//食物
        {
            int plusHp = int.Parse(ItemIdGetName(PlayerControl.myItems[sonSelect - 1], "Auto", 2));
            if (PlayerControl.wearArm == 10001)
                plusHp += 4;

            typeWritter.TypeOpen(ItemControl.itemTextMaxItemSon[PlayerControl.myItems[sonSelect - 1] * 5 - 3], false,
                plusHp, 0, tmp_Text);

            PlayerControl.hp += plusHp;

            if (PlayerControl.hp > PlayerControl.hpMax)
                PlayerControl.hp = PlayerControl.hpMax;
            for (int i = 0; i < ItemControl.itemFoods.Count; i++)
            {
                if (ItemControl.itemTextMaxItemSon[PlayerControl.myItems[sonSelect - 1] * 5 - 5] == ItemControl.itemFoods[i])
                {
                    string text = ItemControl.itemFoods[i + 1];
                    text = text.Substring(1, text.Length - 1);
                    PlayerControl.myItems[sonSelect - 1] = ItemNameGetId(text, "Foods");
                    break;
                }
            }
            AudioController.instance.GetFx(2, AudioControl.fxClipUI);
        }
    }

    /// <summary>
    /// 转换特殊字符
    /// </summary>
    public List<string> ChangeItemData(List<string> list, bool isData, List<string> ex)
    {
        List<string> newList = new List<string>();
        string text = "";
        bool isXH = false;//检测是否有多个需要循环调用的特殊字符

        for (int i = 0; i < list.Count; i++)
        {
            string name = "";
            for (int j = 0; j < list[i].Length; j++)
            {
                if (name == "" && !isData)
                {
                    int k = j;
                    while (list[i][j] != '\\')
                    {
                        name += list[i][j];
                        j++;
                        if (j >= list[i].Length)
                            break;
                    }
                    j = k;
                    //Debug.Log(list[i] +"/"+ name);
                }

                while (list[i][j] == '<')
                {
                    string texters = "";
                    while ((j != 0 && list[i][j - 1] != '>' && !isXH) || isXH)
                    {
                        texters += list[i][j];
                        j++;
                        if (j >= list[i].Length)
                        {
                            break;
                        }
                        isXH = false;
                    }
                    isXH = true;
                    text = ChangeItemDataSwitch(text, texters, isData, name, ex);
                }
                isXH = false;

                if (list[i][j] == ';')
                {
                    newList.Add(text + ";");
                    text = "";
                }
                else
                {
                    text += list[i][j];
                }
            }
        }
        return newList;
    }

    /// <summary>
    /// ChangeItemData中检测'<''>'符号的Switch语句
    /// </summary>
    private string ChangeItemDataSwitch(string text, string texters, bool isData, string name, List<string> ex)
    {
        switch (texters)
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
                texters = "<data13>";
                goto default;

            case "<autoArm>":
                texters = "<data14>";
                goto default;
            case "<autoArmor>":
                texters = "<data15>";
                goto default;

            case "<autoCheckArm>":
                texters = "<data16>";
                goto default;

            case "<autoCheckArmor>":
                texters = "<data17>";
                goto default;

            case "<autoLoseFood>":
                texters = "<data18>";
                goto default;
            case "<autoLoseArm>":
                texters = "<data19>";
                goto default;
            case "<autoLoseArmor>":
                texters = "<data20>";
                goto default;
            case "<autoLoseOther>":
                texters = "<data21>";
                goto default;

            case "<itemHp>":
                if (name != "" && !isData)
                {
                    text += ItemIdGetName(ItemNameGetId(name, "Foods"), "Auto", 2);
                    break;
                }
                else goto default;

            case "<itemAtk>":
                if (name != "" && !isData)
                {
                    text += ItemIdGetName(ItemNameGetId(name, "Arms"), "Auto", 1);
                    break;
                }
                else goto default;

            case "<itemDef>":
                if (name != "" && !isData)
                {
                    text += ItemIdGetName(ItemNameGetId(name, "Armors"), "Auto", 1);
                    break;
                }
                else goto default;

            case "<getEnemiesName>":
                if (name != "" && !isData)
                {
                    text += ex[0];
                    break;
                }
                else goto default;
            case "<getEnemiesATK>":
                if (name != "" && !isData)
                {
                    text += ex[1];
                    break;
                }
                else goto default;
            case "<getEnemiesDEF>":
                if (name != "" && !isData)
                {
                    text += ex[2];
                    break;
                }
                else goto default;

            default:
                if (IsFrontCharactersMatch("<data", texters))
                {
                    text += ItemControl.itemTextMaxData[int.Parse(texters.Substring(5, texters.Length - 6))].Substring(0, ItemControl.itemTextMaxData[int.Parse(texters.Substring(5, texters.Length - 6))].Length - 1);
                }
                else if (texters.Length > 9)
                {
                    if (texters.Substring(0, 9) == "<itemName" && !isData)
                    {
                        if (name != "")
                        {
                            if (ItemNameGetId(name, texters.Substring(9, texters.Length - 10) + 's') < 10000)
                                text += ItemIdGetName(ItemNameGetId(name, texters.Substring(9, texters.Length - 10) + 's'), texters.Substring(9, texters.Length - 10) + 's', 0);
                            else text += ItemIdGetName(ItemNameGetId(name, texters.Substring(9, texters.Length - 10) + 's'), "Auto", 0);
                        }
                    }
                    else if (texters.Substring(0, 9) == "<autoLose")
                    {
                        switch (texters.Substring(9, texters.Length - 10) + 's')
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
                    }
                    else
                        text += texters;
                }
                else
                {
                    text += texters;
                }
                break;
        }
        return text;
    }

    public bool IsFrontCharactersMatch(string original, string texters)
    {
        return texters.Length > original.Length && texters.Substring(0, original.Length) == original;
    }

    /// <summary>
    /// 检测输入文本内的大写字母，转为全小写。
    /// </summary>
    public string UppercaseToLowercase(string origin)
    {
        string final = "";
        string bet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string betS = "abcdefghijklmnopqrstuvwxyz";
        for (int i = 0; i < origin.Length; i++)
        {
            bool isPlus = false;
            for (int j = 0; j < bet.Length; j++)
            {
                if (origin[i] == bet[j])
                {
                    final += betS[j];
                    break;
                }
                else if (j == bet.Length - 1)
                    isPlus = true;
            }
            if (isPlus)
                final += origin[i];
        }
        return final;
    }

    /// <summary>
    /// 检测输入文本内的小写字母，转为全大写。
    /// </summary>
    public string LowercaseToUppercase(string origin)
    {
        string final = "";
        string betS = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string bet = "abcdefghijklmnopqrstuvwxyz";
        for (int i = 0; i < origin.Length; i++)
        {
            bool isPlus = false;
            for (int j = 0; j < bet.Length; j++)
            {
                if (origin[i] == bet[j])
                {
                    final += betS[j];
                    break;
                }
                else if (j == bet.Length - 1)
                    isPlus = true;
            }
            if (isPlus)
                final += origin[i];
        }
        return final;
    }

    /// <summary>
    /// 输入形如(x,y)的向量
    /// 若向量形如(xRx，yRy)或(xrx，yry)，则在R左右取随机数
    /// </summary>
    public Vector2 StringVector2ToRealVector2(string stringVector2, Vector3 origin)
    {
        stringVector2 = stringVector2.Substring(1, stringVector2.Length - 2) + ",";
        Vector2 realVector2 = Vector2.zero;
        string save = "";
        bool isSetX = false;
        for (int i = 0; i < stringVector2.Length; i++)
        {
            if (stringVector2[i] == ',')
            {
                if (!isSetX)
                {
                    realVector2.x = RandomFloatChange(save, origin.x, false);
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
                save += stringVector2[i];
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
    public float RandomFloatChange(string text, float origin, bool isY = false, float plusSave = 0)
    {
        bool isHaveR = false;
        string save = "";
        float x1 = 0, x2 = 0;
        if (text[0] != 'O' && text[0] != 'o' && text[0] != 'P' && text[0] != 'p')
        {
            for (int i = 0; i < text.Length; i++)
            {
                if ((text[i] == 'r' || text[i] == 'R') && !isHaveR)
                {
                    x1 = float.Parse(save);
                    save = "";
                    isHaveR = true;
                }
                else if (text[i] == '+')
                {
                    plusSave = float.Parse(save);
                    save = "";
                }
                else save += text[i];
            }
            if (isHaveR)
            {
                x2 = float.Parse(save);
                return plusSave + UnityEngine.Random.Range(x1, x2);
            }
            else
            {
                return plusSave + float.Parse(text);
            }
        }
        else if (text == "P" || text == "p")
        {
            if (isY)
            {
                return battlePlayerController.transform.position.y;
            }
            else
            {
                return battlePlayerController.transform.position.x;
            }
        }
        else
        {
            if (text.Length > 1 && (text[0] == 'O' || text[0] == 'o') && text[1] == '+')
            {
                //Debug.LogWarning(text.Substring(2));
                //Debug.Log(RandomFloatChange(text.Substring(2), origin, isY, origin));
                return RandomFloatChange(text.Substring(2), origin, isY, origin);
            }
            else
                return origin;
        }
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
        Color realVector4 = Color.white;
        string save = "";
        int isSet = 0;
        for (int i = 0; i < stringVector4.Length; i++)
        {
            if (stringVector4[i] == ',')
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
                save += stringVector4[i];
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
    private void ItemClassificatio()
    {
        ItemControl.itemFoods.Clear();
        ItemControl.itemArms.Clear();
        ItemControl.itemArmors.Clear();
        ItemControl.itemOthers.Clear();
        for (int i = 0; i < ItemControl.itemMax.Count; i++)//总物品数
        {
            string countText = ItemControl.itemMax[i];
            string[] text = new string[4];
            int texti = 0;
            for (int k = 0; k < countText.Length; k++)//单物品遍历 寻找\符
            {
                if (countText[k] == '\\')
                    texti++;
                else if (countText[k] != ';')
                    text[texti] += countText[k].ToString();

                if (texti == 3 && countText[k] == ';')
                {
                    for (int j = 0; j < text.Length; j++)
                    {
                        if (j != 1)
                            ItemClassificatioAdd(text[1], text[j]);
                    }
                    texti = 0;
                }
            }
        }
    }

    /// <summary>
    /// ItemClassificatio的一个子void
    /// </summary>
    private void ItemClassificatioAdd(string i, string origin)
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
    /// 检测 '\'字符然后分割文本到子List
    /// 批量处理string
    /// </summary>
    public void MaxToOneSon(List<string> parentList, List<string> sonList, char font = '\\')
    {
        sonList.Clear();
        string text = "";
        for (int i = 0; i < parentList.Count; i++)
        {
            for (int j = 0; j < parentList[i].Length; j++)
            {
                if (parentList[i][j] == font || parentList[i][j] == ';')
                {
                    sonList.Add(text);
                    text = "";
                }
                else text += parentList[i][j];
            }
        }
    }

    /// <summary>
    /// 检测 '\'字符然后分割文本到子List
    /// 传入一个string
    /// </summary>
    public void MaxToOneSon(string parentString, List<string> sonList, char font = '\\')
    {
        sonList.Clear();
        string text = "";

        for (int j = 0; j < parentString.Length; j++)
        {
            if (parentString[j] == font || parentString[j] == ';')
            {
                sonList.Add(text);
                text = "";
            }
            else text += parentString[j];
        }
    }

    /// <summary>
    /// 检测到第一个'\'字符就传出
    /// </summary>
    public string MaxToOneSon(string original, char font = '\\')
    {
        string final = "";
        for (int i = 0; i < original.Length; i++)
        {
            if (original[i] != '\\')
                final += original[i];
            else break;
        }
        return final;
    }

    /// <summary>
    /// 反向检测第一个'\'字符就传出，可选忽视掉最后的 ; 号。
    /// </summary>
    public float MaxToLastFloat(string original, bool noLast = true, char font = '\\')
    {
        if (noLast && original.Substring(original.Length - 1) == ";")
            original = original.Substring(0, original.Length - 1);
        string changed = "";
        for (int i = 0; i < original.Length; i++)
        {
            changed += original[original.Length - i - 1];
        }
        changed = MaxToOneSon(changed, font);
        original = "";
        for (int i = 0; i < changed.Length; i++)
        {
            original += changed[changed.Length - i - 1];
        }
        if (float.TryParse(original, out float y))
            return y;
        else
            return 99999999;
    }

    /// <summary>
    /// 用于游戏内文本读取
    /// 传入数据名称返回文本包文本
    /// 给第一个 返第二个)
    /// </summary>
    public string ScreenMaxToOneSon(List<string> parentList, string screen)
    {
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].Length > screen.Length && MaxToOneSon(parentList[i]) == screen)
            {
                return SubText(parentList[i].Substring(screen.Length + 1));
            }
        }

        return "null";
    }

    /// <summary>
    /// 用于游戏内文本读取
    /// 传入数据名称返回所有同名的文本包文本
    /// </summary>
    public List<string> ScreenMaxToAllSon(List<string> parentList, string screen)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].Length > screen.Length && MaxToOneSon(parentList[i]) == screen)
            {
                list.Add(SubText(parentList[i].Substring(screen.Length + 1)));
            }
        }

        return list;
    }

    /// <summary>
    /// 检测list的前几个字符是否与传入的string screen相同。
    /// 若相同则分割文本到子List
    /// </summary>
    public void ScreenMaxToOneSon(List<string> parentList, List<string> sonList, string screen)
    {
        sonList.Clear();
        for (int i = 0; i < parentList.Count; i++)
        {
            if (parentList[i].Substring(0, screen.Length) == screen)
            {
                sonList.Add(parentList[i].Substring(screen.Length));
            }
        }
    }

    /// <summary>
    /// 再分配文本包
    /// </summary>
    private void MaxToSon(List<string> max, string[] text, List<string>[] son)
    {
        //max.Clear();
        for (int i = 0; i < son.Length; i++)
        {
            son[i].Clear();
        }
        for (int i = 0; i < max.Count; i++)
        {
            for (int j = 0; j < text.Length; j++)
            {
                if (max[i].Substring(0, text[j].Length) == text[j])
                {
                    son[j].Add(max[i].Substring(text[j].Length + 1));
                }
            }
        }
    }

    public List<int> ListOrderChanger(List<int> original)
    {
        List<int> newList = new List<int>();
        int plusNumber = original.Count;
        for (int i = 0; i < original.Count; i++)
        {
            if (original[i] != 0)
            {
                newList.Add(original[i]);
                plusNumber--;
            }
        }
        for (int i = 0; i < plusNumber; i++)
        {
            newList.Add(0);
        }

        return newList;
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

        string subText = "";
        if (number == 0)//获取语言包内的名称
        {
            for (int i = 0; i < ItemControl.itemTextMaxItem.Count; i++)
            {
                if (ItemControl.itemTextMaxItem[i].Substring(0, idName.Length) == idName)
                {
                    idName = ItemControl.itemTextMaxItem[i].Substring(idName.Length + 1);
                    break;
                }
            }
            for (int i = 0; i < idName.Length; i++)
            {
                if (idName[i] == '\\')
                {
                    break;
                }
                else subText += idName[i];
            }
        }
        else
        {
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
    /// 识别到0后传出
    /// </summary>
    public int FindMax(List<int> list)
    {
        int max = list.Count;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == 0)
            {
                max = i;
                break;
            }
        }
        return max;
    }

    /// <summary>
    /// 通过物品数据名称搞到它的id.
    /// type：Foods Arms Armors Others
    /// </summary>
    public int ItemNameGetId(string name, string type)
    {
        int id = 0, listInt = 0;
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

            case "Oters":
                list = ItemControl.itemOthers;
                listInt = 3;
                id += 30000;
                break;

            default:
                return 0;
        }
        if (list != null)
        {
            for (int i = 0; i < list.Count / listInt; i++)
            {
                if (list[i * listInt] == name)
                {
                    id += i + 1;
                    break;
                }
            }
        }
        return id;
    }

    /// <summary>
    /// 给List<Int>，检测到空的返回
    /// </summary>
    public int GetRealIntListCount(List<int> ints)
    {
        for (int i = 0; i < ints.Count; i++)
        {
            if (ints[i] == 0)
                return i;
        }
        return ints.Count;
    }

    public List<int> ListIntAdd(List<int> origin, int add)
    {
        for (int i = 0; i < origin.Count; i++)
        {
            if (origin[i] == 0)
            {
                origin[i] = add;
                break;
            }
        }
        return origin;
    }

    /// <summary>
    /// 随机获取-1或1
    /// </summary>
    public int Get1Or_1()
    {
        int i = 0;
        do
        {
            i = UnityEngine.Random.Range(-1, 2);
        }
        while (i == 0);

        return i;
    }

    /// <summary>
    /// 传入数根据正负返回1/-1。
    /// 传0返1。
    /// </summary>
    public int Get1Or_1(float i)
    {
        if (i >= 0)
            i = 1;
        else
            i = -1;
        return (int)i;
    }

    /// <summary>
    /// 给一个指定长度，然后会用空格填充原字符串
    /// </summary>
    /// <param name="origin">原字符串</param>
    /// <param name="length">返回长度</param>
    /// <returns></returns>
    public string FillString(string origin, int length)
    {
        int forNumber = length - origin.Length;
        for (int i = 0; i < forNumber; i++)
        {
            origin += " ";
        }
        return origin;
    }

    public string GetRealTime(int totalSeconds)
    {
        if (totalSeconds < 0)
            totalSeconds = 0;

        int seconds = totalSeconds % 60;
        totalSeconds /= 60;
        int minutes = totalSeconds % 60;
        int hours = totalSeconds / 60;

        string hoursString = hours < 10 ? $"0{hours}" : $"{hours}";
        string minutesString = minutes < 10 ? $"0{minutes}" : $"{minutes}";

        return $"{hoursString}:{minutesString}";
    }


    public string StringRemover(string inputString, int startIndex, int endIndex, string add = "")
    {
        if (startIndex < 0 || endIndex >= inputString.Length || startIndex > endIndex)
        {
            Debug.Log("无效的起始和结束位置");
            return inputString;
        }

        string part1 = inputString.Substring(0, startIndex); // 从开头到A之前的部分
        string part2 = inputString.Substring(endIndex + 1); // 从B之后到字符串末尾的部分
        Debug.Log(inputString.Substring(startIndex + 1));
        string result = part1 + add + part2; // 合并两部分
        return result;
    }

    public void KillPlayer()
    {
        PlayerControl.hp = PlayerControl.hpMax;

        if (!(PlayerControl.isDebug && PlayerControl.invincible))
        {
            //spriteRenderer.color = Color.red;
            OverworldControl.playerDeadPos = transform.position - (Vector3)battlePlayerController.sceneDrift;
            OverworldControl.pause = true;
            TurnController.instance.KillIEnumerator();
            SwitchScene("Gameover", false);
        }
        else
        {
            selectUIController.UITextUpdate(SelectUIController.UITextMode.Hit);
            Debug.Log("Debug无敌模式已将您的血量恢复", "#FF0000");
        }

    }
    
    /// <summary>
    /// 计算多边形中点
    /// </summary>
    public Vector2 CalculatePolygonCenter(List<Vector2> vertexPoints)
    {
        Vector2 center = Vector2.zero;

        if (vertexPoints == null || vertexPoints.Count == 0)
        {
            return center;
        }

        foreach (Vector2 vertex in vertexPoints)
        {
            center += vertex;
        }

        center /= vertexPoints.Count;

        return center;
    }

    /// <summary>
    /// 在球体表面上生成随机点
    /// </summary>
    public Vector3 RandomPointOnSphereSurface(float sphereRadius, Vector3 sphereCenter)
    {
        Vector3 randomDirection = UnityEngine.Random.onUnitSphere;

        randomDirection *= sphereRadius;

        Vector3 randomPointOnSphere = sphereCenter + randomDirection;

        return randomPointOnSphere;
    }
    public string RichText(string richText)
    {
        return $"<{richText}>";
    }

    public string RichText(string richText, int number)
    {
        return $"<{richText}={number}>";
    }

    public string RichTextWithEnd(string richText, string internalString = default)
    {
        return $"<{richText}>{internalString}</{richText}>";
    }

    public string RichTextWithEnd(string richText, int number, string internalString = default)
    {
        return $"<{richText}={number}>{internalString}</{richText}>";
    }


}