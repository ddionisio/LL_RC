﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SedimentaryController : GameModeController<SedimentaryController> {
    [Header("Data")]
    public InventoryData inventory;
    [M8.EnumMask]
    public RockSelectWidget.Filter rockFilter = RockSelectWidget.Filter.Igneous | RockSelectWidget.Filter.Sedimentary | RockSelectWidget.Filter.Metamorphic;
    [M8.EnumMask]
    public RockSelectWidget.Filter organicFilter = RockSelectWidget.Filter.Organics;
    
    [Header("Process")]
    public SequenceInfo processSequence;
    public Button processRockButton;
    public Button processOrganicButton;

    [Header("Source Select")]
    public SequenceInfo sourceSequence;
    public RockSelectWidget sourceSelect;

    [Header("Rock")]
    public SequenceInfo rockErosionSequence;
    public Button erosionButton;
    public Button erosionFinishButton;

    [Header("Compact/Cement")]
    public SequenceInfo compactCementSequence;
    
    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;

    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;

    [Header("Signals")]
    public M8.Signal signalRockResultUpdate;

    private bool mIsSourceSelected;
    private InfoData mSourceSelected;

    private bool mIsErosionFinish;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();
    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        sourceSelect.processRockCallback -= OnSourceSelectProcess;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        processRockButton.onClick.AddListener(OnProcessRockClick);
        processOrganicButton.onClick.AddListener(OnProcessOrganicClick);

        sourceSelect.processRockCallback += OnSourceSelectProcess;

        erosionButton.onClick.AddListener(OnErosionClick);
        erosionFinishButton.onClick.AddListener(OnErosionFinishClick);

        rockResultContinueButton.onClick.AddListener(OnRockResultContinueClick);

        exitButton.onClick.AddListener(OnExitClicked);
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        StartCoroutine(DoProcessSelect());
    }

    IEnumerator DoProcessSelect() {
        ClearSelection();

        //determine selections
        int rockCount = inventory.rocksCount;
        int organicCount = inventory.organicsCount;

        processRockButton.interactable = rockCount == 0;
        processOrganicButton.interactable = organicCount == 0;

        yield return processSequence.Enter();

        if(rockCount == 0 && organicCount == 0)
            exitButton.Select();
        else if(rockCount > 0)
            processRockButton.Select();
        else if(organicCount > 0)
            processOrganicButton.Select();
    }

    delegate IEnumerator routFunc();

    IEnumerator DoSourceSelect(RockSelectWidget.Filter filter, routFunc nextRoutFunc) {
        ClearSelection();

        yield return processSequence.Exit();

        //setup source select widget
        sourceSelect.inventoryFilter = filter;
        sourceSelect.Refresh(true);
        //

        yield return sourceSequence.Enter();

        sourceSelect.Select();

        mIsSourceSelected = false;
        mSourceSelected = null;

        //wait for source to be selected
        while(!mIsSourceSelected)
            yield return null;

        //deposit
        if(mSourceSelected.count > 0)
            mSourceSelected.count--;

        ClearSelection();

        yield return sourceSequence.Exit();

        StartCoroutine(nextRoutFunc());
    }

    IEnumerator DoErosion() {
        yield return rockErosionSequence.Enter();

        erosionFinishButton.interactable = false;

        erosionButton.Select();

        mIsErosionFinish = false;
        while(!mIsErosionFinish)
            yield return null;

        yield return rockErosionSequence.Exit();

        StartCoroutine(DoCompactCementation());
    }

    IEnumerator DoCompactCementation() {
        yield return compactCementSequence.Enter();

        //determine drops

        //do compact
        //do cementing

        //generate rock
        mRockResultList.Clear();

        yield return compactCementSequence.Exit();

        StartCoroutine(DoRockResult());
    }

    IEnumerator DoRockResult() {
        yield return rockResultSequence.Enter();

        //fill rock list widget
        rockResultWidget.Init(mRockResultList, rockResultContinueButton);

        yield return rockResultSequence.Enter();

        //check which rocks have not been seen, display info for each
        for(int i = 0; i < mRockResultList.Count; i++) {
            var rock = mRockResultList[i];
            if(!rock.isSeen) {
                mRockModalParms[ModalInfo.parmInfoData] = rock;

                M8.ModalManager.main.Open(rock.modal, mRockModalParms);

                while(M8.ModalManager.main.isBusy || M8.ModalManager.main.IsInStack(rock.modal))
                    yield return null;
            }
        }

        //wait for rock continue
        rockResultContinueButton.Select();
        mIsRockResultContinue = false;
        while(!mIsRockResultContinue)
            yield return null;

        yield return rockResultSequence.Exit();

        if(signalRockResultUpdate)
            signalRockResultUpdate.Invoke();

        StartCoroutine(DoProcessSelect());
    }

    void OnProcessRockClick() {
        StartCoroutine(DoSourceSelect(rockFilter, DoErosion));
    }

    void OnProcessOrganicClick() {
        StartCoroutine(DoSourceSelect(organicFilter, DoCompactCementation));
    }

    void OnErosionClick() {
        if(!mIsErosionFinish) {

        }
    }

    void OnErosionFinishClick() {
        mIsErosionFinish = true;
    }

    void OnSourceSelectProcess(InfoData dat) {
        if(!mIsSourceSelected) {
            mIsSourceSelected = true;
            mSourceSelected = dat;
        }
    }

    void OnRockResultContinueClick() {
        ClearSelection();
        mIsRockResultContinue = true;
    }

    void OnExitClicked() {
        exitScene.Load();
    }

    void ClearSelection() {
        var es = EventSystem.current;
        if(es)
            es.SetSelectedGameObject(null);
    }
}
