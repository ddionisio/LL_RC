using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SignalInvokeEnableDisable : MonoBehaviour {
    public M8.SignalBoolean signalInvoke;

    void OnDisable() {
        if(signalInvoke) signalInvoke.Invoke(false);
    }

    void OnEnable() {
        if(signalInvoke) signalInvoke.Invoke(true);
    }
}
