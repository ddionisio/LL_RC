using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum MetamorphicFlag {
    None = 0x0,
    Regional = 0x1, //pressure/heat
    Contact = 0x2 //magma contact
}

[CreateAssetMenu(fileName = "rockMetamorphic", menuName = "Game/Rock Metamorphic")]
public class RockMetamorphicData : RockData {
    public override string modal { get { return "infoMetamorphic"; } }

    [Header("Metamorphic Info")]
    [M8.EnumMask]
    public MetamorphicFlag morphFlags;
}
