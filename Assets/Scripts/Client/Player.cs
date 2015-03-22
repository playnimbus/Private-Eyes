﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Player : Photon.MonoBehaviour
{
    public GameObject evidenceIndictor;

    public bool IsMurderer { get; private set; }
    public bool IsDetective { get; private set; }
    public bool IsDead { get; private set; }

    private new PlayerCamera camera;
    private PlayerUI ui;
    private Coroutine stashSearch;

    private bool haveEvidence;

    // Acts as a Start() for network instantiated objects
    void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        InitUI();
        InitCamera();
        SetHaveEvidence(false);

        // HACK ... Kinda. Assumes we only one text field on the player
        GetComponentInChildren<Text>().text = photonView.owner.name;
    }

    void InitUI()
    {
        if (!photonView.isMine) return;

        GameObject menuGO = Instantiate(Resources.Load<GameObject>("ClientMenu")) as GameObject;
        ui = menuGO.GetComponent<PlayerUI>();
        ui.SetHeaderText("No Evidence");
    }
    
    void InitCamera()
    {
        camera = GetComponentInChildren<PlayerCamera>();
        if (camera == null) Debug.LogError("[Player] Couldn't find PlayerCamera", this);

        if (photonView.isMine)
        {
            camera.Init(this.transform);
            camera.transform.parent = null;
        }
        else Destroy(camera.gameObject);
    }

    [RPC]
    public void MakeMurderer()
    {
        IsMurderer = true;
        if (photonView.isMine)
            ui.MarkAsMurderer();
    }

    [RPC]
    public void MakeDetective()
    {
        IsDetective = true;
        GetComponent<Renderer>().material.color = Color.blue;
        if (photonView.isMine)
            ui.MarkAsDetective(true);
    }

    [RPC]
    public void RemoveDetectiveship()
    {
        IsDetective = false;
        if (photonView.isMine)
            ui.MarkAsDetective(false);
    }

    public void ApproachedStash(EvidenceStash stash)
    {
        if (!photonView.isMine) return;

        if(!haveEvidence)
        {
            if (stashSearch != null) StopCoroutine(stashSearch);
            stashSearch = StartCoroutine(StashSearchCoroutine(stash));
            gameObject.layer = LayerMask.NameToLayer("HiddenPlayer");
            camera.GetComponent<Camera>().cullingMask = LayerMask.GetMask("HiddenPlayer") | 1;
        }
    }

    public void LeftStash(EvidenceStash stash)
    {
        if (!photonView.isMine) return;
        if (stashSearch != null) StopCoroutine(stashSearch);
        ui.HideAllButtons();
        gameObject.layer = LayerMask.NameToLayer("Player");
        camera.GetComponent<Camera>().cullingMask = LayerMask.GetMask("HiddenPlayer", "Player") | 1;
    }

    IEnumerator StashSearchCoroutine(EvidenceStash stash)
    {
        int timesTapped = 0;
        ui.ShowButton(0, "Tap to Search", false, () =>
            {
                timesTapped++;
            });
        
        while (timesTapped < 10)
        {
            yield return null;
        }

        // Tapped required amount of time
        stash.GetEvidence((hadEvidence) =>
            {
                photonView.RPC("SetHaveEvidence", PhotonTargets.All, hadEvidence);
            });
        
        stashSearch = null;
        ui.HideAllButtons();
    }
    
    void EnteredRoom(LevelRoom room)
    {
        if (!photonView.isMine) return;

        // camera.MoveToVantage(room.overheadCameraPosition);
        room.Reveal();
    }

    void ExitedRoom(LevelRoom room)
    {
        if (!photonView.isMine) return;

        // camera.ResumeFollow();
        room.Conceal();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!photonView.isMine) return;
        Player otherPlayer = collision.gameObject.GetComponent<Player>();
        if (otherPlayer == null) return;
        if (IsDead) return;
        
        // return when you do an action!
        if (IsMurderer)
        {
            if (MurdererInteraction(otherPlayer)) return;
        }
        else if (IsDetective)
        {
            if (DetectiveInteraction(otherPlayer)) return;
        }
        else // Is bystander
        {
            if (BystanderInteraction(otherPlayer)) return;
        }

        // All players can loot dead 
        if (LootingInteraction(otherPlayer)) return;
    }

    bool MurdererInteraction(Player otherPlayer)
    {
        if (haveEvidence)
        {
            if (otherPlayer.IsDetective)
            {
                ui.ShowButton(1, "Murder", true, () =>
                {
                    otherPlayer.photonView.RPC("Kill", PhotonTargets.All);
                    photonView.RPC("SetHaveEvidence", PhotonTargets.All, false);
                    ui.HideAllButtons();
                });
                ui.ShowButton(0, "Give Evidence", true, () =>
                {
                    otherPlayer.photonView.RPC("SetHaveEvidence", PhotonTargets.All, true);
                    photonView.RPC("SetHaveEvidence", PhotonTargets.All, false);
                    ui.HideAllButtons();
                });
            }
            else
            {
                ui.ShowButton(0, "Murder", true, () =>
                {
                    otherPlayer.photonView.RPC("Kill", PhotonTargets.All);
                    photonView.RPC("SetHaveEvidence", PhotonTargets.All, false);
                });
                return true;
            }
        }

        return false;
    }

    bool DetectiveInteraction(Player otherPlayer)
    {
        if (haveEvidence)
        {
            ui.ShowButton(0, "Accuse", true, () =>
            {
                otherPlayer.photonView.RPC("Accuse", PhotonTargets.All);
                photonView.RPC("SetHaveEvidence", PhotonTargets.All, false);
            });
            return true;
        }

        return false;
    }

    bool BystanderInteraction(Player otherPlayer)
    {
        if (otherPlayer.IsDetective && haveEvidence && !otherPlayer.haveEvidence && !otherPlayer.IsDead)
        {
            ui.ShowButton(0, "Give Evidence", true, () =>
            {
                otherPlayer.photonView.RPC("SetHaveEvidence", PhotonTargets.All, true);
                photonView.RPC("SetHaveEvidence", PhotonTargets.All, false);
            });
            return true;
        }
        if (otherPlayer.IsDetective && otherPlayer.IsDead)
        {
            ui.ShowButton(0, "Become Detective", true, () =>
                {
                    otherPlayer.photonView.RPC("RemoveDetectiveship", PhotonTargets.All);
                    photonView.RPC("MakeDetective", PhotonTargets.All);
                });
            return true;
        }

        return false;
    }

    bool LootingInteraction(Player otherPlayer)
    {
        if (otherPlayer.IsDead && otherPlayer.haveEvidence)
        {
            ui.ShowButton(0, "Take Evidence", true, () =>
            {
                otherPlayer.photonView.RPC("SetHaveEvidence", PhotonTargets.All, false);
                photonView.RPC("SetHaveEvidence", PhotonTargets.All, true);
            });
            return true;
        }

        return false;
    }

    void OnCollisionExit(Collision collision)
    {
        if (!photonView.isMine) return;
        Player other = collision.gameObject.GetComponent<Player>();
        if(other != null)
        {
            ui.HideAllButtons();
        }
    }

    [RPC]
    void Accuse()
    {
        if (IsMurderer)
        {
            GetComponent<Renderer>().material.color = new Color(1f, 0.5f, 0f);
            GetComponent<PlayerMovement>().StopMovement(0f);
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.yellow;
            GetComponent<PlayerMovement>().StopMovement(15f);
            Invoke("ResetColor", 15f);
        }
    }

    void ResetColor()
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
    
    [RPC]
    void Kill()
    {
        IsDead = true;
        GetComponent<Renderer>().material.color = Color.red;
        GetComponent<PlayerMovement>().StopMovement(0f);
    }

    [RPC]
    void SetHaveEvidence(bool value)
    {
        haveEvidence = value;
        evidenceIndictor.SetActive(value);
        if (photonView.isMine && ui != null)
        {
            string[] evidence = { "Hammer", "Knife", "Lead Pipe", "Revolver", "Rope", "Wrench" };
            ui.SetHeaderText(value ? "Evidence: " + evidence[UnityEngine.Random.Range(0, evidence.Length)] : "No Evidence");
        }
    }

}