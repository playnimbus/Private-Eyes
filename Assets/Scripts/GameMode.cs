﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameMode : Photon.MonoBehaviour {

    public GameObject bystanderPlayer;
    public GameObject murdererPlayer;
    public GameObject vigilantePlayer;
    public GameObject bystanderGuiCamera;

    public GameObject connectBtn;
    public GameObject hostBtn;
    public GameObject murdererWinsTxt;
    public GameObject murdererLosesTxt;

    public GameObject roomCovers;

    bool isHost = false;

    float murderAssignCountdown = 20;
    bool murdererAssigned = false;
    TextMesh countDownText;

    public Sprite murderSwordSprite;
    bool Murderfound = false;

	// Use this for initialization
	void Start () 
    {
        PlayerInitialization();

        if (PhotonNetwork.isMasterClient)
            AssignMurderer();
	}

    void AssignMurderer()
    {
        PhotonPlayer player = PhotonNetwork.playerList[Random.Range(0, PhotonNetwork.countOfPlayers)];
        GetComponent<PhotonView>().RPC("MakeMurderer", player, null);

        murdererAssigned = true;
        countDownText.gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Murderfound == false)
        {
            GameObject[] Players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < Players.Length; i++)
            {
                if (Players[i].GetComponent<playerController>().cluesObtained >= 3)
                {
                    Players[i].GetComponent<playerController>().meleeSword.GetComponent<SpriteRenderer>().sprite = murderSwordSprite;
                    Murderfound = true;
                }

            }
        }
	}

    void OnGUI()
    {
        if (!PhotonNetwork.connected)
        {
            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        }
        GUI.Label(new Rect(10, 10, 230, 70), "Players Connected: " + PhotonNetwork.playerList.Length);
        GUI.Label(new Rect(10, 30, 230, 70), "Ping: " + PhotonNetwork.GetPing());
        GUI.Label(new Rect(10, 50, 230, 70), "isMasterClient: " + PhotonNetwork.isMasterClient);
    }

    void PlayerInitialization()
    {
        Debug.Log("Connected");

        if (isHost == false)
        {
            //finds a random spawn position for the player
            GameObject[] playerSpawns = GameObject.FindGameObjectsWithTag("PlayerSpawn");
            Vector3 spawnPostition = playerSpawns[Random.Range(0,playerSpawns.Length)].transform.position;

            GameObject[] playerList = GameObject.FindGameObjectsWithTag("Player");
            GameObject tempPlayer = PhotonNetwork.Instantiate("bystanderPlayer", spawnPostition, Quaternion.identity, 0);
            tempPlayer.name = "Player" + playerList.Length;

            tempPlayer.GetComponent<playerController>().playerThumbpad = bystanderGuiCamera.GetComponent<bystanderGuiCamera>().playerThumbad;
            tempPlayer.GetComponent<playerController>().swordNotify = bystanderGuiCamera.GetComponent<bystanderGuiCamera>().swordNotify;
        }
        else
        {
            GameObject hostCamera = (GameObject)Instantiate(Resources.Load("HostCamera"));
            hostCamera.transform.position = new Vector3(2.5f, 32.87f, 57.2f);

            GameObject[] clueSpawns = GameObject.FindGameObjectsWithTag("WeaponSpawn");
            GameObject clue = PhotonNetwork.Instantiate("Clue", clueSpawns[Random.Range(0, clueSpawns.Length)].transform.position, Quaternion.identity, 0);
            clue.transform.Rotate(new Vector3(90, 0, 0));

            countDownText = GameObject.Find("CountDownTimer").GetComponent<TextMesh>();
            countDownText.text = murderAssignCountdown.ToString();

            roomCovers.SetActive(false);
        }

        connectBtn.SetActive(false);
        hostBtn.SetActive(false);
    }
}
