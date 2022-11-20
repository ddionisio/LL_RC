using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IgneousFormation {
    Intrusion,
    Extrusion
}

[CreateAssetMenu(fileName = "rockIgneous", menuName = "Game/Rock Igneous")]
public class RockIgneousData : RockData {
    public override string modal { get { return "infoIgneous"; } }

    [Header("Igneous Info")]
    public IgneousFormation formation;
    public bool vesicular;
    [M8.Localize]
    public string crystalSizeTextRef;
    public M8.RangeFloat felsicMaficScale;
}
