using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "rockMetamorphic", menuName = "Game/Rock Metamorphic")]
public class RockMetamorphicData : RockData {
    public override string modal { get { return "infoMetamorphic"; } }

    [Header("Metamorphic Info")]
    [M8.Localize]
    public string metamorphismTextRef;
}
