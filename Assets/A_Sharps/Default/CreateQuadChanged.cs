using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateQuadChanged : MonoBehaviour
{
    public List<Vector3> vertPoints = new List<Vector3>();
    public List<Vector2> vertUV = new List<Vector2>();
    public float noHavePointsFloat;
    public float width;
    public bool haveEdgeCollider2D;
    EdgeCollider2D edgeCollider2D;
    GameObject mask;
    public List<GameObject> points = new List<GameObject>();
    public Color color;
    [SerializeField] private Material material = null;
    [SerializeField] private Texture mainTex = null;
    VertexHelper vh = new VertexHelper();
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    LineRenderer lineRenderer;
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
            mask = transform.Find("Mask").gameObject;
            edgeCollider2D = GetComponent<EdgeCollider2D>();
        }
        ReStart();

    }
    private void Update()
    {
        ReStart();
    }
    void ReStart()
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
            points[4] = vertPoints[0];
            for (int i = 0; i < vertPoints.Count; i++)
            {
                points[i] = vertPoints[i];
            }
            edgeCollider2D.points = points;
            edgeCollider2D.edgeRadius = width / 2;
            //此mask仅能用于矩形 其余需要自备sprite
            mask.transform.localPosition = new Vector3(0.5f * (points[0].x + points[1].x), 0.5f * (points[0].y + points[3].y));
            mask.transform.localScale = new Vector3(points[0].x - points[1].x, points[0].y - points[3].y);
        }
    }
}
