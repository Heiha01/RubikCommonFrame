using System;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public static class FilePathTool
{
    public static string GetFileNameWithoutExtension(this string tempPath)
    {
        if (tempPath.IsNull())
        {
            Debug.LogError("path connot be empty");
            return "";
        }
        return Path.GetFileNameWithoutExtension(tempPath);
    }

    public static string GetFileName(this string tempPath)
    {
        if (tempPath.IsNull())
        {
            Debug.LogError("path connot be empty");
            return "";
        }
        return Path.GetFileName(tempPath);
    }

    public static string GetFileExtension(this string tempPath)
    {
        if (tempPath.IsNull())
        {
            Debug.LogError("path connot be empty");
            return "";
        }
        return Path.GetExtension(tempPath);
    }

    /// <summary>
    /// ��ȡ�ļ���·��
    /// </summary>
    /// <param name="tempPath"></param>
    /// <returns></returns>
    public static string GetFileOfFolderPath(this string tempPath)
    {
        if (tempPath.IsNull())
        {
            Debug.LogError("path connot be empty");
            return "";
        }
        return Path.GetDirectoryName(tempPath);
    }

    /// <summary>
    /// Determine whether the folder contains a file with the specified name
    /// </summary>
    /// <param name="folderPath">�ļ��е�ַ</param>
    /// <param name="fileName">�ļ�����</param>
    /// <returns></returns>
    public static bool FileExistsInFolder(this string folderPath, string fileName)
    {
        // ��ȡ�ļ����е������ļ�
        string[] files = Directory.GetFiles(folderPath);

        // ����ļ����Ƿ����ļ��б���
        foreach (string file in files)
        {
            if (Path.GetFileName(file) == fileName)
            {
                return true;
            }
        }

        return false;
    }

    public static string[] GetFile(this string tempFilePath)
    {
        //�ж��ļ���·���Ƿ����
        if (!Directory.Exists(tempFilePath))
        {
            //����
            Directory.CreateDirectory(tempFilePath);
        }
        return Directory.GetFiles(tempFilePath);
    }

#if UNITY_EDITOR
    public static string GetObjectPath(this UnityEngine.Object @object)
    {
        return AssetDatabase.GetAssetPath(@object);
    }
#endif
}


public static partial class CommonTool
{
    public static bool IsNull(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNull<T>(this T ob)where T : Component
    {
        return ob.IsUnityNull();
    }

    public static string GetSuffixStr(this SuffixEnum @suffixEnum)
    {
        return $".{Enum.GetName(typeof(SuffixEnum), @suffixEnum)}";
    }
}

public enum SuffixEnum
{
    json,
    txt,
    xlsx,
    cs,
}