using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour {
    public bool RotateAround;
    public float offset;
    public Transform target;
    float angle = 0;
	// Use this for initialization
	void Start () {
        offset = -target.position.z + transform.position.z;
	}
	
	// Update is called once per frame
	void Update () {

        Vector3 rotationMultiplier = Vector3.one;
        if (RotateAround)
        {
            angle = transform.eulerAngles.y * Mathf.Deg2Rad;
            rotationMultiplier.z = Mathf.Cos(angle) * offset;
            rotationMultiplier.x = Mathf.Sin(angle) * offset;
        }
        transform.position = target.position + rotationMultiplier;
	}
}
