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
    bool inputDetected;
    float timeAtInput;
    bool startFade;
    
    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inputDetected == false)
        { 

            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            {

                inputDetected = true;
                timeAtInput = Time.time;
            }
        }

        if(inputDetected == true)
        {

            if(Time.time -timeAtInput > timeBeforeFade)
            {

                startFade = true;
                timeAtInput = Time.time;
            }

            if(startFade == true)
            {

                alpha = 1 - ((Time.time - timeAtInput) * fadeSpeed);
                Color imageColor = new Color(image.color.r, image.color.g, image.color.b, alpha);
                image.color = imageColor;
                Color textColor = new Color(text.color.r, text.color.g, text.color.b, alpha);
                text.color = textColor;
                if (alpha < 0)
                    Destroy(gameObject);
            }
        }
    }
}
