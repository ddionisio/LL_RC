using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    [ActionCategory("Game")]
    public class CriteriaCheckComplete : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InventoryData))]
        public FsmObject inventory;

        [RequiredField]
        [ObjectType(typeof(CriteriaData))]
        public FsmObject criteria;

        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public FsmBool everyFrame;

        public override void Reset() {
            inventory = null;
            criteria = null;

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
            var invDat = inventory.Value as InventoryData;
            if(!invDat)
                return;

            var criteriaDat = criteria.Value as CriteriaData;
            if(!criteriaDat)
                return;

            var isComplete = criteriaDat.IsComplete(invDat);

            storeResult = isComplete;

            Fsm.Event(isComplete ? isTrue : isFalse);
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