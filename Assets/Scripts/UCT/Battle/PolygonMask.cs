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
        private static readonly int PolygonCount = Shader.PropertyToID("_PolygonCount");
        private static readonly int PolygonInfos = Shader.PropertyToID("_PolygonInfos");

        private static readonly int PolygonVertices = Shader.PropertyToID("_PolygonVertices");
        private static readonly int Mode = Shader.PropertyToID("_Mode");

        private int _currentPolygonInfoCount;
        private int _currentPolygonVertexCount;

        private ComputeBuffer _polygonInfoBuffer;
        private ComputeBuffer _polygonVerticesBuffer;
        private GameObject _projectionBoxes;
        private Renderer _targetRenderer;

        private void Awake()
        {
            _targetRenderer = GetComponent<Renderer>();
            var material = Instantiate(Resources.Load<Material>("Materials/PolygonMask"));

            if (!_targetRenderer)
            {
                Debug.LogError("PolygonMask 需要一个 Renderer 组件（SpriteRenderer 或 LineRenderer）！");
                enabled = false;
                return;
            }

            _targetRenderer.material = material;
            _projectionBoxes = MainControl.Instance.selectUIController.projectionBoxes;
        }

        private void Update()
        {
            UpdateBuffers();
        }

        private void OnEnable()
        {
            UpdateBuffers();
            if (_targetRenderer is LineRenderer)
            {
                _targetRenderer.material.SetFloat(Mode, 1);
            }
        }


        private void OnDestroy()
        {
            ReleaseBuffers();
        }

        private void ReleaseBuffers()
        {
            if (_polygonInfoBuffer != null)
            {
                _polygonInfoBuffer.Release();
                _polygonInfoBuffer = null;
            }

            if (_polygonVerticesBuffer != null)
            {
                _polygonVerticesBuffer.Release();
                _polygonVerticesBuffer = null;
            }

            _currentPolygonInfoCount = 0;
            _currentPolygonVertexCount = 0;
        }

        private void UpdateBuffers()
        {
            var polygons = SetPolygons();
            var (polygonInfos, verticesList) = ProcessPolygons(polygons);

            if (verticesList.Count == 0)
            {
                verticesList.Add(Vector2.zero);
            }

            if (polygonInfos.Count == 0)
            {
                HandleEmptyPolygons();
                return;
            }

            UpdateComputeBuffer(ref _polygonInfoBuffer, polygonInfos, sizeof(float) + sizeof(int), PolygonInfos,
                polygonInfos.Count, ref _currentPolygonInfoCount);
            UpdateComputeBuffer(ref _polygonVerticesBuffer, verticesList, sizeof(float) * 2, PolygonVertices,
                verticesList.Count, ref _currentPolygonVertexCount);

            _targetRenderer.material.SetInt(PolygonCount, polygonInfos.Count);
        }

        private (List<PolygonInfo>, List<Vector2>) ProcessPolygons(List<List<Vector2>> polygons)
        {
            var polygonInfos = new List<PolygonInfo>();
            var verticesList = new List<Vector2>();

            if (polygons == null)
            {
                return (polygonInfos, verticesList);
            }

            foreach (var polygon in polygons.Where(polygon => polygon != null && polygon.Count != 0))
            {
                polygonInfos.Add(new PolygonInfo
                {
                    VertexCount = polygon.Count,
                    StartIndex = verticesList.Count
                });
                verticesList.AddRange(polygon);
            }

            return (polygonInfos, verticesList);
        }

        private void HandleEmptyPolygons()
        {
            _targetRenderer.material.SetInt(PolygonCount, 0);
            EnsureBufferInitialized(ref _polygonInfoBuffer, 1, sizeof(float) + sizeof(int), PolygonInfos,
                ref _currentPolygonInfoCount, new PolygonInfo { VertexCount = 0, StartIndex = 0 });
            EnsureBufferInitialized(ref _polygonVerticesBuffer, 1, sizeof(float) * 2, PolygonVertices,
                ref _currentPolygonVertexCount, Vector2.zero);
        }

        private void EnsureBufferInitialized<T>(ref ComputeBuffer buffer,
            int size,
            int stride,
            int shaderProperty,
            ref int currentCount,
            T defaultValue) where T : struct
        {
            if (buffer == null)
            {
                buffer = new ComputeBuffer(size, stride);
                _targetRenderer.material.SetBuffer(shaderProperty, buffer);
                currentCount = size;
            }

            buffer.SetData(new[] { defaultValue });
        }

        private void UpdateComputeBuffer<T>(ref ComputeBuffer buffer,
            List<T> data,
            int stride,
            int shaderProperty,
            int newSize,
            ref int currentSize) where T : struct
        {
            if (buffer == null || currentSize != newSize)
            {
                buffer?.Release();
                buffer = new ComputeBuffer(newSize, stride);
                _targetRenderer.material.SetBuffer(shaderProperty, buffer);
                currentSize = newSize;
            }

            buffer.SetData(data);
        }

        private List<List<Vector2>> SetPolygons()
        {
            if (!BoxController.Instance || BoxController.Instance.boxes == null)
            {
                return new List<List<Vector2>>();
            }

            var polygons = new List<List<Vector2>>();
            const float offset = -0.065f;
            foreach (var boxDrawer in BoxController.Instance.boxes)
            {
                if (boxDrawer.realPoints == null)
                {
                    continue;
                }

                var points = boxDrawer.realPoints
                    .Select(p => RotatePoint(Vector2.zero, p, boxDrawer.rotation.eulerAngles.z) +
                                 (Vector2)boxDrawer.transform.position)
                    .ToList();
                polygons.Add(MathUtilityService.CalculateInwardOffset(points, offset));

                for (var i = 0; i < _projectionBoxes.transform.childCount; i++)
                {
                    points = boxDrawer.realPoints
                        .Select(p =>
                        {
                            var projectionBoxes = _projectionBoxes.transform.GetChild(i);
                            return RotatePoint(Vector2.zero, p,
                                       boxDrawer.rotation.eulerAngles.z + projectionBoxes.eulerAngles.z) +
                                   (Vector2)boxDrawer.localPosition +
                                   (Vector2)projectionBoxes.transform.position;
                        })
                        .ToList();
                    polygons.Add(MathUtilityService.CalculateInwardOffset(points, offset));
                }
            }

            return polygons;
        }

        /// <summary>
        ///     获取旋转后的点坐标
        /// </summary>
        private static Vector2 RotatePoint(Vector2 center, Vector2 point, float angle)
        {
            if (Mathf.Approximately(angle, 0f) || Mathf.Approximately(angle % 360f, 0f))
            {
                return point;
            }

            var radian = angle * Mathf.Deg2Rad;
            var cosA = Mathf.Cos(radian);
            var sinA = Mathf.Sin(radian);

            // 计算旋转后的点
            var relative = point - center;
            var rotatedPoint =
                new Vector2(relative.x * cosA - relative.y * sinA, relative.x * sinA + relative.y * cosA) + center;
            return rotatedPoint;
        }


        private struct PolygonInfo
        {
            // ReSharper disable once NotAccessedField.Local
            public float VertexCount;

            // ReSharper disable once NotAccessedField.Local
            public int StartIndex;
        }
    }
}