using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TextureFinder : MonoBehaviour
{
    [MenuItem("Tools/UCT/��ѯ�ó�����Textureʹ�����")]
    public static void FindAllTexturesInScene()
    {
        Dictionary<Texture, List<(GameObject obj, string usageType)>> textureUsageMap = new Dictionary<Texture, List<(GameObject, string)>>();

        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            foreach (Material mat in renderer.sharedMaterials)
            {
                if (mat != null)
                {
                    foreach (string property in mat.GetTexturePropertyNames())
                    {
                        Texture texture = mat.GetTexture(property);
                        if (texture != null)
                        {
                            if (!textureUsageMap.ContainsKey(texture))
                            {
                                textureUsageMap[texture] = new List<(GameObject, string)>();
                            }
                            textureUsageMap[texture].Add((renderer.gameObject, "Material"));
                        }
                    }
                }
            }
        }

        SpriteRenderer[] spriteRenderers = FindObjectsOfType<SpriteRenderer>();

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer.sprite != null && spriteRenderer.sprite.texture != null)
            {
                Texture texture = spriteRenderer.sprite.texture;
                if (!textureUsageMap.ContainsKey(texture))
                {
                    textureUsageMap[texture] = new List<(GameObject, string)>();
                }
                textureUsageMap[texture].Add((spriteRenderer.gameObject, "Sprite"));
            }
        }

        UnityEngine.UI.RawImage[] rawImages = FindObjectsOfType<UnityEngine.UI.RawImage>();

        foreach (var rawImage in rawImages)
        {
            if (rawImage.texture != null)
            {
                Texture texture = rawImage.texture;
                if (!textureUsageMap.ContainsKey(texture))
                {
                    textureUsageMap[texture] = new List<(GameObject, string)>();
                }
                textureUsageMap[texture].Add((rawImage.gameObject, "RawImage"));
            }
        }

        foreach (KeyValuePair<Texture, List<(GameObject obj, string usageType)>> entry in textureUsageMap)
        {
            Debug.LogWarning($"�ó�����ʹ����<b> {entry.Value[0].usageType}</b>: <b>{entry.Key.name}</b>", entry.Key);
            foreach (var (obj, usageType) in entry.Value)
            {
                Debug.Log($"  - ʹ�ø� <b>{usageType}</b> �Ķ���: <b>{obj.name}</b>)", obj);
            }
        }
    }
}
