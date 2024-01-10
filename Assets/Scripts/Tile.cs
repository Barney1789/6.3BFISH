using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tile : MonoBehaviour,IPunObservable
{
    
    public enum FishType { SingleFish, DoubleFish, TrioFish }
    public FishType fishType;
    private int points;
    private SpriteRenderer spriteRenderer;
    public Sprite singleFishSprite;
    public Sprite doubleFishSprite;
    public Sprite trioFishSprite;
    public bool HasPenguin { get; private set; } = false;
    private bool isOccupied = false; 
    public GameObject penguinPrefab; // Assign this in the inspector
    private PhotonView photonView;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        AssignRandomFishType();
    }

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            // Here, your code sends its data to other players
            stream.SendNext((int)fishType);
        } else {
            // Here, your code receives data from other players
            this.fishType = (FishType)stream.ReceiveNext();
            UpdateSprite(); // Update the tile's appearance based on the new fish type
        }
    }
    void OnMouseDown()
    {
        // Add points to the game manager's score
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(points);
        }

        // If the tile is not occupied, you can place a penguin
        if (!isOccupied)
        {
            PlacePenguin();
            isOccupied = true; // Mark this tile as occupied
        }
        else
        {
            Debug.Log("This tile is already occupied.");
        }
    }

    public void AssignRandomFishType()
    {
        // Create a weighted list for fish types
        List<FishType> weightedFishTypes = new List<FishType>
        {
            FishType.SingleFish, FishType.SingleFish, FishType.SingleFish, // 3 times
            FishType.DoubleFish, FishType.DoubleFish, // 2 times
            FishType.TrioFish // 1 time
        };

        // Select a random fish type based on the weighted probabilities
        fishType = weightedFishTypes[Random.Range(0, weightedFishTypes.Count)];
        SetPoints();
        UpdateSprite();
    }

    void SetPoints()
    {
        // Assign points based on fish type
        switch (fishType)
        {
            case FishType.SingleFish:
                points = 1;
                break;
            case FishType.DoubleFish:
                points = 2;
                break;
            case FishType.TrioFish:
                points = 3;
                break;
        }
    }

    void UpdateSprite()
    {
        // Change the sprite based on the fishType
        switch (fishType)
        {
            case FishType.SingleFish:
                spriteRenderer.sprite = singleFishSprite;
                break;
            case FishType.DoubleFish:
                spriteRenderer.sprite = doubleFishSprite;
                break;
            case FishType.TrioFish:
                spriteRenderer.sprite = trioFishSprite;
                break;
        }
    }

    public void SetFishType(FishType type)
    {
        fishType = type;
        SetPoints(); // Update the points based on the new fish type
        UpdateSprite(); // Update the sprite based on the new fish type
    }

    // Method to place a penguin on the tile
    public void PlacePenguin()
    {
        // Check if the current tile is not occupied and the player hasn't placed all penguins
        if (!isOccupied && GameManager.Instance.GetPenguinCount(PhotonNetwork.LocalPlayer) < 4)
        {
            // Call RPC to place the penguin across the network
            photonView.RPC("RPCPlacePenguin", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
            GameManager.Instance.IncrementPenguinCount(PhotonNetwork.LocalPlayer);
            isOccupied = true; // Mark this tile as occupied
        }
    }
    
    [PunRPC]
    void RPCPlacePenguin(int playerActorNumber)
    {
        // Instantiate the penguin for the player who invoked the RPC
        if (!isOccupied)
        {
            GameObject penguin = PhotonNetwork.Instantiate(penguinPrefab.name, transform.position, Quaternion.identity);
            penguin.GetComponent<Penguin>().SetOwner(playerActorNumber);
            isOccupied = true;
        }
    }
    [PunRPC]
    public void UpdateTileState(int state) {
        SetFishType((FishType)state);
    }
}