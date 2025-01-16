using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [ContextMenu("pppp")]
    public void ShowApplicationPath()
    {
        Debug.LogError(Application.dataPath);
    }
}
