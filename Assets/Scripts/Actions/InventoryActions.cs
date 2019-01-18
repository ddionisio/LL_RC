using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions.Game {
    #region General Items
    [ActionCategory("Game")]
    public class InventoryItemGetCount : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InfoData))]
        public FsmObject data;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmInt output;

        public override void Reset() {
            data = null;
            output = null;
        }

        public override void OnEnter() {
            var infData = data.Value as InfoData;
            if(infData)
                output.Value = infData.count;

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class InventoryItemSetCount : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InfoData))]
        public FsmObject data;

        public FsmInt count;

        public override void Reset() {
            data = null;
            count = null;
        }

        public override void OnEnter() {
            var infData = data.Value as InfoData;
            if(infData)
                infData.count = count.Value;

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class InventoryItemSetSeen : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InfoData))]
        public FsmObject data;

        public override void Reset() {
            data = null;
        }

        public override void OnEnter() {
            var infData = data.Value as InfoData;
            if(infData)
                infData.isSeen = true;

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class InventoryItemCheckSeen : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InfoData))]
        public FsmObject data;

        [UIHint(UIHint.Variable)]
        public FsmBool storeResult;

        public FsmEvent isTrue;
        public FsmEvent isFalse;

        public FsmBool everyFrame;

        public override void Reset() {
            data = null;

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
            var infData = data.Value as InfoData;
            if(!infData)
                return;

            var isSeen = infData.isSeen;

            storeResult = isSeen;

            Fsm.Event(isSeen ? isTrue : isFalse);
        }

        public override string ErrorCheck() {
            if(everyFrame.Value &&
                FsmEvent.IsNullOrEmpty(isTrue) &&
                FsmEvent.IsNullOrEmpty(isFalse))
                return "Action sends no events!";
            return "";
        }
    }
    #endregion

    #region Magma
    [ActionCategory("Game")]
    public class InventoryMagmaGetCapacity : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(MagmaData))]
        public FsmObject data;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmInt output;

        public override void Reset() {
            data = null;
            output = null;
        }

        public override void OnEnter() {
            var infData = data.Value as MagmaData;
            if(infData)
                output.Value = infData.capacity;

            Finish();
        }
    }
    #endregion

    #region Inventory
    [ActionCategory("Game")]
    public class InventoryGetMineralsCount : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InventoryData))]
        public FsmObject inventory;

        [RequiredField]
        [UIHint(UIHint.Variable)]
        public FsmInt output;

        public override void Reset() {
            inventory = null;
            output = null;
        }

        public override void OnEnter() {
            var inv = inventory.Value as InventoryData;
            if(inv)
                output.Value = inv.mineralsCount;

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class InventoryClearMineralsCount : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InventoryData))]
        public FsmObject inventory;

        public override void Reset() {
            inventory = null;
        }

        public override void OnEnter() {
            var inv = inventory.Value as InventoryData;
            if(inv)
                inv.ClearMineralsCount();

            Finish();
        }
    }

    [ActionCategory("Game")]
    public class InventoryDeleteAll : FsmStateAction {
        [RequiredField]
        [ObjectType(typeof(InventoryData))]
        public FsmObject inventory;

        public bool deleteSeen; //if true, also remove seen

        public override void Reset() {
            inventory = null;
        }

        public override void OnEnter() {
            var inv = inventory.Value as InventoryData;
            if(inv)
                inv.DeleteAll(deleteSeen);

            Finish();
        }
    }
    #endregion
}