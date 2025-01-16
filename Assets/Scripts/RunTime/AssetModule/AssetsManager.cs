using System.Collections;
using UnityEditor;
using UnityEngine;
using UtilsModule;

public class AssetsManager : ManagerBase<AssetsManager>
{
    private AssetBundle _loadingBundle;
    private AssetBundle _mainBundle;
    public float BundleLoadingProgress { get; private set; }
    public bool LoadCompleted { get; private set; }

    public override void Init()
    {
        LoadCompleted = false;
        StartCoroutine(LoadMainAssetsBundle());
        PoolManager.Instance.Init();
    }

    private IEnumerator LoadMainAssetsBundle()
    {
        string loadingBundlePath =
#if UNITY_EDITOR
            Application.dataPath + "/StreamingAssets/AssetsBundles/loadingbundle";
#elif UNITY_IOS || UNITY_IPHONE
                Application.streamingAssetsPath+ "/AssetsBundles/loadingbundle";
#elif UNITY_ANDROID
                "jar:file://"  + Application.dataPath + "!/assets/AssetsBundles/loadingbundle";
#endif
        string mainBundlePath =
#if UNITY_EDITOR
            Application.dataPath + "/StreamingAssets/AssetsBundles/mainbundle";
#elif UNITY_IOS || UNITY_IPHONE
                Application.streamingAssetsPath+ "/AssetsBundles/mainbundle";
#elif UNITY_ANDROID
                "jar:file://"  + Application.dataPath + "!/assets/AssetsBundles/mainbundle";
#endif

        _loadingBundle = AssetBundle.LoadFromFile(loadingBundlePath);
        yield return _loadingBundle;
        BundleLoadingProgress = 0.01f;


        AssetBundleCreateRequest abRequest = AssetBundle.LoadFromFileAsync(mainBundlePath);
        // 异步等待AB包加载完成
        while (!abRequest.isDone)
        {
            BundleLoadingProgress = abRequest.progress;
            yield return null;
        }

        // 获取AB包
        _mainBundle = abRequest.assetBundle;
        BundleLoadingProgress = 1;
        LoadCompleted = true;
    }

    /// <summary>
    ///  从AB包中加载资源并返回
    /// </summary>
    /// <param name="assetPath"></param>
    /// <returns></returns>
    public T LoadAssetImmediate<T>(string assetPath) where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
        return asset;
#else
               T asset = null;
            if(assetPath.Contains(StrConstantsContainer.BundlesPathName.LoadingBundle))
                asset = _loadingBundle.LoadAsset<T>(assetPath);
            else if(assetPath.Contains(StrConstantsContainer.BundlesPathName.MainBundle))
                asset = _mainBundle.LoadAsset<T>(assetPath);
            return asset;
#endif
    }
}
