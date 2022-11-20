using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class RockSpawnController : MonoBehaviour {
    public string poolGroup = "rock";
    public GameObject template;
    public int capacity;

    public Transform spawnPoint;

    private M8.PoolController mPool;

    private M8.GenericParams mParms = new M8.GenericParams();
    
    public void SpawnAll(SpriteShape spriteShape, float delayPerSpawn) {
        StartCoroutine(DoSpawnAll(spriteShape, delayPerSpawn));
    }

    public void Clear() {
        StopAllCoroutines();
        mPool.ReleaseAllByType(name);
    }
    
    void Awake() {
        mPool = M8.PoolController.CreatePool(poolGroup);
        mPool.AddType(name, template, capacity, capacity);
    }

    IEnumerator DoSpawnAll(SpriteShape spriteShape, float delayPerSpawn) {
        var wait = new WaitForSeconds(delayPerSpawn);

        mParms[Rock.parmSpriteShape] = spriteShape;
        mParms[Rock.parmDir] = (Vector2)spawnPoint.up;

        for(int i = 0; i < capacity; i++) {
            mPool.Spawn(name, "", null, spawnPoint.position, mParms);
            yield return wait;
        }
    }
}
