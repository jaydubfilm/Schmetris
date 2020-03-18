﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

//Manages HUD and other in-game UI
public class GameUI : MonoBehaviour
{
    //Text displays
    public Text moneyText;
    public Text pauseText;
    public Text levelText;
    public Text timerText;
    public Text speedText;
    public Text progressText;

    //Lives
    public Transform livesGroup;
    public GameObject livesIcon;
    List<GameObject> livesUI = new List<GameObject>();

    //Resources
    float resourceBarWidth = 0;
    public RectTransform redBar;
    public RectTransform blueBar;
    public RectTransform greenBar;
    public RectTransform yellowBar;
    public RectTransform greyBar;

    //Burn rates

    //Popups
    public Text noFuelDisplay;
    public GameObject levelCompleteDisplay;
    public GameObject lifeLostDisplay;
    public GameObject returnButton;
    public GameObject gameOverDisplay;
    public GameObject continueButton;

    //Init
    private void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        pauseText.enabled = false;
#else
        pauseText.GetComponentInChildren<Image>().enabled = false;
#endif

        resourceBarWidth = redBar.sizeDelta.x;
    }

    //Update displayed money
    public void SetMoney(int money)
    {
        moneyText.text = "$" + GameController.Instance.money;
    }

    //Update displayed level
    public void SetLevel(int level)
    {
        levelText.text = "Level: " + level;
    }

    //Update displayed time
    public void SetTimer(float time)
    {
        timerText.text = "Time remaining: " + Mathf.Max(0, Mathf.Round(time));
        if(time <= 0 && !levelCompleteDisplay.activeSelf)
        {
            SetLevelCompletePopup(true);
        }
    }

    //Update displayed speed
    public void SetSpeed(float speed)
    {
        speedText.text = "x" + speed.ToString() + " Speed";
    }

    //Update displayed lives
    public void SetLives(int lives)
    {
        while (lives < livesUI.Count && livesUI.Count > 0)
        {
            Destroy(livesUI[0]);
            livesUI.RemoveAt(0);
        }
        while (lives > livesUI.Count)
        {
            livesUI.Add(Instantiate(livesIcon, livesGroup));
        }
    }

    //Toggle 'No Fuel' popup
    public void SetNoFuelPopup(bool isActive)
    {
        noFuelDisplay.color = isActive ? Color.white : Color.clear;
    }

    //Toggle 'Level Complete' popup
    public void SetLevelCompletePopup(bool isActive)
    {
        levelCompleteDisplay.SetActive(isActive);
    }

    //Toggle 'Life Lost' popup
    public void SetLifeLostPopup(bool isActive, string message = "")
    {
        returnButton.SetActive(false);
        lifeLostDisplay.GetComponent<Text>().text = message + " - Life Lost";
        lifeLostDisplay.SetActive(isActive);
        if (isActive)
            StartCoroutine(SetReturnButton());
    }

    //Activate return button after life lost popup
    IEnumerator SetReturnButton()
    {
        yield return new WaitForSeconds(2.0f);
        returnButton.SetActive(true);
    }

    //Toggle 'Game Over' popup
    public void SetGameOverPopup(bool isActive, string message = "")
    {
        continueButton.SetActive(false);
        gameOverDisplay.GetComponent<Text>().text = message + " - Game Over";
        gameOverDisplay.SetActive(isActive);
        if(isActive)
            StartCoroutine(SetContinueButton());
    }

    //Set progress text
    public void SetProgressText(string message)
    {
        progressText.text = message;
    }

    //Activate continue button after game over popup
    IEnumerator SetContinueButton()
    {
        yield return new WaitForSeconds(2.0f);
        continueButton.SetActive(true);
    }

    //Update display
    public void Update()
    {
        //Fade out no fuel popup
        noFuelDisplay.color = new Color(1, 1, 1, Mathf.Max(0, noFuelDisplay.color.a - Time.deltaTime));

        //Keyboard controls
        if (continueButton.activeSelf)
        {
            if (Input.anyKeyDown)
            {
                ReturnToMainMenu();
            }
        }
        else if (returnButton.activeSelf)
        {
            if (Input.anyKeyDown)
            {
                ReturnToScrapyard();
            }
        }
        else 
        {
            if (Input.GetKeyDown(KeyCode.Equals))
            {
                SpeedUp();
            }
            else if (Input.GetKeyDown(KeyCode.Minus))
            {
                SpeedDown();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                PauseGame();
            }
        }

        //Resources
        Vector2 barSize = redBar.sizeDelta;
        redBar.sizeDelta = new Vector2(resourceBarWidth * GameController.Instance.bot.GetResourcePercent(ResourceType.Red), barSize.y);
        blueBar.sizeDelta = new Vector2(resourceBarWidth * GameController.Instance.bot.GetResourcePercent(ResourceType.Blue), barSize.y);
        greenBar.sizeDelta = new Vector2(resourceBarWidth * GameController.Instance.bot.GetResourcePercent(ResourceType.Green), barSize.y);
        yellowBar.sizeDelta = new Vector2(resourceBarWidth * GameController.Instance.bot.GetResourcePercent(ResourceType.Yellow), barSize.y);
        greyBar.sizeDelta = new Vector2(resourceBarWidth * GameController.Instance.bot.GetResourcePercent(ResourceType.Grey), barSize.y);
    }

    //Increase speed
    public void SpeedUp()
    {
        GameController.Instance.SpeedUp();
    }

    //Decrease speed
    public void SpeedDown()
    {
        GameController.Instance.SpeedDown();
    }

    //Pause game
    public void PauseGame()
    {
        GameController.Instance.PauseGame();
    }

    //Return to scrapyard on life lost
    public void ReturnToScrapyard()
    {
        GameController.Instance.LoadScrapyard();
    }

    //Return to main menu on game over
    public void ReturnToMainMenu()
    {
        GameController.Instance.StartMenu();
    }
}
