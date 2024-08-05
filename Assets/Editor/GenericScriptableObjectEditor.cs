using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(ScriptableObject), true)]
public class GenericScriptableObjectEditor : Editor
{
    private static Dictionary<System.Type, string> iconPaths = new Dictionary<System.Type, string>
    {
        { typeof(AudioControl), "Assets/Sprites/icons/AudioIcon.png" },
        { typeof(BattleControl),"Assets/Sprites/icons/BattleIcon.png" },
        { typeof(OverworldControl), "Assets/Sprites/icons/OverworldIcon.png" },
        { typeof(PlayerControl), "Assets/Sprites/icons/PlayerIcon.png" },
        { typeof(ItemControl), "Assets/Sprites/icons/ItemIcon.png" },
    };

    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        System.Type type = target.GetType();

        if (iconPaths.TryGetValue(type, out string iconPath))
        {
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(iconPath);

            if (icon != null)
            {

                Texture2D previewIcon = new Texture2D(width, height);
                EditorUtility.CopySerialized(icon, previewIcon);
                return previewIcon;
            }
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}
