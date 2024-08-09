using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Avatar : MonoBehaviour
{
    public int virusload;
    public int maskstatus;
    public int handstatus;

    public void InteractWithAvatar(Avatar otherAvatar, float interactionTime, bool physicalContact)
    {
        // Determine the transmitter and receiver
        Avatar transmitter = this;
        Avatar receiver = otherAvatar;

        // Calculate the rate of increase based on transmitter's virusload
        float rateOfIncrease = transmitter.virusload * interactionTime;

        // Apply maskstatus reduction
        switch (receiver.maskstatus)
        {
            case 1:
                rateOfIncrease *= 0.5f;
                break;
            case 2:
                rateOfIncrease *= 0.01f;
                break;
        }

        // Apply the increase in virusload based on maskstatus and interactionTime
        receiver.virusload += (int)rateOfIncrease;

        // Apply the increase in virusload based on handstatus and physicalContact
        int handIncreaseFactor = 0;
        switch (physicalContact)
        {
            case false:
                handIncreaseFactor = 0;
                break;
            case true when receiver.handstatus == 1:
                handIncreaseFactor = 2;
                break;
            case true when receiver.handstatus == 2:
                handIncreaseFactor = 4;
                break;
        }

        receiver.virusload += transmitter.virusload * handIncreaseFactor;
    }
}


