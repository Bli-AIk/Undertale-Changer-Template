using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections.Generic;
using static BulletController;

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
            gridPainter.triggerFollowMode = bullet.triggerFollowMode;
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

        // �洢ÿ��Box Collider�Ĵ�С��ƫ�Ƶ��б�
        public List<Vector2> triggerSize = new List<Vector2>();
        public List<Vector2> triggerOffset = new List<Vector2>();

        public FollowMode triggerFollowMode = FollowMode.NoFollow;

        protected override void ImmediateRepaint()
        {
            var rect = contentRect;

            // ����ÿ��Box Collider
            for (int i = 0; i < triggerSize.Count; i++)
            {
                Vector2 size = triggerSize[i];
                Vector2 offset = triggerOffset[i];

                if (triggerFollowMode == FollowMode.CutFollow)
                    size = Vector2.one - triggerSize[i];

                // ȷ��ÿ��Box Collider�����ĵ�
                float centerX = offset.x * rect.width + rect.width / 2;
                float centerY = (offset.y - 1) * rect.height + rect.height / 2;

                // ����ÿ��Box Collider�İ���ȺͰ�߶�
                float halfWidth = size.x * rect.width / 2;
                float halfHeight = size.y * rect.height / 2;

                // ����ÿ��Box Collider�ı߽�
                float xStart = centerX - halfWidth;
                float yStart = -centerY + halfHeight;
                float xEnd = centerX + halfWidth;
                float yEnd = -centerY - halfHeight;

                Handles.color = lineColor;

                // ���ƾ��ε�������
                // �±�
                Handles.DrawLine(new Vector3(xStart, yStart), new Vector3(xEnd, yStart));
                // �ϱ�
                Handles.DrawLine(new Vector3(xStart, yEnd), new Vector3(xEnd, yEnd));
                // ���
                Handles.DrawLine(new Vector3(xStart, yStart), new Vector3(xStart, yEnd));
                // �ұ�
                Handles.DrawLine(new Vector3(xEnd, yStart), new Vector3(xEnd, yEnd));
            }
        }
    }


}
