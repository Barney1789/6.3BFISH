using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerController : MonoBehaviour, IPunInstantiateMagicCallback, IPunObservable
{

    public GameObject penguinPrefab; // Assign this in the inspector
    private Vector3 playerScale;

    public FixedJoystick fixedJoystick;
    private PhotonView photonView;
    private Rigidbody2D rigidbody2D;
    private float horizontal, vertical;
    private Vector3 playerPos;

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] instantiationData = info.photonView.InstantiationData;
        string colour = (string) instantiationData[0];
        float boxRandomSize = (float)instantiationData[1];
        print("Received Colour:" + colour);
        print("Received Size:" + boxRandomSize);


        //change colour
        if(colour == "Red")       
            GetComponent<SpriteRenderer>().color = Color.red;        
        else if(colour == "Blue")       
            GetComponent<SpriteRenderer>().color = Color.blue;        
        else if(colour == "Green")        
            GetComponent<SpriteRenderer>().color = Color.green;

        //change the box size
        playerScale = new Vector3(boxRandomSize, boxRandomSize, 1);

        //change the player's name
        GetComponentInChildren<TextMeshProUGUI>().text = info.photonView.Owner.NickName;
    }

    // Start is called before the first frame update
    void Start()
    {
        photonView = PhotonView.Get(this);

        if (photonView.IsMine)
        {
            //this player owns this photonview, therefore give him access to the joystick
            fixedJoystick = GameObject.Find("Fixed Joystick").GetComponent<FixedJoystick>();
            rigidbody2D = GetComponent<Rigidbody2D>();
        }
        else
        {
            //if player object is not mine, then it should automatically be controlled by photon
            //data, therefore we are going to destroy the rigidbody
            Destroy(GetComponent<Rigidbody2D>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button was clicked
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Tile tile = hit.collider.GetComponent<Tile>();
                if (tile != null && !tile.HasPenguin) // Check if the tile has no penguin
                {
                    PlacePenguin(tile.transform.position);
                }
            }
        }
        //change the scale of the prefab square
        transform.localScale = new Vector3(playerScale.x, playerScale.y, playerScale.z);

        if (photonView.IsMine)
        {
            //we on this instance (box) so get the data from the joystick
            horizontal = fixedJoystick.Horizontal;
            vertical = fixedJoystick.Vertical;
        }
        else
        {
            //we don't own this object, therefore we have to move it by data from the server
            //the data that we receiving from the OnPhotonSerializeView()
            //transform.position = playerPos;

            //we are going to make use of lerp function to fill (predict) missing data
            //this wil create a smooth animation between two sets of data
            transform.position = Vector3.Lerp(transform.position, playerPos, Time.deltaTime * 10);
        }
    }

    private void PlacePenguin(Vector3 position)
    {
        Instantiate(penguinPrefab, position, Quaternion.identity);
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            float runSpeed = 5.0f;
            rigidbody2D.velocity = new Vector2(horizontal * runSpeed, vertical * runSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //stream.iswriting is executed if we own this object. send data to other client
            stream.SendNext(transform.position);
            //print("Sending Position Data:" + transform.position);
        }
        else
        {
            //receive data from server
            playerPos = (Vector3) stream.ReceiveNext();
            //print("Receiving Position Data:" + playerPos);
        }
    }

    public void ChangeSizeFromMaster(List<PlayerInfo> playersInfo)
    {
        foreach(PlayerInfo playerInfo in playersInfo)
        {
            if(photonView.Owner.ActorNumber == playerInfo.actorNumber)
            {
                this.playerScale = playerInfo.size;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (photonView.IsMine)
            {
                //get scale of object we are colliding with
                float scaleOther = collision.transform.localScale.x;

                //get scale of mine (of this player)
                float scaleMine = transform.localScale.x;

                //get id of the smallest player
                int destroyPlayerId;
                if(scaleMine > scaleOther)
                {
                    destroyPlayerId = collision.gameObject.GetComponent<PlayerController>()
                        .photonView.Owner.ActorNumber;

                }
                else
                {
                    destroyPlayerId = photonView.Owner.ActorNumber;
                }

                //inform everyone to destroy (eat) smallest player
                //this means we have to make use of RPC
                GameObject.Find("Scripts").GetComponent<NetworkManager>()
                    .DestroyPlayer(destroyPlayerId);

            }
        }
    }
}
