using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameUI : MonoBehaviour {
    GameObject myMenu;
    GameObject currentlyActive;
    GameObject menuStart;
    SpecialAction specialMe;
    public bool toggleMenu;
    float previousTimeScale;
    public bool ToggleMenu
    {
        set
        {
            toggleMenu = value;
        }
    }

	// Use this for initialization
	void Start () {
        myMenu = transform.parent.Find("Menu").gameObject;
        menuStart = myMenu.transform.Find("Panel/Menu Objects").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		if(toggleMenu)
        {
            if (myMenu.activeInHierarchy)
            {
                CloseEverything();
                specialMe.Pause(false);
                Time.timeScale = previousTimeScale;
            }
            else
            {
                myMenu.SetActive(true);
                specialMe.Pause(true);
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            toggleMenu = false;
        }
	}

    void CloseEverything()
    {
        if (currentlyActive != null)
        {
            currentlyActive.SetActive(false);
            switch(currentlyActive.name)
            {
                case "Rules ScrollView":
                case "Training Options":
                case "Camera ScrollView":
                    currentlyActive.transform.parent.parent.gameObject.SetActive(false);
                    break;
            }
        }
        menuStart.SetActive(true);
        myMenu.SetActive(false);
    }

    public void ButtonClick(GameObject you)
    {
        you.transform.parent.gameObject.SetActive(false);
        switch (you.name)
        {
            case "Call Timeout":
                you.transform.parent.gameObject.SetActive(true);
                break;
            case "Options":
                currentlyActive = you.transform.parent.parent.Find("Options List").gameObject;
                currentlyActive.SetActive(true);
                break;
            case "Camera":
                you.transform.parent.parent.Find("Options ScrollView").gameObject.SetActive(true);
                currentlyActive = you.transform.parent.parent.Find("Options ScrollView/Panel/Camera ScrollView").gameObject;
                currentlyActive.SetActive(true);
                break;
            case "Back Options ScrollView":
                you.transform.parent.gameObject.SetActive(true);
                you.transform.parent.parent.gameObject.SetActive(false);
                currentlyActive.SetActive(false);
                break;
            default:
                
                print("Don't have a Button Click case for " + you.name + " yet");
                break;
        }
        
    }

    public void Disable(GameObject you)
    {
        you.transform.parent.gameObject.SetActive(false);
    }

    public void Enable(GameObject them)
    {
        them.SetActive(true);
        currentlyActive = them;
    }

    public void SetToggleMenu(bool toggle, SpecialAction you)
    {
        specialMe = you;
        toggleMenu = toggle;
    }
}
