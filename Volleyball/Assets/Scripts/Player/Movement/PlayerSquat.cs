using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSquat : MonoBehaviour, IInitializable {

    public bool squat;
    bool dive;
    [Range(.001f,.5f)]
    public float squatSpeed =.1f;

    CapsuleCollider[] myColliders;
    CapsuleCollider myCollider;
    CapsuleCollider myTrigger;

    float squatThreshhold = 0.01f;
    Vector2 originMeshHeightY;
    Vector2 squatMeshHeightY;
    Vector2 originColliderHeightY;
    Vector2 squatColliderHeightY;
    Vector2 originTriggerHeightY;
    Vector2 squatTriggerHeightY;
    Vector3 originalGroundPosition;
    Vector3 squatGroundPosition;

    public float HEIGHT = 2f;
    public float SQUAT_HEIGHT = 1f;

    SpecialAction player;

    // Use this for initialization
    public void Initialize(SpecialAction player) {
        this.player = player;
        myColliders = player.myParent.parent.GetComponents<CapsuleCollider>();
        //Gets the the component that controlls the collision bounds
        myCollider = player.myParent.parent.GetComponent<CapsuleCollider>();

        //gets the component that controlls the ball collisions
        myTrigger = myColliders[myColliders.Length - 1];

        //the original heights for the mesh, collider, and collider for the ball
        originColliderHeightY = new Vector2(myCollider.height, myCollider.center.y);
        originMeshHeightY = new Vector2(transform.localScale.y, transform.localPosition.y);
        originTriggerHeightY = new Vector2(myTrigger.height, myTrigger.center.y);
        originalGroundPosition = player.myMovement.ground.localPosition;

        //the predefined crouch heights for the mesh, collider, and the collider for the ball
        squatColliderHeightY = new Vector2(1, 1f);
        squatMeshHeightY = new Vector2(0.5f, 0.5f);
        squatGroundPosition = originalGroundPosition + Vector3.up * .3f;
        squatTriggerHeightY = new Vector2(originTriggerHeightY.x, originTriggerHeightY.y - .75f);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //changes the height of the player when you want to squat and returns
    //the height back to normal when you don't want to squat
    public void Squat()
    {
        float hittingOffset;
        hittingOffset = player.Hitting ? player.myParent.transform.position.y : 0;
        if (dive || (squat && !player.myMovement.isWalking))
        {
            Dive();
        }
        else
        {
            if (squat)
            {
                if (myCollider.center.y > squatColliderHeightY.y)
                {
                    myCollider.height = squatColliderHeightY.x;
                    myCollider.center = new Vector3(myCollider.center.x, squatColliderHeightY.y, myCollider.center.z);
                    transform.localScale = new Vector3(transform.localScale.x, squatMeshHeightY.x, transform.localScale.z);
                    transform.localPosition = new Vector3(transform.localPosition.x, squatMeshHeightY.y + hittingOffset, transform.localPosition.z);
                    myTrigger.height = squatTriggerHeightY.x;
                    myTrigger.center = new Vector3(myTrigger.center.x, squatTriggerHeightY.y, myTrigger.center.z);
                    player.myMovement.ground.localPosition = squatGroundPosition;
                }
                //else if (Mathf.Abs(myCollider.height - squatColliderHeightY.x) > squatThreshhold)
                {
                    myCollider.height = Mathf.Lerp(myCollider.height, squatColliderHeightY.x, squatSpeed);
                    myCollider.center = Vector3.Lerp(myCollider.center, new Vector3(myCollider.center.x, /*squatColliderHeightY.y*/1, myCollider.center.z), squatSpeed);
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, squatMeshHeightY.x, transform.localScale.z), squatSpeed);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, squatMeshHeightY.y + hittingOffset, transform.localPosition.z), squatSpeed);
                    myTrigger.height = Mathf.Lerp(myTrigger.height, squatTriggerHeightY.x, squatSpeed);
                    myTrigger.center = Vector3.Lerp(myTrigger.center, new Vector3(myTrigger.center.x, squatTriggerHeightY.y, myTrigger.center.z), squatSpeed);
                    player.myMovement.ground.localPosition = Vector3.Lerp(player.myMovement.ground.localPosition, squatGroundPosition, squatSpeed);

                }

            }
            else
            {

                if (myCollider.center.y < originColliderHeightY.y)
                {
                    myCollider.height = originColliderHeightY.x;
                    myCollider.center = new Vector3(myCollider.center.x, originColliderHeightY.y, myCollider.center.z);
                    transform.localScale = new Vector3(transform.localScale.x, originMeshHeightY.x, transform.localScale.z);
                    transform.localPosition = new Vector3(transform.localPosition.x, originMeshHeightY.y + hittingOffset, transform.localPosition.z);
                    myTrigger.height = originTriggerHeightY.x;
                    myTrigger.center = new Vector3(myTrigger.center.x, originTriggerHeightY.y, myTrigger.center.z);
                    player.myMovement.ground.localPosition = originalGroundPosition;
                }
                else if (Mathf.Abs(myCollider.height - originColliderHeightY.x) > squatThreshhold)
                {
                    myCollider.height = Mathf.Lerp(myCollider.height, originColliderHeightY.x, squatSpeed);
                    myCollider.center = Vector3.Lerp(myCollider.center, new Vector3(myCollider.center.x, originColliderHeightY.y, myCollider.center.z), squatSpeed);
                    transform.localScale = Vector3.Lerp(transform.localScale, new Vector3(transform.localScale.x, originMeshHeightY.x, transform.localScale.z), squatSpeed);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, new Vector3(transform.localPosition.x, originMeshHeightY.y + hittingOffset, transform.localPosition.z), squatSpeed);
                    myTrigger.height = Mathf.Lerp(myTrigger.height, originTriggerHeightY.x, squatSpeed);
                    myTrigger.center = Vector3.Lerp(myTrigger.center, new Vector3(myTrigger.center.x, originTriggerHeightY.y, myTrigger.center.z), squatSpeed);
                    player.myMovement.ground.localPosition = Vector3.Lerp(player.myMovement.ground.localPosition, originalGroundPosition, squatSpeed);
                }

            }
        }

    }

    void Dive()
    {
        float hittingOffset;
        hittingOffset = player.Hitting ? player.myParent.transform.position.y : 0;
        if (squat && !player.myMovement.isWalking)
        {

            float relativeAngle = (Mathf.Atan2(player.myMovement.rb.velocity.x, player.myMovement.rb.velocity.z) * Mathf.Rad2Deg - player.myParent.parent.eulerAngles.y);
            relativeAngle -= ((int)(relativeAngle / 360)) * 360;
            relativeAngle *= -1;
            Vector3 tempAxis = Vector3.up * relativeAngle + Vector3.right * (Mathf.Atan2(player.myMovement.rb.velocity.x, player.myMovement.rb.velocity.z)) * Mathf.Rad2Deg + Vector3.forward * player.myParent.parent.eulerAngles.y;
            Vector3 rotationAxis = Vector3.right * Mathf.Cos(relativeAngle * Mathf.Deg2Rad) + Vector3.forward * Mathf.Sin(relativeAngle * Mathf.Deg2Rad);
            player.myParent.localEulerAngles = (rotationAxis).normalized * 60;
            dive = true;
        }
        else
        {
            player.myParent.localEulerAngles = Vector3.zero;

            if (player.myParent.localEulerAngles.x + player.myParent.localEulerAngles.z < 1f)
            {
                dive = false;
            }
        }
    }
}
