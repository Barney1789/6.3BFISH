using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Penguin : MonoBehaviour
{
    // The player number or ID of the penguin's owner
    public int ownerID;

    // Method to set the owner of the penguin
    public void SetOwner(int playerID)
    {
        ownerID = playerID;
        // Additional logic to update the penguin based on the owner
    }
}
