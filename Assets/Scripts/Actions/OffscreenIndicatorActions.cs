using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class OffscreenIndicatorSetPosition : ComponentAction<OffscreenIndicatorPosition> {
        [RequiredField]
        [ObjectType(typeof(OffscreenIndicatorPosition))]
        public FsmOwnerDefault gameObject;

        public FsmOwnerDefault target;

        public override void Reset() {
            gameObject = null;
            target = null;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                var targetGO = Fsm.GetOwnerDefaultTarget(target);
                if(targetGO)
                    cachedComponent.targetPosition = targetGO.transform.position;
            }

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class OffscreenIndicatorSetActive : ComponentAction<OffscreenIndicatorPosition> {
        [RequiredField]
        [ObjectType(typeof(OffscreenIndicatorPosition))]
        public FsmOwnerDefault gameObject;

        public FsmBool active;

        public override void Reset() {
            gameObject = null;
            active = false;
        }

        public override void OnEnter() {
            var go = Fsm.GetOwnerDefaultTarget(gameObject);
            if(UpdateCache(go)) {
                cachedComponent.targetActive = active.Value;
            }

            Finish();
        }
    }
}