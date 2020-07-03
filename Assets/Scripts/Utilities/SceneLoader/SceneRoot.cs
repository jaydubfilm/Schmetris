using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StarSalvager.SceneLoader
{
    public class SceneRoot : MonoBehaviour
    {
        [NonSerialized]
        public Scene Scene;
        public bool IsStarted { get; private set; } = false;
        public bool IsReady { get; private set; } = false;

        protected virtual void Start()
        {
            Scene = gameObject.scene;
            SceneLoader.SubscribeSceneRoot(this, Scene.name);

            IsStarted = true;
        }

        public virtual void SetupScene()
        {
            IsReady = true;
        }
    }
}