using System;
using System.Collections;
using StarSalvager.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : Singleton<ScreenFade>
{
    [SerializeField]
    private Image image;

    public bool Fading => _fading;

    private bool _fading;
    
    //Unity Functions
    //====================================================================================================================//
    
    // Start is called before the first frame update
    private void Start()
    {
        image.gameObject.SetActive(false);
    }

    //ScreenFade Functions
    //====================================================================================================================//

    public static void Fade(Action onFadedCallback, float time = 1f)
    {
        if (Instance == null)
            return;

        Instance.FadeScreen(onFadedCallback, time);

    }

    private void FadeScreen(Action onFadedCallback, float time)
    {
        if (_fading)
            return;

        StartCoroutine(FadeCoroutine(onFadedCallback, time));

    }

    public void WaitForFade(Action onFadeFinishedCallback)
    {
        StartCoroutine(WaitForFadeCoroutine(onFadeFinishedCallback));
    }

    //Coroutines
    //====================================================================================================================//
    
    private static IEnumerator WaitForFadeCoroutine(Action onFadeFinishedCallback)
    {
        yield return new WaitUntil(() => Instance.Fading == false);
        
        onFadeFinishedCallback?.Invoke();
    }
    

    private IEnumerator FadeCoroutine(Action onFadedCallback, float time)
    {
        _fading = true;
        
        var startColor = Color.clear;
        var endColor = Color.black;
        
        image.gameObject.SetActive(true);

        yield return StartCoroutine(FadeColorCoroutine(image, startColor, endColor, time / 2f));
        
        onFadedCallback?.Invoke();

        yield return StartCoroutine(FadeColorCoroutine(image, endColor, startColor, time / 2f));

        image.gameObject.SetActive(false);
        
        _fading = false;
    }

    private static IEnumerator FadeColorCoroutine(Graphic targetImage, Color startColor, Color endColor, float time)
    {
        var t = 0f;

        targetImage.color = startColor;

        while (t / time < 1f)
        {
            targetImage.color = Color.Lerp(startColor, endColor, t / time);

            t += Time.deltaTime;
            yield return null;
        }
    }
}
