using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectController : MonoBehaviour {
    [System.Serializable]
    public struct CollectOutcome {
        public InfoData dat;
        public int count;
    }

    public CollectOutcome[] collectOutcomes;
    public int collectMaxCount = 5;

    [Header("Sound")]
    [M8.SoundPlaylist]
    public string soundComplete;

    [Header("Signals")]
    public SignalInteger signalCollect; //listen to get amount collected
    public M8.Signal signalCollectComplete; //open up exit

    public int collectCurrent { get; private set; }

    public List<InfoData> collectList { get; private set; }
    
    private bool mIsCollectComplete = false;
    private bool mIsOutcomeApplied = false;

    public void ApplyOutcome() {
        if(!mIsOutcomeApplied) {
            for(int i = 0; i < collectOutcomes.Length; i++) {
                if(collectOutcomes[i].dat) {
                    collectOutcomes[i].dat.count += collectOutcomes[i].count;
                    collectOutcomes[i].dat.isSeen = true;
                }
            }

            mIsOutcomeApplied = true;
        }
    }

    void OnDestroy() {
        signalCollect.callback -= OnCollect;
    }

    void Awake() {
        collectList = new List<InfoData>(collectOutcomes.Length);
        for(int i = 0; i < collectOutcomes.Length; i++) {
            if(collectOutcomes[i].dat)
                collectList.Add(collectOutcomes[i].dat);
        }

        signalCollect.callback += OnCollect;
    }

    void OnCollect(int amt) {
        collectCurrent += amt;

        if(!mIsCollectComplete && collectCurrent >= collectMaxCount) {
            mIsCollectComplete = true;

            M8.SoundPlaylist.instance.Play(soundComplete, false);

            signalCollectComplete.Invoke();
        }
    }
}
