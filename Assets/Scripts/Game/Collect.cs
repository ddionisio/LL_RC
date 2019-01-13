using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collect : MonoBehaviour {
    [Header("Info")]
    public int amount = 1;

    [Header("Data")]
    [M8.TagSelector]
    public string playerTag = "Player";

    public PlayMakerFSM fsm;
    public string fsmCollectEvent = "COLLECT";

    public GameObject collectGO;
    public GameObject collectedGO;
    
    [Header("Signals")]
    public SignalInteger signalCollect;

    public bool isCollected { get; private set; }

    private Collider2D mCollider;
        
    void Awake() {
        if(!fsm)
            fsm = GetComponent<PlayMakerFSM>();

        if(collectGO) collectGO.SetActive(true);
        if(collectedGO) collectedGO.SetActive(false);

        mCollider = GetComponent<Collider2D>();
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if(isCollected)
            return;

        Collected();
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if(isCollected)
            return;

        var go = collision.gameObject;

        if(!go.CompareTag(playerTag))
            return;

        //only collect if we are hit from below by rigidbody
        var bodyCtrl = collision.gameObject.GetComponent<M8.RigidBodyController2D>();
        if(bodyCtrl) {
            var flags = bodyCtrl.GetCollisionFlag(collision);
            if((flags & CollisionFlags.Below) != CollisionFlags.None) {
                Collected();
            }
        }
    }

    void Collected() {
        isCollected = true;

        if(mCollider)
            mCollider.enabled = false;

        if(collectGO) collectGO.SetActive(false);
        if(collectedGO) collectedGO.SetActive(true);

        if(fsm && !string.IsNullOrEmpty(fsmCollectEvent))
            fsm.SendEvent(fsmCollectEvent);

        signalCollect.Invoke(amount);
    }
}
