using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class PlayerResourceDataCtrl
{
    public GameNumBase<BigInteger> coin;
    private PlayerResourceData _playerResourceData;

}

/// <summary>
/// 玩家的资源，比如：金币
/// </summary>
public class PlayerResourceData
{
    public GameNumBase<BigInteger> coin;
}
