using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolleyballScript : MonoBehaviour {
    public bool grounded;
    Rigidbody rb;
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y < 0f || transform.position.y > 30)
        {
            rb.MovePosition(new Vector3(transform.position.x, 5, transform.position.z));
            rb.velocity = rb.velocity / 10f;
        }
	}

    void OnCollionEnter(Collision other)
    {
        if(other.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
        }
    }
}
