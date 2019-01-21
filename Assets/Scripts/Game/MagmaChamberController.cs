using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagmaChamberController : GameModeController<MagmaChamberController> {
    [Header("Data")]
    public InventoryData inventory;
    public M8.SceneAssetPath exitScene;

    [Header("UI")]
    public Selectable mineralsProcessor;
    public RockSelectWidget rockSelector;
    public Button exitButton;

    public void MagmaProcess() {
        inventory.ClearMineralsCount();

        inventory.magma.count += inventory.magma.capacity;

        RefreshInterfaces();
    }

    protected override void OnInstanceInit() {
        base.OnInstanceInit();

        RefreshInterfaces();
    }

    protected override IEnumerator Start() {
        yield return base.Start();

        //animation entrance
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

            exitButton.Select();
        }
        else if(mineralsCounts > 0) {
            mineralsProcessor.interactable = true;
            mineralsProcessor.Select();

            rockSelector.interactable = false;

            exitNavigation.selectOnUp = mineralsProcessor;
        }
        else {
            mineralsProcessor.interactable = false;

            rockSelector.interactable = true;
            rockSelector.Select();

            exitNavigation.selectOnUp = rockSelector;
        }

        exitButton.navigation = exitNavigation;
    }
}
