using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BulletControl))]
public class BulletControlEditor : Editor
{
    public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
    {
        BulletControl bullet = (BulletControl)target;

        if (bullet.sprite != null)
        {
            Texture2D icon = new Texture2D(width, height);
            EditorUtility.CopySerialized(bullet.sprite.texture, icon);
            return icon;
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}
