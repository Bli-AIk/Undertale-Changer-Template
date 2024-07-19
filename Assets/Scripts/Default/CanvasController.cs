using DG.Tweening;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// UI interface, including: FPS display Long press ESC to exit Setup interface
/// </summary>
public class CanvasController : MonoBehaviour
{
    public int framePic;

    public static CanvasController instance;
    public bool openTurn;
    //Enemy turn cannot be opened
    public TextMeshProUGUI fps;
    public Image frame;
    private Image exitImage;
    private float clock;
    public List<Sprite> sprites;
    public List<Vector2> sizes;

    public Image setting;
    private Image settingSoul;
    private TextMeshProUGUI settingTmp, settingTmpSon, settingTmpUnder;
    public RenderMode renderMode;

    private int settingSelect, settingSelectMax;
    //Currently Max is only used to configure language packs.

    [HideInInspector]
    public int settingLevel;
    //Toggle Layer 0 Defaults Layer 1 Key Settings Layer 2 Language Pack Configuration

    private int controlPage, controlSelect;
    //Page is page flip Select is switching primary and secondary key setting
    private bool isSettingName;
    //Whether to check
    private float saveVolume;
    private bool isSettingControl;

    [HideInInspector]
    public bool freeze;
    //preventing the whole thing from happening during cut scenes

    private Canvas canvas;
    private TypeWritter[] typeWritters;
    //Store the typewriter to pause the coprogramming

    public Animator animator;

    public float animSpeed = 0.25f;
    private void Awake()
    {
        instance = this;

        animator = GetComponent<Animator>();
        canvas = GetComponent<Canvas>();
        fps = transform.Find("FPS Text").GetComponent<TextMeshProUGUI>();
        exitImage = transform.Find("Exit Image").GetComponent<Image>();

        settingTmp = transform.Find("Setting/Setting Text").GetComponent<TextMeshProUGUI>();
        settingTmpSon = settingTmp.transform.Find("Setting Son").GetComponent<TextMeshProUGUI>();
        settingSoul = settingTmp.transform.Find("Soul").GetComponent<Image>();
        settingTmpUnder = settingTmp.transform.Find("Setting Under").GetComponent<TextMeshProUGUI>();
        setting = transform.Find("Setting").GetComponent<Image>();

        frame = transform.Find("Frame").GetComponent<Image>();
        typeWritters = (TypeWritter[])Resources.FindObjectsOfTypeAll(typeof(TypeWritter));
    }

    public void Start()
    {
        settingLevel = 0;
        setting.rectTransform.sizeDelta = new Vector2(0, setting.rectTransform.sizeDelta.y);
        settingTmp.color = Color.white;
        settingTmp.rectTransform.anchoredPosition = new Vector2(-610, 140);

        freeze = false;

        canvas.renderMode = renderMode;

        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = Camera.main;
        }

        m_LastUpdateShowTime = Time.realtimeSinceStartup;

        SettingText();
    }

    private void SettingText(bool OnlySetSon = false, bool isSetting = false)
    {
        switch (settingLevel)
        {
            case 0:
                if (!OnlySetSon)
                {
                    if (!isSetting)
                    {
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Setting") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingMainVolume") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingControl") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingFullScreen") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingResolving") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingSFX") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingFPS") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingBackMenu") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingBackGame");
                    }
                    else
                    {
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Setting") + "\n<color=yellow>" +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingMainVolume") + "</color>\n" +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingControl") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingFullScreen") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingResolving") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingSFX") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingFPS") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingBackMenu") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingBackGame");
                    }
                }
                if (!isSetting)
                    settingTmpSon.text = ((int)(MainControl.instance.OverworldControl.mainVolume * 100)).ToString() + "%\n\n" + OpenOrClose(MainControl.instance.OverworldControl.fullScreen) + '\n' +
                    MainControl.instance.OverworldControl.resolution.x + '×' + MainControl.instance.OverworldControl.resolution.y + '\n' + OpenOrClose(MainControl.instance.OverworldControl.noSFX) + '\n' + OpenOrClose(MainControl.instance.OverworldControl.openFPS);
                else settingTmpSon.text = "<color=yellow>" + ((int)(MainControl.instance.OverworldControl.mainVolume * 100)).ToString() + "%</color>\n\n" + OpenOrClose(MainControl.instance.OverworldControl.fullScreen) + '\n' +
                    MainControl.instance.OverworldControl.resolution.x + '×' + MainControl.instance.OverworldControl.resolution.y + '\n' + OpenOrClose(MainControl.instance.OverworldControl.noSFX) + '\n' + OpenOrClose(MainControl.instance.OverworldControl.openFPS);

                break;

            case 1:
                List<string> strings = new List<string>();

                for (int i = 0; i < 6; i++)
                {
                    if (isSetting && i == settingSelect)
                        strings.Add("<color=yellow>");
                    else
                        strings.Add("");
                }
                switch (controlPage)
                {
                    case 0:
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Control") + "</color>" + '\n' +
                       strings[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlDown") + "</color>" + '\n' +
                       strings[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlRight") + "</color>" + '\n' +
                       strings[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlUp") + "</color>" + '\n' +
                       strings[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlLeft") + "</color>" + '\n' +
                       strings[4] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlZ") + "</color>" + '\n' +
                       strings[5] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlX") + "</color>" + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "PageDown") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlDefault") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Back");

                        settingTmpSon.text = "";
                        for (int i = 0; i < 6; i++)
                        {
                            if (isSetting && i == settingSelect)
                            {
                                settingTmpSon.text += "<color=yellow>";
                            }
                            switch (controlSelect)
                            {
                                case 0:
                                    settingTmpSon.text += MainControl.instance.OverworldControl.keyCodes[i] + "</color>\n";
                                    break;

                                case 1:
                                    settingTmpSon.text += MainControl.instance.OverworldControl.keyCodesBack1[i] + "</color>\n";
                                    break;

                                case 2:
                                    settingTmpSon.text += MainControl.instance.OverworldControl.keyCodesBack2[i] + "</color>\n";
                                    break;
                            }
                        }

                        break;

                    case 1:
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Control") + '\n' +
                       strings[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlC") + "</color>" + '\n' +
                       strings[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlV") + "</color>" + '\n' +
                       strings[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlF4") + "</color>" + '\n' +
                       strings[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlTab") + "</color>" + '\n' +
                       strings[4] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlSemicolon") + "</color>" + '\n' +
                       strings[5] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlEsc") + "</color>" + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "PageUp") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlDefault") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Back");
                        settingTmpSon.text = "";
                        for (int i = 6; i < 12; i++)
                        {
                            if (isSetting && i - 6 == settingSelect)
                            {
                                settingTmpSon.text += "<color=yellow>";
                            }
                            switch (controlSelect)
                            {
                                case 0:
                                    settingTmpSon.text += MainControl.instance.OverworldControl.keyCodes[i] + "</color>\n";
                                    break;

                                case 1:
                                    settingTmpSon.text += MainControl.instance.OverworldControl.keyCodesBack1[i] + "</color>\n";
                                    break;

                                case 2:
                                    settingTmpSon.text += MainControl.instance.OverworldControl.keyCodesBack2[i] + "</color>\n";
                                    break;
                            }
                        }

                        break;
                }

                break;

            case 2:
                string pathStringSaver = "";

                if (isSetting)
                {
                    MainControl.instance.Initialization(settingSelect);
                }
                if (!OnlySetSon)
                    settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "LanguagePack") + '\n';
                settingTmpSon.text = "";
                settingSelectMax = 0;
                int settingSelectBack = settingSelect;
                if (OnlySetSon)
                    settingSelect = MainControl.instance.languagePack;

                for (int i = 0; i < MainControl.instance.languagePackInsideNum; i++)
                //Built-in package information
                {
                    string pathString = "TextAssets/LanguagePacks/" + MainControl.instance.GetLanguageInsideId(i);

                    if (settingSelectMax == settingSelect)
                    {
                        pathStringSaver = pathString;
                    }
                    settingSelectMax++;

                    if (!OnlySetSon)
                        settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", false) + '\n';
                }
                foreach (string pathString in Directory.GetDirectories(Application.dataPath + "\\LanguagePacks"))
                {
                    if (settingSelectMax == settingSelect)
                        pathStringSaver = pathString;
                    settingSelectMax++;
                    if (!OnlySetSon)
                        settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", true) + '\n';
                }
                if (!OnlySetSon)
                    settingTmp.text += MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Back");

                settingTmpUnder.text = GetLanguagePacksName(pathStringSaver, "LanguagePackInformation", settingSelect >= MainControl.instance.languagePackInsideNum) + '\n' + GetLanguagePacksName(pathStringSaver, "LanguagePackAuthor", settingSelect >= MainControl.instance.languagePackInsideNum);

                settingSelect = settingSelectBack;
                break;
        }
    }

    /// <summary>
    /// Get package information.
    /// return returnString
    /// </summary>
    private string GetLanguagePacksName(string pathString, string returnString, bool isOutSide)
    {
        List<string> strings = new List<string>();
        MainControl.instance.LoadItemData(strings, ReadFile(pathString + "\\LanguagePackInformation", isOutSide));
        strings = MainControl.instance.ChangeItemData(strings, true, new List<string>());
        return MainControl.instance.ScreenMaxToOneSon(strings, returnString);
    }

    private string ReadFile(string PathName, bool isOutSide)
    {
        string strs;
        if (!isOutSide)
            strs = Resources.Load<TextAsset>(PathName).text;
        else
            strs = File.ReadAllText(PathName + ".txt");
        return strs;
    }

    /// <summary>
    /// Returns the on/off text.
    /// </summary>
    private string OpenOrClose(bool booler)
    {
        if (booler)
            return MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Open");
        else
            return MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "Close");
    }

    private void Update()
    {
        if (freeze)
            return;
        if (MainControl.instance.OverworldControl.openFPS)
            fps.text = FPSFlash(fps.text);
        else
            fps.text = "";

        if (MainControl.instance.KeyArrowToControl(KeyCode.Escape, 1))
        {
            if (exitImage.color.a < 1)
                exitImage.color += Time.deltaTime * Color.white;
            if (clock < 3)
            {
                exitImage.sprite = sprites[(int)clock];
                exitImage.rectTransform.sizeDelta = sizes[(int)clock];
                clock += Time.deltaTime;
            }
            else Application.Quit();
        }
        if (MainControl.instance.KeyArrowToControl(KeyCode.Escape, 2))
        {
            clock = 0;
            exitImage.color = new Color(1, 1, 1, 0);
        }

        //Setup menu
        if (isSettingControl)
        {
            SettingText(false, true);
            if (SettingControl() != KeyCode.None)
            {
                int j = 0;
                switch (controlPage)
                {
                    case 0:
                        j = 0;
                        goto default;
                    case 1:
                        j = 6;
                        goto default;
                    default:
                        KeyCode origin = KeyCode.None;

                        switch (controlSelect)
                        {
                            case 0:
                                origin = MainControl.instance.OverworldControl.keyCodes[settingSelect + j];
                                MainControl.instance.OverworldControl.keyCodes[settingSelect + j] = SettingControl();
                                goto default;
                            case 1:
                                origin = MainControl.instance.OverworldControl.keyCodesBack1[settingSelect + j];
                                MainControl.instance.OverworldControl.keyCodesBack1[settingSelect + j] = SettingControl();
                                goto default;
                            case 2:
                                origin = MainControl.instance.OverworldControl.keyCodesBack2[settingSelect + j];
                                MainControl.instance.OverworldControl.keyCodesBack2[settingSelect + j] = SettingControl();
                                goto default;
                            default:
                                List<KeyCode> keycodes = new List<KeyCode>()
                                {
                                    MainControl.instance.OverworldControl.keyCodes[settingSelect + j],
                                    MainControl.instance.OverworldControl.keyCodesBack1[settingSelect + j],
                                    MainControl.instance.OverworldControl.keyCodesBack2[settingSelect + j]
                                };
                                for (int i = 0; i < MainControl.instance.OverworldControl.keyCodes.Count; i++)
                                {
                                    if (MainControl.instance.OverworldControl.keyCodes[i] == keycodes[controlSelect] && i != settingSelect + j)
                                    {
                                        MainControl.instance.OverworldControl.keyCodes[i] = origin;
                                        break;
                                    }
                                }
                                for (int i = 0; i < MainControl.instance.OverworldControl.keyCodesBack1.Count; i++)
                                {
                                    if (MainControl.instance.OverworldControl.keyCodesBack1[i] == keycodes[controlSelect] && i != settingSelect + j)
                                    {
                                        MainControl.instance.OverworldControl.keyCodesBack1[i] = origin;
                                        break;
                                    }
                                }
                                for (int i = 0; i < MainControl.instance.OverworldControl.keyCodesBack2.Count; i++)
                                {
                                    if (MainControl.instance.OverworldControl.keyCodesBack2[i] == keycodes[controlSelect] && i != settingSelect + j)
                                    {
                                        MainControl.instance.OverworldControl.keyCodesBack2[i] = origin;
                                        break;
                                    }
                                }
                                SettingText();
                                break;
                        }

                        break;
                }
                isSettingControl = false;
            }

            return;
        }

        if ((openTurn && TurnController.instance != null && TurnController.instance.isMyTurn) || !openTurn)
        {
            if (SceneManager.GetActiveScene().name != "Story" && MainControl.instance.KeyArrowToControl(KeyCode.V) && !MainControl.instance.OverworldControl.isSetting && !MainControl.instance.blacking)
            {
                foreach (TypeWritter typeWritter in typeWritters)
                {
                    typeWritter.TypePause(true);
                }

                InSetting();
            }
        }
        if (!MainControl.instance.OverworldControl.isSetting)
            return;

        settingSoul.rectTransform.anchoredPosition = new Vector2(-325f, -28f + settingSelect * -37);

        if (settingTmp.rectTransform.anchoredPosition.x > 125)
        {
            switch (settingLevel)
            {
                case 0:
                    if (!isSettingName)
                    {
                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            settingSelect++;
                            if (settingSelect > 7)
                                settingSelect = 0;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            settingSelect--;
                            if (settingSelect < 0)
                                settingSelect = 7;
                        }
                    }
                    else
                    {
                        if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            if (MainControl.instance.OverworldControl.mainVolume > 0)
                            {
                                AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                                MainControl.instance.OverworldControl.mainVolume -= 0.01f;
                                AudioListener.volume = MainControl.instance.OverworldControl.mainVolume;
                            }
                            SettingText(false, true);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            if (MainControl.instance.OverworldControl.mainVolume < 1)
                            {
                                AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                                MainControl.instance.OverworldControl.mainVolume += 0.01f;
                                AudioListener.volume = MainControl.instance.OverworldControl.mainVolume;
                            }
                            SettingText(false, true);
                        }
                    }

                    if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                    {
                        AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                        if (!isSettingName)
                            switch (settingSelect)
                            {
                                case 0:
                                    saveVolume = MainControl.instance.OverworldControl.mainVolume;
                                    isSettingName = true;
                                    SettingText(false, true);
                                    break;

                                case 1:
                                    settingLevel = 1;
                                    SettingText();
                                    settingSelect = 0;
                                    break;

                                case 2:
                                    MainControl.instance.OverworldControl.fullScreen = !MainControl.instance.OverworldControl.fullScreen;
                                    MainControl.instance.SetResolution(MainControl.instance.OverworldControl.resolutionLevel);

                                    goto default;
                                case 3:
                                    MainControl.instance.ChangeResolution();
                                    goto default;
                                case 4:
                                    MainControl.instance.OverworldControl.noSFX = !MainControl.instance.OverworldControl.noSFX;
                                    MainControl.instance.FindAndChangeAllSFX(MainControl.instance.OverworldControl.noSFX);
                                    PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.instance.OverworldControl.noSFX));
                                    goto default;
                                case 5:
                                    MainControl.instance.OverworldControl.openFPS = !MainControl.instance.OverworldControl.openFPS;
                                    goto default;
                                case 6:
                                    if (SceneManager.GetActiveScene().name == "Rename")
                                        return;
                                    else if (SceneManager.GetActiveScene().name == "Menu")
                                        goto case 7;
                                    else
                                    {
                                        MainControl.instance.OutBlack("Menu", Color.black, true, animSpeed);
                                        CloseSetting();
                                        freeze = true;
                                        break;
                                    }
                                case 7:
                                    ExitSetting();
                                    break;

                                default:
                                    SettingText();
                                    break;
                            }
                        else
                        {
                            SettingText();
                            isSettingName = false;
                        }
                    }
                    else if (SceneManager.GetActiveScene().name != "Story" && (MainControl.instance.KeyArrowToControl(KeyCode.X) || MainControl.instance.KeyArrowToControl(KeyCode.V)))
                    {
                        if (!isSettingName)
                        {
                            ExitSetting();
                        }
                        else
                        {
                            MainControl.instance.OverworldControl.mainVolume = saveVolume;
                            AudioListener.volume = MainControl.instance.OverworldControl.mainVolume;
                            SettingText();
                            isSettingName = false;
                        }
                    }
                    else if (MainControl.instance.KeyArrowToControl(KeyCode.C))
                    {
                        if (!isSettingName)
                        {
                            if (settingSelect == 2)
                            {
                                AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                                if ((int)MainControl.instance.OverworldControl.vsyncMode < 2)
                                    MainControl.instance.OverworldControl.vsyncMode++;
                                else
                                    MainControl.instance.OverworldControl.vsyncMode = OverworldControl.VSyncMode.DonNotSync;

                                PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(MainControl.instance.OverworldControl.vsyncMode));
                            }
                            else if (settingSelect == 3)
                            {
                                AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                                MainControl.instance.OverworldControl.hdResolution = !MainControl.instance.OverworldControl.hdResolution;
                                MainControl.instance.ChangeResolution();
                                SettingText();
                                PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(MainControl.instance.OverworldControl.hdResolution));
                            }
                        }
                    }

                    string textForUnder = "";
                    switch (settingSelect)
                    {
                        case 0:
                            textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingMainVolumeTip");
                            break;

                        case 1:
                            textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingControlTip");
                            break;

                        case 2:
                            string vsyncModeAdd;
                            switch (MainControl.instance.OverworldControl.vsyncMode)
                            {
                                case OverworldControl.VSyncMode.DonNotSync:
                                    vsyncModeAdd = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "VSyncNone");
                                    break;

                                case OverworldControl.VSyncMode.SyncToRefreshRate:
                                    vsyncModeAdd = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "VSyncFull");
                                    break;

                                case OverworldControl.VSyncMode.HalfSync:
                                    vsyncModeAdd = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "VSyncHalf");
                                    break;

                                default:
                                    goto case OverworldControl.VSyncMode.DonNotSync;
                            }

                            if (!MainControl.instance.OverworldControl.fullScreen)
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingFullScreenTipOpen") + "\n" + vsyncModeAdd;
                            else
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingFullScreenTipClose") + "\n" + vsyncModeAdd;
                            break;

                        case 3:
                            if (!MainControl.instance.OverworldControl.hdResolution)
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingResolvingTip");
                            else
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingResolvingTipHD");

                            break;

                        case 4:
                            textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingSFXTip");
                            break;

                        case 5:
                            textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingFPSTip");
                            break;

                        case 6:
                            textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingBackMenuTip");
                            break;

                        case 7:
                            textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "SettingBackGameTip");
                            break;
                    }
                    settingTmpUnder.text = textForUnder;
                    break;

                case 1:
                    if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                    {
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        settingSelect++;
                        if (settingSelect > 8)
                            settingSelect = 0;
                    }
                    else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                    {
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        settingSelect--;
                        if (settingSelect < 0)
                            settingSelect = 8;
                    }
                    if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                    {
                        AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                        if (settingSelect < 6)
                            isSettingControl = true;
                        else if (settingSelect == 6)
                            switch (controlPage)
                            {
                                case 0:
                                    controlPage = 1;
                                    break;

                                case 1:
                                    controlPage = 0;
                                    break;
                            }
                        else if (settingSelect == 7)
                            MainControl.instance.ApplyDefaultControl();
                        else
                        {
                            settingLevel = 0;
                            settingSelect = 0;

                            SettingText();
                            return;
                        }

                        SettingText(false, true);
                    }
                    else if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                    {
                        settingLevel = 0;
                        settingSelect = 0;

                        SettingText();
                        return;
                    }
                    else if (MainControl.instance.KeyArrowToControl(KeyCode.C))
                    {
                        AudioController.instance.GetFx(3, MainControl.instance.AudioControl.fxClipUI);
                        if (controlSelect < 2)
                            controlSelect++;
                        else controlSelect = 0;

                        SettingText();
                    }
                    settingTmpUnder.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlUnder" + controlSelect);

                    break;

                case 2:
                    if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                    {
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        settingSelect++;
                        if (settingSelect > settingSelectMax)
                            settingSelect = 0;
                    }
                    else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                    {
                        AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                        settingSelect--;
                        if (settingSelect < 0)
                            settingSelect = settingSelectMax;
                    }
                    if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                    {
                        if (settingSelect != settingSelectMax)
                        {
                            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                            SettingText(false, true);
                            MainControl.instance.languagePack = settingSelect;
                        }
                        else
                        {
                            ExitSetting(true);
                        }
                    }
                    else if (SceneManager.GetActiveScene().name != "Story" && (MainControl.instance.KeyArrowToControl(KeyCode.X) || MainControl.instance.KeyArrowToControl(KeyCode.V)))
                    {
                        ExitSetting(true);
                        return;
                    }
                    break;
            }
        }
    }

    private void CloseSetting(bool isLan = false)
    {
        MainControl.instance.OverworldControl.isSetting = false;
        foreach (TypeWritter typeWritter in typeWritters)
        {
            typeWritter.TypePause(false);
        }
        if (isLan)
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private KeyCode SettingControl()
    {
        if (Input.anyKeyDown)
        {
            foreach (KeyCode item in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                    continue;
                if (Input.GetKeyDown(item))
                {
                    return item;
                }
            }
        }
        return KeyCode.None;
    }

    public void InSetting()
    {
        MainControl.instance.OverworldControl.isSetting = true;
        DOTween.To(() => setting.rectTransform.sizeDelta, x => setting.rectTransform.sizeDelta = x, new Vector2(6000, setting.rectTransform.sizeDelta.y), animSpeed).SetEase(Ease.InCirc);
        settingTmp.DOColor(Color.white, 1).SetEase(Ease.InCubic);
        DOTween.To(() => settingTmp.rectTransform.anchoredPosition, x => settingTmp.rectTransform.anchoredPosition = x, new Vector2(140, 140), animSpeed + 0.25f).SetEase(Ease.OutCubic);
        settingSelect = 0;
        settingTmpUnder.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlEggshell");
        SettingText();
        if (settingLevel == 2)
            SettingText(true);
    }

    private void ExitSetting(bool isLan = false)
    {
        settingLevel = 0;
        DOTween.To(() => setting.rectTransform.sizeDelta, x => setting.rectTransform.sizeDelta = x, new Vector2(0, setting.rectTransform.sizeDelta.y), animSpeed).SetEase(Ease.OutCirc);
        settingTmp.DOColor(Color.white, 0).SetEase(Ease.OutCubic);
        DOTween.To(() => settingTmp.rectTransform.anchoredPosition, x => settingTmp.rectTransform.anchoredPosition = x, new Vector2(-610, 140), animSpeed + 0.25f).SetEase(Ease.InSine).OnKill(() => CloseSetting(isLan));
        settingTmpUnder.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverworldControl.settingSave, "ControlEggshell");
    }

    private float m_LastUpdateShowTime = 0f;
    //The last time the frame rate was updated.
    private float m_UpdateShowDeltaTime = 0.2f;
    //Interval for updating the frame rate.
    private int m_FrameUpdate = 0;
    //frames.
    private float m_FPS = 0;
    //Frame rate

    private string FPSFlash(string origin)
    {
        m_FrameUpdate++;
        if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
        {
            //FPS = total number of frames in a certain period of time / a certain period of time
            m_FPS = m_FrameUpdate / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
            m_FrameUpdate = 0;
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
            return ((int)m_FPS).ToString();
        }
        else return origin;
    }

    /// <summary>
    /// Anim call
    /// </summary>
    public void AnimSetHeartPos()
    {
        Vector2 uiPos = WorldToUgui(MainControl.instance.OverworldControl.playerDeadPos);
        transform.Find("Heart").GetComponent<RectTransform>().anchoredPosition = uiPos;
    }

    public Vector2 WorldToUgui(Vector3 position)
    {
        RectTransform canvasRectTransform = GetComponent<RectTransform>();
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);
        //World coordinates converted to screen coordinates
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        screenPoint -= screenSize / 2;
        //Transform screen coordinates to center of screen as origin
        Vector2 anchorPos = screenPoint / screenSize * canvasRectTransform.sizeDelta;
        //scaling to get UGUI coordinates

        return anchorPos;
    }

    public void AnimSetHeartRed(int isRed)
    {
        if (Convert.ToBoolean(isRed))
            transform.Find("Heart").GetComponent<Image>().color = Color.red;
        else
            transform.Find("Heart").GetComponent<Image>().color = Color.clear;
    }

    public void AnimHeartGo()
    {
        RectTransform i = transform.Find("Heart").GetComponent<RectTransform>();
        Image j = i.GetComponent<Image>();
        j.DOColor(new Color(j.color.r, j.color.g, j.color.b, 0), animSpeed).SetEase(Ease.Linear);
        DOTween.To(() => i.anchoredPosition, x => i.anchoredPosition = x, new Vector2(-330, -250), 1.5f).SetEase(Ease.OutCirc).OnKill(() => AnimOpen());
    }

    public void PlayFX(int i)
    {
        AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipUI);
    }

    private void AnimOpen()
    {
        animator.SetBool("Open", false);
        MainControl.instance.OutBlack("Battle", Color.black, false, -0.5f);
    }
}
