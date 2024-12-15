using System.Collections.Generic;
using UCT.Control;
using UnityEditor;
using UnityEngine;

namespace Editor.Default
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class ScriptableObjectIconSetter : UnityEditor.Editor
    {
        private const string IconDefaultPath = "Assets/Sprites/icons";

        private static string GetIconPath(string iconName)
        {
            return $"{IconDefaultPath}/{iconName}.png";
        }

        private static readonly Dictionary<System.Type, string> IconPaths = new()
        {
            { typeof(AudioControl), GetIconPath("AudioIcon") },
            { typeof(BattleControl), GetIconPath("BattleIcon") },
            { typeof(OverworldControl), GetIconPath("OverworldIcon") },
            { typeof(PlayerControl), GetIconPath("PlayerIcon") },
            { typeof(ItemControl), GetIconPath("ItemIcon") },
        };

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var type = target.GetType();

            if (!IconPaths.TryGetValue(type, out var iconPath))
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            if (icon == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
            var previewIcon = new Texture2D(width, height);
            EditorUtility.CopySerialized(icon, previewIcon);
            return previewIcon;

        }
    }
}
