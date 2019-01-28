using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MagmaCoolerController : GameModeController<MagmaCoolerController> {
    [Header("Data")]
    public InventoryData inventory;
    public RockIgneousData[] intrusiveRocks; //which type of rock to produce based on cooling delay
    public RockIgneousData[] extrusiveRocks;
    public int extrusiveResultCount = 3;
    
    [Header("Process")]
    public SequenceInfo processSequence;

    public Button processIntrusiveButton;
    public int processIntrusiveDisableProgress; //which progress this is disabled

    public Button processExtrusiveButton;
    public int processExtrusiveDisableProgress; //which progress this is disabled

    [Header("Intrusive")]
    public SequenceInfo intrusiveSequence;
    public Button intrusiveReturnButton;
    public Button intrusiveProceedButton;

    [Header("Cooling")]
    public SequenceInfo coolingSequence;
    public Button coolingStopButton;
    public Slider coolingSlider;
    public float coolingDelay = 1f; //delay per rock
    public float coolingSegmentDelay = 0.5f; //delay before resuming cooling
        
    [Header("Extrusive")]
    public SequenceInfo extrusiveSequence;
    public Button extrusiveReturnButton;
    public Button extrusiveProceedButton;

    [Header("Rock Result")]
    public SequenceInfo rockResultSequence;
    public InfoDataListWidget rockResultWidget;
    public Button rockResultContinueButton;
    
    [Header("Exit")]
    public Button exitButton;
    public M8.SceneAssetPath exitScene;

    [Header("Signals")]
    public M8.Signal signalRockResultUpdate;

    private bool mIsCoolingStop;

    private bool mIsRockResultContinue;
    private List<InfoData> mRockResultList = new List<InfoData>();
    private M8.GenericParams mRockModalParms = new M8.GenericParams();

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        processSequence.Init();
        intrusiveSequence.Init();
        coolingSequence.Init();
        extrusiveSequence.Init();
        rockResultSequence.Init();

        processIntrusiveButton.onClick.AddListener(OnProcessIntrusive);
        processExtrusiveButton.onClick.AddListener(OnProcessExtrusive);

        intrusiveProceedButton.onClick.AddListener(OnIntrusiveProceed);
        intrusiveReturnButton.onClick.AddListener(OnIntrusiveBack);

        coolingStopButton.onClick.AddListener(OnCoolingStop);

        extrusiveProceedButton.onClick.AddListener(OnExtrusiveProceed);
        extrusiveReturnButton.onClick.AddListener(OnExtrusiveBack);

        rockResultContinueButton.onClick.AddListener(OnRockResultContinue);

        exitButton.onClick.AddListener(OnExit);
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        StartCoroutine(DoProcessSelect());
    }

    IEnumerator DoProcessSelect() {
        //check if there's magma
        if(inventory.magma.count > 0) {
            int progress = LoLManager.isInstantiated ? LoLManager.instance.curProgress : 0;

            //determine if intrusive/extrusive locked
            processIntrusiveButton.interactable = processIntrusiveDisableProgress != progress;
            processExtrusiveButton.interactable = processExtrusiveDisableProgress != progress;
            //

            yield return processSequence.Enter();

            if(processIntrusiveButton.interactable)
                processIntrusiveButton.Select();
            else if(processExtrusiveButton.interactable)
                processExtrusiveButton.Select();
            else
                exitButton.Select();
        }
        else { //show nothing, just exit
            exitButton.Select();
        }
    }

    //intrusive process    
    IEnumerator DoProcessToIntrusiveProceed() {
        ClearSelection();

        yield return processSequence.Exit();

        yield return intrusiveSequence.Enter();
        
        intrusiveProceedButton.Select();
    }

    IEnumerator DoIntrusiveBack() {
        ClearSelection();

        yield return intrusiveSequence.Exit();

        StartCoroutine(DoProcessSelect());
    }

    IEnumerator DoIntrusiveProceed() {
        ClearSelection();

        coolingSlider.normalizedValue = 0f;
                
        yield return intrusiveSequence.Exit();

        //do intrusive fill
        yield return new WaitForSeconds(0.5f);

        //cooling
        yield return coolingSequence.Enter();
        
        coolingStopButton.Select();
                
        int intrusiveRockInd = 0;
        float curTime = 0f;
        float curPauseTime = 0f;
        float coolingSegment = 1.0f / intrusiveRocks.Length;

        //do cooling and fill animation
        mIsCoolingStop = false;
        while(!mIsCoolingStop) {
            if(intrusiveRockInd < intrusiveRocks.Length - 1) {                
                if(curTime < coolingDelay) {
                    curTime += Time.deltaTime;
                    float t = Mathf.Clamp01(curTime / coolingDelay);
                    coolingSlider.normalizedValue = (intrusiveRockInd + t) * coolingSegment;
                }
                else if(curPauseTime < coolingSegmentDelay) {
                    curPauseTime += Time.deltaTime;
                }
                else {
                    curTime = 0f;
                    curPauseTime = 0f;
                    intrusiveRockInd++;
                }
            }

            yield return null;
        }

        yield return coolingSequence.Exit();
        //

        //generate rock
        mRockResultList.Clear();

        var rock = intrusiveRocks[intrusiveRockInd];
        rock.count++;

        var magmaCount = inventory.magma.count - inventory.magma.rockValue;
        if(magmaCount < 0)
            magmaCount = 0;
        inventory.magma.count = magmaCount;

        mRockResultList.Add(rock);
        //

        //show rock result
        StartCoroutine(DoRockResult());
    }
    //

    //extrusive process
    IEnumerator DoProcessToExtrusiveProceed() {
        ClearSelection();

        yield return processSequence.Exit();

        yield return extrusiveSequence.Enter();
        
        extrusiveProceedButton.Select();
    }

    IEnumerator DoExtrusiveBack() {
        ClearSelection();

        yield return extrusiveSequence.Exit();

        StartCoroutine(DoProcessSelect());
    }

    IEnumerator DoExtrusiveProceed() {
        ClearSelection();

        yield return extrusiveSequence.Exit();

        //do extrusive fill
        yield return new WaitForSeconds(0.5f);

        //explode
        yield return new WaitForSeconds(0.5f);

        //just shove random rocks into the list
        var magmaCount = inventory.magma.count;

        M8.ArrayUtil.Shuffle(extrusiveRocks);
        mRockResultList.Clear();
        for(int i = 0; i < extrusiveResultCount; i++) {            
            var rock = extrusiveRocks[i];
            rock.count++;

            magmaCount -= inventory.magma.rockValue;
            if(magmaCount < 0)
                magmaCount = 0;

            mRockResultList.Add(rock);
        }

        inventory.magma.count = magmaCount;
        //

        //show rocks
        StartCoroutine(DoRockResult());
    }
    //

    IEnumerator DoRockResult() {
        ClearSelection();

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

    void OnExit() {
        exitScene.Load();
    }

    void OnProcessIntrusive() {        
        StartCoroutine(DoProcessToIntrusiveProceed());
    }

    void OnIntrusiveBack() {
        StartCoroutine(DoIntrusiveBack());
    }

    void OnProcessExtrusive() {        
        StartCoroutine(DoProcessToExtrusiveProceed());
    }

    void OnExtrusiveBack() {
        StartCoroutine(DoExtrusiveBack());
    }

    void OnIntrusiveProceed() {        
        StartCoroutine(DoIntrusiveProceed());
    }

    void OnExtrusiveProceed() {        
        StartCoroutine(DoExtrusiveProceed());
    }

    void OnCoolingStop() {
        ClearSelection();
        mIsCoolingStop = true;
    }

    void OnRockResultContinue() {
        ClearSelection();
        mIsRockResultContinue = true;
    }

    void ClearSelection() {
        var es = EventSystem.current;
        if(es)
            es.SetSelectedGameObject(null);
    }
}
