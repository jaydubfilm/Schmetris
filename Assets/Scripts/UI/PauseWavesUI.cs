using Sirenix.OdinInspector;
using StarSalvager.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class PauseWavesUI : MonoBehaviour
    {

        [SerializeField, Required, BoxGroup("View")]
        private Button continueButton;
        [SerializeField, Required, BoxGroup("View")]
        private Button scrapyardButton;

        // Start is called before the first frame update
        void Start()
        {
            InitButtons();
        }

        private void InitButtons()
        {
            continueButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                gameObject.SetActive(false);
            });


            scrapyardButton.onClick.AddListener(() =>
            {
                GameTimer.SetPaused(false);
                StarSalvager.SceneLoader.SceneLoader.ActivateScene("ScrapyardScene", "AlexShulmanTestScene");
            });
        }
    }
}