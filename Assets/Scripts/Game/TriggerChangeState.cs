using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerChangeState : MonoBehaviour {
    [M8.TagSelector]
    public string[] tagFilters;

    public M8.State state;

    private void OnTriggerEnter2D(Collider2D collision) {
        if(tagFilters.Length != 0) {
            bool tagMatched = false;
            for(int i = 0; i < tagFilters.Length; i++) {
                if(collision.CompareTag(tagFilters[i])) {
                    tagMatched = true;
                    break;
                }
            }

            if(!tagMatched)
                return;
        }

        var stateCtrl = collision.GetComponent<M8.StateController>();
        if(stateCtrl)
            stateCtrl.state = state;
    }
}
