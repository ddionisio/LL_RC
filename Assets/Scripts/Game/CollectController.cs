using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectController : M8.SingletonBehaviour<CollectController> {
    public int collectMaxCount = 5;

    [Header("Signals")]
    public SignalCollect signalCollect; //listen to get amount collected
    public M8.Signal signalCollectComplete; //open up exit

    public int collectCurrent { get; private set; }

    public List<InfoData> collectList { get { return mCollectList; } }
    
    private bool[] mBonusMineralCollected;

    private bool mIsCollectComplete;

    private List<InfoData> mCollectList = new List<InfoData>();

    void OnDestroy() {
        signalCollect.callback -= OnCollect;
    }

    void Awake() {
        signalCollect.callback += OnCollect;
    }

    void OnCollect(InfoData dat, int amt) {
        collectCurrent += amt;

        if(!mCollectList.Contains(dat))
            mCollectList.Add(dat);

        if(!mIsCollectComplete && collectCurrent >= collectMaxCount) {
            mIsCollectComplete = true;
            signalCollectComplete.Invoke();
        }
    }
}
