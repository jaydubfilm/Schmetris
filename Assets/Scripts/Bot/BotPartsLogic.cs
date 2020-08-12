﻿using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
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

        public GameObject shieldPrefab;

        //==============================================================================================================//

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
        
        //==============================================================================================================//


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

        private Dictionary<Part, float> _projectileTimers;
        
        //Shield Prototype Functionality
        //============================================================================================================//

        private Dictionary<Part, ShieldData> _shields;

        private class ShieldData
        {
            public readonly float waitTime;
            public int radius;
            
            public float currentHp;
            public float timer;

            public Shield shield;

            public ShieldData(float waitTime)
            {
                this.waitTime = waitTime;
            }
        }

        //==============================================================================================================//

        private void OnEnable()
        {
            PlayerPersistentData.PlayerData.OnValuesChanged += ForceUpdateResourceUI;
        }

        private void OnDisable()
        {
            PlayerPersistentData.PlayerData.OnValuesChanged -= ForceUpdateResourceUI;
        }


        //==============================================================================================================//


        public void SetMagentOverride(int magnet)
        {
            magnetOverride = magnet;
            magnetCount = magnetOverride;
        }
        
        //==============================================================================================================//


        public void AddCoreHeat(float amount)
        {
            coreHeat += amount;
            coolTimer = coolDelay;
        }
        
        //==============================================================================================================//

        public float TryHitShield(Vector2Int coordinate, float damage)
        {
            //If no shields exist, don't attempt to sort damage distribution
            if (_shields == null || _shields.Count == 0)
                return damage;

            var shieldHitParts = new List<Part>();
            
            //Search through our active shields to determine if any were hit 
            foreach (var shieldData in _shields)
            {
                var rad = shieldData.Value.radius;
                var part = shieldData.Key;
                var direction = coordinate - part.Coordinate;

                if (Mathf.Abs(direction.x) > rad || Mathf.Abs(direction.y) > rad)
                    continue;
                
                shieldHitParts.Add(part);
            }

            //If we see that 0 were hit, return our full damage amount
            if (shieldHitParts.Count <= 0)
                return damage;

            var outDamage = 0f;
            
            //FIXME I feel as if there is a better way of tackling this problem, as I don't like the back and forth calculations
            var dividedDamage = damage / shieldHitParts.Count;
            foreach (var hitPart in shieldHitParts)
            {
                _shields[hitPart].currentHp -= dividedDamage;
                _shields[hitPart].timer = 0f;
                
                //Check to see if the shield still has health
                if (_shields[hitPart].currentHp >= 0f)
                    continue;

                //If the damage added goes below 0, push it back to outDamage value to be returned
                outDamage += Mathf.Abs(_shields[hitPart].currentHp);
                _shields[hitPart].currentHp = 0f;
            }

            return outDamage;
        }
        
        //==============================================================================================================//


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
            }

            CheckForRecycledParts();
            
            
            magnetCount = 0;

            foreach (var part in _parts)
            {
                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);
                
                float value;
                switch (part.Type)
                {
                    case PART_TYPE.MAGNET:
                    case PART_TYPE.CORE:
                        if (magnetOverride > 0)
                            break;
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
                        {
                            magnetCount += (int)value;
                        }
                        break;
                    //Determine if we need to setup the shield elements for the bot
                    //FIXME I'll need a way of disposing of the shield visual object
                    case PART_TYPE.SHIELD:
                        if (_shields == null)
                            _shields = new Dictionary<Part, ShieldData>();

                        if (_shields.ContainsKey(part))
                            break;

                        //TODO Need to add the use of the recycler
                        var shield = FactoryManager.Instance.GetFactory<BotFactory>().CreateShield();

                        //if (!Recycler.TryGrab<Shield>(out Shield shield))
                        //{
                        //    shield = Instantiate(shieldPrefab).GetComponent<Shield>();
                        //}

                        shield.transform.SetParent(part.transform);
                        shield.transform.localPosition = Vector3.zero;
                        
                        
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Radius, out value))
                        {
                            shield.SetSize((int)value);
                        }
                        
                        shield.SetAlpha(0.5f);
                        _shields.Add(part, new ShieldData(4f)
                        {
                            shield = shield,
                            
                            currentHp = 25,
                            radius = (int)value,
                            
                            timer = 0f
                        });
                        
                        break;
                }
            }
        }

        //============================================================================================================//
        
        /// <summary>
        /// Parts specific update Loop. Updates all part information based on currently attached parts.
        /// </summary>
        public void PartsUpdateLoop()
        {
            //Be careful to not use return here
            foreach (var part in _parts)
            {
                PartRemoteData partRemoteData =
                    FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type);

                var levelData = partRemoteData.levels[part.level];

                //If there's nothing using these resources ignore
                if(levelData.burnRate == 0f)
                    continue;

                var resourceValue = GetValueToBurn(levelData, partRemoteData.burnType);
                
                //If we no longer have liquid to use, find a bit that could be refined
                if (resourceValue <= 0f && useBurnRate)
                {
                    var targetBit = GetFurthestBitToBurn(levelData, partRemoteData.burnType);

                    if (targetBit == null)
                    {
                        //TODO Need to show a no resource icon over par
                    }
                    else
                    {
                        var addAmount = FactoryManager.Instance
                            .GetFactory<BitAttachableFactory>().GetBitRemoteData(targetBit.Type).levels[targetBit.level]
                            .resources;
                    
                        PlayerPersistentData.PlayerData.AddLiquidResource(partRemoteData.burnType, addAmount);

                        bot.DestroyAttachable<Bit>(targetBit);

                        resourceValue = addAmount;
                    }
                    
                }

                switch (part.Type)
                {
                    case PART_TYPE.CORE:

                        if (resourceValue > 0f && useBurnRate)
                            resourceValue -= levelData.burnRate * Time.deltaTime;

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
                            break;

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
                                break;
                                
                        }

                        if (useBurnRate)
                        {
                            if(resourceValue > 0f)
                                resourceValue -= levelData.burnRate * Time.deltaTime;
                        }

                        var repairAmount = levelData.GetDataValue(DataTest.TEST_KEYS.Heal);
                        
                        //Increase the health of this part depending on the current level of the repairer
                        toRepair.ChangeHealth(repairAmount * Time.deltaTime);

                        break;
                    case PART_TYPE.GUN:

                        if (resourceValue <= 0f && useBurnRate)
                            break;
                        
                        //TODO Need to determine if the shoot type is looking for enemies or not
                        //--------------------------------------------------------------------------------------------//
                        if (_projectileTimers == null)
                            _projectileTimers = new Dictionary<Part, float>();

                        if (!_projectileTimers.ContainsKey(part))
                            _projectileTimers.Add(part, 0f);

                        //TODO This needs to fire every x Seconds
                        //--------------------------------------------------------------------------------------------//

                        var cooldown = levelData.GetDataValue(DataTest.TEST_KEYS.Cooldown);
                        
                        if (_projectileTimers[part] < cooldown)
                        {
                            _projectileTimers[part] += Time.deltaTime;
                            break;
                        }

                        _projectileTimers[part] = 0f;

                        //Check if we have a target before removing resources
                        //--------------------------------------------------------------------------------------------//

                        var enemy = EnemyManager.GetClosestEnemy(transform.position, 100 * Constants.gridCellSize);
                        //TODO Determine if this fires at all times or just when there are active enemies in range
                        if (enemy == null)
                            break;


                        //Use resources
                        //--------------------------------------------------------------------------------------------//

                        if (useBurnRate)
                        {
                            if(resourceValue > 0)
                                resourceValue -= levelData.burnRate;
                        }

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
                    case PART_TYPE.SHIELD:

                        const float fakeHealth = 25f;
                        var data = _shields[part];
                        var shield = data.shield;
                        //shield.transform.position = part.transform.position;
                        
                        if (resourceValue <= 0f && useBurnRate && data.currentHp <= 0)
                        {
                            shield.SetAlpha(0f);
                            break;
                        }

                        if (data.currentHp < fakeHealth)
                        {
                            //TODO Shield has countdown before it can begin recharging
                            if (data.timer >= data.waitTime)
                            {
                                //TODO Shield only use resources when recharging
                                resourceValue -= levelData.burnRate * Time.deltaTime;
                                _shields[part].currentHp += levelData.burnRate * Time.deltaTime;
                            }
                            else
                            {
                                _shields[part].timer += Time.deltaTime;
                            }
                        }
                        

                        shield.SetAlpha(0.5f * (data.currentHp / fakeHealth));
                        
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
        
        //============================================================================================================//

        private void CheckForRecycledParts()
        {
            if (_shields == null || _shields.Count == 0)
                return;
            
            var copy = new Dictionary<Part, ShieldData>(_shields);
            foreach (var data in copy.Where(data => data.Key.IsRecycled))
            {
                Recycler.Recycle<Shield>(data.Value.shield.gameObject);
                _shields.Remove(data.Key);
            }
        }
        
        public void ClearList()
        {
            _parts.Clear();
        }
        
        //============================================================================================================//

    }
}