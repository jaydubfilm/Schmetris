﻿using System;
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

        private Dictionary<Part, float> projectileTimers;

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
            magnetCount = 0;

            foreach (var part in _parts)
            {
                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);

                switch (part.Type)
                {
                    case PART_TYPE.MAGNET:
                    case PART_TYPE.CORE:
                        magnetCount += partData.data[part.level];
                        break;
                }
            }
        }

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

                var bitType = partRemoteData.burnRates[part.level].type;
                
                var resourceValue = GetValueToBurn(partRemoteData, bitType);

                if (resourceValue <= 0f && useBurnRate)
                {
                    return;
                }

                switch (part.Type)
                {
                    case PART_TYPE.CORE:

                        
                        resourceValue -= partRemoteData.burnRates[part.level].amount * Time.deltaTime;

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

                        if (!useBurnRate)
                            break;
                        
                        //TODO Determine if this heals Bits & parts or just parts
                        //TODO This needs to fire every x Seconds
                        var toRepair = bot.attachedBlocks.GetAttachablesAroundInRadius<Part>(part, part.level + 1)
                            .Where(p => p.CurrentHealth < p.StartingHealth)
                            .Select(x => new KeyValuePair<Part, float>(x, FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(x.Type).priority / (x.CurrentHealth / x.StartingHealth)))
                            .OrderByDescending(x => x.Value)
                            .FirstOrDefault().Key;

                        //If we weren't able to find a part, see if the repairer needs to be fixed
                        if (toRepair is null)
                        {
                            //If the repairer is also fine, then we can break out
                            if (part.CurrentHealth < part.StartingHealth)
                                toRepair = part;
                            else
                                break;
                        }

                        resourceValue -= partRemoteData.burnRates[part.level].amount * Time.deltaTime;

                        //Increase the health of this part depending on the current level of the repairer
                        toRepair.ChangeHealth(partRemoteData.data[part.level] * Time.deltaTime);

                        break;
                    case PART_TYPE.GUN:
                        //TODO Need to determine if the shoot type is looking for enemies or not
                        //--------------------------------------------------------------------------------------------//
                        if (projectileTimers == null)
                            projectileTimers = new Dictionary<Part, float>();

                        if (!projectileTimers.ContainsKey(part))
                            projectileTimers.Add(part, 0f);

                        //TODO This needs to fire every x Seconds
                        //--------------------------------------------------------------------------------------------//

                        if (projectileTimers[part] < partRemoteData.data[part.level] / damageGuess)
                        {
                            projectileTimers[part] += Time.deltaTime;
                            break;
                        }

                        projectileTimers[part] = 0f;

                        //Check if we have a target before removing resources
                        //--------------------------------------------------------------------------------------------//

                        Vector2 shootDirection;

                        var enemy = EnemyManager.GetClosestEnemy(transform.position, 10 * Constants.gridCellSize);
                        //TODO Determine if this fires at all times or just when there are active enemies in range
                        if (enemy == null)
                            break;

                        shootDirection = enemy.transform.position - transform.position;


                        //Use resources
                        //--------------------------------------------------------------------------------------------//

                        if (useBurnRate)
                        {
                            resourceValue -= partRemoteData.burnRates[part.level].amount;
                        }

                        Debug.Log("Fire");
                        
                        //Create projectile
                        //--------------------------------------------------------------------------------------------//

                        //TODO Might need to add something to change the projectile used for each gun piece
                        var projectile = FactoryManager.Instance.GetFactory<ProjectileFactory>()
                            .CreateObject<Projectile>(
                                "Basic Projectile",
                                shootDirection,
                                "Enemy");

                        projectile.transform.position = part.transform.position;

                        LevelManager.Instance.ProjectileManager.AddProjectile(projectile);

                        //--------------------------------------------------------------------------------------------//

                        break;
                }

                UpdateUI(bitType, resourceValue);
                PlayerPersistentData.PlayerData.liquidResource[bitType] = resourceValue;
            }
        }
        
        //============================================================================================================//

        private Bit GetFurthestBitToBurn(PartRemoteData remoteData, BIT_TYPE type)
        {
            if (!useBurnRate)
                return null;
            
            if (remoteData.burnRates.Length == 0)
                return null;
            
            return bot.attachedBlocks.OfType<Bit>()
                .Where(b => b.Type == type)
                .GetFurthestAttachable(Vector2Int.zero);
        }
        
        private float GetValueToBurn(PartRemoteData remoteData, BIT_TYPE type)
        {
            if (!useBurnRate)
                return default;

            var value = remoteData.burnRates.Length == 0
                ? default
                : PlayerPersistentData.PlayerData.liquidResource[type];

            if (value > 0)
                return value;
            
            var targetBit = GetFurthestBitToBurn(remoteData, type);
                    
            //If we're unable to find a bit to burn, then we can't use this part
            if (targetBit == null)
                return default;

            value = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetTotalResource(targetBit);
            bot.DestroyAttachable(targetBit);

            return value;
        }
        
        //============================================================================================================//

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