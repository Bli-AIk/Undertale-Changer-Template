using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 旧的战斗框绘制脚本：已废弃
/// 新的：DrawFrame
/// </summary>

[Obsolete]
public class CreateQuadChanged : MonoBehaviour
{
    public static CreateQuadChanged instance;

    private void Awake()
    {
        instance = this;
    }

    //public float debug;
    public List<Vector3> vertPoints = new List<Vector3>();

    public List<Vector2> vertUV = new List<Vector2>();
    public List<Vector3> vertEdge = new List<Vector3>();

    //public bool havePoints;
    //public List<Vector3> noHavePoints = new List<Vector3>();
    public float noHavePointsFloat;

    public float width;
    public bool haveEdgeCollider2D;
    private EdgeCollider2D edgeCollider2D;
    public List<GameObject> points = new List<GameObject>();
    public Color color;
    [SerializeField] private Material material = null;
    [SerializeField] private Texture mainTex = null;
    private VertexHelper vh = new VertexHelper();
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private LineRenderer lineRenderer;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.positionCount = vertPoints.Count;
        for (int i = 0; i < vertPoints.Count; i++)
        {
            points.Add(transform.Find("Point" + i).gameObject);
        }
        if (haveEdgeCollider2D)
        {
            edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>() ?? gameObject.GetComponent<EdgeCollider2D>();
        }
        ReStart();
    }

    private void Update()
    {
        ReStart();
    }

    private void ReStart()
    {
        vh.Clear();
        for (int i = 0; i < vertPoints.Count; i++)
        {
            vertPoints[i] = points[i].transform.localPosition;
            lineRenderer.SetPosition(i, points[i].transform.position);
            vh.AddVert(vertPoints[i], color, vertUV[i]);
        }
        vh.AddTriangle(0, 2, 1);
        vh.AddTriangle(0, 3, 2);
        Mesh mesh = new Mesh();
        mesh.name = "Quad";
        vh.FillMesh(mesh);
        meshFilter.mesh = mesh;

        meshRenderer.material = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        // 设置主贴图
        meshRenderer.material.mainTexture = mainTex;

        if (haveEdgeCollider2D)
        {
            Vector2[] points = new Vector2[5];
            points[4] = vertPoints[0] + vertEdge[0];
            for (int i = 0; i < vertPoints.Count; i++)
            {
                points[i] = vertPoints[i] + vertEdge[i];
            }
            edgeCollider2D.points = points;
        }
    }
}