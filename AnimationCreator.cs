using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.IO;
using UnityEditor.Animations;
using Unity.EditorCoroutines.Editor;

namespace MantenseiLib.Editor
{
    public static class AnimationCreator
    {
        [MenuItem("Assets/Create/Mantensei/Animation/Create Animation")]
        static void CreateAnimation()
        {
            foreach(var texture in Selection.GetFiltered<Texture2D>(SelectionMode.Assets))
            {
                if(AssetUtility.TryFindObjectInParentFolders(texture, out ScriptableAnimationImportData importData))
                {
                    CreateAnimation(importData.GetImportData(texture));
                }
                else
                {
                    Debug.Log("ScriptableAnimationImportDataを生成してください。");
                }
            }
        }

        //AssetPostProcessorから呼び出すときは、アセットの更新待ちしなければいけないのでこちらを呼び出す
        public static void StartDelayedCreateAnimation(AnimationImportData animData)
        {
            IEnumerator DelayedCreateAnimation(AnimationImportData animData)
            {
                animData.UpdateTextureData();

                // Textureの更新が完了するまで待機
                while (true)
                {
                    animData.UpdateTextureData();
                    if (animData.Texture as UnityEngine.Object != null)
                        if (animData.Column * animData.Row == SpriteUtility.GetSprites(animData.Texture).Length)
                            break;

                    yield return new WaitForSeconds(1f);
                }

                CreateAnimation(animData);
            }

            EditorCoroutineUtility.StartCoroutineOwnerless(DelayedCreateAnimation(animData));
        }

        public static void CreateAnimation(AnimationImportData animData)
        {
            var texture = animData.Texture;
            var folder = GetAnimationFolder(texture);
            var header = GetHeaderName(texture);

            var controller = CreateAnimatorController(folder, header);
            var clips = CreateAnimationClips(animData, folder);

            //AssetPostProcessorの自動生成だと、なぜかセットしたクリップが剝がれるので遅延実行させる
            IEnumerator SetClips()
            {
                //不細工だけど1f待機させる
                //根拠のある数字じゃないので不具合が起きるかも
                yield return new WaitForSeconds(1f);
                foreach (var clip in clips)
                {
                    var state = controller.layers[0].stateMachine.AddState(clip.name);
                    state.motion = clip;
                }
            }
            EditorCoroutineUtility.StartCoroutineOwnerless(SetClips());
        }

        static AnimationClip[] CreateAnimationClips(AnimationImportData animData, string folderPath)
        {
            List<AnimationClip> clipList = new List<AnimationClip>();
            var datas = animData.ClipDataList;

            for (int y = 0; y < datas.Length; y++)
            {
                clipList.Add(CreateAnimationClip(animData, folderPath, y));
            }

            return clipList.ToArray();
        }

        static AnimationClip CreateAnimationClip(AnimationImportData animData, string folderPath, int clipIndex)
        {
            var clip = new AnimationClip();
            var curveBinding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            int totalFrames = animData.Column;
            float frameRate = animData.FrameRate;
            float frameTime = 1f / frameRate;
            var keyFrames = new ObjectReferenceKeyframe[totalFrames + 1]; // 1フレーム余分に足す
            var clipInfo = animData.ClipDataList[clipIndex];

            for (int i = 0; i < totalFrames; i++)
            {
                var sprite = SpriteUtility.GetSpriteByIndex(animData.Texture, clipIndex * totalFrames + i);

                keyFrames[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameTime,
                    value = sprite
                };
            }

            // 最終フレームにキーを追加（これを追加しないと最終フレームが一瞬で終わる）
            keyFrames[totalFrames] = new ObjectReferenceKeyframe
            {
                time = totalFrames * frameTime,
                value = keyFrames[totalFrames - 1].value
            };


            AnimationUtility.SetObjectReferenceCurve(clip, curveBinding, keyFrames);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = clipInfo.loop;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            var path = Path.Combine(folderPath, $"{GetHeaderName(animData.Texture)}_{clipInfo.name}.anim");
            AssetDatabase.CreateAsset(clip, path);

            return clip;
        }

        static AnimatorController CreateAnimatorController(string folder, string header)
        {
            var path = Path.Combine(folder, $"{header}_controller.controller");
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);

            return controller ??= AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        static string GetAnimationFolder(Texture2D texture)
        {
            var header = GetHeaderName(texture) + "_anim";
            return AssetUtility.CreateFolder(texture, header);
        }

        static string GetHeaderName(UnityEngine.Object @object) => GetHeaderName(AssetDatabase.GetAssetPath(@object));
        static string GetHeaderName(string path)
        {
            var fileName = Path.GetFileNameWithoutExtension(path);
            var head = fileName.Split(new[] { ' ', '_', '.' }, StringSplitOptions.RemoveEmptyEntries).First();
            return head;
        }
    }
}