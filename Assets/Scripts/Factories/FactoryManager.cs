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

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private AttachableProfileScriptableObject bitProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private BitRemoteDataScriptableObject bitRemoteData;

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
        private FactoryBase _bitAttachableFactory;
        private FactoryBase _partAttachableFactory;
        private FactoryBase _shapeFactory;
        private FactoryBase _enemyFactory;
        private FactoryBase _projectileFactory;
        private FactoryBase _comboFactory;
        private FactoryBase _botFactory;
        private FactoryBase _damageFactory;
        
        //============================================================================================================//
        
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        
        [SerializeField, Required, BoxGroup("Attachables/Bits"), Space(10f)]
        private AttachableProfileScriptableObject bitProfileAlt;
        
        private bool bitToggle;
        [BoxGroup("Attachables/Bits"), Button("Toggle Bit Profile"), DisableInEditorMode]
        public void ToggleBitProfile()
        {
            bitToggle = !bitToggle;

            _bitAttachableFactory = bitToggle
                ? new BitAttachableFactory(bitProfileAlt, bitRemoteData)
                : new BitAttachableFactory(bitProfile, bitRemoteData);
        }
        
        #endif
        
        //============================================================================================================//

    
        /// <summary>
        /// Obtains a FactoryBase of Type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        //TODO Investigate whether or not I can combine both BitAttachableFactory & PartAttachableFactory into a single 
        public T GetFactory<T>() where T: FactoryBase
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
        }

        //============================================================================================================//

        public EditorBotShapeGeneratorData ImportBotShapeRemoteData()
        {
            if (!File.Exists(Application.dataPath + "/RemoteData/BotShapeEditorData.txt"))
                return new EditorBotShapeGeneratorData();

            var loaded = JsonConvert.DeserializeObject<EditorBotShapeGeneratorData>(File.ReadAllText(Application.dataPath + "/RemoteData/BotShapeEditorData.txt"));

            return loaded;
        }
    }
}


