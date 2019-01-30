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
    public Selectable mineralsProcessor;
    public RockSelectWidget rockSelector;
    public Button exitButton;

    public void MineralProcess() {
        inventory.ClearMineralsCount();

        inventory.magma.count += inventory.magma.capacity;

        RefreshInterfaces();

        //animation
    }

    public void RockProcess(InfoData dat) {
        if(dat.count > 0) {
            dat.count--;

            int magmaValue = 1;

            inventory.magma.count += magmaValue;

            criteria.InvokeUpdate(dat);
        }

        rockSelector.RefreshRock(dat);

        if(rockSelector.rockCount == 0)
            RefreshInterfaces();

        //animation
    }

    protected override void OnInstanceDeinit() {
        base.OnInstanceDeinit();

        if(rockSelector)
            rockSelector.processRockCallback -= RockProcess;
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        rockSelector.processRockCallback += RockProcess;

        EventSystem.current.SetSelectedGameObject(null);
        mineralsProcessor.interactable = false;
        rockSelector.interactable = false;

        exitButton.onClick.AddListener(OnExitClicked);
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //animation entrance
        RefreshInterfaces();
    }

    void OnExitClicked() {
        exitScene.Load();
    }

    void RefreshInterfaces() {
        int mineralsCounts = inventory.mineralsCount;
        int rocksCounts = inventory.rocksCount;

        var exitNavigation = exitButton.navigation;

        //initialize minerals
        //initialize rocks
        if(mineralsCounts == 0 && rocksCounts == 0) {
            mineralsProcessor.interactable = false;
            rockSelector.interactable = false;

            exitNavigation.mode = Navigation.Mode.None;

            exitButton.Select();
        }
        else if(mineralsCounts > 0) {
            mineralsProcessor.interactable = true;
            mineralsProcessor.Select();

            rockSelector.interactable = false;

            exitNavigation.mode = Navigation.Mode.Explicit;
            exitNavigation.selectOnUp = mineralsProcessor;
        }
        else {
            mineralsProcessor.interactable = false;

            rockSelector.interactable = true;
            rockSelector.Select();

            exitNavigation.mode = Navigation.Mode.Explicit;
            exitNavigation.selectOnUp = rockSelector;
        }

        exitButton.navigation = exitNavigation;
    }
}
