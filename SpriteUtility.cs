using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MantenseiLib.Editor
{
    public static class SpriteUtility
    {
        public static Sprite[] GetSpritesByIndex(Texture2D texture, params int[] index)
            => index.Select(x => GetSpriteByIndex(texture, x)).ToArray();

        public static Sprite GetSpriteByIndex(Texture2D texture, int index)
        {
            return GetSprites(texture)[index];
        }

        public static Sprite[] GetSprites(Texture2D texture)
        {
            string assetPath = AssetDatabase.GetAssetPath(texture);
            var assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            return assets.OfType<Sprite>().ToArray();
        }
    } 
}
