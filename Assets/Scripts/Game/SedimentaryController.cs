using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SedimentaryController : GameModeController<SedimentaryController> {
    [Header("Data")]
    public InventoryData inventory;
    public CriteriaData criteria;
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

    [Header("Rock Erosion")]
    public SequenceInfo rockErosionSequence;
    public Button erosionButton;
    public Button erosionFinishButton;
    public int erosionCapacity = 4; //how many times can we erode the rock

    [Header("Compact/Cement")]
    public SequenceInfo compactCementSequence;
    
    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;

    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;
    
    private int mErosionCount = 0;
    private bool mIsErosionFinish;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();
    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    private List<InfoData> mSourceSelects;
    private int mSourceSelectCount; //required selection to make

    private Dictionary<GrainSize, List<RockSedimentaryData>> mClasticOutput;
    private Dictionary<InfoData, List<RockSedimentaryData>> mOrganicOutput;

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        sourceSelect.processRockCallback -= OnSourceSelectProcess;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        processSequence.Init();
        sourceSequence.Init();
        rockErosionSequence.Init();
        compactCementSequence.Init();
        rockResultSequence.Init();

        mClasticOutput = new Dictionary<GrainSize, List<RockSedimentaryData>>();
        mOrganicOutput = new Dictionary<InfoData, List<RockSedimentaryData>>();

        for(int i = 0; i < inventory.rocksSedimentary.Length; i++) {
            var rock = inventory.rocksSedimentary[i];
                                                
            if(rock.isOrganicOrChemicallyFormed) {
                for(int j = 0; j < rock.input.Length; j++) {
                    List<RockSedimentaryData> rockList = null;
                    if(!mOrganicOutput.TryGetValue(rock.input[j], out rockList)) {
                        rockList = new List<RockSedimentaryData>();
                        mOrganicOutput.Add(rock.input[j], rockList);
                    }

                    rockList.Add(rock);
                }
            }
            else {
                List<RockSedimentaryData> rockList = null;
                if(!mClasticOutput.TryGetValue(rock.grainSize, out rockList)) {
                    rockList = new List<RockSedimentaryData>();
                    mClasticOutput.Add(rock.grainSize, rockList);
                }

                rockList.Add(rock);
            }
        }

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
        int rockTypeCount = inventory.rockTypeValidCount;
        int organicCount = inventory.organicsCount;

        processRockButton.interactable = rockCount > 0 && rockTypeCount >= inventory.sedimentaryRockCount;
        processOrganicButton.interactable = organicCount > 0;

        yield return processSequence.Enter();

        if(!(processRockButton.interactable || processOrganicButton.interactable))
            exitButton.Select();
        else if(processRockButton.interactable)
            processRockButton.Select();
        else
            processOrganicButton.Select();
    }

    delegate IEnumerator routFunc();

    IEnumerator DoSourceSelect(RockSelectWidget.Filter filter, routFunc nextRoutFunc) {
        ClearSelection();

        yield return processSequence.Exit();

        //setup source select widget
        sourceSelect.inventoryFilter = filter;
        sourceSelect.Refresh(true, null);
        //

        yield return sourceSequence.Enter();

        sourceSelect.Select();

        if(mSourceSelects != null) mSourceSelects.Clear();
        else mSourceSelects = new List<InfoData>();

        //wait for source to be selected
        while(mSourceSelects.Count < mSourceSelectCount)
            yield return null;

        //deposit
        for(int i = 0; i < mSourceSelects.Count; i++) {
            if(mSourceSelects[i].count > 0)
                mSourceSelects[i].count--;
        }

        ClearSelection();

        yield return sourceSequence.Exit();

        StartCoroutine(nextRoutFunc());
    }

    IEnumerator DoErosion() {
        yield return rockErosionSequence.Enter();

        erosionButton.interactable = true;
        erosionFinishButton.interactable = false;

        erosionButton.Select();

        mErosionCount = 0;
        mIsErosionFinish = false;
        while(!mIsErosionFinish)
            yield return null;

        yield return rockErosionSequence.Exit();

        StartCoroutine(DoCompactCementationClastic());
    }

    IEnumerator DoCompactCementationClastic() {
        yield return compactCementSequence.Enter();
                                                
        //clear out source, decrement count for each item
        if(mSourceSelects != null) {
            for(int i = 0; i < mSourceSelects.Count; i++) {
                if(mSourceSelects[i].count > 0) {
                    mSourceSelects[i].count--;

                    criteria.InvokeUpdate(mSourceSelects[i]);
                }
            }

            mSourceSelects.Clear();
        }

        //setup rock output
        var grainType = (GrainSize)(mErosionCount - 1);
        var rockList = mClasticOutput[grainType];
        var rockOutput = rockList[Random.Range(0, rockList.Count)];
        rockOutput.count += inventory.sedimentaryRockCount;
        mRockResultList.Clear();
        mRockResultList.Add(rockOutput);

        criteria.InvokeUpdate(rockOutput);

        //setup compaction material

        //do compact
        //do cementing

        yield return compactCementSequence.Exit();

        StartCoroutine(DoRockResult());
    }

    IEnumerator DoCompactCementationOrganic() {
        yield return compactCementSequence.Enter();

        mRockResultList.Clear();

        //setup rock output, assume only one source
        if(mSourceSelects.Count == 0) {
            //fail-safe
            Debug.LogWarning("No source selected.");
            yield return compactCementSequence.Exit();
            StartCoroutine(DoProcessSelect());
            yield break;
        }

        var source = mSourceSelects[0];
        
        var rockList = mOrganicOutput[source];
        var rockOutput = rockList[Random.Range(0, rockList.Count)];
        rockOutput.count += inventory.sedimentaryRockCount;
        mRockResultList.Add(rockOutput);

        criteria.InvokeUpdate(rockOutput);

        //clear out source
        if(source.count > 0)
            source.count--;

        mSourceSelects.Clear();

        //setup compaction material

        //do compact
        //do cementing

        yield return compactCementSequence.Exit();

        StartCoroutine(DoRockResult());
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

        StartCoroutine(DoProcessSelect());
    }

    void OnProcessRockClick() {
        mSourceSelectCount = inventory.sedimentaryRockCount;

        StartCoroutine(DoSourceSelect(rockFilter, DoErosion));
    }

    void OnProcessOrganicClick() {
        mSourceSelectCount = 1;

        StartCoroutine(DoSourceSelect(organicFilter, DoCompactCementationOrganic));
    }

    void OnErosionClick() {
        if(!mIsErosionFinish) {
            erosionFinishButton.interactable = true;

            mErosionCount++;
            if(mErosionCount == erosionCapacity) {
                erosionButton.interactable = false;

                erosionFinishButton.Select();
            }
        }
    }

    void OnErosionFinishClick() {
        mIsErosionFinish = true;
    }

    void OnSourceSelectProcess(InfoData dat) {
        if(!mSourceSelects.Contains(dat)) {
            mSourceSelects.Add(dat);

            if(mSourceSelects.Count < mSourceSelectCount) //refresh selection with hide filter
                sourceSelect.Refresh(false, mSourceSelects);

            //drop source animation
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
