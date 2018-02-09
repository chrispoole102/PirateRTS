using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.AI;

public class MyNetworkPlayer : NetworkBehaviour
{
    public GameObject basicUnitPrefab;//because the game piece is spawned, it must be added to the network manager's "registered spawnable prefabs" list
    public GameObject commanderPrefab;

    //public List<GameObject> myUnits;//I don't think I need to sync this list because the unit's sync their owners

    private GameObject xMarkCanvas;
    private GameObject startUI;
    private Dropdown startUIDropdown;
    private GameObject inGameUI;

    [SyncVar]
    public GameObject selected;

    [SyncVar]
    public GameObject commander;

    public Color teamColor;
    public int team;//only used for cleanup not used for controlling ships
                    //also doesn't have to be sync var because its only used on the server.

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

            startUI = GameObject.Find("StartUICanvas");
            startUI.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => onClickSetSail());
            startUIDropdown = startUI.transform.GetChild(0).GetComponent<Dropdown>();

            inGameUI = GameObject.Find("InGameCanvas");
            inGameUI.GetComponent<InGameUI>().np = this;
            inGameUI.SetActive(false);
            //Cmd_SetName("Player " + (GameObject.FindGameObjectsWithTag("NetworkPlayer").Length));
        }

        if (isServer)
        {
            //see Cmd_AttempJoinTeam
        }
        StartCoroutine(slowUpdate());
    }
    //-----------------
    //----START UI-----
    //-----------------
    public void onClickSetSail()
    {
        Cmd_AttemptJoinTeam(startUIDropdown.value);
    }
    [Command]
    public void Cmd_AttemptJoinTeam(int team)
    {
        if (TeamManager.teamTaken[team])
        {
            //send message to client that sent command
            Rpc_SendStartUIMessage("Team Already Taken!");
        }
        else
        {
            TeamManager.teamTaken[team] = true;
            teamColor = TeamManager.teams[team];
            this.team = team;

            Rpc_JoinGame();

            //--------------------
            //SPAWN STARTING SHIPS
            //--------------------

            StartCoroutine(spawnUnit(Vector2.zero, UnitType.COMMANDER));
            //StartCoroutine(spawnUnit(new Vector2(0, 2), UnitType.BASIC));
        }
    }
    [ClientRpc]
    public void Rpc_SendStartUIMessage(string message)
    {
        if (isLocalPlayer)
            startUI.transform.GetChild(2).GetComponent<Text>().text = message;
    }
    [ClientRpc]
    public void Rpc_JoinGame()
    {
        if (isLocalPlayer)
        {
            Destroy(startUI);
            inGameUI.SetActive(true);
            StartCoroutine(inGameUI.GetComponent<InGameUI>().waitForCommander());//wait for the server to spawn the commander ship
        }
    }
    [Command]
    public void Cmd_spawnCommand(Vector2 pos, UnitType type)
    {
        StartCoroutine(spawnUnit(pos, type));
    }
    //ONLY CALL THIS ON SERVER
    public IEnumerator spawnUnit(Vector2 pos, UnitType type, float spawnDelay = 0.0f)
    {
        //while (myName == "")//wait for us to get a name
        //{
        //    yield return new WaitForSeconds(0.1f);
        //}
        yield return new WaitForSeconds(spawnDelay);

        GameObject Temp;

        switch (type)
        {
            case UnitType.BASIC: Temp = Instantiate(basicUnitPrefab); break;
            case UnitType.COMMANDER: Temp = Instantiate(commanderPrefab); break;
            default: Temp = Instantiate(basicUnitPrefab); break;
        }

        Temp.GetComponent<Unit>().owner = this.gameObject;

        Temp.GetComponent<NavMeshAgent>().Warp(new Vector3(pos.x, Sea.SEA_HEIGHT, pos.y));//needs to do this instead of transform.postion
        Temp.GetComponent<NavMeshAgent>().SetDestination(new Vector3(pos.x, Sea.SEA_HEIGHT, pos.y));

        Temp.GetComponent<Unit>().color = teamColor;

        NetworkServer.Spawn(Temp);

        if (type == UnitType.COMMANDER)
        {
            commander = Temp;
        }
    }
    //---------------
    //----UPDATE-----
    //---------------
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
                            {
                                Cmd_Target(selected, hit.collider.gameObject);
                            }
                        }
                        else
                        {
                            xMarkCanvas.SetActive(true);
                            xMarkCanvas.transform.position = new Vector3(hit.point.x, Sea.SEA_HEIGHT + 0.1f, hit.point.z);

                            Cmd_Move(hit.point.x, hit.point.z);
                        }
                    }
                }
                if (selected == null)
                {
                    if (xMarkCanvas.activeSelf)
                        xMarkCanvas.SetActive(false);
                }
                else
                {
                    if (selected.GetComponent<Unit>().target != null)
                    {
                        if (!xMarkCanvas.activeSelf)
                            xMarkCanvas.SetActive(true);
                        xMarkCanvas.transform.position = selected.GetComponent<Unit>().target.transform.position;
                    }
                }
            }
            yield return new WaitForSeconds(.05f);
        }
    }
    //------------------------------
    //----SHIP CONTROL COMMANDS-----
    //------------------------------
    [Command]
    public void Cmd_Select(GameObject s)
    {
        selected = s;
    }
    [Command]//Commands are only run on the server
    public void Cmd_Move(float x, float z)
    {
        if (selected!=null)
            selected.GetComponent<NavMeshAgent>().destination = new Vector3(x, Sea.SEA_HEIGHT, z);//move the piece
    }
    [Command]
    public void Cmd_Target(GameObject a, GameObject b)
    {
        Debug.Log("targeting");
        a.GetComponent<Unit>().target = b;
    }

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
        TeamManager.teamTaken[team] = false;
    }
}
