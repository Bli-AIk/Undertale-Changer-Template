using System;
using System.Collections.Generic;
using UCT.Control;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Default
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ScriptableObjectIconSetter : UnityEditor.Editor
    {
        private const string IconDefaultPath = "Icons";

        private static readonly Dictionary<Type, string> IconPaths = new()
        {
            { typeof(AudioControl), GetIconPath("Control/AudioIcon") },
            { typeof(BattleControl), GetIconPath("Control/BattleIcon") },
            { typeof(OverworldControl), GetIconPath("Control/OverworldIcon") },
            { typeof(PlayerControl), GetIconPath("Control/PlayerIcon") },
            { typeof(LanguagePackControl), GetIconPath("Control/LanguagePackIcon") }
        };

        private static string GetIconPath(string iconName)
        {
            return $"{IconDefaultPath}/{iconName}.png";
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var type = target.GetType();

            if (!IconPaths.TryGetValue(type, out var iconPath))
            {
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }

            var icon = EditorGUIUtility.Load(iconPath) as Texture2D;

            if (!icon)
            {
                throw new NullReferenceException();
            }

            var previewIcon = new Texture2D(width, height);
            EditorUtility.CopySerialized(icon, previewIcon);
            return previewIcon;
        }
    }
}