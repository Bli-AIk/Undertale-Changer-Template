using System.Collections.Generic;
using UnityEngine;

namespace UCT.Settings
{
    public static class SettingsStorage
    {
        public static bool Pause { get; set; }

        public static bool TextWidth { get; set; }

        public static int ResolutionLevel { get; set; }

        public static int FrameRate { get; set; }

        public static bool FullScreen { get; set; }

        public static float MainVolume { get; set; } = 0.5f;

        public static float BGMVolume { get; set; } = 1;

        public static float FXVolume { get; set; } = 1;

        public static bool IsSimplifySfx { get; set; }

        public static bool IsDisplayFPS { get; set; }

        public static Vector2 Resolution { get; set; }

        public static VSyncMode VsyncMode { get; set; } = VSyncMode.DonNotSync;

        public static bool IsUsingHdFrame { get; set; }

        public static KeyBindingType KeyBindingType { get; set; } = KeyBindingType.Primary;

        public static TypingSpeed TypingSpeed { get; set; } = TypingSpeed.Medium;

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