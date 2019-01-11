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
}