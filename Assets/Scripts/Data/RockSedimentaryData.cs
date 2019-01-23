using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "rockSedimentary", menuName = "Game/Rock Sedimentary")]
public class RockSedimentaryData : RockData {
    public override string modal { get { return "infoSedimentary"; } }

    [Header("Sedimentary Info")]
    [M8.Localize]
    public string grainSizeTextRef;
    public bool isOrganicOrChemicallyFormed;
}
