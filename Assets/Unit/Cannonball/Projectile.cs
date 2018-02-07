using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public GameObject target;
    public float speed;
    public float dieTimer;
        
	// Update is called once per frame
	void Update ()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, speed * Time.deltaTime);
        dieTimer -= Time.deltaTime;
        if (dieTimer < 0)
            Destroy(gameObject);
    }
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject == target)
            Destroy(gameObject);
    }
}
