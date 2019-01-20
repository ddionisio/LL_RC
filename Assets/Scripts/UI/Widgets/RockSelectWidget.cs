using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RockSelectWidget : Selectable {
    [Header("Inventory")]
    public InventoryData inventory;

    [Header("Input")]
    public M8.InputAction processInput; //use the accept input action
    public M8.InputAction selectAxisInput; //axis input to select prev/next (-1 = left/down, 1 = right/up)

    [Header("Select")]
    public GameObject selectActiveGO;
    public Image selectProcessDisplay; //while holding down, this is progressed
    public float selectProcessDelay = 1f; //delay before processing while pointer is down
    public float selectProcessRepeatDelay = 1f; //repeat call to process while pointer is down    

    public bool isProcess { get; private set; } //pointer remains down after delay

    private bool mIsSelected;

    protected override void OnEnable() {
        base.OnEnable();

        EventSystem es = EventSystem.current;
        if(es)
            SetSelected(es.currentSelectedGameObject == gameObject);
    }

    public override void OnSelect(BaseEventData eventData) {
        base.OnSelect(eventData);

        SetSelected(true);
    }

    public override void OnDeselect(BaseEventData eventData) {
        base.OnDeselect(eventData);

        SetSelected(false);
    }

    void SetSelected(bool selected) {
        mIsSelected = selected;
        if(selectActiveGO) selectActiveGO.SetActive(selected);
    }
}
