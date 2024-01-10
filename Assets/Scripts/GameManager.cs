using Newtonsoft.Json;
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Realtime; 

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }
    public GameObject hexagonsParent; // Assign this in the inspector
    public int Score { get; private set; }
    private PhotonView photonView;
    private Dictionary<int, int> penguinCounts = new Dictionary<int, int>();
    private int currentPlayerTurnIndex = 0;
    private List<int> playerTurnOrder = new List<int>();
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
        // Initialize turn order based on the player list in the room
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerTurnOrder.Add(player.ActorNumber);
             // Initialize penguin count for each player
            penguinCounts[player.ActorNumber] = 0;
        }// Master client starts the first turn
        if (PhotonNetwork.IsMasterClient)
        {
            ShuffleTiles();
            photonView.RPC("UpdateCurrentTurn", RpcTarget.AllBuffered, playerTurnOrder[currentPlayerTurnIndex]);
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
                    tile.AssignRandomFishType(); // Assuming this method assigns a random type and visually updates the tile
                    tileStates.Add((int)tile.fishType); // Convert FishType to int for serialization
                }
            }

            // Send the shuffled state to all clients
            photonView.RPC("SyncShuffledTiles", RpcTarget.OthersBuffered, tileStates.ToArray());
        }
    }
    // Call this method to get how many penguins a player has placed
    public int GetPenguinCount(Player player)
    {
        if (penguinCounts.TryGetValue(player.ActorNumber, out int count))
        {
            return count;
        }
        return 0;
    }

     // Call this method to increment the penguin count for a player
    public void IncrementPenguinCount(Player player)
    {
        int count = GetPenguinCount(player);
        penguinCounts[player.ActorNumber] = count + 1;

        // After incrementing, check if the player has placed all their penguins
        if (count + 1 >= 4)
        {
            // If so, advance the turn
            AdvanceTurn();
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

    public bool CanPlacePenguin(Player player) //dont allow more than 4 pengüns
    {
        return GetPenguinCount(player) < 4;
    }

    [PunRPC]
    void UpdateCurrentTurn(int currentPlayerIndex)
    {
        currentPlayerTurnIndex = currentPlayerIndex;
        Debug.Log("It is now player " + playerTurnOrder[currentPlayerTurnIndex] + "'s turn.");
    }

    // Call this method to advance the turn
    public void AdvanceTurn()
    {
        // Make sure we increment first before modulus
        currentPlayerTurnIndex = (currentPlayerTurnIndex + 1) % playerTurnOrder.Count;
        
        // Check if the next player has already placed all penguins
        while (penguinCounts.ContainsKey(playerTurnOrder[currentPlayerTurnIndex]) && 
               penguinCounts[playerTurnOrder[currentPlayerTurnIndex]] >= 4)
        {
            // Skip to the next player if the current one is done
            currentPlayerTurnIndex = (currentPlayerTurnIndex + 1) % playerTurnOrder.Count;
        }
        
        // Update the turn for all clients
        photonView.RPC("UpdateCurrentTurn", RpcTarget.AllBuffered, currentPlayerTurnIndex);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playerTurnOrder.Remove(otherPlayer.ActorNumber); // Remove the player from the turn order
        penguinCounts.Remove(otherPlayer.ActorNumber); // Remove their penguin count

        // We need to check if the player who left was the current player and advance the turn if necessary
        if (otherPlayer.ActorNumber == playerTurnOrder[currentPlayerTurnIndex])
        {
            AdvanceTurn();
        }
    }

    // Method to check if it's the local player's turn
    public bool IsLocalPlayerTurn()
    {
        return PhotonNetwork.LocalPlayer.ActorNumber == playerTurnOrder[currentPlayerTurnIndex];
    }
}
