using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalCollectionsProxy : MonoBehaviour {
    public M8.ModalManagerPath modalManager;
    public string modal = "collections";
    public bool ignoreNewlySeen = true;

    private M8.GenericParams mParms = new M8.GenericParams();

    public void Execute() {
        mParms[ModalCollections.parmIgnoreNewlySeen] = ignoreNewlySeen;

        var uiMgr = modalManager.manager;
        uiMgr.Open(modal, mParms);
    }
}
