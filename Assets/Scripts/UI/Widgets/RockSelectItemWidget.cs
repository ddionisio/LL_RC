using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RockSelectItemWidget : MonoBehaviour {
    public Image iconImage;
    public Image iconFillImage;

    public float fill {
        get { return iconFillImage ? iconFillImage.fillAmount : 0f; }
        set {
            if(iconFillImage)
                iconFillImage.fillAmount = value;
        }
    }
    
    public void Init(InfoData aData) {
        if(iconImage) {
            iconImage.sprite = aData.icon;
            iconImage.SetNativeSize();
        }

        fill = 0f;
    }
}
