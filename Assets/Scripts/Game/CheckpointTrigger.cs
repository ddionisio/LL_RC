using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointTrigger : MonoBehaviour {
    [Header("Data")]
    [M8.TagSelector]
    public string playerTag = "Player";

    public Transform target;

    void OnTriggerEnter2D(Collider2D collision) {
        if(!string.IsNullOrEmpty(playerTag) && !collision.CompareTag(playerTag))
            return;

        Checkpoint.localPosition = target.position;
        Checkpoint.localRotation = target.eulerAngles.z;
    }

    void Awake() {
        if(!target)
            target = transform;
    }
}
