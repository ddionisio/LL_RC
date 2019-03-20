using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CriteriaWidget : MonoBehaviour {
    public Image iconImage;
    public bool iconImageResize = true;

    public Text countText;
    public string countTextFormat = "00";

    public GameObject unlockedGO;
    public GameObject lockedGO;
    
    public void SetUnlocked(InfoData dat) {
        if(unlockedGO) unlockedGO.SetActive(true);
        if(lockedGO) lockedGO.SetActive(false);

        if(iconImage) {
            if(dat.icon) {
                iconImage.sprite = dat.icon;
                if(iconImageResize)
                    iconImage.SetNativeSize();
            }
        }

        if(countText)
            countText.text = dat.count.ToString(countTextFormat);
    }

    public void SetLocked() {
        if(unlockedGO) unlockedGO.SetActive(false);
        if(lockedGO) lockedGO.SetActive(true);
    }
}
