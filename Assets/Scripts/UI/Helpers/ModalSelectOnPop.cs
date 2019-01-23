using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModalSelectOnPop : MonoBehaviour, M8.IModalPush, M8.IModalPop {
    public const string parmSelectGO = "modalSelectOnPopGO";

    public enum Mode {
        None,
        Previous,
        Param
    }

    public Mode mode = Mode.Previous;

    private GameObject mSelect;

    void M8.IModalPop.Pop() {
        var es = EventSystem.current;
        if(es)
            es.SetSelectedGameObject(mSelect);

        mSelect = null;
    }

    void M8.IModalPush.Push(M8.GenericParams parms) {
        switch(mode) {
            case Mode.None:
                mSelect = null;
                break;

            case Mode.Previous:
                var es = EventSystem.current;
                if(es)
                    mSelect = es.currentSelectedGameObject;
                else
                    mSelect = null;
                break;

            case Mode.Param:
                if(parms != null && parms.ContainsKey(parmSelectGO))
                    mSelect = parms.GetValue<GameObject>(parmSelectGO);
                else
                    mSelect = null;
                break;
        }
    }
}
