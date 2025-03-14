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
        private List<List<Vector2>> Polygons { get; set; } = new();

        private void Awake()
        {
            _material = Instantiate(Resources.Load<Material>("Materials/PolygonMask"));
            GetComponent<SpriteRenderer>().material = _material;
            _projectionBoxes = MainControl.Instance.selectUIController.projectionBoxes;
        }

        private void OnEnable()
        {
            UpdateVertexTexture();
        }

        private void Update()
        {
            UpdateVertexTexture();
        }

        private void SetPolygons()
        {
            if (!BoxController.Instance || BoxController.Instance.boxes == null)
            {
                return;
            }

            Polygons ??= new List<List<Vector2>>();
            Polygons.Clear();
            const float offset = -0.075f;
            foreach (var boxDrawer in BoxController.Instance.boxes)
            {
                if (boxDrawer.realPoints == null)
                {
                    continue;
                }

                var points = boxDrawer.realPoints.Select(p => p + (Vector2)boxDrawer.transform.position).ToList();
                Polygons.Add(MathUtilityService.CalculateInwardOffset(points, offset));

                for (var i = 0; i < _projectionBoxes.transform.childCount; i++)
                {
                    points = boxDrawer.realPoints
                        .Select(p => p + (Vector2)_projectionBoxes.transform.GetChild(i).transform.position).ToList();
                    Polygons.Add(MathUtilityService.CalculateInwardOffset(points, offset));
                }
            }
        }


        private void UpdateVertexTexture()
        {
            SetPolygons();
            if (Polygons == null || Polygons.Count == 0)
            {
                return;
            }

            var textureWidth = Polygons.Max(p => p.Count) + 1;
            var textureHeight = Polygons.Count;

            var vertexTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < Polygons.Count; y++)
            {
                var polygon = Polygons[y];

                vertexTexture.SetPixel(0, y, new Color(polygon.Count, Polygons.Count, 0f, 1f));

                for (var x = 0; x < polygon.Count; x++)
                {
                    vertexTexture.SetPixel(x + 1, y, new Color(polygon[x].x, polygon[x].y, 0f, 1f));
                }
            }

            vertexTexture.Apply();
            _material.SetTexture(VerticesTex, vertexTexture);
        }
    }
}