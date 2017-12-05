using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour, IInitializable
{

    public Vector3 aimSpot;
    [SerializeField]
    Vector3 aimDir;
    Vector3 originAimDir;
    SpecialAction player;

    public void Initialize(SpecialAction player)
    {
        this.player = player;
    }

    public Vector3 UserAim()
    {
        //make sure that the mouse is visible
        Cursor.visible = true;

        //have the directon be where the mouse is currently pointing to
        Ray mousePoint = Camera.main.ScreenPointToRay(Input.mousePosition);

        //normalize the current direction so that the magnitude == 1
        aimDir = mousePoint.direction;
        aimDir = aimDir.normalized;
        
        return aimDir;
    }

    public ShootCapsule ComputerAim()
    {
        Pass aimSpotInfo = player.Court.LocalRelate.AimSpotInfo(player);
        float time = Strategy.PassSpeedToFloat(aimSpotInfo.speed);
        bool addSpin = false;
        if (aimSpotInfo.position <= player.team.numberOfActivePlayers)
        {
            if (aimSpotInfo.type == PassType.Location)
                aimSpot = LocationRelation.StrategyLocationToCourt(aimSpotInfo.location, player.currentSide);
            else if(aimSpotInfo.type == PassType.Person)
            {
                SpecialAction Passplayer = player.team.players.Find(x => x.currentPosition == aimSpotInfo.position);
                print("aimSpotInfo_pass_position: " + aimSpotInfo.position);

                float horizontalSpeedAimSpotConst = Strategy.PassSpeedToFloat(PassSpeed.SuperQuick)*2 / (Strategy.PassSpeedToFloat(PassSpeed.SuperQuick) +time);
                aimSpot = Vector3.ProjectOnPlane(Passplayer.transform.position + Passplayer.myMovement.rb.velocity * time *horizontalSpeedAimSpotConst, Vector3.up)
                    + (Passplayer.mySquat.HEIGHT + Passplayer.myMovement.transform.position.y) * Vector3.up + aimSpotInfo.offset;
                aimSpot += Vector3.up * (Passplayer.myMovement.rb.velocity.y * time + .5f * Physics.gravity.y * time * time + .5f);
                player.passPlayer = Passplayer;
                
                if (CourtScript.FloatToSide(aimSpot.z) != player.currentSide)
                    aimSpot.z = 0.25f * (float)player.currentSide;
            }
            if (aimSpot.y < player.mySquat.HEIGHT)
                aimSpot.y = player.mySquat.HEIGHT;
        }
        //If you're trying to pass the ball to a player who doesn't exist then, then hit the ball
        //over the net.
        else
        {
            aimSpot = player.Court.LocalRelate.AimSpot(CourtScript.OppositeSide(player.currentSide));
            addSpin = true;
        }
        player.Court.LocalRelate.aimSpot = aimSpot;
        Vector3 distance = aimSpot - player.vBall.transform.position;

        float timeTillNet = (player.Court.net.transform.position.z - player.vBall.transform.position.z)/distance.z;
        float distanceToNet = (distance * timeTillNet).magnitude;
        float netHeight = player.Court.NetHeight;
        ShootCapsule aimDir = new ShootCapsule();
        aimDir.aimDirection = distance;
        aimDir.obstacles.Add(new Obstacle(distanceToNet, netHeight));
        aimDir.shotTime = time;
        aimDir.aimSpot = aimSpot;
        aimDir.currentPosition = player.vBall.transform.position;
        aimDir.addSpin = addSpin;
        return aimDir;
    }
    public ShootCapsule ComputerAimFor(CourtDimensions.Rectangle aimArea)
    {
        aimSpot = player.Court.LocalRelate.AimSpot(aimArea);
        player.Court.LocalRelate.aimSpot = aimSpot;
        Vector3 distance = aimSpot - player.vBall.transform.position;

        float timeTillNet = (player.Court.net.transform.position.z - player.vBall.transform.position.z) / distance.z;
        float distanceToNet = (distance * timeTillNet).magnitude;
        float netHeight = player.Court.NetHeight;
        ShootCapsule aimDir = new ShootCapsule();
        aimDir.aimDirection = distance;
        aimDir.obstacles.Add(new Obstacle(distanceToNet, netHeight));
        aimDir.shotTime = Random.Range(Strategy.PassSpeedToFloat(PassSpeed.SuperQuick),Strategy.PassSpeedToFloat(PassSpeed.Quick));
        aimDir.aimSpot = aimSpot;
        aimDir.currentPosition = player.vBall.transform.position;
        aimDir.addSpin = true;
        return aimDir;
    }
}
