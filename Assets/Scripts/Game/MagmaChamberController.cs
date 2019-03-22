using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MagmaChamberController : GameModeController<MagmaChamberController> {
    [Header("Data")]
    public InventoryData inventory;
    public CriteriaData criteria;
    public M8.SceneAssetPath exitScene;

    [Header("UI")]
    public GameObject mineralsProcessorGO;
    public Selectable mineralsSelector;
    public GameObject rockProcessorGO;
    public RockSelectWidget rockSelector;
    public Button exitButton;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeUIEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeUIExit;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeMachineProcess;

    private Selectable mSelectableActive;

    public void MineralProcess() {
        int mineralsCount = inventory.mineralsCount;

        inventory.ClearMineralsCount();

        if(mineralsCount < inventory.magma.capacity)
            inventory.magma.count += inventory.magma.capacity;
        else
            inventory.magma.count += mineralsCount;

        //animation
        StartCoroutine(DoRockProcess());
    }

    public void RockProcess(InfoData dat) {
        if(dat.count > 0) {
            dat.count--;

            int magmaValue = 1;

            inventory.magma.count += magmaValue;

            criteria.InvokeUpdate(dat);
        }

        rockSelector.RefreshRock(dat);

        //if(rockSelector.rockCount == 0)
            //RefreshInterfaces();

        //animation
        StartCoroutine(DoRockProcess());
    }

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(rockSelector)
            rockSelector.processRockCallback -= RockProcess;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        mineralsProcessorGO.SetActive(false);
        mineralsSelector.interactable = false;

        rockProcessorGO.SetActive(false);
        rockSelector.interactable = false;
        rockSelector.processRockCallback += RockProcess;

        exitButton.onClick.AddListener(OnExitClicked);

        EventSystem.current.SetSelectedGameObject(null);

        if(animator && !string.IsNullOrEmpty(takeUIEnter))
            animator.ResetTake(takeUIEnter);

        //initialize smelter and background
        //animate smelter if magma > 0?
    }

    protected override IEnumerator Start() {
        yield return base.Start();
                
        RefreshInterfaces();

        //animation entrance
        if(mSelectableActive)
            StartCoroutine(DoEnterInterface());
        else {
            //TODO: dialog about requiring minerals or rocks to process
        }
    }

    void OnExitClicked() {
        exitScene.Load();
    }

    IEnumerator DoEnterInterface() {
        if(!mSelectableActive) //fail-safe
            yield break;

        mSelectableActive.interactable = false;

        if(animator && !string.IsNullOrEmpty(takeUIEnter))
            yield return animator.PlayWait(takeUIEnter);

        mSelectableActive.interactable = true;
        mSelectableActive.Select();
    }

    IEnumerator DoRockProcess() {
        if(mSelectableActive) {
            if(animator && !string.IsNullOrEmpty(takeUIExit))
                yield return animator.PlayWait(takeUIExit);
        }

        if(animator && !string.IsNullOrEmpty(takeMachineProcess))
            yield return animator.PlayWait(takeMachineProcess);

        RefreshInterfaces();

        StartCoroutine(DoEnterInterface());
    }

    void RefreshInterfaces() {
        int mineralsCounts = inventory.mineralsCount;
        int rocksCounts = inventory.rocksCount;

        mineralsProcessorGO.SetActive(false);
        rockProcessorGO.SetActive(false);

        mSelectableActive = null;

        //initialize minerals
        //initialize rocks
        if(mineralsCounts == 0 && rocksCounts == 0) {
            //no minerals or rocks, just exit
        }
        else if(mineralsCounts > 0) {
            //minerals
            mineralsProcessorGO.SetActive(true);
            mSelectableActive = mineralsSelector;
            //mineralsProcessor.Select();
        }
        else {
            //rocks
            rockProcessorGO.SetActive(true);
            mSelectableActive = rockSelector;
            //rockSelector.Select();
        }

        var exitNavigation = exitButton.navigation;

        if(mSelectableActive) {
            exitNavigation.mode = Navigation.Mode.Explicit;
            exitNavigation.selectOnUp = mSelectableActive;
            exitButton.navigation = exitNavigation;
        }
        else {
            exitNavigation.mode = Navigation.Mode.None;
            exitButton.Select();
        }
    }
}
