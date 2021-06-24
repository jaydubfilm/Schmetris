using System;
using System.Collections;
using StarSalvager.Utilities;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFade : Singleton<ScreenFade>
{
    public const float DEFAULT_TIME = 1f;
    [SerializeField]
    private Image image;

    public static bool Fading { get; private set; }

    //Unity Functions
    //====================================================================================================================//
    
    // Start is called before the first frame update
    private void Start()
    {
        image.gameObject.SetActive(false);
    }

    //ScreenFade Functions
    //====================================================================================================================//

    public static void Fade(Action onFadedCallback, Action onFadeCompleted = null, float time = DEFAULT_TIME)
    {
        if (Instance == null)
            return;

        Instance.FadeScreen(onFadedCallback, onFadeCompleted, time);

    }

    private void FadeScreen(Action onFadedCallback, Action onFadeCompleted, float time)
    {
        if (Fading) return;

        StartCoroutine(FadeCoroutine(onFadedCallback, onFadeCompleted, time));

    }

    public static void WaitForFade(Action onFadeFinishedCallback)
    {
        if (Instance == null)
            return;
        
        Instance.StartCoroutine(WaitForFadeCoroutine(onFadeFinishedCallback));
    }

    //Coroutines
    //====================================================================================================================//
    
    private static IEnumerator WaitForFadeCoroutine(Action onFadeFinishedCallback)
    {
        yield return new WaitUntil(() => Fading == false);
        
        onFadeFinishedCallback?.Invoke();
    }
    

    private IEnumerator FadeCoroutine(Action onFadedCallback, Action onFadeCompleted, float time)
    {
        Fading = true;
        
        var startColor = Color.clear;
        var endColor = Color.black;
        
        image.gameObject.SetActive(true);

        yield return StartCoroutine(FadeColorCoroutine(image, startColor, endColor, time / 2f));
        
        onFadedCallback?.Invoke();

        yield return StartCoroutine(FadeColorCoroutine(image, endColor, startColor, time / 2f));

        image.gameObject.SetActive(false);
        
        onFadeCompleted?.Invoke();
        
        Fading = false;
        
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
