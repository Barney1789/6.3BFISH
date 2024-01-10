using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
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

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        AssignRandomFishType();
    }

    void OnMouseDown()
    {
        // Add points to the game manager's score
        GameManager.Instance.AddPoints(points);
                // If the tile is not occupied, you can place a penguin
        if (!isOccupied)
        {
            // Assuming you have a reference to the PenguinPlacer script and penguinPrefab
            PenguinPlacer placer = FindObjectOfType<PenguinPlacer>();
            if (placer != null && placer.penguinPrefab != null)
            {
                Instantiate(placer.penguinPrefab, transform.position, Quaternion.identity);
                isOccupied = true; // Mark this tile as occupied
            }
            else
            {
                Debug.LogError("PenguinPlacer or penguinPrefab is not found or not assigned!");
            }
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

    public void PlacePenguin()
    {
        HasPenguin = true;
        // Additional logic to visually represent the penguin on the tile
    }
}