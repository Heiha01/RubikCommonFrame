using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
//using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class LoadJsonAsync : MonoBehaviour 
{
    private Action<string> _onJsonLoadComplete;

    public LoadJsonAsync LoadJsonAsyncFun(string jsonFileName, Action<string> onJsonLoadComplete)
    {
        _onJsonLoadComplete = onJsonLoadComplete;
        StartCoroutine(LoadJson(jsonFileName));
        return this;
    }

    private IEnumerator LoadJson(string jsonFileName)
    {
        var jsonPath = Path.Combine(Application.streamingAssetsPath, "JsonFile", jsonFileName);
#if UNITY_ANDROID || UNITY_EDITOR_WIN
        UnityWebRequest request = UnityWebRequest.Get(jsonPath);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Error loading JSON file: " + jsonFileName + request.error);
        }
        else
        {
            var jsonData = request.downloadHandler.text;
            _onJsonLoadComplete?.Invoke(jsonData);
            _onJsonLoadComplete = null;
            Destroy(this);
        }
#else
            try
            {
                Task<string> jsonData = System.IO.File.ReadAllTextAsync(jsonPath);
                _onJsonLoadComplete?.Invoke(jsonData.Result);
                _onJsonLoadComplete = null;
            }
            catch (Exception e)
            {
                Debug.LogError("Error loading JSON file: " + e.Message);
            }
            yield return null;
#endif

    }
}

