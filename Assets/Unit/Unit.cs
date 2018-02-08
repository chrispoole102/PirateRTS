using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AI;

public class Unit : NetworkBehaviour
{
    [SyncVar]
    public GameObject owner;

    public UnitType type;//doesn't need to sync because only set once at spawn and always the same

    [SyncVar]
    public int hp = 5;

    public GameObject bulletPrefab;
    public float bulletSpeed;
    [SyncVar]
    public GameObject target;
    [SyncVar]
    public bool canShoot = true;
    public float shootTimer = 1;
    public float range = 2;
    public float rotateSpeedForAttack;

    public Material selectedMaterial;
    public Material normalMaterial;

    public NavMeshAgent nma;

    // Use this for initialization
    void Start ()
    {
        nma = GetComponent<NavMeshAgent>();

        StartCoroutine(slowUpdate());
    }

    public void onSelected()//ONLY VISUAL NO STATE CHANGE
    {
        GetComponent<Renderer>().material = selectedMaterial;
    }
    public void onDeselected()//ONLY VISUAL NO STATE CHANGE
    {
        GetComponent<Renderer>().material = normalMaterial;
    }

    public IEnumerator slowUpdate()
    {
        yield return new WaitForSeconds(1);
        while (true)
        {
            if (hp <= 0)
                Destroy(gameObject);

            if (isClient)
            {
                if (target != null && canShoot)
                {
                    //fireAt(target);
                }
            }
            if (isServer)
            {
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
                                        StartCoroutine(DamageDelay(target.GetComponent<Unit>()));
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
                                StartCoroutine(DamageDelay(units[index].GetComponent<Unit>()));
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

    IEnumerator DamageDelay(Unit unitToDamage, int damage = 1)
    {
        yield return new WaitForSeconds(0.5f);
        unitToDamage.hp -= damage;
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
}
