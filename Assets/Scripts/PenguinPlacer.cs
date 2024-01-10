using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PenguinPlacer : MonoBehaviour
{
    public GameObject penguinPrefab; // Assign this in the Unity Editor
    private PhotonView photonView;
    public static PenguinPlacer Instance;

    void Update()
    {
        // Check for mouse input in the editor or standalone builds
        if (Input.GetMouseButtonDown(0))
        {
            SpawmPenguin(Input.mousePosition);
        }

        // Check for touch input on mobile devices
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                SpawmPenguin(touch.position);
            }
        }
    }
    private void SpawmPenguin(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Tile tile = hit.collider.GetComponent<Tile>();
            if (tile != null && GameManager.Instance.IsLocalPlayerTurn())
            {
                tile.PlacePenguin();
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

