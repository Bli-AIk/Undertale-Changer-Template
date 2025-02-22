using System;
using System.Collections.Generic;
using UCT.Global.Core;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.Settings
{
    /// <summary>
    ///     设置页面层级接口
    /// </summary>
    public interface ISettingsLayer
    {
        List<SettingsOption> AllSettingsOptions { get; }
        List<SettingsOption> DisplayedSettingsOptions { get; set; }

        /// <summary>
        ///     添加退出层级/设置页面的选项
        /// </summary>
        /// <param name="layerName">层级名称</param>
        /// <param name="dataName">选项本身的数据名称</param>
        /// <param name="descriptionDataName">描述文本的数据名称</param>
        void AddBackOptionForDisplay(string layerName, string dataName = "SettingBack",
            string descriptionDataName = "SettingBackTip");

        void AddSwitchPageOptionForDisplay(string dataName = "PageUp", string changedValue = "PageDown");

        /// <summary>
        ///     添加语言包配置的选项
        /// </summary>
        void AddLanguagePackageOption();

        /// <summary>
        ///     清除所有设置项
        /// </summary>
        void Clear();
    }

    public abstract class SettingsLayerBase : ISettingsLayer
    {
        public List<SettingsOption> AllSettingsOptions { get; private set; } = new();
        public List<SettingsOption> DisplayedSettingsOptions { get; set; } = new();


        //  添加给DisplayedSettingsOptions

        /// <summary>
        ///     添加退出层级/设置页面的选项
        /// </summary>
        /// <param name="layerName">层级名称</param>
        /// <param name="dataName">选项本身的数据名称</param>
        /// <param name="descriptionDataName">描述文本的数据名称</param>
        public void AddBackOptionForDisplay(string layerName, string dataName = "SettingBack",
            string descriptionDataName = "SettingBackTip")
        {
            DisplayedSettingsOptions.Add(new SettingsOption(layerName)
            {
                DataName = dataName,
                DescriptionDataName = new[] { descriptionDataName },
                Type = OptionType.Back
            });
        }

        public void AddSwitchPageOptionForDisplay(string dataName = "PageUp", string changedValue = "PageDown")
        {
            DisplayedSettingsOptions.Add(new SettingsOption(false)
            {
                DataName = dataName,
                Type = OptionType.SwitchPage,
                SelectionBasedChangedValueGetter = () => changedValue,
                OptionDisplayMode = OptionDisplayMode.DataName
            });
        }

        /// <summary>
        ///     添加语言包配置的选项。
        ///     特别地，它是加给AllSettingsOptions的。
        /// </summary>
        public void AddLanguagePackageOption()
        {
            AllSettingsOptions.Add(new SettingsOption(null)
            {
                Type = OptionType.LanguagePackage
            });
        }

        /// <summary>
        ///     清除所有设置项
        /// </summary>
        public void Clear()
        {
            AllSettingsOptions = new List<SettingsOption>();
            DisplayedSettingsOptions = new List<SettingsOption>();
        }

        /// <summary>
        ///     添加进入层级的选项
        /// </summary>
        /// <param name="layerName">层级名称</param>
        /// <param name="dataName">选项本身的数据名称</param>
        /// <param name="descriptionDataName">描述文本的数据名称</param>
        /// <param name="newSelectedOption">进入层级后的选项索引</param>
        protected void AddEnterLayerOption(string layerName, string dataName, string[] descriptionDataName,
            int newSelectedOption = 0)
        {
            AllSettingsOptions.Add(new SettingsOption(layerName)
            {
                DataName = dataName,
                DescriptionDataName = descriptionDataName,
                NewSelectedOption = newSelectedOption,
                Type = OptionType.EnterLayer
            });
        }

        /// <summary>
        ///     添加进入场景的选项
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="dataName">选项本身的数据名称</param>
        /// <param name="descriptionDataName">描述文本的数据名称</param>
        protected void AddEnterSceneOption(string sceneName, string dataName, string descriptionDataName)
        {
            AllSettingsOptions.Add(new SettingsOption(sceneName)
            {
                DataName = dataName,
                DescriptionDataName = new[] { descriptionDataName },
                Type = OptionType.EnterScene
            });
        }

        /// <summary>
        ///     添加按键设置的选项
        /// </summary>
        /// <param name="dataName">数据名称</param>
        protected void AddConfigurableKeyOption(string dataName)
        {
            AllSettingsOptions.Add(new SettingsOption(dataName)
            {
                DataName = dataName,
                Type = OptionType.ConfigurableKeyFalse,
                SelectionBasedChangedValueGetter = () =>
                    KeyBindings.GetKeyCode(SettingsStorage.KeyBindingType, dataName),
                SelectionBasedChangedValueSetter = value =>
                    KeyBindings.SetKeyCode(SettingsStorage.KeyBindingType, dataName, (KeyCode)value)
            });
        }

        /// <summary>
        ///     添加重置初始按键设置的选项
        /// </summary>
        /// <param name="dataName">数据名称</param>
        protected void AddKeyBindingsResetOption(string dataName = "KeyBindingsReset")
        {
            AllSettingsOptions.Add(new SettingsOption(null)
            {
                DataName = dataName,
                DescriptionDataName = new[] { dataName },
                Type = OptionType.KeyBindingsReset
            });
        }
    }


    #region HomeSettingsLayer

    public class HomeSettingsLayer : SettingsLayerBase
    {
        public HomeSettingsLayer()
        {
            AddEnterLayerOption("VideoSettingsLayer", "VideoSettingsLayer", new[] { "VideoSettingsLayerTip" });
            AddEnterLayerOption("AudioSettingsLayer", "AudioSettingsLayer", new[] { "AudioSettingsLayerTip" });
            AddEnterLayerOption("InputSettingsLayer", "InputSettingsLayer", new[] { "InputSettingsLayerTip" });
            AddEnterLayerOption("SettingLanguagePackageLayer", "SettingLanguagePackageLayer",
                new[] { "SettingLanguagePackageLayerTip" });
            AddEnterLayerOption("SubtitleSettingsLayer", "SubtitleSettingsLayer", new[] { "SubtitleSettingsLayerTip" });
            AddEnterSceneOption("Menu", "SettingBackMenu", "SettingBackMenuTip");
        }
    }

    #endregion

    #region VideoSettingsLayer

    public class VideoSettingsLayer : SettingsLayerBase
    {
        public VideoSettingsLayer()
        {
            #region SettingFullScreen

            AllSettingsOptions.Add(new SettingsOption(false)
            {
                DataName = "SettingFullScreen",
                DescriptionDataName = new[] { "SettingFullScreenTipOpen", "SettingFullScreenTipClose" },
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => SettingsStorage.fullScreen,
                SelectionBasedChangedValueSetter = value => SettingsStorage.fullScreen = (bool)value,
                OnSelected = () =>
                {
                    SettingsStorage.fullScreen = !SettingsStorage.fullScreen;
                    GameUtilityService.SetResolution(SettingsStorage.resolutionLevel);
                }
            });

            #endregion

            #region SettingResolving

            AllSettingsOptions.Add(new SettingsOption(0)
            {
                DataName = "SettingResolving",
                DescriptionDataName = new[] { "SettingResolvingTip" },
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Resolution,
                SelectionBasedChangedValueGetter = () => SettingsStorage.resolution,
                SelectionBasedChangedValueSetter = value => SettingsStorage.resolution = (Vector2)value,
                OnSelected = () =>
                {
                    SettingsStorage.resolutionLevel = GameUtilityService.UpdateResolutionSettings
                        (SettingsStorage.isUsingHdFrame, SettingsStorage.resolutionLevel);
                }
            });

            #endregion

            #region GraphicSettingsLayer

            AddEnterLayerOption("GraphicSettingsLayer", "GraphicSettingsLayer", new[] { "GraphicSettingsLayerTip" });

            #endregion

            //TODO: 切换16:9分辨率

            //TODO: 启用像素完美滤镜

            //TODO: 启用低分辨率高显

            #region SettingFPS

            AllSettingsOptions.Add(new SettingsOption(false)
            {
                DataName = "SettingFPS",
                DescriptionDataName = new[] { "SettingFPSTip" },
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => SettingsStorage.isDisplayFPS,
                SelectionBasedChangedValueSetter = value => SettingsStorage.isDisplayFPS = (bool)value,
                OnSelected = () => { SettingsStorage.isDisplayFPS = !SettingsStorage.isDisplayFPS; }
            });

            #endregion

            #region LockFrameRate

            AllSettingsOptions.Add(new SettingsOption(SettingsStorage.frameRate)
            {
                DataName = "LockFrameRate",
                DescriptionDataName = new[] { "LockFrameRateTip" },
                Type = OptionType.SelectionBasedFalse,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => SettingsStorage.frameRate,
                SelectionBasedChangedValueSetter = value =>
                {
                    SettingsStorage.frameRate = Convert.ToInt32(value);
                    Application.targetFrameRate = Convert.ToInt32(value);
                },
                SelectionBasedChangedUnit = 10,
                SelectionBasedChangedUnitWhenGetC = 1,
                SelectionBasedChangedMax = 1000,
                SelectionBasedChangedMin = 0,
                GetSpDataNameWithIndex = new Dictionary<int, string>
                {
                    { 0, "UnlimitedFrameRate" }
                }
            });

            #endregion

            #region SettingVSync

            AllSettingsOptions.Add(new SettingsOption(0)
            {
                DataName = "SettingVSync",
                DescriptionDataName = new[] { "SettingVSyncTip" },
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => (int)SettingsStorage.vsyncMode,
                SelectionBasedChangedValueSetter = value => SettingsStorage.vsyncMode = (VSyncMode)value,
                OnSelected = () =>
                {
                    QualitySettings.vSyncCount = (int)SettingsStorage.vsyncMode;
                    if ((int)SettingsStorage.vsyncMode < 2)
                    {
                        SettingsStorage.vsyncMode++;
                    }
                    else
                    {
                        SettingsStorage.vsyncMode = VSyncMode.DonNotSync;
                    }

                    PlayerPrefs.SetInt("vsyncMode",
                        Convert.ToInt32(SettingsStorage.vsyncMode));
                },
                GetSpDataNameWithIndex = new Dictionary<int, string>
                {
                    { 0, "NoSync" },
                    { 1, "Sync" },
                    { 2, "HalfSync" }
                }
            });

            #endregion
        }
    }

    #endregion

    #region AudioSettingsLayer

    public class AudioSettingsLayer : SettingsLayerBase
    {
        public AudioSettingsLayer()
        {
            #region SettingMainVolume

            AllSettingsOptions.Add(new SettingsOption(SettingsStorage.mainVolume)
            {
                DataName = "SettingMainVolume",
                DescriptionDataName = new[] { "SettingMainVolumeTip" },
                Type = OptionType.SelectionBasedFalse,
                OptionDisplayMode = OptionDisplayMode.Percentage,
                SelectionBasedChangedValueGetter = () => AudioListener.volume,
                SelectionBasedChangedValueSetter = value =>
                {
                    AudioListener.volume = (float)value;
                    SettingsStorage.mainVolume = (float)value;
                },
                SelectionBasedChangedUnit = 0.01f,
                SelectionBasedChangedUnitWhenGetC = 0.1f,
                SelectionBasedChangedMax = 1,
                SelectionBasedChangedMin = 0
            });

            #endregion

            #region SettingBgmVolume

            AllSettingsOptions.Add(new SettingsOption(SettingsStorage.bgmVolume)
            {
                DataName = "SettingBgmVolume",
                DescriptionDataName = new[] { "SettingBgmVolumeTip" },
                Type = OptionType.SelectionBasedFalse,
                OptionDisplayMode = OptionDisplayMode.Percentage,
                SelectionBasedChangedValueGetter = () =>
                {
                    MainControl.Instance.AudioControl.globalAudioMixer.GetFloat("BgmVolume", out var value);
                    return MathUtilityService.DbToNormalizedValue(value);
                },
                SelectionBasedChangedValueSetter = value =>
                {
                    var bgmVolume = MathUtilityService.NormalizedValueToDb((float)value);
                    MainControl.Instance.AudioControl.globalAudioMixer.SetFloat("BgmVolume", bgmVolume);
                    SettingsStorage.bgmVolume = (float)value;
                },
                SelectionBasedChangedUnit = 0.01f,
                SelectionBasedChangedUnitWhenGetC = 0.1f,
                SelectionBasedChangedMax = 1,
                SelectionBasedChangedMin = 0
            });

            #endregion

            #region SettingFxVolume

            AllSettingsOptions.Add(new SettingsOption(SettingsStorage.fxVolume)
            {
                DataName = "SettingFxVolume",
                DescriptionDataName = new[] { "SettingFxVolumeTip" },
                Type = OptionType.SelectionBasedFalse,
                OptionDisplayMode = OptionDisplayMode.Percentage,
                SelectionBasedChangedValueGetter = () =>
                {
                    MainControl.Instance.AudioControl.globalAudioMixer.GetFloat("FxVolume", out var value);
                    return MathUtilityService.DbToNormalizedValue(value);
                },
                SelectionBasedChangedValueSetter = value =>
                {
                    var bgmVolume = MathUtilityService.NormalizedValueToDb((float)value);
                    MainControl.Instance.AudioControl.globalAudioMixer.SetFloat("FxVolume", bgmVolume);
                    SettingsStorage.fxVolume = (float)value;
                },
                SelectionBasedChangedUnit = 0.01f,
                SelectionBasedChangedUnitWhenGetC = 0.1f,
                SelectionBasedChangedMax = 1,
                SelectionBasedChangedMin = 0
            });

            #endregion

            //TODO: 语音音量

            //TODO: 音频设备管理
        }
    }

    #endregion

    #region GraphicSettingsLayer

    public class GraphicSettingsLayer : SettingsLayerBase
    {
        public GraphicSettingsLayer()
        {
            //TODO: 图形质量

            #region SettingSFX

            AllSettingsOptions.Add(new SettingsOption(false)
            {
                DataName = "SettingSFX",
                DescriptionDataName = new[] { "SettingSFXTip" },
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => SettingsStorage.isSimplifySfx,
                SelectionBasedChangedValueSetter = value => SettingsStorage.isSimplifySfx = (bool)value,
                OnSelected = () =>
                {
                    SettingsStorage.isSimplifySfx = !SettingsStorage.isSimplifySfx;
                    GameUtilityService.ToggleAllSfx(SettingsStorage.isSimplifySfx);
                    PlayerPrefs.SetInt("noSFX", Convert.ToInt32(SettingsStorage.isSimplifySfx));
                }
            });

            #endregion

            //TODO: 拆解 SettingSFX 为 光效开关 和 后处理开关
        }
    }

    #endregion

    #region InputSettingsLayer

    public class InputSettingsLayer : SettingsLayerBase
    {
        public InputSettingsLayer()
        {
            //TODO: 控制器管理

            AddEnterLayerOption("SettingKeyControlLayer", "SettingKeyControlLayer", new[]
                { "SettingKeyControlLayerTip" });
        }
    }

    #endregion

    #region SettingKeyControlLayer

    public class SettingKeyControlLayer : SettingsLayerBase
    {
        public SettingKeyControlLayer()
        {
            AddConfigurableKeyOption("MoveDown");
            AddConfigurableKeyOption("MoveRight");
            AddConfigurableKeyOption("MoveUp");
            AddConfigurableKeyOption("MoveLeft");
            AddConfigurableKeyOption("Confirm");
            AddConfigurableKeyOption("Cancel");
            AddConfigurableKeyOption("Backpack");
            AddConfigurableKeyOption("Settings");
            AddConfigurableKeyOption("FullScreen");
            AddConfigurableKeyOption("Resolution");
            AddConfigurableKeyOption("Sfx");
            AddKeyBindingsResetOption();
        }
    }

    #endregion

    #region SettingLanguagePackageLayer

    public class SettingLanguagePackageLayer : SettingsLayerBase
    {
        //  Implement in code

        //TODO: 强制使用Unicode字体

        //TODO: 强制使用全角
    }

    #endregion

    #region SubtitleSettingsLayer

    public class SubtitleSettingsLayer : SettingsLayerBase
    {
        public SubtitleSettingsLayer()
        {
            #region SettingTypingSpeed

            AllSettingsOptions.Add(new SettingsOption(SettingsStorage.typingSpeed)
            {
                DataName = "SettingTypingSpeed",
                DescriptionDataName = new[] { "SettingTypingSpeedTip" },
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => (int)SettingsStorage.typingSpeed,
                SelectionBasedChangedValueSetter = value => SettingsStorage.typingSpeed = (TypingSpeed)value,
                OnSelected = () =>
                {
                    QualitySettings.vSyncCount = (int)SettingsStorage.typingSpeed;
                    if ((int)SettingsStorage.typingSpeed < 2)
                    {
                        SettingsStorage.typingSpeed++;
                    }
                    else
                    {
                        SettingsStorage.typingSpeed = TypingSpeed.Slow;
                    }

                    PlayerPrefs.SetInt("typingSpeed",
                        Convert.ToInt32(SettingsStorage.typingSpeed));
                },
                GetSpDataNameWithIndex = new Dictionary<int, string>
                {
                    { 0, "Slow" },
                    { 1, "Medium" },
                    { 2, "Fast" }
                }
            });

            #endregion
        }

        //TODO: CC字幕
    }

    #endregion

    //TODO: 无障碍Layer
    //  TODO: 讲述人
    //  TODO: 色盲模式
    
    

    //TODO: 成就Layer
    //  TODO: 成就页面
    //  TODO: 成就页面设置
}