using UnityEngine;
using TMPro;

public class DynamicTMP : MonoBehaviour
{
    TMP_Text tmp;
    public int mode;
    private void Start()
    {
        tmp = GetComponent<TextMeshPro>();
    }
    void FixedUpdate()
    {
        tmp.ForceMeshUpdate();

        var textInfo = tmp.textInfo;

        switch (mode)
        {
            case 0:
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    Vector3 random = new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f), 0);
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        //动画
                        verts[charInfo.vertexIndex + j] = orig + random;

                    }
                }
                break;
            case 1:

                int randomIs = Random.Range(0, 120);
                if (randomIs == 0)
                {
                    int j = Random.Range(0, textInfo.characterCount);
                    var charInfo = textInfo.characterInfo[j];
                    if (charInfo.isVisible)
                    {
                        var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                        Vector3 random = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
                        for (int i = 0; i < 4; i++)
                        {
                            var orig = verts[charInfo.vertexIndex + i];
                            //动画
                            verts[charInfo.vertexIndex + i] = orig + random;

                        }
                    }

                }
                break;
            case 2:
                Vector3 randomer = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        //动画
                        verts[charInfo.vertexIndex + j] = orig + randomer;

                    }
                }
                break;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            var meshInfo = textInfo.meshInfo[i];
            meshInfo.mesh.vertices = meshInfo.vertices;
            tmp.UpdateGeometry(meshInfo.mesh, i);
        }
    }
}
