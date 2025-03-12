using UnityEngine;

namespace UCT
{
    public class PolygonMask : MonoBehaviour
    {
        private static readonly int VerticesTex = Shader.PropertyToID("_VerticesTex");
        private static readonly int VertexCount = Shader.PropertyToID("_VertexCount");
        public Vector2[] polygonVertices;
        private Material _material;

        private void Start()
        {
            _material = GetComponent<SpriteRenderer>().material;
            SetMaterial();
        }

        private void Update()
        {
            SetMaterial();
        }

        private void SetMaterial()
        {
            var vertexTexture = new Texture2D(polygonVertices.Length, 1, TextureFormat.RGFloat, false)
            {
                filterMode = FilterMode.Point 
            };
            var colors = new Color[polygonVertices.Length];

            for (var i = 0; i < polygonVertices.Length; i++)
            {
                colors[i] = new Color(polygonVertices[i].x, polygonVertices[i].y, 0f, 1f);
            }

            vertexTexture.SetPixels(colors);
            vertexTexture.Apply();

            _material.SetTexture(VerticesTex, vertexTexture);
            _material.SetInt(VertexCount, polygonVertices.Length);
        }
    }
}