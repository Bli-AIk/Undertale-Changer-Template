using System.Collections.Generic;
using UnityEngine;

namespace UCT.Global.Settings
{
    public static class SettingsStorage
    {
        public static bool Pause;
        public static bool TextWidth;
        public static int ResolutionLevel;
        public static int FrameRate;
        public static bool FullScreen;
        public static float MainVolume = 0.5f;
        public static float BGMVolume = 1;
        public static float FXVolume = 1;
        public static bool IsSimplifySfx;
        public static bool IsDisplayFPS;
        public static Vector2 Resolution;
        public static VSyncMode VsyncMode = VSyncMode.DonNotSync;
        public static bool IsUsingHdFrame;
        public static KeyBindingType KeyBindingType = KeyBindingType.Primary;
        public static TypingSpeed TypingSpeed = TypingSpeed.Medium;

        public static Dictionary<string, SettingsLayerBase> CubismSettingsLayers { get; private set; } = new()
        {
            { "HomeSettingsLayer", new HomeSettingsLayer() },
            { "VideoSettingsLayer", new VideoSettingsLayer() },
            { "AudioSettingsLayer", new AudioSettingsLayer() },
            { "GraphicSettingsLayer", new GraphicSettingsLayer() },
            { "InputSettingsLayer", new InputSettingsLayer() },
            { "SettingKeyControlLayer", new SettingKeyControlLayer() },
            { "SubtitleSettingsLayer", new SubtitleSettingsLayer() },
            { "SettingLanguagePackageLayer", new SettingLanguagePackageLayer() }
        };
    }

    public enum VSyncMode
    {
        DonNotSync,
        Sync,
        HalfSync
    }

    public enum TypingSpeed
    {
        Slow,
        Medium,
        Fast
    }
}