using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZoomAxis {X ,Y ,Z ,XY ,XZ ,YZ ,XYZ}

public class CameraFollow : MonoBehaviour {
    public bool allowZoom;
    public ZoomAxis zoomAxis;
    public float offset;
    //[Range(.01f,4)]
    public float zoomSensitivity = .5f;
    [Range(.01f, 1)]
    public float zoomSmooth = .1f;
    public Vector3 baseReferencePosition;
    Vector3 referencePosition;
    float baseRadius;
    Vector3 originalPosition;
    [SerializeField]
    Vector3 basePosition;
    [SerializeField]
    Vector3 deltaPosition;
    Vector3 addPosition;
    Vector3 zerooneone;

    float originalAngle;
    [SerializeField]
    float baseAngle;
    [SerializeField]
    float currentAngle;
    GameObject pivot;
    float angleDiff;
    float lerpOffset;
    
    MyMouseLook myMouseLook;
	// Use this for initialization
	void Start () {
        pivot = new GameObject("CameraPivot");
        pivot.transform.parent = transform.parent;
        pivot.transform.localPosition = transform.localPosition + baseReferencePosition;
        basePosition = transform.localPosition;
        originalPosition = basePosition;
        myMouseLook = GetComponent<MyMouseLook>();
        originalAngle = transform.localEulerAngles.x;
        deltaPosition = transform.localPosition - pivot.transform.localPosition; //+ lerpVector;
        deltaPosition.Scale(zerooneone);
        baseAngle = Mathf.Atan2(deltaPosition.y, deltaPosition.z);
        zerooneone = new Vector3(0, 1, 1);
        GetOffset();
        currentAngle = baseAngle;
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (allowZoom)
            Zoom();
        FollowAngle();


    }

    void FollowAngle()
    {
        angleDiff = (transform.localEulerAngles.x - originalAngle) * Mathf.Deg2Rad;
        deltaPosition = transform.localPosition - pivot.transform.localPosition;
        deltaPosition.Scale(zerooneone);
        baseAngle = Mathf.Atan2(deltaPosition.y, deltaPosition.z);
        currentAngle += -angleDiff * Mathf.Rad2Deg;
        addPosition.y = -Mathf.Sin(currentAngle * Mathf.Deg2Rad) * (basePosition - pivot.transform.localPosition).magnitude + basePosition.y;
        addPosition.z = -Mathf.Cos(currentAngle * Mathf.Deg2Rad) * (basePosition - pivot.transform.localPosition).magnitude;

        originalAngle = transform.localEulerAngles.x;
        transform.localPosition = Vector3.Lerp(transform.localPosition,addPosition,0.25f);

    }
    void GetOffset()
    {
        
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
        
        Gizmos.color = Color.yellow;
        if(pivot == null)
            Gizmos.DrawSphere(transform.localPosition + baseReferencePosition + transform.parent.position, .1f);
        else
            Gizmos.DrawSphere(pivot.transform.localPosition + transform.parent.position, .1f);
    }

    void Zoom()
    {
        offset += Input.mouseScrollDelta.y * zoomSensitivity;
        switch (zoomAxis)
        {
            case ZoomAxis.X:

                break;
            case ZoomAxis.Y:

                break;
            case ZoomAxis.Z:
                lerpOffset = Mathf.Lerp(basePosition.z, offset, zoomSmooth);
                basePosition = Vector3.Scale(basePosition, new Vector3(1, 1, 0)) + Vector3.forward * lerpOffset;

                break;
        }
    }

    public void SetZoomSensitivity(float input)
    {
        zoomSensitivity = input;
    }
}
