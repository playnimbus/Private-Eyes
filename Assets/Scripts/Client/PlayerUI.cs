﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

// Displays what a player has gathered.
public class PlayerUI : MonoBehaviour 
{
    [Serializable]
    public struct PlayerButton
    {
        public Button button;
        public Text text;
        public Action callback;
        public Image Icon;
        public bool hideOnPressed;
    }

    public Image murdererIndicator;
    public Image detectiveIndicator;
    public Text headerText;
    public PlayerButton[] buttons;
    public PlayerButton powerupUpButton;
    public GameObject tutorialMurderer;
    public GameObject tutorialBystander;
    public GameObject tutorialDetective;

    // Tap interaction vars
    private Coroutine tapCoroutine;
    public GameObject tapInfoPanel;
    public Text tapInfoText;

    #region Initialization

    void Start()
    {
        tapInfoPanel.SetActive(false);
        murdererIndicator.enabled = false;
        detectiveIndicator.enabled = false;
        foreach (PlayerButton button in buttons)
            button.button.gameObject.SetActive(false);
    }

    public void MarkAsMurderer()
    {
        // HACK
        Invoke("M", 0.5f);
    }

    void M() { 
        murdererIndicator.enabled = true;
        detectiveIndicator.enabled = false;
        tutorialMurderer.SetActive(true);
        tutorialBystander.SetActive(false);
    }

    public void MarkAsDetective(bool val)
    {
        // HACK
        if(val) Invoke("D", 0.5f);
        else detectiveIndicator.enabled = false;
    }

    void D() 
    { 
        detectiveIndicator.enabled = true;
        murdererIndicator.enabled = false;
        tutorialDetective.SetActive(true);
        tutorialBystander.SetActive(false);
    }

    #endregion

    #region Buttons

    public void ShowButton(int num, string text, bool hideOnPressed, Action callback)
    {
        buttons[num].button.gameObject.SetActive(true);
        buttons[num].text.text = text;
        buttons[num].callback = callback;
        buttons[num].hideOnPressed = hideOnPressed;
    }

    public void InitPowerupButton(Action callback)
    {
        powerupUpButton.callback = callback;
    }
        
    public void HideAllButtons()
    {
        for (int num = 0; num < buttons.Length; num++)
        {
            buttons[num].button.gameObject.SetActive(false);
            buttons[num].callback = null;
        }
    }
    
    public void ButtonPressed(int num)
    {
        if (buttons[num].callback != null) buttons[num].callback();
        if (buttons[num].hideOnPressed) buttons[num].button.gameObject.SetActive(false);
    }
    public void PowerupButtonPressed()
    {
        powerupUpButton.callback();
    }

    public void SetHeaderText(string s)
    {
        if (headerText != null)
            headerText.text = s;
        else
            Debug.LogWarning("[PlayerUI] headerText not set.", this);
    }

    #endregion

    #region Icons

    public void SetPowerupIcon(Sprite powerupIcon)
    {
        powerupUpButton.Icon.sprite = powerupIcon;
    }

    public void FadeInMurderIcon(float seconds)
    {
        StartCoroutine(MurderIconFadeCoroutine(seconds));
    }

    IEnumerator MurderIconFadeCoroutine(float seconds)
    {
        // Fade from invisible to full
        float elapsedTime = 0f;
        while (elapsedTime < seconds)
        {
            Color c = murdererIndicator.color;
            c.a = elapsedTime / seconds;
            murdererIndicator.color = c;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Scale it up to "pop" at the end
        elapsedTime = 0f;
        Vector3 initialScale = murdererIndicator.rectTransform.localScale;
        while (elapsedTime < 0.25f)
        {
            float progress = elapsedTime / 0.25f;  // between 0-1
            float angle = progress * Mathf.PI;  // map that to 0-pi
            float scalePercent = Mathf.Sin(angle); // take sin for smooth curve
            Vector3 scale = initialScale * (1 + scalePercent * 0.25f); // scale up to 1.25x original size
            murdererIndicator.rectTransform.localScale = scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion

    #region New Tap Interaction

    public void SetTapAction(string message, Action callback)
    {
        if (callback == null) Debug.LogError("[PlayerUI.RegisterTapAction] Passed in null callback", this);

        tapInfoPanel.SetActive(true);
        tapInfoText.text = message;
        tapCoroutine = StartCoroutine(TapListenerCoroutine(callback));
    }

    public void RemoveTapAction()
    {
        tapInfoPanel.SetActive(false);

        if (tapCoroutine != null) 
            StopCoroutine(tapCoroutine);
    }

    // Listen for double tap
    private IEnumerator TapListenerCoroutine(Action callback)
    {
        while (true)
        {
            // Listen for tap one
            yield return StartCoroutine(WaitForTapLength(0.25f));
            
            // Note completion time
            float firstTapTime = Time.time;

            // Listen for tap two
            yield return StartCoroutine(WaitForTapLength(0.25f));

            // See if time between tap finishes was quick enough
            if (Time.time < firstTapTime + 0.5f)
            {
                // If so, callback and return
                callback();
                yield break;
            }
        }
    }

    // Waits for a tap that is at most length seconds long
    private IEnumerator WaitForTapLength(float length)
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                float downTime = Time.time;

                while (Input.GetMouseButton(0))
                    yield return null;

                if (Time.time < downTime + length)
                    yield break;
            }
            
            yield return null;
        }
    }
    
    #endregion
}
