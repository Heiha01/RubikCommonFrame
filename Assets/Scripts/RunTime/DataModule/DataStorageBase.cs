using System;
using System.IO;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

public class DataStorageBase 
{
    protected  string _saveKey;
    protected virtual string SaveKey
    {
        get
        {
            if (_saveKey.IsUnityNull())
            {
                _saveKey = this.GetType().Name;
            }
            return _saveKey;
        }
    }

    public virtual void Init()
    {
        _saveKey = this.GetType().Name;
    }


    protected T ToLoad<T>()
    {
        var tempStr = PlayerPrefs.GetString(SaveKey);
        if (tempStr.IsUnityNull()) { 
        return JsonConvert.DeserializeObject<T>(tempStr);
        }
        return default(T);
    }

    protected void SaveData()
    {
        PlayerPrefs.SetString(SaveKey, _saveKey);
    }

}
