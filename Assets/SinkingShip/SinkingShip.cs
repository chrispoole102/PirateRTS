using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinkingShip : MonoBehaviour {

    private float tiltX;
    private float tiltZ;

	// Use this for initialization
	void Start ()
    {
        if (Random.Range(0,1)==0)
            tiltX = Random.Range(-0.5f, -0.1f);
        else
            tiltX = Random.Range(0.1f, 0.5f);

        tiltZ = Random.Range(-0.25f, 0.25f);
    }
	
	// Update is called once per frame
	void Update ()
    {
        transform.position += Vector3.down * 1f * Time.deltaTime;
        transform.Rotate(new Vector3(-0.5f, 0, 0));
        if (transform.position.y < -5)
            Destroy(gameObject);
	}
}
