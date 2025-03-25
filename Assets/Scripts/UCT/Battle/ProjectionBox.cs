using UnityEngine;

namespace UCT.Battle
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class ProjectionBox : MonoBehaviour
    {
        public float scaleFactor = 4.0f;

        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            GenerateMesh();
            LoadMaterial();
        }

        private void OnValidate()
        {
            if (_meshFilter)
            {
                GenerateMesh();
            }
        }

        private void GenerateMesh()
        {
            var mesh = new Mesh();

            var width = 4f * scaleFactor * 3.5f * 0.95375f;
            var height = 3f * scaleFactor * 3.5f * 0.95375f;

            var vertices = new Vector3[]
            {
                new(-width / 2, -height / 2, 0),
                new(width / 2, -height / 2, 0),
                new(-width / 2, height / 2, 0),
                new(width / 2, height / 2, 0)
            };

            var triangles = new[]
            {
                0, 2, 1,
                2, 3, 1
            };

            var uv = new Vector2[]
            {
                new(0, 0),
                new(1, 0),
                new(0, 1),
                new(1, 1)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();

            _meshFilter.mesh = mesh;
        }

        private void LoadMaterial()
        {
            var mat = Resources.Load<Material>("Materials/ProjectionBox");
            if (mat)
            {
                _meshRenderer.material = mat;
            }
            else
            {
                Debug.LogWarning("Material not found in Resources.");
            }
        }
    }
}