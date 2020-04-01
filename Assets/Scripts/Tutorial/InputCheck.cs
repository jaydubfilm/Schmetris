using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputCheck : MonoBehaviour
{

    float alpha;
    Image image;
    TextMeshProUGUI text;
    public float timeBeforeFade;
    public float fadeSpeed;
    public float timeUntilNextPrompt = 6;
    bool inputDetected;
    float timeAtInput;
    bool startFade;
    bool queueNext;
    private bool canDetect = true;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputDetected == false && canDetect == true)
        { 

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
            {

                inputDetected = true;
                timeAtInput = Time.time;
                print("keypress");
            }
        }

        if(inputDetected == true)
        {

            if(Time.time -timeAtInput > timeBeforeFade && canDetect == true)
            {

                timeAtInput = Time.time;
                startFade = true;
                canDetect = false;
                //inputDetected = false;
                print("2");
            }

            if(startFade == true)
            {
                alpha = 1 - ((Time.time - timeAtInput) * fadeSpeed);
                print(alpha);
                Color imageColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
                image.color = imageColor;
                Color textColor = new Color(text.color.r, text.color.g, text.color.b, alpha);
                text.color = textColor;
                if (alpha < 0)
                {
                    startFade = false;
                    alpha = 0;
                    print("zero");
                }
            }

            if (queueNext == false)
            {
                if (Time.time - timeAtInput > timeBeforeFade + timeUntilNextPrompt)
                {
                    print("next");
                    TutorialManager.Instance.TutorialPopup(2, true, true, 0);
                    queueNext = true;
                }
            }
        }
    }
}
