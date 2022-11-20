using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class CheckpointSet : FsmStateAction {
        [RequiredField]
        public FsmOwnerDefault target;

        public bool start;

        public override void Reset() {
            target = null;
            start = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(target);
            if(go) {
                if(start)
                    Checkpoint.SetStart(go.transform);
                else
                    Checkpoint.SetLocal(go.transform);
            }

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class CheckpointGet : FsmStateAction {
        [RequiredField]
        public FsmOwnerDefault target;

        public bool start;

        public override void Reset() {
            target = null;
            start = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(target);
            if(go) {
                if(start) {
                    go.transform.position = Checkpoint.startPosition;
                    go.transform.eulerAngles = new Vector3(0f, 0f, Checkpoint.startRotation);
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
    public class CheckpointRemoveStart : FsmStateAction {
        public override void OnEnter() {
            Checkpoint.RemoveStart();

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class CheckpointCheckStartExists : FsmStateAction {
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
            var exists = Checkpoint.startAvailable;

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