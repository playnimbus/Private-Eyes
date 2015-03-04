﻿using UnityEngine;
using System.Collections.Generic;
using System;



// Master client specific game state management logic
public class MasterGame : Game
{
    private MasterLobby Lobby { get { return base.lobby as MasterLobby; } }
    private List<PhotonPlayer> playersInSession;

    protected void Start()
    {
        base.Start();
        base.lobby = gameObject.AddComponent<MasterLobby>();

        network.onConnected += network.CreateRoom;
        network.onCreateRoomFailed += network.CreateRoom;
        network.onCreatedRoom += InitiateControl;
        network.onCreatedRoom += lobby.Enter;
        network.onInitiateMasterControl += InitiateMasterControl;
        network.onPlayerConnected += PlayerJoined;

        Lobby.onLaunchSession += RequestLaunchSession;
    }

    void InitiateControl()
    {
        // Take control of the game
        network.RaiseCustomEvent(CustomEvent.InitiateMasterControl, PhotonNetwork.player.ID);
    }
    
    protected override Session CreateSession(SessionType type)
    {
        Session session = null;

        switch (type)
        {
            case SessionType.Default:
            default:
                session = gameObject.AddComponent<DefaultMasterSession>();
                break;
        }

        session.onFinished += RequestSessionFinish;
        return session;
    }

    void PlayerJoined(PhotonPlayer player)
    {
            
    }

    void RequestLaunchSession(SessionType type)
    {
        photonView.RPCEx("LaunchSession", PhotonTargets.All, type);
    }

    void RequestSessionFinish()
    {
        this.session.onFinished -= RequestSessionFinish;
        photonView.RPCEx("FinishSession", PhotonTargets.All);
    }

}
