using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalRockCycle : M8.ModalController, M8.IModalPush, M8.IModalActive {
    public const string parmCycleIndex = "index";
    public const string parmShowClose = "isCloseShown";

    [System.Serializable]
    public class Group {
        public M8.UI.Graphics.ColorPulse[] highlights;

        public void SetActive(bool active) {
            for(int i = 0; i < highlights.Length; i++) {
                if(highlights[i])
                    highlights[i].enabled = active;
            }
        }
    }

    public GameObject backGO;
    public GameObject closeGO;

    public Group[] groups;

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {

        }
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        bool closeShow = false;
        int index = -1;

        if(parms != null) {
            if(parms.ContainsKey(parmCycleIndex))
                index = parms.GetValue<int>(parmCycleIndex);

            if(parms.ContainsKey(parmShowClose))
                closeShow = parms.GetValue<bool>(parmShowClose);
        }

        for(int i = 0; i < groups.Length; i++)
            groups[i].SetActive(i == index);

        if(backGO) backGO.SetActive(closeShow);
        if(closeGO) closeGO.SetActive(closeShow);
    }

    void Awake() {
        if(backGO) backGO.SetActive(false);
        if(closeGO) closeGO.SetActive(false);
    }
}
