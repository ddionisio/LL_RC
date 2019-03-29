using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CriteriaWidget : MonoBehaviour {
    public RawImage iconImage;

    public Text countText;
    public string countTextFormat = "00";

    public GameObject unlockedGO;
    public GameObject lockedGO;

    public bool isLocked { get; private set; }
    
    public void SetUnlocked(InfoData dat) {
        if(unlockedGO) unlockedGO.SetActive(true);
        if(lockedGO) lockedGO.SetActive(false);

        if(iconImage) {
            var rockDat = dat as RockData;
            if(rockDat && rockDat.spriteShape) {
                iconImage.texture = rockDat.spriteShape.fillTexture;
            }
        }

        if(countText)
            countText.text = dat.count.ToString(countTextFormat);

        isLocked = false;
    }

    public void SetLocked() {
        if(unlockedGO) unlockedGO.SetActive(false);
        if(lockedGO) lockedGO.SetActive(true);

        isLocked = true;
    }
}
