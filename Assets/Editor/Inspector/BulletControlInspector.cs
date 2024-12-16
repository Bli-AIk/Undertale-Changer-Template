using System.Collections.Generic;
using UCT.Control;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using static UCT.Battle.BulletController;

namespace Editor.Inspector
{
    [CustomEditor(typeof(BulletControl))]
    public class BulletControlInspector : UnityEditor.Editor
    {
        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            var bullet = (BulletControl)target;

            if (!bullet.sprite) return base.RenderStaticPreview(assetPath, subAssets, width, height);
            var icon = new Texture2D(width, height);
            EditorUtility.CopySerialized(bullet.sprite.texture, icon);
            return icon;

        }

        public override VisualElement CreateInspectorGUI()
        {
            var bullet = (BulletControl)target;

            var root = new VisualElement();

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            if (bullet.sprite == null) return root;
            var background = new VisualElement()
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
            var spr = new Image
            {
                image = bullet.sprite.texture,
                style =
                {
                    width = 150,
                    height = 150,
                }
            };

            var gridPainter = new GridPainter
            {
                style =
                {
                    flexGrow = 1,
                },
                TriggerSize = bullet.triggerSize,
                TriggerOffset = bullet.triggerOffset,
                TriggerFollowMode = bullet.triggerFollowMode
            };

            root.Add(background);
            background.Add(spr);
            spr.Add(gridPainter);

            return root;
        }

        private class GridPainter : ImmediateModeElement
        {
            private readonly Color _lineColor = Color.green;

            public List<Vector2> TriggerSize = new();
            public List<Vector2> TriggerOffset = new();

            public FollowMode TriggerFollowMode = FollowMode.NoFollow;

            protected override void ImmediateRepaint()
            {
                var rect = contentRect;

                for (var i = 0; i < TriggerSize.Count; i++)
                {
                    var size = TriggerSize[i];
                    var offset = TriggerOffset[i];

                    if (TriggerFollowMode == FollowMode.CutFollow)
                        size = Vector2.one - TriggerSize[i];

                    var centerX = offset.x * rect.width + rect.width / 2;
                    var centerY = (offset.y - 1) * rect.height + rect.height / 2;

                    var halfWidth = size.x * rect.width / 2;
                    var halfHeight = size.y * rect.height / 2;

                    var xStart = centerX - halfWidth;
                    var yStart = -centerY + halfHeight;
                    var xEnd = centerX + halfWidth;
                    var yEnd = -centerY - halfHeight;

                    Handles.color = _lineColor;

                    Handles.DrawLine(new Vector3(xStart, yStart), new Vector3(xEnd, yStart));
                    Handles.DrawLine(new Vector3(xStart, yEnd), new Vector3(xEnd, yEnd));
                    Handles.DrawLine(new Vector3(xStart, yStart), new Vector3(xStart, yEnd));
                    Handles.DrawLine(new Vector3(xEnd, yStart), new Vector3(xEnd, yEnd));
                }
            }
        }


    }
}
