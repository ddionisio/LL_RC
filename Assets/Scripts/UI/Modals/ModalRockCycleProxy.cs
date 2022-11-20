using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalRockCycleProxy : MonoBehaviour {
    public M8.ModalManagerPath modalManager;
    public string modal = "rockCycle";
    public int highlightInd = -1;
    public bool showClose = true;

    private M8.GenericParams mParms = new M8.GenericParams();

    public void Execute() {
        mParms[ModalRockCycle.parmCycleIndex] = highlightInd;
        mParms[ModalRockCycle.parmShowClose] = showClose;

        var uiMgr = modalManager.manager;
        uiMgr.Open(modal, mParms);
    }
}
