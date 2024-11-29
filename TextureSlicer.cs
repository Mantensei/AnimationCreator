using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.U2D.Sprites;

namespace MantenseiLib.Editor
{
    public static class TextureSlicer
    {
        public static void Slice(Texture2D texture, int spriteWidth, int spriteHeight, TextureImporter importer = null)
        {
            var path = AssetDatabase.GetAssetPath(texture);
            importer ??= AssetImporter.GetAtPath(path) as TextureImporter;

            if(importer == null)
            {
                throw new System.NullReferenceException($"{path}のインポーター見つかりません。");
            }

            var textureWidth = texture.width;
            var textureHeight = texture.height;

            var hCount = textureWidth / spriteWidth;
            var vCount = textureHeight / spriteHeight;
            var metaDataArray = CreateSpriteMetaDataArray(texture, hCount, vCount).ToArray();

            SetSpriteMetaData(importer, metaDataArray);

            EditorApplication.delayCall += () =>
            {
                importer.SaveAndReimport();
            };
        }

        private static IEnumerable<SpriteMetaData> CreateSpriteMetaDataArray(Texture2D texture, int horizontalCount, int verticalCount, params string[] footerNames)
        {
            float spriteWidth = texture.width / horizontalCount;
            float spriteHeight = texture.height / verticalCount;

            bool anyFooterName = footerNames.Any();
            int count = 0;

            for (int y = 0; y < verticalCount; y++)
            {
                for (int x = 0; x < horizontalCount; x++)
                {
                    var index = anyFooterName ? x : count;
                    var footer = anyFooterName ? "_" + footerNames[y] : string.Empty;
                    count++;

                    yield return new SpriteMetaData()
                    {
                        name = $"{texture.name}{footer}_{index}",
                        rect = new Rect(spriteWidth * x, texture.height - spriteHeight * (y + 1), spriteWidth, spriteHeight)
                    };
                }
            }
        }

        private static void SetSpriteMetaData(TextureImporter importer, SpriteMetaData[] spriteMetaDataArray)
        {
            if (spriteMetaDataArray.Length == 0)
                return;

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            dataProvider.InitSpriteEditorDataProvider();
            var spriteRects = dataProvider.GetSpriteRects().ToList();

            spriteRects.Clear();
            foreach (var metaData in spriteMetaDataArray)
            {
                spriteRects.Add(new SpriteRect
                {
                    name = metaData.name,
                    spriteID = GUID.Generate(),
                    rect = metaData.rect
                });
            }

            dataProvider.SetSpriteRects(spriteRects.ToArray());
            dataProvider.Apply();
        }       

    }
}