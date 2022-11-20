using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModalInfoRock : ModalInfo {
    [Header("Rock Data")]
    public Shapes2D.Shape rockShape;

    protected override void ApplyInfoData() {
        base.ApplyInfoData();

        var rockInfoData = infoData as RockData;

        if(rockInfoData) {
            rockShape.settings.fillTexture = rockInfoData.spriteShape.fillTexture;
        }
    }
}
