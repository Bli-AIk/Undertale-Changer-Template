using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Obsolete
{
    /// <summary>
    /// 旧的战斗框绘制脚本：已废弃
    /// 新的：DrawFrame
    /// </summary>

    [Obsolete]
    public class CreateQuadChanged : MonoBehaviour
    {
        public static CreateQuadChanged Instance;

        private void Awake()
        {
            Instance = this;
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
        private EdgeCollider2D _edgeCollider2D;
        public List<GameObject> points = new List<GameObject>();
        public Color color;
        [SerializeField] private Material material;
        [SerializeField] private Texture mainTex;
        private VertexHelper _vh = new VertexHelper();
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private LineRenderer _lineRenderer;

        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _lineRenderer = GetComponent<LineRenderer>();
            _lineRenderer.loop = true;
            _lineRenderer.startWidth = width;
            _lineRenderer.endWidth = width;
            _lineRenderer.positionCount = vertPoints.Count;
            for (int i = 0; i < vertPoints.Count; i++)
            {
                points.Add(transform.Find("Point" + i).gameObject);
            }
            if (haveEdgeCollider2D)
            {
                _edgeCollider2D = gameObject.AddComponent<EdgeCollider2D>() ?? gameObject.GetComponent<EdgeCollider2D>();
            }
            ReStart();
        }

        private void Update()
        {
            ReStart();
        }

        private void ReStart()
        {
            _vh.Clear();
            for (int i = 0; i < vertPoints.Count; i++)
            {
                vertPoints[i] = points[i].transform.localPosition;
                _lineRenderer.SetPosition(i, points[i].transform.position);
                _vh.AddVert(vertPoints[i], color, vertUV[i]);
            }
            _vh.AddTriangle(0, 2, 1);
            _vh.AddTriangle(0, 3, 2);
            Mesh mesh = new Mesh();
            mesh.name = "Quad";
            _vh.FillMesh(mesh);
            _meshFilter.mesh = mesh;

            _meshRenderer.material = material;
            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;
            // 设置主贴图
            _meshRenderer.material.mainTexture = mainTex;

            if (haveEdgeCollider2D)
            {
                Vector2[] points = new Vector2[5];
                points[4] = vertPoints[0] + vertEdge[0];
                for (int i = 0; i < vertPoints.Count; i++)
                {
                    points[i] = vertPoints[i] + vertEdge[i];
                }
                _edgeCollider2D.points = points;
            }
        }
    }
}