using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Obsolete
{
    public class FramePlotterScript : MonoBehaviour
    {
        public Vector3[] points;
        public Color[] colors;
        [FormerlySerializedAs("UVs")] public Vector4[] uVs;

        [SerializeField] private Material material;
        [SerializeField] private Texture mainTex;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private readonly VertexHelper _vh = new();

        private void Start()
        {
            _vh.Clear();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _meshRenderer.material = material;
            _meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
            _meshRenderer.receiveShadows = false;

            _meshRenderer.material.mainTexture = mainTex;

            var mesh = new Mesh();
            mesh.name = "Quad";
            _vh.FillMesh(mesh);
            _meshFilter.mesh = mesh;
        }

        private void Update()
        {
            _vh.Clear();
            for (var i = 0; i < points.Length; i++) _vh.AddVert(points[i], colors[0], uVs[i]);
            _vh.AddTriangle(0, 2, 1);
            _vh.AddTriangle(0, 3, 2);
        }
    }
}