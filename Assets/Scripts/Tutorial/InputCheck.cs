using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using Sirenix.OdinInspector;

public class InputCheck : MonoBehaviour
{

    float alpha;
    private bool startAdditional;
    Image image;
    TextMeshProUGUI text;
    public float timeBeforeFade;
    public float fadeSpeed;
    public float timeUntilNextPrompt = 6;
    bool inputDetected;
    float timeAtInput;
    bool startFade;
    bool queueNext;
    public bool needsInput;
    private bool canDetect = true;
    public bool QueueAdditional;
    public bool queuedIsSequential;
    //public float queueDelay;
    public bool changeSectionOnFinish;
    public bool runEventOnFinish;
    public UnityEvent eventOnFinish;

    bool startAdditionalAtStart;
    public float timeBeforeFadeAtStart;
    public float fadeSpeedAtStart;
    bool inputDetectedAtStart;
    float timeAtInputAtStart;
    bool startFadeAtStart;
    bool queueNextAtStart;
    public bool needsInputAtStart;
    private bool canDetectAtStart;
    public bool QueueAdditionalAtStart;
    public bool queuedIsSequentialAtStart;
    public bool changeSectionOnFinishAtStart;
    public bool runEventOnFinishAtStart;


    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();


        startAdditionalAtStart = startAdditional;
        timeBeforeFadeAtStart = timeBeforeFade;
        fadeSpeedAtStart = fadeSpeed;
        inputDetectedAtStart = inputDetected;
        timeAtInputAtStart = timeAtInput;
        startFadeAtStart = startFade;
        queueNextAtStart = queueNext;
        needsInputAtStart = needsInput;
        canDetectAtStart = canDetect;
        QueueAdditionalAtStart = QueueAdditional;
        queuedIsSequentialAtStart = queuedIsSequential;
        changeSectionOnFinishAtStart = changeSectionOnFinish;
        runEventOnFinishAtStart = runEventOnFinish;

        if (inputDetected == false && canDetect == true && needsInput == false)
        {
            inputDetected = true;
            timeAtInput = Time.time;
        }
    }

    void OnEnable()
    {
        
        if (inputDetected == false && canDetect == true && needsInput == false)
        {
            inputDetected = true;
            timeAtInput = Time.time;
        }

        alpha = 0.686f;


    }

    // Update is called once per frame
    void Update()
    {
        if (inputDetected == false && canDetect == true && needsInput == true)
        { 

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
            {

                inputDetected = true;
                timeAtInput = Time.time;
            }
        }

       


        if (inputDetected == true)
        {

            if(Time.time -timeAtInput > timeBeforeFade && canDetect == true)
            {

                timeAtInput = Time.time;
                startFade = true;
                canDetect = false;
            }

            if(startFade == true)
            {
                alpha = 0.686f - ((Time.time - timeAtInput) * fadeSpeed);
                Color imageColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
                image.color = imageColor;
                Color textColor = new Color(text.color.r, text.color.g, text.color.b, alpha);
                text.color = textColor;
                if (alpha < 0)
                {

                    

                    startFade = false;

                    if (runEventOnFinish)
                    {
                        eventOnFinish.Invoke();
                    }

                    if (changeSectionOnFinish)
                    {
                        GameController.Instance.LoadNextLevelSection();
                    }
                    alpha = 0;
                    //Reset();
                    startAdditional = true;

                }
            }

            if (QueueAdditional == true && startAdditional)
            {
                if (queueNext == false)
                {
                    if (Time.time - timeAtInput > timeBeforeFade + timeUntilNextPrompt)
                    {

                        //GameController.Instance.LoadNextLevelSection();
                        //"Get In, Get out"....

                        TutorialManager.Instance.CloseAndOpenNextUnpaused();
                        queueNext = true;
                    }
                }
            }
        }
    }

    public  void Reset()
    {

        //if (inputDetected == false && canDetect == true && needsInput == false)
        //{
        //    timeAtInput = Time.time;
        //}
        //gameObject.SetActive(false);
        inputDetected = false;
        alpha = 0.686f;
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0.686f);
        text.color = new Color(text.color.r, text.color.g, text.color.b, 0.686f);
        startAdditional = startAdditionalAtStart;
        timeBeforeFade = timeBeforeFadeAtStart;
        fadeSpeed = fadeSpeedAtStart;
        inputDetected = false;
        startFade = startFadeAtStart;
        queueNext = queueNextAtStart;
        needsInput = needsInputAtStart;
        canDetect = canDetectAtStart;
        QueueAdditional = QueueAdditionalAtStart;
        queuedIsSequential = queuedIsSequentialAtStart;
        changeSectionOnFinish = changeSectionOnFinishAtStart;
        runEventOnFinish = runEventOnFinishAtStart;
        gameObject.SetActive( false);

    }

    [Button]
    public void ShowTime()
    {
        print(timeAtInput);
    }
}
