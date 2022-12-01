using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualPadButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public SignalButtonState signalState;

    private M8.InputAction.ButtonState mCurState;
    private bool mIsDown;

    void OnEnable() {
        mIsDown = false;
        mCurState = M8.InputAction.ButtonState.None;
    }

    void Update() {
        var newState = mCurState;

        if(mIsDown) {
            switch(mCurState) {
                case M8.InputAction.ButtonState.None:
                    newState = M8.InputAction.ButtonState.Pressed;
                    break;
                case M8.InputAction.ButtonState.Pressed:
                    newState = M8.InputAction.ButtonState.Down;
                    break;
            }
        }
        else {
            switch(mCurState) {
                case M8.InputAction.ButtonState.Down:
                    newState = M8.InputAction.ButtonState.Released;
                    break;
                case M8.InputAction.ButtonState.Released:
                    newState = M8.InputAction.ButtonState.None;
                    break;
            }
        }

        if(mCurState != newState) {
            mCurState = newState;
            if(signalState)
                signalState.Invoke(mCurState);
        }
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData) {
        mIsDown = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData) {
        mIsDown = false;
    }
}
