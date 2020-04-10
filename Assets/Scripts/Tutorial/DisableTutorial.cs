using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableTutorial : MonoBehaviour
{


    public bool disableTutorial;
    public TutorialManager tutorialManager;
    public GameObject tutorialPanel;

    
    public void RemoveTutorial()
    {
        if (disableTutorial == true)
        {
            if (tutorialManager != null)
                tutorialManager.enabled = false;
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
            //Destroy(tutorialManager);
        }
        else
        {
            GameController.Instance.tutorialHasStarted = false;
            if (tutorialManager != null)
                tutorialManager.enabled = true;
            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);
        }
    }
}
