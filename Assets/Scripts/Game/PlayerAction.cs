using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlayerAction : MonoBehaviour {
    public abstract void ActionInvoke(PlayerController player);
}
