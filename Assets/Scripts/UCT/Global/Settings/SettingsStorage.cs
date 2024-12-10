using System.Collections.Generic;
using UnityEngine;

namespace UCT.Global.Settings
{
    public static class SettingsStorage
    {
        public static Dictionary<string, SettingsLayerBase> CubismSettingsLayers { get; private set; } = new()
        {
            { "HomeSettingsLayer", new HomeSettingsLayer() },
            { "VideoSettingsLayer", new VideoSettingsLayer() },
            { "AudioSettingsLayer", new AudioSettingsLayer() },
            { "GraphicSettingsLayer", new GraphicSettingsLayer() },
            { "InputSettingsLayer", new InputSettingsLayer() },
            { "SettingKeyControlLayer", new SettingKeyControlLayer() },
            { "SettingLanguagePackageLayer", new SettingLanguagePackageLayer() },
        };

        public static bool pause;
        public static bool textWidth;
        public static int resolutionLevel;
        public static int frameRate;
        public static bool fullScreen;
        public static float mainVolume = 0.5f;
        public static bool isSimplifySfx;
        public static bool isDisplayFPS;
        public static Vector2 resolution;
        public static VSyncMode vsyncMode = VSyncMode.DonNotSync;
        public static bool isUsingHdFrame;
        public static KeyBindingType KeyBindingType = KeyBindingType.Primary;
    }

    public enum VSyncMode
    {
        DonNotSync,
        Sync,
        HalfSync
    }
}