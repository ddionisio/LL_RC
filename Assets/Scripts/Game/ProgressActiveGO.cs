using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressActiveGO : MonoBehaviour {
    public int[] progressMatch;

    [Header("Show")]
    public GameObject activeGO;
    public GameObject inactiveGO;

    [Header("FSM")]
    public PlayMakerFSM fsm;
    public string fsmEventActive;
    public string fsmEventInactive;

    [Header("Signals")]
    public M8.Signal signalRefresh;

    void OnDisable() {
        if(signalRefresh)
            signalRefresh.callback -= OnRefresh;
    }

    void OnEnable() {
        Refresh();

        if(signalRefresh)
            signalRefresh.callback += OnRefresh;
    }

    public void Refresh() {
        int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

        bool isMatch = false;
        for(int i = 0; i < progressMatch.Length; i++) {
            if(progress == progressMatch[i]) {
                isMatch = true;
                break;
            }
        }

        if(activeGO) activeGO.SetActive(isMatch);
        if(inactiveGO) inactiveGO.SetActive(!isMatch);

        if(fsm)
            fsm.SendEvent(isMatch ? fsmEventActive : fsmEventInactive);
    }

    void OnRefresh() {
        Refresh();
    }
}
