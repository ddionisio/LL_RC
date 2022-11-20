using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerController : MonoBehaviour {
    public enum ParamType {
        Integer,
        Float,
        Vector2,
        String,
        Bool
    }

    [System.Serializable]
    public struct ParamData {
        public string key;

        public ParamType type;

        public int iVal;
        public float fVal;
        public Vector2 vVal;
        public string sVal;
        public bool bVal;

        public void Apply(M8.GenericParams parms) {
            switch(type) {
                case ParamType.Integer:
                    parms[key] = iVal;
                    break;
                case ParamType.Float:
                    parms[key] = fVal;
                    break;
                case ParamType.Vector2:
                    parms[key] = vVal;
                    break;
                case ParamType.String:
                    parms[key] = sVal;
                    break;
                case ParamType.Bool:
                    parms[key] = bVal;
                    break;
            }
        }
    }

    [Header("Template")]
    public GameObject template;
    public string poolName;
    public int poolCapacity;

    [Header("Data")]
    public Transform spawnPoint;
    public float startDelay = 0.5f;
    public float spawnAfterDelay = 1f;
    public int activeCount = 3;

    [Header("Parameters")]
    public ParamData[] parms;

    private M8.PoolController mPool;
    private M8.CacheList<M8.PoolDataController> mActives;

    private M8.GenericParams mParms = new M8.GenericParams();

    void OnDisable() {
        if(mActives != null) {
            for(int i = 0; i < mActives.Count; i++) {
                var itm = mActives[i];
                if(itm) {
                    itm.despawnCallback -= OnSpawnRelease;
                    itm.Release();
                }
            }

            mActives.Clear();
        }
    }

    void OnEnable() {
        StartCoroutine(DoSpawn());
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(poolName);
        mPool.AddType(template, poolCapacity, poolCapacity);

        mActives = new M8.CacheList<M8.PoolDataController>(activeCount);

        ApplyParams();
    }

    IEnumerator DoSpawn() {
        var waitStart = new WaitForSeconds(startDelay);
        var waitAfterSpawn = new WaitForSeconds(spawnAfterDelay);

        yield return waitStart;

        while(true) {
            Vector2 spawnPt = spawnPoint.position;

#if UNITY_EDITOR
            ApplyParams();
#endif

            var spawn = mPool.Spawn(template.name, template.name, null, mParms);
            spawn.despawnCallback += OnSpawnRelease;

            spawn.transform.position = spawnPt;

            mActives.Add(spawn);

            while(mActives.Count == activeCount)
                yield return null;

            yield return waitAfterSpawn;
        }
    }

    void OnSpawnRelease(M8.PoolDataController item) {
        item.despawnCallback -= OnSpawnRelease;

        mActives.Remove(item);
    }

    void ApplyParams() {
        for(int i = 0; i < parms.Length; i++)
            parms[i].Apply(mParms);
    }
}
