using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Character : MonoBehaviour
{
    //attached to Character top level
    #region Variables
    public int viralLoad; //number of viruses on the character
    public int maskStatus; //levels of mask wearing
    public int handStatus; //levels of hand washing
    public int immunity; //susceptiblity is 0, resistance is 1 but could be more subtle

    public int daysInfected;  //this affects viral load and infectivity in both a positive and negative way - 0-2, not infective regardless of load but load increases by day, 
    //then decreases after 7 days to be nothing after 14

    public GameObject virusIndicator; // report on virus level on character - currently a cylinder
    public GameObject maskIndicator;  //object to report on mask wearing of character - currently a cylinder
    public GameObject[] handIndicator; //object to report on hand washing of character - currently a cylinder
    public GameObject charbody; //the main body of hte character

    //text fields for on-screen info labels
    public TMP_Text nametag;
    public TMP_Text maskstat;
    public TMP_Text handstat;
    public TMP_Text viruslev;

    //materials to indicate statuses
    public Material maskOn;
    public Material maskOff;
    public Material handsSanit;
    public Material handsClean;
    public Material handsDirty;
    public Material immunised;
    public Material plain;

    //it seems silly to define colours with terms like green, yellow red when these are built in... may also conflict
    private Color green = new Color(0f, 1f, 0f, 0.2f); // Nearly transparent green
    private Color yellow = new Color(1f, 1f, 0f, 0.5f); // Yellow
    private Color red = new Color(1f, 0f, 0f, 1f); // Opaque red
    #endregion

    public void UpdateMaskIndicator()
    {
        if(maskStatus == 0) //mask off
        {
            maskIndicator.GetComponent<Renderer>().material = maskOff;
            maskstat.text = "Mask Off";
        }
        else
        {
            maskIndicator.GetComponent<Renderer>().material = maskOn;
            maskstat.text = "Mask On";
        }
    }

    public void UpdateHandsIndiactor()
    {
        switch (handStatus)
        {
            case 0: //hands santised
                foreach (GameObject hand in handIndicator)
                {
                    hand.GetComponent<Renderer>().material = handsDirty;
                }
                handstat.text = "Hands Dirty";
                break;
            case 1: //hands clean
                foreach(GameObject hand in handIndicator)
                {
                    hand.GetComponent<Renderer>().material = handsClean;
                }          
                handstat.text = "Hands Clean";
                break;
            case 2: //hands dirty
                foreach (GameObject hand in handIndicator)
                {
                    hand.GetComponent<Renderer>().material = handsSanit;
                }
                handstat.text = "Hands Sanitized";
                break;
        }
    }

    public void UpdateVirusIndicator()
    {
        float normalizedValue = Mathf.Clamp01(viralLoad / 100f); //Clamp01 returns value between 0 and 1

        // Interpolate between the colors based on the normalized value.
        Color lerpedColor = Color.Lerp(green, yellow, normalizedValue);
        lerpedColor = Color.Lerp(lerpedColor, red, normalizedValue);

        virusIndicator.GetComponent<Renderer>().material.color = lerpedColor;
        viruslev.text = "Viral Load " + viralLoad.ToString();
    }

    public void UpdateImmunisedIndicator()
    {
        if(immunity != 0)
        {
            charbody.GetComponent<Renderer>().material = immunised;
        }
    }

    public void SetAllMaterialsToPlain()
    {
        charbody.GetComponent<Renderer>().material = plain;
        virusIndicator.GetComponent<Renderer>().material.color = Color.white;
        foreach (GameObject hand in handIndicator)
        {
            hand.GetComponent<Renderer>().material = plain;
        }
        maskIndicator.GetComponent<Renderer>().material = plain;
    }
}
