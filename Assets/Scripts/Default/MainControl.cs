using DG.Tweening;
using Log;
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
/// ��������ScriptableObject ����������ݺ����԰��ĵ���
/// �������󲿷ֳ��õĺ���
/// </summary>
public class MainControl : MonoBehaviour
{
    public static MainControl instance;
    public int languagePack;
    public int dataNum;

    public readonly int languagePackInsideNum = 3;//�������԰�����

    public bool blacking = false;

    [Header("-MainControl����-")]
    [Space]
    [Header("״̬:����,ս����")]
    public SceneState sceneState;

    public bool haveInOutBlack, noInBlack;
    public bool notPauseIn;

    private Image inOutBlack;

    [Header("�����õ�")]
    [Header("ս����")]
    public PlayerBehaviour playerBehaviour;

    //[Header("ս����")]
    //public OldBoxController OldBoxController;

    private Camera cameraMainInBattle;

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
    /// ��ȡ�������԰�ID
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
    /// ���ض�Ӧ���԰�������
    /// </summary>
    private string LoadLanguageData(string path)
    {
        if (languagePack < languagePackInsideNum)
        {
            return Resources.Load<TextAsset>("TextAssets/LanguagePacks/" + GetLanguageInsideId(languagePack) + "/" + path).text;
        }
        else
        {
            return File.ReadAllText(Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePack - languagePackInsideNum] + "\\" + path + ".txt");
        }
    }

    private void LanguagePackDetection()
    {
        if ((languagePack < 0)
            || (languagePack >= Directory.GetDirectories(Application.dataPath + "\\LanguagePacks").Length + languagePackInsideNum))
        {
            languagePack = 2;
        }
    }

    public void InitializationLoad()
    {
        //����ScriptableObject
        //--------------------------------------------------------------------------------
        PlayerControl = Resources.Load<PlayerControl>("PlayerControl");
        AudioControl = Resources.Load<AudioControl>("AudioControl");
        //InitializationOverworld�ڵ���OverworldControl
        //Initialization�ڵ���ItemControl
        //--------------------------------------------------------------------------------
    }

    /// <summary>
    /// ��ʼ������һ�������
    /// </summary>
    public void Initialization(int lan)
    {
        if (ItemControl == null)
            ItemControl = Resources.Load<ItemControl>("ItemControl");

        if (lan != languagePack)
            languagePack = lan;

        LanguagePackDetection();

        //ItemControl����
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
        //OverworldControl����
        //--------------------------------------------------------------------------------

        OverworldControl.sceneTextsAsset = LoadLanguageData("Overworld\\" + SceneManager.GetActiveScene().name);

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
        //������԰�ȫ���
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
        //BattleControl����
        //--------------------------------------------------------------------------------
        if (BattleControl == null)
            BattleControl = Resources.Load<BattleControl>("BattleControl");

        BattleControl.turnDialogAsset = new List<string>();

        BattleControl.uiText = LoadLanguageData("Battle\\UIBattleText");

        string[] turnSave;
        TextAsset[] textAssets;
        if (languagePack < languagePackInsideNum)
        {
            textAssets = Resources.LoadAll<TextAsset>("TextAssets/LanguagePacks/" + GetLanguageInsideId(languagePack) + "/Battle/Turn");

            turnSave = new string[textAssets.Length];
            for (int i = 0; i < textAssets.Length; i++)
            {
                turnSave[i] = textAssets[i].text;
            }
        }
        else
            turnSave = Directory.GetFiles(Directory.GetDirectories(Application.dataPath + "\\LanguagePacks")[languagePack - languagePackInsideNum] + "\\Battle\\Turn");

        for (int i = 0; i < turnSave.Length; i++)
        {
            string file = turnSave[i];

            if (languagePack < languagePackInsideNum)
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
        if (PlayerPrefs.GetInt("dataNum", 0) >= 0)
            dataNum = PlayerPrefs.GetInt("dataNum", 0);
        else
        {
            PlayerPrefs.SetInt("dataNum", 0);
            dataNum = 0;
        }
        if (dataNum > (SaveController.GetDataNum() - 1))
        {
            dataNum = (SaveController.GetDataNum() - 1);
        }

        instance = this;
        InitializationLoad();
        Initialization(languagePack);

        if (dataNum == -1)
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
    }

    /// <summary>
    /// �����ַ�����ʽ�������ɫ��
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
    /// �����ַ�����ʽ�������ɫ��
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
    }

    /// <summary>
    /// Ӧ��Ĭ�ϼ�λ
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
    /// ����Ĭ��KeyCode��ת��Ϊ��Ϸ�ڼ�λ��
    /// mode:0���� 1���� 2̧��
    /// </summary>
    public bool KeyArrowToControl(KeyCode key, int mode = 0)
    {

        switch (mode)
        {
            case 0:
                switch (key)
                {
                    case KeyCode.DownArrow:
                        return Input.GetKeyDown(OverworldControl.keyCodes[0]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[0]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[0]);

                    case KeyCode.RightArrow:
                        return Input.GetKeyDown(OverworldControl.keyCodes[1]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[1]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[1]);

                    case KeyCode.UpArrow:
                        return Input.GetKeyDown(OverworldControl.keyCodes[2]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[2]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[2]);

                    case KeyCode.LeftArrow:
                        return Input.GetKeyDown(OverworldControl.keyCodes[3]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[3]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[3]);

                    case KeyCode.Z:
                        return Input.GetKeyDown(OverworldControl.keyCodes[4]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[4]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[4]);

                    case KeyCode.X:
                        return Input.GetKeyDown(OverworldControl.keyCodes[5]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[5]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[5]);

                    case KeyCode.C:
                        return Input.GetKeyDown(OverworldControl.keyCodes[6]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[6]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[6]);

                    case KeyCode.V:
                        return Input.GetKeyDown(OverworldControl.keyCodes[7]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[7]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[7]);

                    case KeyCode.F4:
                        return Input.GetKeyDown(OverworldControl.keyCodes[8]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[8]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[8]);

                    case KeyCode.Tab:
                        return Input.GetKeyDown(OverworldControl.keyCodes[9]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[9]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[9]);

                    case KeyCode.Semicolon:
                        return Input.GetKeyDown(OverworldControl.keyCodes[10]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[10]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[10]);

                    case KeyCode.Escape:
                        return Input.GetKeyDown(OverworldControl.keyCodes[11]) || Input.GetKeyDown(OverworldControl.keyCodesBack1[11]) || Input.GetKeyDown(OverworldControl.keyCodesBack2[11]);

                    default:
                        return false;
                }
            case 1:
                switch (key)
                {
                    case KeyCode.DownArrow:
                        return Input.GetKey(OverworldControl.keyCodes[0]) || Input.GetKey(OverworldControl.keyCodesBack1[0]) || Input.GetKey(OverworldControl.keyCodesBack2[0]);

                    case KeyCode.RightArrow:
                        return Input.GetKey(OverworldControl.keyCodes[1]) || Input.GetKey(OverworldControl.keyCodesBack1[1]) || Input.GetKey(OverworldControl.keyCodesBack2[1]);

                    case KeyCode.UpArrow:
                        return Input.GetKey(OverworldControl.keyCodes[2]) || Input.GetKey(OverworldControl.keyCodesBack1[2]) || Input.GetKey(OverworldControl.keyCodesBack2[2]);

                    case KeyCode.LeftArrow:
                        return Input.GetKey(OverworldControl.keyCodes[3]) || Input.GetKey(OverworldControl.keyCodesBack1[3]) || Input.GetKey(OverworldControl.keyCodesBack2[3]);

                    case KeyCode.Z:
                        return Input.GetKey(OverworldControl.keyCodes[4]) || Input.GetKey(OverworldControl.keyCodesBack1[4]) || Input.GetKey(OverworldControl.keyCodesBack2[4]);

                    case KeyCode.X:
                        return Input.GetKey(OverworldControl.keyCodes[5]) || Input.GetKey(OverworldControl.keyCodesBack1[5]) || Input.GetKey(OverworldControl.keyCodesBack2[5]);

                    case KeyCode.C:
                        return Input.GetKey(OverworldControl.keyCodes[6]) || Input.GetKey(OverworldControl.keyCodesBack1[6]) || Input.GetKey(OverworldControl.keyCodesBack2[6]);

                    case KeyCode.V:
                        return Input.GetKey(OverworldControl.keyCodes[7]) || Input.GetKey(OverworldControl.keyCodesBack1[7]) || Input.GetKey(OverworldControl.keyCodesBack2[7]);

                    case KeyCode.F4:
                        return Input.GetKey(OverworldControl.keyCodes[8]) || Input.GetKey(OverworldControl.keyCodesBack1[8]) || Input.GetKey(OverworldControl.keyCodesBack2[8]);

                    case KeyCode.Tab:
                        return Input.GetKey(OverworldControl.keyCodes[9]) || Input.GetKey(OverworldControl.keyCodesBack1[9]) || Input.GetKey(OverworldControl.keyCodesBack2[9]);

                    case KeyCode.Semicolon:
                        return Input.GetKey(OverworldControl.keyCodes[10]) || Input.GetKey(OverworldControl.keyCodesBack1[10]) || Input.GetKey(OverworldControl.keyCodesBack2[10]);

                    case KeyCode.Escape:
                        return Input.GetKey(OverworldControl.keyCodes[11]) || Input.GetKey(OverworldControl.keyCodesBack1[11]) || Input.GetKey(OverworldControl.keyCodesBack2[11]);

                    default:
                        return false;
                }
            case 2:
                switch (key)
                {
                    case KeyCode.DownArrow:
                        return Input.GetKeyUp(OverworldControl.keyCodes[0]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[0]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[0]);

                    case KeyCode.RightArrow:
                        return Input.GetKeyUp(OverworldControl.keyCodes[1]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[1]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[1]);

                    case KeyCode.UpArrow:
                        return Input.GetKeyUp(OverworldControl.keyCodes[2]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[2]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[2]);

                    case KeyCode.LeftArrow:
                        return Input.GetKeyUp(OverworldControl.keyCodes[3]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[3]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[3]);

                    case KeyCode.Z:
                        return Input.GetKeyUp(OverworldControl.keyCodes[4]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[4]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[4]);

                    case KeyCode.X:
                        return Input.GetKeyUp(OverworldControl.keyCodes[5]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[5]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[5]);

                    case KeyCode.C:
                        return Input.GetKeyUp(OverworldControl.keyCodes[6]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[6]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[6]);

                    case KeyCode.V:
                        return Input.GetKeyUp(OverworldControl.keyCodes[7]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[7]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[7]);

                    case KeyCode.F4:
                        return Input.GetKeyUp(OverworldControl.keyCodes[8]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[8]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[8]);

                    case KeyCode.Tab:
                        return Input.GetKeyUp(OverworldControl.keyCodes[9]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[9]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[9]);

                    case KeyCode.Semicolon:
                        return Input.GetKeyUp(OverworldControl.keyCodes[10]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[10]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[10]);

                    case KeyCode.Escape:
                        return Input.GetKeyUp(OverworldControl.keyCodes[11]) || Input.GetKeyUp(OverworldControl.keyCodesBack1[11]) || Input.GetKeyUp(OverworldControl.keyCodesBack2[11]);

                    default:
                        return false;
                }
            default:
                return false;
        }
    }

    /// <summary>
    /// ��/�� SFX
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
    /// ����tab�ĸķֱ���������))
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
    /// �ͷֱ����������׵Ļ���
    /// </summary>
    private int ScreenSet(int y)
    {
        //if (OverworldControl.background)
        //    y = y / 9 * 16;
        //else
        y = y / 3 * 4;
        return y;
    }

    private void SetCanvasFrameSprite(int framePic = 2)//һ��ΪCanvasController.instance.framePic
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
    /// �ֱ�������
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
            // BackpackBehaviour rawImage����ű��п���
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

            // BackpackBehaviour rawImage����ű��п���
            /*
            RectTransform rectTransform = BackpackBehaviour.instance.rawImage.rectTransform;

            rectTransform.offsetMin = new Vector2(107, 0);

            rectTransform.offsetMax = new Vector2(-107, 0);

            rectTransform.localScale = Vector3.one * 0.89f;
            */
            if (BackpackBehaviour.instance != null)
                BackpackBehaviour.instance.SuitResolution();

            //��SetCanvasFrameSprite���趨
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
    /// ���� ������ת��������
    /// banMusic�ǽ���
    /// time>0�ж��� =0��ֱ���г��� <0ʱ����time�ľ���ֵ
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
    /// ����string������ɾȥĩβi���ַ���string
    /// </summary>
    public string SubText(string str, int i = 1)
    {
        str = str.Substring(0, str.Length - i);
        return str;
    }

    /// <summary>
    /// �������һ����λ����Ӣ��
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

    public IEnumerator _LoadItemDataForTurn(List<string> list, TextAsset texter)//�����list �����text
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
    /// ��������(����TextAsset)
    /// </summary>
    public void LoadItemData(List<string> list, TextAsset texter)//�����list �����text
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
    /// ��������(����string)
    /// </summary>
    public void LoadItemData(List<string> list, string texter)//�����list �����text
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
    /// ����ʹ�ñ������ĸ�����
    /// Ȼ���ʹ�� ��true��˳���ѱ���˳��������
    /// Ȼ�����ô��ֻ������
    /// plusText��0���Լ�����
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
        else//ʳ��
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
    /// ת�������ַ�
    /// </summary>
    public List<string> ChangeItemData(List<string> list, bool isData, List<string> ex)
    {
        List<string> newList = new List<string>();
        string text = "";
        bool isXH = false;//����Ƿ��ж����Ҫѭ�����õ������ַ�

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
                    //DebugLogger.Log(list[i] +"/"+ name);
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
    /// ChangeItemData�м��'<''>'���ŵ�Switch���
    /// </summary>
    private string ChangeItemDataSwitch(string text, string texters, bool isData, string name, List<string> ex)
    {
        //DebugLogger.Log(text + "/" + texters + "/" + isData);
        switch (texters)
        {
            case "<playerName>":
                text += PlayerControl.playerName;
                break;

            case "<enter>"://�س�
                text += "\n";
                break;

            case "<stop...>":
                text += ".<stop>.<stop>.<stop>";
                break;

            case "<stop......>":
                text += ".<stop>.<stop>.<stop>.<stop>.<stop>.<stop>";
                break;

            case "<autoFoodFull>":
                text += ItemControl.itemTextMaxData[11].Substring(0, ItemControl.itemTextMaxData[11].Length - 1) + "\n";
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
                /*

                if (IsFrontCharactersMatch("<fx", texters))
                {
                    texters = texters.Substring(4);
                    texters = texters.Substring(0, texters.Length - 1);
                    int q = int.Parse(texters);
                    text += "��" + q + "��";
                    break;
                }

                if (IsFrontCharactersMatch("<image", texters))
                {
                    texters = texters.Substring(7);
                    texters = texters.Substring(0, texters.Length - 1);
                    int q = int.Parse(texters);
                    text += "" + q + "";
                    break;
                }
                */
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
    /// ��������ı��ڵĴ�д��ĸ��תΪȫСд��
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
    /// ��������ı��ڵ�Сд��ĸ��תΪȫ��д��
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
    /// ��������(x,y)������
    /// ����������(xRx��yRy)��(xrx��yry)������R����ȡ�����
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
    /// ����xRx / xrx / O   ����ֿ�
    /// ���û��r��R�Ļ��ͻ᷵��ԭ���ģ��ǳ���ʵ��
    ///
    /// ������ӣ�P/p��ȡ���λ�� ͨ��isYȷ����X����Y
    /// ͨ��xxx + xRx����ʽʵ��һ���̶��ϵĹ̶���
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
                //DebugLogger.LogWarning(text.Substring(2));
                //DebugLogger.Log(RandomFloatChange(text.Substring(2), origin, isY, origin));
                return RandomFloatChange(text.Substring(2), origin, isY, origin);
            }
            else
                return origin;
        }
    }

    /*֮�����������ʶ���ⲻ��һ��ǿ��ת�����¶���
     *
    /// <summary>
    /// ��������(x,y)������
    ///
    /// </summary>
    public Vector3 StringVector2ToRealVector3(string stringVector2)
    {
        Vector2 endVector2 = StringVector2ToRealVector2(stringVector2);
        return new Vector3(endVector2.x, endVector2.y);
    }
    */

    /// <summary>
    /// ��������(r,g,b,a)������
    /// ͬ��֧�������
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
    /// ��num1��num2֮���ж� ���Ϻ󷵻�num2.���򴫻�num1.
    /// </summary>
    public float JudgmentNumber(bool isGreater, float num1, float num2)
    {
        if (isGreater)
        {
            if (num1 > num2)
                num1 = num2;
        }
        else
        {
            if (num1 < num2)
                num1 = num2;
        }
        return num1;
    }

    /// <summary>
    /// ����Item����
    /// </summary>
    private void ItemClassificatio()
    {
        ItemControl.itemFoods.Clear();
        ItemControl.itemArms.Clear();
        ItemControl.itemArmors.Clear();
        ItemControl.itemOthers.Clear();
        for (int i = 0; i < ItemControl.itemMax.Count; i++)//����Ʒ��
        {
            string countText = ItemControl.itemMax[i];
            string[] text = new string[4];
            int texti = 0;
            for (int k = 0; k < countText.Length; k++)//����Ʒ���� Ѱ��\��
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
    /// ItemClassificatio��һ����void
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
    /// ��� '\'�ַ�Ȼ��ָ��ı�����List
    /// ��������string
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
    /// ��� '\'�ַ�Ȼ��ָ��ı�����List
    /// ����һ��string
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
    /// ��⵽��һ��'\'�ַ��ʹ���
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
    /// �������һ��'\'�ַ��ʹ�������ѡ���ӵ����� ; �š�
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
    /// ������Ϸ���ı���ȡ
    /// �����������Ʒ����ı����ı�
    /// ����һ�� ���ڶ���)
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
    /// ������Ϸ���ı���ȡ
    /// �����������Ʒ�������ͬ�����ı����ı�
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
    /// ���list��ǰ�����ַ��Ƿ��봫���string screen��ͬ��
    /// ����ͬ��ָ��ı�����List
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
    /// �ٷ����ı���
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
        int plusNum = original.Count;
        for (int i = 0; i < original.Count; i++)
        {
            if (original[i] != 0)
            {
                newList.Add(original[i]);
                plusNum--;
            }
        }
        for (int i = 0; i < plusNum; i++)
        {
            newList.Add(0);
        }

        return newList;
    }

    /// <summary>
    /// ͨ��Id��ȡItem��Ϣ��
    /// type��Foods Arms Armors Others Auto
    /// num��0���԰�����
    ///     1/2��data1/2.
    ///     �������.
    ///     Arm��Armorֻ��1
    /// </summary>
    public string ItemIdGetName(int id, string type, int num)
    {
        int realId;
        List<string> list;
        string idName;//��ȡ�������
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
        if (num == 0)//��ȡ���԰��ڵ�����
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
            subText = list[realId + num];
        }
        return subText;
    }

    /// <summary>
    /// ͨ��Id��ȡItem�����ݣ�HP��ATK�ȣ���
    /// type��Foods Arms Armors Others Auto
    /// justId:���Ļ������ +xxHP/AT/DF����Ϣ
    /// </summary>
    public string ItemIdGetData(int id, string type, bool notJustId = false)
    {
        int realId;
        List<string> list;
        string idData;//��ȡ�������
        switch (type)
        {
            case "Foods":
                list = ItemControl.itemFoods;
                realId = id * 3 - 1;

                if (notJustId)
                {
                    idData = list[realId] + "HP";
                    if (int.Parse(list[realId]) > 0)
                        idData = "+" + idData;
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
                        idData = "+" + idData;
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
                        idData = "+" + idData;
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
                            idData = "+" + idData;
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
                            idData = "+" + idData;
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
                            idData = "+" + idData;
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
    /// ʶ��0�󴫳�
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
    /// ͨ����Ʒ�������Ƹ㵽����id.
    /// type��Foods Arms Armors Others
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
    /// ��List<Int>����⵽�յķ���
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
    /// �����ȡ-1��1
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
    /// ������������������1/-1��
    /// ��0��1��
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
    /// ��һ��ָ�����ȣ�Ȼ����ÿո����ԭ�ַ���
    /// </summary>
    /// <param name="origin">ԭ�ַ���</param>
    /// <param name="length">���س���</param>
    /// <returns></returns>
    public string FillString(string origin, int length)
    {
        int forNum = length - origin.Length;
        for (int i = 0; i < forNum; i++)
        {
            origin += " ";
        }
        return origin;
    }

    public string GetRealTime(int second)
    {
        int hr;
        int min;
        int sec;
        if (second < 0)
            second = 0;
        sec = second % 60;
        second = second - sec;
        second /= 60;
        min = second % 60;
        second -= min;
        hr = second / 60;
        string shiS, fenS;
        if (hr < 10)
            shiS = "0" + hr;
        else shiS = "" + hr;
        if (min < 10)
            fenS = "0" + min;
        else fenS = "" + min;

        return (shiS + ":" + fenS);
    }

    public string StringRemover(string inputString, int startIndex, int endIndex, string add = "")
    {
        if (startIndex < 0 || endIndex >= inputString.Length || startIndex > endIndex)
        {
            DebugLogger.Log("��Ч����ʼ�ͽ���λ��", DebugLogger.Type.err);
            return inputString;
        }

        string part1 = inputString.Substring(0, startIndex); // �ӿ�ͷ��A֮ǰ�Ĳ���
        string part2 = inputString.Substring(endIndex + 1); // ��B֮���ַ���ĩβ�Ĳ���
        DebugLogger.Log(inputString.Substring(startIndex + 1));
        string result = part1 + add + part2; // �ϲ�������
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
            DebugLogger.Log("Debug�޵�ģʽ�ѽ�����Ѫ���ָ�", DebugLogger.Type.nor, "#FF0000");
        }

    }
    
    /// <summary>
    /// ���������е�
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
    /// ��������������������
    /// </summary>
    public Vector3 RandomPointOnSphereSurface(float minRandomValue, float maxRandomValue, float sphereRadius, Vector3 sphereCenter)
    {
        Vector3 randomDirection = UnityEngine.Random.onUnitSphere;

        randomDirection *= sphereRadius;

        Vector3 randomPointOnSphere = sphereCenter + randomDirection;

        return randomPointOnSphere;
    }
}