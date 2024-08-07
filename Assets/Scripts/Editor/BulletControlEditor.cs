using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;

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


            gridPainter.triggerSize = bullet.triggerSize;
            gridPainter.triggerOffset = bullet.triggerOffset;

            //gridPainter.GridSizeX = bullet.sprite.texture.width;
            //gridPainter.GridSizeY = bullet.sprite.texture.height;

            //Debug.Log(gridPainter.GridSizeX + "   " + gridPainter.GridSizeY);


            root.Add(background);
            background.Add(spr);
            spr.Add(gridPainter);
        }

        return root;
    }

    private class GridPainter : ImmediateModeElement
    {
        private Color lineColor = Color.green;

        // 存储每个Box Collider的大小和偏移的列表
        public List<Vector2> triggerSize = new List<Vector2>();
        public List<Vector2> triggerOffset = new List<Vector2>();

        protected override void ImmediateRepaint()
        {
            var rect = contentRect;

            // 遍历每个Box Collider
            for (int i = 0; i < triggerSize.Count; i++)
            {
                Vector2 size = triggerSize[i];
                Vector2 offset = triggerOffset[i];

                // 确定每个Box Collider的中心点
                float centerX = offset.x * rect.width + rect.width / 2;
                float centerY = (offset.y - 1) * rect.height + rect.height / 2;

                // 计算每个Box Collider的半宽度和半高度
                float halfWidth = size.x * rect.width / 2;
                float halfHeight = size.y * rect.height / 2;

                // 计算每个Box Collider的边界
                float xStart = centerX - halfWidth;
                float yStart = -centerY + halfHeight;
                float xEnd = centerX + halfWidth;
                float yEnd = -centerY - halfHeight;

                Handles.color = lineColor;

                // 绘制矩形的四条边
                // 下边
                Handles.DrawLine(new Vector3(xStart, yStart), new Vector3(xEnd, yStart));
                // 上边
                Handles.DrawLine(new Vector3(xStart, yEnd), new Vector3(xEnd, yEnd));
                // 左边
                Handles.DrawLine(new Vector3(xStart, yStart), new Vector3(xStart, yEnd));
                // 右边
                Handles.DrawLine(new Vector3(xEnd, yStart), new Vector3(xEnd, yEnd));
            }
        }
    }


}
