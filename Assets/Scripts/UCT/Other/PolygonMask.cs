using System.Linq;
using UnityEngine;

namespace UCT
{
    [System.Serializable]
    public class Polygon
    {
        public Vector2[] vertices;
    }

    public class PolygonMask : MonoBehaviour
    {
        private static readonly int VerticesTex = Shader.PropertyToID("_VerticesTex");
        private Material _material;
        public Polygon[] polygons;

        private void Start()
        {
            _material = GetComponent<SpriteRenderer>().material;
            UpdateVertexTexture();
        }

        private void Update()
        {
            UpdateVertexTexture();
        }

        private void UpdateVertexTexture()
        {
            if (polygons.Length == 0)
            {
                return;
            }

            var textureWidth = polygons.Max(p => p.vertices.Length) + 1; 
            var textureHeight = polygons.Length; 

            var vertexTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            for (var y = 0; y < polygons.Length; y++)
            {
                var polygon = polygons[y];

                vertexTexture.SetPixel(0, y, new Color(polygon.vertices.Length, polygons.Length, 0f, 1f));

                for (var x = 0; x < polygon.vertices.Length; x++)
                {
                    vertexTexture.SetPixel(x + 1, y, new Color(polygon.vertices[x].x, polygon.vertices[x].y, 0f, 1f));
                }
            }

            vertexTexture.Apply(); 
            _material.SetTexture(VerticesTex, vertexTexture);
        }

    }
}