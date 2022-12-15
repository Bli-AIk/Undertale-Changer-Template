using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;
using System.IO;
/// <summary>
/// 有一说一 一个FPS显示 一个长按ESC退出 一个设置界面 还挺有用的吧(
/// </summary>
public class CanvasController : MonoBehaviour
{
    public bool openRound;//敌人回合不能开
    RoundController roundController;
    TextMeshProUGUI fps;
    Image image;
    float clock;
    public List<Sprite> sprites;
    public List<Vector2> sizes;
    
    Image setting, settingSoul;
    TextMeshProUGUI settingTmp, settingTmpSon, settingTmpUnder;
    public RenderMode renderMode;
    public int settingSelent, settingSelentMax;//目前 Max仅用于配置语言包
    public int settingLevel;//切换层级 0层默认 1层按键设置 2层语言包配置
    public int controlPage, controlSelent;//Page是翻页 Selent是切换主次按键设置
    public bool isSettingName;//是否选中
    public bool canNotSetting;
    float saveVolume;
    bool isSettingControl;

    bool freeze;//防止切场景时整事儿

    Canvas canvas;
    TypeWritter[] typeWritters;//存储打字机以暂停协程
    // Start is called before the first frame update
    void Start()
    {

        canvas = GetComponent<Canvas>();
        canvas.renderMode = renderMode;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            canvas.worldCamera = Camera.main;
        fps = transform.Find("FPS Text").GetComponent<TextMeshProUGUI>();
        image = transform.Find("Exit Image").GetComponent<Image>();
        if (!canNotSetting)
        {

            settingTmp = transform.Find("Setting/Setting Text").GetComponent<TextMeshProUGUI>();
            settingTmpSon = settingTmp.transform.Find("Setting Son").GetComponent<TextMeshProUGUI>();
            settingSoul = settingTmp.transform.Find("Soul").GetComponent<Image>();
            settingTmpUnder = settingTmp.transform.Find("Setting Under").GetComponent<TextMeshProUGUI>();
            setting = transform.Find("Setting").GetComponent<Image>();

        }
        if (openRound)
            roundController = MainControl.instance.GetComponent<RoundController>();
        m_LastUpdateShowTime = Time.realtimeSinceStartup;
        if (!canNotSetting)
            SettingText();
        /*
     foreach (var item in Screen.resolutions)
     {
         Debug.Log(item);
     }
     */
        typeWritters = (TypeWritter[])Resources.FindObjectsOfTypeAll(typeof(TypeWritter));
    }
    void SettingText(bool OnlySetSon = false,bool isSetting = false)
    {
        switch (settingLevel)
        {
            case 0:
                if (!OnlySetSon)
                {
                    if (!isSetting)
                    {
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Setting") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingMainVolume") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingControl") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingFullScreen") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingResolving") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingSFX") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingFPS") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingBackMenu") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingBackGame");
                    }
                    else
                    {
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Setting") + "\n<color=yellow>" +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingMainVolume") + "</color>\n" +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingControl") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingFullScreen") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingResolving") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingSFX") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingFPS") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingBackMenu") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingBackGame");
                    }
                }
                if (!isSetting)
                    settingTmpSon.text = ((int)(MainControl.instance.OverwroldControl.mainVolume * 100)).ToString() + "%\n\n" + OpenOrClose(MainControl.instance.OverwroldControl.fullScreen) + '\n' +
                    MainControl.instance.OverwroldControl.resolution.x + '×' + MainControl.instance.OverwroldControl.resolution.y + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.noSFX) + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.openFPS);
                else settingTmpSon.text = "<color=yellow>" + ((int)(MainControl.instance.OverwroldControl.mainVolume * 100)).ToString() + "%</color>\n\n" + OpenOrClose(MainControl.instance.OverwroldControl.fullScreen) + '\n' +
                    MainControl.instance.OverwroldControl.resolution.x + '×' + MainControl.instance.OverwroldControl.resolution.y + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.noSFX) + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.openFPS);

                break;
            case 1:
                List<string> strings = new List<string>();

                for (int i = 0; i < 6; i++)
                {
                    if (isSetting && i == settingSelent)
                        strings.Add("<color=yellow>");
                    else
                        strings.Add("");
                }
                switch (controlPage)
                {
                    case 0:
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Control") + "</color>" + '\n' +
                       strings[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlDown") + "</color>"+ '\n' +
                       strings[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlRight") + "</color>"+ '\n' +
                       strings[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlUp") + "</color>" +'\n' +
                       strings[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlLeft") + "</color>" + '\n' +
                       strings[4] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlZ") + "</color>" + '\n' +
                       strings[5] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlX") + "</color>" + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "PageNext") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlDefault") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Back");


                        settingTmpSon.text = "";
                        for (int i = 0; i < 6; i++)
                        {
                            if (isSetting && i == settingSelent)
                            {
                                settingTmpSon.text += "<color=yellow>";
                            }
                            switch (controlSelent)
                            {
                                case 0:
                                    settingTmpSon.text += MainControl.instance.OverwroldControl.keyCodes[i] + "</color>\n";
                                    break;
                                case 1:
                                    settingTmpSon.text += MainControl.instance.OverwroldControl.keyCodesBack1[i] + "</color>\n";
                                    break;
                                case 2:
                                    settingTmpSon.text += MainControl.instance.OverwroldControl.keyCodesBack2[i] + "</color>\n";
                                    break;
                            }
                        }

                        break;
                    case 1:
                        settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Control") + '\n' +
                       strings[0] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlC") + "</color>" + '\n' +
                       strings[1] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlV") + "</color>" + '\n' +
                       strings[2] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlF4") + "</color>" + '\n' +
                       strings[3] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlTab") + "</color>" + '\n' +
                       strings[4] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlSemicolon") + "</color>" + '\n' +
                       strings[5] + MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlEsc") + "</color>" + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "PageLast") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlDefault") + '\n' +
                       MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Back");
                        settingTmpSon.text = "";
                        for (int i = 6; i < 12; i++)
                        {
                            if (isSetting && i - 6 == settingSelent)
                            {
                                settingTmpSon.text += "<color=yellow>";
                            }
                            switch (controlSelent)
                            {
                                case 0:
                                    settingTmpSon.text += MainControl.instance.OverwroldControl.keyCodes[i] + "</color>\n";
                                    break;
                                case 1:
                                    settingTmpSon.text += MainControl.instance.OverwroldControl.keyCodesBack1[i] + "</color>\n";
                                    break;
                                case 2:
                                    settingTmpSon.text += MainControl.instance.OverwroldControl.keyCodesBack2[i] + "</color>\n";
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
                    MainControl.instance.Initialization(settingSelent);
                }
                if (!OnlySetSon)
                    settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "LanguagePack")+'\n';
                settingTmpSon.text = "";
                settingSelentMax = 0;
                int settingSelentBack = settingSelent;
                if (OnlySetSon)
                    settingSelent = MainControl.instance.OverwroldControl.languagePack;

                foreach (string pathString in Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage"))//内置包信息
                {

                    if (settingSelentMax == settingSelent)
                    {
                        pathStringSaver = pathString;
                    }
                    settingSelentMax++;
                    if (!OnlySetSon)
                        settingTmp.text += GetLanguagePackageName(pathString, "LanguagePackName") + '\n';
                        
                }
                foreach (string pathString in Directory.GetDirectories(Application.dataPath + "\\LanguagePacks"))
                {
                    if (settingSelentMax == settingSelent)
                        pathStringSaver = pathString;
                    settingSelentMax++;
                    if (!OnlySetSon)
                        settingTmp.text += GetLanguagePackageName(pathString, "LanguagePackName") + '\n';

                    //if (settingSelentMax % 5 == 0 && !OnlySetSon)
                    //    settingTmp.text += MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "PageNext") + '\n';

                }
                if (!OnlySetSon)
                    settingTmp.text += MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Back");
                
                settingTmpUnder.text = GetLanguagePackageName(pathStringSaver, "LanguagePackInformation") + '\n' + GetLanguagePackageName(pathStringSaver, "LanguagePackAuthor");


                settingSelent = settingSelentBack;
                break;
        }
     
    }
    /// <summary>
    /// 获取语言包信息
    /// 返回returnString
    /// </summary>
    string GetLanguagePackageName(string pathString,string returnString)
    {
        List<string> strings = new List<string>();
        MainControl.instance.LoadItemData(strings, ReadFile(pathString + "\\LanguagePackInformation.txt"));
        strings = MainControl.instance.ChangeItemData(strings, true, new List<string>());
        return MainControl.instance.ScreenMaxToOneSon(strings, returnString);
    }


    string ReadFile(string PathName)
    {
        string strs = File.ReadAllText(PathName);
        return strs;
    }

    /// <summary>
    /// 返回开/关文本
    /// </summary>
    string OpenOrClose(bool booler)
    {
        if (booler)
            return MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Open");
        else
            return MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Close");
    }
    // Update is called once per frame
    void Update()
    {
        if (freeze)
            return;
        if (MainControl.instance.OverwroldControl.openFPS)
            fps.text = FPSFlash(fps.text);
        else
            fps.text = "";

        if (MainControl.instance.KeyArrowToControl(KeyCode.Escape,1))
        {
            if (image.color.a < 1)
                image.color += Time.deltaTime * Color.white;
            if (clock < 3)
            {
                image.sprite = sprites[(int)clock];
                image.rectTransform.sizeDelta = sizes[(int)clock];
                clock += Time.deltaTime;
            }
            else Application.Quit();
        }
        if (MainControl.instance.KeyArrowToControl(KeyCode.Escape, 2))
        {
            clock = 0;
            image.color = new Color(1, 1, 1, 0);
        }

        //设置菜单
        if(isSettingControl)
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
                      
                        switch (controlSelent)
                        {
                            case 0:
                                origin = MainControl.instance.OverwroldControl.keyCodes[settingSelent + j];
                                MainControl.instance.OverwroldControl.keyCodes[settingSelent + j] = SettingControl();
                                goto default;
                            case 1:
                                origin = MainControl.instance.OverwroldControl.keyCodesBack1[settingSelent + j];
                                MainControl.instance.OverwroldControl.keyCodesBack1[settingSelent + j] = SettingControl();
                                goto default;
                            case 2:
                                origin = MainControl.instance.OverwroldControl.keyCodesBack2[settingSelent + j];
                                MainControl.instance.OverwroldControl.keyCodesBack2[settingSelent + j] = SettingControl();
                                goto default;
                            default:
                                List<KeyCode> keycodes = new List<KeyCode>()
                                {
                                    MainControl.instance.OverwroldControl.keyCodes[settingSelent + j],
                                    MainControl.instance.OverwroldControl.keyCodesBack1[settingSelent + j],
                                    MainControl.instance.OverwroldControl.keyCodesBack2[settingSelent + j]
                                };
                                for (int i = 0; i < MainControl.instance.OverwroldControl.keyCodes.Count; i++)
                                {
                                    if (MainControl.instance.OverwroldControl.keyCodes[i] == keycodes[controlSelent] && i != settingSelent + j)
                                    {
                                        MainControl.instance.OverwroldControl.keyCodes[i] = origin;
                                        break;
                                    }
                                }
                                for (int i = 0; i < MainControl.instance.OverwroldControl.keyCodesBack1.Count; i++)
                                {
                                    if (MainControl.instance.OverwroldControl.keyCodesBack1[i] == keycodes[controlSelent] && i != settingSelent + j)
                                    {
                                        MainControl.instance.OverwroldControl.keyCodesBack1[i] = origin;
                                        break;
                                    }
                                }
                                for (int i = 0; i < MainControl.instance.OverwroldControl.keyCodesBack2.Count; i++)
                                {
                                    if (MainControl.instance.OverwroldControl.keyCodesBack2[i] == keycodes[controlSelent] && i != settingSelent + j)
                                    {
                                        MainControl.instance.OverwroldControl.keyCodesBack2[i] = origin;
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
        if (!canNotSetting)
        {
            if ((openRound && roundController.isMyRound) || !openRound)
                if (MainControl.instance.KeyArrowToControl(KeyCode.V) && !MainControl.instance.OverwroldControl.isSetting)
                {

                    foreach (TypeWritter typeWritter in typeWritters)
                    {
                        typeWritter.TypePause(true);
                    }

                    InSetting();
                }
            if (!MainControl.instance.OverwroldControl.isSetting)
                return;

            settingSoul.rectTransform.anchoredPosition = new Vector2(-325f, -28f + settingSelent * -37);

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
                                settingSelent++;
                                if (settingSelent > 7)
                                    settingSelent = 0;
                            }
                            else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                            {
                                AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                                settingSelent--;
                                if (settingSelent < 0)
                                    settingSelent = 7;
                            }
                        }
                        else
                        {
                            if (MainControl.instance.KeyArrowToControl(KeyCode.LeftArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                            {
                                if (MainControl.instance.OverwroldControl.mainVolume > 0)
                                {
                                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                                    MainControl.instance.OverwroldControl.mainVolume -= 0.01f;
                                    AudioListener.volume = MainControl.instance.OverwroldControl.mainVolume;
                                }
                                SettingText(false, true);
                            }
                            else if (MainControl.instance.KeyArrowToControl(KeyCode.RightArrow, 1) || MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                            {
                                if (MainControl.instance.OverwroldControl.mainVolume < 1)
                                {
                                    AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                                    MainControl.instance.OverwroldControl.mainVolume += 0.01f;
                                    AudioListener.volume = MainControl.instance.OverwroldControl.mainVolume;
                                }
                                SettingText(false, true);
                            }
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                            if (!isSettingName)
                                switch (settingSelent)
                                {
                                    case 0:
                                        saveVolume = MainControl.instance.OverwroldControl.mainVolume;
                                        isSettingName = true;
                                        SettingText(false, true);
                                        break;
                                    case 1:
                                        settingLevel = 1;
                                        SettingText();
                                        settingSelent = 0;
                                        break;
                                    case 2:
                                        MainControl.instance.OverwroldControl.fullScreen = !MainControl.instance.OverwroldControl.fullScreen;
                                        MainControl.instance.SetResolution(MainControl.instance.OverwroldControl.resolutionLevel);

                                        goto default;
                                    case 3:
                                        MainControl.instance.ChangeResolution();
                                        goto default;
                                    case 4:
                                        MainControl.instance.OverwroldControl.noSFX = !MainControl.instance.OverwroldControl.noSFX;
                                        MainControl.instance.FindAndChangeAllSFX(MainControl.instance.OverwroldControl.noSFX);
                                        goto default;
                                    case 5:
                                        MainControl.instance.OverwroldControl.openFPS = !MainControl.instance.OverwroldControl.openFPS;
                                        goto default;
                                    case 6:
                                        if (SceneManager.GetActiveScene().name == "Rename")
                                            return;
                                        else if (SceneManager.GetActiveScene().name == "Menu")
                                            goto case 7;
                                        else
                                        {
                                            MainControl.instance.OutBlack("Menu", true);
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
                        if (MainControl.instance.KeyArrowToControl(KeyCode.X) || MainControl.instance.KeyArrowToControl(KeyCode.V))
                        {
                            if (!isSettingName)
                            {
                                ExitSetting();
                            }
                            else
                            {
                                MainControl.instance.OverwroldControl.mainVolume = saveVolume;
                                AudioListener.volume = MainControl.instance.OverwroldControl.mainVolume;
                                SettingText();
                                isSettingName = false;
                            }
                        }

                        string textForUnder = "";
                        switch (settingSelent)
                        {

                            case 0:
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingMainVolumeTip");
                                break;
                            case 1:
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingControlTip");
                                break;
                            case 2:
                                if (!MainControl.instance.OverwroldControl.fullScreen)
                                    textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingFullScreenTipOpen");
                                else
                                    textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingFullScreenTipClose");
                                break;
                            case 3:
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingResolvingTip");
                                break;
                            case 4:
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingSFXTip");
                                break;
                            case 5:
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingFPSTip");
                                break;
                            case 6:
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingBackMenuTip");
                                break;
                            case 7:
                                textForUnder = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "SettingBackGameTip");
                                break;
                        }
                        settingTmpUnder.text = textForUnder;
                        break;
                    case 1:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            settingSelent++;
                            if (settingSelent > 8)
                                settingSelent = 0;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            settingSelent--;
                            if (settingSelent < 0)
                                settingSelent = 8;
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                            if (settingSelent < 6)
                                isSettingControl = true;
                            else if (settingSelent == 6)
                                switch (controlPage)
                                {
                                    case 0:
                                        controlPage = 1;
                                        break;
                                    case 1:
                                        controlPage = 0;
                                        break;
                                }
                            else if (settingSelent == 7)
                                MainControl.instance.ApplyDefaultControl();
                            else
                            {
                                settingLevel = 0;
                                settingSelent = 0;

                                SettingText();
                                return;
                            }

                            SettingText(false, true);
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.X))
                        {
                            settingLevel = 0;
                            settingSelent = 0;

                            SettingText();
                            return;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.C))
                        {
                            AudioController.instance.GetFx(3, MainControl.instance.AudioControl.fxClipUI);
                            if (controlSelent < 2)
                                controlSelent++;
                            else controlSelent = 0;

                            SettingText();
                        }
                        settingTmpUnder.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlUnder" + controlSelent);

                        break;
                    case 2:
                        if (MainControl.instance.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            settingSelent++;
                            if (settingSelent > settingSelentMax)
                                settingSelent = 0;
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            AudioController.instance.GetFx(0, MainControl.instance.AudioControl.fxClipUI);
                            settingSelent--;
                            if (settingSelent < 0)
                                settingSelent = settingSelentMax;
                        }
                        if (MainControl.instance.KeyArrowToControl(KeyCode.Z))
                        {
                            if (settingSelent != settingSelentMax)
                            {
                                AudioController.instance.GetFx(1, MainControl.instance.AudioControl.fxClipUI);
                                SettingText(false, true);
                                MainControl.instance.OverwroldControl.languagePack = settingSelent;
                            }
                            else
                            {
                                ExitSetting();
                            }
                        }
                        else if (MainControl.instance.KeyArrowToControl(KeyCode.X) || MainControl.instance.KeyArrowToControl(KeyCode.V))
                        {
                            ExitSetting();
                            return;
                        }
                        break;
                }


            }
        }
       

    }
    void CloseSetting()
    {
        MainControl.instance.OverwroldControl.isSetting = false;
        foreach (TypeWritter typeWritter in typeWritters)
        {
            typeWritter.TypePause(false);
        }
    }
    KeyCode SettingControl()
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
        MainControl.instance.OverwroldControl.isSetting = true;
        DOTween.To(() => setting.rectTransform.sizeDelta, x => setting.rectTransform.sizeDelta = x, new Vector2(6000, setting.rectTransform.sizeDelta.y), 0.75f).SetEase(Ease.InCirc);
        settingTmp.DOColor(Color.white, 1).SetEase(Ease.InCubic);
        DOTween.To(() => settingTmp.rectTransform.anchoredPosition, x => settingTmp.rectTransform.anchoredPosition = x, new Vector2(140, 140), 1).SetEase(Ease.OutCubic);
        settingSelent = 0;
        settingTmpUnder.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlEggshell");
        SettingText();
        if(settingLevel == 2)
            SettingText(true);
    }
    void ExitSetting()
    {
        settingLevel = 0;
        DOTween.To(() => setting.rectTransform.sizeDelta, x => setting.rectTransform.sizeDelta = x, new Vector2(0, setting.rectTransform.sizeDelta.y), 0.75f).SetEase(Ease.OutCirc);
        settingTmp.DOColor(Color.white, 0).SetEase(Ease.OutCubic);
        DOTween.To(() => settingTmp.rectTransform.anchoredPosition, x => settingTmp.rectTransform.anchoredPosition = x, new Vector2(-610, 140), 1).SetEase(Ease.InSine).OnKill(CloseSetting);
        settingTmpUnder.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlEggshell");
    }
    private float m_LastUpdateShowTime = 0f;  //上一次更新帧率的时间;  
    private float m_UpdateShowDeltaTime = 0.2f;//更新帧率的时间间隔;  
    private int m_FrameUpdate = 0;//帧数;  
    private float m_FPS = 0;//帧率

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
        else return origin;
    }

    /// <summary>
    /// 别问 给OW内切战斗场景动画用的，草
    /// </summary>
    public void AnimSetHeartPos()
    {
        Vector2 uiPos = WorldToUgui(MainControl.instance.PlayerControl.deadPos);
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
        j.DOColor(new Color(j.color.r, j.color.g, j.color.b, 0), 0.75f).SetEase(Ease.Linear);
        DOTween.To(() => i.anchoredPosition, x => i.anchoredPosition = x, new Vector2(-330, -250), 1.5f).SetEase(Ease.OutCirc).OnKill(() => MainControl.instance.OutBlack("Battle", false, 0.5f));
        
    }
    public void PlayFX(int i)
    {
        AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipUI);

    }

}
