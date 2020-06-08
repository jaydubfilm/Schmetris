using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Flying text asset used for score and resource gain effects
    public class FloatingText : MonoBehaviour
    {
        //Components
        Text scoreText;
        Vector3 targetPos = Vector3.zero;
        const float floatSpeed = 100.0f;
        const float fadeTime = 1.0f;

        //Specifics of text effect to be set at spawn
        public void Init(string message, Vector3 target, int size, Color color)
        {
            scoreText = GetComponent<Text>();
            targetPos = target;
            scoreText.text = message;
            scoreText.fontSize = size;
            scoreText.color = color;
            StartCoroutine(FadeOverTime());
        }

        //Move toward target position
        void Update()
        {
            transform.position += (targetPos - transform.position).normalized * floatSpeed * Time.deltaTime;
        }

        //Fade out over time and destroy once completely invisible
        IEnumerator FadeOverTime()
        {
            float time = 0;
            while (time < fadeTime)
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
}