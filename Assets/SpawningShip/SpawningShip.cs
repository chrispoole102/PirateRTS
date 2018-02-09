using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawningShip : MonoBehaviour {

    public GameObject unit;

    public float fallSpeed = 1;

    public int startHeight = 16;

    private float timeInAir = 0;

    private int animState;
    //0 = fall
    //1 = bounce back above surface

	// Use this for initialization
	void Start ()
    {
        transform.position += Vector3.up * startHeight;
        animState = 0;
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (animState == 0)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            timeInAir += Time.deltaTime;
            transform.Rotate(50 * Time.deltaTime, 0, 0);
            fallSpeed += 0.5f;
            if (transform.position.y <= Sea.SEA_HEIGHT - 1f)
            {
                animState = 1;
            }
        }
        else if (animState == 1)
        {
            transform.position += Vector3.up * fallSpeed/1.5f * Time.deltaTime;
            transform.Rotate((50 * timeInAir)/-(fallSpeed/1.5f)*Time.deltaTime, 0, 0);
            if (transform.position.y >= Sea.SEA_HEIGHT)
            {
                unit.SetActive(true);
                Destroy(gameObject);
            }
        }
    }
}
