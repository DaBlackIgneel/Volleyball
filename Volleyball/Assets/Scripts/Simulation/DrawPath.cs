using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPath : MonoBehaviour {

    public ParticleSystem mySystem;
    public Vector3 particleStartPosition;
    [Range(0.2f, 1)]
    public float aimingDetail = .5f;
    
    private float myAimingQuality;
    [SerializeField]
    private float pathSpread;
    ParticleSystem.Burst[] myBurst;
    ParticleSystem.Particle[] myParticle;
    Function function;
    const float regFixedDeltaTime = 1 / 50f;
    float netLowerBound = 1.43f - .264f / 2f;
    float netUpperBound = 2.43f + .264f / 2f;
    LineRenderer lr;


    // Use this for initialization
    void Start () {
        function = new Function();
        //the component that controlls the amount of particles emmitted for the aiming
        myBurst = new ParticleSystem.Burst[1];
        lr = mySystem.GetComponent<LineRenderer>();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void UpdateAimingQuality()
    {
        if (aimingDetail != myBurst[0].minCount)
        {
            short burstCount = System.Convert.ToInt16(Mathf.Pow(aimingDetail, 1.5f) * 200);
            myBurst[0].minCount = burstCount;
            myBurst[0].maxCount = burstCount;
            mySystem.emission.SetBursts(myBurst);
        }
    }

    //starts the particle creations
    void GetParticles()
    {
        if (mySystem.isStopped)
        {
            mySystem.Play();
            mySystem.emission.GetBursts(myBurst);
            UpdateAimingQuality();
            myParticle = new ParticleSystem.Particle[myBurst[0].minCount];
        }
        

        //gets all the current particles that are alive
        if (mySystem.isPlaying)
        {
            mySystem.GetParticles(myParticle);
            lr.enabled = true;
            lr.numPositions = myParticle.Length;
        }
        
    }

    //stops the particle creation and deletes all particles
    public void ClearParticles()
    {
        if (mySystem.isPlaying)
        {
            mySystem.Clear();
            mySystem.Stop();
            lr.numPositions = 0;
            lr.enabled = false;
        }
    }

    public void SetFunction(Function function)
    {
        this.function = function;
    }

    public void AddFunction(Function function)
    {
        this.function.AddFunction(function);
    }

    public void ClearFunction()
    {
        this.function = new Function();
    }

    public void AddGravity()
    {
        AddFunction(Function.Gravity());
    }

    public bool FunctionExists(Function function)
    {
        return this.function.Contains(function);
    }

    public Function GetFunction()
    {
        return this.function;
    }

    public void SetPathSpread(float pathSpread)
    {
        this.pathSpread = pathSpread;
    }

    //Draws the Path using a particle system;
    public void SimulatePath(SpecialAction sp)
    {
        GetParticles();
        
        if(mySystem.particleCount > 0)
        {
            float time = pathSpread * regFixedDeltaTime / mySystem.particleCount;
            Color particleColor = isGoingToHitNet() ? Color.red : Color.white;
            for (int i = 0; i < myParticle.Length; i++)
            {
                Vector3 pathPosition = function.Evaluate(i * time);
                myParticle[i].position = particleStartPosition + pathPosition;
                myParticle[i].remainingLifetime = 100000;
                myParticle[i].startColor = particleColor;

                lr.SetPosition(i, sp.vBall.transform.position + pathPosition);
            }
            lr.startColor = particleColor;
            lr.endColor = particleColor;
            //move the particles to the predicted position
            mySystem.SetParticles(myParticle, myParticle.Length);
        }
    }

    public bool isGoingToHitNet()
    {
        Function function = GetFunction().Flatten();
        Vector3 landing = Vector3.zero;

        float netTime = 0;
        if (Mathf.Abs(function.GetAcceleration().z) > Mathf.Epsilon)
            netTime = (-function.GetVelocity().z - Mathf.Sqrt(Mathf.Pow(function.GetVelocity().z, 2) - 4 * function.GetAcceleration().z * 0.5f * (function.GetPosition().z + particleStartPosition.z))) / (function.GetAcceleration().z);
        else
            netTime = -(function.GetPosition().z + particleStartPosition.z) / function.GetVelocity().z;
        landing = particleStartPosition + 0.5f * function.GetAcceleration() * Mathf.Pow(netTime, 2) + function.GetVelocity() * netTime + function.GetPosition();
        return landing.y < netUpperBound && landing.y > netLowerBound && landing.x > -4.5f && landing.x < 4.5f;
        
    }
}
public class Function : System.IEquatable<Function>
{
    Vector3 acceleration;
    Vector3 velocity;
    Vector3 position;
    float timeOffset;
    List<Function> addedFuctions;

    public Function(Vector3? position = null, Vector3? velocity = null, Vector3? acceleration = null)
    {
        timeOffset = int.MinValue;
        
        this.position = position ?? Vector3.zero;
        this.velocity = velocity ?? Vector3.zero;
        this.acceleration = acceleration ?? Vector3.zero;
        addedFuctions = new List<Function>();
    }

    public Function(float timeOffset, Vector3? position = null, Vector3? velocity = null, Vector3? acceleration = null)
    {
        this.timeOffset = timeOffset;
        this.position = position ?? Vector3.zero;
        this.velocity = velocity ?? Vector3.zero;
        this.acceleration = acceleration ?? Vector3.zero;
        addedFuctions = new List<Function>();
    }

    public Vector3 Evaluate(float time)
    {
        Vector3 addFunctionValue = Vector3.zero;
        foreach (Function function in addedFuctions)
            addFunctionValue += function.Evaluate(time);
        
        if (time > timeOffset)
        {
            float currentTime = timeOffset > 0 ? time - timeOffset : time;

            return addFunctionValue + (acceleration / 2) * Mathf.Pow(currentTime, 2) + 
                    velocity * currentTime + position;
        }
        else
            return addFunctionValue;
    }

    public Function Flatten()
    {
        Vector3 acceleration = GetAcceleration();
        Vector3 velocity = GetVelocity();
        Vector3 position = GetPosition();
        foreach (Function function in addedFuctions)
        {
            Function f = function.Flatten();
            acceleration += f.GetAcceleration();
            velocity += f.GetVelocity();
            position += f.GetPosition();
        }
        Function fun = new Function(acceleration, velocity, position);
        fun.addedFuctions = new List<Function>();
        return fun;
    }

    public void AddFunction(Function function)
    {
        addedFuctions.Add(function);
    }

    public void SetTimeOffset(float offset)
    {
        timeOffset = offset;
    }

    public void SyncTimeOffset()
    {
        for(int i = 0; i < addedFuctions.Count; i++)
        {
            addedFuctions[i].SetTimeOffset(timeOffset);
        }
    }

    public static Function Gravity()
    {
        return new Function(null, null, Physics.gravity);
    }

    public override bool Equals(object obj)
    {
        Function function = obj as Function;
        if (function != null)
            return Equals(function);
        else
            return base.Equals(obj);
    }

    public bool Equals(Function other)
    {
        if (other != null)
            return position == other.position && timeOffset == other.timeOffset &&
                velocity == other.velocity && acceleration == other.acceleration;
        else
            return false;
    }
    /*
    public void Assign(Function other)
    {
        acceleration = other.GetAcceleration();
        velocity = other.GetVelocity();
        position = other.GetPosition();
        timeOffset = other.timeOffset;
        if (other.addedFuctions != null)
            addedFuctions = other.addedFuctions;
        else
            addedFuctions = new List<Function>();
    }*/

    public Vector3 GetVelocity()
    {
        return velocity;
    }
    public Vector3 GetAcceleration()
    {
        return acceleration;
    }
    public Vector3 GetPosition()
    {
        return position;
    }

    public bool Contains(Function function)
    {
        return Equals(function) || addedFuctions.Contains(function);
    }

    public int numberOfFunctions
    {
        get { return addedFuctions.Count + 1; }
    }

    public override string ToString()
    {
        System.Text.StringBuilder message = new System.Text.StringBuilder(
            "Function: \nacceleration: " + acceleration + "\nvelocity: " 
            + velocity + "\nposition: " + position + "\nTime Offset: " 
            + timeOffset + "\n");
        foreach (Function f in addedFuctions)
            message.Append(f.ToString());

        return message.ToString();
    }
}