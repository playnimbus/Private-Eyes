﻿using UnityEngine;
using System.Collections;

public class playerController : Photon.MonoBehaviour
{
    public Camera playerCamera;

    public GameObject playerThumbpad;
    public GameObject meleeSword;
    public GameObject swordSpawn;
    public GameObject swordNotify;
    public TextMesh name;

    public float cluesObtained;
    public float playerSpeed;

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;

    private Vector3 thumbOrigin;
    private Vector3 syncStartPosition = Vector3.zero;
    private Vector3 syncEndPosition = Vector3.zero;

    private bool hasWeapon;


    string[] names = { "Mike", "Nick", "Jacob", "John", "Joe", "Ian", "Marc","Swei","Flood","Mudry","Neo","Adam","Pup0","Yuka","Sarah","Brit","Casey","Jess","Nora","Jamie","Renzo","Keith","Kyle","Ryan","Kate","Ali","Rich", "Dave","Nate","Monty","Mina","Sam" };

    void Start()
    {
        thumbOrigin = playerThumbpad.transform.localPosition;
        name.text = names[Random.Range(0, names.Length)];
    }

    // Update is called once per frame
    void Update()
    { 

        if (cluesObtained >= 3 && gameObject.name == "Player0") 
        { 
            hasWeapon = true;
            swordNotify.SetActive(true);
        } 
 
        if (photonView.isMine) 
        { 
            MovementAnalog();  
        }
    } 
 

    void FixedUpdate() 
    { 
        if (!photonView.isMine) 
        { 
            SyncedMovement(); 
        } 
    } 
 

    public void SwingSword() 
    {

        if (meleeSword.transform.position.y <= 25)
        {
            meleeSword.transform.position = new Vector3(0, 50, 0); 
        }
        else if (meleeSword.transform.position.y > 25)
        {
            meleeSword.transform.position = swordSpawn.transform.position;
        }

    } 


    public void Attacked()
    {
        
    }

    public void MovementAnalog()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit))
        {
            if (Input.GetMouseButton(0))
            {
                if (hit.transform.gameObject.name == "AnalogBG")
                {
                    Vector3 normalizedCastPosition = hit.point - hit.transform.position;
                    Vector3 forceToAdd = new Vector3(((hit.point.x - hit.transform.position.x) * playerSpeed), 0, ((hit.point.z - hit.transform.position.z) * playerSpeed));
                    gameObject.rigidbody.velocity = forceToAdd;
                    transform.LookAt(gameObject.transform.position + forceToAdd);

                    playerThumbpad.transform.position = hit.point;
                }
            }
            else
            {
                playerThumbpad.transform.localPosition = thumbOrigin;
                gameObject.rigidbody.velocity = Vector2.zero;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (hasWeapon)
                {
                    if (hit.transform.gameObject.name == "AttackButton")
                    {
                        print("Swing Sword");
                        SwingSword();
                    }
                }
            }



        }
        else
        {
            
            gameObject.rigidbody.velocity = Vector2.zero;
        }
    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            stream.SendNext(rigidbody.position);
        }
        else
        {
         //   print("stream is not writing");
            syncEndPosition = (Vector3)stream.ReceiveNext();
            syncStartPosition = rigidbody.position;

            syncTime = 0f;
            syncDelay = Time.time - lastSynchronizationTime;
            lastSynchronizationTime = Time.time;
        }
    }

    private void SyncedMovement()
    {
        syncTime += Time.deltaTime;
        transform.position = Vector3.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Clue")
        {
            print("found 1 clue!");
            GameObject.Destroy(collision.gameObject);
            cluesObtained += 1;


            GameObject[] clues = GameObject.FindGameObjectsWithTag("Clue");

            if (clues.Length <= 1)
            {
                GameObject[] clueSpawns = GameObject.FindGameObjectsWithTag("WeaponSpawn");
                GameObject clue = PhotonNetwork.Instantiate("Clue", clueSpawns[Random.Range(0, clueSpawns.Length)].transform.position, Quaternion.identity, 0);
                clue.transform.Rotate(new Vector3(90, 0, 0));
            }
            
        }
        else if (collision.collider.tag == "Sword")
        {
            Debug.Log(gameObject.name + "Such a sad day to stop living");
            
      //      PhotonNetwork.Disconnect();
     //       PhotonNetwork.NetworkStatisticsReset();
      //      PhotonNetwork.Destroy(gameObject);
      //      PhotonNetwork.LeaveRoom();

     //       if (PhotonNetwork.isMasterClient)
     //       {
                PhotonNetwork.Destroy(gameObject);
     //       }
     //       GameObject.Destroy(gameObject);
        }
    }


}