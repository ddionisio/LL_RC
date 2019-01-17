using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "rockMetamorphic", menuName = "Game/Rock Metamorphic")]
public class RockMetamorphicData : RockData {
    [Header("Metamorphic Info")]
    [M8.Localize]
    public string metamorphismTextRef;
}
