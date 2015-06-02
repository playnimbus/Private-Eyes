﻿using UnityEngine;
using System;

// Hooks and utility for a room
public class LevelRoom : MonoBehaviour
{
    // Vantage point for the camera to go to when player enters
    public Transform overheadCameraPosition;

    // Fades the cover for a room
    private Fade roomCoverFade;
    private Door[] doors;

    void Start()
    {
        roomCoverFade = GetComponentInChildren<Fade>();

        TriggerListener listener = GetComponentInChildren<TriggerListener>();
        listener.onTriggerEntered += OnRoomEnter;
        listener.onTriggerExited += OnRoomExit;
    }

    void OnRoomEnter(Collider coll)
    {
        if (coll.gameObject.CompareTag(Tags.Player)) coll.SendMessage("EnteredRoom", this, SendMessageOptions.DontRequireReceiver);
    }

    void OnRoomExit(Collider coll)
    {
        if (coll.gameObject.CompareTag(Tags.Player)) coll.SendMessage("ExitedRoom", this, SendMessageOptions.DontRequireReceiver);
    }

    public void Reveal() { 
        roomCoverFade.FadeOut();

        doors = GetComponentsInChildren<Door>();
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].openDoor();
        }
    }
    public void Conceal() { roomCoverFade.FadeIn(); }

}