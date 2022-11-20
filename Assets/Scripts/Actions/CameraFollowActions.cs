using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class CameraFollowSetTarget : ComponentAction<CameraFollow> {
        [RequiredField]
        [CheckForComponent(typeof(CameraFollow))]
        [Tooltip("The source.")]
        public FsmOwnerDefault gameObject;

        public FsmOwnerDefault target;

        public override void Reset() {
            gameObject = null;
            target = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                var followGO = Fsm.GetOwnerDefaultTarget(target);

                Transform followT = followGO ? followGO.transform : null;

                cachedComponent.follow = followT;
            }

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class CameraFollowCheckState : ComponentAction<CameraFollow> {
        [RequiredField]
        [CheckForComponent(typeof(CameraFollow))]
        [Tooltip("The source.")]
        public FsmOwnerDefault gameObject;

        public CameraFollow.State state;

        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public FsmBool everyFrame;

        public override void Reset() {
            gameObject = null;

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
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(!UpdateCache(go))
                return;

            var isMatch = cachedComponent.state == state;

            storeResult = isMatch;

            Fsm.Event(isMatch ? isTrue : isFalse);
        }

        public override string ErrorCheck() {
            if(everyFrame.Value &&
                FsmEvent.IsNullOrEmpty(isTrue) &&
                FsmEvent.IsNullOrEmpty(isFalse))
                return "Action sends no events!";
            return "";
        }
    }

    [ActionCategory("Game")]
    public class CameraFollowGotoCurrentTarget : ComponentAction<CameraFollow> {
        [RequiredField]
        [CheckForComponent(typeof(CameraFollow))]
        [Tooltip("The source.")]
        public FsmOwnerDefault gameObject;

        public override void Reset() {
            gameObject = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                cachedComponent.GotoCurrentFollow();
            }

            Finish();
        }
    }
}