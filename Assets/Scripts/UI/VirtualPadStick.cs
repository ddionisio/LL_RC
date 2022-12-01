using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualPadStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [Header("Data")]
    public Transform center;
    public float radius;
    public float deadZone;
    public float scaleOffset;

    [Header("Display")]
    public Transform knob;

    [Header("Signals")]
    public SignalAxis signalInvokeAxisHorz;

    private PointerEventData mDownEventData;

    private Canvas mCanvas;
    private float mCurAxisHorz;

    void OnEnable() {
        if(knob)
            knob.position = center.position;

        mDownEventData = null;

        mCurAxisHorz = 0f;
    }

    void Awake() {
        mCanvas = GetComponentInParent<Canvas>();

        if(!center)
            center = transform;
    }

    void Update() {
        if(mDownEventData != null) {
            Vector2 centerPt = center.position;
            //centerPt.x *= mCanvas.scaleFactor;
            //centerPt.y *= mCanvas.scaleFactor;

            var delta = mDownEventData.position - centerPt;
            var len = delta.magnitude;

            Vector2 dir;
            float deltaRadius;
            float scaleX;

            if(len > 0f) {
                dir = delta/len;
                deltaRadius = len / mCanvas.scaleFactor;

                if(deltaRadius > deadZone)
                    scaleX = Mathf.Clamp(Mathf.Sign(dir.x) * (((deltaRadius - deadZone) / (radius - deadZone)) + scaleOffset), -1f, 1f);
                else
                    scaleX = 0f;
            }
            else {
                dir = Vector2.zero;
                deltaRadius = 0f;
                scaleX = 0f;
            }

            if(knob) {
                var r = Mathf.Clamp(deltaRadius, 0f, radius) * mCanvas.scaleFactor;
                knob.position = centerPt + dir * r;
            }

            if(mCurAxisHorz != scaleX)
                UpdateAxisValue(scaleX);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        mDownEventData = eventData;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        mDownEventData = null;

        if(knob)
            knob.position = center.position;

        UpdateAxisValue(0f);
    }

    private void UpdateAxisValue(float val) {
        mCurAxisHorz = val;

        if(signalInvokeAxisHorz)
            signalInvokeAxisHorz.Invoke(mCurAxisHorz);
    }

    void OnDrawGizmos() {
        if(radius > 0f) {
            var pos = center ? center.position : transform.position;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(pos, radius);
        }

        if(deadZone > 0f) {
            var pos = center ? center.position : transform.position;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(pos, deadZone);
        }
    }
}
