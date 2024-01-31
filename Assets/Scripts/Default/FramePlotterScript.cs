using UnityEngine;
using UnityEngine.UI;

public class FramePlotterScript : MonoBehaviour
{
    public Vector3[] points;
    public Color[] colors;
    public Vector4[] UVs;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private VertexHelper vh = new VertexHelper();

    [SerializeField] private Material material = null;
    [SerializeField] private Texture mainTex = null;

    private void Start()
    {
        vh.Clear();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        meshRenderer.material = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        meshRenderer.material.mainTexture = mainTex;

        Mesh mesh = new Mesh();
        mesh.name = "Quad";
        vh.FillMesh(mesh);
        meshFilter.mesh = mesh;
    }

    private void Update()
    {
        vh.Clear();
        for (int i = 0; i < points.Length; i++)
        {
            vh.AddVert(points[i], colors[0], UVs[i]);
        }
        vh.AddTriangle(0, 2, 1);
        vh.AddTriangle(0, 3, 2);
    }
}