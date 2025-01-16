using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerResourceDataCtrl:DataStorageBase
{
    private PlayerResourceData _playerResourceData;
    public PlayerResourceData PlayerResourceData=>_playerResourceData;

    public override void Init()
    {
        _playerResourceData = ToLoad<PlayerResourceData>();
        if (_playerResourceData.IsUnityNull())
        {
            _playerResourceData = new PlayerResourceData();
        }
    }

}

/// <summary>
/// ��ҵ���Դ�����磺���
/// </summary>
public class PlayerResourceData
{
    public GameNumBase<BigInteger> coin;
    public PlayerResourceData()
    {
        coin = new GameNumBase<BigInteger>(0);
    }
}
