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
    /// 获取文件夹路径
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
    /// <param name="folderPath">文件夹地址</param>
    /// <param name="fileName">文件名字</param>
    /// <returns></returns>
    public static bool FileExistsInFolder(this string folderPath, string fileName)
    {
        // 获取文件夹中的所有文件
        string[] files = Directory.GetFiles(folderPath);

        // 检查文件名是否在文件列表中
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
        //判断文件夹路径是否存在
        if (!Directory.Exists(tempFilePath))
        {
            //创建
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