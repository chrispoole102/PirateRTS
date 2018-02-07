using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicUnit : Unit
{
    void Awake()
    {
        hp = 5;
        type = UnitType.BASIC;
    }
}
