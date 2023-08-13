using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Use LineRenderer and Polygon Shader to draw polygonal boxes for combat boxes, UI, and more.
/// </summary>
public class DrawFrameController : MonoBehaviour
{
    [Header("width")]
    public float width;

    [Header("points max")]
    public int pointsMax = 4;


    [Header("points")]
    public List<Transform> points = new List<Transform>();

    [Header("Enable collision (for battle boxes)")]
    public bool isCollider;
    PolygonCollider2D polygonCollider2D;
    EdgeCollider2D edgeCollider2D;

    LineRenderer lineRenderer;
    Material material;
    
    
    void Start()
    {
        points.Clear();
        if (pointsMax < 3)
        {
            Debug.LogWarning("pointsMax < 3 , Changed to 3");
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
