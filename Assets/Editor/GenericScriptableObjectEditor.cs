using System.Collections.Generic;
using UCT.Control;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(ScriptableObject), true)]
    public class GenericScriptableObjectEditor : UnityEditor.Editor
    {
        private static readonly Dictionary<System.Type, string> IconPaths = new()
        {
            { typeof(AudioControl), "Assets/Sprites/icons/AudioIcon.png" },
            { typeof(BattleControl),"Assets/Sprites/icons/BattleIcon.png" },
            { typeof(OverworldControl), "Assets/Sprites/icons/OverworldIcon.png" },
            { typeof(PlayerControl), "Assets/Sprites/icons/PlayerIcon.png" },
            { typeof(ItemControl), "Assets/Sprites/icons/ItemIcon.png" },
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
