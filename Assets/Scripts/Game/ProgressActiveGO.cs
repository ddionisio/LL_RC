using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

public class ProgressActiveGO : MonoBehaviour {
    public enum InactiveDataCheck {
        None,
        Greater,
        Less,
        AnyRockOrganic,
        AnyRock
    }

    public int[] progressMatch;

    [Header("Show")]
    public GameObject activeGO;
    public GameObject inactiveGO;

    [Header("Inactive")]
    public InactiveDataCheck inactiveDataCheck;
    public InventoryData inactiveInventory; //for use in specialized check
    public InfoData inactiveData;
    public int inactiveCount;

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

        if(isMatch) {
            switch(inactiveDataCheck) {
                case InactiveDataCheck.Greater:
                    if(inactiveData.count >= inactiveCount)
                        isMatch = false;
                    break;

                case InactiveDataCheck.Less:
                    if(inactiveData.count <= inactiveCount)
                        isMatch = false;
                    break;

                case InactiveDataCheck.AnyRockOrganic:
                    if(inactiveInventory.rocksCount <= 0 && inactiveInventory.organicsCount <= 0)
                        isMatch = false;
                    break;

                case InactiveDataCheck.AnyRock:
                    if(inactiveInventory.rocksCount <= 0)
                        isMatch = false;
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
