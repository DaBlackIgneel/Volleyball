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
    List<Function> functionPath;
    const float regFixedDeltaTime = 1 / 50f;

    // Use this for initialization
    void Start () {
        functionPath = new List<Function>();
        //the component that controlls the amount of particles emmitted for the aiming
        myBurst = new ParticleSystem.Burst[1];
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
        }
    }

    //stops the particle creation and deletes all particles
    public void ClearParticles()
    {
        if (mySystem.isPlaying)
        {
            mySystem.Clear();
            mySystem.Stop();
        }
    }

    public void SetFunction(Function function)
    {
        functionPath.Clear();
        functionPath.Add(function);
    }

    public void AddFunction(Function function)
    {
        functionPath.Add(function);
    }

    public void ClearFunction()
    {
        functionPath.Clear();
    }

    public void AddGravity()
    {
        AddFunction(Function.Gravity());
    }

    public bool FunctionExists(Function function)
    {
        return functionPath.Contains(function);
    }

    public void SetPathSpread(float pathSpread)
    {
        this.pathSpread = pathSpread;
    }

    //Draws the Path using a particle system;
    public void SimulatePath()
    {
        GetParticles();
        
        if(mySystem.particleCount > 0)
        {
            float time = pathSpread * regFixedDeltaTime / mySystem.particleCount;
            for (int i = 0; i < myParticle.Length; i++)
            {
                Vector3 pathPosition = Vector3.zero;
                foreach (Function function in functionPath)
                    pathPosition += function.Evaluate(i * time);
                myParticle[i].position = particleStartPosition + pathPosition;
                myParticle[i].remainingLifetime = 100000;
            }
            //move the particles to the predicted position
            mySystem.SetParticles(myParticle, myParticle.Length);
        }
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
        List<Function> addedFuctions = new List<Function>();
    }

    public Function(float timeOffset, Vector3? position = null, Vector3? velocity = null, Vector3? acceleration = null)
    {
        this.timeOffset = timeOffset;
        this.position = position ?? Vector3.zero;
        this.velocity = velocity ?? Vector3.zero;
        this.acceleration = acceleration ?? Vector3.zero;
        List<Function> addedFuctions = new List<Function>();
    }

    public Vector3 Evaluate(float time)
    {
        Vector3 addFunctionValue = Vector3.zero;
        if (addedFuctions != null)
        {
            foreach (Function function in addedFuctions)
                addFunctionValue += function.Evaluate(time);
        }
        if (time > timeOffset)
        {
            float currentTime = timeOffset > 0 ? time - timeOffset : time;

            return (acceleration / 2) * Mathf.Pow(currentTime, 2) + 
                    velocity * currentTime + 
                    position;
        }
        else
            return Vector3.zero;
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
            return position == function.position && timeOffset == function.timeOffset &&
                velocity == function.velocity && acceleration == function.acceleration;
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
    public Vector3 GetVelocity()
    {
        return velocity;
    }
}