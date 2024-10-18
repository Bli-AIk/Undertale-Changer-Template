using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace UCT.Global.UI
{
    /// <summary>
    /// 使用LineRenderer与多边形shader绘制多边形框，用于战斗框、UI等。
    /// </summary>
    [Obsolete]
    public class OldBoxController : MonoBehaviour
    {
        [Header("线长")]
        public float width;

        [Header("顶点数")]
        public int pointsMax = 4;

        [Header("顶点")]
        public List<Transform> points = new List<Transform>();

        [Header("开启碰撞（用于战斗框）")]
        public bool isCollider;

        [Header("ID检测：使用_Point (0)")]
        public bool useBracketId;

        [Header("使用这个可以让它创建时绘制正多边形")]
        public bool startDraw;

        [Header("关闭自动获取材质")]
        public bool noAutoMaterial;

        private PolygonCollider2D _polygonCollider2D;
        private EdgeCollider2D _edgeCollider2D;

        private LineRenderer _lineRenderer;
        private Material _material;

        private void Start()
        {
            points.Clear();
            if (pointsMax < 3)
            {
                Other.Debug.Log("pointsMax < 3 , 已更改为3");
                pointsMax = 3;
            }
            for (var i = 0; i < pointsMax; i++)
            {
                if (!useBracketId)
                    points.Add(transform.Find("Point" + i));
                else
                    points.Add(transform.Find("Point (" + i + ")"));
                if (points[i] == null)
                {
                    var obj = Instantiate(new GameObject());
                    obj.transform.SetParent(transform);
                    if (!useBracketId)
                        obj.name = "Point" + i;
                    else obj.name = "Point (" + i + ")";

                    points[i] = obj.transform;
                }
            }

            _lineRenderer = gameObject.GetComponent<LineRenderer>();
            _lineRenderer.loop = true;
            _lineRenderer.positionCount = points.Count;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            if (!noAutoMaterial)
            {
                _material = Instantiate(Resources.Load<Material>("Materials/DrawFrame"));
                transform.Find("Back").GetComponent<SpriteRenderer>().material = _material;
            }
            else
                _material = transform.Find("Back").GetComponent<SpriteRenderer>().material;

            if (isCollider)
            {
                _polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>() ?? gameObject.GetComponent<PolygonCollider2D>();

                _polygonCollider2D.pathCount = 2;
                _polygonCollider2D.SetPath(0, new Vector2[4] { new Vector2(100, 100), new Vector2(-100, 100), new Vector2(-100, -100), new Vector2(100, -100) });

                _edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>() ?? gameObject.GetComponent<EdgeCollider2D>();
                _edgeCollider2D.edgeRadius = width / 2;
            }

            if (startDraw)
            {
                points = Draw(points, 2);
            }
        }

        private void Update()
        {
            var localPoss = new List<Vector2>();
            for (var i = 0; i < points.Count; i++)
            {
                _lineRenderer.SetPosition(i, points[i].transform.position);
                if (!useBracketId)
                    _material.SetVector("_Point" + i, points[i].transform.position);
                else
                    _material.SetVector("_Point_" + i, points[i].transform.position);
                localPoss.Add(points[i].transform.localPosition);
            }
            if (isCollider)
            {
                _polygonCollider2D.SetPath(1, localPoss.ToArray());
                localPoss.Add(localPoss[0]);
                _edgeCollider2D.SetPoints(localPoss);
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                var rectangleVertices = new List<Vector2>
                {
                    new Vector2(-1, 1),  // Top-left
                    new Vector2(1, 1),   // Top-right
                    new Vector2(-1, -1), // Bottom-left
                    new Vector2(1, -1)   // Bottom-right
                };

                AnimateToRectangle(points, 1, rectangleVertices);
            }
        }

        private List<Transform> Draw(List<Transform> pointList, float drawRadius)
        {
            var sides = pointList.Count;  // 根据points的数量确定边数
            var drawnPoints = new List<Transform>();

            for (var i = 0; i < sides; i++)
            {
                var angle = 2 * Mathf.PI * i / sides;
                var pos = new Vector3(Mathf.Cos(angle) * drawRadius, Mathf.Sin(angle) * drawRadius, 0);

                if (pointList[i] != null)
                {
                    pointList[i].transform.position = pos;
                    drawnPoints.Add(pointList[i]);
                }
            }

            return drawnPoints;
        }

        private void AnimateToRectangle(List<Transform> pointList, float duration, List<Vector2> vertices)
        {
            if (pointList.Count != vertices.Count)
            {
                Other.Debug.Log("The number of points and vertices must be the same.");
                return;
            }

            // Calculate the new positions for the points
            var newPositions = new Vector2[pointList.Count];
            for (var i = 0; i < pointList.Count; i++)
            {
                var offset = vertices[i] - (Vector2)pointList[i].position;
                newPositions[i] = vertices[i] - offset.normalized * offset.magnitude;
            }

            // Apply the animations using DOTween
            for (var i = 0; i < pointList.Count; i++)
            {
                pointList[i].DOMove(new Vector3(newPositions[i].x, newPositions[i].y, 0), duration);
            }
        }
    }
}