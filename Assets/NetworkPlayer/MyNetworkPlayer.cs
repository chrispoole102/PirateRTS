using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.AI;

public class MyNetworkPlayer : NetworkBehaviour
{
    public GameObject basicUnitPrefab;//because the game piece is spawned, it must be added to the network manager's "registered spawnable prefabs" list

    //public List<GameObject> myUnits;//I don't think I need to sync this list because the unit's sync their owners

    public GameObject xMarkCanvas;

    [SyncVar]
    public GameObject selected;

    //[SyncVar]
    //public string myName;

    //private GameObject scoreDisplay;

    // Use this for initialization
    void Start()
    {

        if (isLocalPlayer)
        {
            xMarkCanvas = GameObject.Find("xMarkerCanvas");
            xMarkCanvas.SetActive(false);
            //Cmd_SetName("Player " + (GameObject.FindGameObjectsWithTag("NetworkPlayer").Length));
        }

        if (isServer)
        {
            StartCoroutine(spawnUnit(Vector2.zero, UnitType.BASIC));
            StartCoroutine(spawnUnit(new Vector2(0,2), UnitType.BASIC));
        }
        StartCoroutine(slowUpdate());
    }
    public IEnumerator spawnUnit(Vector2 pos, UnitType type, float spawnDelay = 0.0f)
    {
        //while (myName == "")//wait for us to get a name
        //{
        //    yield return new WaitForSeconds(0.1f);
        //}
        yield return new WaitForSeconds(spawnDelay);

        GameObject Temp = Instantiate(basicUnitPrefab);
        
        Temp.GetComponent<Unit>().owner = this.gameObject;

        Temp.GetComponent<NavMeshAgent>().Warp(new Vector3(pos.x, Sea.SEA_HEIGHT, pos.y));//needs to do this instead of transform.postion
        Temp.GetComponent<NavMeshAgent>().SetDestination(new Vector3(pos.x, Sea.SEA_HEIGHT, pos.y));

        NetworkServer.Spawn(Temp);
        //myUnits.Add(Temp);
    }
    public IEnumerator slowUpdate()
    {
        while (true)
        {
            if (isLocalPlayer)
            {
                if (Input.GetMouseButton(0))//LEFT CLICK
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.gameObject.tag == "Unit")
                        {
                            if (hit.collider.gameObject.GetComponent<Unit>().owner == gameObject)
                            {
                                if (selected!=null)
                                    selected.GetComponent<Unit>().onDeselected();
                                Cmd_Select(hit.collider.gameObject);
                                hit.collider.gameObject.GetComponent<Unit>().onSelected();

                                xMarkCanvas.SetActive(true);
                                xMarkCanvas.transform.position = new Vector3(hit.collider.gameObject.GetComponent<NavMeshAgent>().destination.x, Sea.SEA_HEIGHT + 0.1f, hit.collider.gameObject.GetComponent<NavMeshAgent>().destination.z);
                            }
                        }
                    }
                }
                else if (Input.GetMouseButton(1) && selected!=null)//RIGHT CLICK
                {
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    
                    if (Physics.Raycast(ray, out hit))
                    {
                        if (hit.collider.gameObject.tag == "Unit")
                        {
                            if (hit.collider.gameObject.GetComponent<Unit>().owner != gameObject)
                                Cmd_Target(selected, hit.collider.gameObject);
                        }
                        else
                        {
                            xMarkCanvas.SetActive(true);
                            xMarkCanvas.transform.position = new Vector3(hit.point.x, Sea.SEA_HEIGHT + 0.1f, hit.point.z);

                            Cmd_Move(hit.point.x, hit.point.z);
                        }
                    }
                }

            }
            yield return new WaitForSeconds(.05f);
        }
    }
    [Command]
    public void Cmd_Select(GameObject s)
    {
        selected = s;
    }
    [Command]//Commands are only run on the server
    public void Cmd_Move(float x, float z)
    {
        selected.GetComponent<NavMeshAgent>().destination = new Vector3(x, Sea.SEA_HEIGHT, z);//move the piece
    }
    [Command]
    public void Cmd_Target(GameObject a, GameObject b)
    {
        a.GetComponent<Unit>().target = b;
    }
    /*
    [Command]
    public void Cmd_SetName(string n)
    {
        myName = n;
    }*/

    void OnPlayerDisconnected(UnityEngine.NetworkPlayer player)
    {
        Debug.Log("Clean up after player " + player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }

    void OnDestroy()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Unit"))
        {
            if (go.GetComponent<Unit>().owner == gameObject)
            {
                Destroy(go);
            }
        }
    }
}
