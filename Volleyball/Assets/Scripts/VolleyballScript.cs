using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction { X, Y }

public class VolleyballScript : MonoBehaviour {
    public bool grounded;
    public float SpinAddConst { get { return spinAddConst; } }
    public float MaxAdditions { get { return maxAdditions; } }


    [System.NonSerialized]
    public Rigidbody rb;
    SpecialAction previousPlayer;
    CourtScript court;
    
    [SerializeField]
    Vector2 spin;

    [SerializeField]
    float torqueConst = 200;

    [SerializeField]
    float maxAngularVelocity = 300;

    [SerializeField]
    float spinAddConst = 1.01f;

    [SerializeField]
    int maxAdditions = 50;
    int currentAdditions = 0;

    [SerializeField]
    float floaterConst = 1;
    [SerializeField]
    float scrollSpeed = 0.25f;
    [SerializeField]
    float stepSpeed = 0.1f;
    float noisePosition = 0;
	// Use this for initialization
	void Start () {
        currentAdditions = maxAdditions;
        spin = Vector2.up;
        rb = GetComponent<Rigidbody>();
        court = GameObject.FindGameObjectWithTag("Court").GetComponent<CourtScript>();
    }
	
	// Update is called once per frame
	void Update () {
        rb.maxAngularVelocity = maxAngularVelocity;
        if (transform.position.y < 0f || transform.position.y > 30)
        {
            rb.MovePosition(new Vector3(transform.position.x, 5, transform.position.z));
            rb.velocity = rb.velocity / 10f;
        }
	}

    void FixedUpdate()
    {
        AddSpin();
        if(currentAdditions >= maxAdditions)
            noisePosition += scrollSpeed;
    }

    void OnCollionEnter(Collision other)
    {
        if(other.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            grounded = true;
        }
    }

    public void Shoot(Vector3 force, Transform reference)
    {
        SetSpin(Vector2.up, reference);
        rb.AddForce(force);
    }

    public void Shoot(Vector3 force, Vector2 spin, Transform reference)
    {
        Shoot(force, reference);
        SetSpin(spin, reference);
    }

    void SetSpin(Vector2 spin, Transform reference)
    {
        this.spin = spin;
        currentAdditions = 0;
        rb.AddTorque((reference.up * spin.x * 1 + reference.right * spin.y) * torqueConst);
    }

    void AddSpin()
    {
        if (currentAdditions < maxAdditions)
        {
            Vector3 spinAddition = Vector3.zero;
            if (spin.y >= 1)
            {
                spinAddition.y = -SpinCalculator(ref spin.y);
                spinAddition.x = SpinCalculator(ref spin.x);
            }
            else if (spin.y < 1)
            {
                spinAddition.x = GetFloater(currentAdditions, Direction.X);
                spinAddition.y = GetFloater(currentAdditions, Direction.Y);
            }
            rb.velocity += spinAddition;
            currentAdditions++;
        }
    }

    float SpinCalculator(ref float mySpin)
    {
        mySpin *= spinAddConst;
        return mySpin/100f;
    }

    public void CollideWithPlayer(SpecialAction other)
    {
        if (previousPlayer == other)
        {
            if (court.serve && other.currentPosition == 1)
            {
                court.serve = false;
            }
            else
            {
                court.UpdateScore(court.OppositeSide(other.VolleyballSide));
            }
        }
        currentAdditions = maxAdditions;
        previousPlayer = other;
    }

    public void ResetMotion()
    {
        spin = Vector2.up;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Reset()
    {
        previousPlayer = null;
        ResetMotion();

    }

    public float GetFloater(float noiseOffset, Direction dimension)
    {
        return GetNoise(noiseOffset * stepSpeed, dimension) * (1-spin.y) * floaterConst;
    }

    public float GetFloater(float noiseOffset, float spiny, Direction dimension)
    {
        return GetNoise(noiseOffset * stepSpeed, dimension) * (1 - spiny) * floaterConst;
    }

    float GetNoise(float noiseOffset, Direction dimension)
    {
        Vector2 offset;
        if (dimension == Direction.X)
            offset = Vector2.right * noiseOffset;
        else
            offset = Vector2.up * noiseOffset;
        return PerlinNoiseDerivative(offset, dimension);
    }

    float PerlinNoiseDerivative(Vector2 noiseOffset, Direction dimension)
    {
        Vector2 stepBack;
        if (dimension == Direction.X)
            stepBack = Vector2.right * stepSpeed;
        else
            stepBack = Vector2.up * stepSpeed;
        stepBack = noiseOffset - stepBack;
        float derivative = (Mathf.PerlinNoise(noisePosition + noiseOffset.x, noiseOffset.y) - Mathf.PerlinNoise(noisePosition + stepBack.x, stepBack.y))/stepSpeed;
        if (dimension == Direction.Y)
            derivative /= 2;
        return derivative; 
    }

    void OnCollisionEnter(Collision other)
    {
        ZeroMotion();
    }

    void ZeroMotion()
    {
        spin = Vector2.up;
        currentAdditions = maxAdditions;

    }
}
