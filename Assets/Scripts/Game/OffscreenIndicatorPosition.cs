﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffscreenIndicatorPosition : MonoBehaviour {
    public Vector3 targetPosition { get; set; }

    [SerializeField]
    bool _targetActive;

    public bool targetActive { get { return _targetActive; } set { _targetActive = value; } }

    [M8.TagSelector]
    public string cameraTag = ""; //grab from this if no direct camera
    [SerializeField]
    Camera _camera = null;

    public Transform displayRoot;

    [Header("Radius Anchor")]
    public bool useAnchorLimit;
    [M8.TagSelector]
    public string anchorLimitTag;
    public float anchorLimitRadius;

    private GameObject mAnchorGO;
        
    public Camera cameraSource {
        get {
            if(!_camera && !string.IsNullOrEmpty(cameraTag)) {
                var go = GameObject.FindGameObjectWithTag(cameraTag);
                if(go)
                    _camera = go.GetComponent<Camera>();
            }

            return _camera;
        }

        set { _camera = value; }
    }

    void Awake() {
        displayRoot.gameObject.SetActive(false);
    }

    void Update() {
        if(!cameraSource) return;
        if(!targetActive) return;

        Vector3 vp = cameraSource.WorldToViewportPoint(targetPosition);

        bool isEdge = false;

        if(vp.x > 1) {
            vp.x = 1; isEdge = true;
        }
        else if(vp.x < 0) {
            vp.x = 0; isEdge = true;
        }

        if(vp.y > 1) {
            vp.y = 1; isEdge = true;
        }
        else if(vp.y < 0) {
            vp.y = 0; isEdge = true;
        }

        if(isEdge) {
            displayRoot.gameObject.SetActive(true);

            Vector3 pos = cameraSource.ViewportToWorldPoint(vp);

            if(useAnchorLimit) {
                if(!mAnchorGO)
                    mAnchorGO = GameObject.FindGameObjectWithTag(anchorLimitTag);

                if(mAnchorGO) {
                    Vector2 fromPos = mAnchorGO.transform.position;
                    Vector2 toPos = pos;

                    Vector2 dPos = toPos - fromPos;
                    float len = dPos.magnitude;
                    if(len > 0f) {
                        Vector2 dir = dPos / len;

                        if(len > anchorLimitRadius)
                            toPos = fromPos + dir * anchorLimitRadius;

                        displayRoot.position = toPos;
                        displayRoot.up = dir;
                    }
                }
            }
            else {
                displayRoot.position = new Vector3(pos.x, pos.y, 0f);
                displayRoot.up = ((Vector2)(displayRoot.position - cameraSource.transform.position)).normalized;// new Vector3(vp.x - 0.5f, vp.y - 0.5f, 0.0f);
            }
        }
        else {
            displayRoot.gameObject.SetActive(false);
        }
    }

    void OnDrawGizmos() {
        if(useAnchorLimit) {
            if(anchorLimitRadius > 0f) {
                Gizmos.color = new Color(0f, 1f, 0f);
                Gizmos.DrawWireSphere(transform.position, anchorLimitRadius);
            }
        }
    }
}
