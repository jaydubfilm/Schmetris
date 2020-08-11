using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using UnityEngine;
using Enemy = StarSalvager.AI.Enemy;
using GameUI = StarSalvager.UI.GameUI;

namespace StarSalvager
{
    [RequireComponent(typeof(Bot))]
    public class BotPartsLogic : MonoBehaviour
    {
        public Bot bot;

        //================================================================================================================//

        private List<Part> _parts;

        //FIXME This needs to something more manageable
        private EnemyManager EnemyManager
        {
            get
            {
                if (_EnemyManager == null)
                    _EnemyManager = FindObjectOfType<EnemyManager>();
                return _EnemyManager;
            }   
        }
        private EnemyManager _EnemyManager;

        private GameUI GameUI
        {
            get
            {
                if (_GameUI == null)
                    _GameUI = FindObjectOfType<GameUI>();
                
                return _GameUI;
            }
        }
        private GameUI _GameUI;

        #region Parts

        [SerializeField, BoxGroup("Magnets")] public bool useMagnet = true;
        [SerializeField, BoxGroup("Magnets")] public MAGNET currentMagnet = MAGNET.DEFAULT;

        [SerializeField, BoxGroup("BurnRates")]
        private bool useBurnRate = true;

        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        public float coreHeat;

        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("/s", Overlay = true)]
        private float coolSpeed;

        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("s", Overlay = true)]
        private float coolDelay;

        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        private float coolTimer;

        [ShowInInspector, BoxGroup("Bot Part Data"), ReadOnly]
        public int magnetCount { get; private set; }

        private int magnetOverride;

        private Dictionary<Part, float> projectileTimers;

        //================================================================================================================//

        private void OnEnable()
        {
            PlayerPersistentData.PlayerData.OnValuesChanged += ForceUpdateResourceUI;
        }

        private void OnDisable()
        {
            PlayerPersistentData.PlayerData.OnValuesChanged -= ForceUpdateResourceUI;
        }


        //================================================================================================================//


        public void SetMagentOverride(int magnet)
        {
            magnetOverride = magnet;
            magnetCount = magnetOverride;
        }
        
        //================================================================================================================//


        public void AddCoreHeat(float amount)
        {
            coreHeat += amount;
            coolTimer = coolDelay;
        }

        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        public void UpdatePartsList()
        {
            _parts = bot.attachedBlocks.OfType<Part>().ToList();

            UpdatePartData();
        }

        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void UpdatePartData()
        {
            if (magnetOverride > 0)
            {
                magnetCount = magnetOverride;
                return;
            }
            
            magnetCount = 0;

            foreach (var part in _parts)
            {
                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);
                
                magnetCount += (int)partData.levels[part.level].GetDataValue(DataTest.TEST_KEYS.Magnet);

                /*switch (part.Type)
                {
                    case PART_TYPE.MAGNET:
                    case PART_TYPE.CORE:
                        //.data[part.level];
                        break;
                }*/
            }
        }

        //private Dictionary<Part, Bit> _burnRef = new Dictionary<Part, Bit>();

        
        /// <summary>
        /// Parts specific update Loop. Updates all part information based on currently attached parts.
        /// </summary>
        public void PartsUpdateLoop()
        {
            const float damageGuess = 5f;

            //Be careful to not use return here
            foreach (var part in _parts)
            {
                PartRemoteData partRemoteData =
                    FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type);

                var levelData = partRemoteData.levels[part.level];

                //If there's nothing using these resources ignore
                if(levelData.burnRate == 0f)
                    continue;

                //var data = partRemoteData.levels[part.level].data;
                //var burnAmount = partRemoteData.levels[part.level].burnRate.amount;
                //var bitType = partRemoteData.levels[part.level].burnRate.type;
                
                var resourceValue = GetValueToBurn(levelData, partRemoteData.burnType);
                
                //If we no longer have liquid to use, find a bit that could be refined
                if (resourceValue <= 0f && useBurnRate)
                {
                    var targetBit = GetFurthestBitToBurn(levelData, partRemoteData.burnType);
                    
                    if (targetBit == null)
                        continue;

                    var addAmount = FactoryManager.Instance
                        .GetFactory<BitAttachableFactory>().GetBitRemoteData(targetBit.Type).levels[targetBit.level]
                        .resources;
                    
                    PlayerPersistentData.PlayerData.AddLiquidResource(partRemoteData.burnType, addAmount);

                    bot.DestroyAttachable(targetBit);

                    resourceValue = addAmount;

                    /*if (!_burnRef.ContainsKey(part))
                    {
                        var targetBit = GetFurthestBitToBurn(levelData, partRemoteData.burnType);
            
                    
                        //If we're unable to find a bit to burn, then we can't use this part
                        if (targetBit == null)
                            continue;
                
                        _burnRef.Add(part, targetBit);
                    }
                    else if (_burnRef[part] == null || _burnRef[part].IsRecycled)
                    {
                        var targetBit = GetFurthestBitToBurn(levelData, partRemoteData.burnType);
            
                    
                        //If we're unable to find a bit to burn, then we can't use this part
                        if (targetBit == null)
                            continue;

                        _burnRef[part] = targetBit;
                    }*/
                }

                switch (part.Type)
                {
                    case PART_TYPE.CORE:

                        if (resourceValue > 0f && useBurnRate)
                            resourceValue -= levelData.burnRate * Time.deltaTime;
                        /*else if (_burnRef[part] && useBurnRate)
                        {
                            _burnRef[part].ChangeHealth(-levelData.burnRate * Time.deltaTime);
                        }*/

                        //TODO Need to check on Heating values for the core
                        if (coreHeat <= 0)
                        {
                            GameUI.SetHeatSliderValue(0f);
                            break;
                        }

                        GameUI.SetHeatSliderValue(coreHeat / 100f);


                        part.SetColor(Color.Lerp(Color.white, Color.red, coreHeat / 100f));

                        if (coolTimer > 0f)
                        {
                            coolTimer -= Time.deltaTime;
                            break;
                        }
                        else
                            coolTimer = 0;

                        coreHeat -= coolSpeed * Time.deltaTime;

                        if (coreHeat < 0)
                        {
                            coreHeat = 0;
                            part.SetColor(Color.white);
                        }

                        break;
                    case PART_TYPE.REPAIR:

                        
                        
                        
                        if (resourceValue <= 0f && useBurnRate)
                        {
                            /*if (!_burnRef[part] && useBurnRate)
                            {
                                
                                continue;
                            }*/
                            continue;
                        }

                        IHealth toRepair;
                        
                        var radius = (int)levelData.GetDataValue(DataTest.TEST_KEYS.Radius);
                        
                        //FIXME I don't think using linq here, especially twice is the best option
                        //TODO This needs to fire every x Seconds
                        toRepair = bot.attachedBlocks.GetAttachablesAroundInRadius<Part>(part, radius)
                            .Where(p => p.CurrentHealth < p.StartingHealth)
                            .Select(x => new KeyValuePair<Part, float>(x, FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(x.Type).priority / (x.CurrentHealth / x.StartingHealth)))
                            .OrderByDescending(x => x.Value)
                            .FirstOrDefault().Key;

                        //If we weren't able to find a part, see if the repairer needs to be fixed
                        if (toRepair is null)
                        {
                            //TODO Need to determine if this is already happening
                            //If the repairer is also fine, then we can break out
                            if (part.CurrentHealth < part.StartingHealth)
                                toRepair = part;
                            else
                            {
                                //Trying out the parts only repair system
                                /*//Find Bits to repair if any need it
                                toRepair = bot.attachedBlocks
                                    .GetAttachablesAroundInRadius<IAttachable>(part, part.level + 1)
                                    .Where(x => !(x is Part) && !(x is Enemy))
                                    .OfType<IHealth>()
                                    .FirstOrDefault(p => p.CurrentHealth < p.StartingHealth);

                                if (toRepair == null)*/
                                    break;
                            }
                                
                        }

                        if (useBurnRate)
                        {
                            if(resourceValue > 0f)
                                resourceValue -= levelData.burnRate * Time.deltaTime;
                            /*else if(_burnRef[part])
                                _burnRef[part].ChangeHealth(-levelData.burnRate * Time.deltaTime);*/
                        }

                        var repairAmount = levelData.GetDataValue(DataTest.TEST_KEYS.Heal);
                        
                        //Increase the health of this part depending on the current level of the repairer
                        toRepair.ChangeHealth(repairAmount * Time.deltaTime);

                        break;
                    case PART_TYPE.GUN:
                        
                        if (resourceValue <= 0f && useBurnRate)
                        {
                            /*if (!_burnRef[part] && useBurnRate)
                            {
                                continue;
                            }*/
                            continue;
                            
                        }
                        
                        //TODO Need to determine if the shoot type is looking for enemies or not
                        //--------------------------------------------------------------------------------------------//
                        if (projectileTimers == null)
                            projectileTimers = new Dictionary<Part, float>();

                        if (!projectileTimers.ContainsKey(part))
                            projectileTimers.Add(part, 0f);

                        //TODO This needs to fire every x Seconds
                        //--------------------------------------------------------------------------------------------//

                        var cooldown = levelData.GetDataValue(DataTest.TEST_KEYS.Cooldown);
                        
                        if (projectileTimers[part] < cooldown)
                        {
                            projectileTimers[part] += Time.deltaTime;
                            break;
                        }

                        projectileTimers[part] = 0f;

                        //Check if we have a target before removing resources
                        //--------------------------------------------------------------------------------------------//

                        /*Vector2 shootDirection;*/

                        var enemy = EnemyManager.GetClosestEnemy(transform.position, 100 * Constants.gridCellSize);
                        //TODO Determine if this fires at all times or just when there are active enemies in range
                        if (enemy == null)
                            break;

                        /*shootDirection = enemy.transform.position - transform.position;*/


                        //Use resources
                        //--------------------------------------------------------------------------------------------//

                        if (useBurnRate)
                        {
                            if(resourceValue > 0)
                                resourceValue -= levelData.burnRate;
                            /*else if(_burnRef[part])
                                _burnRef[part].ChangeHealth(-levelData.burnRate * Time.deltaTime);*/
                        }

                        //Debug.Log("Fire");
                        
                        //Create projectile
                        //--------------------------------------------------------------------------------------------//

                        const string PROJECTILE_ID = "083be790-7a08-4f27-b506-e8e09a116bc8";
                        
                        //TODO Might need to add something to change the projectile used for each gun piece
                        var projectile = FactoryManager.Instance.GetFactory<ProjectileFactory>()
                            .CreateObject<Projectile>(
                                PROJECTILE_ID,
                                /*shootDirection*/Vector2.up,
                                "Enemy");

                        projectile.transform.position = part.transform.position;

                        LevelManager.Instance.ProjectileManager.AddProjectile(projectile);

                        //--------------------------------------------------------------------------------------------//

                        break;
                }

                UpdateUI(partRemoteData.burnType, resourceValue);
                PlayerPersistentData.PlayerData.liquidResource[partRemoteData.burnType] = resourceValue;
            }
        }
        
        //============================================================================================================//

        private Bit GetFurthestBitToBurn(PartLevelData partLevelData, BIT_TYPE type)
        {
            if (!useBurnRate)
                return null;
            
            if (partLevelData.burnRate == 0f)
                return null;
            
            return bot.attachedBlocks.OfType<Bit>()
                .Where(b => b.Type == type)
                .GetFurthestAttachable(Vector2Int.zero);
        }

        
        private float GetValueToBurn(PartLevelData partLevelData, BIT_TYPE type)
        {
            if (!useBurnRate)
                return default;

            var value = partLevelData.burnRate == 0
                ? default
                : PlayerPersistentData.PlayerData.liquidResource[type];

            return  value;


            /*if (!_burnRef.ContainsKey(part))
            {
                var targetBit = GetFurthestBitToBurn(partLevelData, type);
            
                    
                //If we're unable to find a bit to burn, then we can't use this part
                if (targetBit == null)
                    return default;
                
                _burnRef.Add(part, targetBit);
            }
            
            
            
           

            value = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetTotalResource(targetBit);
            bot.DestroyAttachable(targetBit);

            return value;*/
        }
        
        //============================================================================================================//

        private void ForceUpdateResourceUI()
        {
            foreach (var f in PlayerPersistentData.PlayerData.liquidResource)
            {
                UpdateUI(f.Key, f.Value);
            }
        }

        private void UpdateUI(BIT_TYPE type, float value)
        {
            switch (type)
            {
                case BIT_TYPE.BLUE:
                    GameUI.SetWaterValue(value);
                    break;
                case BIT_TYPE.GREEN:
                    GameUI.SetRepairValue(value);
                    break;
                case BIT_TYPE.GREY:
                    GameUI.SetAmmoValue(value);
                    break;
                case BIT_TYPE.RED:
                    GameUI.SetFuelValue(value);
                    break;
                case BIT_TYPE.YELLOW:
                    GameUI.SetPowerValue(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        #endregion //Parts

        public void ClearList()
        {
            _parts.Clear();
        }
    }
}