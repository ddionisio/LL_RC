﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LoLExt;

public class ModalVictory : M8.ModalController, M8.IModalPush {
    [Header("Data")]
    [M8.TagSelector]
    public string collectControllerTag;

    public M8.SceneAssetPath toScene;

    [Header("UI")]
    public InfoDataListWidget infoList;

    [Header("Sound")]
    [M8.SoundPlaylist]
    public string soundPlay;

    public void Proceed() {
        //
        if(LoLManager.isInstantiated)
            LoLManager.instance.ApplyProgress(LoLManager.instance.curProgress + 1);

        toScene.Load();
    }
    
    void M8.IModalPush.Push(M8.GenericParams parms) {
        //apply outcome
        CollectController collectCtrl = null;

        var go = GameObject.FindGameObjectWithTag(collectControllerTag);
        if(go)
            collectCtrl = go.GetComponent<CollectController>();

        if(!collectCtrl) {
            Debug.LogWarning("Collect Controller Not Found!");
            return;
        }

        M8.SoundPlaylist.instance.Play(soundPlay, false);

        collectCtrl.ApplyOutcome();

        //setup display of collections
        var collects = collectCtrl.collectList;
        infoList.Init(collects, null);
    }
}
