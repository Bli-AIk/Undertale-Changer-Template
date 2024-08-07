using UnityEngine;
using UnityEditor;

public class SpriteImportSettings : AssetPostprocessor
{
    /*
    void OnPreprocessTexture()
    {
        TextureImporter importer = assetImporter as TextureImporter;

        if (importer != null && importer.textureType == TextureImporterType.Sprite)
        {
            // 设置Pixels Per Unit
            importer.spritePixelsPerUnit = 20; // 修改为你需要的值

            // 设置Filter Mode
            importer.filterMode = FilterMode.Point; // 或其他模式，例如FilterMode.Bilinear

            // 设置其他导入选项
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        }
    }
    */
}
