using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZoomAxis {X ,Y ,Z ,XY ,XZ ,YZ ,XYZ}


public class CameraFollow : MonoBehaviour {
    public bool allowZoom;
    public ZoomAxis zoomAxis;
    public float offset;

    public float zoomSensitivity = .5f;
    public float zoomSmooth = .1f;
    public float followSmooth = .25f;
    public Vector3 baseReferencePosition;

    [System.NonSerialized]
    public Camera camera;

    Vector3 referencePosition;
    [SerializeField]
    Vector3 basePosition;
    [SerializeField]
    Vector3 deltaPosition;
    Vector3 newPosition;

    Vector3 zerooneone;
    
    float lastAngle;
    [SerializeField]
    float baseAngle;
    [SerializeField]
    float currentAngle;
    float angleDiff;

    GameObject pivot;

    // Use this for initialization
    void Start () {

        //This is the point that the camera rotates around
        pivot = new GameObject("CameraPivot");
        camera = GetComponent<Camera>();
        pivot.transform.parent = transform.parent;
        pivot.transform.localPosition = transform.localPosition + baseReferencePosition;

        //the base position used in the Follow Angle calculations
        basePosition = transform.localPosition;

        //set the last angle to the current angle
        lastAngle = transform.localEulerAngles.x;

        //calculate the angle between the pivot and the camera
        deltaPosition = transform.localPosition - pivot.transform.localPosition; //+ lerpVector;
        deltaPosition.Scale(zerooneone);
        baseAngle = Mathf.Atan2(deltaPosition.y, deltaPosition.z);
        currentAngle = baseAngle;

        //used with the vector scale function to get rid of the X axis
        zerooneone = new Vector3(0, 1, 1);

        //sets the current offset between the player and the camera to the current position
        SetOffset();
        
    }
	
	// Update is called once per frame
	void Update ()
    {
        //only zoom when allowed to
        if (allowZoom)
            Zoom();

        //Move the camera around the pivot when the camera angle changes
        FollowAngle();


    }

    void FollowAngle()
    {
        //find the difference between the previous angle, and the current angle in radians
        angleDiff = (transform.localEulerAngles.x - lastAngle) * Mathf.Deg2Rad;

        //find the distance between the pivot point and the current camera position
        deltaPosition = transform.localPosition - pivot.transform.localPosition;
        deltaPosition.Scale(Vector3.up + Vector3.forward); //get rid of the x component

        //find the angle of elevation from the pivot to the camera position
        baseAngle = Mathf.Atan2(deltaPosition.y, deltaPosition.z);

        //subtract the camera angle differential from the angle of elevation to get the current angle
        //you subtract because you want the camera to move upwards as you look down so you can still
        //see the player
        currentAngle -= angleDiff * Mathf.Rad2Deg;

        //calculate the new position
        newPosition.y = -Mathf.Sin(currentAngle * Mathf.Deg2Rad) * (basePosition - pivot.transform.localPosition).magnitude + basePosition.y;
        newPosition.z = -Mathf.Cos(currentAngle * Mathf.Deg2Rad) * (basePosition - pivot.transform.localPosition).magnitude;

        //make the current angle the previous angle
        lastAngle = transform.localEulerAngles.x;

        //move the camera to the new position smoothly
        transform.localPosition = Vector3.Lerp(transform.localPosition,newPosition, followSmooth);

    }
    void SetOffset()
    {
        //make the current position as the offset
        switch (zoomAxis)
        {
            case ZoomAxis.X:
                offset = transform.localPosition.z;
                break;
            case ZoomAxis.Y:
                offset = transform.localPosition.z;
                break;
            case ZoomAxis.Z:
                offset = transform.localPosition.z;
                break;
        }
    }

    void OnDrawGizmosSelected()
    {
        //Draw the pivot point in the scene view
        Gizmos.color = Color.yellow;
        if(pivot == null)
            Gizmos.DrawSphere(transform.localPosition + baseReferencePosition + transform.parent.position, .1f);
        else
            Gizmos.DrawSphere(pivot.transform.localPosition + transform.parent.position, .1f);
    }

    void Zoom()
    {
        //Zoom out with the scroll wheel of the mouse
        offset += Input.mouseScrollDelta.y * zoomSensitivity;
        switch (zoomAxis)
        {
            case ZoomAxis.X:

                break;
            case ZoomAxis.Y:

                break;
            case ZoomAxis.Z:
                //zoom out the base position that is used for the FollowAngle calculations
                basePosition = Vector3.Scale(basePosition, new Vector3(1, 1, 0)) + Vector3.forward * offset;

                break;
        }
    }

    public void SetZoomSensitivity(float input)
    {
        zoomSensitivity = input;
    }
}
