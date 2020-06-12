using System;
using System.Collections;
using System.Collections.Generic;
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
            private int SceneIndex;
            [SerializeField]
            private string SceneTitle;
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

        [SerializeField]
        private Button resetButton;
        [SerializeField]
        private Button returnToMenuButton;
        // Start is called before the first frame update
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(EventSystem.current.gameObject);
            
            resetButton.gameObject.SetActive(false);
            returnToMenuButton.gameObject.SetActive(false);
            
            
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

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                SceneSelectionWindowObject.SetActive(scene.buildIndex == 0);
                resetButton.gameObject.SetActive(scene.buildIndex != 0);
                returnToMenuButton.gameObject.SetActive(scene.buildIndex != 0);
            };
        }
    }
}

