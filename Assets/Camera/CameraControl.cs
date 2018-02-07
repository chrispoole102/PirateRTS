using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    public Rigidbody rigid;
    public float speed;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
    }

	void Update ()
    {
        float xMove = Input.GetAxis("Horizontal");
        float zMove = Input.GetAxis("Vertical");

        rigid.velocity = new Vector3(xMove * speed, 0, zMove * speed);
	}
}
