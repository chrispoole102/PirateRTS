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

    public Material selectedMaterial;
    public Material normalMaterial;

    // Use this for initialization
    void Start ()
    {
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

            Debug.Log("me");
            if (isClient)
            {
                /*if (target != null && canShoot)
                {
                    GameObject temp = GameObject.Instantiate(bulletPrefab, transform.position, transform.rotation);
                    //move towards target
                }*/
                Debug.Log("hi2");
                if (Input.GetAxisRaw("Jump")>0)
                {
                    Debug.Log("hi");
                    fireAt(GameObject.FindGameObjectWithTag("Finish"));
                }
            }
            if (isServer)
            {
                Debug.Log("hi3");
                if (target != null && canShoot)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, target.transform.position - transform.position, out hit))
                    {
                        if (hit.collider.gameObject == target)
                        {
                            target.GetComponent<Unit>().hp--;
                            transform.LookAt(target.transform.position);//use rigidbody not transform
                            canShoot = false;
                            StartCoroutine(Timer());
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.05f);
        }
    }
    IEnumerator Timer()
    {
        yield return new WaitForSeconds(shootTimer);
        canShoot = true;
    }
    public void fireAt(GameObject t)
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, transform.position, transform.rotation);
        bullet.GetComponent<Projectile>().speed = bulletSpeed;
        bullet.GetComponent<Projectile>().target = t;
    }
}
