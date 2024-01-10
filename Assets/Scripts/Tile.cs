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
    public void OnMouseDown()
    {
        if (GameManager.Instance.IsLocalPlayerTurn() && GameManager.Instance.CanPlacePenguin(PhotonNetwork.LocalPlayer) && !isOccupied)
        {
            PlacePenguin();
        }
        else
        {
            Debug.Log("It's not your turn, or you have already placed all your penguins, or the tile is occupied.");
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
        if (!isOccupied && GameManager.Instance.CanPlacePenguin(PhotonNetwork.LocalPlayer))
        {
            // Instantiate the penguin slightly above the tile to ensure it's visible
            Vector3 spawnPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);
            GameObject penguin = PhotonNetwork.Instantiate(penguinPrefab.name, spawnPosition, Quaternion.identity);

            // Set the owner and other necessary properties of the penguin here...
            // For example: penguin.GetComponent<Penguin>().SetOwner(PhotonNetwork.LocalPlayer.ActorNumber);

            photonView.RPC("RPCPlacePenguin", RpcTarget.AllBuffered, PhotonNetwork.LocalPlayer.ActorNumber);
            
            isOccupied = true; // Mark the tile as occupied
            GameManager.Instance.IncrementPenguinCount(PhotonNetwork.LocalPlayer); // Increment the count of penguins for the player
        }
        else if (isOccupied)
        {
            Debug.Log("This tile is already occupied.");
        }
        else
        {
            Debug.Log("It's not your turn or you have already placed all your penguins.");
        }
    }
    
    [PunRPC]
    void RPCPlacePenguin(int playerActorNumber)
    {
        if (!isOccupied)
        {
            GameObject penguinObject = PhotonNetwork.Instantiate(penguinPrefab.name, transform.position, Quaternion.identity);
            Penguin penguin = penguinObject.GetComponent<Penguin>();
            if (penguin != null)
            {
                penguin.SetOwner(playerActorNumber);
            }
            isOccupied = true;
        }
    }

    [PunRPC]
    public void UpdateTileState(int state) {
        SetFishType((FishType)state);
    }
}