using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using static UnityEngine.UI.Image;
using static UnityEngine.Rendering.DebugUI.MessageBox;

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

    public override VisualElement CreateInspectorGUI()
    {

        BulletControl bullet = (BulletControl)target;

        VisualElement root = new VisualElement();

        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        if (bullet.sprite != null)
        {
            VisualElement background = new VisualElement()
            {
                style =
                {
                    height = 250,
                    marginTop = 10,
                    marginBottom = 10,
                    flexGrow = 1,
                    backgroundColor = new Color(42f / 255f, 42f / 255f, 42f / 255f, 1f),
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                }
            };
            Image spr = new Image
            {
                image = bullet.sprite.texture,
                //scaleMode = ScaleMode.ScaleToFit,
                style =
                {
                    width = 150,
                    height = 150,
        }
            };
            //spr.style.width = new StyleLength(StyleKeyword.Auto);
            //spr.style.height = new StyleLength(StyleKeyword.Auto);

            GridPainter gridPainter = new GridPainter()
            {
                style =
                {
                    flexGrow = 1,
                }
            };

            gridPainter.GridSizeX = bullet.sprite.texture.width;
            gridPainter.GridSizeY = bullet.sprite.texture.height;

            Debug.Log(gridPainter.GridSizeX + "   " + gridPainter.GridSizeY);


            root.Add(background);
            background.Add(spr);
            spr.Add(gridPainter);
        }

        return root;
    }


    private class GridPainter : ImmediateModeElement
    {
        public float GridSizeX = 16f;
        public float GridSizeY = 16f;
        private readonly Color LineColor = Color.green;

        protected override void ImmediateRepaint()
        {
            var rect = contentRect;

            Debug.LogWarning(rect.width + "   " + rect.height);

            for (float x = 0; x < rect.width; x += rect.width / GridSizeX * 2)
            {
                Handles.color = LineColor;
                Handles.DrawLine(new Vector3(x, 0), new Vector3(x, rect.height));
            }

            for (float y = 0; y < rect.height; y += rect.width / GridSizeY * 2)
            {
                Handles.color = LineColor;
                Handles.DrawLine(new Vector3(0, y), new Vector3(rect.width, y));
            }
        }
    }
}
