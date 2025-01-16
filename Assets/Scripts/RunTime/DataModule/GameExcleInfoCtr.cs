using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;
using UnityEngine;
using System.IO;
public class GameExcleInfoCtr
{
    private static int _loadCompleteCount = 1; //当前加载的数量
    private static int _loadTargetCount = 0; //目标加载数量

    private string pathPre = "";

    public  bool LoadCompleted => _loadCompleteCount >= _loadTargetCount; //是否全部表格加载完成
    public float LoadingProgress => _loadCompleteCount / (float)_loadTargetCount; //加载进度
    private Dictionary<string, List<GameExcleInfoBase>> _gameExcleInfo;
    private GameObject _loadJsonOb;

    public GameExcleInfoCtr()
    {
        _loadJsonOb = new GameObject("LoadJsonAsync");
        _loadJsonOb.transform.SetParent(GameLaunch.Instance.transform);
        pathPre  = Path.Combine(Application.streamingAssetsPath, "JsonFile");

        _gameExcleInfo = new Dictionary<string, List<GameExcleInfoBase>>();
        LoadInfoFromService<Book1_Sheet1>("Book1", ".json");
        LoadInfoFromService<Book2_Sheet1>("Book2", ".json");
        DelayGC();
    }

    private void LoadInfoFromService<T>(string key, string suffix) where T : GameExcleInfoBase
    {
        var _loadJsonAsync = _loadJsonOb.AddComponent<LoadJsonAsync>();
        var infoName = key + suffix;
        _loadJsonAsync.LoadJsonAsyncFun(infoName,
            (tempJson) =>
            {
                var tempList = JsonConvert.DeserializeObject<List<T>>(tempJson);
                var tempBaseList = tempList.OfType<GameExcleInfoBase>().ToList();
                if (_gameExcleInfo.ContainsKey(key))
                {
                    _gameExcleInfo[key].Clear();
                    _gameExcleInfo[key] = tempBaseList;
                }
                else
                {
                    _gameExcleInfo.Add(key, tempBaseList);
                }
                _loadTargetCount++;
            }
            );
    }

    public List<GameExcleInfoBase> GetExcleInfo(string jsonName)
    {
        if (_gameExcleInfo.ContainsKey(jsonName))
        {
            return _gameExcleInfo[jsonName];
        }
        return null;
    }

    public T GetGameExcleInfoSingle<T>(string jsonName, string tempId) where T : GameExcleInfoBase
    {
        if (_gameExcleInfo.ContainsKey(jsonName))
        {
            List<GameExcleInfoBase> tempList =_gameExcleInfo[jsonName];
            foreach (var item in tempList)
            {
                if (item.id == tempId)
                {
                    return item as T;
                }
            }
        }
        return null;
    }

    private async void DelayGC()
    {
        await Task.Delay(10);
        if (LoadCompleted)
        {
            GC.Collect();
            Debug.Log("GC Commpleted");
        }
    }
}
