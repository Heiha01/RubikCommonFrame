using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilsModule;

public class GameConfigManager : ManagerBase<GameConfigManager>
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera uiCamera;

    public Camera MainCamera => mainCamera;

    public Camera UiCamera => uiCamera;
    public override void Init()
    {

    }
}
