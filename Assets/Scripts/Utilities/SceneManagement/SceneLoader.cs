using StarSalvager.Utilities.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarSalvager.Utilities.SceneManagement
{
    public static class SceneLoader
    {
        private static Dictionary<string, SceneRoot> _scenes = new Dictionary<string, SceneRoot>()
        {
            { "MainMenuScene", null },
            { "LevelScene", null },
            { "ScrapyardScene", null },
            { "AlexShulmanTestScene", null }
        };

        private static MonoBehaviour _coroutineRunner = null;
        private static bool _sceneLoaderReady = false;


        public static void SubscribeSceneRoot(SceneRoot sceneRoot, string sceneName)
        {
            if (!_sceneLoaderReady)
            {
                SetupSceneLoader(sceneRoot, sceneName);
                return;
            }

            if (_scenes[sceneName] == null)
            {
                _scenes[sceneName] = sceneRoot;
                return;
            }
        }

        private static void SetupSceneLoader(SceneRoot sceneRoot, string sceneName)
        {
            _sceneLoaderReady = true;

            if (_coroutineRunner == null)
                _coroutineRunner = sceneRoot;

            SubscribeSceneRoot(sceneRoot, sceneName);

            _coroutineRunner.StartCoroutine(Startup());
        }

        private static IEnumerator Startup()
        {
            List<string> enumerationKeys = _scenes.Keys.ToList();
            
            foreach (string entry in enumerationKeys)
            {
                yield return _coroutineRunner.StartCoroutine(LoadSceneAsync(entry));
            }

            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("StarSalvagerMainScene"))
            {
                ActivateScene("MainMenuScene");
                SetActiveScene("MainMenuScene");
            }
            else
            {
                ActivateScene(SceneManager.GetActiveScene().name);
            }
        }

        private static IEnumerator LoadSceneAsync(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).IsValid())
            {
                yield break;
            }

            AsyncOperation asyncLoadLevel = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoadLevel.isDone)
            {
                yield return null;
            }

            while(_scenes[sceneName] == null || !_scenes[sceneName].IsStarted)
            {
                yield return null;
            }

            DeactivateScene(sceneName);
        }

        public static bool SetActiveScene(string sceneName)
        {
            if (_scenes.ContainsKey(sceneName))
            {
                SceneManager.SetActiveScene(_scenes[sceneName].Scene);
                return true;
            }

            return false;
        }

        public static bool ActivateScene(string sceneName)
        {
            return SetSceneObjectsActive(sceneName, true);
        }

        public static bool DeactivateScene(string sceneName)
        {
            return SetSceneObjectsActive(sceneName, false);
        }

        public static bool ActivateScene(string sceneName, string sceneNameToDeload)
        {
            SetSceneObjectsActive(sceneNameToDeload, false);
            return SetSceneObjectsActive(sceneName, true);
        }

        private static bool SetSceneObjectsActive(string sceneName, bool active)
        {
            if (_scenes.ContainsKey(sceneName) && _scenes[sceneName] != null)
            {
                _scenes[sceneName].SetSceneObjectsActive(active);
                return true;
            }
            
            Debug.Log("Attempted to set scene active that is not loaded");
            return false;
        }
    }
}