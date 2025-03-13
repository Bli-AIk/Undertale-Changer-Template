using UnityEngine;

namespace UCT
{
    public class PolygonMask : MonoBehaviour
    {
        private static readonly int VerticesTex = Shader.PropertyToID("_VerticesTex");
        public Vector2[] polygonVertices;
        private Material _material;

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
            int textureWidth = polygonVertices.Length + 1;
            var vertexTexture = new Texture2D(textureWidth, 1, TextureFormat.RGFloat, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };

            Color[] colors = new Color[textureWidth];
            
            // 第一个像素存储顶点数量
            colors[0] = new Color(polygonVertices.Length, 0f, 0f, 1f);
            
            // 后续像素存储顶点坐标
            for (int i = 0; i < polygonVertices.Length; i++)
            {
                colors[i + 1] = new Color(polygonVertices[i].x, polygonVertices[i].y, 0f, 1f);
            }

            vertexTexture.SetPixels(colors);
            vertexTexture.Apply();
            _material.SetTexture(VerticesTex, vertexTexture);
        }
    }
}