using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    Text scoreText;

    Vector3 targetPos = Vector3.zero;

    const float floatSpeed = 100.0f;
    const float fadeTime = 1.0f;

    public void Init(string message, Vector3 target, int size, Color color)
    {
        scoreText = GetComponent<Text>();
        targetPos = target;
        scoreText.text = message;
        scoreText.fontSize = size;
        scoreText.color = color;
        StartCoroutine(FadeOverTime());
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += (targetPos - transform.position).normalized * floatSpeed * Time.deltaTime;
    }

    IEnumerator FadeOverTime()
    {
        float time = 0;
        while(time < fadeTime)
        {
            time += Time.deltaTime;
            Color textColor = scoreText.color;
            textColor.a = 1 - time / fadeTime;
            scoreText.color = textColor;
            yield return 0;
        }
        Destroy(gameObject);
    }
}
