using Newtonsoft.Json;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public GameObject hexagonsParent; // Assign this in the inspector
    public int Score { get; private set; }
    private PhotonView photonView;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        photonView = GetComponent<PhotonView>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ShuffleTiles();
        }

        if (!PhotonNetwork.IsMasterClient)
        {
            //do not show the change sizes button
            GameObject.Find("ButtonChangeSizes").SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // ... Other methods ...

    public void AddPoints(int pointsToAdd)
    {
        Score += pointsToAdd;
        // Update score display logic here
    }

    // Method that is only executed by the master client to shuffle tiles and synchronize the state
    public void ShuffleTiles()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            List<int> tileStates = new List<int>();

            // Iterate over each Hexagon tile GameObject
            foreach (Transform child in hexagonsParent.transform)
            {
                Tile tile = child.GetComponent<Tile>();
                if (tile != null)
                {
                    tile.AssignRandomFishType();
                    tileStates.Add((int)tile.fishType); // Convert FishType to int for serialization
                }
            }

            // Send the shuffled state to all clients
            photonView.RPC("SyncShuffledTiles", RpcTarget.OthersBuffered, tileStates.ToArray());
        }
    }

    // RPC method to synchronize tile states across all clients
    [PunRPC]
    void SyncShuffledTiles(int[] tileStates)
    {
        int index = 0;
        foreach (Transform child in hexagonsParent.transform)
        {
            Tile tile = child.GetComponent<Tile>();
            if (tile != null && index < tileStates.Length)
            {
                tile.SetFishType((Tile.FishType)tileStates[index]);
                index++;
            }
        }
    }
}
