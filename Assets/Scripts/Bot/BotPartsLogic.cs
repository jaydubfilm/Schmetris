using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Values;
using UnityEngine;
using GameUI = StarSalvager.UI.GameUI;

namespace StarSalvager
{
    //FIXME This will need to be reorganized badly
    [RequireComponent(typeof(Bot))]
    public class BotPartsLogic : MonoBehaviour
    {
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
        
        public Bot bot;

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

        //temp variables
        float batteryDrainTimer = 0;
        float waterDrainTimer = 0;

        //==============================================================================================================//
        
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
        
        //==============================================================================================================//


        private Dictionary<Part, float> _projectileTimers;
        private Dictionary<Part, ShieldData> _shields;
        private Dictionary<Part, FlashSprite> _flashes;
        private Dictionary<Part, float> _bombTimers;
        
        //Unity Functions
        //==============================================================================================================//

        private void OnEnable()
        {
            PlayerData.OnValuesChanged += ForceUpdateResourceUI;
        }

        private void OnDisable()
        {
            PlayerData.OnValuesChanged -= ForceUpdateResourceUI;
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

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void UpdatePartData()
        {
            if (magnetOverride > 0)
            {
                magnetCount = magnetOverride;
            }
            
            PlayerPersistentData.PlayerData.ClearLiquidCapacity();
            var capacities = new Dictionary<BIT_TYPE, int>
            {
                {BIT_TYPE.RED, 0},
                {BIT_TYPE.BLUE, 0},
                {BIT_TYPE.YELLOW, 0},
                {BIT_TYPE.GREEN, 0},
                {BIT_TYPE.GREY, 0},
            };

            CheckIfShieldShouldRecycle();
            CheckIfFlashIconShouldRecycle();
            CheckIfBombsShouldRecycle();
            GameUI?.ShowBombIcon(false);
            
            
            magnetCount = 0;

            foreach (var part in _parts)
            {
                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);
                
                int value;
                switch (part.Type)
                {
                    case PART_TYPE.CORE:
                        
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.RED] += value;
                            capacities[BIT_TYPE.GREEN] += value;
                            capacities[BIT_TYPE.GREY] += value;
                            capacities[BIT_TYPE.YELLOW] += value;
                            capacities[BIT_TYPE.BLUE] += value;
                        }
                        
                        if (magnetOverride > 0)
                            break;
                        
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
                        {
                            magnetCount += value;
                        }
                        break;
                    case PART_TYPE.MAGNET:
                    
                        if (magnetOverride > 0)
                            break;
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
                        {
                            magnetCount += value;
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
                            shield.SetSize(value);
                        }
                        
                        shield.SetAlpha(0.5f);
                        _shields.Add(part, new ShieldData(4f)
                        {
                            shield = shield,
                            
                            currentHp = 25,
                            radius = value,
                            
                            timer = 0f
                        });
                        
                        break;
                    case PART_TYPE.STORE:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.RED] += value;
                            capacities[BIT_TYPE.GREEN] += value;
                            capacities[BIT_TYPE.GREY] += value;
                        }
                        break;
                    case PART_TYPE.STORE_RED:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.RED] += value;
                        }
                        break;
                    case PART_TYPE.STORE_GREEN:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.GREEN] += value;
                        }
                        break;
                    case PART_TYPE.STORE_GREY:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.GREY] += value;
                        }
                        break;
                    case PART_TYPE.BOMB:
                        if(_bombTimers == null)
                            _bombTimers = new Dictionary<Part, float>();

                        if (_bombTimers.ContainsKey(part))
                            break;
                        
                        GameUI.ShowBombIcon(true);
                        _bombTimers.Add(part, 0f);
                        break;
                }
            }

            //Force update capacities, once new values determined
            PlayerPersistentData.PlayerData.SetCapacities(capacities);
        }

        //============================================================================================================//
        
        //FIXME I Will want to separate these functions as this is getting too large
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
                        //FIXME I don't like how often this is called, will need to rethink this
                        //Display the icon for this part if we have no more resources
                        GetAlertIcon(part).SetActive(true);
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
                else
                {
                    //FIXME I don't like how often this is called, will need to rethink this
                    //Hide the icon for part if it exists
                    if(_flashes != null && _flashes.ContainsKey(part))
                        GetAlertIcon(part).SetActive(false);
                }

                switch (part.Type)
                {
                    case PART_TYPE.CORE:

                        if (resourceValue > 0f && useBurnRate)
                            resourceValue -= levelData.burnRate * Time.deltaTime;

                        //Determines if the player can move with no available fuel
                        InputManager.Instance.LockSideMovement = resourceValue <= 0f;

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
                        
                        var radius = levelData.GetDataValue<int>(DataTest.TEST_KEYS.Radius);
                        
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

                        var repairAmount = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Heal);
                        
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

                        var cooldown = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Cooldown);
                        
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

                        //const string PROJECTILE_ID = "083be790-7a08-4f27-b506-e8e09a116bc8";
                        
                        var projectileId = levelData.GetDataValue<string>(DataTest.TEST_KEYS.Projectile);
                        
                        //TODO Might need to add something to change the projectile used for each gun piece
                        var projectile = FactoryManager.Instance.GetFactory<ProjectileFactory>()
                            .CreateObject<Projectile>(
                                projectileId,
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
                    case PART_TYPE.BOMB:

                        //TODO This still needs to account for multiple bombs
                        if (!_bombTimers.TryGetValue(part, out var timer))
                            break;

                        //FIXME I don't like that this is getting called so often
                        var hasAmmo = PlayerPersistentData.PlayerData.liquidResource[partRemoteData.burnType] >= levelData.burnRate;
                        GameUI.SetHasBombResource(hasAmmo);
                        //GetAlertIcon(part).SetActive(!hasAmmo);

                        if (timer <= 0f)
                            break;

                        levelData.TryGetValue(DataTest.TEST_KEYS.Cooldown, out cooldown);
                            
                        _bombTimers[part] -= Time.deltaTime;
                        GameUI.SetBombFill(1f - _bombTimers[part] / cooldown);
                        
                        break;
                }

                UpdateUI(partRemoteData.burnType, resourceValue);
                PlayerPersistentData.PlayerData.SetLiquidResource(partRemoteData.burnType, resourceValue);
            }

            batteryDrainTimer += Time.deltaTime / 2;
            waterDrainTimer += Time.deltaTime / 4;

            if (batteryDrainTimer >= 1 && PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW] > 0)
            {
                batteryDrainTimer--;
                PlayerPersistentData.PlayerData.SetResources(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW] - 1);
            }
            if (waterDrainTimer >= 1 && PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] > 0)
            {
                waterDrainTimer--;
                PlayerPersistentData.PlayerData.SetResources(BIT_TYPE.BLUE, PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] - 1);
            }
            UpdateUI(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW]);
            UpdateUI(BIT_TYPE.BLUE, PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE]);
        }
        
        //============================================================================================================//

        #region Bomb

        public void TryTriggerBomb()
        {
            if (_bombTimers == null || _bombTimers.Count == 0)
                return;

            var part = _bombTimers.FirstOrDefault(x => x.Value <= 0f).Key;


            if (part == null)
                return;
            
            var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);
            var partLevelData = partData.levels[part.level];

            var burnType = partData.burnType;
            var useCost = partLevelData.burnRate;

            
            if (PlayerPersistentData.PlayerData.liquidResource[burnType] < useCost)
                return;
            
            //Remove the resources here
            PlayerPersistentData.PlayerData.SubtractLiquidResource(burnType, useCost);
            
            //Set the cooldown time
            if (partLevelData.TryGetValue(DataTest.TEST_KEYS.Cooldown, out float cooldown))
            {
                _bombTimers[part] = cooldown;
            }
            
            //Damage all the enemies
            if (partLevelData.TryGetValue(DataTest.TEST_KEYS.Damage, out float damage))
            {
                EnemyManager.DamageAllEnemies(damage);
            }
        }
        
        #endregion
        
        //============================================================================================================//

        #region Shield
        
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
        
        #endregion //Shield
        
        //============================================================================================================//

        public void SetMagnetOverride(int magnet)
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

        //Updating UI
        //============================================================================================================//

        private void ForceUpdateResourceUI()
        {
            foreach (var f in PlayerPersistentData.PlayerData.liquidResource)
            {
                UpdateUI(f.Key, f.Value);
            }
            
            UpdateUI(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW]);
            UpdateUI(BIT_TYPE.BLUE, PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE]);
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

        //Getting Alert Icons/Flash Sprites
        //============================================================================================================//

        private FlashSprite GetAlertIcon(Part part)
        {
            if(_flashes == null)
                _flashes = new Dictionary<Part, FlashSprite>();

            if (_flashes.ContainsKey(part))
                return _flashes[part];


            var flash = FactoryManager.Instance.GetFactory<BotFactory>().CreateAlertIcon();//Instantiate(flashSpritePrefab).GetComponent<FlashSprite>();
            flash.transform.SetParent(part.transform, false);
            flash.transform.localPosition = Vector3.zero;


            var burnType = FactoryManager.Instance.PartsRemoteData.GetRemoteData(part.Type).burnType;
            var bitColor = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitProfile(burnType).color;
            
            flash.SetColor(bitColor);
            
            _flashes.Add(part, flash);

            return _flashes[part];
        }
        
        //Find Bits/Values to burn
        //============================================================================================================//

        private Bit GetFurthestBitToBurn(PartLevelData partLevelData, BIT_TYPE type)
        {
            if (!useBurnRate)
                return null;
            
            if (partLevelData.burnRate == 0f)
                return null;

            return GetFurthestBitToBurn(type);
        }

        private Bit GetFurthestBitToBurn(BIT_TYPE type)
        {
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
        
        //Checking for recycled extras
        //============================================================================================================//

        private void CheckIfShieldShouldRecycle()
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
        
        private void CheckIfFlashIconShouldRecycle()
        {
            if (_flashes == null || _flashes.Count == 0)
                return;
            
            var copy = new Dictionary<Part, FlashSprite>(_flashes);
            foreach (var data in copy.Where(data => data.Key.IsRecycled))
            {
                Recycler.Recycle<FlashSprite>(data.Value.gameObject);
                _flashes.Remove(data.Key);
            }
        }

        private void CheckIfBombsShouldRecycle()
        {
            if (_bombTimers == null || _bombTimers.Count == 0)
                return;
            
            var copy = new Dictionary<Part, float>(_bombTimers);
            foreach (var data in copy.Where(data => data.Key.IsRecycled))
            {
               // Recycler.Recycle<FlashSprite>(data.Value.gameObject);
               _bombTimers.Remove(data.Key);
            }
            
            GameUI.ShowBombIcon(_bombTimers.Count > 0);
            
            
        }
        
        public void ClearList()
        {
            _parts.Clear();
        }
        
        //============================================================================================================//

    }
}