using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public enum PicEvent { Toggle, Slider}

//[RequireComponent(typeof(UnityEngine.UI.Image))]
public class PassDownEvents : MonoBehaviour {

    public SpecialAction myPass;
    public PicEvent myEvent;
    public EventTrigger.TriggerEvent myFunction;
    public Color goodColor;
    public Color badColor;
    UnityEngine.UI.Image myImage;
    bool gotImage;
	// Use this for initialization
	void Start () {
        try
        {
            myImage = GetComponent<UnityEngine.UI.Image>();
            gotImage = true; 
        }
        catch
        {
            gotImage = false;
            //myFunction.Invoke();
        }
        
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        if(gotImage)
        {
            switch (myEvent)
            {
                case PicEvent.Toggle:
                    ChangeImage();
                    break;
                case PicEvent.Slider:
                    IncrementImage();
                    break;
            }
        }
	}
    /*
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball")
            myPass.MyTriggerEnter(other);
    }
    */
    void ChangeImage()
    {
        if(gotImage)
        {
            myImage.color = myPass.GetHittable()? goodColor : badColor;
        }
    } 

    void IncrementImage()
    {
        myImage.type = UnityEngine.UI.Image.Type.Filled;
        myImage.fillAmount = myPass.GetHitTimer();
        Color colorDiff = goodColor - badColor;
        myImage.color = goodColor;// - colorDiff * (1-myImage.fillAmount);
    }

}
