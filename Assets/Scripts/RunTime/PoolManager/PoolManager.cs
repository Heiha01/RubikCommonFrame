using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilsModule;

public class PoolManager : ManagerBase<PoolManager>
{
    private static Dictionary<string, ObjPoolCtrl> _pools;
    private float _time;

    public override void Init()
    {
        _pools = new Dictionary<string, ObjPoolCtrl>();
    }

    public static PoolObjBase Get(string prefabPath)
    {
        if (!_pools.ContainsKey(prefabPath))
        {
            _pools.Add(prefabPath, new ObjPoolCtrl(prefabPath, AssetsManager.Instance.LoadAssetImmediate<GameObject>(prefabPath)));
        }

        return _pools[prefabPath].GetObject();
    }

    public static PoolObjBase Get(Transform parent, Vector3 localPosition, string poolId)
    {
        var result = Get(poolId);
        var transform1 = result.transform;
        transform1.SetParent(parent, false);
        transform1.localPosition = localPosition;
        return result;
    }

    public static PoolObjBase Get(Transform parent, bool worldPosStays, string poolId)
    {
        var result = Get(poolId);
        var transform1 = result.transform;
        transform1.SetParent(parent, worldPosStays);
        return result;
    }


    private class ObjPoolCtrl
    {
        private readonly GameObject _prefab; // 对象池预制体
        private readonly Stack<PoolObjBase> _stackPool;
        private readonly Transform _poolTrans; // 对象池管理的物体的父物体
        private readonly string _poolKey;

        /// <summary>
        ///  对象池构造函数
        /// </summary>
        /// <param name="poolKey">对象池名称</param>
        /// <param name="prefab">对象预制体</param>
        public ObjPoolCtrl(string poolKey, GameObject prefab)
        {
            _poolKey = poolKey;
            _prefab = prefab;
            _stackPool = new Stack<PoolObjBase>();
            var poolTransGo = new GameObject(prefab.name + "Pool");

            _poolTrans = poolTransGo.transform;
        }

        /// <summary>
        ///  创建新对象
        /// </summary>
        /// <returns></returns>
        private PoolObjBase CreateNewObject()
        {
            GameObject newObj = Instantiate(_prefab, _poolTrans);
            newObj.SetActive(false);
            newObj.name = newObj.name.Replace("(Clone)", "");
            PoolObjBase newItem = newObj.GetComponent<PoolObjBase>();
            if (newItem == null) newItem = newObj.AddComponent<PoolObjBase>();
            newItem.Init(_poolKey);

            return newItem;
        }

        /// <summary>
        ///  从对象池中获取对象
        /// </summary>
        /// <returns></returns>
        public PoolObjBase GetObject()
        {
            PoolObjBase result;
            if (_stackPool.Count > 0)
            {
                result = _stackPool.Pop();
            }
            else
            {
                result = CreateNewObject();
            }
            result.Appear();
            return result;
        }

        public virtual void Recycle(PoolObjBase recyclable)
        {
            if (_stackPool.Contains(recyclable))
            {
                return;
            }

            recyclable.Disappear();
            recyclable.Reset();
            recyclable.transform.SetParent(_poolTrans, false);
            _stackPool.Push(recyclable);
        }
    }
}
