using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using UnityEngine;

namespace StarSalvager.Factories
{
    
    //Based on: https://www.dofactory.com/net/factory-method-design-pattern
    public class FactoryManager : Singleton<FactoryManager>
    {
        [SerializeField, Required, BoxGroup("Temporary")]
        private MissionRemoteDataScriptableObject missionRemoteData;
        public MissionRemoteDataScriptableObject MissionRemoteData => missionRemoteData;

        [SerializeField, Required, BoxGroup("Temporary")]
        private List<SectorModularData> m_sectorRemoteData;

        public List<SectorRemoteDataScriptableObject> SectorRemoteData => m_sectorRemoteData[currentModularDataIndex].SectorData;

        [SerializeField, Required, BoxGroup("Temporary")]
        public int currentModularDataIndex = 0;

        public int ModularDataCount => m_sectorRemoteData.Count;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private AttachableProfileScriptableObject bitProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private BitRemoteDataScriptableObject bitRemoteData;
        
        [SerializeField, Required, BoxGroup("Attachables/Components")]
        private AttachableProfileScriptableObject componentProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Components")]
        private ComponentRemoteDataScriptableObject componentRemoteData;

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

        [SerializeField, Required, BoxGroup("Attachables/Parts")] 
        private AttachableProfileScriptableObject partProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Parts")] 
        private RemotePartProfileScriptableObject partRemoteData;

        [SerializeField, Required, BoxGroup("Attachables")] 
        private GameObject shapePrefab;

        [SerializeField, Required, BoxGroup("Enemies")]
        private EnemyProfileScriptableObject enemyProfile;
        public EnemyProfileScriptableObject EnemyProfile => enemyProfile;

        [SerializeField, Required, BoxGroup("Enemies")]
        private EnemyRemoteDataScriptableObject enemyRemoteData;

        [SerializeField, Required, BoxGroup("Projectiles")]
        private ProjectileProfileScriptableObject projectileProfile;
        public ProjectileProfileScriptableObject ProjectileProfile => projectileProfile;
        
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject botPrefab;
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject scrapyardBotPrefab;
        [SerializeField, Required, BoxGroup("Puzzle Combos")]
        private ComboRemoteDataScriptableObject comboRemoteData;
        
        [SerializeField, Required, BoxGroup("Damage")]
        private GameObject damageFactory;

        //============================================================================================================//

        //FIXME This needs to be converted to an array
        //private FactoryBase _bitAttachableFactory;
        //private FactoryBase _partAttachableFactory;
        //private FactoryBase _shapeFactory;
        //private FactoryBase _enemyFactory;
        //private FactoryBase _projectileFactory;
        //private FactoryBase _comboFactory;
        //private FactoryBase _botFactory;
        //private FactoryBase _damageFactory;

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
        
        //#if UNITY_EDITOR || DEVELOPMENT_BUILD
        //
        //[SerializeField, Required, BoxGroup("Attachables/Bits"), Space(10f)]
        //private AttachableProfileScriptableObject bitProfileAlt;
        //
        //private bool bitToggle;
        //[BoxGroup("Attachables/Bits"), Button("Toggle Bit Profile"), DisableInEditorMode]
        //public void ToggleBitProfile()
        //{
        //    bitToggle = !bitToggle;
//
        //    _bitAttachableFactory = bitToggle
        //        ? new BitAttachableFactory(bitProfileAlt, bitRemoteData)
        //        : new BitAttachableFactory(bitProfile, bitRemoteData);
        //}
        //
        //#endif
        
        //============================================================================================================//

    
        /// <summary>
        /// Obtains a FactoryBase of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        //TODO Investigate whether or not I can combine both BitAttachableFactory & PartAttachableFactory into a single 
        /*public T GetFactory<T>() where T: FactoryBase
        {
            var typeName = typeof(T).Name;
            switch (typeName)
            {
                case nameof(BitAttachableFactory):
                    return (_bitAttachableFactory ?? (_bitAttachableFactory = new BitAttachableFactory(bitProfile, bitRemoteData))) as T;

                case nameof(PartAttachableFactory):
                    return (_partAttachableFactory ?? (_partAttachableFactory = new PartAttachableFactory(partProfile, partRemoteData))) as T;
                
                case nameof(ShapeFactory):
                    return (_shapeFactory ?? (_shapeFactory = new ShapeFactory(shapePrefab, EditorBotShapeData.GetEditorShapeData()))) as T;

                case nameof(EnemyFactory):
                    return (_enemyFactory ?? (_enemyFactory = new EnemyFactory(enemyProfile, enemyRemoteData))) as T;

                case nameof(ProjectileFactory):
                    return (_projectileFactory ?? (_projectileFactory = new ProjectileFactory(projectileProfile))) as T;
                
                case nameof(ComboFactory):
                    return (_comboFactory ?? (_comboFactory = new ComboFactory(comboRemoteData))) as T;
                
                case nameof(BotFactory):
                    return (_botFactory ?? (_botFactory = new BotFactory(botPrefab, scrapyardBotPrefab))) as T;
                
                case nameof(DamageFactory):
                    return (_damageFactory ?? (_damageFactory = new DamageFactory(damageFactory))) as T;

                default:
                    throw new ArgumentOutOfRangeException(nameof(typeName), typeName, null);
            }
        }*/

        private T CreateFactory<T>() where T : FactoryBase
        {
            var type = typeof(T);
            switch (true)
            {
                case bool _ when type == typeof(BitAttachableFactory):
                    return new BitAttachableFactory(bitProfile, bitRemoteData) as T;
                case bool _ when type == typeof(PartAttachableFactory):
                    return new PartAttachableFactory(partProfile, partRemoteData) as T;
                case bool _ when type == typeof(ComponentAttachableFactory):
                    return new ComponentAttachableFactory(componentProfile, componentRemoteData) as T;
                case bool _ when type == typeof(ShapeFactory):
                    return new ShapeFactory(shapePrefab, EditorBotShapeData.GetEditorShapeData()) as T;
                case bool _ when type == typeof(EnemyFactory):
                    return new EnemyFactory(enemyProfile, enemyRemoteData) as T;
                case bool _ when type == typeof(ProjectileFactory):
                    return new ProjectileFactory(projectileProfile) as T;
                case bool _ when type == typeof(ComboFactory):
                    return new ComboFactory(comboRemoteData) as T;
                case bool _ when type == typeof(BotFactory):
                    return new BotFactory(botPrefab, scrapyardBotPrefab) as T;
                case bool _ when type == typeof(DamageFactory):
                    return new DamageFactory(damageFactory) as T;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type.Name, null);
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
                Debug.LogError("BROKEN");
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
        
        #if UNITY_EDITOR
        public void ClearRemoteData()
        {
            //FIXME This should be using persistent file names
            var files = new[]
            {
                Application.dataPath + "/RemoteData/PlayerPersistentData.player",
                Application.dataPath + "/RemoteData/MissionsCurrentData.mission",
                Application.dataPath + "/RemoteData/MissionsMasterData.mission"
            };

            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    File.Delete(file + ".meta");
                }
                else
                {
                    Debug.LogError($"{file} does not exist");
                }
            }
        }
        #endif
        
        //============================================================================================================//

    }
}


