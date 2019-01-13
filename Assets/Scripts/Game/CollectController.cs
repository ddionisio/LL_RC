using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectController : M8.SingletonBehaviour<CollectController> {
    public MineralData[] minerals;
    public MineralData[] bonusMinerals;

    public int collectMaxCount = 5;

    [Header("Signals")]
    public SignalInteger signalCollect; //listen to get amount collected
    public M8.Signal signalCollectComplete; //open up exit

    public int collectCurrent { get; private set; }
    
    private bool[] mBonusMineralCollected;

    private bool mIsCollectComplete;

    void OnDestroy() {
        signalCollect.callback -= OnCollect;
    }

    void Awake() {
        signalCollect.callback += OnCollect;
    }

    void OnCollect(int collect) {
        collectCurrent += collect;

        if(!mIsCollectComplete && collectCurrent >= collectMaxCount) {
            mIsCollectComplete = true;
            signalCollectComplete.Invoke();
        }
    }
}
