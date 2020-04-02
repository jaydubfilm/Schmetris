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

    public List<GameObject> sequentialModuleList = new List<GameObject>();
    public List<GameObject> nonSequentialModuleList = new List<GameObject>();


    [ShowInInspector]
    int currentSequencedModule;

    [ShowInInspector]
    int currentNonSequencedModule;

    public Transform playerPos;
    float timerStartTime;
    float timerDuration;

    bool timer;

    int storedModule;
    bool storedonOff;
    bool storedPause;
    bool storedSequential;

    int countLastFrame;
    int asteroidHits;
    bool greyHasFallen;

    //public Game
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        
        if(timer)
        {

            float timerPosition = Time.time - timerStartTime;

            if (timerPosition > timerDuration)
            {
                TutorialPopup(storedModule, storedPause, true, storedSequential);
                timer = false;
            }           
        }


        //Check for new pieces
        
        if (GameController.Instance.blockList.Count > countLastFrame)
        {
            print("Block Falling");
            foreach (GameObject block in GameController.Instance.blockList)
            {
                if (greyHasFallen == false)
                {
                    if (block.GetComponentInChildren<Bit>().bitType == 7)
                    {
                        TutorialPopup(4, true, true, true);
                        print(block.gameObject.name);
                        greyHasFallen = true;
                    }
                }
            }
        }
        countLastFrame = GameController.Instance.blockList.Count;
    }

    //is sequential marks events that should happen chronologically
    public void TutorialPopup(int module, bool pauseGame, bool toggleOnOff, bool isSequential)
    {
        print("called");
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
            //if (isSequential && module < sequentialModuleList.Count)
            //    sequentialModuleList[module].SetActive(false);
            //else if (module < nonSequentialModuleList.Count)
            //{
            //    nonSequentialModuleList[module].SetActive(false);
            //    print("disabling");
            //}

            if (isSequential)
                sequentialModuleList[module].SetActive(false);
            else 
            {
                nonSequentialModuleList[module].SetActive(false);
                print("disabling");
            }


            return;
        }
        
        else
        {
            //enable the module
            if (isSequential)
                sequentialModuleList[module].SetActive(true);
            else
                nonSequentialModuleList[module].SetActive(true);


            //Only save our position in the list if this is a sequential event
            if (isSequential) currentSequencedModule = module;
            

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

        //TutorialPopup(currentSequencedModule, false, false, true);
        //TutorialPopup(currentNonSequencedModule, false, false, false);

        for (int i = 0; i < sequentialModuleList.Count; i++)
        {

            TutorialPopup(i, false, false, true);
        }


        for (int j = 0; j < nonSequentialModuleList.Count; j++)
        {

            TutorialPopup(j, false, false, false);
        }

        print("close " + sequentialModuleList[currentSequencedModule]);
    }

    public void OpenNextUnpaused()
       
    {

        TutorialPopup(currentSequencedModule + 1, false, true, true);
    }

    public void CloseAndOpenNextUnpaused()
    {

        CloseCurrent();
        OpenNextUnpaused();
    }

    public void CloseAndOpenWithDelaySequential(int module, bool pauseGame, float delay)
    {
        CloseCurrent();
        timerDuration = delay;
        timerStartTime = Time.time;
        storedModule = module;
        storedPause = pauseGame;
        storedSequential = true;

        timer = true;
    }

    public void CloseAndOpenWithDelayNonSequential(int module, bool pauseGame, float delay)
    {
        CloseCurrent();
        timerDuration = delay;
        timerStartTime = Time.time;
        storedModule = module;
        storedPause = pauseGame;
        storedSequential = false;

        timer = true;
    }

    public void NextWith2SecondDelay()
    {
        CloseAndOpenWithDelaySequential(currentSequencedModule + 1, false, 2);
    }


    public void OnAsteroidHit()
    {

        asteroidHits += 1;
        print(asteroidHits);

        switch (asteroidHits)
        {
            case 1:
                CloseCurrent();
                TutorialPopup(0, true, true, false);
                break;
            case 2:
                CloseCurrent();
                TutorialPopup(1, true, true, false);
                break;
        }
    }
}
