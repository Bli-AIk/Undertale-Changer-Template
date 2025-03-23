using UnityEngine;

namespace UCT.Overworld
{
    /// <summary>
    /// 在Overworld场景中，根据EdgeCollider绘制LineRenderer线，并在Chase时显示。
    /// 线条只会在初始化时绘制一次。
    /// </summary>
   
    [RequireComponent(typeof(EdgeCollider2D), typeof(LineRenderer))]
    public class OverworldChaseLineDrawer : MonoBehaviour
    {
        private EdgeCollider2D _edgeCollider2D;
        [HideInInspector]
        public LineRenderer lineRenderer;

        private void Start()
        {
            _edgeCollider2D = GetComponent<EdgeCollider2D>();
            lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.startColor = ColorEx.RedClear;
            lineRenderer.endColor = ColorEx.RedClear;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingLayerName = "Tilemap";
            lineRenderer.sortingOrder = 200;
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            UpdateLineRenderer();
        }


        private void UpdateLineRenderer()
        {
            var points = _edgeCollider2D.points;
            var positions = new Vector3[points.Length];

            for (var i = 0; i < points.Length; i++)
            {
                positions[i] = _edgeCollider2D.transform.TransformPoint(points[i]) + (Vector3)_edgeCollider2D.offset;
            }

            lineRenderer.positionCount = positions.Length;
            lineRenderer.SetPositions(positions);
        }
    }
}