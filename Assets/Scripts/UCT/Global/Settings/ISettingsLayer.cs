using System;
using System.Collections.Generic;
using UCT.Control;
using UCT.Service;
using UnityEngine;

namespace UCT.Global.Settings
{
    /// <summary>
    ///     设置页面层级接口
    /// </summary>
    public interface ISettingsLayer
    {
        List<SettingsOption> SettingsOptions { get; } // 选项列表
    }

    public abstract class SettingsLayerBase : ISettingsLayer
    {
        public List<SettingsOption> SettingsOptions { get; protected set; } = new();
    }


    public class HomeSettingsLayer : SettingsLayerBase
    {
        public HomeSettingsLayer()
        {
            var overworldControl = Resources.Load<OverworldControl>("OverworldControl");
            SettingsOptions.Add(new SettingsOption(overworldControl.mainVolume)
            {
                DataName = "SettingMainVolume",
                DescriptionDataName = new []{"SettingMainVolumeTip"},
                Type = OptionType.SelectionBasedFalse,
                OptionDisplayMode = OptionDisplayMode.Percentage,
                SelectionBasedChangedValueGetter = () => AudioListener.volume,
                SelectionBasedChangedValueSetter = value =>
                {
                    AudioListener.volume = (float)value;
                    overworldControl.mainVolume = (float)value;
                },
                SelectionBasedChangedUnit = 0.01f,
                SelectionBasedChangedMax = 1,
                SelectionBasedChangedMin = 0
            });

            SettingsOptions.Add(new SettingsOption(null)
            {
                DataName = "SettingControl",
                DescriptionDataName = new []{"SettingControlTip"},
                Type = OptionType.EnterLayer
            });

            SettingsOptions.Add(new SettingsOption(false)
            {
                DataName = "SettingFullScreen",
                DescriptionDataName = new []{"SettingFullScreenTipOpen","SettingFullScreenTipClose"},
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => overworldControl.fullScreen,
                SelectionBasedChangedValueSetter = value => overworldControl.fullScreen = (bool)value,
                OnSelected = () =>
                {
                    overworldControl.fullScreen = !overworldControl.fullScreen;
                    GameUtilityService.SetResolution(overworldControl.resolutionLevel);
                }
            });

            SettingsOptions.Add(new SettingsOption(0)
            {
                DataName = "SettingResolving",
                DescriptionDataName = new []{"SettingResolvingTip"},
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Resolution,
                SelectionBasedChangedValueGetter = () => overworldControl.resolution,
                SelectionBasedChangedValueSetter = value => overworldControl.resolution = (Vector2)value,
                OnSelected = () =>
                {
                    overworldControl.resolutionLevel = GameUtilityService.UpdateResolutionSettings
                        (overworldControl.isUsingHdFrame, overworldControl.resolutionLevel);
                }
            });

            SettingsOptions.Add(new SettingsOption(false)
            {
                DataName = "SettingSFX",
                DescriptionDataName = new []{"SettingSFXTip"},
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => overworldControl.isSimplifySfx,
                SelectionBasedChangedValueSetter = value => overworldControl.isSimplifySfx = (bool)value,
                OnSelected = () =>
                {
                    overworldControl.isSimplifySfx = !overworldControl.isSimplifySfx;
                    GameUtilityService.ToggleAllSfx(overworldControl.isSimplifySfx);
                    PlayerPrefs.SetInt("noSFX", Convert.ToInt32(overworldControl.isSimplifySfx));
                }
            });

            SettingsOptions.Add(new SettingsOption(false)
            {
                DataName = "SettingFPS",
                DescriptionDataName = new []{"SettingFPSTip"},
                Type = OptionType.SelectionToggle,
                OptionDisplayMode = OptionDisplayMode.Default,
                SelectionBasedChangedValueGetter = () => overworldControl.isDisplayFPS,
                SelectionBasedChangedValueSetter = value => overworldControl.isDisplayFPS = (bool)value,
                OnSelected = () => { overworldControl.isDisplayFPS = !overworldControl.isDisplayFPS; }
            });


            SettingsOptions.Add(new SettingsOption("Menu")
            {
                DataName = "SettingBackMenu",
                DescriptionDataName = new []{"SettingBackMenuTip"},
                Type = OptionType.EnterScene
            });

            SettingsOptions.Add(new SettingsOption(null)
            {
                DataName = "SettingBackGame",
                DescriptionDataName = new []{"SettingBackGameTip"},
                Type = OptionType.Back
            });
        }
    }
}