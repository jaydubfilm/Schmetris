using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public bool tutorialHasFinished;
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

    public int currentSection;
    int frameCounter;
    public Sprite greyScale;
    public Sprite level1GreyScale;
    public Sprite level2GreyScale;
    public Sprite red;


    bool collected1Greyscale;
    bool level1Upgraded;
    public List<GameObject> greyScaleAtSectionStart = new List<GameObject>();
    public List<GameObject> redSprites = new List<GameObject>();

    public GameObject gameTimer;

    bool beganGreyscaleSection;
    bool hasHadFuelWarning;
    Bot playerBot;
    bool outOfFuel;
    bool redDropTimer;
    float timeToRedDrop = 0;
    float timerStartRedDrop;
    bool hasSpawnedRed;
    bool collectRed;
    int redCounter;
    int frameChecks;
    public bool isBotDead;
    public GameObject tutorialPanel;
    public GameObject levelComplete;

    //public PowerGridPrefab tutorialGrid;

    //public Game
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
        playerBot = playerPos.GetComponent<Bot>();
        gameTimer.SetActive(false);
    }

    void OnEnable()
    {
        gameTimer.SetActive(false);
        levelComplete.SetActive(false);
    }

    void OnDisable()
    {
        gameTimer.SetActive(true);
        levelComplete.SetActive(true);
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    private void Update()
    {

        if (timer)
        {

            float timerPosition = Time.time - timerStartTime;

            if (timerPosition > timerDuration)
            {
                TutorialPopup(storedModule, storedPause, true, storedSequential);
                timer = false;
            }
        }


       
        countLastFrame = GameController.Instance.blockList.Count;

        //Section Specific Behaviours
        if (currentSection == 2 || currentSection == 3)
        {



            if (frameCounter == 60)
            {
                List<SpriteRenderer> childSprites = new List<SpriteRenderer>(playerPos.GetComponentsInChildren<SpriteRenderer>());
                foreach (SpriteRenderer SR in childSprites)
                {
                    //collect your first greyscale
                    if (collected1Greyscale == false)
                    {
                        //                        print(SR.sprite.name);
                        if (SR.sprite == greyScale)
                        {
                            if (beganGreyscaleSection == false)
                            {
                                greyScaleAtSectionStart.Add(SR.gameObject);
                            }
                            else
                            {
                                if (!greyScaleAtSectionStart.Contains(SR.gameObject))
                                {
                                    TutorialPopup(4, false, true, false);
                                    collected1Greyscale = true;
                                    return;
                                }
                            }
                        }

                    }
                    //level up greyscale
                    if (collected1Greyscale == true && level1Upgraded == false)
                    {
                        if (SR.sprite == level1GreyScale)
                        {
                            CloseCurrent();
                            TutorialPopup(5, false, true, false);
                            level1Upgraded = true;
                            return;
                        }
                    }
                    //level up greyscale 2
                    if (collected1Greyscale == true && level1Upgraded == true)
                    {
                        if (SR.sprite == level2GreyScale)
                        {
                            GameController.Instance.LoadNextLevelSection();
                            CloseCurrent();
                            TutorialPopup(6, false, true, false);
                            level1Upgraded = true;
                            return;
                        }
                    }
                }
                beganGreyscaleSection = true;
                frameCounter = 0;
            }

            frameCounter++;
        }
        //fuel warning
        if (beganGreyscaleSection == true)
        {
            if (hasHadFuelWarning == false)
            {
                if (playerBot.storedRed < 21)
                {

                    hasHadFuelWarning = true;
                    //low fuel warning message
                    TutorialPopup(5, false, true, true);
                    GameController.Instance.LoadNextLevelSection();

                }
            }

            if (playerBot.storedRed < 14 && outOfFuel == false)
            {
                print("out of fuel");
                //NextWith2SecondDelay();
                outOfFuel = true;
                timerStartRedDrop = Time.time;
                redDropTimer = true;
            }

            if (redDropTimer == true)
            {

                if (Time.time - timerStartRedDrop > timeToRedDrop)
                {
                    //SpawnSingle();
                    redDropTimer = false;
                    frameCounter = 0;
                    hasSpawnedRed = true;
                }
            }


            if (hasSpawnedRed)
            {
                           // CloseAndOpenNextUnpaused();
                if (frameCounter == 60)
                {
                    frameChecks++;
                    List<SpriteRenderer> childSprites = new List<SpriteRenderer>(playerPos.GetComponentsInChildren<SpriteRenderer>());
                    foreach (SpriteRenderer SR in childSprites)
                    {
                        if (SR.sprite == red || frameChecks >= 45)
                        {
                            //SingleRed
                            //GameController.Instance.LoadNextLevelSection();
                            frameCounter = 0;
                            collectRed = true;
                            hasSpawnedRed = false;
                        }
                    }
                }

                frameCounter++;
            }

            if (collectRed)
            {
                if (frameCounter == 60)
                {
                    List<SpriteRenderer> childSprites = new List<SpriteRenderer>(playerPos.GetComponentsInChildren<SpriteRenderer>());
                    foreach (SpriteRenderer SR in childSprites)
                    {
                        if (SR.sprite == red)
                        {
                            if (!redSprites.Contains(SR.gameObject))
                            {
                                //print("got 1");
                                redSprites.Add(SR.gameObject);
                                redCounter++;
                                if (redCounter == 3)
                                {
                                    //print("got 3");
                                    //back to yard

                                    //CloseAndOpenNextUnpaused();
                                }
                            }
                        }
                    }
                    frameCounter = 0;
                }
                frameCounter++;
            }
        }
        ////player death
        //if (GameController.Instance.isBotDead == true && isBotDead == false)
        //{

        //    ResetVariables();
        //}
        //isBotDead = GameController.Instance.isBotDead;
    }

    void ResetVariables()
    {

        collected1Greyscale = false;
        level1Upgraded = false;
        //levelComplete = false;
        beganGreyscaleSection = false;
        hasHadFuelWarning = false;
        outOfFuel = false;
        redDropTimer = false;        
        timerStartRedDrop = 0;
        hasSpawnedRed= false;
        collectRed = false;
        redCounter =0;
        frameChecks =0;
        isBotDead = false;

}

    //is sequential marks events that should happen chronologically
    public void TutorialPopup(int module, bool pauseGame, bool toggleOnOff, bool isSequential)
    {
        //Bump the asteroid popups if there's already a message up
        if(isSequential == false && toggleOnOff == true && (module == 0 || module == 1))
        {
            //print("this is an asteroid " + module + " " + isSequential);
            foreach (GameObject item in sequentialModuleList)
            {
                if (item.GetComponent<Image>() && item.activeSelf == true)
                {
                    if (item.GetComponent<Image>().color.a >= 0)
                    {
                        //print("Asteroid Message held for " + item.name);
                        asteroidHits--;
                        return;
                    }
                }
            }

            foreach (GameObject item in nonSequentialModuleList)
            {
                if (item.GetComponent<Image>() && item.activeSelf == true)
                {
                    if (item.GetComponent<Image>().color.a >= 0)
                    {
                        asteroidHits--;
                        //print("Asteroid Message held for " + item.name);
                        return;
                    }
                }
            }
        }

        
        
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
//                print("disabling");
            }


            return;
        }
        
        else
        {
            //enable the module
            if (isSequential)
                if (module < sequentialModuleList.Count)
                {
                    sequentialModuleList[module].SetActive(true);
                }

            if (!isSequential)
                if (module < nonSequentialModuleList.Count)
                {
                    nonSequentialModuleList[module].SetActive(true);
                }

            //Only save our position in the list if this is a sequential event
            if (isSequential) currentSequencedModule = module;
            

        }

      

    }

    [Button]
    public void SpawnSingle()
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

        //print("close " + sequentialModuleList[currentSequencedModule]);
    }

    public void OpenNextUnpaused()
       
    {
        CloseCurrent();
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
       

        switch (asteroidHits)
        {
            case 1:
                //CloseCurrent();
                TutorialPopup(0, false, true, false);
                break;
            case 2:
                //CloseCurrent();
                TutorialPopup(1, false, true, false);
                break;
            default:
                break;
        }
    }


    //[Button]
    public void SetFuel(int fuel)
    {

        playerPos.GetComponent<Bot>().SetFuelAmt(fuel);
    }

    public void OnLevelChange(int newSection)
    {
        currentSection = newSection;
//        print("loaded scene " + newSection);
        switch (newSection)
        {
            case 2: //greyscale
                CloseCurrent();
                TutorialPopup(4, false, true, true);
                break;

            default:
                break;
        }
    }

    public void ToScrapYard()
    {
        //GameController.Instance.LoadNextLevelSection();
        GameController.Instance.game = GameController.Instance.easyNonTutorial;
        GameController.Instance.LoadNextLevelSection();
        tutorialHasFinished = true;
        CloseCurrent();
        SetFuel(40);
        tutorialPanel.SetActive(false);
        this.enabled = false;
        //Destroy(gameObject);
    }

    [Button]
    public void Respawn()
    {
        if (this.enabled)
        {
            CloseCurrent();
            ResetVariables();
            GameController.Instance.RestartOnDestroy();
            SetFuel(1000);
            foreach (GameObject item in sequentialModuleList)
            {
                if (item.GetComponent<InputCheck>())
                {
                    item.GetComponent<InputCheck>().Reset();
                }
            }

            foreach (GameObject item in nonSequentialModuleList)
            {
                if (item.GetComponent<InputCheck>())
                {
                    item.GetComponent<InputCheck>().Reset();
                }
            }
            TutorialManager.Instance.CloseAndOpenWithDelaySequential(0, false, 2.2f);
            GameController.Instance.lives = 3;
        }
    }

    public void CloseTutorial()
    {
        if (this.enabled)
        {
            CloseCurrent();
            foreach (GameObject item in sequentialModuleList)
            {
                if (item.GetComponent<InputCheck>())
                {
                    item.GetComponent<InputCheck>().Reset();
                }
            }

            foreach (GameObject item in nonSequentialModuleList)
            {
                if (item.GetComponent<InputCheck>())
                {
                    item.GetComponent<InputCheck>().Reset();
                }
            }
            ResetVariables();
            tutorialPanel.SetActive(false);
            GameController.Instance.tutorialHasStarted = false;
            this.enabled = false;
        }

    }

    public void WhiteEnergy()
    {
        TutorialPopup(7, false, true, false);

    }

    [Button]
    void checkSection()
    {
        print(currentSection);
    }
}
