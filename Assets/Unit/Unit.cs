using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;
using UnityEngine.UI;

public class Unit : NetworkBehaviour
{
    [SyncVar]
    public GameObject owner;
    
    [SyncVar]
    public Color color;

    public UnitType type;//doesn't need to sync because only set once at spawn and always the same

    [SyncVar]
    public int hp = 100;
    [SyncVar]
    public int maxhp;

    public GameObject bulletPrefab;
    public float bulletSpeed;
    [SyncVar]
    public GameObject target;
    [SyncVar]
    public bool canShoot = true;
    public float shootTimer = 1;
    public float range = 2;
    public int damage = 30;
    public float rotateSpeedForAttack;

    public GameObject sinkingShipPrefab;
    public GameObject spawningShipPrefab;

    private MeshRenderer render;
    private Material[] mats;

    public Material selectedMaterial;
    public Material normalMaterial;

    private NavMeshAgent nma;

    private Image hpBar;

    [SyncVar]
    public float fallSpeed = 1f;

    // Use this for initialization
    void Start ()
    {
        hp = maxhp;

        nma = GetComponent<NavMeshAgent>();

        render = GetComponent<MeshRenderer>();
        mats = GetComponent<MeshRenderer>().materials;

        mats[0].color = color;

        mats[1] = normalMaterial;
        render.materials = mats;

        hpBar = transform.GetChild(0).GetChild(1).GetComponent<Image>();

        StartCoroutine(slowUpdate());

        if (isClient)//NOTE: because of Hosts, don't bank on server's version being active during animation
        {
            GameObject spawning = GameObject.Instantiate(spawningShipPrefab, transform.position, transform.rotation);
            spawning.GetComponent<MeshRenderer>().materials[0].color = color;
            spawning.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
            spawning.transform.localScale = transform.localScale;
            spawning.GetComponent<SpawningShip>().unit = gameObject;
            //disappear and wait for spawn animation to finish
            gameObject.SetActive(false);
        }
    }

    public void onSelected()//ONLY VISUAL NO STATE CHANGE
    {
        mats[1] = selectedMaterial;
        render.materials = mats;
    }
    public void onDeselected()//ONLY VISUAL NO STATE CHANGE
    {
        mats[1] = normalMaterial;
        render.materials = mats;
    }

    public IEnumerator slowUpdate()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (isClient)
            {
                hpBar.fillAmount = (float)hp / (float)maxhp;
            }
            if (isServer)
            {
                
                if (hp <= 0)
                {
                    Rpc_sink();
                    Destroy(gameObject);
                }
                if (canShoot)
                {
                    if (target != null)
                    {
                        if (Vector3.Distance(target.transform.position, transform.position) < range)
                        {
                            nma.destination = transform.position;//stop
                            RaycastHit hit;
                            if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit))
                            {
                                if (hit.collider.gameObject == target)
                                {
                                    if (isFacing(target))
                                    {
                                        //transform.LookAt(target.transform.position);//use rigidbody not transform
                                        canShoot = false;
                                        StartCoroutine(DamageDelay(target));
                                        StartCoroutine(Timer());
                                        Rpc_fireAtTarget();
                                    }
                                    else
                                        rotateToFace(target);
                                }
                            }
                        }
                        else
                        {
                            nma.destination = target.transform.position;//move towards target if not in range
                        }
                    }
                    else
                    {
                        //find targets from nearby
                        
                        GameObject[] units = GameObject.FindGameObjectsWithTag("Unit");
                        float dist = range;
                        int index = -1;
                        for (int i = 0; i<units.Length; i++)
                        {
                            if (units[i].GetComponent<Unit>().owner != owner)
                            {
                                if (Vector3.Distance(units[i].transform.position, transform.position) < dist)
                                {
                                    RaycastHit hit;
                                    if (Physics.Raycast(transform.position, units[i].transform.position - transform.position, out hit))
                                    {
                                        if (hit.collider.gameObject == units[i])
                                        {
                                            index = i;
                                            dist = Vector3.Distance(units[i].transform.position, transform.position);
                                        }
                                    }
                                }
                            }
                        }
                        if (index != -1)
                        {
                            //this turned out being a bit awkward considering I can't just set the target because that overrides all movement
                            if (isFacing(units[index]))
                            {
                                //transform.LookAt(units[index].transform.position);//use rigidbody not transform
                                canShoot = false;
                                StartCoroutine(DamageDelay(units[index]));
                                StartCoroutine(Timer());
                                Rpc_fireAt(units[index]);
                            }
                            else
                                rotateToFace(units[index]);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }
    public bool isFacing(GameObject t)
    {
        Vector3 dirFromAtoB = (t.transform.position - transform.position).normalized;
        float dotProd = Vector3.Dot(dirFromAtoB, transform.forward);
        
        return (dotProd > 0.9);
    }
    public void rotateToFace(GameObject t)
    {
        Vector3 targetDir = t.transform.position - transform.position;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, rotateSpeedForAttack * Time.deltaTime, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);
    }

    IEnumerator DamageDelay(GameObject goToDamage)
    {
        yield return new WaitForSeconds(Vector3.Distance(goToDamage.transform.position, transform.position)/bulletSpeed);
        goToDamage.GetComponent<Unit>().hp -= damage;
    }
    IEnumerator Timer()
    {
        yield return new WaitForSeconds(shootTimer);
        canShoot = true;
    }
    [ClientRpc]
    public void Rpc_fireAtTarget()//I seperated these two so there wouldn't be unnecessary network traffic when fighting the target
    {
        GameObject t = target;
        GameObject bullet = GameObject.Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.GetComponent<Projectile>().speed = bulletSpeed;
        bullet.GetComponent<Projectile>().target = t;
    }
    [ClientRpc]
    public void Rpc_fireAt(GameObject t)
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.GetComponent<Projectile>().speed = bulletSpeed;
        bullet.GetComponent<Projectile>().target = t;
    }
    [ClientRpc]
    public void Rpc_sink()
    {
        GameObject sinking = GameObject.Instantiate(sinkingShipPrefab, transform.position, transform.rotation);
        sinking.GetComponent<MeshRenderer>().materials[0].color = color;
        sinking.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
        sinking.transform.localScale = transform.localScale;
    }
}
