using System;
using System.Linq;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace StarSalvager.Prototype
{
    public class SceneLoaderPrototype : MonoBehaviour
    {
        [Serializable]
        public class SceneData
        {
            [SerializeField]
            public int SceneIndex;
            [SerializeField]
            private string SceneTitle;

            [SerializeField, TextArea]
            public string SceneDescription;
            [Required]
            [SerializeField]
            private Button button;

            public void Init()
            {
                button.GetComponentInChildren<TMP_Text>().text = SceneTitle;
            
                button.onClick.AddListener(() =>
                {
                    SceneManager.LoadScene(SceneIndex);
                });
            }
        
        }

        [SerializeField]
        private SceneData[] _sceneDatas;

        [SerializeField, Required]
        private GameObject SceneSelectionWindowObject;

        [SerializeField, Required]
        private GameObject SceneDescriptionWindowObject;
        private TMP_Text descriptionText;
        
        [SerializeField]
        private Button quitButton;

        [SerializeField]
        private Button resetButton;
        [SerializeField]
        private Button returnToMenuButton;
        // Start is called before the first frame update
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(EventSystem.current.gameObject);
            
            descriptionText = SceneDescriptionWindowObject.GetComponentInChildren<TMP_Text>();
            
            resetButton.gameObject.SetActive(false);
            returnToMenuButton.gameObject.SetActive(false);
            SceneDescriptionWindowObject.SetActive(false);

            foreach (var sceneData in _sceneDatas)
            {
                sceneData.Init();
            }
            
            resetButton.onClick.AddListener(() =>
            {
                var index = SceneManager.GetActiveScene().buildIndex;
                SceneManager.LoadScene(index);
            });
            
            returnToMenuButton.onClick.AddListener(() =>
            {
                SceneManager.LoadScene(0);
            });
            
            quitButton.onClick.AddListener(() =>
            {
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                
#else
Application.Quit();
#endif
            });

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                var index = scene.buildIndex;
                SceneSelectionWindowObject.SetActive(index == 0);
                SceneDescriptionWindowObject.SetActive(index != 0);
                
                resetButton.gameObject.SetActive(index != 0);
                returnToMenuButton.gameObject.SetActive(index != 0);
                
                var data = _sceneDatas.FirstOrDefault(d => d.SceneIndex == index);

                if (data != null)
                    descriptionText.text = data.SceneDescription;

            };
        }
    }
}

