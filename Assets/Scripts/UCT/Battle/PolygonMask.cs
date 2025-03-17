using System;
using System.Collections.Generic;
using System.Linq;
using UCT.Core;
using UCT.Service;
using UCT.UI;
using UnityEngine;

namespace UCT.Battle
{
    public class PolygonMask : MonoBehaviour
    {
        private static readonly int VerticesTex = Shader.PropertyToID("_VerticesTex");
        private Material _material;
        private GameObject _projectionBoxes;

        private void Awake()
        {
            _material = Instantiate(Resources.Load<Material>("Materials/PolygonMask"));
            GetComponent<SpriteRenderer>().material = _material;
            _projectionBoxes = MainControl.Instance.selectUIController.projectionBoxes;
        }

        private Texture2D _vertexTexture;

        private void OnEnable()
        {
            InitializeTexture();
            UpdateTexture();
        }

        private void FixedUpdate()
        {
            UpdateTexture();
        }

        private void InitializeTexture()
        {
            var polygons = SetPolygons();
            if (polygons == null || polygons.Count == 0)
            {
                return;
            }

            var textureWidth = polygons.Max(p => p.Count) + 1;
            var textureHeight = polygons.Count;

            _vertexTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            _material.SetTexture(VerticesTex, _vertexTexture);
        }

        private void UpdateTexture()
        {
            var polygons = SetPolygons();
            if (polygons == null || polygons.Count == 0 || !_vertexTexture)
            {
                return;
            }

            for (var y = 0; y < polygons.Count; y++)
            {
                var polygon = polygons[y];

                _vertexTexture.SetPixel(0, y, new Color(polygon.Count, polygons.Count, 0f, 1f));

                for (var x = 0; x < polygon.Count; x++)
                {
                    _vertexTexture.SetPixel(x + 1, y, new Color(polygon[x].x, polygon[x].y, 0f, 1f));
                }
            }

            _vertexTexture.Apply();
        }


        private List<List<Vector2>> SetPolygons()
        {
            if (!BoxController.Instance || BoxController.Instance.boxes == null)
            {
                return new List<List<Vector2>>();
            }

            var polygons = new List<List<Vector2>>();
            const float offset = -0.075f;
            foreach (var boxDrawer in BoxController.Instance.boxes)
            {
                if (boxDrawer.realPoints == null)
                {
                    continue;
                }

                var points = boxDrawer.realPoints
                    .Select(p => p + (Vector2)boxDrawer.transform.position).ToList();
                polygons.Add(MathUtilityService.CalculateInwardOffset(points, offset));

                for (var i = 0; i < _projectionBoxes.transform.childCount; i++)
                {
                    points = boxDrawer.realPoints
                        .Select(p =>
                            p + (Vector2)boxDrawer.localPosition +
                            (Vector2)_projectionBoxes.transform.GetChild(i).transform.position).ToList();
                    polygons.Add(MathUtilityService.CalculateInwardOffset(points, offset));
                }
            }

            return polygons;
        }


    }
}