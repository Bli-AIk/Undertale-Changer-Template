using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using Log;
using System;
/// <summary>
/// Use LineRenderer with polygon shader to draw polygonal boxes for combat boxes, UI, etc.
/// </summary>
[Obsolete]
public class OldBoxController : MonoBehaviour
{
    [Header("Line length")]
    public float width;

    [Header("Number of vertices")]
    public int pointsMax = 4;

    [Header("Vertex")]
    public List<Transform> points = new List<Transform>();

    [Header("Turn on collision (for combat box)")]
    public bool isCollider;

    [Header("ID detection: use _Point (0)")]
    public bool useBracketId;

    [Header("Use this to allow it to draw orthopolygons when it is created.")]
    public bool startDraw;

    [Header("Turn off automatic material acquisition")]
    public bool noAutoMaterial;

    private PolygonCollider2D polygonCollider2D;
    private EdgeCollider2D edgeCollider2D;

    private LineRenderer lineRenderer;
    private Material material;

    private void Start()
    {
        points.Clear();
        if (pointsMax < 3)
        {
            DebugLogger.Log("pointsMax < 3 , 已更改为3", DebugLogger.Type.war);
            pointsMax = 3;
        }
        for (int i = 0; i < pointsMax; i++)
        {
            if (!useBracketId)
                points.Add(transform.Find("Point" + i));
            else
                points.Add(transform.Find("Point (" + i + ")"));
            if (points[i] == null)
            {
                GameObject obj = Instantiate(new GameObject());
                obj.transform.SetParent(transform);
                if (!useBracketId)
                    obj.name = "Point" + i;
                else obj.name = "Point (" + i + ")";

                points[i] = obj.transform;
            }
        }

        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.positionCount = points.Count;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        if (!noAutoMaterial)
        {
            material = Instantiate(Resources.Load<Material>("Materials/DrawFrame"));
            transform.Find("Back").GetComponent<SpriteRenderer>().material = material;
        }
        else
            material = transform.Find("Back").GetComponent<SpriteRenderer>().material;

        if (isCollider)
        {
            polygonCollider2D = gameObject.AddComponent<PolygonCollider2D>() ?? gameObject.GetComponent<PolygonCollider2D>();

            polygonCollider2D.pathCount = 2;
            polygonCollider2D.SetPath(0, new Vector2[4] { new Vector2(100, 100), new Vector2(-100, 100), new Vector2(-100, -100), new Vector2(100, -100) });

            edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>() ?? gameObject.GetComponent<EdgeCollider2D>();
            edgeCollider2D.edgeRadius = width / 2;
        }

        if (startDraw)
        {
            points = Draw(points, 2);
        }
    }

    private void Update()
    {
        List<Vector2> localPoss = new List<Vector2>();
        for (int i = 0; i < points.Count; i++)
        {
            lineRenderer.SetPosition(i, points[i].transform.position);
            if (!useBracketId)
                material.SetVector("_Point" + i, points[i].transform.position);
            else
                material.SetVector("_Point_" + i, points[i].transform.position);
            localPoss.Add(points[i].transform.localPosition);
        }
        if (isCollider)
        {
            polygonCollider2D.SetPath(1, localPoss.ToArray());
            localPoss.Add(localPoss[0]);
            edgeCollider2D.SetPoints(localPoss);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            List<Vector2> rectangleVertices = new List<Vector2>
        {
            new Vector2(-1, 1),
            // Top-left
            new Vector2(1, 1),
            // Top-right
            new Vector2(-1, -1),
            // Bottom-left
            new Vector2(1, -1)
            // Bottom-right
        };

            AnimateToRectangle(points, 1, rectangleVertices);
        }
    }

    private List<Transform> Draw(List<Transform> pointList, float drawRadius)
    {
        int sides = pointList.Count;
        // Determine the number of edges based on the number of points.
        List<Transform> drawnPoints = new List<Transform>();

        for (int i = 0; i < sides; i++)
        {
            float angle = 2 * Mathf.PI * i / sides;
            Vector3 pos = new Vector3(Mathf.Cos(angle) * drawRadius, Mathf.Sin(angle) * drawRadius, 0);

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
            DebugLogger.Log("The number of points and vertices must be the same.",DebugLogger.Type.war);
            return;
        }

        // Calculate the new positions for the points
        Vector2[] newPositions = new Vector2[pointList.Count];
        for (int i = 0; i < pointList.Count; i++)
        {
            Vector2 offset = vertices[i] - (Vector2)pointList[i].position;
            newPositions[i] = vertices[i] - offset.normalized * offset.magnitude;
        }

        // Apply the animations using DOTween
        for (int i = 0; i < pointList.Count; i++)
        {
            pointList[i].DOMove(new Vector3(newPositions[i].x, newPositions[i].y, 0), duration);
        }
    }
}
