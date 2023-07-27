using UnityEngine;
using UnityEngine.UI;
using ShortcutExtension;
using System.Collections;

namespace CustomUI
{
    class CreateCircular : MonoBehaviour
    {
        [SerializeField] private int radius = 1;                        // 半径
        [SerializeField] private int segment = 30;                      // 分段数
        [SerializeField] private Material material = null;
        [SerializeField] private Texture mainTex = null;

        private WaitForSeconds waitTime = new WaitForSeconds(2f);
        private void Start()
        {
            VertexHelper vh = new VertexHelper();
            vh.Clear();

            vh.AddVert(Vector3.zero, Color.white, Vector2.zero);
            float deltaAngle = 2 * Mathf.PI / segment;
            for (int i = 0; i < segment; i++)
            {
                float seta = i * deltaAngle;
                float x = radius * Mathf.Sin(seta);
                float y = radius * Mathf.Cos(seta);
                vh.AddVert(new Vector3(x, y, 0), Color.white, new Vector2(x, y));
            }

            for (int i = 0; i < segment-1; i++)
            {
                vh.AddTriangle(0, i+1, i+2);
            }
            vh.AddTriangle(0, segment, 1);

            MeshFilter meshFilter = gameObject.GetOrAddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            mesh.name = "Circular";
            vh.FillMesh(mesh);
            meshFilter.mesh = mesh;

            MeshRenderer meshRenderer = gameObject.GetOrAddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
            // 设置主贴图
            meshRenderer.material.mainTexture = mainTex;

            StartCoroutine(ChangeColor(mesh));
        }

        private IEnumerator ChangeColor(Mesh mesh)
        {
            int vertexCnt = mesh.colors.Length;
            int cnt = 0;
            int part = 5;                           // 把整个圆分成6个大块

            Color[] vertexColors = new Color[vertexCnt];
            vertexColors[0] = Color.white;
            while (true)
            {
                yield return waitTime;

                for (int i = 1; i < vertexCnt; i++)
                {
                    int partIndex = (i - 1) / part;
                    if (cnt == 0)
                    {
                        switch (partIndex)
                        {
                            case 0:
                                vertexColors[i] = Color.blue;
                                break;
                            case 1:
                                vertexColors[i] = Color.cyan;
                                break;
                            case 2:
                                vertexColors[i] = Color.green;
                                break;
                            case 3:
                                vertexColors[i] = Color.magenta;
                                break;
                            case 4:
                                vertexColors[i] = Color.red;
                                break;
                            case 5:
                                vertexColors[i] = Color.yellow;
                                break;
                            default:
                                break;
                        }
                    }
                    else if(cnt == 1)
                    {
                        switch (partIndex)
                        {
                            case 0:
                                vertexColors[i] = Color.cyan;
                                break;
                            case 1:
                                vertexColors[i] = Color.green;
                                break;
                            case 2:
                                vertexColors[i] = Color.magenta;
                                break;
                            case 3:
                                vertexColors[i] = Color.red;
                                break;
                            case 4:
                                vertexColors[i] = Color.yellow;
                                break;
                            case 5:
                                vertexColors[i] = Color.blue; 
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch (partIndex)
                        {
                            case 0:
                                vertexColors[i] = Color.green;
                                break;
                            case 1:
                                vertexColors[i] = Color.magenta;
                                break;
                            case 2:
                                vertexColors[i] = Color.red;
                                break;
                            case 3:
                                vertexColors[i] = Color.yellow;
                                break;
                            case 4:
                                vertexColors[i] = Color.blue;
                                break;
                            case 5:
                                vertexColors[i] = Color.cyan; 
                                break;
                            default:
                                break;
                        }
                    }

                }
                mesh.colors = vertexColors;

                cnt++;
                if (cnt > 2)
                {
                    cnt = 0;
                }
            }
        }
    }
}