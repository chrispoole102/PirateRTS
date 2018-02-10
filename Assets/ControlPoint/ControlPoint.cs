using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ControlPoint : NetworkBehaviour
{
    public float range;//the distance a ship has to be from the flag to cap it
    [SyncVar]
    public float capPercent;//how far the flag is up the pole
    [SyncVar]
    public GameObject owner;//the network player that owns this point

    public float topFlagPos;
    public float botFlagPos;

    private float flagX;
    private float flagZ;

    public GameObject flag;
    public Image rangeImage;

	// Use this for initialization
	void Start ()
    {
        flagX = flag.transform.localPosition.x;
        flagZ = flag.transform.localPosition.z;
        StartCoroutine(slowUpdate());
    }

    public IEnumerator slowUpdate()
    {
        while (true)
        {
            if (isClient)
            {
                if (owner != null)
                    flag.GetComponent<MeshRenderer>().material.color = owner.GetComponent<MyNetworkPlayer>().teamColor;
                //set flag height to cap % height
                flag.transform.localPosition = new Vector3(flagX, botFlagPos + ((topFlagPos - botFlagPos) * capPercent), flagZ);
            }
            if (isServer)
            {
                if (Input.GetAxis("Jump") > 0)
                {
                    if (capPercent > 0)
                        capPercent -= 0.05f;
                }
                Collider [] cols = Physics.OverlapSphere(new Vector3(transform.position.x,Sea.SEA_HEIGHT,transform.position.z), range);

                List<GameObject> owners = new List<GameObject>();

                foreach (Collider c in cols)
                {
                    if (c.gameObject.tag == "Unit")
                    {
                        owners.Add(c.gameObject.GetComponent<Unit>().owner);
                    }
                }
                //cap % adjust
                if (capPercent > 0)
                {
                    if (owners.Count > 0 && !hasAlly(owners))
                    {
                        //if range has enemies but no allies, flag goes down
                        capPercent -= 0.05f;
                    }
                }
                else
                {
                    //if flag is at bottom and all units have one owner, change my owner
                    GameObject newOwner = hasSingleOwner(owners);
                    if (newOwner != null)
                        owner = newOwner;
                }
                
                if (owner != null && hasSingleOwner(owners) == owner)
                {
                    //if range has allies but no enemies, flag goes up
                    capPercent += 0.05f;
                    if (capPercent > 1)
                        capPercent = 1;
                }
            }
            yield return new WaitForSeconds(.1f);
        }
    }
    public bool hasAlly(List<GameObject> ow)
    {
        foreach (GameObject o in ow)
        {
            if (o == owner)
                return true;//return true if their is any ally
        }
        return false;
    }
    public GameObject hasSingleOwner(List<GameObject> ow)
    {
        GameObject singleOwner = null;
        foreach (GameObject o in ow)
        {
            if (singleOwner == null)
                singleOwner = o;//if we haven't set a single owner do that
            else if (singleOwner != o)//if we have an owner and this next one isn't that, return false
                return null;
        }
        return singleOwner;//if all the ships had the same owner, return it
    }
}
