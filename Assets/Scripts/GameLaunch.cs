using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilsModule;

public class GameLaunch : ManagerBase<GameLaunch>
{
    public bool LoadCompletedAll { get; private set; }
    private void Awake()
    {
        Init();
    }
    public override void Init()
    {
        StartCoroutine(InitManager());
    }

    private IEnumerator InitManager()
    {
        var tempWait = new WaitForSeconds(0.1f);
        //var tempAssetsManager =  this.gameObject.AddComponent<AssetsManager>();
        //tempAssetsManager.Init();
        //while (!tempAssetsManager.LoadCompleted)
        //{
        //    yield return tempWait;
        //}
        var tempDataManager = this.gameObject.AddComponent<DataManager>();
        tempDataManager.Init();
        while (!tempDataManager.LoadCompleted)
        {
            yield return tempWait;
        }
        LoadCompletedAll = true;
    }

    [ContextMenu("33")]
    public void PP()
    {
        Debug.LogError(DataManager.Instance.PlayerResourceDataCtrl.PlayerResourceData.coin.Value);
    }
}
