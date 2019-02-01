using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderChangeState : MonoBehaviour {
    [M8.TagSelector]
    public string[] tagFilters;

    public M8.State state;
        
    private void OnCollisionEnter2D(Collision2D collision) {
        var go = collision.gameObject;

        if(tagFilters.Length != 0) {
            bool tagMatched = false;
            for(int i = 0; i < tagFilters.Length; i++) {
                if(go.CompareTag(tagFilters[i])) {
                    tagMatched = true;
                    break;
                }
            }

            if(!tagMatched)
                return;
        }

        var stateCtrl = go.GetComponent<M8.StateController>();
        if(stateCtrl)
            stateCtrl.state = state;
    }
}
