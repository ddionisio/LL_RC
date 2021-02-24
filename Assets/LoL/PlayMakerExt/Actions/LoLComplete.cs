using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LoLExt;

namespace HutongGames.PlayMaker.Actions.LoL {
    [ActionCategory("Legends of Learning")]
    [Tooltip("This will effectively end the game.")]
    public class LoLComplete : FsmStateAction {
        public override void OnEnter() {
            if(LoLManager.isInstantiated) {
                //check if there are speak queued
                if(!LoLManager.instance.isSpeechQueued) {
                    LoLManager.instance.Complete();
                    Finish();
                }
            }
            else
                Finish();
        }

        public override void OnUpdate() {
            //check if there are speak queued
            if(!LoLManager.instance.isSpeechQueued) {
                LoLManager.instance.Complete();
                Finish();
            }
        }
    }
}