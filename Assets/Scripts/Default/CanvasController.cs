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
/// UI���棬������FPS��ʾ ����ESC�˳� ���ý���
/// </summary>
public class CanvasController : MonoBehaviour
{
    public bool openRound;//���˻غϲ��ܿ�
    TextMeshProUGUI fps;
    Image image;
    float clock;
    public List<Sprite> sprites;
    public List<Vector2> sizes;
    
    Image setting, settingSoul;
    TextMeshProUGUI settingTmp, settingTmpSon, settingTmpUnder;
    public RenderMode renderMode;

    int settingSelect, settingSelectMax;//Ŀǰ Max�������������԰�
    [HideInInspector]
    public int settingLevel;//�л��㼶 0��Ĭ�� 1�㰴������ 2�����԰�����
    int controlPage, controlSelect;//Page�Ƿ�ҳ Select���л����ΰ�������
    bool isSettingName;//�Ƿ�ѡ��
    float saveVolume;
    bool isSettingControl;
    [HideInInspector]
    public bool freeze;//��ֹ�г���ʱ���¶�

    Canvas canvas;
    TypeWritter[] typeWritters;//�洢���ֻ�����ͣЭ��

    public Animator animator;
    public static CanvasController instance;
    private void Awake()
    {
        instance = this;


        animator = GetComponent<Animator>();
        canvas = GetComponent<Canvas>();
        fps = transform.Find("FPS Text").GetComponent<TextMeshProUGUI>();
        image = transform.Find("Exit Image").GetComponent<Image>();

        settingTmp = transform.Find("Setting/Setting Text").GetComponent<TextMeshProUGUI>();
        settingTmpSon = settingTmp.transform.Find("Setting Son").GetComponent<TextMeshProUGUI>();
        settingSoul = settingTmp.transform.Find("Soul").GetComponent<Image>();
        settingTmpUnder = settingTmp.transform.Find("Setting Under").GetComponent<TextMeshProUGUI>();
        setting = transform.Find("Setting").GetComponent<Image>();


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
                    MainControl.instance.OverwroldControl.resolution.x + '��' + MainControl.instance.OverwroldControl.resolution.y + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.noSFX) + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.openFPS);
                else settingTmpSon.text = "<color=yellow>" + ((int)(MainControl.instance.OverwroldControl.mainVolume * 100)).ToString() + "%</color>\n\n" + OpenOrClose(MainControl.instance.OverwroldControl.fullScreen) + '\n' +
                    MainControl.instance.OverwroldControl.resolution.x + '��' + MainControl.instance.OverwroldControl.resolution.y + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.noSFX) + '\n' + OpenOrClose(MainControl.instance.OverwroldControl.openFPS);

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
                            if (isSetting && i == settingSelect)
                            {
                                settingTmpSon.text += "<color=yellow>";
                            }
                            switch (controlSelect)
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
                            if (isSetting && i - 6 == settingSelect)
                            {
                                settingTmpSon.text += "<color=yellow>";
                            }
                            switch (controlSelect)
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
                    MainControl.instance.Initialization(settingSelect);
                }
                if (!OnlySetSon)
                    settingTmp.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "LanguagePack")+'\n';
                settingTmpSon.text = "";
                settingSelectMax = 0;
                int settingSelectBack = settingSelect;
                if (OnlySetSon)
                    settingSelect = MainControl.instance.OverwroldControl.languagePack;

                foreach (string pathString in Directory.GetDirectories(Application.dataPath + "\\TextAssets\\LanguagePackage"))//���ð���Ϣ
                {

                    if (settingSelectMax == settingSelect)
                    {
                        pathStringSaver = pathString;
                    }
                    settingSelectMax++;
                    if (!OnlySetSon)
                        settingTmp.text += GetLanguagePackageName(pathString, "LanguagePackName") + '\n';
                        
                }
                foreach (string pathString in Directory.GetDirectories(Application.dataPath + "\\LanguagePacks"))
                {
                    if (settingSelectMax == settingSelect)
                        pathStringSaver = pathString;
                    settingSelectMax++;
                    if (!OnlySetSon)
                        settingTmp.text += GetLanguagePackageName(pathString, "LanguagePackName") + '\n';

                    //if (settingSelectMax % 5 == 0 && !OnlySetSon)
                    //    settingTmp.text += MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "PageNext") + '\n';

                }
                if (!OnlySetSon)
                    settingTmp.text += MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "Back");
                
                settingTmpUnder.text = GetLanguagePackageName(pathStringSaver, "LanguagePackInformation") + '\n' + GetLanguagePackageName(pathStringSaver, "LanguagePackAuthor");


                settingSelect = settingSelectBack;
                break;
        }
     
    }
    /// <summary>
    /// ��ȡ���԰���Ϣ
    /// ����returnString
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
    /// ���ؿ�/���ı�
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

        //���ò˵�
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
                      
                        switch (controlSelect)
                        {
                            case 0:
                                origin = MainControl.instance.OverwroldControl.keyCodes[settingSelect + j];
                                MainControl.instance.OverwroldControl.keyCodes[settingSelect + j] = SettingControl();
                                goto default;
                            case 1:
                                origin = MainControl.instance.OverwroldControl.keyCodesBack1[settingSelect + j];
                                MainControl.instance.OverwroldControl.keyCodesBack1[settingSelect + j] = SettingControl();
                                goto default;
                            case 2:
                                origin = MainControl.instance.OverwroldControl.keyCodesBack2[settingSelect + j];
                                MainControl.instance.OverwroldControl.keyCodesBack2[settingSelect + j] = SettingControl();
                                goto default;
                            default:
                                List<KeyCode> keycodes = new List<KeyCode>()
                                {
                                    MainControl.instance.OverwroldControl.keyCodes[settingSelect + j],
                                    MainControl.instance.OverwroldControl.keyCodesBack1[settingSelect + j],
                                    MainControl.instance.OverwroldControl.keyCodesBack2[settingSelect + j]
                                };
                                for (int i = 0; i < MainControl.instance.OverwroldControl.keyCodes.Count; i++)
                                {
                                    if (MainControl.instance.OverwroldControl.keyCodes[i] == keycodes[controlSelect] && i != settingSelect + j)
                                    {
                                        MainControl.instance.OverwroldControl.keyCodes[i] = origin;
                                        break;
                                    }
                                }
                                for (int i = 0; i < MainControl.instance.OverwroldControl.keyCodesBack1.Count; i++)
                                {
                                    if (MainControl.instance.OverwroldControl.keyCodesBack1[i] == keycodes[controlSelect] && i != settingSelect + j)
                                    {
                                        MainControl.instance.OverwroldControl.keyCodesBack1[i] = origin;
                                        break;
                                    }
                                }
                                for (int i = 0; i < MainControl.instance.OverwroldControl.keyCodesBack2.Count; i++)
                                {
                                    if (MainControl.instance.OverwroldControl.keyCodesBack2[i] == keycodes[controlSelect] && i != settingSelect + j)
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

        if ((openRound && RoundController.instance.isMyRound) || !openRound)
        {
            if (MainControl.instance.KeyArrowToControl(KeyCode.V) && !MainControl.instance.OverwroldControl.isSetting)
            {
                foreach (TypeWritter typeWritter in typeWritters)
                {
                    typeWritter.TypePause(true);
                }

                InSetting();
            }
        }
        if (!MainControl.instance.OverwroldControl.isSetting)
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
                            switch (settingSelect)
                            {
                                case 0:
                                    saveVolume = MainControl.instance.OverwroldControl.mainVolume;
                                    isSettingName = true;
                                    SettingText(false, true);
                                    break;
                                case 1:
                                    settingLevel = 1;
                                    SettingText();
                                    settingSelect = 0;
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
                                        MainControl.instance.OutBlack("Menu", Color.black, true, 0.75f);
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
                    switch (settingSelect)
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
                    settingTmpUnder.text = MainControl.instance.ScreenMaxToOneSon(MainControl.instance.OverwroldControl.menuAndSettingSave, "ControlUnder" + controlSelect);

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
                            MainControl.instance.OverwroldControl.languagePack = settingSelect;
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
        settingSelect = 0;
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
    private float m_LastUpdateShowTime = 0f;  //��һ�θ���֡�ʵ�ʱ��;  
    private float m_UpdateShowDeltaTime = 0.2f;//����֡�ʵ�ʱ����;  
    private int m_FrameUpdate = 0;//֡��;  
    private float m_FPS = 0;//֡��

    private string FPSFlash(string origin)
    {
        m_FrameUpdate++;
        if (Time.realtimeSinceStartup - m_LastUpdateShowTime >= m_UpdateShowDeltaTime)
        {
            //FPS = ĳ��ʱ���ڵ���֡�� / ĳ��ʱ��
            m_FPS = m_FrameUpdate / (Time.realtimeSinceStartup - m_LastUpdateShowTime);
            m_FrameUpdate = 0;
            m_LastUpdateShowTime = Time.realtimeSinceStartup;
            return ((int)m_FPS).ToString();
        }
        else return origin;
    }

    /// <summary>
    /// Anim����
    /// </summary>
    public void AnimSetHeartPos()
    {
        Vector2 uiPos = WorldToUgui(MainControl.instance.PlayerControl.deadPos);
        transform.Find("Heart").GetComponent<RectTransform>().anchoredPosition = uiPos;
       
    }
    public Vector2 WorldToUgui(Vector3 position)
    {
        RectTransform canvasRectTransform = GetComponent<RectTransform>();
        Vector2 screenPoint = Camera.main.WorldToScreenPoint(position);//��������ת��Ϊ��Ļ����
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        screenPoint -= screenSize / 2;//����Ļ����任Ϊ����Ļ����Ϊԭ��
        Vector2 anchorPos = screenPoint / screenSize * canvasRectTransform.sizeDelta;//���ŵõ�UGUI����

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
        DOTween.To(() => i.anchoredPosition, x => i.anchoredPosition = x, new Vector2(-330, -250), 1.5f).SetEase(Ease.OutCirc).OnKill(() => AnimOpen());
        
    }
    public void PlayFX(int i)
    {
        AudioController.instance.GetFx(i, MainControl.instance.AudioControl.fxClipUI);

    }
    void AnimOpen()
    {
        animator.SetBool("Open", false);
        MainControl.instance.OutBlack("Battle", Color.black, false, -0.5f);
    }
}
