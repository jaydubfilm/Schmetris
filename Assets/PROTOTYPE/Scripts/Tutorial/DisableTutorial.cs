using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is culled from UI Buttons To disable the tutorial assets so they don't interfere with other levels
namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
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
}