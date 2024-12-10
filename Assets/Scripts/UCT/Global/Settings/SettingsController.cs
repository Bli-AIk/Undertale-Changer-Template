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
    [RequireComponent(typeof(SettingsAnimEventHelper))]
    public class SettingsController : MonoBehaviour
    {
        internal const float AnimSpeed = 0.25f;
        private const int SettingOptionsCount = 7;
        public static SettingsController Instance;
        internal static readonly int Open = Animator.StringToHash("Open");

        [ShowInInspector] [ReadOnly] private static int _settingOptionsPage;

        [ShowInInspector] [ReadOnly] private static int _settingOptionsPageMax;

        // 主要控件
        public OverworldControl overworldControl;

        // 配置相关字段
        [FormerlySerializedAs("framePic")] public int frameSpriteIndex;

        [HideInInspector] public string settingsLayer = "HomeSettingsLayer";

        // 渲染模式
        [ReadOnly] [LabelText("Render Mode (Editor ReadOnly)")]
        public RenderMode renderMode;

        private readonly List<string> _languagePackageKeyTexts = new();

        private readonly List<string> _settingsPreviousLayers = new();
        private Canvas _canvas;
        private int _endIndexCurrent;
        private bool _isPageUp;

        [ShowInInspector] [ReadOnly] [LabelText("Is Pause (Editor ReadOnly)")]
        private bool _isPauseCanvas;

        // 设置页的状态与操作
        private bool _isSettingKey;
        private bool _isSettingSelectionBased;
        private int _languagePackIdStorage;
        private int _languagePackSelectedOptionMax; // 当前用于配置语言包的最大值

        // UI 元素
        private TextMeshProUGUI _optionKeyText, _optionValueText, _optionTipText;
        private float _saveSelectionBasedValue;
        private SettingsOption _selectedSettingsOption;

        private Image _settingPageBackground;

        [ShowInInspector] [ReadOnly] [LabelText("Option (Editor ReadOnly)")]
        private int _settingSelectedOption;

        private int _settingSelectedOptionCheck;
        private List<int> _settingSelectedOptionMax;
        private ISettingsLayer _settingsLayer;
        private Image _settingSoul;
        private Tween _settingSoulTween;
        private int _startIndexCurrent;

        // 属性
        public Image Frame { get; private set; }
        public Animator Animator { get; private set; }


        private void Awake()
        {
            Instance = this;
            Animator = GetComponent<Animator>();
            _canvas = GetComponent<Canvas>();
            _settingPageBackground = transform.Find("Setting").GetComponent<Image>();
            _optionKeyText = transform.Find("Setting/OptionKeyText").GetComponent<TextMeshProUGUI>();
            _optionValueText = _optionKeyText.transform.Find("OptionValueText").GetComponent<TextMeshProUGUI>();
            _settingSoul = _optionKeyText.transform.Find("Soul").GetComponent<Image>();
            _optionTipText = _optionKeyText.transform.Find("OptionTipText").GetComponent<TextMeshProUGUI>();
            Frame = transform.Find("Frame").GetComponent<Image>();
        }

        public void Start()
        {
            SetSettingsLayer();
            StartInitialization();
            UpdateLayerDisplay(_settingsLayer.DisplayedSettingsOptions);
        }

        private void Update()
        {
            SetSettingsLayer();
            if (!IsUpdateInitializationPassed()) return;
            UpdateLayer(_settingsLayer, _settingSelectedOption);
        }

        private void SetSettingsLayer()
        {
            _settingsLayer = SettingsStorage.CubismSettingsLayers[settingsLayer];

            if (_settingsLayer is SettingLanguagePackageLayer)
            {
                _settingsLayer.Clear();
                for (var i = 0;
                     i < MainControl.LanguagePackageInternalNumber + MainControl.LanguagePackageExternalNumber;
                     i++)
                    _settingsLayer.AddLanguagePackageOption();
            }

            var allSettingsOptionsCopy = _settingsLayer.AllSettingsOptions.ToList();

            if (allSettingsOptionsCopy.Count > SettingOptionsCount)
            {
                const int settingOptionsCountModified = SettingOptionsCount - 1;

                _settingSelectedOptionMax = new List<int>();

                _settingOptionsPageMax =
                    Mathf.CeilToInt((float)allSettingsOptionsCopy.Count / settingOptionsCountModified) - 1;

                for (var page = 0; page <= _settingOptionsPageMax; page++)
                {
                    var startIndex = page * settingOptionsCountModified;
                    var endIndex = Mathf.Min(startIndex + settingOptionsCountModified, allSettingsOptionsCopy.Count);
                    _settingSelectedOptionMax.Add(endIndex - startIndex + 1);
                }

                _startIndexCurrent = _settingOptionsPage * settingOptionsCountModified;
                _endIndexCurrent = Mathf.Min(_startIndexCurrent + settingOptionsCountModified,
                    allSettingsOptionsCopy.Count);

                if (_startIndexCurrent >= allSettingsOptionsCopy.Count)
                    throw new ArgumentOutOfRangeException(nameof(_settingOptionsPage),
                        "Setting options page is out of range.");

                _settingsLayer.DisplayedSettingsOptions =
                    allSettingsOptionsCopy.GetRange(_startIndexCurrent, _endIndexCurrent - _startIndexCurrent);

                _settingsLayer.AddSwitchPageOptionForDisplay();
            }

            else
            {
                _settingsLayer.DisplayedSettingsOptions = allSettingsOptionsCopy;
            }

            _settingsLayer.AddBackOptionForDisplay(null);
        }


        private void StartInitialization()
        {
            _settingPageBackground.color = Color.clear;
            _optionKeyText.color = ColorEx.WhiteClear;
            _optionValueText.color = ColorEx.WhiteClear;
            _optionTipText.color = ColorEx.WhiteClear;
            _settingSoul.color = ColorEx.RedClear;
            _isPauseCanvas = false;
            _canvas.renderMode = renderMode;
            if (_canvas.renderMode == RenderMode.ScreenSpaceCamera)
                _canvas.worldCamera = Camera.main;
            if (!overworldControl)
                overworldControl = MainControl.Instance.overworldControl;
            _settingSoul.rectTransform.anchoredPosition = new Vector2(-225f, 147.5f);
            _settingOptionsPage = 0;
            _languagePackIdStorage = MainControl.Instance.languagePackId;
        }

        private bool IsUpdateInitializationPassed()
        {
            if (IsInScene("Story")) return false;
            if (IsInBattleAndNotMyTurn()) return false;
            if (_isPauseCanvas) return false;
            if (IsSettingKey()) return false;
            if (MainControl.Instance.isSceneSwitching) return false;

            if (InputService.GetKeyDown(KeyCode.V) && !overworldControl.isSetting)
            {
                TypeWritter.TypePause(true);
                OpenSetting();
            }

            if (!overworldControl.isSetting) return false;
            if (_settingSelectedOptionCheck == _settingSelectedOption) return _settingPageBackground.color.a > 0.7;
            _settingSelectedOptionCheck = _settingSelectedOption;
            MoveSettingSoul();
            return _settingPageBackground.color.a > 0.7;
        }

        private void MoveSettingSoul()
        {
            MoveSettingSoul(new Vector2(-225f + 269 * Convert.ToInt32(_isPageUp),
                147.5f + _settingSelectedOption * -37.5f));
        }

        private void MoveSettingSoul(Vector2 newAnchoredPosition)
        {
            _settingSoulTween.Kill();
            _settingSoulTween = DOTween.To(() => _settingSoul.rectTransform.anchoredPosition,
                x => _settingSoul.rectTransform.anchoredPosition = x,
                newAnchoredPosition, 0.25f).SetEase(Ease.OutCubic);
        }


        private void UpdateLayer(ISettingsLayer layer, int option)
        {
            var settingsOption = layer.DisplayedSettingsOptions[option];
            if ((InputService.GetKeyDown(KeyCode.X) || InputService.GetKeyDown(KeyCode.V)) &&
                layer.DisplayedSettingsOptions[option].Type != OptionType.SelectionBasedTrue)
                ReturnToPreviousLayer(layer);

            if (layer is SettingsLayerBase)
                if (UpdateLayerBase(layer, option))
                    return;

            if (layer.DisplayedSettingsOptions[option].Type != OptionType.SelectionBasedTrue)
                _settingSelectedOption = GetKeyDownToUpdateSelectedOption
                    (_settingSelectedOption, layer.DisplayedSettingsOptions.Count - 1);

            _optionTipText.text = GetOptionTipTextWith(settingsOption);
            UpdateLayerDisplay(layer.DisplayedSettingsOptions);
        }

        private bool UpdateLayerBase(ISettingsLayer layer, int option)
        {
            var settingsOption = layer.DisplayedSettingsOptions[option];
            switch (settingsOption.Type)
            {
                case OptionType.SelectionToggle:
                    GetKeyDownToUpdateSelectionToggle(settingsOption);
                    break;
                case OptionType.SelectionBasedFalse:
                    GetKeyDownToUpdateSelectionBasedFalse(settingsOption);
                    break;
                case OptionType.SelectionBasedTrue:
                    GetKeyDownToUpdateSelectionBasedTrue(settingsOption);
                    break;
                case OptionType.EnterLayer:
                    if (GetKeyDownToEnterLayer(layer, option)) return true;
                    break;
                case OptionType.EnterScene:
                    if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        if (SceneManager.GetActiveScene().name == "Rename") return true;
                        var sceneName = (string)layer.DisplayedSettingsOptions[option].GetValue();
                        if (SceneManager.GetActiveScene().name == sceneName) goto case OptionType.Back;
                        ReturnToScene(sceneName);
                    }

                    break;
                case OptionType.Back:
                    if (InputService.GetKeyDown(KeyCode.Z)) ReturnToPreviousLayer(layer);
                    break;
                case OptionType.ConfigurableKeyFalse:
                    if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        settingsOption.Type = OptionType.ConfigurableKeyTrue;
                        _isSettingKey = true;
                        _selectedSettingsOption = settingsOption;
                    }

                    if (InputService.GetKeyDown(KeyCode.C))
                        SettingsStorage.KeyBindingType = EnumService.IncrementEnum(SettingsStorage.KeyBindingType);
                    break;
                case OptionType.ConfigurableKeyTrue:
                    //  无事发生
                    break;
                case OptionType.KeyBindingsReset:
                    if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        KeyBindings.ResetDictionary();
                    }

                    break;
                case OptionType.SwitchPage:
                    if (InputService.GetKeyDown(KeyCode.LeftArrow) || InputService.GetKeyDown(KeyCode.RightArrow))
                    {
                        AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                        _isPageUp = !_isPageUp;
                        MoveSettingSoul();
                    }

                    if (InputService.GetKeyDown(KeyCode.UpArrow) || InputService.GetKeyDown(KeyCode.DownArrow))
                        _isPageUp = false;
                    if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        if (!_isPageUp) //  PageDown
                        {
                            if (_settingOptionsPage == 0)
                                break;
                            _settingOptionsPage--;
                        }
                        else
                        {
                            if (_settingOptionsPage == _settingOptionsPageMax)
                                break;
                            _settingOptionsPage++;
                        }

                        _isPageUp = !_isPageUp;
                        MoveSettingSoul();
                        _settingSelectedOption = _settingSelectedOptionMax[_settingOptionsPage] - 1;
                    }

                    break;
                case OptionType.LanguagePackage:
                    if (InputService.GetKeyDown(KeyCode.Z))
                    {
                        AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
                        MainControl.Instance.languagePackId = _settingSelectedOption;
                        MainControl.Instance.Initialization(_settingSelectedOption + _startIndexCurrent);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return false;
        }

        private bool GetKeyDownToEnterLayer(ISettingsLayer layer, int option)
        {
            if (!InputService.GetKeyDown(KeyCode.Z)) return false;
            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
            _settingsPreviousLayers.Add(settingsLayer);
            settingsLayer = (string)layer.DisplayedSettingsOptions[option].GetValue();
            var settingSelectedOption = layer.DisplayedSettingsOptions[option].NewSelectedOption;
            if (settingSelectedOption < 0)
                settingSelectedOption = layer.DisplayedSettingsOptions.Count - 1 + settingSelectedOption;

            _settingSelectedOption = settingSelectedOption;
            return true;
        }

        private void GetKeyDownToUpdateSelectionBasedFalse(SettingsOption settingsOption)
        {
            if (!InputService.GetKeyDown(KeyCode.Z)) return;
            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
            settingsOption.Type = OptionType.SelectionBasedTrue;
            _saveSelectionBasedValue = Convert.ToSingle(settingsOption.GetValue());
        }

        private void ReturnToPreviousLayer(ISettingsLayer layer)
        {
            _isPageUp = false;
            _settingOptionsPage = 0;

            if (layer is SettingLanguagePackageLayer && _languagePackIdStorage != MainControl.Instance.languagePackId)
            {
                _languagePackIdStorage = MainControl.Instance.languagePackId;
                ReturnToPreviousLayer(null, false);
                GameUtilityService.RefreshTheScene();
            }

            ReturnToPreviousLayer(_settingsPreviousLayers.Count > 0 ? _settingsPreviousLayers[^1] : null, false);
            if (_settingsPreviousLayers.Count > 0)
                _settingsPreviousLayers.RemoveAt(_settingsPreviousLayers.Count - 1);
        }

        private static string GetOptionTipTextWith(SettingsOption settingsOption)
        {
            if (settingsOption.Type is OptionType.ConfigurableKeyFalse or OptionType.ConfigurableKeyTrue)
                return TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.overworldControl.settingSave,
                    "OptionTip" + (int)SettingsStorage.KeyBindingType);
            if (settingsOption.DescriptionDataName == null)
                return "";

            object value;
            switch (settingsOption.Type)
            {
                case OptionType.SelectionBasedFalse or OptionType.SelectionBasedTrue or OptionType.EnterLayer
                    or OptionType.EnterScene or OptionType.Back or OptionType.SwitchPage:
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
            var dataName = settingsOption.DescriptionDataName[valueInt];
            var result = TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.overworldControl.settingSave, dataName);
            return result;
        }

        private static void GetKeyDownToUpdateSelectionToggle(SettingsOption settingsOption)
        {
            if (!InputService.GetKeyDown(KeyCode.Z)) return;
            AudioController.Instance.GetFx(1, MainControl.Instance.AudioControl.fxClipUI);
            settingsOption.OnSelected?.Invoke();
        }

        private void GetKeyDownToUpdateSelectionBasedTrue(SettingsOption settingsOption)
        {
            var unit = InputService.GetKey(KeyCode.C)
                ? settingsOption.SelectionBasedChangedUnitWhenGetC
                : settingsOption.SelectionBasedChangedUnit;
            if (InputService.GetKey(KeyCode.LeftArrow) || InputService.GetKeyDown(KeyCode.DownArrow))
            {
                if (Convert.ToSingle(settingsOption.GetValue()) > settingsOption.SelectionBasedChangedMin)
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    var newValue = (float)settingsOption.GetValue() - unit;
                    settingsOption.SetValue(newValue);
                    if (Convert.ToSingle(settingsOption.GetValue()) < settingsOption.SelectionBasedChangedMin)
                        settingsOption.SetValue(settingsOption.SelectionBasedChangedMin);
                    settingsOption.SelectionBasedChangedValueSetter(Convert.ToSingle(settingsOption.GetValue()));
                }
            }
            else if (InputService.GetKey(KeyCode.RightArrow) || InputService.GetKeyDown(KeyCode.UpArrow))
            {
                if (Convert.ToSingle(settingsOption.GetValue()) < settingsOption.SelectionBasedChangedMax)
                {
                    AudioController.Instance.GetFx(0, MainControl.Instance.AudioControl.fxClipUI);
                    var newValue = Convert.ToSingle(settingsOption.GetValue()) + unit;
                    settingsOption.SetValue(newValue);
                    if (Convert.ToSingle(settingsOption.GetValue()) > settingsOption.SelectionBasedChangedMax)
                        settingsOption.SetValue(settingsOption.SelectionBasedChangedMax);
                    settingsOption.SelectionBasedChangedValueSetter(Convert.ToSingle(settingsOption.GetValue()));
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
                settingsOption.SelectionBasedChangedValueSetter(_saveSelectionBasedValue);
                settingsOption.Type = OptionType.SelectionBasedFalse;
            }
        }

        private void UpdateLayerDisplay(List<SettingsOption> settingsOptions)
        {
            _optionKeyText.text = null;
            _optionValueText.text = null;
            var settingsOptionsForSetting = new List<SettingsOption>();

            if (settingsLayer == "SettingLanguagePackageLayer")
                UpdateLanguagePacksConfigDisplay();
            else
                settingsOptionsForSetting.Add(null);

            settingsOptionsForSetting.AddRange(settingsOptions);
            _optionKeyText.text += BuildSettingText(settingsOptionsForSetting, GetOptionKeyText);
            _optionValueText.text += BuildSettingText(settingsOptionsForSetting, GetOptionValueText);
        } // ReSharper disable Unity.PerformanceAnalysis
        private static string BuildSettingText(List<SettingsOption> settingsOptions,
            Func<SettingsOption, string> textExtractor)
        {
            var result = new StringBuilder();
            foreach (var option in settingsOptions)
            {
                var extractor = textExtractor(option);

                switch (option?.Type)
                {
                    case OptionType.LanguagePackage:
                        continue;
                    case OptionType.SelectionBasedTrue or OptionType.ConfigurableKeyTrue:
                        result.Append("<color=yellow>");
                        break;
                }

                result.Append(extractor);
                result.Append("</color>\n");
            }

            return result.ToString();
        }


        private static string GetOptionKeyText(SettingsOption option)
        {
            if (option is { Type: OptionType.LanguagePackage }) return null;

            var dataName = option != null ? option.DataName : "Setting";
            var result = "";
            if (dataName == "PageUp" && _settingOptionsPage == 0)
                result += "<color=#808080>";
            result += TextProcessingService.GetFirstChildStringByPrefix(
                MainControl.Instance.overworldControl.settingSave, dataName);
            return result + "</color>";
        }

        private static string GetOptionValueText(SettingsOption option)
        {
            if (option == null) return null;
            switch (option.OptionDisplayMode)
            {
                case OptionDisplayMode.Default:
                    return GetDefaultDisplayValue(option);
                case OptionDisplayMode.Percentage:
                    var percentage = (int)((float)option.SelectionBasedChangedValueGetter?.Invoke()! * 100);
                    return percentage + "%";
                case OptionDisplayMode.Resolution:
                    var resolution = (Vector2)option.SelectionBasedChangedValueGetter?.Invoke()!;
                    return resolution.x + "×" + resolution.y;
                case OptionDisplayMode.DataName:
                    var result = "";
                    if (_settingOptionsPage == _settingOptionsPageMax)
                        result += "<color=#808080>";
                    return result + TextProcessingService.GetFirstChildStringByPrefix(
                        MainControl.Instance.overworldControl.settingSave,
                        (option.SelectionBasedChangedValueGetter?.Invoke()!).ToString()) + "</color>";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private static string GetDefaultDisplayValue(SettingsOption option)
        {
            var gotValue = option.SelectionBasedChangedValueGetter?.Invoke();

            if (gotValue is int intValue && option.GetSpDataWithIndex != null &&
                option.GetSpDataWithIndex.TryGetValue(intValue, out var value1))
                gotValue = TextProcessingService.GetFirstChildStringByPrefix(
                    MainControl.Instance.overworldControl.settingSave, value1);

            return gotValue switch
            {
                bool value => GetOpenOrCloseString(value),
                _ => gotValue?.ToString() ?? string.Empty
            };
        }


        private void ReturnToScene(string sceneName)
        {
            GameUtilityService.FadeOutAndSwitchScene(sceneName, Color.black, true, AnimSpeed);
            overworldControl.isSetting = false;
            TypeWritter.TypePause(false);
            _isPauseCanvas = true;
            _isPageUp = false;
            _settingOptionsPage = 0;
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
            var settingsOptions = _settingsLayer.DisplayedSettingsOptions;
            UpdateLayerDisplay(settingsOptions);

            var pressedKeycode = GetSettingKeyControl();
            if (pressedKeycode == KeyCode.None) return true;

            var dataName = _selectedSettingsOption.GetValue().ToString();
            var oldKeyCodeStorage = KeyBindings.GetKeyCode(SettingsStorage.KeyBindingType, dataName);

            for (var i = 0; i < KeyBindings.GetKeyCodes(KeyBindingType.Primary).Count; i++)
                foreach (KeyBindingType keyBindingType in Enum.GetValues(typeof(KeyBindingType)))
                    if (KeyBindings.GetKeyCodeAtIndex(keyBindingType, i) == pressedKeycode)
                        KeyBindings.SetKeyCodeAtIndex(keyBindingType, i, oldKeyCodeStorage);

            KeyBindings.SetKeyCode(SettingsStorage.KeyBindingType, dataName, pressedKeycode);

            UpdateLayerDisplay(settingsOptions);
            _isSettingKey = false;
            foreach (var settingsOption in settingsOptions.Where(settingsOption =>
                         settingsOption.Type == OptionType.ConfigurableKeyTrue))
                settingsOption.Type = OptionType.ConfigurableKeyFalse;
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
        ///     设置语言包页配置
        /// </summary>
        private void UpdateLanguagePacksConfigDisplay()
        {
            var pathStringSaver = "";

            _optionKeyText.text =
                TextProcessingService.GetFirstChildStringByPrefix(overworldControl.settingSave, "LanguagePack")
                + '\n';

            _optionValueText.text = "\n";
            _languagePackSelectedOptionMax = 0;


            //TODO: 换页后得用换页的index
            _languagePackageKeyTexts.Clear();
            pathStringSaver = TraversingInternalLanguagePackagesFrom(pathStringSaver);
            pathStringSaver = TraversingExternalLanguagePackagesFrom(pathStringSaver);

            var fixedEndIndexCurrent = _endIndexCurrent;
            if (_languagePackageKeyTexts.Count <= 7)
                fixedEndIndexCurrent++;
            for (var i = 0; i < _languagePackageKeyTexts.Count; i++)
            {
                if (_endIndexCurrent != 0 && (i < _startIndexCurrent || i >= fixedEndIndexCurrent)) continue;
                var keyText = _languagePackageKeyTexts[i];
                _optionKeyText.text += keyText;
                _optionValueText.text += "\n";
            }

            MainControl.LanguagePackageExternalNumber =
                _languagePackSelectedOptionMax - MainControl.LanguagePackageInternalNumber;

            var fixedOption = _settingSelectedOption + _startIndexCurrent;
            _optionTipText.text =
                _endIndexCurrent != 0 && (fixedOption < _startIndexCurrent || fixedOption >= _endIndexCurrent)
                    ? TextProcessingService.GetFirstChildStringByPrefix(overworldControl.settingSave, "LanguageBack")
                    : GetLanguagePacksName(pathStringSaver, "LanguagePackInformation",
                          fixedOption >= MainControl.LanguagePackageInternalNumber) + '\n' +
                      GetLanguagePacksName(pathStringSaver, "LanguagePackAuthor",
                          fixedOption >= MainControl.LanguagePackageInternalNumber);
        }

        private static int GetLanguagePackagesOptionFrom(int i, bool isExternal)
        {
            if (!isExternal) return i;
            return MainControl.LanguagePackageInternalNumber + i;
        }

        private string TraverseLanguagePackages(string pathStringSaver, bool isExternal)
        {
            var basePath = isExternal ? Application.dataPath + "\\LanguagePacks" : "TextAssets/LanguagePacks/";
            var languagePackCount = isExternal
                ? Directory.GetDirectories(basePath).Length
                : MainControl.LanguagePackageInternalNumber;

            for (var i = 0; i < languagePackCount; i++)
            {
                var pathString = isExternal
                    ? Directory.GetDirectories(basePath)[i]
                    : basePath + DataHandlerService.GetLanguageInsideId(i);

                if (_languagePackSelectedOptionMax == _settingSelectedOption + _startIndexCurrent)
                    pathStringSaver = pathString;

                _languagePackSelectedOptionMax++;

                var newKeyText = "";
                if (MainControl.Instance.languagePackId == GetLanguagePackagesOptionFrom(i, isExternal))
                    newKeyText += "<color=yellow>";

                newKeyText += GetLanguagePacksName(pathString, "LanguagePackName", isExternal) + "</color>\n";

                _languagePackageKeyTexts.Add(newKeyText);
            }

            return pathStringSaver;
        }

        private string TraversingExternalLanguagePackagesFrom(string pathStringSaver)
        {
            return TraverseLanguagePackages(pathStringSaver, true);
        }

        private string TraversingInternalLanguagePackagesFrom(string pathStringSaver)
        {
            return TraverseLanguagePackages(pathStringSaver, false);
        }


        /// <summary>
        ///     获取语言包信息
        /// </summary>
        /// <param name="pathString">语言包路径</param>
        /// <param name="returnString">返回的字符串标识</param>
        /// <param name="isExternal">是否为外部路径</param>
        private static string GetLanguagePacksName(string pathString, string returnString, bool isExternal)
        {
            if (string.IsNullOrEmpty(pathString)) return "";
            var strings =
                DataHandlerService.LoadItemData(ReadFile(pathString + "\\LanguagePackInformation", isExternal));
            strings = DataHandlerService.ChangeItemData(strings, true, new List<string>());
            return TextProcessingService.GetFirstChildStringByPrefix(strings, returnString);
        }

        private static string ReadFile(string pathName, bool isExternal)
        {
            return !isExternal ? Resources.Load<TextAsset>(pathName).text : File.ReadAllText(pathName + ".txt");
        }

        /// <summary>
        ///     返回开/关文本
        /// </summary>
        private static string GetOpenOrCloseString(bool inputBool)
        {
            return TextProcessingService.GetFirstChildStringByPrefix
                (MainControl.Instance.overworldControl.settingSave, inputBool ? "Open" : "Close");
        }

        /// <summary>
        ///     检测并返回按下的设置控制键
        /// </summary>
        /// <returns>按下的键</returns>
        private static KeyCode GetSettingKeyControl()
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
        public void OpenSetting(string inputSettingsLayer = null)
        {
            if (inputSettingsLayer != null)
            {
                settingsLayer = inputSettingsLayer;
                SetSettingsLayer();
            }

            overworldControl.isSetting = true;
            _settingPageBackground.DOColor(new Color(0, 0, 0, 0.75f), AnimSpeed);
            _optionKeyText.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _optionValueText.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _optionTipText.DOColor(Color.white, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(Color.red, AnimSpeed).SetEase(Ease.Linear);
            _settingSelectedOption = 0;
            _optionTipText.text =
                TextProcessingService.GetFirstChildStringByPrefix(overworldControl.settingSave,
                    "ControlCommonDescriptionText");
            UpdateLayerDisplay(_settingsLayer.DisplayedSettingsOptions);
        }

        /// <summary>
        ///     退出设置页面
        /// </summary>
        private void ReturnToPreviousLayer(string layerString, bool isLan)
        {
            if (layerString != null)
            {
                settingsLayer = layerString;
                _settingSelectedOption = 0;
                return;
            }

            _settingPageBackground.DOColor(Color.clear, AnimSpeed);
            _optionKeyText.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear).OnKill(() =>
            {
                overworldControl.isSetting = false;
                TypeWritter.TypePause(false);
                settingsLayer = "HomeSettingsLayer";
                _settingSoul.rectTransform.anchoredPosition = new Vector2(-225f, 147.5f);
                if (isLan)
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
            _optionValueText.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear);
            _optionTipText.DOColor(ColorEx.WhiteClear, AnimSpeed).SetEase(Ease.Linear);
            _settingSoul.DOColor(ColorEx.RedClear, AnimSpeed).SetEase(Ease.Linear);
            _optionTipText.text =
                TextProcessingService.GetFirstChildStringByPrefix(overworldControl.settingSave,
                    "ControlCommonDescriptionText");
        }


        /// <summary>
        ///     将世界坐标转换为UGUI坐标
        /// </summary>
        /// <param name="position">世界坐标</param>
        /// <returns>转换后的UGUI坐标</returns>
        public Vector2 WorldPositionToUGUI(Vector3 position)
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
    }
}