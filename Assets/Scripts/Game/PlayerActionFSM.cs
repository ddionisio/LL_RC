using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActionFSM : PlayerAction {
    public PlayMakerFSM FSM;
    public string actionEvent;

    public override void ActionInvoke(PlayerController player) {
        FSM.SendEvent(actionEvent);
    }

    void Awake() {
        if(!FSM)
            FSM = GetComponent<PlayMakerFSM>();
    }
}
