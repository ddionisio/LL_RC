using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RockSelectItemWidget : MonoBehaviour {
    public GameObject iconRootGO;
    public Image iconImage; //if icon is available

    public GameObject iconRawRootGO;
    public RawImage iconRawImage; //for rocks

    public Image iconFillImage;
    public M8.UI.Graphics.ColorGroup colorGroup;

    public float fill {
        get { return iconFillImage ? iconFillImage.fillAmount : 0f; }
        set {
            if(iconFillImage)
                iconFillImage.fillAmount = value;
        }
    }
    
    public void Init(InfoData aData, bool active) {
        //prefer icon over raw texture
        if(aData.icon) {
            iconRootGO.SetActive(true);
            iconRawRootGO.SetActive(false);

            iconImage.sprite = aData.icon;
            iconImage.SetNativeSize();
        }
        else {
            //TODO: only rocks for now
            var rockDat = aData as RockData;
            if(rockDat && rockDat.spriteShape) {
                iconRootGO.SetActive(false);
                iconRawRootGO.SetActive(true);

                iconRawImage.texture = rockDat.spriteShape.fillTexture;
            }
            else {
                iconRootGO.SetActive(false);
                iconRawRootGO.SetActive(false);
            }
        }

        fill = 0f;

        if(active)
            colorGroup.Revert();
        else
            colorGroup.ApplyColor(); //start disabled
    }
}
