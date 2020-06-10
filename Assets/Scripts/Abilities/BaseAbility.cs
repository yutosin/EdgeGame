using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseAbility
{
    Face AbilityFace { get; set; }
    bool IsInitialized { get; set; }
    bool IsActing { get; set; }

    void InitializeAbility(Face face);
}
