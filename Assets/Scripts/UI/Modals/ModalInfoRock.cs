using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModalInfoRock : ModalInfo {
    [Header("Rock Data")]
    public Image rockShape;

    protected override void ApplyInfoData() {
        base.ApplyInfoData();

        var rockInfoData = infoData as RockData;

        if(rockInfoData) {
            var mat = rockShape.material;

            mat.SetTexture(Rock.materialRockTextureOverlay, rockInfoData.spriteShape.fillTexture);
        }
    }
}
