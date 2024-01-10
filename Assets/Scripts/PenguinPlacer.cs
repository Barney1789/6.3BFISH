using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PenguinPlacer : MonoBehaviour
{
    public GameObject penguinPrefab; // Assign this in the Unity Editor
    private PhotonView photonView;
    public static PenguinPlacer Instance;

    void Update(){
    if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
            if (Physics.Raycast(ray, out hit, 100.0f))
                {
                    Tile tile = hit.collider.GetComponent<Tile>();
                    if (tile != null)
                    {
                        tile.PlacePenguin();
                    }
                }
        }
    }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

}

