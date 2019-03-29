using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class RockSpawnController : MonoBehaviour {
    [System.Serializable]
    public class RockTemplateInfo {
        public GameObject template;
        public int capacity;

        public M8.CacheList<Rock> activeRocks { get; private set; }

        private M8.GenericParams mSpawnParms = new M8.GenericParams();

        public void Init(M8.PoolController pool) {
            pool.AddType(template, capacity, capacity);

            activeRocks = new M8.CacheList<Rock>(capacity);
        }

        public void Spawn(M8.PoolController pool, SpriteShape spriteShape, Vector2 pt) {
            mSpawnParms[Rock.parmSpriteShape] = spriteShape;

            var rock = pool.Spawn<Rock>(template.name, "", null, pt, mSpawnParms);

            activeRocks.Add(rock);
        }

        public Rock GetRandomRock() {
            if(activeRocks != null && activeRocks.Count > 0)
                return activeRocks[Random.Range(0, activeRocks.Count)];
            return null;
        }

        public void Despawn(Rock rock) {
            activeRocks.Remove(rock);
            rock.Despawn();
        }

        public void Clear() {
            if(activeRocks != null) {
                for(int i = 0; i < activeRocks.Count; i++) {
                    if(activeRocks[i])
                        activeRocks[i].Release();
                }

                activeRocks.Clear();
            }
        }
    }

    public struct SpawnQueueInfo {
        public int index;
        public Vector2 point;
        public SpriteShape spriteShape;
    }

    [Header("Template")]
    public string poolGroup;
    public RockTemplateInfo[] rockTemplates;

    [Header("Data")]
    public Transform spawnPointsRoot;
    public float spawnQueueDelay = 0.3f;

    public bool isSpawnQueueBusy { get { return mSpawnQueueRout != null; } }

    public bool isAllRockAsleep {
        get {
            int rockActiveCount = 0;
            int rockSleepCount = 0;

            for(int i = 0; i < rockTemplates.Length; i++) {
                var template = rockTemplates[i];
                if(template.activeRocks != null) {
                    for(int j = 0; j < template.activeRocks.Count; j++) {
                        var rock = template.activeRocks[j];
                        if(rock) {
                            rockActiveCount++;

                            if(rock.isAsleep)
                                rockSleepCount++;
                        }
                    }
                }
            }

            return rockActiveCount == rockSleepCount;
        }
    }

    private M8.PoolController mPool;

    private Queue<SpawnQueueInfo> mSpawnQueue = new Queue<SpawnQueueInfo>();

    private Coroutine mSpawnQueueRout;
    private int mCurSpawnPointInd;

    private Vector2[] mSpawnPts;

    public int GetActiveCount(int index) {
        return rockTemplates[index].activeRocks.Count;
    }

    public void SpawnSplitFrom(int index) {
        if(index + 1 >= rockTemplates.Length)
            return;

        var template = rockTemplates[index];
        var templateNext = rockTemplates[index + 1];

        var rock = template.GetRandomRock();
        if(rock) {
            var spriteShape = rock.spriteShapeCtrl.spriteShape;

            //spawn from rock's spawn points
            int _count = rock.spawnPoints.Length;
            for(int i = 0; i < _count; i++) {
                Vector2 pt = rock.spawnPoints[i].position;
                templateNext.Spawn(mPool, spriteShape, pt);
            }

            template.Despawn(rock);
        }
    }

    public void SpawnQueue(int index, SpriteShape spriteShape, int count) {
        for(int i = 0; i < count; i++) {
            Vector2 pt = mSpawnPts[mCurSpawnPointInd];

            mSpawnQueue.Enqueue(new SpawnQueueInfo { index=index, spriteShape = spriteShape, point=pt });

            mCurSpawnPointInd++;
            if(mCurSpawnPointInd == mSpawnPts.Length)
                mCurSpawnPointInd = 0;
        }

        if(mSpawnQueueRout == null)
            mSpawnQueueRout = StartCoroutine(DoSpawnQueue());
    }

    public void Clear() {
        for(int i = 0; i < rockTemplates.Length; i++)
            rockTemplates[i].Clear();
    }

    void Awake() {
        mPool = M8.PoolController.CreatePool(poolGroup);

        for(int i = 0; i < rockTemplates.Length; i++) {
            rockTemplates[i].Init(mPool);
        }

        mSpawnPts = new Vector2[spawnPointsRoot.childCount];
        for(int i = 0; i < spawnPointsRoot.childCount; i++)
            mSpawnPts[i] = spawnPointsRoot.GetChild(i).position;
        M8.ArrayUtil.Shuffle(mSpawnPts);
    }

    IEnumerator DoSpawnQueue() {
        var wait = new WaitForSeconds(spawnQueueDelay);

        while(mSpawnQueue.Count > 0) {
            var dat = mSpawnQueue.Dequeue();

            rockTemplates[dat.index].Spawn(mPool, dat.spriteShape, dat.point);

            yield return wait;
        }

        mSpawnQueueRout = null;
    }
}
