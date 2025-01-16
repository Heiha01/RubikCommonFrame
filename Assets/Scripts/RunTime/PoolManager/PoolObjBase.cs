using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObjBase : MonoBehaviour
{
    private string _id;
    private float _lastActiveTime;

    public float LastActiveTime => _lastActiveTime;

    public string Id => _id;

    public virtual void Init(string id)
    {
        _id = id;
    }

    public virtual void Appear()
    {
        gameObject.SetActive(true);
    }

    public virtual void Disappear()
    {
        _lastActiveTime = Time.time;
        gameObject.SetActive(false);
    }

    public virtual void Reset()
    {

    }
}
