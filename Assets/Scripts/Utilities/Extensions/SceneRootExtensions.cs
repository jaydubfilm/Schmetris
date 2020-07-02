using StarSalvager.SceneLoader;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class SceneRootExtensions
    {
        public static void SetSceneObjectsActive(this SceneRoot sceneRoot, bool state)
        {
            foreach(GameObject gameObject in sceneRoot.Scene.GetRootGameObjects())
            {
                gameObject.SetActive(state);
            }
        }
    }
}
