using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour
{
    public LayerMask cantSpawnOnLayer;
    public MyNetworkPlayer np;
    public Transform commander;

    public IEnumerator waitForCommander()
    {
        while (np.commander==null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        commander = np.commander.transform;
    }

    public void spawnBasicShip()
    {
        if (commander!=null)
        {
            if (np.isLocalPlayer)
            {
                Vector3 newPos;
                int escape = 0;
                do
                {
                    newPos = commander.position + new Vector3(Random.Range(-6, 6), 0, Random.Range(-6, 6));
                    escape++;
                }
                while (Physics.CheckSphere(newPos + Vector3.up, 2f, cantSpawnOnLayer) && escape<30);

                if (escape<30)
                {
                    np.Cmd_spawnCommand(new Vector2(newPos.x, newPos.z), UnitType.LIGHT);
                }
                else
                {
                    Debug.Log("ERROR: COULD NOT FIND LOCATION");
                }
            }
        }
    }
}
