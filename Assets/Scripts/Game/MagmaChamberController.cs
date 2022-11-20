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
    public GameObject exitInstructGO;

    [Header("Furnace")]
    public GameObject furnaceMagmaGONone;
    public GameObject furnaceMagmaGOActive;
    public GameObject furnaceMagmaGOAction;
    public float furnaceMagmaActionDelay = 2.0f;

    [Header("Furnace Animation")]
    public M8.Animator.Animate furnaceAnimator;
    [M8.Animator.TakeSelector(animatorField = "furnaceAnimator")]
    public string furnaceTakeAction;

    [Header("Flame")]
    public GameObject flameMagmaGONone;
    public GameObject flameMagmaGOActive;

    [Header("Animation")]
    public M8.Animator.Animate animator;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeUIEnter;
    [M8.Animator.TakeSelector(animatorField = "animator")]
    public string takeUIExit;

    [Header("Audio")]
    [M8.SoundPlaylist]
    public string soundFurnaceProcess;

    private Selectable mSelectableActive;

    private float mFurnaceActionLastTime;
    private Coroutine mFurnaceActionRout;

    private bool mIsSoundFurnacePlaying;

    public void MineralProcess() {
        int mineralsCount = inventory.mineralsCount;

        inventory.ClearMineralsCount();

        PlayFurnaceSound();

        //machine animation
        if(furnaceAnimator && !string.IsNullOrEmpty(furnaceTakeAction))
            furnaceAnimator.Play(furnaceTakeAction);

        mFurnaceActionLastTime = Time.time;
        if(mFurnaceActionRout == null)
            mFurnaceActionRout = StartCoroutine(DoFurnaceAction());

        if(mineralsCount < inventory.magma.capacity)
            inventory.magma.count += inventory.magma.capacity;
        else
            inventory.magma.count += mineralsCount;

        RefreshFireDisplay();

        StartCoroutine(DoChangeInterface());

        //show exit
        exitInstructGO.SetActive(true);
    }

    public void RockProcess(InfoData dat) {
        if(dat.count > 0) {
            if(GlobalSettings.isUnlimitedResource) {
                inventory.magma.count = GlobalSettings.unlimitedResourceMagma;
            }
            else {
                dat.count--;

                int magmaValue = 1;

                inventory.magma.count += magmaValue;
            }

            criteria.InvokeUpdate(dat);
        }

        rockSelector.RefreshRock(dat);

        RefreshFireDisplay();

        PlayFurnaceSound();

        //machine animation
        if(furnaceAnimator && !string.IsNullOrEmpty(furnaceTakeAction))
            furnaceAnimator.Play(furnaceTakeAction);

        mFurnaceActionLastTime = Time.time;
        if(mFurnaceActionRout == null)
            mFurnaceActionRout = StartCoroutine(DoFurnaceAction());

        if(rockSelector.rockCount == 0)
            StartCoroutine(DoChangeInterface());
    }

    void PlayFurnaceSound() {
        if(!mIsSoundFurnacePlaying) {
            mIsSoundFurnacePlaying = true;
            M8.SoundPlaylist.instance.Play(soundFurnaceProcess, delegate (M8.GenericParams parms) { mIsSoundFurnacePlaying = false; }, null);
        }
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

        exitInstructGO.SetActive(false);

        EventSystem.current.SetSelectedGameObject(null);

        if(animator && !string.IsNullOrEmpty(takeUIEnter))
            animator.ResetTake(takeUIEnter);

        //initialize smelter and background
        RefreshFireDisplay();
        RefreshFurnaceDisplay();
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

    IEnumerator DoChangeInterface() {
        if(mSelectableActive) {
            var es = EventSystem.current;
            if(es)
                es.SetSelectedGameObject(null);

            mSelectableActive.interactable = false;

            if(animator && !string.IsNullOrEmpty(takeUIExit))
                yield return animator.PlayWait(takeUIExit);
        }

        RefreshInterfaces();

        StartCoroutine(DoEnterInterface());
    }

    IEnumerator DoFurnaceAction() {
        if(furnaceMagmaGOAction) furnaceMagmaGOAction.SetActive(true);
        if(furnaceMagmaGONone) furnaceMagmaGONone.SetActive(false);
        if(furnaceMagmaGOActive) furnaceMagmaGOActive.SetActive(false);

        while(Time.time - mFurnaceActionLastTime < furnaceMagmaActionDelay)
            yield return null;

        mFurnaceActionRout = null;

        RefreshFurnaceDisplay();
    }

    void RefreshFireDisplay() {
        bool isActive = inventory.magma.count > 0;

        if(flameMagmaGONone) flameMagmaGONone.SetActive(!isActive);
        if(flameMagmaGOActive) flameMagmaGOActive.SetActive(isActive);
    }

    void RefreshFurnaceDisplay() {
        if(mFurnaceActionRout != null) {
            StopCoroutine(mFurnaceActionRout);
            mFurnaceActionRout = null;
        }

        if(furnaceMagmaGOAction) furnaceMagmaGOAction.SetActive(false);

        bool isActive = inventory.magma.count > 0;

        if(furnaceMagmaGONone) furnaceMagmaGONone.SetActive(!isActive);
        if(furnaceMagmaGOActive) furnaceMagmaGOActive.SetActive(isActive);
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
