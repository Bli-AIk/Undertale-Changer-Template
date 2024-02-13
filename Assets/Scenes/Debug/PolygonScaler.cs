using System.Collections.Generic;
using UnityEngine;

public class PolygonScaler : MonoBehaviour
{
    public LineRenderer originalLineRenderer; // ���ڻ���ԭʼ�����
    public LineRenderer movedLineRenderer; // ���ڻ����ƶ���Ķ����
    public float moveDistance = 0.5f; // �������ƶ��ľ���
                                      // ����һ���򵥵Ķ���ζ����б�
  public  List<Vector2> vertices = new List<Vector2>()
        {
            new Vector2(-1, -1),
            new Vector2(1, -1),
            new Vector2(1, 1),
            new Vector2(-1, 1)
        };

    void Update()
    {
      

        // ���������е㲢�ƶ�����
        List<Vector2> movedVertices = MoveVerticesTowardsCenter(vertices, moveDistance);

        // ����ԭʼ�����
        DrawPolygon(originalLineRenderer, vertices);

        // �����ƶ���Ķ����
        DrawPolygon(movedLineRenderer, movedVertices);
    }

    // �������ƶ�����ζ���ķ���
    private List<Vector2> MoveVerticesTowardsCenter(List<Vector2> originalVertices, float distance)
    {
        Vector2 center = CalculatePolygonCenter(originalVertices);
        List<Vector2> movedVertices = new List<Vector2>();
        foreach (Vector2 vertex in originalVertices)
        {
            Vector2 direction = (center - vertex).normalized; // �Ӷ��㵽���ĵ�ķ���
            Vector2 movedVertex = vertex + direction * distance; // �����ĵ��ƶ��̶�����
            movedVertices.Add(movedVertex);
        }
        return movedVertices;
    }

    // �����������ĵķ���
    private Vector2 CalculatePolygonCenter(List<Vector2> vertices)
    {
        Vector2 sum = Vector2.zero;
        foreach (Vector2 vertex in vertices)
        {
            sum += vertex;
        }
        return sum / vertices.Count;
    }

    // ʹ��LineRenderer���ƶ���εķ���
    private void DrawPolygon(LineRenderer lineRenderer, List<Vector2> vertices)
    {
        lineRenderer.positionCount = vertices.Count + 1; // ���ö�������+1��Ϊ�˱պ϶���Σ�
        for (int i = 0; i < vertices.Count; i++)
        {
            lineRenderer.SetPosition(i, vertices[i]);
        }
        lineRenderer.SetPosition(vertices.Count, vertices[0]); // �պ϶����
    }
}
