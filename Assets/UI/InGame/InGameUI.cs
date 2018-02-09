﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour
{

    public MyNetworkPlayer np;
    public Transform commander;

    public IEnumerator waitForCommander()
    {
        while (np.commander==null)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Debug.Log("found commander");
        commander = np.commander.transform;
    }

    public void spawnBasicShip()
    {
        if (commander!=null)
        {
            if (np.isLocalPlayer)
                StartCoroutine(np.spawnUnit(commander.position + Vector3.right * 2, UnitType.BASIC));
        }
    }
}