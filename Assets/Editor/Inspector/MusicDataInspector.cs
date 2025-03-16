using UCT.Control;
using UnityEditor;
using UnityEngine;

namespace Editor.Inspector
{
    [CustomEditor(typeof(MusicData), true)]
    public class MusicDataInspector : UnityEditor.Editor
    {
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var musicData = (MusicData)target;

            if (!musicData.cover)
            {
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }

            var firstSprite = musicData.cover.texture;
            var previewIcon = new Texture2D(width, height);
            EditorUtility.CopySerialized(firstSprite, previewIcon);
            return previewIcon;
        }
    }
}