using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolleyballScript : MonoBehaviour {
    public bool grounded;
    [System.NonSerialized]
    public Rigidbody rb;
    GameObject previousPlayer;
    CourtScript court;
    
    Vector2 spin;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
        court = GameObject.FindGameObjectWithTag("Court").GetComponent<CourtScript>();
    }
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y < 0f || transform.position.y > 30)
        {
            rb.MovePosition(new Vector3(transform.position.x, 5, transform.position.z));
            rb.velocity = rb.velocity / 10f;
        }
	}

    void FixedUpdate()
    {

    }

    void OnCollionEnter(Collision other)
    {
        if(other.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
        }
    }

    public void CollideWithPlayer(GameObject other)
    {
        //if (other.tag == "Player")
        //{
            
            SpecialAction player = other.GetComponentInChildren<SpecialAction>();
            if (previousPlayer == other.gameObject)
            {
                if (court.serve && player.currentPosition == 1)
                {
                    court.serve = false;
                }
                else
                {
                    court.UpdateScore(court.OppositeSide(player.VolleyballSide));
                }
            }
            previousPlayer = other.gameObject;
       // }
    }

    public void ResetMotion()
    {
        spin = Vector2.zero;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Reset()
    {
        previousPlayer = null;
        ResetMotion();

    }

    void OnTriggerEnter(Collider other)
    {
        
    }
}
