﻿using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Utilities.FileIO;
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

        //[SerializeField, Required, BoxGroup("Temporary")]
        //private BitProfileScriptableObject[] _bitProfileScriptableObjects;
        //[SerializeField, Required, BoxGroup("Temporary")]
        //private PartProfileScriptableObject[] _partProfileScriptableObjects;
        
        public EditorBotShapeGeneratorData EditorBotShapeData => _editorBotShapeData ?? (_editorBotShapeData = Files.ImportBotShapeRemoteData());
        private EditorBotShapeGeneratorData _editorBotShapeData;

        public List<SectorRemoteDataScriptableObject> SectorRemoteData => m_sectorRemoteData[currentModularDataIndex].SectorData;

        [SerializeField, Required, BoxGroup("Temporary")]
        public int currentModularDataIndex = 0;

        public int ModularDataCount => m_sectorRemoteData.Count;

        //============================================================================================================//
        public BitRemoteDataScriptableObject BitsRemoteData => bitRemoteData;
        public BitProfileScriptableObject BitProfileData => bitProfile as BitProfileScriptableObject;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private AttachableProfileScriptableObject bitProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private BitRemoteDataScriptableObject bitRemoteData;
        
        //============================================================================================================//
        
        public ComponentProfileScriptableObject ComponentProfile => componentProfile as ComponentProfileScriptableObject;
        
        [SerializeField, Required, BoxGroup("Attachables/Components")]
        private AttachableProfileScriptableObject componentProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Components")]
        public ComponentRemoteDataScriptableObject componentRemoteData;


        
        //============================================================================================================//

        public RemotePartProfileScriptableObject PartsRemoteData => partRemoteData;
        public PartProfileScriptableObject PartsProfileData => partProfile as PartProfileScriptableObject;

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
        public EnemyRemoteDataScriptableObject EnemyRemoteData => enemyRemoteData;

        [SerializeField, Required, BoxGroup("Projectiles")]
        private ProjectileProfileScriptableObject projectileProfile;
        public ProjectileProfileScriptableObject ProjectileProfile => projectileProfile;
        
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject botPrefab;
        /*[SerializeField, Required, BoxGroup("Bot")]
        private GameObject shieldPrototypePrefab;
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject alertIconPrefab;*/
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject scrapyardBotPrefab;
        [SerializeField, Required, BoxGroup("Puzzle Combos")]
        private ComboRemoteDataScriptableObject comboRemoteData;

        //Effects Properties
        //====================================================================================================================//

        [SerializeField, Required, BoxGroup("Effects") ]
        private EffectProfileScriptableObject effectProfileScriptableObject;
        public EffectProfileScriptableObject EffectProfileScriptableObject => effectProfileScriptableObject;
        
        /*//============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Damage")]
        private GameObject damageFactory;
        
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Particles")]
        private GameObject explosionPrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        private GameObject labelPrefab;

        [SerializeField, Required, BoxGroup("Particles")]
        private GameObject floatingTextPrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        private GameObject connectedSpritePrefab;
        [SerializeField, Required, BoxGroup("Particles")]
        private GameObject fadeSpritePrefab;
        
        [SerializeField, Required, BoxGroup("Particles")]
        private GameObject shrinkLinePrefab;*/

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Asteroid")]
        private AsteroidProfileScriptableObject asteroidProfile;

        [SerializeField, Required, BoxGroup("Asteroid")]
        private AsteroidRemoteDataScriptableObject asteroidRemote;

        [SerializeField, Required, BoxGroup("Asteroid")]
        private GameObject asteroidPrefab;

        //============================================================================================================//

        public FacilityRemoteDataScriptableObject FacilityRemote => facilityRemote;
        [SerializeField, Required, BoxGroup("Facilities")]
        private FacilityRemoteDataScriptableObject facilityRemote;
        

        //============================================================================================================//

        private Dictionary<Type, FactoryBase> _factoryBases;

        //============================================================================================================//

        /*private void Start()
        {
            ChangeBitProfile(0);
            ChangePartProfile(0);
        }

        public void ChangeBitProfile(int index)
        {
            bitProfile = _bitProfileScriptableObjects[index];
            var type = typeof(BitAttachableFactory);
            
            if (_factoryBases == null || !_factoryBases.ContainsKey(type))
                return;

            //Force update the BitFactory to use new sprite sheet
            _factoryBases[type] = CreateFactory<BitAttachableFactory>();
        }
        public void ChangePartProfile(int index)
        {
            partProfile = _partProfileScriptableObjects[index];
            var type = typeof(PartAttachableFactory);
            
            if (_factoryBases == null || !_factoryBases.ContainsKey(type))
                return;

            //Force update the BitFactory to use new sprite sheet
            _factoryBases[type] = CreateFactory<PartAttachableFactory>();
        }*/

        //====================================================================================================================//
        
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
                case bool _ when type == typeof(AsteroidFactory):
                    return new AsteroidFactory(asteroidPrefab, asteroidProfile, asteroidRemote) as T;
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
                    return new BotFactory(botPrefab, scrapyardBotPrefab) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(EffectFactory):
                    return new EffectFactory(EffectProfileScriptableObject) as T;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type.Name, null);
                //----------------------------------------------------------------------------------------------------//
            }
        }

        //============================================================================================================//

    }
}


