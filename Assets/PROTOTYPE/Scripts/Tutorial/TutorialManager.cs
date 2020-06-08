using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Sirenix.OdinInspector;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//The Tutorial Manager Manages the tutorial level in these ways:
// - triggers progression through the level sections
// - enables and disbles information panels in the tutorial panel in the UI hierarchy
    // - There are 2 lists of information panels:
        // - SequentialModuleList stores UI Popups scheduled to appear in order
        // - nonSequentialModuleList stores UI popups that can be triggered dynamically, such as hitting an asteroid
namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
    public class TutorialManager : MonoBehaviour
    {
        public bool tutorialHasFinished;
        public static TutorialManager Instance { get; private set; }
        public TMP_FontAsset font;

        public List<GameObject> sequentialModuleList = new List<GameObject>();
        public List<GameObject> nonSequentialModuleList = new List<GameObject>();


        [ShowInInspector] int currentSequencedModule;

        [ShowInInspector] int currentNonSequencedModule;

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
        GameObject fuelFreezeObj;

        //public PowerGridPrefab tutorialGrid;

        //public Game
        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
            playerBot = playerPos.GetComponent<Bot>();
            gameTimer.SetActive(false);
            fuelFreezeObj = GetComponentInChildren<FreezeFuel>(true).gameObject;
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
            if (GameController.Instance.isPaused)
                return;

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
                    List<SpriteRenderer> childSprites =
                        new List<SpriteRenderer>(playerPos.GetComponentsInChildren<SpriteRenderer>());
                    foreach (SpriteRenderer SR in childSprites)
                    {
                        //collect your first greyscale
                        if (collected1Greyscale == false)
                        {
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

                        //low fuel warning message
                        hasHadFuelWarning = true;
                        if (nonSequentialModuleList[2].gameObject.activeSelf == false)
                            TutorialPopup(5, false, true, true);

                        GameController.Instance.LoadNextLevelSection();
                    }
                }

                if (playerBot.storedRed < 14 && outOfFuel == false)
                {

                    print("out of fuel");
                    outOfFuel = true;
                    timerStartRedDrop = Time.time;
                    redDropTimer = true;
                }

                if (redDropTimer == true)
                {

                    if (Time.time - timerStartRedDrop > timeToRedDrop)
                    {

                        redDropTimer = false;
                        frameCounter = 0;
                        hasSpawnedRed = true;
                    }
                }


                if (hasSpawnedRed)
                {

                    if (frameCounter == 60)
                    {

                        frameChecks++;
                        List<SpriteRenderer> childSprites =
                            new List<SpriteRenderer>(playerPos.GetComponentsInChildren<SpriteRenderer>());
                        foreach (SpriteRenderer SR in childSprites)
                        {

                            if (SR.sprite == red || frameChecks >= 45)
                            {

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

                        List<SpriteRenderer> childSprites =
                            new List<SpriteRenderer>(playerPos.GetComponentsInChildren<SpriteRenderer>());
                        foreach (SpriteRenderer SR in childSprites)
                        {

                            if (SR.sprite == red)
                            {

                                if (!redSprites.Contains(SR.gameObject))
                                {

                                    redSprites.Add(SR.gameObject);
                                    redCounter++;
                                }
                            }
                        }

                        frameCounter = 0;
                    }

                    frameCounter++;
                }
            }
        }

        //Called when the player dies, quits or when we exit to menus
        public void ResetVariables()
        {

            collected1Greyscale = false;
            level1Upgraded = false;
            beganGreyscaleSection = false;
            hasHadFuelWarning = false;
            outOfFuel = false;
            redDropTimer = false;
            timerStartRedDrop = 0;
            hasSpawnedRed = false;
            collectRed = false;
            redCounter = 0;
            frameChecks = 0;
            isBotDead = false;
            frameCounter = 0;
            fuelFreezeObj.SetActive(false);

        }

        //is sequential marks events that should happen chronologically
        public void TutorialPopup(int module, bool pauseGame, bool toggleOnOff, bool isSequential)
        {
            //Bump the asteroid popups if there's already a message up
            if (isSequential == false && toggleOnOff == true && (module == 0 || module == 1))
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
            if (pauseGame == true || GameController.Instance.isPaused)
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

                if (isSequential)
                    sequentialModuleList[module].SetActive(false);
                else
                {

                    nonSequentialModuleList[module].SetActive(false);
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
        }

        public void CloseCurrent()
        {

            for (int i = 0; i < sequentialModuleList.Count; i++)
            {

                TutorialPopup(i, false, false, true);
            }

            for (int j = 0; j < nonSequentialModuleList.Count; j++)
            {

                TutorialPopup(j, false, false, false);
            }
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
                    TutorialPopup(0, false, true, false);
                    break;
                case 2:
                    TutorialPopup(1, false, true, false);
                    break;
                default:
                    break;
            }
        }

        public void SetFuel(int fuel)
        {

            playerPos.GetComponent<Bot>().SetFuelAmt(fuel);
        }

        public void OnLevelChange(int newSection)
        {
            currentSection = newSection;
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
            GameController.Instance.game = GameController.Instance.easyNonTutorial;
            GameController.Instance.LoadNextLevelSection();
            tutorialHasFinished = true;
            CloseCurrent();
            SetFuel(40);
            tutorialPanel.SetActive(false);
            this.enabled = false;
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
}