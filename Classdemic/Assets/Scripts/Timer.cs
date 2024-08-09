using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    //attached to  Empty Controller controls main timer

    public float timeRemaining; //the numerical number of time
    public bool timerIsRunning;  //this switches timer on/off
    public TMP_Text timeText;  //the UI element that will display the time
    int minutes;
    float seconds;


    void Update()
    {
        if (timerIsRunning)
        {
            timeRemaining -= Time.deltaTime;
            if(timeRemaining <= 0)
            {
                timeRemaining = 0;
                timeText.color = Color.red;
            }
            else
            {
                timeText.color = Color.green;
            }
            DisplayTime(timeRemaining);
        }
    }

    //take numerical time in seconds and convert to mins/seconds
    public void DisplayTime(float timeToDisplay)
    {
        minutes = Mathf.FloorToInt((timeToDisplay / 60) % 60); //minutes is seconds divided by 60 but mod 60 because 60 minutes make an hour
        seconds = Mathf.FloorToInt(timeToDisplay % 60);   //seconds is always mod 60 of the total seconds

        //timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        timeText.text = string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public void ToggleOnOff() //actually not used as we never turn the timer off
    {
        if (timerIsRunning)
        {
            timerIsRunning = false;
        }
        else
        {
            timerIsRunning = true;
        }
    }
}
