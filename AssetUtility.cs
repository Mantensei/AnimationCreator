using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace MantenseiLib.Editor
{
    public static partial class AssetUtility
    {
        public static bool TryFindObjectInParentFolders<T>(Object asset, out T foundObject) where T : Object
            => TryFindObjectInParentFolders(AssetDatabase.GetAssetPath(asset), out foundObject); 

        public static bool TryFindObjectInParentFolders<T>(string assetPath, out T foundObject) where T : Object
        {
            string currentPath = Path.GetDirectoryName(assetPath);

            while (!string.IsNullOrEmpty(currentPath))
            {
                string[] assets = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { currentPath });
                foreach (string assetGUID in assets)
                {
                    string assetPathFound = AssetDatabase.GUIDToAssetPath(assetGUID);
                    foundObject = AssetDatabase.LoadAssetAtPath<T>(assetPathFound);

                    if (foundObject != null)
                    {
                        return true;
                    }
                }

                currentPath = Path.GetDirectoryName(currentPath);
            }

            foundObject = null;
            return false;
        }

        public static string CreateFolder(Object asset, string folderName)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            var parent = Path.GetDirectoryName(path);
            string folderPath = Path.Combine(parent, folderName).Replace('\\', '/');

            // フォルダが存在しない場合にのみ作成
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }

            return folderPath;
        }
    }

    //ログ操作専用の分離クラス
    public static partial class AssetUtility
    {
        private const string _targetFileName = "AssetUtility.cs";
        private const string _logFileName = "_AssetGenerationLog.txt";

        // ログファイルのパスを動的に取得するメソッド
        private static string GetLogFilePath()
        {
            string scriptFilePath = FindScriptFilePath(_targetFileName);
            if (scriptFilePath != null)
            {
                string directoryPath = Path.GetDirectoryName(scriptFilePath);
                return Path.Combine(directoryPath, _logFileName);
            }
            else
            {
                // デフォルトのパスにフォールバック
                return Path.Combine("Assets", _logFileName);
            }
        }

        // スクリプトファイルのパスを検索するメソッド
        private static string FindScriptFilePath(string scriptFileName)
        {
            // 't:Script' フィルタを使用してすべてのスクリプトファイルを検索
            string[] guids = AssetDatabase.FindAssets("t:Script");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileName(path) == scriptFileName)
                {
                    return path;
                }
            }
            return null;
        }


        // ログファイルにアセットパスを書き込む
        public static void WriteToLog(string path)
        {
            string logFilePath = GetLogFilePath();
            File.AppendAllText(logFilePath, path + System.Environment.NewLine);
        }

        // ログファイルからアセットパスを削除する
        public static void DeleteLog(string path)
        {
            string logFilePath = GetLogFilePath();
            if (!File.Exists(logFilePath)) return;

            var lines = new List<string>(File.ReadAllLines(logFilePath));
            lines.RemoveAll(line => line.Equals(path, System.StringComparison.OrdinalIgnoreCase));

            File.WriteAllLines(logFilePath, lines);
        }

        // ログファイルにアセットパスが存在するか確認する
        public static bool ExistInLog(string path)
        {
            string logFilePath = GetLogFilePath();
            if (!File.Exists(logFilePath)) return false;

            var lines = File.ReadAllLines(logFilePath);
            foreach (var line in lines)
            {
                if (line.Equals(path, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}