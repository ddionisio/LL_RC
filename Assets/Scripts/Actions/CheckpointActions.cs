using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class CheckpointSet : FsmStateAction {
        [RequiredField]
        public FsmOwnerDefault target;

        public bool global;

        public override void Reset() {
            target = null;
            global = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(target);
            if(go) {
                if(global) {
                    Checkpoint.globalPosition = go.transform.position;
                    Checkpoint.globalRotation = go.transform.eulerAngles.z;
                }
                else {
                    Checkpoint.localPosition = go.transform.position;
                    Checkpoint.localRotation = go.transform.eulerAngles.z;
                }
            }

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class CheckpointGet : FsmStateAction {
        [RequiredField]
        public FsmOwnerDefault target;

        public bool global;

        public override void Reset() {
            target = null;
            global = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(target);
            if(go) {
                if(global) {
                    go.transform.position = Checkpoint.globalPosition;
                    go.transform.eulerAngles = new Vector3(0f, 0f, Checkpoint.globalRotation);
                }
                else {
                    go.transform.position = Checkpoint.localPosition;
                    go.transform.eulerAngles = new Vector3(0f, 0f, Checkpoint.localRotation);
                }
            }

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class CheckpointRemoveGlobal : FsmStateAction {
        public override void OnEnter() {
            Checkpoint.RemoveGlobal();

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class CheckpointCheckGlobalExists : FsmStateAction {
        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public FsmBool everyFrame;

        public override void Reset() {
            storeResult = null;
            isTrue = null;
            isFalse = null;

            everyFrame = false;
        }

        public override void OnEnter() {
            DoCheck();

            if(!everyFrame.Value)
                Finish();
        }

        public override void OnUpdate() {
            DoCheck();
        }

        void DoCheck() {
            var exists = Checkpoint.globalAvailable;

            storeResult = exists;

            Fsm.Event(exists ? isTrue : isFalse);
        }

        public override string ErrorCheck() {
            if(everyFrame.Value &&
                FsmEvent.IsNullOrEmpty(isTrue) &&
                FsmEvent.IsNullOrEmpty(isFalse))
                return "Action sends no events!";
            return "";
        }
    }
}