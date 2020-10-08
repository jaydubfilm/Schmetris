using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using StarSalvager.UI;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Values;
using UnityEngine;
using AudioController = StarSalvager.Audio.AudioController;
using GameUI = StarSalvager.UI.GameUI;
using Random = UnityEngine.Random;

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

        [ShowInInspector, ReadOnly]
        public bool CanSelfDestruct { get; private set; }

        private List<Part> _parts;
        private List<Part> _smartWeapons;
        private int maxSmartWeapons;

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
        //float batteryDrainTimer = 0;
        float waterDrainTimer = 0;

        //==============================================================================================================//

        [SerializeField, BoxGroup("Magnets")] public bool useMagnet = true;
        [SerializeField, BoxGroup("Magnets")] public MAGNET currentMagnet = MAGNET.DEFAULT;

        //FIXME I don't think this is the best way of preventing using resouces. Should consider another way
        [SerializeField, BoxGroup("BurnRates")]
        private bool useBurnRate = true;

        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        public float coreHeat;

        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("/s", Overlay = true)]
        private float coolSpeed;

        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("s", Overlay = true)]
        private float coolDelay;

        [ShowInInspector, BoxGroup("Bot Part Data"), ReadOnly]
        private float _coreCoolTimer;

        [ShowInInspector, BoxGroup("Bot Part Data"), ReadOnly]
        public int MagnetCount { get; private set; }
        private int _magnetOverride;

        //==============================================================================================================//

        private Dictionary<Part, bool> _playingSounds;


        private Dictionary<Part, float> _projectileTimers;
        private Dictionary<Part, ShieldData> _shields;
        private Dictionary<Part, FlashSprite> _flashes;
        private Dictionary<Part, float> _bombTimers;

        private Dictionary<Part, Asteroid> _asteroidTargets;

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
            _smartWeapons = _parts.Where(p => p.Type == PART_TYPE.BOMB).ToList();

            //TODO Need to update the UI here for the amount of smart weapons able to be used

            UpdatePartData();
        }

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void UpdatePartData()
        {
            
            
            if (_magnetOverride > 0)
            {
                MagnetCount = _magnetOverride;
            }

            PlayerPersistentData.PlayerData.ClearLiquidCapacity(bot.IsRecoveryDrone);
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

            //Update the Game UI for the Smart Weapons
            //--------------------------------------------------------------------------------------------------------//

            GameUI?.ResetIcons();

            for (int i = 0; i < maxSmartWeapons; i++)
            {
                if (i >= _smartWeapons.Count)
                    break;

                GameUI.SetIconImage(i, _smartWeapons[i].renderer.sprite);
                GameUI.ShowIcon(i, true);
            }

            //--------------------------------------------------------------------------------------------------------//


            MagnetCount = 0;

            foreach (var part in _parts)
            {
                //Destroyed or disabled parts should not contribute to the stats of the bot anymore
                if (part.Destroyed || part.Disabled)
                    continue;
                
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

                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.SMRTCapacity, out value))
                        {
                            maxSmartWeapons = value;
                        }

                        if (_magnetOverride > 0)
                            break;

                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
                        {
                            MagnetCount += value;
                        }
                        break;
                    case PART_TYPE.MAGNET:

                        if (_magnetOverride > 0)
                            break;
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
                        {
                            MagnetCount += value;
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
                    case PART_TYPE.STORERED:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.RED] += value;
                        }
                        break;
                    case PART_TYPE.STOREGREEN:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.GREEN] += value;
                        }
                        break;
                    case PART_TYPE.STOREGREY:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.GREY] += value;
                        }
                        break;
                    case PART_TYPE.STOREYELLOW:
                        if (partData.levels[part.level].TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.YELLOW] += value;
                        }
                        break;
                    case PART_TYPE.BOMB:
                        if(_bombTimers == null)
                            _bombTimers = new Dictionary<Part, float>();

                        if (_bombTimers.ContainsKey(part))
                            break;

                        //GameUI.ShowBombIcon(true);
                        _bombTimers.Add(part, 0f);
                        break;
                }
            }

            //Force update capacities, once new values determined
            PlayerPersistentData.PlayerData.SetCapacities(capacities, bot.IsRecoveryDrone);

            bot.ForceCheckMagnets();
        }

        //============================================================================================================//

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Parts specific update Loop. Updates all part information based on currently attached parts.
        /// </summary>
        public void PartsUpdateLoop()
        {
            float cooldown;

            var powerValue = PlayerPersistentData.PlayerData.liquidResource[BIT_TYPE.YELLOW];
            var powerToRemove = 0f;
            
            
            //Be careful to not use return here
            foreach (var part in _parts)
            {
                if(part.Destroyed || part.Disabled)
                    continue;

                PartRemoteData partRemoteData =
                    FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type);

                var levelData = partRemoteData.levels[part.level];

                //FIXME THis shouldn't happen often, though I may want to reconsider how this is being approached
                if (powerValue == 0f && levelData.powerDraw > 0)
                {
                    part.Disabled = true;
                    UpdatePartData();
                    continue;
                }

                if(levelData.powerDraw > 0f)
                    powerToRemove += levelData.powerDraw * Time.deltaTime;

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

                        PlayerPersistentData.PlayerData.AddLiquidResource(partRemoteData.burnType, addAmount, bot.IsRecoveryDrone);

                        //If we want to process a bit, we want to remove it from the attached list while its processed
                        bot.MarkAttachablePendingRemoval(targetBit);
                        
                        //TODO May want to play around with the order of operations here
                        StartCoroutine(RefineBitCoroutine(targetBit, 1.6f,
                            () =>
                        {
                            bot.DestroyAttachable<Bit>(targetBit);
                        }));



                        resourceValue = addAmount;
                        SessionDataProcessor.Instance.LiquidProcessed(targetBit.Type, addAmount);
                        AudioController.PlaySound(SOUND.BIT_REFINED);
                        bot.ForceCheckMagnets();
                    }

                }
                else
                {
                    //FIXME I don't like how often this is called, will need to rethink this
                    //Hide the icon for part if it exists
                    if(_flashes != null && _flashes.ContainsKey(part))
                        GetAlertIcon(part).SetActive(false);
                }

                //Used to measure total consumption of parts over time
                float resoucesConsumed = 0f;

                switch (part.Type)
                {
                    case PART_TYPE.CORE:
                        var outOfFuel = resourceValue <= 0f && useBurnRate;
                        GameUI.ShowAbortWindow(outOfFuel);

                        
                        //Determines if the player can move with no available fuel
                        //NOTE: This needs to happen before the subtraction of resources to prevent premature force-stop
                        InputManager.Instance.LockSideMovement = resourceValue <= 0f;

                        if (resourceValue > 0f && useBurnRate)
                        {
                            resoucesConsumed = levelData.burnRate * Time.deltaTime;
                            resourceValue -= resoucesConsumed;
                        }

                        
                        CanSelfDestruct = outOfFuel;
                        //LevelManagerUI.OverrideText = outOfFuel ? "Out of Fuel. 'D' to self destruct" : string.Empty;

                        //TODO Need to check on Heating values for the core
                        if (coreHeat <= 0)
                        {
                            GameUI.SetHeatSliderValue(0f);
                            break;
                        }

                        GameUI.SetHeatSliderValue(coreHeat / 100f);


                        part.SetColor(Color.Lerp(Color.white, Color.red, coreHeat / 100f));

                        if (_coreCoolTimer > 0f)
                        {
                            _coreCoolTimer -= Time.deltaTime;
                            break;
                        }
                        else
                            _coreCoolTimer = 0;

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
                            //TODO Need to play the no resources for repair sound here
                            break;
                        }

                        IHealth toRepair;

                        var radius = levelData.GetDataValue<int>(DataTest.TEST_KEYS.Radius);

                        //FIXME I don't think using linq here, especially twice is the best option
                        //TODO This needs to fire every x Seconds
                        toRepair = bot.attachedBlocks.GetAttachablesAroundInRadius<Part>(part, radius)
                            .Where(p => p.Destroyed == false)
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

                        resoucesConsumed = levelData.burnRate * Time.deltaTime;
                        resourceValue -= resoucesConsumed;

                        var repairAmount = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Heal);

                        //FIXME This will need some sort of time cooldown
                        //AudioController.PlaySound(SOUND.REPAIRER_PULSE);

                        //Increase the health of this part depending on the current level of the repairer
                        toRepair.ChangeHealth(repairAmount * Time.deltaTime);
                        

                        TryPlaySound(part, SOUND.REPAIRER_PULSE, toRepair.CurrentHealth < toRepair.StartingHealth);
                        break;
                    case PART_TYPE.BLASTER:
                        
                        //--------------------------------------------------------------------------------------------//
                        if (_projectileTimers == null)
                            _projectileTimers = new Dictionary<Part, float>();

                        if (!_projectileTimers.ContainsKey(part))
                            _projectileTimers.Add(part, 0f);

                        //Cooldown
                        //--------------------------------------------------------------------------------------------//

                        cooldown = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Cooldown);

                        if (_projectileTimers[part] < cooldown)
                        {
                            _projectileTimers[part] += Time.deltaTime;
                            break;
                        }

                        _projectileTimers[part] = 0f;

                        //Check if we have a target before removing resources
                        //--------------------------------------------------------------------------------------------//

                        if (_asteroidTargets.IsNullOrEmpty())
                        {
                            _asteroidTargets = new Dictionary<Part, Asteroid>();
                        }

                        if (!_asteroidTargets.TryGetValue(part, out var asteroid) || asteroid.IsRecycled)
                        {
                            //TODO Find closest asteroids
                            asteroid = LevelManager.Instance.ObstacleManager.Asteroids.FindClosestObstacleInRange(
                                    transform.position, 10);
                            
                            if (asteroid == null)
                                break;
                        }
                        
                        
                        //TODO Determine if this fires at all times or just when there are active enemies in range
                        


                        //Use resources
                        //--------------------------------------------------------------------------------------------//

                        if (useBurnRate)
                        {
                            if (resourceValue <= 0f)
                            {
                                AudioController.PlaySound(SOUND.GUN_CLICK);
                                break;
                            }

                            if (resourceValue > 0)
                            {
                                resoucesConsumed = levelData.burnRate;
                                resourceValue -= resoucesConsumed;
                            }
                        }

                        //--------------------------------------------------------------------------------------------//
                        
                        //TODO Create projectile shooting at new target

                        CreateProjectile(part, levelData, asteroid.transform.position, "Asteroid");

                        break;
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.MISSILE:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.GUN:

                        //TODO Need to determine if the shoot type is looking for enemies or not
                        //--------------------------------------------------------------------------------------------//
                        if (_projectileTimers == null)
                            _projectileTimers = new Dictionary<Part, float>();

                        if (!_projectileTimers.ContainsKey(part))
                            _projectileTimers.Add(part, 0f);

                        //TODO This needs to fire every x Seconds
                        //--------------------------------------------------------------------------------------------//

                        cooldown = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Cooldown);

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
                            if (resourceValue <= 0f)
                            {
                                AudioController.PlaySound(SOUND.GUN_CLICK);
                                break;
                            }

                            if (resourceValue > 0)
                            {
                                resoucesConsumed = levelData.burnRate;
                                resourceValue -= resoucesConsumed;
                            }
                        }

                        Vector3 target;
                        switch (part.Type)
                        {
                            case PART_TYPE.GUN:
                            case PART_TYPE.TRIPLESHOT:
                                target = part.transform.position + part.transform.up;
                                CreateProjectile(part, levelData, target);
                                break;
                            case PART_TYPE.MISSILE:
                                CreateProjectile(part, levelData, enemy);
                                break;
                            case PART_TYPE.SNIPER:
                                var direction = (enemy.transform.position + ((Vector3)Random.insideUnitCircle * 3) - part.transform.position).normalized;

                                var lineShrink = FactoryManager.Instance.GetFactory<ParticleFactory>()
                                    .CreateObject<LineShrink>();
                                
                                var chance = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Probability);
                                var didHitTarget = Random.value <= chance;


                                lineShrink.Init(part.transform.position,
                                    didHitTarget 
                                        ?  enemy.transform.position 
                                        : part.transform.position + direction * 100);
                                
                                if (didHitTarget)
                                {
                                    var damage = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Damage);
                                    enemy.TryHitAt(enemy.transform.position, damage);
                                }
                                
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

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
                                resoucesConsumed = levelData.burnRate * Time.deltaTime;
                                
                                //TODO Shield only use resources when recharging
                                resourceValue -= resoucesConsumed;
                                _shields[part].currentHp += levelData.burnRate * Time.deltaTime;
                                
                                TryPlaySound(part, SOUND.SHIELD_RECHARGE, true);
                            }
                            else
                            {
                                _shields[part].timer += Time.deltaTime;
                            }
                        }
                        else
                        {
                            TryPlaySound(part, SOUND.SHIELD_RECHARGE, false);
                        }

                        //FIXME This needs to have some sort of play cooldown
                        //AudioController.PlaySound(SOUND.SHIELD_RECHARGE);

                        shield.SetAlpha(0.5f * (data.currentHp / fakeHealth));

                        break;
                    case PART_TYPE.BOMB:

                        //TODO This still needs to account for multiple bombs
                        if (!_bombTimers.TryGetValue(part, out var timer))
                            break;

                        if (timer <= 0f)
                            break;

                        var index = _smartWeapons.FindIndex(0, _smartWeapons.Count, x => x == part);

                        if (useBurnRate && resourceValue <= 0)
                        {
                            //FIXME I don't like that this is getting called so often
                            //GameUI.SetHasResource(index, false);
                            break;
                        }

                        //GameUI.SetHasResource(index, true);


                        levelData.TryGetValue(DataTest.TEST_KEYS.Cooldown, out float shieldCooldown);

                        resoucesConsumed = Time.deltaTime;
                        resourceValue -= resoucesConsumed;

                        _bombTimers[part] -= Time.deltaTime;
                        GameUI.SetFill(index, 1f - _bombTimers[part] / shieldCooldown);

                        break;
                    
                }

                UpdateUI(partRemoteData.burnType, resourceValue);

                PlayerPersistentData.PlayerData.SetLiquidResource(partRemoteData.burnType, resourceValue, bot.IsRecoveryDrone);

                if(resoucesConsumed > 0)
                    LevelManager.Instance.WaveEndSummaryData.AddConsumedBit(partRemoteData.burnType, resoucesConsumed);
            }

            powerValue -= powerToRemove;
            if (powerValue < 0)
                powerValue = 0f;
            
            PlayerPersistentData.PlayerData.SetLiquidResource(BIT_TYPE.YELLOW, powerValue, bot.IsRecoveryDrone);
            

            //batteryDrainTimer += Time.deltaTime / 2;
            waterDrainTimer += Time.deltaTime * Constants.waterDrainRate;

            /*if (batteryDrainTimer >= 1 && PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW] > 0)
            {
                batteryDrainTimer--;
                PlayerPersistentData.PlayerData.SetResources(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.resources[BIT_TYPE.YELLOW] - 1);
            }*/
            if (waterDrainTimer >= 1 && PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] > 0)
            {
                waterDrainTimer--;
                PlayerPersistentData.PlayerData.SetResources(BIT_TYPE.BLUE, PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE] - 1);
            }
            UpdateUI(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.liquidResource[BIT_TYPE.YELLOW]);
            UpdateUI(BIT_TYPE.BLUE, PlayerPersistentData.PlayerData.resources[BIT_TYPE.BLUE]);
            
            
        }


        //====================================================================================================================//

        #region Weapons

        private static void CreateProjectile(in Part part, PartLevelData levelData, in Enemy enemy, string collisionTag = "Enemy")
        {
            var projectileId = levelData.GetDataValue<string>(DataTest.TEST_KEYS.Projectile);
            var damage = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Damage);

            var position = part.transform.position;

            //TODO Might need to add something to change the projectile used for each gun piece
            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    projectileId,
                    position,
                    enemy,
                    damage,
                    collisionTag,
                    true);
        }
        private static void CreateProjectile(in Part part, PartLevelData levelData, Vector2 targetPosition, string collisionTag = "Enemy")
        {
            var projectileId = levelData.GetDataValue<string>(DataTest.TEST_KEYS.Projectile);
            var damage = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Damage);

            var position = part.transform.position;

            //TODO Might need to add something to change the projectile used for each gun piece
            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    projectileId,
                    position,
                    targetPosition,
                    damage,
                    collisionTag,
                    true);
        }

        #endregion //Weapons

        //============================================================================================================//

        #region Bomb

        /// <summary>
        /// This should use values similar to an array (ie. starts at [0])
        /// </summary>
        /// <param name="index"></param>
        public void TryTriggerSmartWeapon(int index)
        {
            if (_smartWeapons == null || _smartWeapons.Count == 0)
                return;
            //TODO Need to check the capacity of smart weapons on the bot
            if (index - 1 > maxSmartWeapons)
                return;

            if (index >= _smartWeapons.Count)
                return;

            var part = _smartWeapons[index];

            switch (part.Type)
            {
                case PART_TYPE.BOMB:
                    TriggerBomb(part);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Part.Type), _smartWeapons[index].Type, null);
            }
        }

        private void TriggerBomb(Part part)
        {
            if (_bombTimers == null || _bombTimers.Count == 0)
                return;

            //If the bomb is still recharging, we tell the player that its unavailable
            if (_bombTimers[part] > 0f)
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);

            var partLevelData = partData.levels[part.level];

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

            AudioController.PlaySound(SOUND.BOMB_BLAST);
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

            AudioController.PlaySound(SOUND.SHIELD_ABSORB);

            return outDamage;
        }

        #endregion //Shield

        //============================================================================================================//

        public void SetMagnetOverride(int magnet)
        {
            _magnetOverride = magnet;
            MagnetCount = _magnetOverride;
        }

        //==============================================================================================================//

        public void AddCoreHeat(float amount)
        {
            coreHeat += amount;
            _coreCoolTimer = coolDelay;
        }

        //Updating UI
        //============================================================================================================//

        private void ForceUpdateResourceUI()
        {
            IReadOnlyDictionary<BIT_TYPE, float> liquidResource;
            if (bot.IsRecoveryDrone)
            {
                liquidResource = PlayerPersistentData.PlayerData.recoveryDroneLiquidResource;
            }
            else
            {
                liquidResource = PlayerPersistentData.PlayerData.liquidResource;
            }

            foreach (var f in liquidResource)
            {
                UpdateUI(f.Key, f.Value);
            }

            //UpdateUI(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.li[BIT_TYPE.YELLOW]);
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


            /*var flash = FactoryManager.Instance.GetFactory<BotFactory>().CreateAlertIcon();//Instantiate(flashSpritePrefab).GetComponent<FlashSprite>();
            flash.transform.SetParent(part.transform, false);
            flash.transform.localPosition = Vector3.zero;

            flash.SetColor(bitColor);*/
            
            var burnType = FactoryManager.Instance.PartsRemoteData.GetRemoteData(part.Type).burnType;
            var bitColor = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitProfile(burnType).color;
            
            var flash = FlashSprite.Create(part.transform, Vector3.zero, bitColor);

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

            if (bot.IsRecoveryDrone)
            {
                var value = partLevelData.burnRate == 0
                    ? default
                    : PlayerPersistentData.PlayerData.recoveryDroneLiquidResource[type];
                return value;
            }
            else
            {
                var value = partLevelData.burnRate == 0
                    ? default
                    : PlayerPersistentData.PlayerData.liquidResource[type];
                return value;
            }
        }

        //Checking for recycled extras
        //============================================================================================================//

        private void CheckIfShieldShouldRecycle()
        {
            if (_shields == null || _shields.Count == 0)
                return;

            var copy = new Dictionary<Part, ShieldData>(_shields);
            foreach (var data in copy.Where(data => data.Key.IsRecycled || data.Key.Destroyed))
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
            foreach (var data in copy.Where(data => data.Key.IsRecycled || data.Key.Destroyed))
            {
                _flashes.Remove(data.Key);
                Recycler.Recycle<FlashSprite>(data.Value.gameObject);
            }
        }

        private void CheckIfBombsShouldRecycle()
        {
            if (_bombTimers == null || _bombTimers.Count == 0)
                return;

            var copy = new Dictionary<Part, float>(_bombTimers);
            foreach (var data in copy.Where(data => data.Key.IsRecycled || data.Key.Destroyed))
            {
               _bombTimers.Remove(data.Key);

               var index = _smartWeapons.FindIndex(0, _smartWeapons.Count, x => x == data.Key);
               GameUI.ShowIcon(index, false);
            }

        }

        public void ClearList()
        {
            CheckIfShieldShouldRecycle();
            CheckIfFlashIconShouldRecycle();
            CheckIfBombsShouldRecycle();
            
            _parts.Clear();
        }
        
        //============================================================================================================//

        private void TryPlaySound(Part part, SOUND sound, bool play)
        {
            if(_playingSounds == null)
                _playingSounds = new Dictionary<Part, bool>();

            if (!_playingSounds.ContainsKey(part))
            {
                _playingSounds.Add(part, play);
            }
            else if(_playingSounds[part] == play)
                return;

            _playingSounds[part] = play;
            
            if(play)
                AudioController.PlaySound(sound);
            else
                AudioController.StopSound(sound);
        }

        //============================================================================================================//

        [SerializeField]
        private AnimationCurve refineScaleCurve = new AnimationCurve();

        [SerializeField]
        private AnimationCurve moveSpeedCurve = new AnimationCurve();

        private IEnumerator RefineBitCoroutine(Bit bit, float speed, Action onFinishedCallback)
        {
            var bitStartPosition = bit.transform.position;
            var endPosition = bot.transform.position;
            var t = 0f;

            bit.SetColliderActive(false);
            bit.Coordinate = Vector2Int.zero;
            bit.renderer.sortingOrder = 10000;

            while (t < 1f)
            {
                bit.transform.position = Vector3.Lerp(bitStartPosition, endPosition, t);

                //TODO Need to adjust the scale here
                bit.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, refineScaleCurve.Evaluate(t));

                t += Time.deltaTime * speed * moveSpeedCurve.Evaluate(t);

                yield return null;
            }

            onFinishedCallback?.Invoke();
            bit.transform.localScale = Vector3.one;


        }

        //============================================================================================================//
        


    }
}
