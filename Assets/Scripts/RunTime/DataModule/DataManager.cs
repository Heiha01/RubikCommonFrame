using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilsModule;

public class DataManager : ManagerBase<DataManager>
{
    public PlayerResourceDataCtrl PlayerResourceDataCtrl { get; private set; }

    public GameExcleInfoCtr GameExcleInfoCtr { get; private set; }
    public bool LoadCompleted { get; private set; }
    public override void Init()
    {
        LoadCompleted = false;
        StartCoroutine(InitCtr());
    }

    private IEnumerator InitCtr()
    {
        PlayerResourceDataCtrl = new PlayerResourceDataCtrl();
        PlayerResourceDataCtrl.Init();
        GameExcleInfoCtr = new GameExcleInfoCtr();
        while (!GameExcleInfoCtr.LoadCompleted)
        {
            yield return new WaitForSeconds(0.1f);
        }
        LoadCompleted = true;
    }
}
