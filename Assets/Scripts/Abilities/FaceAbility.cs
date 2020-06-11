using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFaceAbility
{
    Face AbilityFace { get; set; }
    bool IsInitialized { get; set; }
    bool IsActing { get; set; }
    int AbilityTimes { get; set; }

    void InitializeAbility(Face face);
}
