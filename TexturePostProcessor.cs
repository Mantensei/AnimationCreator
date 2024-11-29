using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MantenseiLib.Editor
{
    public class TexturePostProcessor : AssetPostprocessor
    {
        void OnPostprocessTexture(Texture2D texture)
        {
            if (AssetUtility.TryFindObjectInParentFolders(assetPath, out ScriptableAnimationImportData importData))
            {
                //�������[�v�h�~�̂��߁A���O�Ƀp�X���Ȃ����`�F�b�N
                if (AssetUtility.ExistInLog(assetPath))
                {
                    AssetUtility.DeleteLog(assetPath);
                }
                else
                {
                    //�������[�v�h�~�̂��߁A���O��������
                    AssetUtility.WriteToLog(assetPath);

                    var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                    TextureImporterConfigurator.ConfigureTexture(assetPath, importData.PixelsPerUnit);

                    var animData = importData.GetImportData(texture, assetPath);
                    TextureSlicer.Slice(texture, animData.SpriteWidth, animData.SpriteHeight, importer);

                    if (importData.AutoGenerateAnimation)
                    {
                        AnimationCreator.StartDelayedCreateAnimation(animData);
                    }
                }
            }
        }
    }
}
