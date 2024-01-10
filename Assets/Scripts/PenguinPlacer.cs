using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinPlacer : MonoBehaviour
{
    public GameObject penguinPrefab; // Assign this in the Unity Editor

    void Update()
    {
        // Check for a mouse click
        if (Input.GetMouseButtonDown(0))
        {
            PlacePenguinAtMousePosition();
        }
    }

    private void PlacePenguinAtMousePosition()
    {
        // Raycast from the camera to the mouse position to find the tile
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

        // Check if the ray hit a collider
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Tile"))
        {
            // Optional: Verify the tile is valid for placement
            if (IsTileValidForPlacement(hit.collider.gameObject))
            {
                // Instantiate the penguin prefab at the center of the tile
                Instantiate(penguinPrefab, hit.collider.gameObject.transform.position, Quaternion.identity);
                // Add additional logic here for network synchronization if needed
            }
        }
    }
    private bool IsTileValidForPlacement(GameObject tile)
    {
        // Implement your logic to check if the tile is valid for placing a penguin
        // For example, check if the tile is empty and if it's the player's turn
        // This is a placeholder, you need to add your actual game logic here

        // Example placeholder logic: check if the tile doesn't already have a penguin
        return tile.transform.childCount == 0;
    }
}

