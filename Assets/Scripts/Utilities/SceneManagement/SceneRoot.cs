        #if UNITY_EDITOR

        [SerializeField, Required, BoxGroup("Editor Only")]
        private SceneLoaderScriptableObject sceneLoadObjects;

        private static bool _sceneEditorObjectsLoaded;
        
        private void Awake()
        {
            if (gameObject.scene.buildIndex == 0 || _sceneEditorObjectsLoaded)
            {
                _sceneEditorObjectsLoaded = true;
                return;
            }

            foreach (var requiredPrefab in sceneLoadObjects.sceneRequiredPrefabs)
            {
                Instantiate(requiredPrefab);
            }

            _sceneEditorObjectsLoaded = true;
        }
        
        #endif
