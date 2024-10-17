using System;
using System.Collections.Generic;
using System.IO;
using DG.Tweening;
using TMPro;
using UCT.Battle;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UCT.Global.UI
{
    /// <summary>
    /// UI界面，包括：FPS显示 长按ESC退出 设置界面
    /// </summary>
    public class CanvasController : MonoBehaviour
    {
        public int framePic;

        public static CanvasController instance;
        public bool openTurn;//敌人回合不能开
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

        private int settingSelect, settingSelectMax;//目前 Max仅用于配置语言包

        [HideInInspector]
        public int settingLevel;//切换层级 0层默认 1层按键设置 2层语言包配置

        private int controlPage, controlSelect;//Page是翻页 Select是切换主次按键设置
        private bool isSettingName;//是否选中
        private float saveVolume;
        private bool isSettingControl;

        [HideInInspector]
        public bool freeze;//防止切场景时整事儿

        private Canvas canvas;
        private TypeWritter[] typeWritters;//存储打字机以暂停协程

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
                            settingTmp.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Setting") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingMainVolume") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingControl") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingFullScreen") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingResolving") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingSFX") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingFPS") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingBackMenu") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingBackGame");
                        }
                        else
                        {
                            settingTmp.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Setting") + "\n<color=yellow>" +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingMainVolume") + "</color>\n" +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingControl") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingFullScreen") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingResolving") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingSFX") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingFPS") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingBackMenu") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingBackGame");
                        }
                    }
                    if (!isSetting)
                        settingTmpSon.text = ((int)(MainControl.Instance.OverworldControl.mainVolume * 100)) + "%\n\n" + OpenOrClose(MainControl.Instance.OverworldControl.fullScreen) + '\n' +
                                             MainControl.Instance.OverworldControl.resolution.x + '×' + MainControl.Instance.OverworldControl.resolution.y + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.noSFX) + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.openFPS);
                    else settingTmpSon.text = "<color=yellow>" + ((int)(MainControl.Instance.OverworldControl.mainVolume * 100)) + "%</color>\n\n" + OpenOrClose(MainControl.Instance.OverworldControl.fullScreen) + '\n' +
                                              MainControl.Instance.OverworldControl.resolution.x + '×' + MainControl.Instance.OverworldControl.resolution.y + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.noSFX) + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.openFPS);

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
                            settingTmp.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Control") + "</color>" + '\n' +
                                              strings[0] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlDown") + "</color>" + '\n' +
                                              strings[1] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlRight") + "</color>" + '\n' +
                                              strings[2] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlUp") + "</color>" + '\n' +
                                              strings[3] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlLeft") + "</color>" + '\n' +
                                              strings[4] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlZ") + "</color>" + '\n' +
                                              strings[5] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlX") + "</color>" + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "PageDown") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlDefault") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Back");

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
                                        settingTmpSon.text += MainControl.Instance.OverworldControl.keyCodes[i] + "</color>\n";
                                        break;

                                    case 1:
                                        settingTmpSon.text += MainControl.Instance.OverworldControl.keyCodesBack1[i] + "</color>\n";
                                        break;

                                    case 2:
                                        settingTmpSon.text += MainControl.Instance.OverworldControl.keyCodesBack2[i] + "</color>\n";
                                        break;
                                }
                            }

                            break;

                        case 1:
                            settingTmp.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Control") + '\n' +
                                              strings[0] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlC") + "</color>" + '\n' +
                                              strings[1] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlV") + "</color>" + '\n' +
                                              strings[2] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlF4") + "</color>" + '\n' +
                                              strings[3] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlTab") + "</color>" + '\n' +
                                              strings[4] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlSemicolon") + "</color>" + '\n' +
                                              strings[5] + MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlEsc") + "</color>" + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "PageUp") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlDefault") + '\n' +
                                              MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Back");
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
                                        settingTmpSon.text += MainControl.Instance.OverworldControl.keyCodes[i] + "</color>\n";
                                        break;

                                    case 1:
                                        settingTmpSon.text += MainControl.Instance.OverworldControl.keyCodesBack1[i] + "</color>\n";
                                        break;

                                    case 2:
                                        settingTmpSon.text += MainControl.Instance.OverworldControl.keyCodesBack2[i] + "</color>\n";
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
                        MainControl.Instance.Initialization(settingSelect);
                    }
                    if (!OnlySetSon)
                        settingTmp.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "LanguagePack") + '\n';
                    settingTmpSon.text = "";
                    settingSelectMax = 0;
                    int settingSelectBack = settingSelect;
                    if (OnlySetSon)
                        settingSelect = MainControl.Instance.languagePack;

                    for (int i = 0; i < MainControl.LanguagePackInsideNumber; i++) //内置包信息
                    {
                        string pathString = "TextAssets/LanguagePacks/" + MainControl.Instance.GetLanguageInsideId(i);

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
                        settingTmp.text += MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Back");

                    settingTmpUnder.text = GetLanguagePacksName(pathStringSaver, "LanguagePackInformation", settingSelect >= MainControl.LanguagePackInsideNumber) + '\n' + GetLanguagePacksName(pathStringSaver, "LanguagePackAuthor", settingSelect >= MainControl.LanguagePackInsideNumber);

                    settingSelect = settingSelectBack;
                    break;
            }
        }

        /// <summary>
        /// 获取语言包信息
        /// 返回returnString
        /// </summary>
        private string GetLanguagePacksName(string pathString, string returnString, bool isOutSide)
        {
            List<string> strings = new List<string>();
            MainControl.Instance.LoadItemData(strings, ReadFile(pathString + "\\LanguagePackInformation", isOutSide));
            strings = MainControl.Instance.ChangeItemData(strings, true, new List<string>());
            return MainControl.Instance.ScreenMaxToOneSon(strings, returnString);
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
        /// 返回开/关文本
        /// </summary>
        private string OpenOrClose(bool booler)
        {
            if (booler)
                return MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Open");
            return MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "Close");
        }

        private void Update()
        {
            if (freeze)
                return;
            if (MainControl.Instance.OverworldControl.openFPS)
                fps.text = FPSFlash(fps.text);
            else
                fps.text = "";

            if (MainControl.Instance.KeyArrowToControl(KeyCode.Escape, 1))
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
            if (MainControl.Instance.KeyArrowToControl(KeyCode.Escape, 2))
            {
                clock = 0;
                exitImage.color = new Color(1, 1, 1, 0);
            }

            //设置菜单
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
                                    origin = MainControl.Instance.OverworldControl.keyCodes[settingSelect + j];
                                    MainControl.Instance.OverworldControl.keyCodes[settingSelect + j] = SettingControl();
                                    goto default;
                                case 1:
                                    origin = MainControl.Instance.OverworldControl.keyCodesBack1[settingSelect + j];
                                    MainControl.Instance.OverworldControl.keyCodesBack1[settingSelect + j] = SettingControl();
                                    goto default;
                                case 2:
                                    origin = MainControl.Instance.OverworldControl.keyCodesBack2[settingSelect + j];
                                    MainControl.Instance.OverworldControl.keyCodesBack2[settingSelect + j] = SettingControl();
                                    goto default;
                                default:
                                    List<KeyCode> keycodes = new List<KeyCode>
                                    {
                                        MainControl.Instance.OverworldControl.keyCodes[settingSelect + j],
                                        MainControl.Instance.OverworldControl.keyCodesBack1[settingSelect + j],
                                        MainControl.Instance.OverworldControl.keyCodesBack2[settingSelect + j]
                                    };
                                    for (int i = 0; i < MainControl.Instance.OverworldControl.keyCodes.Count; i++)
                                    {
                                        if (MainControl.Instance.OverworldControl.keyCodes[i] == keycodes[controlSelect] && i != settingSelect + j)
                                        {
                                            MainControl.Instance.OverworldControl.keyCodes[i] = origin;
                                            break;
                                        }
                                    }
                                    for (int i = 0; i < MainControl.Instance.OverworldControl.keyCodesBack1.Count; i++)
                                    {
                                        if (MainControl.Instance.OverworldControl.keyCodesBack1[i] == keycodes[controlSelect] && i != settingSelect + j)
                                        {
                                            MainControl.Instance.OverworldControl.keyCodesBack1[i] = origin;
                                            break;
                                        }
                                    }
                                    for (int i = 0; i < MainControl.Instance.OverworldControl.keyCodesBack2.Count; i++)
                                    {
                                        if (MainControl.Instance.OverworldControl.keyCodesBack2[i] == keycodes[controlSelect] && i != settingSelect + j)
                                        {
                                            MainControl.Instance.OverworldControl.keyCodesBack2[i] = origin;
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
                if (SceneManager.GetActiveScene().name != "Story" && MainControl.Instance.KeyArrowToControl(KeyCode.V) && !MainControl.Instance.OverworldControl.isSetting && !MainControl.Instance.blacking)
                {
                    foreach (TypeWritter typeWritter in typeWritters)
                    {
                        typeWritter.TypePause(true);
                    }

                    InSetting();
                }
            }
            if (!MainControl.Instance.OverworldControl.isSetting)
                return;

            settingSoul.rectTransform.anchoredPosition = new Vector2(-325f, -28f + settingSelect * -37);

            if (settingTmp.rectTransform.anchoredPosition.x > 125)
            {
                switch (settingLevel)
                {
                    case 0:
                        if (!isSettingName)
                        {
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
                            {
                                AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                settingSelect++;
                                if (settingSelect > 7)
                                    settingSelect = 0;
                            }
                            else if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow))
                            {
                                AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                settingSelect--;
                                if (settingSelect < 0)
                                    settingSelect = 7;
                            }
                        }
                        else
                        {
                            if (MainControl.Instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
                            {
                                if (MainControl.Instance.OverworldControl.mainVolume > 0)
                                {
                                    AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                    MainControl.Instance.OverworldControl.mainVolume -= 0.01f;
                                    AudioListener.volume = MainControl.Instance.OverworldControl.mainVolume;
                                }
                                SettingText(false, true);
                            }
                            else if (MainControl.Instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow))
                            {
                                if (MainControl.Instance.OverworldControl.mainVolume < 1)
                                {
                                    AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                    MainControl.Instance.OverworldControl.mainVolume += 0.01f;
                                    AudioListener.volume = MainControl.Instance.OverworldControl.mainVolume;
                                }
                                SettingText(false, true);
                            }
                        }

                        if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                        {
                            AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                            if (!isSettingName)
                                switch (settingSelect)
                                {
                                    case 0:
                                        saveVolume = MainControl.Instance.OverworldControl.mainVolume;
                                        isSettingName = true;
                                        SettingText(false, true);
                                        break;

                                    case 1:
                                        settingLevel = 1;
                                        SettingText();
                                        settingSelect = 0;
                                        break;

                                    case 2:
                                        MainControl.Instance.OverworldControl.fullScreen = !MainControl.Instance.OverworldControl.fullScreen;
                                        MainControl.Instance.SetResolution(MainControl.Instance.OverworldControl.resolutionLevel);

                                        goto default;
                                    case 3:
                                        MainControl.Instance.ChangeResolution();
                                        goto default;
                                    case 4:
                                        MainControl.Instance.OverworldControl.noSFX = !MainControl.Instance.OverworldControl.noSFX;
                                        MainControl.Instance.FindAndChangeAllSFX(MainControl.Instance.OverworldControl.noSFX);
                                        PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.Instance.OverworldControl.noSFX));
                                        goto default;
                                    case 5:
                                        MainControl.Instance.OverworldControl.openFPS = !MainControl.Instance.OverworldControl.openFPS;
                                        goto default;
                                    case 6:
                                        if (SceneManager.GetActiveScene().name == "Rename")
                                            return;
                                        if (SceneManager.GetActiveScene().name == "Menu")
                                            goto case 7;
                                        MainControl.Instance.OutBlack("Menu", Color.black, true, animSpeed);
                                        CloseSetting();
                                        freeze = true;
                                        break;
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
                        else if (SceneManager.GetActiveScene().name != "Story" && (MainControl.Instance.KeyArrowToControl(KeyCode.X) || MainControl.Instance.KeyArrowToControl(KeyCode.V)))
                        {
                            if (!isSettingName)
                            {
                                ExitSetting();
                            }
                            else
                            {
                                MainControl.Instance.OverworldControl.mainVolume = saveVolume;
                                AudioListener.volume = MainControl.Instance.OverworldControl.mainVolume;
                                SettingText();
                                isSettingName = false;
                            }
                        }
                        else if (MainControl.Instance.KeyArrowToControl(KeyCode.C))
                        {
                            if (!isSettingName)
                            {
                                if (settingSelect == 2)
                                {
                                    AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                    if ((int)MainControl.Instance.OverworldControl.vsyncMode < 2)
                                        MainControl.Instance.OverworldControl.vsyncMode++;
                                    else
                                        MainControl.Instance.OverworldControl.vsyncMode = OverworldControl.VSyncMode.DonNotSync;

                                    PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(MainControl.Instance.OverworldControl.vsyncMode));
                                }
                                else if (settingSelect == 3)
                                {
                                    AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                    MainControl.Instance.OverworldControl.hdResolution = !MainControl.Instance.OverworldControl.hdResolution;
                                    MainControl.Instance.ChangeResolution();
                                    SettingText();
                                    PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(MainControl.Instance.OverworldControl.hdResolution));
                                }
                            }
                        }

                        string textForUnder = "";
                        switch (settingSelect)
                        {
                            case 0:
                                textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingMainVolumeTip");
                                break;

                            case 1:
                                textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingControlTip");
                                break;

                            case 2:
                                string vsyncModeAdd;
                                switch (MainControl.Instance.OverworldControl.vsyncMode)
                                {
                                    case OverworldControl.VSyncMode.DonNotSync:
                                        vsyncModeAdd = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "VSyncNone");
                                        break;

                                    case OverworldControl.VSyncMode.SyncToRefreshRate:
                                        vsyncModeAdd = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "VSyncFull");
                                        break;

                                    case OverworldControl.VSyncMode.HalfSync:
                                        vsyncModeAdd = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "VSyncHalf");
                                        break;

                                    default:
                                        goto case OverworldControl.VSyncMode.DonNotSync;
                                }

                                if (!MainControl.Instance.OverworldControl.fullScreen)
                                    textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingFullScreenTipOpen") + "\n" + vsyncModeAdd;
                                else
                                    textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingFullScreenTipClose") + "\n" + vsyncModeAdd;
                                break;

                            case 3:
                                if (!MainControl.Instance.OverworldControl.hdResolution)
                                    textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingResolvingTip");
                                else
                                    textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingResolvingTipHD");

                                break;

                            case 4:
                                textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingSFXTip");
                                break;

                            case 5:
                                textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingFPSTip");
                                break;

                            case 6:
                                textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingBackMenuTip");
                                break;

                            case 7:
                                textForUnder = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "SettingBackGameTip");
                                break;
                        }
                        settingTmpUnder.text = textForUnder;
                        break;

                    case 1:
                        if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            settingSelect++;
                            if (settingSelect > 8)
                                settingSelect = 0;
                        }
                        else if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            settingSelect--;
                            if (settingSelect < 0)
                                settingSelect = 8;
                        }
                        if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                        {
                            AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
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
                                MainControl.Instance.ApplyDefaultControl();
                            else
                            {
                                settingLevel = 0;
                                settingSelect = 0;

                                SettingText();
                                return;
                            }

                            SettingText(false, true);
                        }
                        else if (MainControl.Instance.KeyArrowToControl(KeyCode.X))
                        {
                            settingLevel = 0;
                            settingSelect = 0;

                            SettingText();
                            return;
                        }
                        else if (MainControl.Instance.KeyArrowToControl(KeyCode.C))
                        {
                            AudioController.instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
                            if (controlSelect < 2)
                                controlSelect++;
                            else controlSelect = 0;

                            SettingText();
                        }
                        settingTmpUnder.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlUnder" + controlSelect);

                        break;

                    case 2:
                        if (MainControl.Instance.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            settingSelect++;
                            if (settingSelect > settingSelectMax)
                                settingSelect = 0;
                        }
                        else if (MainControl.Instance.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            settingSelect--;
                            if (settingSelect < 0)
                                settingSelect = settingSelectMax;
                        }
                        if (MainControl.Instance.KeyArrowToControl(KeyCode.Z))
                        {
                            if (settingSelect != settingSelectMax)
                            {
                                AudioController.instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                SettingText(false, true);
                                MainControl.Instance.languagePack = settingSelect;
                            }
                            else
                            {
                                ExitSetting(true);
                            }
                        }
                        else if (SceneManager.GetActiveScene().name != "Story" && (MainControl.Instance.KeyArrowToControl(KeyCode.X) || MainControl.Instance.KeyArrowToControl(KeyCode.V)))
                        {
                            ExitSetting(true);
                        }
                        break;
                }
            }
        }

        private void CloseSetting(bool isLan = false)
        {
            MainControl.Instance.OverworldControl.isSetting = false;
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
            MainControl.Instance.OverworldControl.isSetting = true;
            DOTween.To(() => setting.rectTransform.sizeDelta, x => setting.rectTransform.sizeDelta = x, new Vector2(6000, setting.rectTransform.sizeDelta.y), animSpeed).SetEase(Ease.InCirc);
            settingTmp.DOColor(Color.white, 1).SetEase(Ease.InCubic);
            DOTween.To(() => settingTmp.rectTransform.anchoredPosition, x => settingTmp.rectTransform.anchoredPosition = x, new Vector2(140, 140), animSpeed + 0.25f).SetEase(Ease.OutCubic);
            settingSelect = 0;
            settingTmpUnder.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlEggshell");
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
            settingTmpUnder.text = MainControl.Instance.ScreenMaxToOneSon(MainControl.Instance.OverworldControl.settingSave, "ControlEggshell");
        }

        private float m_LastUpdateShowTime;  //上一次更新帧率的时间;
        private float m_UpdateShowDeltaTime = 0.2f;//更新帧率的时间间隔;
        private int m_FrameUpdate;//帧数;
        private float m_FPS;//帧率

        private string FPSFlash(string origin)
        {
            m_FrameUpdate++;
            if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
            {
                //FPS = 某段时间内的总帧数 / 某段时间
                m_FPS = m_FrameUpdate / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
                m_FrameUpdate = 0;
                m_LastUpdateShowTime = Time.realtimeSinceStartup;
                return ((int)m_FPS).ToString();
            }

            return origin;
        }

        /// <summary>
        /// Anim调用
        /// </summary>
        public void AnimSetHeartPos()
        {
            Vector2 uiPos = WorldToUgui(MainControl.Instance.OverworldControl.playerDeadPos);
            transform.Find("Heart").GetComponent<RectTransform>().anchoredPosition = uiPos;
        }

        public Vector2 WorldToUgui(Vector3 position)
        {
            RectTransform canvasRectTransform = GetComponent<RectTransform>();
            Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);//世界坐标转换为屏幕坐标
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            screenPoint -= screenSize / 2;//将屏幕坐标变换为以屏幕中心为原点
            Vector2 anchorPos = screenPoint / screenSize * canvasRectTransform.sizeDelta;//缩放得到UGUI坐标

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
            AudioController.instance.GetFx(i, MainControl.Instance.AudioControl.fxClipUI);
        }

        private void AnimOpen()
        {
            animator.SetBool("Open", false);
            MainControl.Instance.OutBlack("Battle", Color.black, false, -0.5f);
        }
    }
}