using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Alchemy.Inspector;
using DG.Tweening;
using TMPro;
using UCT.Battle;
using UCT.Control;
using UCT.Extensions;
using UCT.Global.Audio;
using UCT.Global.Core;
using UCT.Global.UI;
using UCT.Service;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UCT.Global.Settings
{
    /// <summary>
    ///     设置界面
    /// </summary>
   
    public class CanvasController : MonoBehaviour
    {
        public static CanvasController Instance;

        public enum SettingsLayer
        {
            Home, //主层级
            KeyConfiguration, //按键设置层级
            LanguageConfiguration //语言包层级
        }

        private const int SettingSelectMax = 7;
        private const float AnimSpeed = 0.25f;
        private static readonly int Open = Animator.StringToHash("Open");

        [FormerlySerializedAs("framePic")]
        public int frameSpriteIndex;

        [ReadOnly]
        public RenderMode renderMode;
        [ShowInInspector][ReadOnly] 
        private bool _isPauseCanvas; //防止切场景时整事儿
        public Image Frame { get; private set; }
        public Animator Animator { get; private set; }

        private static OverworldControl _overworldControl;
        private Canvas _canvas;
        private Image _settingPageBackground;
        private SettingsLayer _settingsLayer;
        private bool _isSettingKey;
        private bool _isSettingName; //是否选中
        private KeyMapping _keyMapping;
        private int _settingKeyPage;
        private int _languagePackSelectMax; //目前 Max仅用于配置语言包
        private float _saveVolume;
        private int _settingSelect;
        private Image _settingSoul;
        private TextMeshProUGUI _settingTmp, _settingTmpSon, _settingTmpBottom;

        private void Awake()
        {
            Instance = this;
            Animator = GetComponent<Animator>();
            _canvas = GetComponent<Canvas>();
            _settingPageBackground = transform.Find("Setting").GetComponent<Image>();
            _settingTmp = transform.Find("Setting/Setting Text").GetComponent<TextMeshProUGUI>();
            _settingTmpSon = _settingTmp.transform.Find("Setting Son").GetComponent<TextMeshProUGUI>();
            _settingSoul = _settingTmp.transform.Find("Soul").GetComponent<Image>();
            _settingTmpBottom = _settingTmp.transform.Find("Setting Under").GetComponent<TextMeshProUGUI>();
            Frame = transform.Find("Frame").GetComponent<Image>();
        }

        public void Start()
        {
            _settingsLayer = SettingsLayer.Home;
            _settingPageBackground.color = Color.clear;
            _settingTmp.color = ColorEx.WhiteClear;
            _settingTmpSon.color = ColorEx.WhiteClear;
            _settingTmpBottom.color = ColorEx.WhiteClear;
            _settingSoul.color = ColorEx.RedClear;
            _isPauseCanvas = false;

            _canvas.renderMode = renderMode;

            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _canvas.worldCamera = Camera.main;
            if (!_overworldControl)
                _overworldControl = MainControl.Instance.overworldControl;

            UpdateSettingDisplay();
        }

        private void Update()
        {
            if (_isPauseCanvas) return;
            if (IsSettingKey()) return;
            if (IsInBattleAndNotMyTurn()) return;
            if (IsInScene("Story")) return;
            
            // 设置菜单
            if (InputService.GetKeyDown(KeyCode.V) &&
                !_overworldControl.isSetting && !MainControl.Instance.isSceneSwitching)
            {
                TypeWritter.TypePause(true);
                OpenSetting();
            }

            if (!_overworldControl.isSetting) return;
            _settingSoul.rectTransform.anchoredPosition = new Vector2(-225f, 147.5f + _settingSelect * -37.5f);
            if (!(_settingPageBackground.color.a > 0.7)) return;

            switch (_settingsLayer)
            {
                case SettingsLayer.Home:
                    HomeLayer();
                    break;
                case SettingsLayer.KeyConfiguration:
                    KeyConfigurationLayer();
                    break;
                case SettingsLayer.LanguageConfiguration:
                    LanguageConfigurationLayer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void LanguageConfigurationLayer()
        {
            if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                _settingSelect++;
                if (_settingSelect > _languagePackSelectMax)
                    _settingSelect = 0;
            }
            else if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                _settingSelect--;
                if (_settingSelect < 0)
                    _settingSelect = _languagePackSelectMax;
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                if (_settingSelect != _languagePackSelectMax)
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
            else if (SceneManager.GetActiveScene().name != "Story" &&
                     (InputService.GetKeyDown(KeyCode.X) ||
                      InputService.GetKeyDown(KeyCode.V)))
            {
                ExitSetting(true);
            }

        }

        private void KeyConfigurationLayer()
        {
            if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                _settingSelect++;
                if (_settingSelect > 8)
                    _settingSelect = 0;
            }
            else if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                _settingSelect--;
                if (_settingSelect < 0)
                    _settingSelect = 8;
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                switch (_settingSelect)
                {
                    case < 6:
                    {
                        _isSettingKey = true;
                        break;
                    }
                    case 6:
                    {
                        _settingKeyPage = _settingKeyPage switch
                        {
                            0 => 1,
                            1 => 0,
                            _ => _settingKeyPage
                        };
                        break;
                    }
                    case 7:
                    {
                        _overworldControl.KeyCodes =
                            InputService.ApplyDefaultControl();
                        break;
                    }
                    default:
                    {
                        _settingsLayer = 0;
                        _settingSelect = 0;

                        UpdateSettingDisplay();
                        return;
                    }
                }

                UpdateSettingDisplay(false, true);
            }
            else if (InputService.GetKeyDown(KeyCode.X))
            {
                _settingsLayer = 0;
                _settingSelect = 0;

                UpdateSettingDisplay();
                return;
            }
            else if (InputService.GetKeyDown(KeyCode.C))
            {
                AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
                if ((int)_keyMapping < 2)
                    _keyMapping++;
                else
                    _keyMapping = 0;

                UpdateSettingDisplay();
            }

            _settingTmpBottom.text = TextProcessingService.GetFirstChildStringByPrefix(
                _overworldControl.settingSave, "ControlUnder" + (int)_keyMapping);

        }

        private void HomeLayer()
        {
            if (!_isSettingName)
            {
                if (InputService.GetKeyDown(KeyCode.DownArrow))
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    _settingSelect++;
                    if (_settingSelect > SettingSelectMax)
                        _settingSelect = 0;
                }
                else if (InputService.GetKeyDown(KeyCode.UpArrow))
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    _settingSelect--;
                    if (_settingSelect < 0)
                        _settingSelect = SettingSelectMax;
                }
            }
            else
            {
                if (InputService.GetKey(KeyCode.LeftArrow) ||
                    InputService.GetKeyDown(KeyCode.DownArrow))
                {
                    if (_overworldControl.mainVolume > 0)
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        _overworldControl.mainVolume -= 0.01f;
                        AudioListener.volume = _overworldControl.mainVolume;
                    }

                    UpdateSettingDisplay(false, true);
                }
                else if (InputService.GetKey(KeyCode.RightArrow) ||
                         InputService.GetKeyDown(KeyCode.UpArrow))
                {
                    if (_overworldControl.mainVolume < 1)
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        _overworldControl.mainVolume += 0.01f;
                        AudioListener.volume = _overworldControl.mainVolume;
                    }

                    UpdateSettingDisplay(false, true);
                }
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                if (!_isSettingName)
                {
                    switch (_settingSelect)
                    {
                        case 0:
                        {
                            _saveVolume = _overworldControl.mainVolume;
                            _isSettingName = true;
                            UpdateSettingDisplay(false, true);
                            break;
                        }

                        case 1:
                        {
                            _settingsLayer = SettingsLayer.KeyConfiguration;
                            UpdateSettingDisplay();
                            _settingSelect = 0;
                            break;
                        }

                        case 2:
                        {
                            _overworldControl.fullScreen =
                                !_overworldControl.fullScreen;
                            GameUtilityService.SetResolution(_overworldControl
                                .resolutionLevel);

                            goto default;
                        }
                        case 3:
                        {
                            _overworldControl.resolutionLevel =
                                GameUtilityService.UpdateResolutionSettings(
                                    _overworldControl.isUsingHDFrame,
                                    _overworldControl.resolutionLevel);
                            goto default;
                        }
                        case 4:
                        {
                            _overworldControl.noSfx =
                                !_overworldControl.noSfx;
                            GameUtilityService.ToggleAllSfx(_overworldControl.noSfx);
                            PlayerPrefs.SetInt("noSFX",
                                Convert.ToInt32(_overworldControl.noSfx));
                            goto default;
                        }
                        case 5:
                        {
                            _overworldControl.openFPS =
                                !_overworldControl.openFPS;
                            goto default;
                        }
                        case 6:
                        {
                            if (SceneManager.GetActiveScene().name == "Rename")
                                return;
                            if (SceneManager.GetActiveScene().name == "Menu")
                                goto case 7;
                            GameUtilityService.FadeOutAndSwitchScene("Menu", Color.black, true, AnimSpeed);
                            _overworldControl.isSetting = false;
                            TypeWritter.TypePause(false);
                            _isPauseCanvas = true;
                            break;
                        }
                        case 7:
                        {
                            ExitSetting();
                            break;
                        }

                        default:
                        {
                            UpdateSettingDisplay();
                            break;
                        }
                    }
                }
                else
                {
                    UpdateSettingDisplay();
                    _isSettingName = false;
                }
            }
            else if (SceneManager.GetActiveScene().name != "Story" &&
                     (InputService.GetKeyDown(KeyCode.X) ||
                      InputService.GetKeyDown(KeyCode.V)))
            {
                if (!_isSettingName)
                {
                    ExitSetting();
                }
                else
                {
                    _overworldControl.mainVolume = _saveVolume;
                    AudioListener.volume = _overworldControl.mainVolume;
                    UpdateSettingDisplay();
                    _isSettingName = false;
                }
            }
            else if (InputService.GetKeyDown(KeyCode.C))
            {
                if (!_isSettingName)
                    switch (_settingSelect)
                    {
                        case 2:
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                            if ((int)_overworldControl.vsyncMode < 2)
                                _overworldControl.vsyncMode++;
                            else
                                _overworldControl.vsyncMode =
                                    OverworldControl.VSyncMode.DonNotSync;

                            PlayerPrefs.SetInt("vsyncMode",
                                Convert.ToInt32(_overworldControl.vsyncMode));
                            break;
                        }
                        case 3:
                        {
                            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                            _overworldControl.isUsingHDFrame =
                                !_overworldControl.isUsingHDFrame;
                            _overworldControl.resolutionLevel =
                                GameUtilityService.UpdateResolutionSettings(
                                    _overworldControl.isUsingHDFrame,
                                    _overworldControl.resolutionLevel);
                            UpdateSettingDisplay();
                            PlayerPrefs.SetInt("hdResolution",
                                Convert.ToInt32(_overworldControl.isUsingHDFrame));
                            break;
                        }
                    }
            }

            var textForUnder = "";
            switch (_settingSelect)
            {
                case 0:
                {
                    textForUnder = TextProcessingService.GetFirstChildStringByPrefix(
                        _overworldControl.settingSave, "SettingMainVolumeTip");
                    break;
                }

                case 1:
                {
                    textForUnder =
                        TextProcessingService.GetFirstChildStringByPrefix(
                            _overworldControl.settingSave, "SettingControlTip");
                    break;
                }

                case 2:
                {
                    string vsyncModeAdd;
                    switch (_overworldControl.vsyncMode)
                    {
                        case OverworldControl.VSyncMode.DonNotSync:
                        {
                            vsyncModeAdd =
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    _overworldControl.settingSave, "VSyncNone");
                            break;
                        }

                        case OverworldControl.VSyncMode.SyncToRefreshRate:
                        {
                            vsyncModeAdd =
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    _overworldControl.settingSave, "VSyncFull");
                            break;
                        }

                        case OverworldControl.VSyncMode.HalfSync:
                        {
                            vsyncModeAdd =
                                TextProcessingService.GetFirstChildStringByPrefix(
                                    _overworldControl.settingSave, "VSyncHalf");
                            break;
                        }

                        default:
                        {
                            goto case OverworldControl.VSyncMode.DonNotSync;
                        }
                    }

                    if (!_overworldControl.fullScreen)
                        textForUnder =
                            TextProcessingService.GetFirstChildStringByPrefix(
                                _overworldControl.settingSave, "SettingFullScreenTipOpen") +
                            "\n" + vsyncModeAdd;
                    else
                        textForUnder =
                            TextProcessingService.GetFirstChildStringByPrefix(
                                _overworldControl.settingSave,
                                "SettingFullScreenTipClose") + "\n" + vsyncModeAdd;
                    break;
                }

                case 3:
                {
                    textForUnder = TextProcessingService.GetFirstChildStringByPrefix(
                        _overworldControl.settingSave,
                        !_overworldControl.isUsingHDFrame
                            ? "SettingResolvingTip"
                            : "SettingResolvingTipHD");

                    break;
                }

                case 4:
                {
                    textForUnder =
                        TextProcessingService.GetFirstChildStringByPrefix(
                            _overworldControl.settingSave, "SettingSFXTip");
                    break;
                }

                case 5:
                {
                    textForUnder =
                        TextProcessingService.GetFirstChildStringByPrefix(
                            _overworldControl.settingSave, "SettingFPSTip");
                    break;
                }

                case 6:
                {
                    textForUnder = TextProcessingService.GetFirstChildStringByPrefix(
                        _overworldControl.settingSave, "SettingBackMenuTip");
                    break;
                }

                case 7:
                {
                    textForUnder = TextProcessingService.GetFirstChildStringByPrefix(
                        _overworldControl.settingSave, "SettingBackGameTip");
                    break;
                }
            }

            _settingTmpBottom.text = textForUnder;
        }

        private bool IsSettingKey()
        {
            if (!_isSettingKey) return false;
            UpdateSettingDisplay(false, true);
            if (GetSettingControlKey() == KeyCode.None) return true;
            var j = 0;
            switch (_settingKeyPage)
            {
                case 0:
                {
                    j = 0;
                    goto default;
                }
                case 1:
                {
                    j = 6;
                    goto default;
                }
                default:
                {
                    var origin = KeyCode.None;

                    switch (_keyMapping)
                    {
                        case KeyMapping.MainKeyMap:
                        {
                            origin = _overworldControl.KeyCodes[0][_settingSelect + j];
                            _overworldControl.KeyCodes[0][_settingSelect + j] =
                                GetSettingControlKey();
                            goto default;
                        }
                        case KeyMapping.SecondaryKeyMap1:
                        {
                            origin = _overworldControl.KeyCodes[1][_settingSelect + j];
                            _overworldControl.KeyCodes[1][_settingSelect + j] =
                                GetSettingControlKey();
                            goto default;
                        }
                        case KeyMapping.SecondaryKeyMap2:
                        {
                            origin = _overworldControl.KeyCodes[2][_settingSelect + j];
                            _overworldControl.KeyCodes[2][_settingSelect + j] =
                                GetSettingControlKey();
                            goto default;
                        }
                        default:
                        {
                            var keyCodes = new List<KeyCode>
                            {
                                _overworldControl.KeyCodes[0][_settingSelect + j],
                                _overworldControl.KeyCodes[1][_settingSelect + j],
                                _overworldControl.KeyCodes[2][_settingSelect + j]
                            };
                            for (var i = 0; i < _overworldControl.KeyCodes[0].Count; i++)
                            {
                                if (_overworldControl.KeyCodes[0][i] !=
                                    keyCodes[(int)_keyMapping] ||
                                    i == _settingSelect + j) continue;
                                _overworldControl.KeyCodes[0][i] = origin;
                                break;
                            }

                            for (var i = 0; i < _overworldControl.KeyCodes[1].Count; i++)
                            {
                                if (_overworldControl.KeyCodes[1][i] !=
                                    keyCodes[(int)_keyMapping] || i == _settingSelect + j) continue;
                                _overworldControl.KeyCodes[1][i] = origin;
                                break;
                            }

                            for (var i = 0; i < _overworldControl.KeyCodes[2].Count; i++)
                            {
                                if (_overworldControl.KeyCodes[2][i] !=
                                    keyCodes[(int)_keyMapping] || i == _settingSelect + j) continue;
                                _overworldControl.KeyCodes[2][i] = origin;
                                break;
                            }

                            UpdateSettingDisplay();
                            break;
                        }
                    }

                    break;
                }
            }

            _isSettingKey = false;

            return true;

        }

        private static bool IsInScene(string sceneName)
        {
            return SceneManager.GetActiveScene().name == sceneName;
        }

        private static bool IsInBattleAndNotMyTurn()
        {
            var isInBattle = MainControl.Instance.sceneState == MainControl.SceneState.InBattle;
            var isInMyTurn = TurnController.Instance && TurnController.Instance.isMyTurn;
            return isInBattle && !isInMyTurn;
        }


        /// <summary>
        ///     更新当前设置层的显示内容。根据设置层调用相应的配置方法。
        /// </summary>
        /// <param name="isOnlySetSon">是否仅设置子项。</param>
        /// <param name="isSetting">是否正在进行设置。</param>
        private void UpdateSettingDisplay(bool isOnlySetSon = false, bool isSetting = false)
        {
            switch (_settingsLayer)
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

        /// <summary>
        ///     设置Home页配置
        /// </summary>
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
                        _overworldControl.settingSave, key)).ToList();

                if (isSetting)
                    settingsText[1] = $"<color=yellow>{settingsText[1]}</color>"; // 为 SettingMainVolume 添加颜色

                _settingTmp.text = string.Join("\n", settingsText);
            }

            if (!isSetting)
                _settingTmpSon.text = "\n" +
                                      (int)(_overworldControl.mainVolume * 100) + "%\n\n" +
                                      GetOpenOrCloseString(_overworldControl.fullScreen) + '\n' +
                                      _overworldControl.resolution.x + '×' +
                                      _overworldControl.resolution.y + '\n' +
                                      GetOpenOrCloseString(_overworldControl.noSfx) + '\n' +
                                      GetOpenOrCloseString(_overworldControl.openFPS) + '\n';
            else
                _settingTmpSon.text = "\n" + "<color=yellow>" +
                                      (int)(_overworldControl.mainVolume * 100) + "%</color>\n\n" +
                                      GetOpenOrCloseString(_overworldControl.fullScreen) + '\n' +
                                      _overworldControl.resolution.x + '×' +
                                      _overworldControl.resolution.y + '\n' +
                                      GetOpenOrCloseString(_overworldControl.noSfx) + '\n' +
                                      GetOpenOrCloseString(_overworldControl.openFPS) + '\n';
        }

        /// <summary>
        ///     设置键位页配置
        /// </summary>
        private void KeyConfiguration(bool isSetting)
        {
            var strings = new List<string>();

            for (var i = 0; i < 6; i++)
                if (isSetting && i == _settingSelect)
                    strings.Add("<color=yellow>");
                else
                    strings.Add("");

            List<string> prefixes;
            StringBuilder result;
            switch (_settingKeyPage)
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
                        result.Append(
                            TextProcessingService.GetFirstChildStringByPrefix(
                                _overworldControl.settingSave, prefixes[i]));
                        if (i < prefixes.Count - 1)
                            result.Append("</color>\n");
                    }

                    _settingTmp.text = result.ToString();

                    _settingTmpSon.text = "\n";
                    for (var i = 0; i < 6; i++)
                    {
                        if (isSetting && i == _settingSelect) _settingTmpSon.text += "<color=yellow>";

                        _settingTmpSon.text += _keyMapping switch
                        {
                            KeyMapping.MainKeyMap => _overworldControl.KeyCodes[0][i] +
                                                     "</color>\n",
                            KeyMapping.SecondaryKeyMap1 => _overworldControl.KeyCodes[1]
                                [i] + "</color>\n",
                            KeyMapping.SecondaryKeyMap2 => _overworldControl.KeyCodes[2]
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
                        result.Append(
                            TextProcessingService.GetFirstChildStringByPrefix(
                                _overworldControl.settingSave, prefixes[i]));
                        if (i < prefixes.Count - 3)
                            result.Append("</color>\n");
                        else if (i < prefixes.Count - 1)
                            result.Append("\n");
                    }

                    _settingTmp.text = result.ToString();

                    _settingTmpSon.text = "\n";
                    for (var i = 6; i < 12; i++)
                    {
                        if (isSetting && i - 6 == _settingSelect) _settingTmpSon.text += "<color=yellow>";

                        _settingTmpSon.text += _keyMapping switch
                        {
                            KeyMapping.MainKeyMap => _overworldControl.KeyCodes[0][i] +
                                                     "</color>\n",
                            KeyMapping.SecondaryKeyMap1 => _overworldControl.KeyCodes[1]
                                [i] + "</color>\n",
                            KeyMapping.SecondaryKeyMap2 => _overworldControl.KeyCodes[2]
                                [i] + "</color>\n",
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    }

                    break;
            }
        }

        /// <summary>
        ///     设置语言包页配置
        /// </summary>
        private void LanguageConfiguration(bool onlySetSon, bool isSetting)
        {
            var pathStringSaver = "";

            if (isSetting)
                MainControl.Instance.Initialization(_settingSelect);

            if (!onlySetSon)
                _settingTmp.text =
                    TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                        "LanguagePack") + '\n';
            _settingTmpSon.text = "";
            _languagePackSelectMax = 0;
            var settingSelectBack = _settingSelect;
            if (onlySetSon)
                _settingSelect = MainControl.Instance.languagePackId;

            for (var i = 0; i < MainControl.LanguagePackInsideNumber; i++) //内置包信息
            {
                var pathString = "TextAssets/LanguagePacks/" + DataHandlerService.GetLanguageInsideId(i);

                if (_languagePackSelectMax == _settingSelect) pathStringSaver = pathString;
                _languagePackSelectMax++;

                if (!onlySetSon)
                    _settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", false) + '\n';
            }

            foreach (var pathString in Directory.GetDirectories(Application.dataPath + "\\LanguagePacks"))
            {
                if (_languagePackSelectMax == _settingSelect)
                    pathStringSaver = pathString;
                _languagePackSelectMax++;
                if (!onlySetSon)
                    _settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", true) + '\n';
            }

            if (!onlySetSon)
                _settingTmp.text +=
                    TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                        "Back");

            _settingTmpBottom.text =
                GetLanguagePacksName(pathStringSaver, "LanguagePackInformation",
                    _settingSelect >= MainControl.LanguagePackInsideNumber) + '\n' + GetLanguagePacksName(
                    pathStringSaver, "LanguagePackAuthor", _settingSelect >= MainControl.LanguagePackInsideNumber);

            _settingSelect = settingSelectBack;
        }

        /// <summary>
        ///     获取语言包信息
        /// </summary>
        /// <param name="pathString">语言包路径</param>
        /// <param name="returnString">返回的字符串标识</param>
        /// <param name="isOutSide">是否为外部路径</param>
        private static string GetLanguagePacksName(string pathString, string returnString, bool isOutSide)
        {
            var strings =
                DataHandlerService.LoadItemData(ReadFile(pathString + "\\LanguagePackInformation", isOutSide));
            strings = DataHandlerService.ChangeItemData(strings, true, new List<string>());
            return TextProcessingService.GetFirstChildStringByPrefix(strings, returnString);
        }

        private static string ReadFile(string pathName, bool isOutSide)
        {
            return !isOutSide ? Resources.Load<TextAsset>(pathName).text : File.ReadAllText(pathName + ".txt");
        }

        /// <summary>
        ///     返回开/关文本
        /// </summary>
        private static string GetOpenOrCloseString(bool inputBool)
        {
            return TextProcessingService.GetFirstChildStringByPrefix
                (_overworldControl.settingSave, inputBool ? "Open" : "Close");
        }

        /// <summary>
        ///     检测并返回按下的设置控制键
        /// </summary>
        /// <returns>按下的键</returns>
        private static KeyCode GetSettingControlKey()
        {
            return !Input.anyKeyDown
                ? KeyCode.None
                : Enum.GetValues(typeof(KeyCode))
                    .Cast<KeyCode>()
                    .Where(_ => !Input.GetMouseButtonDown(0) &&
                                !Input.GetMouseButtonDown(1) &&
                                !Input.GetMouseButtonDown(2))
                    .FirstOrDefault(Input.GetKeyDown);
        }

        /// <summary>
        ///     进入设置页面
        /// </summary>
        public void OpenSetting()
        {
            _overworldControl.isSetting = true;
            _settingPageBackground.DOColor(new Color(0, 0, 0, 0.75f), AnimSpeed);
            _settingTmp.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpSon.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpBottom.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(Color.red, AnimSpeed).SetEase(Ease.Linear);
            _settingSelect = 0;
            _settingTmpBottom.text =
                TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                    "ControlBottomText");
            if (_settingsLayer != SettingsLayer.LanguageConfiguration)
                UpdateSettingDisplay();
            else
                UpdateSettingDisplay(true);
        }

        /// <summary>
        ///     退出设置页面
        /// </summary>
        private void ExitSetting(bool isLan = false)
        {
            _settingsLayer = 0;
            _settingPageBackground.DOColor(Color.clear, AnimSpeed);
            _settingTmp.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear).OnKill(() =>
            {
                _overworldControl.isSetting = false;
                TypeWritter.TypePause(false);

                if (isLan)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
            _settingTmpSon.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpBottom.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(ColorEx.RedClear, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpBottom.text =
                TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                    "ControlBottomText");
        }

     

        // 供Animator使用
        public void AnimSetHeartPos()
        {
            var uiPos = WorldPositionToUGUI(_overworldControl.playerDeadPos);
            transform.Find("Heart").GetComponent<RectTransform>().anchoredPosition = uiPos;
        }

        /// <summary>
        ///     将世界坐标转换为UGUI坐标
        /// </summary>
        /// <param name="position">世界坐标</param>
        /// <returns>转换后的UGUI坐标</returns>
        private Vector2 WorldPositionToUGUI(Vector3 position)
        {
            var canvasRectTransform = GetComponent<RectTransform>();
            if (Camera.main == null)
            {
                Other.Debug.LogError("未找到主摄像机！");
                return position;
            }

            Vector2 screenPoint = Camera.main.WorldToScreenPoint(position); 
            var screenSize = new Vector2(Screen.width, Screen.height);
            screenPoint -= screenSize / 2; 
            var anchorPos = screenPoint / screenSize * canvasRectTransform.sizeDelta; 
            return anchorPos;
        }


        // 供Animator使用
        public void AnimSetHeartRed(int isRed)
        {
            transform.Find("Heart").GetComponent<Image>().color = Convert.ToBoolean(isRed) ? Color.red : ColorEx.WhiteClear;
        }

        // 供Animator使用
        public void AnimHeartGo()
        {
            var i = transform.Find("Heart").GetComponent<RectTransform>();
            var j = i.GetComponent<Image>();
            j.DOColor(new Color(j.color.r, j.color.g, j.color.b, 0), AnimSpeed).SetEase(Ease.Linear);
            DOTween.To(() => i.anchoredPosition, x => i.anchoredPosition = x, new Vector2(-330, -250), 1.5f)
                .SetEase(Ease.OutCirc).OnKill(() =>
                {
                    Animator.SetBool(Open, false);
                    GameUtilityService.FadeOutAndSwitchScene("Battle", Color.black, false, -0.5f);
                });
        }

        // 供Animator使用
        public void AnimPlayFX(int i)
        {
            AudioController.Instance.GetFx(i, MainControl.Instance.AudioControl.fxClipUI);
        }

        private enum KeyMapping //用于控制（按键设置）的主次键位。
        {
            MainKeyMap,
            SecondaryKeyMap1,
            SecondaryKeyMap2
        }

        public void SetSettingsLayer(SettingsLayer settingsLayer)
        {
            _settingsLayer = settingsLayer;
        }
    }
}