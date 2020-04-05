using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableTutorial : MonoBehaviour
{



    public GameObject tutorialManager;
    public GameObject tutorialPanel;

    
    // Update is called once per frame
    public void RemoveTutorial()
    {
       if(tutorialManager != null)
            Destroy(tutorialManager);
       if(tutorialPanel != null)
            Destroy(tutorialPanel);

    }
}
