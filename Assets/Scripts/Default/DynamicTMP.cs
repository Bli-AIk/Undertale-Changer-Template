using TMPro;
using UnityEngine;

/// <summary>
/// Add all sorts of weird distortions/displacements/jiggles to the font blah blah blah!
/// </summary>
public class DynamicTMP : MonoBehaviour
{
    private TMP_Text tmp;
    public OverworldControl.DynamicTMP dynamicMode;
    private float randomStart;

    private void Start()
    {
        tmp = GetComponent<TMP_Text>();
        randomStart = Random.Range(2, 2.5f);
    }

    private void FixedUpdate()
    {
        if (dynamicMode == OverworldControl.DynamicTMP.None) return;

        tmp.ForceMeshUpdate();

        var textInfo = tmp.textInfo;

        switch (dynamicMode)
        {
            case OverworldControl.DynamicTMP.RandomShake:
            // Parkinson's, but each jitter is different
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    Vector3 random = new Vector3(Random.Range(-0.025f, 0.025f), Random.Range(-0.025f, 0.025f), 0);
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        //Animation
                        verts[charInfo.vertexIndex + j] = orig + random;
                    }
                }
                break;

            case OverworldControl.DynamicTMP.RandomShakeSingle:
            //Similar to the original battle's dialog jitter: characters randomly jitter at random times.

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
                            //Animation
                            verts[charInfo.vertexIndex + i] = orig + random;
                        }
                    }
                }
                break;

            case OverworldControl.DynamicTMP.RandomShakeAll:
            // Neatly jittery
                Vector3 randomer = new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;

                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        //Animation
                        verts[charInfo.vertexIndex + j] = orig + randomer;
                    }
                }
                break;

            case OverworldControl.DynamicTMP.CrazyShake:
            //Twitchy jitteriness
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        //Animation
                        verts[charInfo.vertexIndex + j] = orig + new Vector3(0, 0.025f * Mathf.Sin(Random.Range(1, 2.5f) * Time.time + orig.x * 0.45f), 0);

                        orig = verts[charInfo.vertexIndex + j];
                        if (Random.Range(0, 5) != 0)
                            continue;
                        verts[charInfo.vertexIndex + j] = orig + new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
                    }
                }
                break;

            case OverworldControl.DynamicTMP.NapShake:
            // little spooky twitchy jitters
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        //Animation
                        verts[charInfo.vertexIndex + j] = orig + new Vector3(0, 0.05f * Mathf.Cos(randomStart * (Time.time) + orig.x * 0.45f), 0);
                        orig = verts[charInfo.vertexIndex + j];
                        verts[charInfo.vertexIndex + j] = orig + new Vector3(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f), 0);
                    }
                }
                break;

            case OverworldControl.DynamicTMP.NapFloat:
            // Small ghost character floating
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        //Animation
                        verts[charInfo.vertexIndex + j] = orig + new Vector3(0, 0.05f * Mathf.Sin(randomStart * (Time.time) + orig.x * 0.45f), 0);
                    }
                }
                break;

            case OverworldControl.DynamicTMP.Wave:
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    Vector3 waveOffset = new Vector3(0, Mathf.Sin(Time.time * 2f + i * 0.2f) * 0.05f, 0);
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        verts[charInfo.vertexIndex + j] = orig + waveOffset;
                    }
                }
                break;

            case OverworldControl.DynamicTMP.Explode:
                Vector3 center = new Vector3(0, 0, 0);
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    Vector3 explodeOffset = (verts[charInfo.vertexIndex] - center) * Mathf.Sin(Time.time * 2f) * 0.1f;
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        verts[charInfo.vertexIndex + j] = orig + explodeOffset;
                    }
                }
                break;

            case OverworldControl.DynamicTMP.Bounce:
                for (int i = 0; i < textInfo.characterCount; i++)
                {
                    var charInfo = textInfo.characterInfo[i];
                    if (!charInfo.isVisible) continue;

                    var verts = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                    Vector3 bounceOffset = new Vector3(0, Mathf.Abs(Mathf.Sin(Time.time * 2f + i * 0.1f)) * 0.1f, 0);
                    for (int j = 0; j < 4; j++)
                    {
                        var orig = verts[charInfo.vertexIndex + j];
                        verts[charInfo.vertexIndex + j] = orig + bounceOffset;
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
