using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DG.Tweening;
using TMPro;
using UCT.Battle;
using UCT.Control;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UCT.Global.UI
{
    /// <summary>
    /// UI界面，包括：FPS显示 长按ESC退出 设置界面
    /// </summary>
    public class CanvasController : MonoBehaviour
    {
        public int framePic;

        public static CanvasController Instance;
        private static readonly int Open = Animator.StringToHash("Open");
        public bool openTurn;//敌人回合不能开
        public TextMeshProUGUI fps;
        public Image frame;
        private Image _exitImage;
        private float _clock;
        public List<Sprite> sprites;
        public List<Vector2> sizes;

        [FormerlySerializedAs("setting")] public Image settingImage;
        private Image _settingSoul;
        private TextMeshProUGUI _settingTmp, _settingTmpSon, _settingTmpUnder;
        public RenderMode renderMode;

        private int _settingSelect, _settingSelectMax;//目前 Max仅用于配置语言包

        public enum SettingsLayer
        {
            Home,//主层级
            KeyConfiguration,//按键设置层级
            LanguageConfiguration,//语言包层级
        };

        [HideInInspector] public SettingsLayer settingsLayer;

        private int _keyPage;//用于控制（按键设置）的翻页。

        private enum KeyMapping//用于控制（按键设置）的主次键位。
        {
            MainKeyMap,
            SecondaryKeyMap1,
            SecondaryKeyMap2
        }

        private KeyMapping _keyMapping;
        
        private bool _isSettingName;//是否选中
        private float _saveVolume;
        private bool _isSettingControl;

        [HideInInspector]
        public bool freeze;//防止切场景时整事儿

        private Canvas _canvas;
        private TypeWritter[] _typeWritters;//存储打字机以暂停协程

        public Animator animator;

        private const float AnimSpeed = 0.25f;
        private void Awake()
        {
            Instance = this;

            animator = GetComponent<Animator>();
            _canvas = GetComponent<Canvas>();
            fps = transform.Find("FPS Text").GetComponent<TextMeshProUGUI>();
            _exitImage = transform.Find("Exit Image").GetComponent<Image>();

            settingImage = transform.Find("Setting").GetComponent<Image>();
            _settingTmp = transform.Find("Setting/Setting Text").GetComponent<TextMeshProUGUI>();
            _settingTmpSon = _settingTmp.transform.Find("Setting Son").GetComponent<TextMeshProUGUI>();
            _settingSoul = _settingTmp.transform.Find("Soul").GetComponent<Image>();
            _settingTmpUnder = _settingTmp.transform.Find("Setting Under").GetComponent<TextMeshProUGUI>();

            frame = transform.Find("Frame").GetComponent<Image>();
            _typeWritters = (TypeWritter[])Resources.FindObjectsOfTypeAll(typeof(TypeWritter));
        }

        public void Start()
        {
            settingsLayer = SettingsLayer.Home;
            settingImage.color = Color.clear;
            _settingTmp.color = Color.clear;
            _settingTmpSon.color = Color.clear;
            _settingTmpUnder.color = Color.clear;
            _settingSoul.color = new Color(1, 0, 0, 0);
            freeze = false;

            _canvas.renderMode = renderMode;

            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                _canvas.worldCamera = Camera.main;
            }

            _mLastUpdateShowTime = Time.realtimeSinceStartup;

            UpdateSettingDisplay();
        }
        /// <summary>
        /// 更新当前设置层的显示内容。根据设置层调用相应的配置方法。
        /// </summary>
        /// <param name="isOnlySetSon">是否仅设置子项。</param>
        /// <param name="isSetting">是否正在进行设置。</param>
        private void UpdateSettingDisplay(bool isOnlySetSon = false, bool isSetting = false)
        {
            switch (settingsLayer)
            {
                case SettingsLayer.Home:
                    HomeConfiguration(isOnlySetSon, isSetting);
                    break;

                case SettingsLayer.KeyConfiguration:
                    KeyConfiguration(isSetting);
                    break;

                case SettingsLayer.LanguageConfiguration:
                    LanguageConfiguration(isOnlySetSon, isSetting);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void HomeConfiguration(bool onlySetSon, bool isSetting)
        {
            if (!onlySetSon)
            {
                var settingsOptions = new[]
                {
                    "Setting", "SettingMainVolume", "SettingControl",
                    "SettingFullScreen", "SettingResolving", 
                    "SettingSFX", "SettingFPS", 
                    "SettingBackMenu", "SettingBackGame"
                };
                        
                var settingsText = settingsOptions.Select(key =>
                    TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.OverworldControl.settingSave, key)).ToList();

                if (isSetting)
                    settingsText[1] = $"<color=yellow>{settingsText[1]}</color>"; // 为 SettingMainVolume 添加颜色
                        
                _settingTmp.text = string.Join("\n", settingsText);
            }

            if (!isSetting)
                _settingTmpSon.text = "\n" + (int)(MainControl.Instance.OverworldControl.mainVolume * 100) + "%\n\n" + OpenOrClose(MainControl.Instance.OverworldControl.fullScreen) + '\n' +
                                      MainControl.Instance.OverworldControl.resolution.x + '×' + MainControl.Instance.OverworldControl.resolution.y + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.noSfx) + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.openFPS)+ '\n' ;
            else _settingTmpSon.text = "\n" + "<color=yellow>" + ((int)(MainControl.Instance.OverworldControl.mainVolume * 100)) + "%</color>\n\n" + OpenOrClose(MainControl.Instance.OverworldControl.fullScreen) + '\n' +
                                       MainControl.Instance.OverworldControl.resolution.x + '×' + MainControl.Instance.OverworldControl.resolution.y + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.noSfx) + '\n' + OpenOrClose(MainControl.Instance.OverworldControl.openFPS)+ '\n' ;
        }

        private void LanguageConfiguration(bool onlySetSon, bool isSetting)
        {
            var pathStringSaver = "";

            if (isSetting)
            {
                MainControl.Instance.Initialization(_settingSelect);
            }
            if (!onlySetSon)
                _settingTmp.text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "LanguagePack") + '\n';
            _settingTmpSon.text = "";
            _settingSelectMax = 0;
            var settingSelectBack = _settingSelect;
            if (onlySetSon)
                _settingSelect = MainControl.Instance.languagePackId;

            for (var i = 0; i < MainControl.LanguagePackInsideNumber; i++) //内置包信息
            {
                var pathString = "TextAssets/LanguagePacks/" + DataHandlerService.GetLanguageInsideId(i);

                if (_settingSelectMax == _settingSelect)
                {
                    pathStringSaver = pathString;
                }
                _settingSelectMax++;

                if (!onlySetSon)
                    _settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", false) + '\n';
            }
            foreach (var pathString in Directory.GetDirectories(Application.dataPath + "\\LanguagePacks"))
            {
                if (_settingSelectMax == _settingSelect)
                    pathStringSaver = pathString;
                _settingSelectMax++;
                if (!onlySetSon)
                    _settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", true) + '\n';
            }
            if (!onlySetSon)
                _settingTmp.text += TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "Back");

            _settingTmpUnder.text = GetLanguagePacksName(pathStringSaver, "LanguagePackInformation", _settingSelect >= MainControl.LanguagePackInsideNumber) + '\n' + GetLanguagePacksName(pathStringSaver, "LanguagePackAuthor", _settingSelect >= MainControl.LanguagePackInsideNumber);

            _settingSelect = settingSelectBack;
        }

        private void KeyConfiguration(bool isSetting)
        {
            var strings = new List<string>();

            for (var i = 0; i < 6; i++)
            {
                if (isSetting && i == _settingSelect)
                    strings.Add("<color=yellow>");
                else
                    strings.Add("");
            }

            List<string> prefixes;
            StringBuilder result;
            switch (_keyPage)
            {
                case 0:
                    prefixes = new List<string>
                    {
                        "Control", 
                        "ControlDown", "ControlRight", "ControlUp", "ControlLeft", 
                        "ControlZ", "ControlX", "PageDown", "ControlDefault",
                        "Back"
                    };

                    result = new StringBuilder();
                    for (var i = 0; i < prefixes.Count; i++)
                    {
                        if (i > 0 && i - 1 < strings.Count) 
                            result.Append(strings[i - 1]);
                        result.Append(TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, prefixes[i]));
                        if (i < prefixes.Count - 1)
                            result.Append("</color>\n");
                    }

                    _settingTmp.text = result.ToString();

                    _settingTmpSon.text = "\n";
                    for (var i = 0; i < 6; i++)
                    {
                        if (isSetting && i == _settingSelect)
                        {
                            _settingTmpSon.text += "<color=yellow>";
                        }

                        _settingTmpSon.text += _keyMapping switch
                        {
                            KeyMapping.MainKeyMap => MainControl.Instance.OverworldControl.keyCodes[i] +
                                                     "</color>\n",
                            KeyMapping.SecondaryKeyMap1 => MainControl.Instance.OverworldControl.keyCodesBack1
                                [i] + "</color>\n",
                            KeyMapping.SecondaryKeyMap2 => MainControl.Instance.OverworldControl.keyCodesBack2
                                [i] + "</color>\n",
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }

                    break;

                case 1:
                    prefixes = new List<string>
                    {
                        "Control", "ControlC", "ControlV", "ControlF4", 
                        "ControlTab", "ControlSemicolon", "ControlEsc", 
                        "PageUp", "ControlDefault", "Back"
                    };
                    result = new StringBuilder();
                    for (var i = 0; i < prefixes.Count; i++)
                    {
                        if (i > 0 && i - 1 < strings.Count)
                            result.Append(strings[i - 1]);
                        result.Append(TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, prefixes[i]));
                        if (i < prefixes.Count - 3)
                            result.Append("</color>\n");
                        else if (i < prefixes.Count - 1)
                            result.Append("\n");
                    }
                    _settingTmp.text = result.ToString();

                    _settingTmpSon.text = "\n";
                    for (var i = 6; i < 12; i++)
                    {
                        if (isSetting && i - 6 == _settingSelect)
                        {
                            _settingTmpSon.text += "<color=yellow>";
                        }

                        _settingTmpSon.text += _keyMapping switch
                        {
                            KeyMapping.MainKeyMap => MainControl.Instance.OverworldControl.keyCodes[i] +
                                                     "</color>\n",
                            KeyMapping.SecondaryKeyMap1 => MainControl.Instance.OverworldControl.keyCodesBack1
                                [i] + "</color>\n",
                            KeyMapping.SecondaryKeyMap2 => MainControl.Instance.OverworldControl.keyCodesBack2
                                [i] + "</color>\n",
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }

                    break;
            }
        }

        /// <summary>
        /// 获取语言包信息
        /// 返回returnString
        /// </summary>
        private static string GetLanguagePacksName(string pathString, string returnString, bool isOutSide)
        {
            var strings = new List<string>();
            DataHandlerService.LoadItemData(strings, ReadFile(pathString + "\\LanguagePackInformation", isOutSide));
            strings = DataHandlerService.ChangeItemData(strings, true, new List<string>());
            return TextProcessingService.GetFirstChildStringByPrefix(strings, returnString);
        }

        private static string ReadFile(string pathName, bool isOutSide)
        {
            return !isOutSide ? Resources.Load<TextAsset>(pathName).text : File.ReadAllText(pathName + ".txt");
        }

        /// <summary>
        /// 返回开/关文本
        /// </summary>
        private static string OpenOrClose(bool inputBool)
        {
            return TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, inputBool ? "Open" : "Close");
        }

        private void Update()
        {
            if (freeze)
                return;
            fps.text = MainControl.Instance.OverworldControl.openFPS ? UpdateFPS(fps.text) : "";

            if (GameUtilityService.KeyArrowToControl(KeyCode.Escape, 1))
            {
                if (_exitImage.color.a < 1)
                    _exitImage.color += Time.deltaTime * Color.white;
                if (_clock < 3)
                {
                    _exitImage.sprite = sprites[(int)_clock];
                    _exitImage.rectTransform.sizeDelta = sizes[(int)_clock];
                    _clock += Time.deltaTime;
                }
                else Application.Quit();
            }
            if (GameUtilityService.KeyArrowToControl(KeyCode.Escape, 2))
            {
                _clock = 0;
                _exitImage.color = new Color(1, 1, 1, 0);
            }

            //设置菜单
            if (_isSettingControl)
            {
                UpdateSettingDisplay(false, true);
                if (SettingControl() == KeyCode.None) return;
                var j = 0;
                switch (_keyPage)
                {
                    case 0:
                        j = 0;
                        goto default;
                    case 1:
                        j = 6;
                        goto default;
                    default:
                        var origin = KeyCode.None;

                        switch (_keyMapping)
                        {
                            case KeyMapping.MainKeyMap:
                                origin = MainControl.Instance.OverworldControl.keyCodes[_settingSelect + j];
                                MainControl.Instance.OverworldControl.keyCodes[_settingSelect + j] = SettingControl();
                                goto default;
                            case KeyMapping.SecondaryKeyMap1:
                                origin = MainControl.Instance.OverworldControl.keyCodesBack1[_settingSelect + j];
                                MainControl.Instance.OverworldControl.keyCodesBack1[_settingSelect + j] = SettingControl();
                                goto default;
                            case KeyMapping.SecondaryKeyMap2:
                                origin = MainControl.Instance.OverworldControl.keyCodesBack2[_settingSelect + j];
                                MainControl.Instance.OverworldControl.keyCodesBack2[_settingSelect + j] = SettingControl();
                                goto default;
                            default:
                                var keycodes = new List<KeyCode>
                                {
                                    MainControl.Instance.OverworldControl.keyCodes[_settingSelect + j],
                                    MainControl.Instance.OverworldControl.keyCodesBack1[_settingSelect + j],
                                    MainControl.Instance.OverworldControl.keyCodesBack2[_settingSelect + j]
                                };
                                for (var i = 0; i < MainControl.Instance.OverworldControl.keyCodes.Count; i++)
                                {
                                    if (MainControl.Instance.OverworldControl.keyCodes[i] != keycodes[(int)_keyMapping] ||
                                        i == _settingSelect + j) continue;
                                    MainControl.Instance.OverworldControl.keyCodes[i] = origin;
                                    break;
                                }
                                for (var i = 0; i < MainControl.Instance.OverworldControl.keyCodesBack1.Count; i++)
                                {
                                    if (MainControl.Instance.OverworldControl.keyCodesBack1[i] !=
                                        keycodes[(int)_keyMapping] || i == _settingSelect + j) continue;
                                    MainControl.Instance.OverworldControl.keyCodesBack1[i] = origin;
                                    break;
                                }
                                for (var i = 0; i < MainControl.Instance.OverworldControl.keyCodesBack2.Count; i++)
                                {
                                    if (MainControl.Instance.OverworldControl.keyCodesBack2[i] !=
                                        keycodes[(int)_keyMapping] || i == _settingSelect + j) continue;
                                    MainControl.Instance.OverworldControl.keyCodesBack2[i] = origin;
                                    break;
                                }
                                UpdateSettingDisplay();
                                break;
                        }

                        break;
                }
                _isSettingControl = false;

                return;
            }

            if ((openTurn && TurnController.Instance && TurnController.Instance.isMyTurn) || !openTurn)
            {
                if (SceneManager.GetActiveScene().name != "Story" && GameUtilityService.KeyArrowToControl(KeyCode.V) && !MainControl.Instance.OverworldControl.isSetting && !MainControl.Instance.isSceneSwitching)
                {
                    foreach (var typeWritter in _typeWritters)
                    {
                        typeWritter.TypePause(true);
                    }

                    OpenSetting();
                }
            }
            if (!MainControl.Instance.OverworldControl.isSetting)
                return;

            _settingSoul.rectTransform.anchoredPosition = new Vector2(-225f, 147.5f + _settingSelect * -37.5f);

            if (!(settingImage.color.a > 0.7)) return;
            switch (settingsLayer)
            {
                case 0:
                    if (!_isSettingName)
                    {
                        if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            _settingSelect++;
                            if (_settingSelect > 7)
                                _settingSelect = 0;
                        }
                        else if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                            _settingSelect--;
                            if (_settingSelect < 0)
                                _settingSelect = 7;
                        }
                    }
                    else
                    {
                        if (GameUtilityService.KeyArrowToControl(KeyCode.LeftArrow, 1) || GameUtilityService.KeyArrowToControl(KeyCode.DownArrow))
                        {
                            if (MainControl.Instance.OverworldControl.mainVolume > 0)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                MainControl.Instance.OverworldControl.mainVolume -= 0.01f;
                                AudioListener.volume = MainControl.Instance.OverworldControl.mainVolume;
                            }
                            UpdateSettingDisplay(false, true);
                        }
                        else if (GameUtilityService.KeyArrowToControl(KeyCode.RightArrow, 1) || GameUtilityService.KeyArrowToControl(KeyCode.UpArrow))
                        {
                            if (MainControl.Instance.OverworldControl.mainVolume < 1)
                            {
                                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                                MainControl.Instance.OverworldControl.mainVolume += 0.01f;
                                AudioListener.volume = MainControl.Instance.OverworldControl.mainVolume;
                            }
                            UpdateSettingDisplay(false, true);
                        }
                    }

                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        if (!_isSettingName)
                            switch (_settingSelect)
                            {
                                case 0:
                                    _saveVolume = MainControl.Instance.OverworldControl.mainVolume;
                                    _isSettingName = true;
                                    UpdateSettingDisplay(false, true);
                                    break;

                                case 1:
                                    settingsLayer = SettingsLayer.KeyConfiguration;
                                    UpdateSettingDisplay();
                                    _settingSelect = 0;
                                    break;

                                case 2:
                                    MainControl.Instance.OverworldControl.fullScreen = !MainControl.Instance.OverworldControl.fullScreen;
                                    GameUtilityService.SetResolution(MainControl.Instance.OverworldControl.resolutionLevel);

                                    goto default;
                                case 3:
                                    MainControl.Instance.OverworldControl.resolutionLevel = GameUtilityService.UpdateResolutionSettings(MainControl.Instance.OverworldControl.isUsingHDFrame, MainControl.Instance.OverworldControl.resolutionLevel);
                                    goto default;
                                case 4:
                                    MainControl.Instance.OverworldControl.noSfx = !MainControl.Instance.OverworldControl.noSfx;
                                    GameUtilityService.ToggleAllSfx(MainControl.Instance.OverworldControl.noSfx);
                                    PlayerPrefs.SetInt("noSFX", Convert.ToInt32(MainControl.Instance.OverworldControl.noSfx));
                                    goto default;
                                case 5:
                                    MainControl.Instance.OverworldControl.openFPS = !MainControl.Instance.OverworldControl.openFPS;
                                    goto default;
                                case 6:
                                    if (SceneManager.GetActiveScene().name == "Rename")
                                        return;
                                    if (SceneManager.GetActiveScene().name == "Menu")
                                        goto case 7;
                                    GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black, true, AnimSpeed);
                                    MainControl.Instance.OverworldControl.isSetting = false;
                                    foreach (var typeWritter in _typeWritters)
                                    {
                                        typeWritter.TypePause(false);
                                    }
                                    freeze = true;
                                    break;
                                case 7:
                                    ExitSetting();
                                    break;

                                default:
                                    UpdateSettingDisplay();
                                    break;
                            }
                        else
                        {
                            UpdateSettingDisplay();
                            _isSettingName = false;
                        }
                    }
                    else if (SceneManager.GetActiveScene().name != "Story" && (GameUtilityService.KeyArrowToControl(KeyCode.X) || GameUtilityService.KeyArrowToControl(KeyCode.V)))
                    {
                        if (!_isSettingName)
                        {
                            ExitSetting();
                        }
                        else
                        {
                            MainControl.Instance.OverworldControl.mainVolume = _saveVolume;
                            AudioListener.volume = MainControl.Instance.OverworldControl.mainVolume;
                            UpdateSettingDisplay();
                            _isSettingName = false;
                        }
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.C))
                    {
                        if (!_isSettingName)
                        {
                            switch (_settingSelect)
                            {
                                case 2:
                                {
                                    AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                    if ((int)MainControl.Instance.OverworldControl.vsyncMode < 2)
                                        MainControl.Instance.OverworldControl.vsyncMode++;
                                    else
                                        MainControl.Instance.OverworldControl.vsyncMode = OverworldControl.VSyncMode.DonNotSync;

                                    PlayerPrefs.SetInt("vsyncMode", Convert.ToInt32(MainControl.Instance.OverworldControl.vsyncMode));
                                    break;
                                }
                                case 3:
                                    AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                                    MainControl.Instance.OverworldControl.isUsingHDFrame = !MainControl.Instance.OverworldControl.isUsingHDFrame;
                                    MainControl.Instance.OverworldControl.resolutionLevel = GameUtilityService.UpdateResolutionSettings(MainControl.Instance.OverworldControl.isUsingHDFrame, MainControl.Instance.OverworldControl.resolutionLevel);
                                    UpdateSettingDisplay();
                                    PlayerPrefs.SetInt("hdResolution", Convert.ToInt32(MainControl.Instance.OverworldControl.isUsingHDFrame));
                                    break;
                            }
                        }
                    }

                    var textForUnder = "";
                    switch (_settingSelect)
                    {
                        case 0:
                            textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingMainVolumeTip");
                            break;

                        case 1:
                            textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingControlTip");
                            break;

                        case 2:
                            string vsyncModeAdd;
                            switch (MainControl.Instance.OverworldControl.vsyncMode)
                            {
                                case OverworldControl.VSyncMode.DonNotSync:
                                    vsyncModeAdd = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "VSyncNone");
                                    break;

                                case OverworldControl.VSyncMode.SyncToRefreshRate:
                                    vsyncModeAdd = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "VSyncFull");
                                    break;

                                case OverworldControl.VSyncMode.HalfSync:
                                    vsyncModeAdd = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "VSyncHalf");
                                    break;

                                default:
                                    goto case OverworldControl.VSyncMode.DonNotSync;
                            }

                            if (!MainControl.Instance.OverworldControl.fullScreen)
                                textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingFullScreenTipOpen") + "\n" + vsyncModeAdd;
                            else
                                textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingFullScreenTipClose") + "\n" + vsyncModeAdd;
                            break;

                        case 3:
                            textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, !MainControl.Instance.OverworldControl.isUsingHDFrame ? "SettingResolvingTip" : "SettingResolvingTipHD");

                            break;

                        case 4:
                            textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingSFXTip");
                            break;

                        case 5:
                            textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingFPSTip");
                            break;

                        case 6:
                            textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingBackMenuTip");
                            break;

                        case 7:
                            textForUnder = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "SettingBackGameTip");
                            break;
                    }
                    _settingTmpUnder.text = textForUnder;
                    break;

                case SettingsLayer.KeyConfiguration:
                    if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        _settingSelect++;
                        if (_settingSelect > 8)
                            _settingSelect = 0;
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        _settingSelect--;
                        if (_settingSelect < 0)
                            _settingSelect = 8;
                    }
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        switch (_settingSelect)
                        {
                            case < 6:
                                _isSettingControl = true;
                                break;
                            case 6:
                                _keyPage = _keyPage switch
                                {
                                    0 => 1,
                                    1 => 0,
                                    _ => _keyPage
                                };
                                break;
                            case 7:
                                GameUtilityService.ApplyDefaultControl();
                                break;
                            default:
                                settingsLayer = 0;
                                _settingSelect = 0;

                                UpdateSettingDisplay();
                                return;
                        }

                        UpdateSettingDisplay(false, true);
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.X))
                    {
                        settingsLayer = 0;
                        _settingSelect = 0;

                        UpdateSettingDisplay();
                        return;
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.C))
                    {
                        AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
                        if ((int)_keyMapping < 2)
                            _keyMapping++;
                        else
                            _keyMapping = 0;

                        UpdateSettingDisplay();
                    }
                    _settingTmpUnder.text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "ControlUnder" + (int)_keyMapping);

                    break;

                case SettingsLayer.LanguageConfiguration:
                    if (GameUtilityService.KeyArrowToControl(KeyCode.DownArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        _settingSelect++;
                        if (_settingSelect > _settingSelectMax)
                            _settingSelect = 0;
                    }
                    else if (GameUtilityService.KeyArrowToControl(KeyCode.UpArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        _settingSelect--;
                        if (_settingSelect < 0)
                            _settingSelect = _settingSelectMax;
                    }
                    if (GameUtilityService.KeyArrowToControl(KeyCode.Z))
                    {
                        if (_settingSelect != _settingSelectMax)
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                            UpdateSettingDisplay(false, true);
                            MainControl.Instance.languagePackId = _settingSelect;
                        }
                        else
                        {
                            ExitSetting(true);
                        }
                    }
                    else if (SceneManager.GetActiveScene().name != "Story" && (GameUtilityService.KeyArrowToControl(KeyCode.X) || GameUtilityService.KeyArrowToControl(KeyCode.V)))
                    {
                        ExitSetting(true);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private static KeyCode SettingControl()
        {
            return !Input.anyKeyDown ? KeyCode.None : Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>().Where(_ => !Input.GetMouseButtonDown(0) && !Input.GetMouseButtonDown(1) && !Input.GetMouseButtonDown(2)).FirstOrDefault(Input.GetKeyDown);
        }

        /// <summary>
        /// 进入设置页面
        /// </summary>
        public void OpenSetting()
        {
            MainControl.Instance.OverworldControl.isSetting = true;
            settingImage.DOColor(new Color(0, 0, 0, 0.75f), AnimSpeed);
            //DOTween.To(() => settingImage.rectTransform.sizeDelta, x => settingImage.rectTransform.sizeDelta = x, new Vector2(6000, settingImage.rectTransform.sizeDelta.y), animSpeed).SetEase(Ease.InCirc);
            _settingTmp.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpSon.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpUnder.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(Color.red, AnimSpeed).SetEase(Ease.Linear);
            //DOTween.To(() => _settingTmp.rectTransform.anchoredPosition, x => _settingTmp.rectTransform.anchoredPosition = x, new Vector2(140, 140), AnimSpeed + 0.25f).SetEase(Ease.OutCubic);
            _settingSelect = 0;
            _settingTmpUnder.text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "ControlEggshell");
            UpdateSettingDisplay();
            if (settingsLayer == SettingsLayer.LanguageConfiguration)
                UpdateSettingDisplay(true);
        }
        /// <summary>
        /// 退出设置页面
        /// </summary>
        
        private void ExitSetting(bool isLan = false)
        {
            settingsLayer = 0;
            settingImage.DOColor(Color.clear, AnimSpeed);
            _settingTmp.DOColor(Color.clear, AnimSpeed).SetEase(Ease.Linear).OnKill(() =>
            {
                MainControl.Instance.OverworldControl.isSetting = false;
                foreach (var typeWritter in _typeWritters)
                {
                    typeWritter.TypePause(false);
                }
                if (isLan)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
            _settingTmpSon.DOColor(Color.clear, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpUnder.DOColor(Color.clear, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(new Color(1, 0, 0, 0), AnimSpeed).SetEase(Ease.Linear);
            _settingTmpUnder.text = TextProcessingService.GetFirstChildStringByPrefix(MainControl.Instance.OverworldControl.settingSave, "ControlEggshell");
        }

        private float _mLastUpdateShowTime;  //上一次更新帧率的时间;
        private const float MUpdateShowDeltaTime = 0.2f;//更新帧率的时间间隔;
        private int _mFrameUpdate;//帧数;
        private float _mFPS;//帧率

        /// <summary>
        /// 计算并返回当前帧率的字符串表示，每隔指定时间刷新一次。
        /// </summary>
        /// <param name="input">未到间隔时间时返回input</param>
        /// <returns>当前帧率（FPS）的整数形式的字符串表示。</returns>
        private string UpdateFPS(string input)
        {
            _mFrameUpdate++;
            if (!(Time.realtimeSinceStartup - _mLastUpdateShowTime >= MUpdateShowDeltaTime)) return input;
            //FPS = 某段时间内的总帧数 / 某段时间
            _mFPS = _mFrameUpdate / (Time.realtimeSinceStartup - _mLastUpdateShowTime);
            _mFrameUpdate = 0;
            _mLastUpdateShowTime = Time.realtimeSinceStartup;
            return ((int)_mFPS).ToString();
        }
        /// <summary>
        /// Anim调用
        /// </summary>
        public void AnimSetHeartPos()
        {
            var uiPos = WorldToUGUI(MainControl.Instance.OverworldControl.playerDeadPos);
            transform.Find("Heart").GetComponent<RectTransform>().anchoredPosition = uiPos;
        }

        private Vector2 WorldToUGUI(Vector3 position)
        {
            var canvasRectTransform = GetComponent<RectTransform>();
            if (Camera.main == null)
            {
                Other.Debug.LogError("未找到主摄像机！");
                return position;
            }

            Vector2 screenPoint = Camera.main.WorldToScreenPoint(position); //世界坐标转换为屏幕坐标
            var screenSize = new Vector2(Screen.width, Screen.height);
            screenPoint -= screenSize / 2; //将屏幕坐标变换为以屏幕中心为原点
            var anchorPos = screenPoint / screenSize * canvasRectTransform.sizeDelta; //缩放得到UGUI坐标

            return anchorPos;
        }

        public void AnimSetHeartRed(int isRed)
        {
            transform.Find("Heart").GetComponent<Image>().color = Convert.ToBoolean(isRed) ? Color.red : Color.clear;
        }

        public void AnimHeartGo()
        {
            var i = transform.Find("Heart").GetComponent<RectTransform>();
            var j = i.GetComponent<Image>();
            j.DOColor(new Color(j.color.r, j.color.g, j.color.b, 0), AnimSpeed).SetEase(Ease.Linear);
            DOTween.To(() => i.anchoredPosition, x => i.anchoredPosition = x, new Vector2(-330, -250), 1.5f).SetEase(Ease.OutCirc).OnKill(AnimOpen);
        }

        public void PlayFX(int i)
        {
            AudioController.Instance.GetFx(i, MainControl.Instance.AudioControl.fxClipUI);
        }

        private void AnimOpen()
        {
            animator.SetBool(Open, false);
            GameUtilityService.FadeOutAndSwitchScene("Battle", Color.black, false, -0.5f);
        }
    }
}

