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

            // �t�H���_�����݂��Ȃ��ꍇ�ɂ̂ݍ쐬
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parent, folderName);
            }

            return folderPath;
        }
    }

    //���O�����p�̕����N���X
    public static partial class AssetUtility
    {
        private const string _targetFileName = "AssetUtility.cs";
        private const string _logFileName = "_AssetGenerationLog.txt";

        // ���O�t�@�C���̃p�X�𓮓I�Ɏ擾���郁�\�b�h
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
                // �f�t�H���g�̃p�X�Ƀt�H�[���o�b�N
                return Path.Combine("Assets", _logFileName);
            }
        }

        // �X�N���v�g�t�@�C���̃p�X���������郁�\�b�h
        private static string FindScriptFilePath(string scriptFileName)
        {
            // 't:Script' �t�B���^���g�p���Ă��ׂẴX�N���v�g�t�@�C��������
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


        // ���O�t�@�C���ɃA�Z�b�g�p�X����������
        public static void WriteToLog(string path)
        {
            string logFilePath = GetLogFilePath();
            File.AppendAllText(logFilePath, path + System.Environment.NewLine);
        }

        // ���O�t�@�C������A�Z�b�g�p�X���폜����
        public static void DeleteLog(string path)
        {
            string logFilePath = GetLogFilePath();
            if (!File.Exists(logFilePath)) return;

            var lines = new List<string>(File.ReadAllLines(logFilePath));
            lines.RemoveAll(line => line.Equals(path, System.StringComparison.OrdinalIgnoreCase));

            File.WriteAllLines(logFilePath, lines);
        }

        // ���O�t�@�C���ɃA�Z�b�g�p�X�����݂��邩�m�F����
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