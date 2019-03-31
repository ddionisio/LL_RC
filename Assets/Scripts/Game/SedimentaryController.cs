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

    [System.Serializable]
    public class RockSpawnInfo {
        public RockSpawnController spawner;
        public SpriteShapeController layerSpriteCtrl;
        public SpriteShapeColor layerSpriteColor;
        public float fadeInStartDelay = 1f;

        public bool isSpawning { get; private set; }

        public void Reset() {
            layerSpriteCtrl.gameObject.SetActive(false);
            isSpawning = false;
        }

        public IEnumerator Spawn(SpriteShape spriteShape, float delay) {
            layerSpriteCtrl.spriteShape = spriteShape;
            layerSpriteCtrl.gameObject.SetActive(true);
            
            var layerClr = layerSpriteColor.color;
            layerClr.a = 0f;

            layerSpriteColor.color = layerClr;

            isSpawning = true;

            float rockSpawnDelay = delay / spawner.capacity;

            spawner.SpawnAll(spriteShape, rockSpawnDelay);

            if(fadeInStartDelay > 0f)
                yield return new WaitForSeconds(fadeInStartDelay);

            var curTime = 0f;
            while(isSpawning && curTime < delay) {
                yield return null;

                curTime += Time.deltaTime;
                var t = Mathf.Clamp01(curTime / delay);

                layerClr.a = t;
                layerSpriteColor.color = layerClr;
            }
                        
            isSpawning = false;
        }
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
    public ParticleSystem sourceWindFX;
    public float sourcePostDelay = 1f;

    [Header("Source Spawn")]
    public RockSpawnInfo[] sourceSpawners;
    public int sourceSpawnerOrganicIndex = 1;
    public float sourceSpawnDelay = 1f;

    [Header("Rock Erosion")]
    public SequenceInfo rockErosionSequence;
    public Button erosionButton;
    public Button erosionFinishButton;

    public SpriteShapeController erosionSpriteLayer;
    public SpriteShape[] erosionSpriteShapes;

    public int erosionCapacity = 4;

    public M8.Animator.Animate erosionAnimator;
    [M8.Animator.TakeSelector(animatorField = "erosionAnimator")]
    public string erosionTakeEnter; //crush rocks
    [M8.Animator.TakeSelector(animatorField = "erosionAnimator")]
    public string erosionTakeExit; //crush rocks

    public float erosionDelay = 0.5f;
        
    public Text erosionGrainText;
    [M8.Localize]
    public string erosionGrainSizeTitleTextRef;

    [Header("Compact/Cement")]
    public SpriteShapeController sedimentaryRockSprite;
    public M8.Animator.Animate compactCementAnimator;
    [M8.Animator.TakeSelector(animatorField = "compactCementAnimator")]
    public string compactCementTakePlay; //crush rocks
    [M8.Animator.TakeSelector(animatorField = "compactCementAnimator")]
    public string compactCementTakeFadeOut;

    public float compactCementDelay = 1.5f;

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

        for(int i = 0; i < sourceSpawners.Length; i++)
            sourceSpawners[i].Reset();

        if(erosionAnimator) erosionAnimator.gameObject.SetActive(false);
        if(compactCementAnimator) compactCementAnimator.gameObject.SetActive(false);

        erosionSpriteLayer.gameObject.SetActive(false);

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
        yield return DoSourceSpawnWait();

        yield return new WaitForSeconds(sourcePostDelay);

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
        if(erosionGrainText)
            erosionGrainText.text = string.Format("{0}: ----", M8.Localize.Get(erosionGrainSizeTitleTextRef));

        yield return rockErosionSequence.Enter();

        erosionFinishButton.interactable = false;

        if(erosionAnimator) {
            if(!string.IsNullOrEmpty(erosionTakeEnter))
                erosionAnimator.ResetTake(erosionTakeEnter);

            erosionAnimator.gameObject.SetActive(true);
        }
                
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

            mErosionCount++;

            //do erosion enter
            if(erosionAnimator && !string.IsNullOrEmpty(erosionTakeEnter))
                yield return erosionAnimator.PlayWait(erosionTakeEnter);

            //apply erosion level
            int erosionInd = mErosionCount - 1;

            erosionSpriteLayer.spriteShape = erosionSpriteShapes[erosionInd];

            if(erosionInd == 0) {
                //clear out rock layer display
                for(int i = 0; i < sourceSpawners.Length; i++)
                    sourceSpawners[i].Reset();

                erosionSpriteLayer.gameObject.SetActive(true);
            }
            //

            //change text
            if(erosionGrainText) {
                var grainType = (GrainSize)(mErosionCount - 1);
                erosionGrainText.text = string.Format("{0}: {1}", M8.Localize.Get(erosionGrainSizeTitleTextRef), M8.Localize.Get(RockSedimentaryData.GetGrainSizeTextRef(grainType)));
            }

            yield return new WaitForSeconds(erosionDelay);

            //erosion exit
            if(erosionAnimator && !string.IsNullOrEmpty(erosionTakeExit))
                yield return erosionAnimator.PlayWait(erosionTakeExit);

            if(mErosionCount > 0) //ready to proceed
                erosionFinishButton.interactable = true;

            if(mErosionCount == erosionCapacity)
                break;
        }

        if(erosionAnimator) erosionAnimator.gameObject.SetActive(false);

        erosionButton.interactable = false;

        //finish needs to be pressed?
        if(!mIsErosionFinish) {
            erosionFinishButton.Select();
            erosionFinishButton.interactable = true;
            
            while(!mIsErosionFinish)
                yield return null;
        }

        erosionFinishButton.interactable = false;

        yield return rockErosionSequence.Exit();
        
        StartCoroutine(DoCompactCementation());
    }

    IEnumerator DoSourceSpawnWait() {        
        while(true) {
            bool isSpawning = false;
            for(int i = 0; i < sourceSpawners.Length; i++) {
                if(sourceSpawners[i].isSpawning) {
                    isSpawning = true;
                    break;
                }
            }

            if(!isSpawning)
                break;

            yield return null;
        }
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

        //clear out previous mode
        switch(mCurMode) {
            case SourceMode.Rock:
                erosionSpriteLayer.gameObject.SetActive(false);
                break;
            case SourceMode.Organic:
                //clear out rock layer display
                for(int i = 0; i < sourceSpawners.Length; i++)
                    sourceSpawners[i].Reset();
                break;
        }        
                
        mRockResultList.Clear();
        mRockResultList.Add(rockOutput);

        criteria.InvokeUpdate(rockOutput);

        yield return new WaitForSeconds(compactCementDelay);

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

        int rockInd;

        switch(mCurMode) {
            case SourceMode.Rock:
                rockInd = mSourceSelects.Count;
                if(rockInd >= sourceSpawners.Length)
                    rockInd = sourceSpawners.Length - 1;
                break;
            case SourceMode.Organic: //use a fixed index
                rockInd = sourceSpawnerOrganicIndex;
                break;
            default:
                rockInd = 0;
                break;
        }

        var rockSpawner = sourceSpawners[rockInd];

        dat.count--;
        criteria.InvokeUpdate(dat);

        mSourceSelects.Add(dat);

        sourceSelect.Refresh(false, null);//sourceSelect.Refresh(false, mSourceSelects);

        if(dat is RockData) {
            var rockData = dat as RockData;
            StartCoroutine(rockSpawner.Spawn(rockData.spriteShape, sourceSpawnDelay));
        }
        else if(dat is OrganicData) {
            var organicDat = dat as OrganicData;
            StartCoroutine(rockSpawner.Spawn(organicDat.spriteShape, sourceSpawnDelay));
        }

        if(sourceWindFX) sourceWindFX.Play();
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
