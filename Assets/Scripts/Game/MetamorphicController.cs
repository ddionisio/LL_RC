using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MetamorphicController : GameModeController<MagmaCoolerController> {
    [Header("Data")]
    public InventoryData inventory;
    public CriteriaData criteria;

    [Header("Rock Select")]
    public SequenceInfo rockSelectSequence;
    public RockSelectWidget rockSelect;

    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;

    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;

    private RockData mRockSelect;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();

    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(rockSelect)
            rockSelect.processRockCallback += OnRockSelect;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        rockSelectSequence.Init();
        rockResultSequence.Init();

        rockSelect.processRockCallback += OnRockSelect;

        rockResultContinueButton.onClick.AddListener(OnRockResultContinueClick);
        exitButton.onClick.AddListener(OnExitClick);
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        StartCoroutine(DoRockSelect());
    }

    IEnumerator DoRockSelect() {
        var rockCount = inventory.rocksCount;

        if(rockCount > 0) {
            rockSelect.Refresh(true, null);

            yield return rockSelectSequence.Enter();

            rockSelect.Select();

            mRockSelect = null;
            while(!mRockSelect)
                yield return null;

            yield return rockSelectSequence.Exit();

            //animation of transformation

            //generate rock
            if(mRockSelect.count > 0) {
                mRockSelect.count--;
                criteria.InvokeUpdate(mRockSelect);
            }

            mRockSelect.metaOutput.count++;
            criteria.InvokeUpdate(mRockSelect.metaOutput);

            mRockResultList.Clear();
            mRockResultList.Add(mRockSelect.metaOutput);
            //

            StartCoroutine(DoRockResult());
        }
        else {
            //notify that rocks are needed.

            exitButton.Select();
        }
    }

    IEnumerator DoRockResult() {
        yield return rockResultSequence.Enter();

        //fill rock list widget
        rockResultWidget.Init(mRockResultList, null);

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
        
        StartCoroutine(DoRockSelect());
    }

    void OnRockSelect(InfoData dat) {
        mRockSelect = (RockData)dat;
    }

    void OnRockResultContinueClick() {
        ClearSelection();
        mIsRockResultContinue = true;
    }

    void OnExitClick() {
        exitScene.Load();
    }

    void ClearSelection() {
        var es = EventSystem.current;
        if(es)
            es.SetSelectedGameObject(null);
    }
}
