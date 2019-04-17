using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalRockCycle : M8.ModalController, M8.IModalPush, M8.IModalActive {
    public const string parmCycleIndex = "index";

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

    public Group[] groups;

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {

        }
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        int index = -1;

        if(parms != null) {
            if(parms.ContainsKey(parmCycleIndex))
                index = parms.GetValue<int>(parmCycleIndex);
        }

        for(int i = 0; i < groups.Length; i++)
            groups[i].SetActive(i == index);
    }
}
