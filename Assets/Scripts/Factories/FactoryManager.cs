using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities;
using StarSalvager.Utilities.FileIO;
using UnityEngine;

namespace StarSalvager.Factories
{
    
    //Based on: https://www.dofactory.com/net/factory-method-design-pattern
    public class FactoryManager : Singleton<FactoryManager>
    {
        /*[SerializeField, Required, BoxGroup("Temporary")]
        private List<SectorModularData> m_sectorRemoteData;*/
        
        public EditorBotShapeGeneratorData EditorBotShapeData => _editorBotShapeData ?? (_editorBotShapeData = Files.ImportBotShapeRemoteData());
        private EditorBotShapeGeneratorData _editorBotShapeData;

        public RingRemoteDataScriptableObject[] RingRemoteDatas => _ringRemoteDatas;
        [SerializeField, Required]
        private RingRemoteDataScriptableObject[] _ringRemoteDatas;
        
        /*public List<SectorRemoteDataScriptableObject> SectorRemoteData => m_sectorRemoteData[currentModularDataIndex].SectorData;*/

        [SerializeField, Required, BoxGroup("Temporary")]
        public int currentModularDataIndex = 0;

        /*public int ModularDataCount => m_sectorRemoteData.Count;*/


        public Sprite PatchSprite;

        //====================================================================================================================//
        public DamageProfileScriptableObject DamageProfile => damageProfile;
        
        [SerializeField, Required, BoxGroup("Attachables")]
        private DamageProfileScriptableObject damageProfile;

        //============================================================================================================//
        public BitRemoteDataScriptableObject BitsRemoteData => bitRemoteData;
        public BitProfileScriptableObject BitProfileData => bitProfile as BitProfileScriptableObject;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private AttachableProfileScriptableObject bitProfile;
        
        [SerializeField, Required, BoxGroup("Attachables/Bits")]
        private BitRemoteDataScriptableObject bitRemoteData;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Attachables/Components")]
        private GameObject componentPrefab;

        [SerializeField, Required, BoxGroup("Attachables/Components")]
        public ComponentRemoteDataScriptableObject componentRemoteData;

        [SerializeField, Required, BoxGroup("Attachables/Components")]
        public Sprite componentSprite;

        
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
        
        [SerializeField, Required, BoxGroup("Patches")]
        private PatchRemoteDataScriptableObject patchRemoteData;
        public PatchRemoteDataScriptableObject PatchRemoteData => patchRemoteData;
        
        //============================================================================================================//
        
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject botPrefab;
        /*[SerializeField, Required, BoxGroup("Bot")]
        private GameObject shieldPrototypePrefab;
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject alertIconPrefab;*/
        [SerializeField, Required, BoxGroup("Bot")]
        private GameObject scrapyardBotPrefab;
        
        [SerializeField, Required, BoxGroup("Bot")]
        private Sabre sabrePrefab;


        public ComboRemoteDataScriptableObject ComboRemoteData => comboRemoteData;
        
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

        [SerializeField, Required, BoxGroup("Space Junk")]
        private SpaceJunkRemoteDataScriptableObject spaceJunkRemote;

        [SerializeField, Required, BoxGroup("Space Junk")]
        private GameObject spaceJunkPrefab;

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Crate")]
        private CrateRemoteDataScriptableObject crateRemote;

        [SerializeField, Required, BoxGroup("Crate")]
        private GameObject cratePrefab;

        //============================================================================================================//

        public MineRemoteDataScriptableObject MineRemoteData => mineRemote;
        
        [SerializeField, Required, BoxGroup("Mine")]
        private MineRemoteDataScriptableObject mineRemote;

        /*[SerializeField, Required, BoxGroup("Mine")]
        private GameObject minePrefab;*/

        //============================================================================================================//

        [SerializeField, Required, BoxGroup("Black Hole")]
        private BlackHoleRemoteDataScriptableObject blackHoleRemote;

        [SerializeField, Required, BoxGroup("Black Hole")]
        private GameObject blackHolePrefab;        

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
                case bool _ when type == typeof(ComponentFactory):
                    return new ComponentFactory(componentPrefab, componentRemoteData) as T;
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
                    return new BotFactory(botPrefab, scrapyardBotPrefab, sabrePrefab) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(EffectFactory):
                    return new EffectFactory(EffectProfileScriptableObject) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(SpaceJunkFactory):
                    return new SpaceJunkFactory(spaceJunkPrefab, spaceJunkRemote) as T;
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(CrateFactory):
                    return new CrateFactory(cratePrefab, crateRemote) as T;
                //----------------------------------------------------------------------------------------------------//
                /*case bool _ when type == typeof(MineFactory):
                    return new MineFactory(minePrefab, mineRemote) as T;*/
                //----------------------------------------------------------------------------------------------------//
                case bool _ when type == typeof(BlackHoleFactory):
                    return new BlackHoleFactory(blackHolePrefab, blackHoleRemote) as T;
                //----------------------------------------------------------------------------------------------------//
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type.Name, null);
                //----------------------------------------------------------------------------------------------------//
            }
        }

        //============================================================================================================//

    }
}


