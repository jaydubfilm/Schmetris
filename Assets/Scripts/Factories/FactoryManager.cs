using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Factories
{
    
    //Based on: https://www.dofactory.com/net/factory-method-design-pattern
    public class FactoryManager : Singleton<FactoryManager>
    {
        [SerializeField]
        public bool DisableTestingFeatures;
        
        [SerializeField, Required, BoxGroup("Temporary")]
        private MissionRemoteDataScriptableObject missionRemoteData;
        public MissionRemoteDataScriptableObject MissionRemoteData => missionRemoteData;

        [SerializeField, Required, BoxGroup("Temporary")]
        private List<SectorModularData> m_sectorRemoteData;
        
        public EditorBotShapeGeneratorData EditorBotShapeData
        {
            get
            {
                if (editorBotShapeData == null)
                    editorBotShapeData = ImportBotShapeRemoteData();

                return editorBotShapeData;
            }
        }
        
        private EditorBotShapeGeneratorData editorBotShapeData;

        public List<SectorRemoteDataScriptableObject> SectorRemoteData => m_sectorRemoteData[currentModularDataIndex].SectorData;

        [SerializeField, Required, BoxGroup("Temporary")]
        public int currentModularDataIndex = 0;

        public int ModularDataCount => m_sectorRemoteData.Count;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private AttachableProfileScriptableObject bitProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private BitRemoteDataScriptableObject bitRemoteData;
        
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Attachables/Components")]
        private AttachableProfileScriptableObject componentProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Components")]
        public ComponentRemoteDataScriptableObject componentRemoteData;


        
        //============================================================================================================//

        public RemotePartProfileScriptableObject PartsRemoteData => partRemoteData;

        [SerializeField, Required, BoxGroup("Attachables/Parts")] 
        private AttachableProfileScriptableObject partProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Parts")] 
        private RemotePartProfileScriptableObject partRemoteData;

        [SerializeField, Required, BoxGroup("Attachables")] 
        private GameObject shapePrefab;
        
        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Enemies")]
        private EnemyProfileScriptableObject enemyProfile;
        public EnemyProfileScriptableObject EnemyProfile => enemyProfile;

        [SerializeField, Required, BoxGroup("Enemies")]
        private EnemyRemoteDataScriptableObject enemyRemoteData;

        [SerializeField, Required, BoxGroup("Projectiles")]
        private ProjectileProfileScriptableObject projectileProfile;
        public ProjectileProfileScriptableObject ProjectileProfile => projectileProfile;
        
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject botPrefab;
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject shieldPrototypePrefab;
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject alertIconPrefab;
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject scrapyardBotPrefab;
        [SerializeField, Required, BoxGroup("Puzzle Combos")]
        private ComboRemoteDataScriptableObject comboRemoteData;
        
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Damage")]
        private GameObject damageFactory;
        
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Particles")]
        private GameObject explosionPrefab;

        //============================================================================================================//

        private Dictionary<Type, FactoryBase> _factoryBases;
        
        //============================================================================================================//

        public T GetFactory<T>() where T : FactoryBase
        {
            var type = typeof(T);
            
            if (_factoryBases == null)
            {
                _factoryBases = new Dictionary<Type, FactoryBase>();
            }

            if (!_factoryBases.ContainsKey(type))
            {
                _factoryBases.Add(type, CreateFactory<T>());
            }
            
            
            return _factoryBases[type] as T;
        }
        
        //============================================================================================================//

        private T CreateFactory<T>() where T : FactoryBase
        {
            var type = typeof(T);
            switch (true)
            {
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(BitAttachableFactory):
                    return new BitAttachableFactory(bitProfile, bitRemoteData) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(PartAttachableFactory):
                    return new PartAttachableFactory(partProfile, partRemoteData) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(ComponentAttachableFactory):
                    return new ComponentAttachableFactory(componentProfile, componentRemoteData) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(ShapeFactory):
                    return new ShapeFactory(shapePrefab, EditorBotShapeData.GetEditorShapeData()) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(EnemyFactory):
                    return new EnemyFactory(enemyProfile, enemyRemoteData) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(ProjectileFactory):
                    return new ProjectileFactory(projectileProfile) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(ComboFactory):
                    return new ComboFactory(comboRemoteData) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(BotFactory):
                    return new BotFactory(botPrefab, scrapyardBotPrefab, shieldPrototypePrefab, alertIconPrefab) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(DamageFactory):
                    return new DamageFactory(damageFactory) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(ParticleFactory):
                    return new ParticleFactory(explosionPrefab) as T;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type.Name, null);
                //----------------------------------------------------------------------------------------------------//
            }
        }

        //============================================================================================================//

        public string ExportBotShapeRemoteData(EditorBotShapeGeneratorData editorData)
        {
            if (editorData == null)
                return string.Empty;

            var export = JsonConvert.SerializeObject(editorData, Formatting.None);
#if !UNITY_EDITOR
            System.IO.File.WriteAllText(Application.dataPath + "/BuildData/BotShapeEditorData.txt", export);
#else
            System.IO.File.WriteAllText(Application.dataPath + "/RemoteData/AddToBuild/BotShapeEditorData.txt", export);
#endif

            return export;
        }

        public EditorBotShapeGeneratorData ImportBotShapeRemoteData()
        {
#if !UNITY_EDITOR
            if (!File.Exists(Application.dataPath + "/BuildData/BotShapeEditorData.txt"))
            {
                return new EditorBotShapeGeneratorData();
            }

            var loaded = JsonConvert.DeserializeObject<EditorBotShapeGeneratorData>(File.ReadAllText(Application.dataPath + "/BuildData/BotShapeEditorData.txt"));

            return loaded;
#else
            if (!File.Exists(Application.dataPath + "/RemoteData/AddToBuild/BotShapeEditorData.txt"))
                return new EditorBotShapeGeneratorData();

            var loaded = JsonConvert.DeserializeObject<EditorBotShapeGeneratorData>(File.ReadAllText(Application.dataPath + "/RemoteData/AddToBuild/BotShapeEditorData.txt"));

            return loaded;
#endif
        }
        
        public void ClearRemoteData()
        {
            var directory = new DirectoryInfo(Application.dataPath + "/RemoteData/");
            
            //FIXME This should be using persistent file names
            var files = new List<FileInfo>();
            files.AddRange(directory.GetFiles("*.player"));
            files.AddRange(directory.GetFiles("*.mission"));
            files.AddRange(directory.GetFiles("*.player.meta"));
            files.AddRange(directory.GetFiles("*.mission.meta"));


            foreach (var file in files)
            {
                if(file == null)
                    continue;
                
                File.Delete(file.FullName);
            }

            if (Application.isPlaying)
            {
                PlayerPersistentData.ClearPlayerData();
            }

        }
        
        //============================================================================================================//

    }
}


