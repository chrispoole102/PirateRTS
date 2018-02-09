using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingShip : MonoBehaviour {

    private float tiltX;
    private float tiltZ;
    
	// Use this for initialization
	void Start ()
    {
        if (Random.Range(0, 1) == 0)
            tiltX = Random.Range(-5f, -1f);
        else
            tiltX = Random.Range(1f, 5f);

        tiltZ = Random.Range(-2.5f, 2.5f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.position += Vector3.down * 1f * Time.deltaTime;
        transform.Rotate(new Vector3(tiltX * Time.deltaTime, 0, tiltZ * Time.deltaTime));
        if (transform.position.y < -5)
            Destroy(gameObject);
	}
}
