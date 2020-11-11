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
using StarSalvager.Utilities.Saving;
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

            public GameObject gameObject => shield.gameObject;

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

        //FIXME This needs to something more manageable
        private EnemyManager EnemyManager
        {
            get
            {
                if (_enemyManager == null)
                    _enemyManager = FindObjectOfType<EnemyManager>();
                return _enemyManager;
            }
        }
        private EnemyManager _enemyManager;

        private static GameUI GameUI => GameUI.Instance;

        private float _waterDrainTimer;

        //Magnet Properties
        //====================================================================================================================//
        
        [SerializeField, BoxGroup("Magnets")] public bool useMagnet = true;
        [SerializeField, BoxGroup("Magnets")] public MAGNET currentMagnet = MAGNET.DEFAULT;

        //Bit Refining Properties
        //====================================================================================================================//
        
        
        [SerializeField, BoxGroup("Bit Refining")]
        private AnimationCurve refineScaleCurve = new AnimationCurve();

        [SerializeField, BoxGroup("Bit Refining")]
        private AnimationCurve moveSpeedCurve = new AnimationCurve();

        //==============================================================================================================//

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
        
        private Dictionary<Part, Transform> _turrets;
        private Dictionary<Part, Enemy> _gunTargets;
        
        private List<Part> _parts;
        private List<Part> _smartWeapons;
        private int _maxSmartWeapons;

        private Dictionary<FACILITY_TYPE, int> _facilityImprovements;

        private Dictionary<Part, bool> _playingSounds;

        private Dictionary<Part, float> _projectileTimers;
        private Dictionary<Part, ShieldData> _shields;
        private Dictionary<Part, FlashSprite> _flashes;
        private Dictionary<Part, float> _bombTimers;

        private Dictionary<Part, Asteroid> _asteroidTargets;

        private Dictionary<Part, float> _gunRanges;

        private static PartAttachableFactory _partAttachableFactory;

        //Unity Functions
        //==============================================================================================================//

        private void Start()
        {
            _partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
        }

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += ForceUpdateResourceUI;
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= ForceUpdateResourceUI;
        }

        //==============================================================================================================//
        
        public void AddCoreHeat(float amount)
        {
            coreHeat += amount;
            _coreCoolTimer = coolDelay;
        }

        //====================================================================================================================//
        
        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        public void PopulatePartsList()
        {
            _parts = bot.attachedBlocks.OfType<Part>().ToList();
            _smartWeapons = _parts.Where(p => p.Type == PART_TYPE.BOMB || p.Type == PART_TYPE.FREEZE).ToList();

            //TODO Need to update the UI here for the amount of smart weapons able to be used

            InitPartData();
        }

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void InitPartData()
        {
            if (_magnetOverride > 0)
            {
                MagnetCount = _magnetOverride;
            }

            _gunTargets = new Dictionary<Part, Enemy>();

            var liquidCapacities = new Dictionary<BIT_TYPE, int>
            {
                {BIT_TYPE.RED, 0},
                {BIT_TYPE.BLUE, 0},
                {BIT_TYPE.YELLOW, 0},
                {BIT_TYPE.GREEN, 0},
                {BIT_TYPE.GREY, 0},
            };

            var usedResourceTypes = new List<BIT_TYPE>();
            /*
            if(!Globals.UsingTutorial) usedResourceTypes.Add(BIT_TYPE.BLUE);
            */

            UpdateFacilityData();
            
            TryClearPartDictionaries();

            /*CheckIfShieldShouldRecycle();
            CheckIfFlashIconShouldRecycle();
            CheckIfBombsShouldRecycle();*/

            //Update the Game UI for the Smart Weapons
            //--------------------------------------------------------------------------------------------------------//

            GameUI.ResetIcons();

            for (int i = 0; i < _maxSmartWeapons; i++)
            {
                if (i >= _smartWeapons.Count)
                    break;

                GameUI.SetIconImage(i, _smartWeapons[i].Type);
                GameUI.ShowIcon(i, true);
            }

            //--------------------------------------------------------------------------------------------------------//

            FindObjectOfType<GameUI>();
            MagnetCount = 0;

            foreach (var part in _parts)
            {

                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);

                var levelData = partData.levels[part.level];

                if (levelData.burnRate > 0 && !usedResourceTypes.Contains(partData.burnType))
                    usedResourceTypes.Add(partData.burnType);

                if(levelData.powerDraw > 0f && !usedResourceTypes.Contains(BIT_TYPE.YELLOW))
                    usedResourceTypes.Add(BIT_TYPE.YELLOW);

                //Destroyed or disabled parts should not contribute to the stats of the bot anymore
                if (part.Destroyed || part.Disabled)
                    continue;

                int value;
                switch (part.Type)
                {
                    case PART_TYPE.CORE:

                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.RED] += value;
                            liquidCapacities[BIT_TYPE.GREEN] += value;
                            liquidCapacities[BIT_TYPE.GREY] += value;
                            liquidCapacities[BIT_TYPE.YELLOW] += value;
                            liquidCapacities[BIT_TYPE.BLUE] += value;
                        }

                        if (levelData.TryGetValue(DataTest.TEST_KEYS.SMRTCapacity, out value))
                        {
                            _maxSmartWeapons = value;
                        }

                        if (_magnetOverride > 0)
                            break;

                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
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
                        var shield = FactoryManager.Instance.GetFactory<EffectFactory>().CreateObject<Shield>();

                        shield.transform.SetParent(part.transform);
                        shield.transform.localPosition = Vector3.zero;


                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Radius, out value))
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
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            value = Mathf.RoundToInt(value * GetFacilityImprovement(FACILITY_TYPE.CONTAINERIMPROVE));

                            liquidCapacities[BIT_TYPE.RED] += value;
                            liquidCapacities[BIT_TYPE.GREEN] += value;
                            liquidCapacities[BIT_TYPE.GREY] += value;
                        }

                        break;
                    case PART_TYPE.STORERED:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            value = Mathf.RoundToInt(value * GetFacilityImprovement(FACILITY_TYPE.CONTAINERIMPROVE));

                            liquidCapacities[BIT_TYPE.RED] += value;
                        }

                        break;
                    case PART_TYPE.STOREGREEN:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            value = Mathf.RoundToInt(value * GetFacilityImprovement(FACILITY_TYPE.CONTAINERIMPROVE));

                            liquidCapacities[BIT_TYPE.GREEN] += value;
                        }

                        break;
                    case PART_TYPE.STOREGREY:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            value = Mathf.RoundToInt(value * GetFacilityImprovement(FACILITY_TYPE.CONTAINERIMPROVE));

                            liquidCapacities[BIT_TYPE.GREY] += value;
                        }

                        break;
                    case PART_TYPE.STOREYELLOW:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            value = Mathf.RoundToInt(value * GetFacilityImprovement(FACILITY_TYPE.CONTAINERIMPROVE));

                            liquidCapacities[BIT_TYPE.YELLOW] += value;
                        }

                        break;
                    case PART_TYPE.FREEZE:
                    case PART_TYPE.BOMB:
                        if (_bombTimers == null)
                            _bombTimers = new Dictionary<Part, float>();

                        if (_bombTimers.ContainsKey(part))
                            break;

                        //GameUI.ShowBombIcon(true);
                        _bombTimers.Add(part, 0f);
                        break;

                    case PART_TYPE.GUN:
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.MISSILE:
                        _gunTargets.Add(part, null);
                        CreateTurretEffect(part);
                        break;
                }
            }

            SetupHealthBoots();
            SetupGunRangeValues();

            //Force update capacities, once new values determined
            foreach (var capacity in liquidCapacities)
            {
                PlayerDataManager.GetResource(capacity.Key).SetLiquidCapacity(capacity.Value);
            }

            bot.ForceCheckMagnets();
            GameUI.ShowLiquidSliders(usedResourceTypes);
        }
        
        private void SetupHealthBoots()
        {
            var pendingBoosts = new Dictionary<Part, float>();

            //Find & determine every part which will be updated
            foreach (var defenceBoost in _parts.Where(x => x.Type == PART_TYPE.BOOSTDEFENSE))
            {
                var partsAround = _parts.GetAttachablesAround(defenceBoost).OfType<Part>();
                var boostAmount = GetDefenseBoost(defenceBoost);

                foreach (var part in partsAround)
                {
                    if(!pendingBoosts.ContainsKey(part))
                        pendingBoosts.Add(part, boostAmount);
                    else
                    {
                        pendingBoosts[part] += boostAmount;
                    }
                }
            }

            if (pendingBoosts.IsNullOrEmpty())
                return;

            foreach (var pendingBoost in pendingBoosts)
            {
                pendingBoost.Key.SetHealthBoost(pendingBoost.Value);
            }
        }

        //Parts Update Loop
        //============================================================================================================//

        /// <summary>
        /// Returns true if the part has the power required to function
        /// </summary>
        /// <param name="part"></param>
        /// <param name="partLevelData"></param>
        /// <param name="powerValue"></param>
        /// <param name="powerToRemove"></param>
        /// <returns></returns>
        private bool TryUpdatePowerUsage(Part part,
            in PartLevelData partLevelData,
            in float powerValue,
            ref float powerToRemove,
            in float deltaTime)
        {
            if (part.Disabled && powerValue == 0f)
                return false;
            
            if (part.Disabled && powerValue > 0f && partLevelData.powerDraw > 0)
            {
                part.Disabled = false;
                InitPartData();
            }
            //FIXME THis shouldn't happen often, though I may want to reconsider how this is being approached
            else if (powerValue == 0f && partLevelData.powerDraw > 0)
            {
                part.Disabled = true;
                InitPartData();
                return false;
            }

            if(partLevelData.powerDraw > 0f)
                powerToRemove += partLevelData.powerDraw * deltaTime;

            return true;
        }

        private void TryUpdateResourceUsage(Part part, in PartRemoteData partRemoteData, in PartLevelData partLevelData, out float resourceValue)
        {
            resourceValue = default;
            
            //If there's nothing using these resources ignore
            if(partLevelData.burnRate == 0f)
                return;

            resourceValue = GetValueToBurn(partLevelData, partRemoteData.burnType);


            //If we no longer have liquid to use, find a bit that could be refined
            if (resourceValue <= 0f && useBurnRate)
            {
                var targetBit = GetFurthestBitToBurn(partLevelData, partRemoteData.burnType);

                if (targetBit == null)
                {
                    //FIXME I don't like how often this is called, will need to rethink this
                    //Display the icon for this part if we have no more resources
                    GetAlertIcon(part).SetActive(true);
                }
                else
                {
                    resourceValue = ProcessBit(targetBit);
                }

            }
            else
            {
                //FIXME I don't like how often this is called, will need to rethink this
                //Hide the icon for part if it exists
                if(_flashes != null && _flashes.ContainsKey(part))
                    GetAlertIcon(part).SetActive(false);
            }
        }

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Parts specific update Loop. Updates all part information based on currently attached parts.
        /// </summary>
        public void PartsUpdateLoop()
        {
            (PartRemoteData partRemoteData, PartLevelData partLevelData) GetPartData(in Part part)
            {
                var partRemoteData = _partAttachableFactory.GetRemoteData(part.Type);
                var partLevelData = partRemoteData.levels[part.level];

                return (partRemoteData, partLevelData);
            }

            var deltaTime = Time.deltaTime;

            var powerValue = PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquid;
            var powerToRemove = 0f;

            //Be careful to not use return here
            foreach (var part in _parts)
            {
                if(part.Destroyed)
                    continue;
                
                var (partRemoteData, levelData) = GetPartData(part);

                if(!TryUpdatePowerUsage(part, levelData, powerValue, ref powerToRemove, deltaTime))
                    continue;

                TryUpdateResourceUsage(part, partRemoteData, levelData, out var resourceValue);

                //Used to measure total consumption of parts over time
                float resourcesConsumed = 0f;

                switch (part.Type)
                {
                    case PART_TYPE.CORE:
                        CoreUpdate(part, levelData, ref resourceValue, ref resourcesConsumed, deltaTime);
                        break;
                    case PART_TYPE.REPAIR:

                        RepairUpdate(part, levelData, ref resourceValue, ref resourcesConsumed, deltaTime);

                        /*f (resourceValue <= 0f && useBurnRate)
                        {
                            //TODO Need to play the no resources for repair sound here
                            break;
                        }

                        IHealthBoostable toRepair;

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
                            if (part.CurrentHealth < part.BoostedHealth)
                                toRepair = part;
                            else
                                break;
                        }

                        resourcesConsumed = levelData.burnRate * Time.deltaTime;
                        resourceValue -= resourcesConsumed;

                        var repairAmount = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Heal);
                        repairAmount *= GetFacilityImprovement(FACILITY_TYPE.REPAIRERIMPROVE);
                        repairAmount *= GetBoostValue(PART_TYPE.BOOSTRATE, part);

                        //FIXME This will need some sort of time cooldown
                        //AudioController.PlaySound(SOUND.REPAIRER_PULSE);

                        //Increase the health of this part depending on the current level of the repairer
                        toRepair.ChangeHealth(repairAmount * deltaTime);
                        PlayerDataManager.AddRepairsDone(repairAmount * deltaTime);


                        TryPlaySound(part, SOUND.REPAIRER_PULSE, toRepair.CurrentHealth < toRepair.BoostedHealth);*/
                        break;
                    case PART_TYPE.BLASTER:
                        BlasterUpdate(part, levelData, ref resourceValue, ref resourcesConsumed, deltaTime);
                        break;
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.MISSILE:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.GUN:
                        GunUpdate(part, levelData, ref resourceValue, ref resourcesConsumed, deltaTime);
                        break;
                    case PART_TYPE.SHIELD:
                        ShieldUpdate(part, levelData, ref resourceValue, ref resourcesConsumed, deltaTime);
                        break;
                    case PART_TYPE.FREEZE:
                    case PART_TYPE.BOMB:
                        BombUpdate(part, levelData, ref resourceValue, ref resourcesConsumed, deltaTime);
                        break;
                }

                UpdateUI(partRemoteData.burnType, resourceValue);

                if(bot.PROTO_GodMode)
                    continue;

                PlayerDataManager.GetResource(partRemoteData.burnType).SetLiquid(resourceValue);

                if(resourcesConsumed > 0)
                    LevelManager.Instance.WaveEndSummaryData.AddConsumedBit(partRemoteData.burnType, resourcesConsumed);
            }

            TryRemoveResources(powerValue, powerToRemove, deltaTime);

            UpdateUI(BIT_TYPE.YELLOW, PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquid);
            UpdateUI(BIT_TYPE.BLUE, PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource);
        }

        private void TryRemoveResources(float powerValue, float powerToRemove, in float deltaTime)
        {
            if (bot.PROTO_GodMode) 
                return;
            powerValue -= powerToRemove;
            if (powerValue < 0)
                powerValue = 0f;

            PlayerDataManager.GetResource(BIT_TYPE.YELLOW).SetLiquid(powerValue);


            _waterDrainTimer += deltaTime * Constants.waterDrainRate;

            if (_waterDrainTimer >= 1 && PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource > 0)
            {
                _waterDrainTimer--;
            }
        }

        //Individual Part Functions
        //====================================================================================================================//
        
        
        #region Parts

        private void CoreUpdate(in Part part, in PartLevelData partLevelData, ref float resourceValue,
            ref float resourcesConsumed, in float deltaTime)
        {
            var outOfFuel = resourceValue <= 0f && useBurnRate;
            GameUI.ShowAbortWindow(outOfFuel);


            //Determines if the player can move with no available fuel
            //NOTE: This needs to happen before the subtraction of resources to prevent premature force-stop
            InputManager.Instance.LockSideMovement = resourceValue <= 0f;

            if (resourceValue > 0f && useBurnRate)
            {
                resourcesConsumed = partLevelData.burnRate * deltaTime;
                resourceValue -= resourcesConsumed;
            }


            CanSelfDestruct = outOfFuel;
            //LevelManagerUI.OverrideText = outOfFuel ? "Out of Fuel. 'D' to self destruct" : string.Empty;

            //TODO Need to check on Heating values for the core
            if (coreHeat <= 0)
            {
                GameUI.SetHeatSliderValue(0f);
                return;
            }

            GameUI.SetHeatSliderValue(coreHeat / 100f);


            part.SetColor(Color.Lerp(Color.white, Color.red, coreHeat / 100f));

            if (_coreCoolTimer > 0f)
            {
                _coreCoolTimer -= deltaTime;
                return;
            }

            _coreCoolTimer = 0;

            coreHeat -= coolSpeed * GetFacilityImprovement(FACILITY_TYPE.COOLING) * deltaTime;

            if (coreHeat > 0)
                return;

            coreHeat = 0;
            part.SetColor(Color.white);
        }

        private void RepairUpdate(in Part part, in PartLevelData partLevelData, ref float resourceValue,
            ref float resourcesConsumed, in float deltaTime)
        {
            if (resourceValue <= 0f && useBurnRate)
            {
                //TODO Need to play the no resources for repair sound here
                return;
            }

            var radius = partLevelData.GetDataValue<int>(DataTest.TEST_KEYS.Radius);

            //FIXME I don't think using linq here, especially twice is the best option
            //TODO This needs to fire every x Seconds
            IHealthBoostable toRepair = bot.attachedBlocks.GetAttachablesAroundInRadius<Part>(part, radius)
                .Where(p => p.Destroyed == false)
                .Where(p => p.CurrentHealth < p.StartingHealth)
                .Select(x => new KeyValuePair<Part, float>(x,
                    FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(x.Type).priority /
                    (x.CurrentHealth / x.StartingHealth)))
                .OrderByDescending(x => x.Value)
                .FirstOrDefault().Key;

            //If we weren't able to find a part, see if the repairer needs to be fixed
            if (toRepair is null)
            {
                //TODO Need to determine if this is already happening
                //If the repairer is also fine, then we can break out
                if (part.CurrentHealth < part.BoostedHealth)
                    toRepair = part;
                else
                    return;
            }

            resourcesConsumed = partLevelData.burnRate * deltaTime;
            resourceValue -= resourcesConsumed;

            var repairAmount = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Heal);
            repairAmount *= GetFacilityImprovement(FACILITY_TYPE.REPAIRERIMPROVE);
            repairAmount *= GetBoostValue(PART_TYPE.BOOSTRATE, part);

            //FIXME This will need some sort of time cooldown
            //AudioController.PlaySound(SOUND.REPAIRER_PULSE);

            //Increase the health of this part depending on the current level of the repairer
            toRepair.ChangeHealth(repairAmount * deltaTime);
            PlayerDataManager.AddRepairsDone(repairAmount * deltaTime);


            TryPlaySound(part, SOUND.REPAIRER_PULSE, toRepair.CurrentHealth < toRepair.BoostedHealth);
        }

        private void BlasterUpdate(in Part part, in PartLevelData partLevelData, ref float resourceValue,
            ref float resourcesConsumed, in float deltaTime)
        {
            //--------------------------------------------------------------------------------------------//
            if (_projectileTimers == null)
                _projectileTimers = new Dictionary<Part, float>();

            if (!_projectileTimers.ContainsKey(part))
                _projectileTimers.Add(part, 0f);

            //Cooldown
            //--------------------------------------------------------------------------------------------//

            var cooldown = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Cooldown);
            cooldown /= GetBoostValue(PART_TYPE.BOOSTRATE, part);

            if (_projectileTimers[part] < cooldown)
            {
                _projectileTimers[part] += deltaTime;
                return;
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
                    return;
            }


            //TODO Determine if this fires at all times or just when there are active enemies in range



            //Use resources
            //--------------------------------------------------------------------------------------------//

            if (useBurnRate)
            {
                if (resourceValue <= 0f)
                {
                    AudioController.PlaySound(SOUND.GUN_CLICK);
                    return;
                }

                if (resourceValue > 0)
                {
                    resourcesConsumed = partLevelData.burnRate *
                                        GetFacilityImprovement(FACILITY_TYPE.AMMODISCOUNT);
                    resourceValue -= resourcesConsumed;
                }
            }

            //--------------------------------------------------------------------------------------------//

            //TODO Create projectile shooting at new target

            CreateProjectile(part, partLevelData, asteroid, "Asteroid");
        }

        private void GunUpdate(in Part part, in PartLevelData partLevelData, ref float resourceValue,
            ref float resourcesConsumed, in float deltaTime)
        {
            //TODO Need to determine if the shoot type is looking for enemies or not
            //--------------------------------------------------------------------------------------------//
            if (_projectileTimers == null)
                _projectileTimers = new Dictionary<Part, float>();

            if (!_projectileTimers.ContainsKey(part))
                _projectileTimers.Add(part, 0f);

            //Aim the Turret Effect
            //--------------------------------------------------------------------------------------------//

            var target = _gunTargets[part];
            
            if (target && target.IsRecycled == false)
            {
                var targetTransform = target.transform;
                var normDirection = (targetTransform.position - part.transform.position).normalized;
                _turrets[part].up = normDirection;
            }
            else if (target && target.IsRecycled)
            {
                _gunTargets[part] = null;
            }

            //TODO This needs to fire every x Seconds
            //--------------------------------------------------------------------------------------------//

            //FIXME This now might more sense to count down instead of counting up
            var cooldown = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Cooldown);
            cooldown /= GetBoostValue(PART_TYPE.BOOSTRATE, part);

            if (_projectileTimers[part] < cooldown)
            {
                _projectileTimers[part] += deltaTime;
                return;
            }

            _projectileTimers[part] = 0f;

            //Check if we have a target before removing resources
            //--------------------------------------------------------------------------------------------//

            var range = _gunRanges[part];

            var enemy = EnemyManager.GetClosestEnemy(part.transform.position, range);
            //TODO Determine if this fires at all times or just when there are active enemies in range
            if (enemy == null)
                return;

            _gunTargets[part] = enemy;


            //Use resources
            //--------------------------------------------------------------------------------------------//

            if (useBurnRate)
            {
                if (resourceValue <= 0f)
                {
                    AudioController.PlaySound(SOUND.GUN_CLICK);
                    return;
                }

                if (resourceValue > 0)
                {
                    resourcesConsumed = partLevelData.burnRate *
                                        GetFacilityImprovement(FACILITY_TYPE.AMMODISCOUNT);
                    resourceValue -= resourcesConsumed;
                }
            }

            switch (part.Type)
            {
                case PART_TYPE.GUN:
                case PART_TYPE.TRIPLESHOT:
                case PART_TYPE.MISSILE:
                    CreateProjectile(part, partLevelData, enemy);
                    break;
                case PART_TYPE.SNIPER:
                    var direction = (enemy.transform.position + ((Vector3) Random.insideUnitCircle * 3) -
                                     part.transform.position).normalized;

                    var lineShrink = FactoryManager.Instance.GetFactory<EffectFactory>()
                        .CreateObject<LineShrink>();

                    var chance = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Probability);
                    var didHitTarget = Random.value <= chance;


                    lineShrink.Init(part.transform.position,
                        didHitTarget
                            ? enemy.transform.position
                            : part.transform.position + direction * 100);

                    if (didHitTarget)
                    {
                        var damage = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Damage);
                        enemy.TryHitAt(enemy.transform.position, damage);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //--------------------------------------------------------------------------------------------//
        }

        private void ShieldUpdate(in Part part, in PartLevelData partLevelData, ref float resourceValue,
            ref float resourcesConsumed, in float deltaTime)
        {
            const float fakeHealth = 25f;

            var data = _shields[part];
            var shield = data.shield;
            //shield.transform.position = part.transform.position;

            if (resourceValue <= 0f && useBurnRate && data.currentHp <= 0)
            {
                shield.SetAlpha(0f);
                return;
            }

            if (data.currentHp < fakeHealth)
            {
                //TODO Shield has countdown before it can begin recharging
                if (data.timer >= data.waitTime)
                {
                    resourcesConsumed = partLevelData.burnRate * deltaTime;

                    //TODO Shield only use resources when recharging
                    resourceValue -= resourcesConsumed;
                    _shields[part].currentHp += partLevelData.burnRate * deltaTime;

                    TryPlaySound(part, SOUND.SHIELD_RECHARGE, true);
                }
                else
                {
                    _shields[part].timer += deltaTime;
                }
            }
            else
            {
                TryPlaySound(part, SOUND.SHIELD_RECHARGE, false);
            }

            //FIXME This needs to have some sort of play cooldown
            //AudioController.PlaySound(SOUND.SHIELD_RECHARGE);

            shield.SetAlpha(0.5f * (data.currentHp / fakeHealth));
        }
        
        private void BombUpdate(in Part part, in PartLevelData partLevelData, ref float resourceValue,
            ref float resourcesConsumed, in float deltaTime)
        {
            //TODO This still needs to account for multiple bombs
            if (!_bombTimers.TryGetValue(part, out var timer))
                return;

            if (timer <= 0f)
                return;
            
            var tempPart = part;


            var index = _smartWeapons.FindIndex(0, _smartWeapons.Count, x => x == tempPart);

            if (useBurnRate && resourceValue <= 0)
            {
                //FIXME I don't like that this is getting called so often
                //GameUI.SetHasResource(index, false);
                return;
            }

            partLevelData.TryGetValue(DataTest.TEST_KEYS.Cooldown, out float bombCooldown);

            resourcesConsumed = deltaTime;
            resourceValue -= resourcesConsumed;

            _bombTimers[part] -= deltaTime;
            GameUI.SetFill(index, 1f - _bombTimers[part] / bombCooldown);
        }

        #endregion //Parts
        
        

        //====================================================================================================================//

        #region Weapons

        private void SetupGunRangeValues()
        {
            _gunRanges = new Dictionary<Part, float>();

            foreach (var part in _parts)
            {
                //Destroyed or disabled parts should not contribute to the stats of the bot anymore
                if (part.Destroyed || part.Disabled)
                    continue;

                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);

                switch (part.Type)
                {
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.MISSILE:
                    case PART_TYPE.GUN:

                        var projectileID = partData.levels[part.level]
                            .GetDataValue<string>(DataTest.TEST_KEYS.Projectile);

                        _gunRanges.Add(part, GetProjectileRange(part, projectileID));

                        break;
                }
            }
        }

        private void CreateProjectile(in Part part, PartLevelData levelData, in CollidableBase collidableTarget, string collisionTag = "Enemy")
        {
            var projectileId = levelData.GetDataValue<string>(DataTest.TEST_KEYS.Projectile);
            var damage = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Damage);
            damage *= GetBoostValue(PART_TYPE.BOOSTDAMAGE, part);

            var rangeBoost = GetBoostValue(PART_TYPE.BOOSTRANGE, part);

            var position = part.transform.position;

            //TODO Might need to add something to change the projectile used for each gun piece
            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    projectileId,
                    position,
                    collidableTarget,
                    part.transform.up.normalized,
                    damage,
                    rangeBoost,
                    collisionTag,
                    true);
        }

        private float GetProjectileRange(in Part part, string projectileID)
        {
            var projectileData = FactoryManager.Instance.GetFactory<ProjectileFactory>().GetProfileData(projectileID);

            var range = projectileData.ProjectileRange;

            if (range == 0f)
                return 100 * Constants.gridCellSize;

            var rangeBoost = GetBoostValue(PART_TYPE.BOOSTRANGE, part);

            return range * rangeBoost;
        }

        #endregion //Weapons

        //============================================================================================================//

        #region mart Weapons

        /// <summary>
        /// This should use values similar to an array (ie. starts at [0])
        /// </summary>
        /// <param name="index"></param>
        public void TryTriggerSmartWeapon(int index)
        {
            if (_smartWeapons == null || _smartWeapons.Count == 0)
                return;
            //TODO Need to check the capacity of smart weapons on the bot
            if (index - 1 > _maxSmartWeapons)
                return;

            if (index >= _smartWeapons.Count)
                return;

            var part = _smartWeapons[index];

            switch (part.Type)
            {
                case PART_TYPE.BOMB:
                    TriggerBomb(part);
                    break;
                case PART_TYPE.FREEZE:
                    TriggerFreeze(part);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Part.Type), _smartWeapons[index].Type, null);
            }
        }

        private void TriggerBomb(in Part part)
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
                _bombTimers[part] = cooldown * GetFacilityImprovement(FACILITY_TYPE.COOLDOWNDECREASE);
            }

            //Damage all the enemies
            if (partLevelData.TryGetValue(DataTest.TEST_KEYS.Damage, out float damage))
            {
                EnemyManager.DamageAllEnemies(damage);
            }

            CreateBombEffect(part, 50f);

            AudioController.PlaySound(SOUND.BOMB_BLAST);
        }

        private void TriggerFreeze(in Part part)
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
                _bombTimers[part] = cooldown* GetFacilityImprovement(FACILITY_TYPE.COOLDOWNDECREASE);
            }

            partLevelData.TryGetValue(DataTest.TEST_KEYS.Radius, out int radius);
            partLevelData.TryGetValue(DataTest.TEST_KEYS.Time, out float freezeTime);

            var enemies = EnemyManager.GetEnemiesInRange(part.transform.position, radius);

            foreach (var enemy in enemies)
            {
                enemy.SetFrozen(freezeTime);
            }

            //Need to pass the diameter not the radius
            CreateBombEffect(part, radius * 2f);
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

        //Updating UI
        //============================================================================================================//

        #region Update UI

        private void ForceUpdateResourceUI()
        {
            if (GameUI == null)
                return;

            foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE)
                    continue;

                UpdateUI(_bitType, PlayerDataManager.GetResource(_bitType).liquid);
            }

            //UpdateUI(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.li[BIT_TYPE.YELLOW]);
            UpdateUI(BIT_TYPE.BLUE, PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource);
        }

        private static void UpdateUI(BIT_TYPE type, float value)
        {
            if (!GameUI)
                return;

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

        #endregion //Update UI

        //Find Bits/Values to burn
        //============================================================================================================//

        #region Process Bit

        public int ProcessBit(Bit targetBit)
        {
            if (targetBit is ICanCombo iCanCombo && iCanCombo.IsBusy)
                return 0;

            var bitType = targetBit.Type;
            var amountProcessed = FactoryManager.Instance
                .GetFactory<BitAttachableFactory>()
                .GetBitRemoteData(bitType)
                .levels[targetBit.level]
                .resources;

            PlayerResource playerResource = PlayerDataManager.GetResource(bitType);
            var current = playerResource.resource;
            var capacity = playerResource.resourceCapacity;

            /*var current = PlayerPersistentData.PlayerData.liquidResource[bitType];
            var capacity = PlayerPersistentData.PlayerData.liquidCapacity[bitType];*/

            //We wont add any if its already full!
            /*if (current + amountProcessed > capacity)
                return 0;*/

            if (current == capacity)
                return 0;

            PlayerDataManager.GetResource(targetBit.Type).AddLiquid(amountProcessed);

            //If we want to process a bit, we want to remove it from the attached list while its processed
            bot.MarkAttachablePendingRemoval(targetBit);

            //TODO May want to play around with the order of operations here
            StartCoroutine(RefineBitCoroutine(targetBit, 1.6f,
                () =>
                {
                    bot.DestroyAttachable<Bit>(targetBit);
                }));


            SessionDataProcessor.Instance.LiquidProcessed(targetBit.Type, amountProcessed);
            AudioController.PlaySound(SOUND.BIT_REFINED);
            bot.ForceCheckMagnets();

            return amountProcessed;
        }

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
                : PlayerDataManager.GetResource(type).liquid;
            return value;
        }

        #endregion //Process Bit

        //Boosts
        //====================================================================================================================//

        #region Boosts

        //FIXME This is very efficient for finding the parts
        private float GetBoostValue(PART_TYPE boostPart, Part fromPart)
        {
            var boosts = _parts.Where(x => x.Type == boostPart).ToList();

            if (boosts.IsNullOrEmpty())
                return 1f;

            //FIXME Need to think of a way to get the corners for the boosts
            var beside = boosts
                .GetAttachablesAround(fromPart)
                .OfType<Part>()
                .Where(x => boosts.Contains(x))
                .ToList();

            if (beside.IsNullOrEmpty())
                return 1f;

            PartRemoteData partRemoteData =
                FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(boostPart);

            var maxBoost = 1f;
            foreach (var levelData in beside.Select(part => partRemoteData.levels[part.level]))
            {
                if (!levelData.TryGetValue(DataTest.TEST_KEYS.Multiplier, out float mult))
                    continue;

                if (mult > maxBoost)
                    maxBoost = mult;
            }

            return maxBoost * GetFacilityImprovement(FACILITY_TYPE.BOOSTIMPROVE);
        }

        private float GetDefenseBoost(Part part)
        {
            if (part.Type != PART_TYPE.BOOSTDEFENSE)
                return 0f;

            if (part.Destroyed || part.Disabled)
                return 0f;

            PartRemoteData partRemoteData =
                FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type);


            return partRemoteData.levels[part.level].GetDataValue<float>(DataTest.TEST_KEYS.Absorb) *
                   GetFacilityImprovement(FACILITY_TYPE.BOOSTIMPROVE);
        }

        #endregion //Boosts

        //Checking for recycled extras
        //============================================================================================================//

        #region Part Dictionary Recycling

        private void TryClearPartDictionaries()
        {
            CheckShouldRecycle(ref _shields, (ShieldData data) =>
            {
                Recycler.Recycle<Shield>(data.gameObject);
            });
            
            CheckShouldRecycle(ref _flashes, (FlashSprite data) =>
            {
                Recycler.Recycle<FlashSprite>(data.gameObject);
            });
            CheckShouldRecycle(ref _bombTimers, (Part part) =>
            {
                var index = _smartWeapons.FindIndex(0, _smartWeapons.Count, x => x == part);
                GameUI.ShowIcon(index, false);
            });
        }


        /*private void CheckIfShieldShouldRecycle()
        {
            

            /*if (_shields == null || _shields.Count == 0)
                return;

            var copy = new Dictionary<Part, ShieldData>(_shields);
            foreach (var data in copy.Where(data => data.Key.IsRecycled || data.Key.Destroyed))
            {
                Recycler.Recycle<Shield>(data.Value.shield.gameObject);
                _shields.Remove(data.Key);
            }#1#
        }*/

        /*private void CheckIfFlashIconShouldRecycle()
        {
            if (_flashes == null || _flashes.Count == 0)
                return;

            var copy = new Dictionary<Part, FlashSprite>(_flashes);
            foreach (var data in copy.Where(data => data.Key.IsRecycled || data.Key.Destroyed))
            {
                _flashes.Remove(data.Key);
                Recycler.Recycle<FlashSprite>(data.Value.gameObject);
            }
        }*/

        /*private void CheckIfBombsShouldRecycle()
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

        }*/

        public void ClearList()
        {
            TryClearPartDictionaries();
            CleanEffects();
            /*CheckIfShieldShouldRecycle();
            CheckIfFlashIconShouldRecycle();
            CheckIfBombsShouldRecycle();*/

            _parts.Clear();
        }
        
        private static void CheckShouldRecycle<T>(ref Dictionary<Part, T> partDictionary, Action<T> OnRecycleCallback)
        {
            if (partDictionary.IsNullOrEmpty())
                return;

            var copy = new Dictionary<Part, T>(partDictionary);
            foreach (var data in copy.Where(data => data.Key.IsRecycled || data.Key.Destroyed))
            {
                //Recycler.Recycle<Shield>(data.Value.gameObject);
                OnRecycleCallback?.Invoke(data.Value);
                
                partDictionary.Remove(data.Key);
            }
        }
        private static void CheckShouldRecycle<T>(ref Dictionary<Part, T> partDictionary, Action<Part> OnRecycleCallback)
        {
            if (partDictionary.IsNullOrEmpty())
                return;

            var copy = new Dictionary<Part, T>(partDictionary);
            foreach (var data in copy.Where(data => data.Key.IsRecycled || data.Key.Destroyed))
            {
                //Recycler.Recycle<Shield>(data.Value.gameObject);
                OnRecycleCallback?.Invoke(data.Key);
                
                partDictionary.Remove(data.Key);
            }
        }

        #endregion //Part Dictionary Recycling

        //Facility Data
        //====================================================================================================================//

        #region Process Facility Values

        private void UpdateFacilityData()
        {
            _facilityImprovements =
                new Dictionary<FACILITY_TYPE, int>(
                    (IDictionary<FACILITY_TYPE, int>) PlayerDataManager.GetFacilityRanks());
        }

        private float GetFacilityImprovement(FACILITY_TYPE facilityType)
        {
            if (_facilityImprovements.IsNullOrEmpty())
                return 1f;

            if (!_facilityImprovements.TryGetValue(facilityType, out var level))
                return 1f;

            switch (facilityType)
            {
                //----------------------------------------------------------------------------------------------------//
                case FACILITY_TYPE.COOLDOWNDECREASE:
                    return 1f - (0.2f * level);
                case FACILITY_TYPE.COOLING:
                    return 1f + (0.2f * level);
                //----------------------------------------------------------------------------------------------------//
                case FACILITY_TYPE.HULLSTRENGTH:
                    return 1f + (0.25f * level);
                //----------------------------------------------------------------------------------------------------//
                case FACILITY_TYPE.ROTATESPEEDINCREASE:
                case FACILITY_TYPE.CONTAINERIMPROVE:
                case FACILITY_TYPE.BOOSTIMPROVE:
                case FACILITY_TYPE.REPAIRERIMPROVE:
                    return 1f + (0.1f * level);
                //----------------------------------------------------------------------------------------------------//
                case FACILITY_TYPE.AMMODISCOUNT:
                    return 1f - (0.05f * level);
                //----------------------------------------------------------------------------------------------------//
                case FACILITY_TYPE.COREREPAIRSREGEN:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException(nameof(facilityType), facilityType, null);
            }
        }
        

        #endregion //Process Facility Values
        
        //Playing Sounds
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

        //Effects
        //====================================================================================================================//

        
        private FlashSprite GetAlertIcon(Part part)
        {
            if(_flashes == null)
                _flashes = new Dictionary<Part, FlashSprite>();

            if (_flashes.ContainsKey(part))
                return _flashes[part];

            
            var burnType = FactoryManager.Instance.PartsRemoteData.GetRemoteData(part.Type).burnType;
            var bitColor = FactoryManager.Instance.GetFactory<BitAttachableFactory>().GetBitProfile(burnType).color;

            var flash = FlashSprite.Create(part.transform, Vector3.zero, bitColor);

            _flashes.Add(part, flash);

            return _flashes[part];
        }

        private void CreateTurretEffect(in Part part)
        {
            if(_turrets.IsNullOrEmpty())
                _turrets = new Dictionary<Part, Transform>();

            if (_turrets.ContainsKey(part))
                return;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.GUN);
            
            effect.transform.SetParent(part.transform, false);
            
            _turrets.Add(part, effect.transform);
        }

        private void CreateBombEffect(in Part part, in float range)
        {
            Color startColor;

            switch (part.Type)
            {
                case PART_TYPE.BOMB:
                    startColor = Color.red;
                    break;
                case PART_TYPE.FREEZE:
                    startColor = Color.cyan;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(part.Type), part.Type, null);
            }

            var endColor = startColor;
            endColor.a = 0f;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.BOMB);

            effect.transform.position = part.transform.position;
            
            var effectAnimationComponent = effect.GetComponent<ScaleColorSpriteAnimation>();
            
            effectAnimationComponent.SetAllElementColors(startColor, endColor);
            effectAnimationComponent.SetAllElementScales(Vector2.one * 0.2f, Vector2.one * range);
            
            Destroy(effect, effectAnimationComponent.AnimationTime);
        }

        private void CleanEffects()
        {
            if (!_turrets.IsNullOrEmpty())
            {
                var turrets = _turrets.Values;
                foreach (var turret in turrets)
                {
                    Destroy(turret.gameObject);
                }
            }

        }

        //Coroutines
        //============================================================================================================//

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
