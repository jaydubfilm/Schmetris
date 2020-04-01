using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{

    public static TutorialManager Instance { get; private set; }
    public TMP_FontAsset font;

    public List<GameObject> moduleList = new List<GameObject>();

    [ShowInInspector]
    int currentModule;

    [ShowInInspector]
    int currentQueue;

    public Transform playerPos;

    //public Game
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
    
    


    //Set Queue to 0 if you dont want to queue anything
    public void TutorialPopup(int module, bool pauseGame, bool toggleOnOff, int queue)
    {
        currentQueue = queue;
        currentModule = module;

        //pause
        if (pauseGame == true)
        {
            GameController.Instance.hud.gameObject.SetActive(false);
            GameController.Instance.isPaused = true;
            Time.timeScale = 0;
        }
        else
        {
            GameController.Instance.hud.gameObject.SetActive(true);
            GameController.Instance.isPaused = false;
            Time.timeScale = 1;
        }

        //disable
        if (toggleOnOff == false)
        {

            //check if we're showing the queued obj for this module (ie the module has already been disabled)
            if (moduleList[queue].activeSelf == true)
            {
                moduleList[queue].SetActive(false);
                return;
            }
            else
            {
                moduleList[module].SetActive(false);
                //return;
            }

            //disable 
            if (queue > 0)
            {

                moduleList[module].SetActive(false);
                moduleList[queue].SetActive(true);
                return;
            }
        }
        else
        {

            //enable the module
            moduleList[module].SetActive(true);

        }

        if (moduleList[module].GetComponentInChildren<TextMeshProUGUI>())
        {

            moduleList[module].GetComponentInChildren<TextMeshProUGUI>().font = font;
        }
    }

    [Button]
    public void TestSpawnSingle()
    {
        GameController.Instance.SpawnBlock(ScreenStuff.XPositionToCol(playerPos.position.x), 0);
        //TutorialPopup(0, false, false, 1);
    }

    public void CloseCurrent()
    {

        TutorialPopup(currentModule, false, false, currentQueue);

    }
}
