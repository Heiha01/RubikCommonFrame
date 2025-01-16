using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class DataStorageBase 
{
    private string _path;

    protected virtual string FileName => "DataStorageBase"; //文件名，子类重写

    public static string BasePath
    {
        get
        {
#if UNITY_EDITOR
            var folderPath = Application.dataPath.Replace("Assets", "PlayerData");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            return folderPath + "/";

#else
                return Application.persistentDataPath + "/";
#endif
        }
    }

    private string Path
    {
        get
        {
            if (_path == null)
            {
#if UNITY_EDITOR
                var folderPath = Application.dataPath.Replace("Assets", "PlayerData");
                _path =$"{folderPath}/{FileName}.json";
#else
                    _path = $"{Application.persistentDataPath}/{FileName}.json";
#endif
                CreateDirectory(_path);
            }

            return _path;
        }
    }

    private static void CreateDirectory(string targetDir)
    {
        var directories = targetDir.Split('/');

        var currentDir = string.Empty;

        foreach (var dir in directories)
        {
            if (dir.Contains(".json"))
            {
                break;
            }
            currentDir += dir + '/';

            if (!Directory.Exists(currentDir))
            {
                Directory.CreateDirectory(currentDir);
            }
        }
    }

    protected T ToLoad<T>()
    {
        //if (!File.Exists(Path)) return default;

        //try
        //{
        //    using var reader = new StreamReader(Path);
        //    using JsonReader jsonReader = new JsonTextReader(reader);
        //    var serializer = new JsonSerializer();
        //    var data = serializer.Serialize<T>(jsonReader);
        //    return data;
        //}
        //catch (Exception e)
        //{
        //    Debug.LogError($"ToLoadAsync: {Path} : {e.Message}");
        //    return default;
        //}
        return default(T);
    }


}
