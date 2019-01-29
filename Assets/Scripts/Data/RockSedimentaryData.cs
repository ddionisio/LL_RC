using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GrainSize {
    LargeVariant, //pebbles, cobbles, boulders embedded in sand/silt/clay
    Sand,
    Silt,
    Clay,

    FineCourseCrystal,
    Coarse
}

[CreateAssetMenu(fileName = "rockSedimentary", menuName = "Game/Rock Sedimentary")]
public class RockSedimentaryData : RockData {
    public override string modal { get { return "infoSedimentary"; } }

    [Header("Sedimentary Info")]
    public GrainSize grainSize;
    public bool isOrganicOrChemicallyFormed;

    public string grainSizeTextRef { get { return "grainSize_" + grainSize.ToString(); } }
}
