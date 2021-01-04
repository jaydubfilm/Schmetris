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
using StarSalvager.Utilities.Debugging;
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

        public List<BIT_TYPE> CurrentlyUsedBitTypes => _currentlyUsedBitTypes;
        private List<BIT_TYPE> _currentlyUsedBitTypes = new List<BIT_TYPE>();

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

        /*[SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        public float coreHeat;

        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("/s", Overlay = true)]
        private float coolSpeed;

        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("s", Overlay = true)]
        private float coolDelay;

        [ShowInInspector, BoxGroup("Bot Part Data"), ReadOnly]
        private float _coreCoolTimer;*/

        [ShowInInspector, BoxGroup("Bot Part Data"), ReadOnly]
        public int MagnetCount { get; private set; }
        private int _magnetOverride;

        //==============================================================================================================//
        
        private Dictionary<Part, Transform> _turrets;
        private Dictionary<Part, CollidableBase> _gunTargets;
        
        private List<Part> _parts;
        private List<Part> _smartWeapons;
        private int _maxSmartWeapons;

        private Dictionary<Part, bool> _playingSounds;

        private Dictionary<Part, float> _projectileTimers;
        private Dictionary<Part, ShieldData> _shields;
        private Dictionary<Part, FlashSprite> _flashes;
        private Dictionary<Part, float> _bombTimers;

        private Dictionary<Part, Asteroid> _asteroidTargets;
        private Dictionary<Part, SpaceJunk> _spaceJunkTargets;

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
        
        /*public void AddCoreHeat(float amount)
        {
            coreHeat += amount;
            _coreCoolTimer = coolDelay;
        }*/

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

            _gunTargets = new Dictionary<Part, CollidableBase>();
            _repairTarget = new Dictionary<Part, Part>();

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
            
            TryClearPartDictionaries();
            CheckShouldRecycleEffects();

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

                var levelData = partData.levels[0];

                if (levelData.burnRate > 0 && !usedResourceTypes.Contains(partData.burnType))
                    usedResourceTypes.Add(partData.burnType);

                if(levelData.powerDraw > 0f && !usedResourceTypes.Contains(BIT_TYPE.YELLOW))
                    usedResourceTypes.Add(BIT_TYPE.YELLOW);

                if (part.Type == PART_TYPE.REFINER)
                {
                    if(!usedResourceTypes.Contains(BIT_TYPE.BLUE))
                        usedResourceTypes.Add(BIT_TYPE.BLUE);
                    if(!usedResourceTypes.Contains(BIT_TYPE.GREY))
                        usedResourceTypes.Add(BIT_TYPE.GREY);
                    if(!usedResourceTypes.Contains(BIT_TYPE.GREEN))
                        usedResourceTypes.Add(BIT_TYPE.GREEN);
                    if(!usedResourceTypes.Contains(BIT_TYPE.YELLOW))
                        usedResourceTypes.Add(BIT_TYPE.YELLOW);
                }

                //Destroyed or disabled parts should not contribute to the stats of the bot anymore
                if (part.Disabled)
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
                        if (partData.levels[0].TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
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
                            liquidCapacities[BIT_TYPE.RED] += value;
                            liquidCapacities[BIT_TYPE.GREEN] += value;
                            liquidCapacities[BIT_TYPE.GREY] += value;
                        }

                        break;
                    case PART_TYPE.STORERED:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.RED] += value;
                        }

                        break;
                    case PART_TYPE.STOREGREEN:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.GREEN] += value;
                        }

                        break;
                    case PART_TYPE.STOREGREY:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.GREY] += value;
                        }

                        break;
                    case PART_TYPE.STOREYELLOW:
                        if (levelData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
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
                    
                    case PART_TYPE.REPAIR:
                        _repairTarget.Add(part, null);
                        break;

                    case PART_TYPE.GUN:
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.MISSILE:
                        
                        _gunTargets.Add(part, null);
                        
                        if (ShouldUseGunTurret(levelData))
                            CreateTurretEffect(part);
                        break;
                    
                    case PART_TYPE.BOOSTRATE:
                        CreateBoostRateEffect(part);
                        break;
                }
            }

            //SetupHealthBoots();
            SetupGunRangeValues();

            //Force update capacities, once new values determined
            foreach (var capacity in liquidCapacities)
            {
                PlayerDataManager.GetResource(capacity.Key).SetLiquidCapacity(capacity.Value);
            }

            if (!_turrets.IsNullOrEmpty())
            {
                foreach (var kvp in _turrets)
                {
                    var turret = kvp.Value.gameObject;
                    turret.GetComponent<SpriteRenderer>().color = kvp.Key.Disabled ? Color.gray : Color.white;
                }
            }
            
            if (!_boostEffects.IsNullOrEmpty())
            {
                var keys = new List<Part>(_boostEffects.Keys);
                foreach (var key in keys.Where(key => key.Disabled/* || key.Destroyed*/))
                {
                    Destroy(_boostEffects[key]);
                    _boostEffects.Remove(key);
                }
            }


            bot.ForceCheckMagnets();
            _currentlyUsedBitTypes = usedResourceTypes;
            GameUI.ShowLiquidSliders(usedResourceTypes);
        }
        
        /*private void SetupHealthBoots()
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
        }*/

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
            ref float powerValue,
            ref float powerToRemove,
            in float deltaTime)
        {
            //If the part doesn't need power, continue
            if (partLevelData.powerDraw <= 0f)
                return true;
            
            //Check to see if we want to power, that we have some to process
            if (powerValue == 0f)
            {
                var targetBit = GetFurthestBitToBurn(BIT_TYPE.YELLOW);

                if (targetBit != null)
                {
                    powerValue = ProcessBit(part, targetBit);
                }
            }

            switch (part.Disabled)
            {
                //----------------------------------------------------------------------------------------------------//
                //If the part we have is currently disabled, and we have no stored power, return false
                case true when powerValue == 0f:
                    return false;
                //----------------------------------------------------------------------------------------------------//
                //If the part is disabled, it needs power, and we have some stored, be sure to reEnable it
                //FIXME THis shouldn't happen often, though I may want to reconsider how this is being approached
                case true when powerValue > 0f:
                    part.Disabled = false;
                    InitPartData();
                    break;
                //----------------------------------------------------------------------------------------------------//
                //If the part is Enabled and it requires power but we have none to give it
                case false when powerValue == 0f:
                    part.Disabled = true;
                    InitPartData();
                    return false;
                //----------------------------------------------------------------------------------------------------//
            }

            powerToRemove += partLevelData.powerDraw * deltaTime;

            return true;
        }

        private bool ShouldUpdateResourceUsage(in Part part, in PartRemoteData partRemoteData, in PartLevelData partLevelData, out float resourceValue)
        {
            resourceValue = default;
            
            //If there's nothing using these resources ignore
            if(partLevelData.burnRate == 0f || (int)partRemoteData.burnType == 0)
                return false;

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
                    resourceValue = ProcessBit(part, targetBit);
                }

            }
            else
            {
                //FIXME I don't like how often this is called, will need to rethink this
                //Hide the icon for part if it exists
                if(_flashes != null && _flashes.ContainsKey(part))
                    GetAlertIcon(part).SetActive(false);
            }

            return true;
        }

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Parts specific update Loop. Updates all part information based on currently attached parts.
        /// </summary>
        public void PartsUpdateLoop()
        {

            var deltaTime = Time.deltaTime;

            var powerValue = PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquid;
            var powerToRemove = 0f;

            //Be careful to not use return here
            foreach (var part in _parts)
            {
                /*if(part.Destroyed)
                    continue;*/
                
                var (partRemoteData, levelData) = GetPartData(part);

                if(!TryUpdatePowerUsage(part, levelData, ref powerValue, ref powerToRemove, deltaTime))
                    continue;

                var shouldUpdateResource =
                    ShouldUpdateResourceUsage(part, partRemoteData, levelData, out var resourceValue);

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

                if(!bot.CanUseResources)
                    continue;

                if (!shouldUpdateResource)
                    continue;
                
                //UpdateUI(partRemoteData.burnType, resourceValue);
                PlayerDataManager.GetResource(partRemoteData.burnType).SetLiquid(resourceValue, false);

                if(resourcesConsumed > 0)
                    LevelManager.Instance.WaveEndSummaryData.AddConsumedBit(partRemoteData.burnType, resourcesConsumed);
            }

            TryRemovePowerResource(powerValue, powerToRemove, deltaTime);
            LevelManager.Instance.WaveEndSummaryData.AddConsumedBit(BIT_TYPE.YELLOW, powerToRemove);

            //UpdateUI(BIT_TYPE.YELLOW, PlayerDataManager.GetResource(BIT_TYPE.YELLOW).liquid);
            //UpdateUI(BIT_TYPE.BLUE, PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource);

            UpdateAllUI();
        }

        private (PartRemoteData partRemoteData, PartLevelData partLevelData) GetPartData(in Part part)
        {
            var partRemoteData = _partAttachableFactory.GetRemoteData(part.Type);
            var partLevelData = partRemoteData.levels[0];

            return (partRemoteData, partLevelData);
        }

        private void TryRemovePowerResource(float powerValue, float powerToRemove, in float deltaTime)
        {
            if (!bot.CanUseResources) 
                return;
            
            powerValue -= powerToRemove;
            if (powerValue < 0)
                powerValue = 0f;

            PlayerDataManager.GetResource(BIT_TYPE.YELLOW).SetLiquid(powerValue, false);


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

            /*//TODO Need to check on Heating values for the core
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

            coreHeat -= coolSpeed * deltaTime;

            if (coreHeat > 0)
                return;

            coreHeat = 0;
            part.SetColor(Color.white);*/
        }

        private void RepairUpdate(in Part part, in PartLevelData partLevelData, ref float resourceValue,
            ref float resourcesConsumed, in float deltaTime)
        {
            /*if (resourceValue <= 0f && useBurnRate)
            {
                //TODO Need to play the no resources for repair sound here
                return;
            }
            var radius = partLevelData.GetDataValue<int>(DataTest.TEST_KEYS.Radius);


            var repairTarget = _repairTarget[part];


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

            //Repair Effect Confirm
            //--------------------------------------------------------------------------------------------------------//
            
            if (repairTarget && repairTarget != (Part) toRepair)
            {
                _repairEffects[repairTarget].SetActive(false);
            }
            
            //--------------------------------------------------------------------------------------------------------//

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

            //Repair Effect Setup
            //--------------------------------------------------------------------------------------------------------//
            
            var partToRepair = (Part) toRepair;

            if (repairTarget != partToRepair)
            {
                _repairTarget[part] = partToRepair;
                
                if (_repairEffects.IsNullOrEmpty())
                    _repairEffects = new Dictionary<Part, GameObject>();
            
                if (!_repairEffects.TryGetValue(partToRepair, out var effectObject))
                {
                    CreateRepairEffect(partToRepair);
                }
                else
                {
                    effectObject.SetActive(true);
                }
            }
            //--------------------------------------------------------------------------------------------------------//
            
            resourcesConsumed = partLevelData.burnRate * deltaTime;
            resourceValue -= resourcesConsumed;

            var repairAmount = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Heal);
            repairAmount *= GetBoostValue(PART_TYPE.BOOSTRATE, part);

            //FIXME This will need some sort of time cooldown
            //AudioController.PlaySound(SOUND.REPAIRER_PULSE);

            //Increase the health of this part depending on the current level of the repairer
            toRepair.ChangeHealth(repairAmount * deltaTime);
            PlayerDataManager.AddRepairsDone(repairAmount * deltaTime);

            
            //Update the UI if the thing we're repairing is the core
            if (partToRepair && partToRepair.Type == PART_TYPE.CORE)
            {
                GameUI.SetHealthValue(partToRepair.CurrentHealth / partToRepair.BoostedHealth);
            }


            TryPlaySound(part, SOUND.REPAIRER_PULSE, toRepair.CurrentHealth < toRepair.BoostedHealth);*/
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
                    resourcesConsumed = partLevelData.burnRate;
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

            if (target && 
                target.IsRecycled == false && 
                !_turrets.IsNullOrEmpty() &&
                _turrets.TryGetValue(part, out var turretTransform))
            {
                var targetTransform = target.transform;
                var normDirection = (targetTransform.position - part.transform.position).normalized;
                turretTransform.up = normDirection;
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

            if (!_gunRanges.TryGetValue(part, out var range))
            {
                range = 150f;
            }
            
            //TODO: Make us able to pass multiple tags so a shot can hit multiple types of targets
            CollidableBase fireTarget = EnemyManager.GetClosestEnemy(part.transform.position, range);
            string tag = "Enemy";
            //TODO Determine if this fires at all times or just when there are active enemies in range
            if (fireTarget == null)
            {
                fireTarget = LevelManager.Instance.ObstacleManager.GetClosestDestructableCollidable(part.transform.position, range);
                if (fireTarget == null)
                {
                    return;
                }
                else if (fireTarget is SpaceJunk)
                {
                    tag = "Space Junk";
                }
            }

            _gunTargets[part] = fireTarget;


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
                    resourcesConsumed = partLevelData.burnRate;
                    resourceValue -= resourcesConsumed;
                }
            }

            switch (part.Type)
            {
                case PART_TYPE.GUN:
                case PART_TYPE.TRIPLESHOT:
                case PART_TYPE.MISSILE:
                    CreateProjectile(part, partLevelData, fireTarget, tag);
                    break;
                case PART_TYPE.SNIPER:
                    var direction = (fireTarget.transform.position + ((Vector3) Random.insideUnitCircle * 3) -
                                     part.transform.position).normalized;

                    var lineShrink = FactoryManager.Instance.GetFactory<EffectFactory>()
                        .CreateObject<LineShrink>();

                    var chance = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Probability);
                    var didHitTarget = Random.value <= chance;


                    lineShrink.Init(part.transform.position,
                        didHitTarget
                            ? fireTarget.transform.position
                            : part.transform.position + direction * 100);

                    if (didHitTarget)
                    {
                        var damage = partLevelData.GetDataValue<float>(DataTest.TEST_KEYS.Damage);
                        if (fireTarget is ICanBeHit iCanBeHit)
                        {
                            iCanBeHit.TryHitAt(target.transform.position, damage);
                        }
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
                if (/*part.Destroyed || */part.Disabled)
                    continue;

                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);

                switch (part.Type)
                {
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.MISSILE:
                    case PART_TYPE.GUN:

                        var projectileID = partData.levels[0]
                            .GetDataValue<string>(DataTest.TEST_KEYS.Projectile);

                        _gunRanges.Add(part, GetProjectileRange(part, projectileID));

                        break;
                }
            }
        }

        private void CreateProjectile(in Part part, in PartLevelData levelData, in CollidableBase collidableTarget, string collisionTag = "Enemy")
        {
            var projectileId = levelData.GetDataValue<string>(DataTest.TEST_KEYS.Projectile);
            var damage = levelData.GetDataValue<float>(DataTest.TEST_KEYS.Damage);
            damage *= GetBoostValue(PART_TYPE.BOOSTDAMAGE, part);

            var rangeBoost = GetBoostValue(PART_TYPE.BOOSTRANGE, part);

            var position = part.transform.position;
            var shootDirection = ShouldUseGunTurret(levelData)
                ? GetAimedProjectileAngle(collidableTarget, part, projectileId)
                : part.transform.up.normalized;

            //TODO Might need to add something to change the projectile used for each gun piece
            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    projectileId,
                    position,
                    collidableTarget,
                    shootDirection,
                    damage,
                    rangeBoost,
                    collisionTag,
                    true);
        }

        private Vector3 GetAimedProjectileAngle(CollidableBase target, Part part, string projectileId)
        {
            Vector3 targetVelocity;
            switch(target)
            {
                case Enemy enemy:
                    targetVelocity = enemy.EnemyMovementSpeed * enemy.m_mostRecentMovementDirection;
                    break;
                default:
                    targetVelocity = Vector3.zero;
                    break;
            }
            var projectileProfile = FactoryManager.Instance.GetFactory<ProjectileFactory>().GetProfileData(projectileId);
            
            Vector3 totarget = target.transform.position - part.transform.position;

            float a = Vector3.Dot(targetVelocity, targetVelocity) - (projectileProfile.ProjectileSpeed * projectileProfile.ProjectileSpeed);
            float b = 2 * Vector3.Dot(targetVelocity, totarget);
            float c = Vector3.Dot(totarget, totarget);

            float p = -b / (2 * a);
            float q = (float)Math.Sqrt((b * b) - 4 * a * c) / (2 * a);

            float t1 = p - q;
            float t2 = p + q;
            float t;

            if (t1 > t2 && t2 > 0)
            {
                t = t2;
            }
            else
            {
                t = t1;
            }

            Vector3 aimSpot = target.transform.position + targetVelocity * t;
            Vector3 bulletPath = aimSpot - part.transform.position;

            return bulletPath;
            //float timeToImpact = bulletPath.Length() / bullet.speed;//speed must be in units per second
        }

        private float GetProjectileRange(in Part part, in string projectileID)
        {
            var projectileData = FactoryManager.Instance.GetFactory<ProjectileFactory>().GetProfileData(projectileID);

            var range = projectileData.ProjectileRange;

            if (range == 0f)
                return 100 * Constants.gridCellSize;

            var rangeBoost = GetBoostValue(PART_TYPE.BOOSTRANGE, part);

            return range * rangeBoost;
        }

        private static bool ShouldUseGunTurret(in PartLevelData levelData)
        {
            var projectileId = levelData.GetDataValue<string>(DataTest.TEST_KEYS.Projectile);
            var projectileData = FactoryManager.Instance.GetFactory<ProjectileFactory>().GetProfileData(projectileId);

            return !(projectileData is null) && projectileData.FireAtTarget;
        }

        #endregion //Weapons

        //============================================================================================================//

        #region Smart Weapons

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

            var partLevelData = partData.levels[0];

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

            var partLevelData = partData.levels[0];

            //Set the cooldown time
            if (partLevelData.TryGetValue(DataTest.TEST_KEYS.Cooldown, out float cooldown))
            {
                _bombTimers[part] = cooldown;
            }

            partLevelData.TryGetValue(DataTest.TEST_KEYS.Radius, out int radius);
            partLevelData.TryGetValue(DataTest.TEST_KEYS.Time, out float freezeTime);

            var enemies = EnemyManager.GetEnemiesInRange(part.transform.position, radius);

            foreach (var enemy in enemies)
            {
                enemy.SetFrozen(freezeTime);
            }

            //Need to pass the diameter not the radius
            CreateFreezeEffect(part, radius * 2f);
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

            UpdateAllUI();

            /*foreach (BIT_TYPE _bitType in Enum.GetValues(typeof(BIT_TYPE)))
            {
                if (_bitType == BIT_TYPE.WHITE || _bitType == BIT_TYPE.NONE)
                    continue;

                UpdateUI(_bitType, PlayerDataManager.GetResource(_bitType).liquid);
            }

            //UpdateUI(BIT_TYPE.YELLOW, PlayerPersistentData.PlayerData.li[BIT_TYPE.YELLOW]);
            UpdateUI(BIT_TYPE.BLUE, PlayerDataManager.GetResource(BIT_TYPE.BLUE).resource);*/
        }

        private void UpdateAllUI()
        {
            var resources = PlayerDataManager.GetResources();

            foreach (var resource in resources)
            {
                if(!CurrentlyUsedBitTypes.Contains(resource.BitType))
                    continue;

                UpdateUI(resource.BitType, resource.liquid);
            }
            
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

        
        /// <summary>
        /// Checks to see if the current liquid is less than or equal to valueToCheck
        /// </summary>
        /// <param name="part"></param>
        /// <param name="targetBit"></param>
        /// <param name="valueToCheck"></param>
        /// <returns></returns>
        public int ProcessBit(in Part part, Bit targetBit, float valueToCheck)
        {
            PlayerResource playerResource = PlayerDataManager.GetResource(targetBit.Type);
            var current = playerResource.liquid;

            return current > valueToCheck ? 0 : ProcessBit(part, targetBit);
        }

        public int ProcessBit(in Part part, Bit targetBit)
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
            var current = playerResource.liquid;
            var capacity = playerResource.liquidCapacity;

            //We wont add any if its already full!
            /*if (current + amountProcessed > capacity)
                return 0;*/

            if (current == capacity)
                return 0;

            //Get a list of orphans that may need move when we are moving our bits
            var orphans = new List<OrphanMoveData>();
            bot.attachedBlocks.CheckForOrphansFromProcessing(
                targetBit,
                ref orphans);
            
            if(!orphans.IsNullOrEmpty())
                bot.MoveOrphanPieces(orphans, () =>
                {
                    bot.CheckAllForCombos();
                });

            PlayerDataManager.GetResource(targetBit.Type).AddLiquid(amountProcessed);
            targetBit.IsBusy = true;

            //If we want to process a bit, we want to remove it from the attached list while its processed
            bot.MarkAttachablePendingRemoval(targetBit);

            //TODO May want to play around with the order of operations here
            StartCoroutine(RefineBitCoroutine(targetBit,
                part.transform,
                orphans,
                1.6f,
                () =>
                {
                    bot.DestroyAttachable<Bit>(targetBit);
                }));


            SessionDataProcessor.Instance.LiquidProcessed(targetBit.Type, amountProcessed);
            AudioController.PlaySound(SOUND.BIT_REFINED);
            bot.ForceCheckMagnets();
            
            if (part.Type == PART_TYPE.REFINER)
            {
                CreateRefinerEffect(part, bitType);
            }

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
        private float GetBoostValue(PART_TYPE boostPart, in Part fromPart)
        {
            var boosts = _parts
                .Where(x => !x.Disabled /*&& !x.Destroyed*/ && x.Type == boostPart)
                .ToList();

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
            foreach (var levelData in beside.Select(part => partRemoteData.levels[0]))
            {
                if (!levelData.TryGetValue(DataTest.TEST_KEYS.Multiplier, out float mult))
                    continue;

                if (mult > maxBoost)
                    maxBoost = mult;
            }

            return maxBoost;
        }

        /*private float GetDefenseBoost(in Part part)
        {
            if (part.Type != PART_TYPE.BOOSTDEFENSE)
                return 0f;

            if (/*part.Destroyed ||#1# part.Disabled)
                return 0f;

            PartRemoteData partRemoteData =
                FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type);
            
            return partRemoteData.levels[0].GetDataValue<float>(DataTest.TEST_KEYS.Absorb) *
                   GetFacilityImprovement(FACILITY_TYPE.BOOSTIMPROVE);

        }*/

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

        public void ClearList()
        {
            TryClearPartDictionaries();
            CleanEffects();

            _parts.Clear();
        }
        
        private static void CheckShouldRecycle<T>(ref Dictionary<Part, T> partDictionary, Action<T> OnRecycleCallback)
        {
            if (partDictionary.IsNullOrEmpty())
                return;

            var copy = new Dictionary<Part, T>(partDictionary);
            foreach (var data in copy.Where(data => data.Key.IsRecycled /*|| data.Key.Destroyed*/))
            {
                OnRecycleCallback?.Invoke(data.Value);
                
                partDictionary.Remove(data.Key);
            }
        }
        private static void CheckShouldRecycle<T>(ref Dictionary<Part, T> partDictionary, Action<Part> OnRecycleCallback)
        {
            if (partDictionary.IsNullOrEmpty())
                return;

            var copy = new Dictionary<Part, T>(partDictionary);
            foreach (var data in copy.Where(data => data.Key.IsRecycled /*|| data.Key.Destroyed*/))
            {
                OnRecycleCallback?.Invoke(data.Key);
                
                partDictionary.Remove(data.Key);
            }
        }

        #endregion //Part Dictionary Recycling
        
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

        #region Effects

        private Dictionary<Part, GameObject> _boostEffects;
        private Dictionary<Part, GameObject> _repairEffects;
        private Dictionary<Part, Part> _repairTarget;
        
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

        //FIXME The turret setup feels shit, need to clean this
        private void CreateTurretEffect(in Part part)
        {
            if(_turrets.IsNullOrEmpty())
                _turrets = new Dictionary<Part, Transform>();

            if (_turrets.ContainsKey(part))
                return;

            var partEffect = part.Type == PART_TYPE.TRIPLESHOT
                ? EffectFactory.PART_EFFECT.TRIPLE_SHOT
                : EffectFactory.PART_EFFECT.GUN;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(partEffect);
            
            effect.transform.SetParent(part.transform, false);
            
            _turrets.Add(part, effect.transform);
        }

        private void CreateBombEffect(in Part part, in float range)
        {
           
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.BOMB);

            effect.transform.position = part.transform.position;
            
            var effectAnimationComponent = effect.GetComponent<ParticleSystemGroupScaling>();
            
            effectAnimationComponent.SetSimulationSize(range);
            
            Destroy(effect, effectAnimationComponent.AnimationTime);
        }
        
        private void CreateFreezeEffect(in Part part, in float range)
        {
           
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.FREEZE);

            effect.transform.position = part.transform.position;
            
            var effectAnimationComponent = effect.GetComponent<ParticleSystemGroupScaling>();
            
            effectAnimationComponent.SetSimulationSize(range);
            
            Destroy(effect, effectAnimationComponent.AnimationTime);
        }

        private void CreateRefinerEffect(in Part part, in BIT_TYPE bitType)
        {
            var endColor = FactoryManager.Instance.BitProfileData.GetProfile(bitType).color;
            var startColor = endColor;
            startColor.a = 0f;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.REFINER);
            var effectComponent = effect.GetComponent<ScaleColorSpriteAnimation>();
            
            effect.transform.SetParent(part.transform, false);
            
            effectComponent.SetAllElementColors(startColor, endColor);
            
            Destroy(effect, effectComponent.AnimationTime);
        }

        private void CreateRepairEffect(in Part part)
        {
            if(_repairEffects.IsNullOrEmpty())
                _repairEffects = new Dictionary<Part, GameObject>();

            if (_repairEffects.ContainsKey(part))
                return;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.REPAIR);
            
            effect.transform.SetParent(part.transform, false);
            
            _repairEffects.Add(part, effect);
        }
        
        private void CreateBoostRateEffect(in Part part)
        {
            if(_boostEffects.IsNullOrEmpty())
                _boostEffects = new Dictionary<Part, GameObject>();

            if (_boostEffects.ContainsKey(part))
                return;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.RATE_BOOST);
            
            effect.transform.SetParent(part.transform, false);
            
            _boostEffects.Add(part, effect);
        }

        private void CleanEffects()
        {
            if (!_turrets.IsNullOrEmpty())
            {
                var turrets = _turrets.Values;
                foreach (var turret in turrets)
                {
                    if(!turret)
                        continue;
                    
                    Destroy(turret.gameObject);
                }
                
                _turrets = new Dictionary<Part, Transform>();
            }

            if (!_repairEffects.IsNullOrEmpty())
            {
                var repairs = _repairEffects.Values;

                foreach (var repair in repairs)
                {
                    Destroy(repair);
                }
                _repairEffects = new Dictionary<Part, GameObject>();
            }
            
            if (!_boostEffects.IsNullOrEmpty())
            {
                var effectsValues = _boostEffects.Values;

                foreach (var boost in effectsValues)
                {
                    Destroy(boost);
                }
                
                _boostEffects = new Dictionary<Part, GameObject>();
            }

        }

        private void CheckShouldRecycleEffects()
        {
            CheckShouldRecycle(ref _turrets, (Transform data) =>
            {
                Destroy(data.gameObject);
            });
            
            CheckShouldRecycle(ref _repairEffects, (GameObject data) =>
            {
                Destroy(data.gameObject);
            });
            CheckShouldRecycle(ref _boostEffects, (GameObject data) =>
            {
                Destroy(data.gameObject);
            });
        }

        #endregion //Effects

        //Coroutines
        //============================================================================================================//

        private IEnumerator RefineBitCoroutine(Bit bit, Transform processToTranform, IReadOnlyList<OrphanMoveData> orphans, float speed, Action onFinishedCallback)
        {
            var bitStartPosition = bit.transform.localPosition;
            var endPosition = processToTranform.localPosition;
            var t = 0f;

            bit.SetColliderActive(false);
            bit.Coordinate = Vector2Int.zero;
            bit.renderer.sortingOrder = 10000;

            foreach (var omd in orphans)
            {
                omd.attachableBase.Coordinate = omd.intendedCoordinates;
                (omd.attachableBase as Bit)?.SetColliderActive(false);

                if (omd.attachableBase is ICanCombo iCanCombo)
                    iCanCombo.IsBusy = true;
            }

            //Same as above but for Orphans
            //--------------------------------------------------------------------------------------------------------//

            /*var orphanTransforms = orphans.Select(bt => bt.attachableBase.transform).ToArray();
            var orphanTransformPositions = orphanTransforms.Select(bt => bt.localPosition).ToArray();
            var orphanTargetPositions = orphans.Select(o =>
                transform.InverseTransformPoint((Vector2)transform.position +
                                                (Vector2)o.intendedCoordinates * Constants.gridCellSize)).ToArray();*/
            //--------------------------------------------------------------------------------------------------------//

            while (t < 1f)
            {
                bit.transform.localPosition = Vector3.Lerp(bitStartPosition, endPosition, t);

                //TODO Need to adjust the scale here
                bit.transform.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, refineScaleCurve.Evaluate(t));


                //Move the orphans into their new positions
                //----------------------------------------------------------------------------------------------------//

                /*for (var i = 0; i < orphans.Count; i++)
                {
                    var bitTransform = orphanTransforms[i];

                    //Debug.Log($"Start {bitTransform.position} End {position}");

                    bitTransform.localPosition = Vector2.Lerp(orphanTransformPositions[i],
                        orphanTargetPositions[i], t);

                    SSDebug.DrawArrow(bitTransform.position, transform.TransformPoint(orphanTargetPositions[i]), Color.red);
                }*/

                t += Time.deltaTime * speed * moveSpeedCurve.Evaluate(t);

                yield return null;
            }

            //Re-enable the colliders on our orphans, and ensure they're in the correct position
            /*for (var i = 0; i < orphans.Count; i++)
            {
                var attachable = orphans[i].attachableBase;
                orphanTransforms[i].localPosition = orphanTargetPositions[i];

                if (attachable is CollidableBase collidableBase)
                    collidableBase.SetColliderActive(true);

                if (attachable is ICanCombo canCombo)
                    canCombo.IsBusy = false;
            }*/

            onFinishedCallback?.Invoke();
            bit.transform.localScale = Vector3.one;


        }

        //============================================================================================================//

    }
}
