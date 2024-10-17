using System.Collections.Generic;
using UnityEngine;

namespace Debug
{
    public class DebugPolygonScaler : MonoBehaviour
    {
        public LineRenderer originalLineRenderer; // 用于绘制原始多边形
        public LineRenderer movedLineRenderer; // 用于绘制移动后的多边形
        public float moveDistance = 0.5f; // 向中心移动的距离
        // 创建一个简单的多边形顶点列表
        public  List<Vector2> vertices = new List<Vector2>
        {
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1)
        };

        private void Update()
        {
      

            // 计算多边形中点并移动顶点
            List<Vector2> movedVertices = MoveVerticesTowardsCenter(vertices, moveDistance);

            // 绘制原始多边形
            DrawPolygon(originalLineRenderer, vertices);

            // 绘制移动后的多边形
            DrawPolygon(movedLineRenderer, movedVertices);
        }

        // 向中心移动多边形顶点的方法
        private List<Vector2> MoveVerticesTowardsCenter(List<Vector2> originalVertices, float distance)
        {
            Vector2 center = CalculatePolygonCenter(originalVertices);
            List<Vector2> movedVertices = new List<Vector2>();
            foreach (Vector2 vertex in originalVertices)
            {
                Vector2 direction = (center - vertex).normalized; // 从顶点到中心点的方向
                Vector2 movedVertex = vertex + direction * distance; // 向中心点移动固定距离
                movedVertices.Add(movedVertex);
            }
            return movedVertices;
        }

        // 计算多边形中心的方法
        private Vector2 CalculatePolygonCenter(List<Vector2> vertices)
        {
            Vector2 sum = Vector2.zero;
            foreach (Vector2 vertex in vertices)
            {
                sum += vertex;
            }
            return sum / vertices.Count;
        }

        // 使用LineRenderer绘制多边形的方法
        private void DrawPolygon(LineRenderer lineRenderer, List<Vector2> vertices)
        {
            lineRenderer.positionCount = vertices.Count + 1; // 设置顶点数（+1是为了闭合多边形）
            for (int i = 0; i < vertices.Count; i++)
            {
                lineRenderer.SetPosition(i, vertices[i]);
            }
            lineRenderer.SetPosition(vertices.Count, vertices[0]); // 闭合多边形
        }
    }
}
