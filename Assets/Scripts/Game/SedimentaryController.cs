using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SedimentaryController : GameModeController<SedimentaryController> {
    public enum SourceMode {
        None,
        Rock,
        Organic
    }

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

    [Header("Rock Spawn")]
    public RockSpawnController rockSpawner;

    [Header("Rock Erosion")]
    public SequenceInfo rockErosionSequence;
    public Button erosionButton;
    public Button erosionFinishButton;
    public int erosionCapacity = 4; //how many times can we erode the rock
    public int erosionBreakCount = 3; //how many times to do break animation
    public M8.Signal erosionSignalRockJump;
    public float erosionJumpDelay = 0.3f;

    public M8.Animator.Animate erosionAnimator;
    [M8.Animator.TakeSelector(animatorField = "erosionAnimator")]
    public string erosionTakeEnter; //crush rocks
    [M8.Animator.TakeSelector(animatorField = "erosionAnimator")]
    public string erosionTakeExit; //crush rocks

    [Header("Compact/Cement")]
    public SpriteShapeController sedimentaryRockSprite;
    public M8.Animator.Animate compactCementAnimator;
    [M8.Animator.TakeSelector(animatorField = "compactCementAnimator")]
    public string compactCementTakePlay; //crush rocks
    public string compactCementTakeFadeOut;

    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;

    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;

    private bool mIsErosionClicked;
    private int mErosionCount = 0;
    private bool mIsErosionFinish;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();
    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    private List<InfoData> mSourceSelects;

    private Dictionary<GrainSize, List<RockSedimentaryData>> mClasticOutput;
    private Dictionary<InfoData, List<RockSedimentaryData>> mOrganicOutput;
    
    private SourceMode mCurMode = SourceMode.None;

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        //undo counts for selects if not cleared
        if(mSourceSelects != null && mSourceSelects.Count > 0) {
            for(int i = 0; i < mSourceSelects.Count; i++)
                mSourceSelects[i].count++;

            mSourceSelects.Clear();
        }

        sourceSelect.processRockCallback -= OnSourceSelectProcess;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        processSequence.Init();
        sourceSequence.Init();
        rockErosionSequence.Init();
        rockResultSequence.Init();

        if(erosionAnimator) erosionAnimator.gameObject.SetActive(false);
        if(compactCementAnimator) compactCementAnimator.gameObject.SetActive(false);
        
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

        //NOTE: uncomment to restrict to select one type per slot

        //determine selections
        int rockCount = inventory.rocksCount;
        //int rockTypeCount = inventory.rockTypeValidCount;
        int organicCount = inventory.organicsCount;

        processRockButton.interactable = rockCount > 0;// && rockTypeCount >= inventory.sedimentaryRockCount;
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

    IEnumerator DoSourceSelect() {
        ClearSelection();

        yield return processSequence.Exit();

        int sourceSelectCount;

        //setup source select widget
        switch(mCurMode) {
            case SourceMode.Rock:
                sourceSelectCount = inventory.sedimentaryRockCount;
                sourceSelect.inventoryFilter = rockFilter;
                break;
            case SourceMode.Organic:
                sourceSelectCount = 1;
                sourceSelect.inventoryFilter = organicFilter;
                break;
            default: //fail-safe
                sourceSelectCount = 0;
                break;
        }

        sourceSelect.Refresh(true, null);
        //

        yield return sourceSequence.Enter();

        sourceSelect.Select();

        if(mSourceSelects != null) mSourceSelects.Clear();
        else mSourceSelects = new List<InfoData>();

        //wait for source to be selected
        while(mSourceSelects.Count < sourceSelectCount)
            yield return null;

        ClearSelection();

        yield return sourceSequence.Exit();

        //wait for rock spawn to be finish and all rocks asleep
        yield return DoRockSpawnWait();

        switch(mCurMode) {
            case SourceMode.Rock:
                StartCoroutine(DoErosion());
                break;
            case SourceMode.Organic:
                StartCoroutine(DoCompactCementation());
                break;
            default: //fail-safe
                StartCoroutine(DoProcessSelect());
                break;
        }
    }

    IEnumerator DoErosion() {
        yield return rockErosionSequence.Enter();

        erosionFinishButton.interactable = false;

        if(erosionAnimator) erosionAnimator.gameObject.SetActive(true);

        mErosionCount = 0;
        mIsErosionFinish = false;

        while(true) {
            erosionButton.interactable = true;
            erosionButton.Select();

            //wait for a click
            mIsErosionClicked = false;
            while(!mIsErosionClicked && !mIsErosionFinish)
                yield return null;

            if(mIsErosionFinish)
                break;

            erosionButton.interactable = false;

            int rockInd = mErosionCount;
            int rockNextInd = rockInd + 1;

            if(rockNextInd < rockSpawner.rockTemplates.Length) {
                int rockCount = rockSpawner.GetActiveCount(rockInd);

                int rockBreakCount = rockCount / erosionBreakCount;

                while(rockSpawner.GetActiveCount(rockInd) > 0) {
                    if(erosionAnimator && !string.IsNullOrEmpty(erosionTakeEnter))
                        erosionAnimator.Play(erosionTakeEnter);

                    yield return new WaitForSeconds(0.3f); //temp

                    erosionSignalRockJump.Invoke();
                    yield return new WaitForSeconds(erosionJumpDelay);

                    //break rocks
                    for(int i = 0; i < rockBreakCount; i++)
                        rockSpawner.SpawnSplitFrom(rockInd);

                    yield return new WaitForSeconds(0.3f); //temp

                    if(erosionAnimator && !string.IsNullOrEmpty(erosionTakeExit))
                        erosionAnimator.Play(erosionTakeExit);
                }

                //do animation, break rocks
            }

            mErosionCount++;
            if(mErosionCount > 0)
                erosionFinishButton.interactable = true;

            if(mErosionCount == erosionCapacity) {
                mIsErosionFinish = true;
                break;
            }
        }

        if(erosionAnimator) erosionAnimator.gameObject.SetActive(false);

        erosionButton.interactable = false;
        erosionFinishButton.interactable = false;

        yield return rockErosionSequence.Exit();

        //wait for rocks to be asleep
        yield return DoRockSpawnWait();

        StartCoroutine(DoCompactCementation());
    }

    IEnumerator DoRockSpawnWait() {
        while(rockSpawner.isSpawnQueueBusy)
            yield return null;

        float mLastTime = Time.time;
        while(!rockSpawner.isAllRockAsleep && Time.time - mLastTime < 2f)
            yield return null;
    }

    IEnumerator DoCompactCementation() {
        //setup output
        RockSedimentaryData rockOutput = null;
        List<RockSedimentaryData> rockList = null;

        switch(mCurMode) {
            case SourceMode.Rock:
                var grainType = (GrainSize)(mErosionCount - 1);
                rockList = mClasticOutput[grainType];
                break;
            case SourceMode.Organic:
                var source = mSourceSelects[0];
                rockList = mOrganicOutput[source];
                break;
        }

        mSourceSelects.Clear();
                
        if(rockList != null) {
            rockOutput = rockList[Random.Range(0, rockList.Count)];
            rockOutput.count += inventory.sedimentaryRockCount;
        }
        //

        //setup display
        if(rockOutput) {
            if(sedimentaryRockSprite)
                sedimentaryRockSprite.spriteShape = rockOutput.spriteShape;
        }

        //play animation
        if(compactCementAnimator) {
            compactCementAnimator.gameObject.SetActive(true);

            if(!string.IsNullOrEmpty(compactCementTakePlay))
                yield return compactCementAnimator.PlayWait(compactCementTakePlay);
        }

        rockSpawner.Clear();

        mRockResultList.Clear();
        mRockResultList.Add(rockOutput);

        criteria.InvokeUpdate(rockOutput);

        StartCoroutine(DoRockResult());
    }
    
    IEnumerator DoRockResult() {
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

        if(compactCementAnimator) {
            if(!string.IsNullOrEmpty(compactCementTakeFadeOut))
                yield return compactCementAnimator.PlayWait(compactCementTakeFadeOut);

            compactCementAnimator.gameObject.SetActive(false);
        }

        StartCoroutine(DoProcessSelect());
    }

    void OnProcessRockClick() {
        mCurMode = SourceMode.Rock;

        StartCoroutine(DoSourceSelect());
    }

    void OnProcessOrganicClick() {
        mCurMode = SourceMode.Organic;

        StartCoroutine(DoSourceSelect());
    }

    void OnErosionClick() {
        if(!mIsErosionFinish) {
            mIsErosionClicked = true;
        }
    }

    void OnErosionFinishClick() {
        mIsErosionFinish = true;
    }

    void OnSourceSelectProcess(InfoData dat) {
        //NOTE: uncomment to restrict to select one type per slot
        //if(!mSourceSelects.Contains(dat)) {
        
        dat.count--;
        criteria.InvokeUpdate(dat);

        mSourceSelects.Add(dat);

        sourceSelect.Refresh(false, null);//sourceSelect.Refresh(false, mSourceSelects);

        if(dat is RockData) {
            var rockData = dat as RockData;
            rockSpawner.SpawnQueue(0, rockData.spriteShape, 1);
        }
        else if(dat is OrganicData) {
            var organicDat = dat as OrganicData;
            rockSpawner.SpawnQueue(2, organicDat.spriteShape, rockSpawner.rockTemplates[2].capacity);
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
