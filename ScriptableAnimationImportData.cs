using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Serialization;

namespace MantenseiLib.Editor
{
    [CreateAssetMenu(menuName = "Mantensei/Animation/ScriptableObject/AnimationImportData")]
    public class ScriptableAnimationImportData : ScriptableObject
    {
        public bool AutoGenerateAnimation = false;

        [SerializeField] private List<AnimationClipInfo> clipDataList = new List<AnimationClipInfo>();
        [SerializeField] private int pixelsPerUnit = 32;
        public int PixelsPerUnit => pixelsPerUnit;

        [SerializeField] private int spriteWidth = 32;
        [SerializeField] private int frameRate = 30;


        public AnimationImportData GetImportData(Texture2D texture) => GetImportData(texture, AssetDatabase.GetAssetPath(texture));
        public AnimationImportData GetImportData(Texture2D texture, string path)
        {
            return new AnimationImportData
            (
                texture,
                path,
                clipDataList.ToArray(),
                spriteWidth,
                frameRate
            );
        }
    }

    public class AnimationImportData
    {
        public Texture2D Texture { get; private set; }
        public int SpriteWidth { get; }
        public int FrameRate { get; }
        public int SpriteHeight { get; }
        public int Column { get; }
        public int Row { get; }

        public string Path { get; }
        public AnimationClipInfo[] ClipDataList { get; }

        public void UpdateTextureData()
        {
            Texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Path);
        }

        public AnimationImportData(Texture2D texture, string path, AnimationClipInfo[] clipData, int spriteWidth, int frameRate)
        {
            if (texture == null)
            {
                throw new System.ArgumentNullException(nameof(texture));
            }

            if (clipData == null || clipData.Length == 0)
            {
                throw new System.ArgumentException("clipDatas cannot be null or empty", nameof(clipData));
            }

            if (spriteWidth <= 0)
            {
                throw new System.ArgumentOutOfRangeException(nameof(spriteWidth), "width must be greater than 0");
            }

            this.Texture = texture;
            this.ClipDataList = clipData;
            this.SpriteWidth = spriteWidth;
            this.SpriteHeight = texture.height / clipData.Length;
            this.Row = clipData.Length;
            this.Column = texture.width / spriteWidth; 
            this.FrameRate = frameRate;

            Path = path;
        }
    }

    [System.Serializable]
    public class AnimationClipInfo
    {
        public string name;
        public bool loop = true;
    }
}
