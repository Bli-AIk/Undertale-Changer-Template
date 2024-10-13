using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TextureFinder : MonoBehaviour
{
    [MenuItem("Tools/UCT/查询该场景内Texture使用情况")]
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
            Debug.Log($"该场景内使用了<b> {entry.Value[0].usageType}</b>: <b>{entry.Key.name}</b>", entry.Key, "#FFFF00");
            foreach (var (obj, usageType) in entry.Value)
            {
                Debug.Log($"  - 使用该 <b>{usageType}</b> 的对象: <b>{obj.name}</b>)", obj);
            }
        }
    }
}
