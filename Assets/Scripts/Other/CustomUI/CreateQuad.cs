using UnityEngine;
using UnityEngine.UI;
using ShortcutExtension;

namespace CustomUI
{
    class CreateQuad : MonoBehaviour
    {
        [SerializeField] private Material material = null;
        [SerializeField] private Texture mainTex = null;
        private void Start()
        {
            VertexHelper vh = new VertexHelper();
            vh.Clear();
            vh.AddVert(new Vector3(0, 0, 0), Color.red, new Vector2(0, 0));
            vh.AddVert(new Vector3(1, 0, 0), Color.green, new Vector2(1, 0));
            vh.AddVert(new Vector3(1, 1, 0), Color.yellow, new Vector2(1, 1));
            vh.AddVert(new Vector3(0, 1, 0), Color.cyan, new Vector2(0, 1));

            vh.AddTriangle(0, 2, 1);
            vh.AddTriangle(0, 3, 2);

            MeshFilter meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.name = "Quad";
            vh.FillMesh(mesh);
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            // 设置主贴图
            meshRenderer.material.mainTexture = mainTex;
        }
    }
}