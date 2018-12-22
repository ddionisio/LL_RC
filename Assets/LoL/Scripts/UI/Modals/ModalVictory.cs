using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalVictory : M8.ModalController, M8.IModalActive, M8.IModalPush {
    public string sfxPath;

    void M8.IModalActive.SetActive(bool aActive) {
        if(aActive) {
            if(!string.IsNullOrEmpty(sfxPath))
                LoLManager.instance.PlaySound(sfxPath, false, false);
        }
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {

    }
}
