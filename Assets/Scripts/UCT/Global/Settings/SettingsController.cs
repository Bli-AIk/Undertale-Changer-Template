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
    public class SettingsController : MonoBehaviour
    {
        private const int SettingSelectedOptionMax = 7;
        private const float AnimSpeed = 0.25f;

        private static readonly List<ISettingsLayer> SettingsLayers = new()
        {
            new HomeSettingsLayer()
        };

        public static SettingsController Instance;
        private static readonly int Open = Animator.StringToHash("Open");

        private static OverworldControl _overworldControl;

        [FormerlySerializedAs("framePic")] public int frameSpriteIndex;

        [ReadOnly] [LabelText("Render Mode (ReadOnly)")]
        public RenderMode renderMode;

        private Canvas _canvas;

        [ShowInInspector] [ReadOnly] [LabelText("Is Pause Canvas (ReadOnly)")]
        private bool _isPauseCanvas; //防止切场景时整事儿

        private bool _isSettingKey;
        private bool _isSettingSelectionBased;
        private KeyMapping _keyMapping;
        private int _languagePackSelectedOptionMax; //目前 Max仅用于配置语言包
        private float _saveSelectionBasedValue;
        private int _settingKeyPage;
        private Image _settingPageBackground;
        private int _settingSelectedOption;
        private SettingsLayerEnum _settingsLayerEnum;
        private Image _settingSoul;
        private TextMeshProUGUI _settingTmp, _settingTmpSon, _settingTmpDescription;
        public Image Frame { get; private set; }
        public Animator Animator { get; private set; }

        private void Awake()
        {
            Instance = this;
            Animator = GetComponent<Animator>();
            _canvas = GetComponent<Canvas>();
            _settingPageBackground = transform.Find("Setting").GetComponent<Image>();
            _settingTmp = transform.Find("Setting/Setting Text").GetComponent<TextMeshProUGUI>();
            _settingTmpSon = _settingTmp.transform.Find("Setting Son").GetComponent<TextMeshProUGUI>();
            _settingSoul = _settingTmp.transform.Find("Soul").GetComponent<Image>();
            _settingTmpDescription = _settingTmp.transform.Find("Setting Under").GetComponent<TextMeshProUGUI>();
            Frame = transform.Find("Frame").GetComponent<Image>();
        }

        public void Start()
        {
            StartInitialization();
            UpdateLayerDisplay(SettingsLayers[0].SettingsOptions);
        }

        private void Update()
        {
            if (!IsUpdateInitializationPassed()) return;

            switch (_settingsLayerEnum)
            {
                case SettingsLayerEnum.HomeLayer:
                    //HomeLayer();
                    UpdateLayer(SettingsLayers[0], _settingSelectedOption);
                    break;
                case SettingsLayerEnum.KeyConfigLayer:
                    KeyConfigLayer();
                    break;
                case SettingsLayerEnum.LanguagePacksConfigLayer:
                    LanguagePacksConfigLayer();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void StartInitialization()
        {
            _settingsLayerEnum = SettingsLayerEnum.HomeLayer;
            _settingPageBackground.color = Color.clear;
            _settingTmp.color = ColorEx.WhiteClear;
            _settingTmpSon.color = ColorEx.WhiteClear;
            _settingTmpDescription.color = ColorEx.WhiteClear;
            _settingSoul.color = ColorEx.RedClear;
            _isPauseCanvas = false;
            _canvas.renderMode = renderMode;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _canvas.worldCamera = Camera.main;
            if (!_overworldControl)
                _overworldControl = MainControl.Instance.overworldControl;
        }

        private bool IsUpdateInitializationPassed()
        {
            if (IsInScene("Story")) return false;
            if (IsInBattleAndNotMyTurn()) return false;
            if (_isPauseCanvas) return false;
            if (IsSettingKey()) return false;
            if (MainControl.Instance.isSceneSwitching) return false;

            if (InputService.GetKeyDown(KeyCode.V) && !_overworldControl.isSetting)
            {
                TypeWritter.TypePause(true);
                OpenSetting();
            }

            if (!_overworldControl.isSetting) return false;
            _settingSoul.rectTransform.anchoredPosition = new Vector2(-225f, 147.5f + _settingSelectedOption * -37.5f);
            return _settingPageBackground.color.a > 0.7;
        }


        private void UpdateLayer(ISettingsLayer layer, int option)
        {
            if ((InputService.GetKeyDown(KeyCode.X) || InputService.GetKeyDown(KeyCode.V)) &&
                layer.SettingsOptions[option].Type != OptionType.SelectionBasedTrue)
                ExitSetting(); // TODO: 改成检测有没有上一层然后再退出

            var settingsOption = layer.SettingsOptions[option];
            switch (layer.SettingsOptions[option].Type)
            {
                case OptionType.SelectionToggle:
                    GetKeyDownToUpdateSelectionToggle(settingsOption);
                    break;
                case OptionType.SelectionBasedFalse:
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        settingsOption.Type = OptionType.SelectionBasedTrue;
                        _saveSelectionBasedValue = (float)settingsOption.GetValue();
                    }

                    break;
                case OptionType.SelectionBasedTrue:
                    GetKeyDownToUpdateSelectionBased(settingsOption);
                    break;
                case OptionType.EnterLayer:
                    //TODO: 把这块换成真的切换层
                    if (Input.GetKeyDown(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        _settingsLayerEnum = SettingsLayerEnum.KeyConfigLayer;
                        UpdateKeyConfigDisplay();
                        _settingSelectedOption = 0;
                        return;
                    }
                    break;
                case OptionType.EnterScene:
                    if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        if (SceneManager.GetActiveScene().name == "Rename") return;
                        var sceneName = (string)layer.SettingsOptions[option].GetValue();
                        if (SceneManager.GetActiveScene().name == sceneName) goto case OptionType.Back;
                        ReturnToScene(sceneName);
                    }

                    break;
                case OptionType.Back:
                    if (InputService.GetKeyDown(KeyCode.Z))
                        ExitSetting();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (layer.SettingsOptions[option].Type != OptionType.SelectionBasedTrue)
                _settingSelectedOption = GetKeyDownToUpdateSelectedOption
                    (_settingSelectedOption, SettingSelectedOptionMax);

            _settingTmpDescription.text = GetDescriptionTextWith(settingsOption);
            UpdateLayerDisplay(layer.SettingsOptions);
        }

        private static string GetDescriptionTextWith(SettingsOption settingsOption)
        {
            object value;
            switch (settingsOption.Type)
            {
                case OptionType.SelectionBasedFalse or OptionType.SelectionBasedTrue or OptionType.EnterLayer
                    or OptionType.EnterScene or OptionType.Back:
                    value = 0;
                    break;
                case OptionType.SelectionToggle:
                    goto default;
                default:
                    value = settingsOption.SelectionBasedChangedValueGetter?.Invoke();
                    value = value is int or bool ? Convert.ToInt32(value) : 0;
                    break;
            }
            var valueInt = (int)value;
            if (valueInt >= settingsOption.DescriptionDataName.Length)
                valueInt = settingsOption.DescriptionDataName.Length - 1;

            var result = TextProcessingService.GetFirstChildStringByPrefix(
                _overworldControl.settingSave, settingsOption.DescriptionDataName[valueInt]);
            return result;
        }

        private static void GetKeyDownToUpdateSelectionToggle(SettingsOption settingsOption)
        {
            if (!InputService.GetKeyDown(KeyCode.Z)) return;
            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
            settingsOption.OnSelected?.Invoke();
            //else if (InputService.GetKeyDown(KeyCode.C))
            //    SwitchSelectionToggleInHomeLayer();
        }

        private void GetKeyDownToUpdateSelectionBased(SettingsOption settingsOption)
        {
            if (InputService.GetKey(KeyCode.LeftArrow) || InputService.GetKeyDown(KeyCode.DownArrow))
            {
                if ((float)settingsOption.GetValue() > settingsOption.SelectionBasedChangedMin)
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    var newValue = (float)settingsOption.GetValue() -
                                   settingsOption.SelectionBasedChangedUnit;
                    settingsOption.SetValue(newValue);
                    if ((float)settingsOption.GetValue() < settingsOption.SelectionBasedChangedMin)
                        settingsOption.SetValue(settingsOption.SelectionBasedChangedMin);
                    settingsOption.SelectionBasedChangedValueSetter((float)settingsOption.GetValue());
                }
            }
            else if (InputService.GetKey(KeyCode.RightArrow) || InputService.GetKeyDown(KeyCode.UpArrow))
            {
                if ((float)settingsOption.GetValue() < settingsOption.SelectionBasedChangedMax)
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    var newValue = (float)settingsOption.GetValue() +
                                   settingsOption.SelectionBasedChangedUnit;
                    settingsOption.SetValue(newValue);
                    if ((float)settingsOption.GetValue() > settingsOption.SelectionBasedChangedMax)
                        settingsOption.SetValue(settingsOption.SelectionBasedChangedMax);
                    settingsOption.SelectionBasedChangedValueSetter((float)settingsOption.GetValue());
                }
            }

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                settingsOption.Type = OptionType.SelectionBasedFalse;
            }
            else if (InputService.GetKeyDown(KeyCode.X) || InputService.GetKeyDown(KeyCode.V))
            {
                settingsOption.SetValue(_saveSelectionBasedValue);
                AudioListener.volume = (float)settingsOption.GetValue();
                settingsOption.Type = OptionType.SelectionBasedFalse;
            }
        }

        private void UpdateLayerDisplay(List<SettingsOption> settingsOptions)
        {
            // TODO: 给语言页面在MENU进入时一个额外的处理
            
            var settingsStringList = new List<string>(1) { "Setting" };
            settingsStringList.AddRange(settingsOptions.Select(settingsOption => settingsOption.DataName));
            var settingsText = settingsStringList.Select(key =>
                TextProcessingService.GetFirstChildStringByPrefix(
                    _overworldControl.settingSave, key)).ToList();

            _settingTmp.text = "";
            for (var i = 0; i < settingsOptions.Count + 1; i++)
            {
                if (i > 0 && settingsOptions[i - 1].Type == OptionType.SelectionBasedTrue)
                    _settingTmp.text += "<color=yellow>";

                _settingTmp.text += settingsText[i] + "</color>\n";
            }

            _settingTmpSon.text = "\n";
            foreach (var settingsOption in settingsOptions)
            {
                if (settingsOption.Type == OptionType.SelectionBasedTrue)
                    _settingTmpSon.text += "<color=yellow>";
                switch (settingsOption.OptionDisplayMode)
                {
                    case OptionDisplayMode.Default:
                    {
                        if (settingsOption.SelectionBasedChangedValueGetter?.Invoke() is bool value)
                            _settingTmpSon.text += GetOpenOrCloseString(value);
                        else
                            _settingTmpSon.text += settingsOption.SelectionBasedChangedValueGetter;
                        break;
                    }
                    case OptionDisplayMode.Percentage:
                    {
                        var value = (float)settingsOption.SelectionBasedChangedValueGetter?.Invoke()!;
                        _settingTmpSon.text += (int)(value * 100) + "%";
                        break;
                    }
                    case OptionDisplayMode.Resolution:
                    {
                        var value = (Vector2)settingsOption.SelectionBasedChangedValueGetter?.Invoke()!;
                        _settingTmpSon.text += value.x + "×" + value.y;
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException();
                    }
                }

                _settingTmpSon.text += "</color>\n";
            }
        }

        private void ReturnToScene(string sceneName)
        {
            GameUtilityService.FadeOutAndSwitchScene(sceneName, Color.black, true, AnimSpeed);
            _overworldControl.isSetting = false;
            TypeWritter.TypePause(false);
            _isPauseCanvas = true;
        }

        private void KeyConfigLayer()
        {
            _settingSelectedOption = GetKeyDownToUpdateSelectedOption(_settingSelectedOption, 8);

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                switch (_settingSelectedOption)
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
                    case 8:
                    {
                        _settingsLayerEnum = 0;
                        _settingSelectedOption = 0;
                        UpdateLayerDisplay(SettingsLayers[0].SettingsOptions);
                        return;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                UpdateKeyConfigDisplay();
            }
            else if (InputService.GetKeyDown(KeyCode.X) || InputService.GetKeyDown(KeyCode.V))
            {
                _settingsLayerEnum = 0;
                _settingSelectedOption = 0;
                UpdateLayerDisplay(SettingsLayers[0].SettingsOptions);
                return;
            }
            else if (InputService.GetKeyDown(KeyCode.C))
            {
                AudioController.Instance.GetFx(3, MainControl.Instance.AudioControl.fxClipUI);
                if ((int)_keyMapping < 2)
                    _keyMapping++;
                else
                    _keyMapping = 0;

                UpdateKeyConfigDisplay();
            }

            _settingTmpDescription.text = TextProcessingService.GetFirstChildStringByPrefix(
                _overworldControl.settingSave, "ControlUnder" + (int)_keyMapping);
        }

        private void LanguagePacksConfigLayer()
        {
            _settingSelectedOption =
                GetKeyDownToUpdateSelectedOption(_settingSelectedOption, _languagePackSelectedOptionMax);

            if (InputService.GetKeyDown(KeyCode.Z))
            {
                if (_settingSelectedOption != _languagePackSelectedOptionMax)
                {
                    AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                    UpdateLanguagePacksConfigDisplay(false, true);
                    MainControl.Instance.languagePackId = _settingSelectedOption;
                }
                else
                {
                    ExitSetting(true);
                }
            }
            else if (InputService.GetKeyDown(KeyCode.X) || InputService.GetKeyDown(KeyCode.V))
            {
                ExitSetting(true);
            }
        }

        private static int GetKeyDownToUpdateSelectedOption(int selectedOption, int selectedOptionMax)
        {
            var result = selectedOption;
            if (InputService.GetKeyDown(KeyCode.DownArrow))
            {
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                result++;
                if (result > selectedOptionMax) result = 0;
            }
            else if (InputService.GetKeyDown(KeyCode.UpArrow))
            {
                AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                result--;
                if (result < 0) result = selectedOptionMax;
            }

            return result;
        }

        private bool IsSettingKey()
        {
            if (!_isSettingKey) return false;
            UpdateKeyConfigDisplay();
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
                            origin = _overworldControl.KeyCodes[0][_settingSelectedOption + j];
                            _overworldControl.KeyCodes[0][_settingSelectedOption + j] =
                                GetSettingControlKey();
                            goto default;
                        }
                        case KeyMapping.SecondaryKeyMap1:
                        {
                            origin = _overworldControl.KeyCodes[1][_settingSelectedOption + j];
                            _overworldControl.KeyCodes[1][_settingSelectedOption + j] =
                                GetSettingControlKey();
                            goto default;
                        }
                        case KeyMapping.SecondaryKeyMap2:
                        {
                            origin = _overworldControl.KeyCodes[2][_settingSelectedOption + j];
                            _overworldControl.KeyCodes[2][_settingSelectedOption + j] =
                                GetSettingControlKey();
                            goto default;
                        }
                        default:
                        {
                            var keyCodes = new List<KeyCode>
                            {
                                _overworldControl.KeyCodes[0][_settingSelectedOption + j],
                                _overworldControl.KeyCodes[1][_settingSelectedOption + j],
                                _overworldControl.KeyCodes[2][_settingSelectedOption + j]
                            };
                            for (var i = 0; i < _overworldControl.KeyCodes[0].Count; i++)
                            {
                                if (_overworldControl.KeyCodes[0][i] !=
                                    keyCodes[(int)_keyMapping] ||
                                    i == _settingSelectedOption + j) continue;
                                _overworldControl.KeyCodes[0][i] = origin;
                                break;
                            }

                            for (var i = 0; i < _overworldControl.KeyCodes[1].Count; i++)
                            {
                                if (_overworldControl.KeyCodes[1][i] !=
                                    keyCodes[(int)_keyMapping] || i == _settingSelectedOption + j) continue;
                                _overworldControl.KeyCodes[1][i] = origin;
                                break;
                            }

                            for (var i = 0; i < _overworldControl.KeyCodes[2].Count; i++)
                            {
                                if (_overworldControl.KeyCodes[2][i] !=
                                    keyCodes[(int)_keyMapping] || i == _settingSelectedOption + j) continue;
                                _overworldControl.KeyCodes[2][i] = origin;
                                break;
                            }

                            UpdateKeyConfigDisplay();
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
        ///     设置键位页配置
        /// </summary>
        private void UpdateKeyConfigDisplay(bool isSetting = false)
        {
            var strings = new List<string>();

            for (var i = 0; i < 6; i++)
                if (isSetting && i == _settingSelectedOption)
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
                        if (isSetting && i == _settingSelectedOption) _settingTmpSon.text += "<color=yellow>";

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
                        if (isSetting && i - 6 == _settingSelectedOption) _settingTmpSon.text += "<color=yellow>";

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
        private void UpdateLanguagePacksConfigDisplay(bool onlySetSon = false, bool isSetting = false)
        {
            var pathStringSaver = "";

            if (isSetting)
                MainControl.Instance.Initialization(_settingSelectedOption);

            if (!onlySetSon)
                _settingTmp.text =
                    TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                        "LanguagePack") + '\n';
            _settingTmpSon.text = "";
            _languagePackSelectedOptionMax = 0;
            var settingSelectBack = _settingSelectedOption;
            if (onlySetSon)
                _settingSelectedOption = MainControl.Instance.languagePackId;

            for (var i = 0; i < MainControl.LanguagePackInsideNumber; i++) //内置包信息
            {
                var pathString = "TextAssets/LanguagePacks/" + DataHandlerService.GetLanguageInsideId(i);

                if (_languagePackSelectedOptionMax == _settingSelectedOption)
                    pathStringSaver = pathString;
                _languagePackSelectedOptionMax++;

                if (!onlySetSon)
                    _settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", false) + '\n';
            }

            foreach (var pathString in Directory.GetDirectories(Application.dataPath + "\\LanguagePacks"))
            {
                if (_languagePackSelectedOptionMax == _settingSelectedOption)
                    pathStringSaver = pathString;
                _languagePackSelectedOptionMax++;
                if (!onlySetSon)
                    _settingTmp.text += GetLanguagePacksName(pathString, "LanguagePackName", true) + '\n';
            }

            if (!onlySetSon)
                _settingTmp.text +=
                    TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                        "Back");

            _settingTmpDescription.text =
                GetLanguagePacksName(pathStringSaver, "LanguagePackInformation",
                    _settingSelectedOption >= MainControl.LanguagePackInsideNumber) + '\n' + GetLanguagePacksName(
                    pathStringSaver, "LanguagePackAuthor",
                    _settingSelectedOption >= MainControl.LanguagePackInsideNumber);

            _settingSelectedOption = settingSelectBack;
        }

        /// <summary>
        ///     获取语言包信息
        /// </summary>
        /// <param name="pathString">语言包路径</param>
        /// <param name="returnString">返回的字符串标识</param>
        /// <param name="isOutSide">是否为外部路径</param>
        private static string GetLanguagePacksName(string pathString, string returnString, bool isOutSide)
        {
            if (string.IsNullOrEmpty(pathString)) return "";
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
            _settingTmpDescription.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(Color.red, AnimSpeed).SetEase(Ease.Linear);
            _settingSelectedOption = 0;
            _settingTmpDescription.text =
                TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                    "ControlCommonDescriptionText");
            if (_settingsLayerEnum != SettingsLayerEnum.LanguagePacksConfigLayer)
                UpdateLayerDisplay(SettingsLayers[0].SettingsOptions);
            else
                UpdateLanguagePacksConfigDisplay(true);
        }

        /// <summary>
        ///     退出设置页面
        /// </summary>
        private void ExitSetting(bool isLan = false)
        {
            _settingsLayerEnum = 0;
            _settingPageBackground.DOColor(Color.clear, AnimSpeed);
            _settingTmp.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear).OnKill(() =>
            {
                _overworldControl.isSetting = false;
                TypeWritter.TypePause(false);

                if (isLan)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
            _settingTmpSon.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpDescription.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(ColorEx.RedClear, AnimSpeed).SetEase(Ease.Linear);
            _settingTmpDescription.text =
                TextProcessingService.GetFirstChildStringByPrefix(_overworldControl.settingSave,
                    "ControlCommonDescriptionText");
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
            transform.Find("Heart").GetComponent<Image>().color =
                Convert.ToBoolean(isRed) ? Color.red : ColorEx.WhiteClear;
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

        public void SetSettingsLayer(SettingsLayerEnum settingsLayerEnum)
        {
            _settingsLayerEnum = settingsLayerEnum;

            switch (_settingsLayerEnum)
            {
                case SettingsLayerEnum.HomeLayer:
                    UpdateLayerDisplay(SettingsLayers[0].SettingsOptions);
                    break;
                case SettingsLayerEnum.KeyConfigLayer:
                    UpdateKeyConfigDisplay();
                    break;
                case SettingsLayerEnum.LanguagePacksConfigLayer:
                    UpdateLanguagePacksConfigDisplay();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private enum KeyMapping //用于控制（按键设置）的主次键位。
        {
            MainKeyMap,
            SecondaryKeyMap1,
            SecondaryKeyMap2
        }
    }
}