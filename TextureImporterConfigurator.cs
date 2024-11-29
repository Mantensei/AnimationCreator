using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MantenseiLib.Editor
{
    public static class TextureImporterConfigurator
    {
        public static void ConfigureTexture(string path, int ppu)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;

            importer.isReadable = false;
            importer.mipmapEnabled = true;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = ppu;

            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            {
                overridden = true,
                name = importer.GetDefaultPlatformTextureSettings().name,
                maxTextureSize = 2048,
                format = TextureImporterFormat.RGBA32,
            });

            //無限ループ防止のため、メタタグを埋め込む
            //MetaFileUtility.EmbedMetaTag(path);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        }
    }

}