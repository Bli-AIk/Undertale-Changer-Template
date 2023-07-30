using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ʹ��LineRenderer������shader���ƶ���ο�����ս����UI�ȡ�
/// </summary>
public class DrawFrameController : MonoBehaviour
{
    [Header("�߳�")]
    public float width;

    [Header("������")]
    public int pointsMax = 4;


    [Header("����")]
    public List<Transform> points = new List<Transform>();

    [Header("������ײ������ս����")]
    public bool isCollider;
    PolygonCollider2D polygonCollider2D;
    EdgeCollider2D edgeCollider2D;

    LineRenderer lineRenderer;
    Material material;
    
    // Start is called before the first frame update
    void Start()
    {
        points.Clear();
        if (pointsMax < 3)
        {
            Debug.LogWarning("pointsMax < 3 , �Ѹ���Ϊ3");
            pointsMax = 3;
        }
        for (int i = 0; i < pointsMax; i++)
        {
            points.Add(transform.Find("Point" + i));
            if (points[i] == null)
            {
                GameObject obj = Instantiate(new GameObject(), transform);
                obj.name = "Point" + i;
                points[i] = obj.transform;
            }
        }


        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.positionCount = points.Count;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;

        material = Instantiate(Resources.Load<Material>("Materials/DrawFrame"));
        transform.Find("Back").GetComponent<SpriteRenderer>().material = material;

        if (isCollider)
        {
            polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>() ?? gameObject.GetComponent<PolygonCollider2D>();

            polygonCollider2D.pathCount = 2;
            polygonCollider2D.SetPath(0, new Vector2[4] { new Vector2(100, 100), new Vector2(-100, 100), new Vector2(-100, -100), new Vector2(100, -100) });

            edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>() ?? gameObject.GetComponent<EdgeCollider2D>();
            edgeCollider2D.edgeRadius = width / 2;
        }
    }

    // Update is called once per frame
    void Update()
    {
        List<Vector2> localPoss = new List<Vector2>();
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i].transform.position);
            material.SetVector("_Point" + i, points[i].transform.position);
            localPoss.Add(points[i].transform.localPosition);
        }
        if (isCollider)
        {
            polygonCollider2D.SetPath(1, localPoss.ToArray());
            localPoss.Add(localPoss[0]);
            edgeCollider2D.SetPoints(localPoss);
        }
    }
}
