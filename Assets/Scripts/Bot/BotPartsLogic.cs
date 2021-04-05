using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using StarSalvager.Prototype;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.Parts.Data;
using StarSalvager.Utilities.Saving;
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
        private static PART_TYPE[] TriggerPartTypes
        {
            get
            {
                if(_triggerPartTypes.IsNullOrEmpty())
                    _triggerPartTypes = FactoryManager.Instance.PartsRemoteData.GetTriggerParts();

                return _triggerPartTypes;
            }
        }

        private static PART_TYPE[] _triggerPartTypes;
        
        #region Properties

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

        //Magnet Properties
        //====================================================================================================================//
        
        [SerializeField, BoxGroup("Magnets")] public bool useMagnet = true;
        [SerializeField, BoxGroup("Magnets")] public MAGNET currentMagnet = MAGNET.DEFAULT;

        //==============================================================================================================//

        [ShowInInspector, BoxGroup("Bot Part Data"), ReadOnly]
        public int MagnetCount { get; private set; }
        //private int _magnetOverride;

        //==============================================================================================================//

        private bool _shieldActive;

        private GameObject _shieldObject;
        private GameObject _healObject;
        private float _healActiveTimer;

        
        private Dictionary<Part, Transform> _turrets;
        private Dictionary<Part, CollidableBase> _gunTargets;
        
        private List<Part> _parts;
        private List<Part> _triggerParts;
        private static int MAXTriggerParts => 4;

        private Dictionary<Part, bool> _playingSounds;

        private Dictionary<Part, float> _projectileTimers;
        private Dictionary<Part, FlashSprite> _flashes;
        private Dictionary<Part, float> _triggerPartTimers;

        private Dictionary<Part, float> _repairTimers;
        private Dictionary<Part, float> _shieldTimers;
        //private Dictionary<Part, float> _vampireTimers;

        private Dictionary<Part, Asteroid> _asteroidTargets;
        private Dictionary<Part, SpaceJunk> _spaceJunkTargets;

        private Dictionary<Part, float> _gunRanges;

        private static PartAttachableFactory _partAttachableFactory;

        //Sabre Properties
        //====================================================================================================================//
        private Part _corePart;
        
        private bool _sabreActive;
        private Sabre _sabreObject;
        private Dictionary<Part, float> _sabreTimers;


        //Heal Cooldown Timer
        //====================================================================================================================//
        
        private float _healWaitTimer;

        public void ResetHealCooldown()
        {
            _healWaitTimer = Globals.BotHealWaitTime;
        }

        #endregion //Properties

        //Unity Functions
        //==============================================================================================================//

        private void Start()
        {
            _partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
        }

        private void LateUpdate()
        {
            if (!GameManager.IsState(GameState.LevelEndWave))
                return;

            if (_sabreActive && _sabreObject)
            {
                _sabreObject.SetActive(false);
                _sabreActive = false;
            }
            
        }

        //====================================================================================================================//

        public Dictionary<PART_TYPE, bool> GetPartStates()
        {
            var outData = new Dictionary<PART_TYPE, bool>();
            foreach (var part in _parts)
            {
                var partType = part.Type;
                
                if (partType == PART_TYPE.EMPTY /*|| partType == PART_TYPE.CORE*/)
                    continue;
                
                if(outData.ContainsKey(partType))
                    continue;
                
                outData.Add(partType, !part.Disabled);
            }

            return outData;
        }
        
        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        public void PopulatePartsList()
        {
            _parts = bot.AttachedBlocks.OfType<Part>().ToList();
            _triggerParts = _parts
                .Where(p =>
                    TriggerPartTypes.Contains(p.Type))
                .ToList();

            //TODO Need to update the UI here for the amount of smart weapons able to be used

            InitPartData();
        }

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void InitPartData()
        {
            
            var partRemote = FactoryManager.Instance.PartsRemoteData;
            
            void SetIcon(in int index, in BIT_TYPE bitType)
            {
                var type = bitType;
                var part = _parts.FirstOrDefault(x => x.category == type);

                GameUI.SetIconImage(index, part is null ? PART_TYPE.EMPTY : part.Type);
            }

            //--------------------------------------------------------------------------------------------------------//
            

            _gunTargets = new Dictionary<Part, CollidableBase>();
            _repairTarget = new Dictionary<Part, Bit>();

            TryClearPartDictionaries();
            CheckShouldRecycleEffects();

            //Update the Game UI for the Smart Weapons
            //--------------------------------------------------------------------------------------------------------//

            GameUI.ResetIcons();

            //Using a fixed order for the BIT_TYPES since the UI needs it in this order
            for (var i = 0; i < Constants.BIT_ORDER.Length; i++)
            {
                SetIcon(i, Constants.BIT_ORDER[i]);
            }

            //--------------------------------------------------------------------------------------------------------//

            FindObjectOfType<GameUI>();
            MagnetCount = 0;

            //int value;
            foreach (var part in _parts)
            {
                if (part.Type == PART_TYPE.EMPTY)
                {
                    continue;
                }

                var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);

                //FIXME Need to determine how things can become enabled/disabled
                //part.Disabled = !HasPartGrade(part, partRemoteData, out _);

                //Destroyed or disabled parts should not contribute to the stats of the bot anymore
                if (part.Disabled)
                    continue;

                switch (part.Type)
                {
                    case PART_TYPE.CORE:
                        var magnetAmount = partRemoteData.GetDataValue<int>(PartProperties.KEYS.Magnet);
                        var capacityAmount = partRemoteData.GetDataValue<int>(PartProperties.KEYS.Capacity);

                        MagnetCount = magnetAmount;

                        foreach (var bitType in Constants.BIT_ORDER)
                        {
                            var resource = PlayerDataManager.GetResource(bitType);
                            resource.SetAmmoCapacity(capacityAmount, false);
                        }
                        
                        PlayerDataManager.OnCapacitiesChanged?.Invoke();

                        /*if (HasPartGrade(part, partRemoteData, out var floatValue))
                        {
                            MagnetCount += (int)floatValue;
                        }*/
                        _corePart = part;
                        break;
                    case PART_TYPE.SHIELD:
                        if (_shieldTimers == null)
                            _shieldTimers = new Dictionary<Part, float>();

                        if (_shieldTimers.ContainsKey(part))
                            break;
                        
                        _shieldTimers.Add(part, 0f);                        
                        break;
                    /*case PART_TYPE.VAMPIRE:
                        if (_vampireTimers == null)
                            _vampireTimers = new Dictionary<Part, float>();

                        if (_vampireTimers.ContainsKey(part))
                            break;
                        
                        _vampireTimers.Add(part, 0f);                        
                        break;*/
                    
                    case PART_TYPE.REPAIR:
                        if (_repairTimers == null)
                            _repairTimers = new Dictionary<Part, float>();
                        
                        if (!_repairTimers.ContainsKey(part))
                            _repairTimers.Add(part, 0f);
                        
                        _repairTarget.Add(part, null);
                        break;
                    case PART_TYPE.GUN:
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.MISSILE:
                        
                        _gunTargets.Add(part, null);
                        
                        if (Globals.UseCenterFiring == false && ShouldUseGunTurret(partRemoteData))
                            CreateTurretEffect(part);
                        break;
                    
                    case PART_TYPE.SABRE:
                        if (_sabreTimers == null)
                            _sabreTimers = new Dictionary<Part, float>();

                        if (_sabreTimers.ContainsKey(part))
                            break;
                        
                        _sabreTimers.Add(part, 0f);                        
                        break;
                }
            }

            if (_triggerPartTimers == null)
                _triggerPartTimers = new Dictionary<Part, float>();
            
            foreach (var triggerPart in _triggerParts)
            {
                if (_triggerPartTimers.ContainsKey(triggerPart))
                    continue;
                
                _triggerPartTimers.Add(triggerPart, 0f);
            }

            SetupGunRangeValues();

            if (!_turrets.IsNullOrEmpty())
            {
                foreach (var kvp in _turrets)
                {
                    var turret = kvp.Value.gameObject;
                    turret.GetComponent<SpriteRenderer>().color = kvp.Key.Disabled ? Color.gray : Color.white;
                }
            }

            bot.ForceCheckMagnets();
        }

        //Parts Update Loop
        //============================================================================================================//

        //FIXME I Will want to separate these functions as this is getting too large
        /// <summary>
        /// Parts specific update Loop. Updates all part information based on currently attached parts.
        /// </summary>
        public void PartsUpdateLoop()
        {
            var deltaTime = Time.deltaTime;

            //Be careful to not use return here
            foreach (var part in _parts)
            {
                if (part.Disabled)
                    continue;
                
                var partRemoteData = GetPartData(part);
                
                switch (part.Type)
                {
                    //------------------------------------------------------------------------------------------------//
                    case PART_TYPE.REPAIR:
                        RepairUpdate(part, partRemoteData, deltaTime);
                        break;
                    //------------------------------------------------------------------------------------------------//
                    case PART_TYPE.BLASTER:
                        BlasterUpdate(part, partRemoteData, deltaTime);
                        break;
                    //------------------------------------------------------------------------------------------------//
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.MISSILE:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.GUN:
                        GunUpdate(part, partRemoteData, deltaTime);
                        break;
                    //------------------------------------------------------------------------------------------------//
                    case PART_TYPE.SHIELD:
                        ShieldUpdate(part, partRemoteData, deltaTime);
                        break;
                    //------------------------------------------------------------------------------------------------//
                    case PART_TYPE.REGEN:
                        RegenUpdate(part, partRemoteData, deltaTime);
                        break;
                    //------------------------------------------------------------------------------------------------//
                    case PART_TYPE.CORE:
                        HealUpdate(part, partRemoteData, deltaTime);
                        break;

                    //--------------------------------------------------------------------------------------------------------//
                    case PART_TYPE.SABRE:
                        SabreUpdate(part, partRemoteData, deltaTime);
                        break;
                }
            }

            foreach (var triggerPart in _triggerParts)
            {
                var partRemoteData = GetPartData(triggerPart);
                TriggerPartUpdates(triggerPart, partRemoteData, deltaTime);
            }
        }

        private PartRemoteData GetPartData(in Part part)
        {
            var partRemoteData = _partAttachableFactory.GetRemoteData(part.Type);

            return partRemoteData;
        }

        //Individual Part Functions
        //====================================================================================================================//
        
        #region Part Updates

        //FIXME Need to restructure this to only trigger after cooldown time
        private void RepairUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            var timer = _repairTimers[part];

            if (timer > 0f)
            {
                timer -= deltaTime;
                _repairTimers[part] = timer;

                return;
            }


            var repairTarget = _repairTarget[part];


            //FIXME I don't think using linq here, especially twice is the best option
            //TODO This needs to fire every x Seconds
            IHealth toRepair = bot.AttachedBlocks.OfType<Bit>()
                .Where(p => p.CurrentHealth < p.StartingHealth)
                .Select(x => new KeyValuePair<Bit, float>(x, x.level / (x.CurrentHealth / x.StartingHealth)))
                .OrderByDescending(x => x.Value)
                .FirstOrDefault().Key;

            //Repair Effect Confirm
            //--------------------------------------------------------------------------------------------------------//

            if (repairTarget && repairTarget != (Bit) toRepair)
            {
                _repairEffects[repairTarget].SetActive(false);
            }

            //--------------------------------------------------------------------------------------------------------//

            //If we weren't able to find a part, see if the repairer needs to be fixed
            if (toRepair is null)
                return;

            //Repair Effect Setup
            //--------------------------------------------------------------------------------------------------------//

            var bitToRepair = (Bit) toRepair;

            if (repairTarget != bitToRepair)
            {
                _repairTarget[part] = bitToRepair;

                if (_repairEffects.IsNullOrEmpty())
                    _repairEffects = new Dictionary<Bit, GameObject>();

                if (!_repairEffects.TryGetValue(bitToRepair, out var effectObject))
                {
                    CreateRepairEffect(bitToRepair);
                }
                else
                {
                    effectObject.SetActive(true);
                }
            }
            //--------------------------------------------------------------------------------------------------------//

            //Get Cost
            //--------------------------------------------------------------------------------------------------------//
            
            if (TryUseAmmo(part, partRemoteData) == false)
                return;

            //--------------------------------------------------------------------------------------------------------//
            
            if (!TryGetPartProperty(PartProperties.KEYS.Cooldown, part, partRemoteData, out var cooldown))
                throw new ArgumentOutOfRangeException();
            
            if (!TryGetPartProperty(PartProperties.KEYS.Heal, part, partRemoteData, out var repairAmount))
                throw new ArgumentOutOfRangeException();
            
            _repairTimers[part] = cooldown;
            toRepair.ChangeHealth(repairAmount * deltaTime);

            TryPlaySound(part, SOUND.REPAIRER_PULSE, toRepair.CurrentHealth < toRepair.StartingHealth);
        }

        private void RegenUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            var repairTarget = bot;

            if (repairTarget.CurrentHealth >= repairTarget.StartingHealth)
            {
                return;
            }
            
            if (!TryGetPartProperty(PartProperties.KEYS.Heal, part, partRemoteData, out var repairAmount))
                throw new ArgumentOutOfRangeException();

            //Get Cost
            //--------------------------------------------------------------------------------------------------------//
            
            if (TryUseAmmo(part, partRemoteData, Time.deltaTime / repairAmount) == false)
                return;

            //--------------------------------------------------------------------------------------------------------//

            
            
            repairTarget.ChangeHealth(repairAmount * deltaTime);

            
            TryPlaySound(part, SOUND.REPAIRER_PULSE, repairTarget.CurrentHealth < repairTarget.StartingHealth);
        }

        private void HealUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            /*if (_healActiveTimer <= 0)
            {
                return;
            }
            _healActiveTimer -= Time.deltaTime;*/

            if (_healWaitTimer > 0f)
            {
                _healWaitTimer -= deltaTime;
                return;
            }
            
            
            var repairTarget = bot;

            if (repairTarget.CurrentHealth >= repairTarget.StartingHealth)
            {
                return;
            }
            

            //--------------------------------------------------------------------------------------------------------//

            var ammoCost = partRemoteData.ammoUseCost;

            if (!TryGetPartProperty(PartProperties.KEYS.Heal, part, partRemoteData, out var healAmount))
                throw new ArgumentOutOfRangeException();

            var cost = ammoCost * Time.deltaTime;
            
            var ammoResource = PlayerDataManager.GetResource(partRemoteData.category);
            
            if (ammoResource.Ammo < cost)
                return;
            
            ammoResource.SubtractAmmo(cost);
            
            var heal = healAmount * deltaTime;
            repairTarget.ChangeHealth(heal);
            
            

            TryPlaySound(part, SOUND.REPAIRER_PULSE, repairTarget.CurrentHealth < repairTarget.StartingHealth);
        }

        private void ShieldUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            if (!_shieldActive)
                return;

            var timer = _shieldTimers[part];

            timer -= deltaTime;

            if (timer <= 0f)
            {
                _shieldActive = false;
                _shieldObject.SetActive(false);
            }

            _shieldTimers[part] = timer;
        }
        
        private void SabreUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            if (!_sabreActive)
                return;

            if (_sabreObject == null)
                return;

            var timer = _sabreTimers[part];

            timer -= deltaTime;

            if (timer <= 0f)
            {
                _sabreActive = false;
                _sabreObject.SetActive(false);
            }

            _sabreTimers[part] = timer;

            //var multiplier = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Multiplier);

            var size =bot.ContinousRotation ? _sabreObject.maxSize : _sabreObject.minSize;
            _sabreObject.SetSize(size);
            
            var dir = (part.Position - _corePart.Position).normalized;
            var pos = part.Position + (dir * (size / 2)) + (dir * (Constants.gridCellSize / 2f));

            _sabreObject.SetTransform(pos, dir);
        }

        private void BlasterUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            //--------------------------------------------------------------------------------------------//
            if (_projectileTimers == null)
                _projectileTimers = new Dictionary<Part, float>();

            if (!_projectileTimers.ContainsKey(part))
                _projectileTimers.Add(part, 0f);

            //Cooldown
            //--------------------------------------------------------------------------------------------//

            var cooldown = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Cooldown);
            //cooldown /= GetBoostValue(PART_TYPE.BOOSTRATE, part);

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

            //TODO Create projectile shooting at new target

            CreateProjectile(part, partRemoteData, asteroid, "Asteroid");
        }

        private void GunUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
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
            var cooldown = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Cooldown);
            var cooldownBoost = part.Patches.GetPatchMultiplier(PATCH_TYPE.FIRE_RATE);

            if (_projectileTimers[part] < cooldown * cooldownBoost)
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

            //Get Cost
            //--------------------------------------------------------------------------------------------------------//
            if (TryUseAmmo(part, partRemoteData) == false)
                return;
            
            //--------------------------------------------------------------------------------------------------------//
            
            _gunTargets[part] = fireTarget;


            //Use resources
            //--------------------------------------------------------------------------------------------//

            switch (part.Type)
            {
                case PART_TYPE.GUN:
                case PART_TYPE.TRIPLESHOT:
                case PART_TYPE.MISSILE:
                    CreateProjectile(part, partRemoteData, fireTarget, tag);
                    break;
                case PART_TYPE.SNIPER:
                    var direction = (fireTarget.transform.position + ((Vector3) Random.insideUnitCircle * 3) - bot.transform.position).normalized;

                    var lineShrink = FactoryManager.Instance
                        .GetFactory<EffectFactory>()
                        .CreateObject<LineShrink>();

                    var chance = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Probability);
                    var didHitTarget = Random.value <= chance;


                    lineShrink.Init(part.transform.position,
                        didHitTarget
                            ? fireTarget.transform.position
                            : bot.transform.position + direction * 100);

                    if (didHitTarget)
                    {
                        /*var damage = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Damage);
                        var damageBoost = part.Patches.GetPatchMultiplier(PATCH_TYPE.POWER);*/

                        if (!TryGetPartProperty(PartProperties.KEYS.Damage, part, partRemoteData, out var damage))
                            throw new ArgumentOutOfRangeException($"Missing {nameof(PartProperties.KEYS.Damage)} on {partRemoteData.name}");

                        if (fireTarget is ICanBeHit iCanBeHit)
                        {
                            iCanBeHit.TryHitAt(fireTarget.transform.position, damage/* * damageBoost*/);
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //--------------------------------------------------------------------------------------------//
        }
        
        private void TriggerPartUpdates(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            var types = new List<BIT_TYPE>
            {
                BIT_TYPE.RED,
                BIT_TYPE.YELLOW,
                BIT_TYPE.GREEN,
                BIT_TYPE.GREY,
                BIT_TYPE.BLUE,
            };
            //TODO This still needs to account for multiple bombs
            if (!_triggerPartTimers.TryGetValue(part, out var timer))
                return;

            if (timer <= 0f)
                return;

            //Wait for the shield to be inactive before the cooldown can begin
            if (part.Type == PART_TYPE.SHIELD && _shieldActive)
                return;
            
            if (part.Type == PART_TYPE.SABRE && _sabreActive)
                return;
            
            //Find the index of the ui element to show cooldown
            var tempPart = part;
            var uiIndex = types.FindIndex(x => x == tempPart.category);//_triggerParts.FindIndex(0, _triggerParts.Count, x => x == tempPart);

            //Get the max cooldown value
            //--------------------------------------------------------------------------------------------------------//

            if (!partRemoteData.TryGetValue(PartProperties.KEYS.Cooldown, out float triggerCooldown))
                throw new ArgumentException($"Remote data for {partRemoteData.name} does not contain a value for {nameof(PartProperties.KEYS.Cooldown)}");
            
            //Update the timer value for this frame
            //--------------------------------------------------------------------------------------------------------//

            timer -= deltaTime;
            var fill = 1f - timer / triggerCooldown;
            GameUI.SetFill(uiIndex, fill);

            _triggerPartTimers[part] = timer;

            //--------------------------------------------------------------------------------------------------------//

            //if (timer < 1f)
            //    return;

            //var canAfford = CanAffordAmmo(part, partRemoteData, out _);
            
            //TODO Setup Can Afford
            //var hasPartGrade = HasPartGrade(part, out _);
            
            //GameUI.SetInteractable(uiIndex, fill >= 1f && canAfford);
        }

        #endregion //Part Updates

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

                        var projectileID = partData.GetDataValue<string>(PartProperties.KEYS.Projectile);

                        _gunRanges.Add(part, GetProjectileRange(part, projectileID));

                        break;
                }
            }
        }

        private void CreateProjectile(in Part part, in PartRemoteData partRemoteData, in CollidableBase collidableTarget, string collisionTag = "Enemy")
        {
            var patches = part.Patches;
            var rangeBoost = patches.GetPatchMultiplier(PATCH_TYPE.RANGE);
            //var damageBoost = patches.GetPatchMultiplier(PATCH_TYPE.POWER);
            
            
            var projectileId = partRemoteData.GetDataValue<string>(PartProperties.KEYS.Projectile);
            //var damage = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Damage);
            
            if (!TryGetPartProperty(PartProperties.KEYS.Damage, part, partRemoteData, out var damage))
                throw new ArgumentOutOfRangeException($"Missing {nameof(PartProperties.KEYS.Damage)} on {partRemoteData.name}");

            /*if ((part.Type == PART_TYPE.GUN || part.Type == PART_TYPE.SNIPER) &&
                HasPartGrade(part, partRemoteData, out var multiplier))
            {
                damage *= multiplier;
            }*/


            var position = bot.transform.position;
            var shootDirection = ShouldUseGunTurret(partRemoteData)
                ? GetAimedProjectileAngle(collidableTarget,
                    Globals.UseCenterFiring ? bot.transform.position : part.Position, projectileId)
                : part.transform.up.normalized;

            //--------------------------------------------------------------------------------------------------------//
            Bot vampireCaster = null;
            var vampirism = 0f;
            /*var vampireCaster = part.Patches.Any(x => x.Type == (int)PATCH_TYPE.VAMPIRE) ? bot : null;

            var vampirism = vampireCaster is null
                ? 0f
                : FactoryManager.Instance.PatchRemoteData.GetRemoteData(PATCH_TYPE.VAMPIRE)
                    .GetDataValue<float>(1, PartProperties.KEYS.Multiplier);*/
            
            //--------------------------------------------------------------------------------------------------------//

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
                    vampireCaster,
                    vampirism,
                    true);
        }

        private static Vector3 GetAimedProjectileAngle(in Actor2DBase target, in Vector3 partPosition, string projectileId)
        {
            Vector3 targetVelocity;
            switch(target)
            {
                case Enemy enemy:
                    targetVelocity = enemy.EnemyMovementSpeed * enemy.MostRecentMovementDirection;
                    break;
                default:
                    targetVelocity = Vector3.zero;
                    break;
            }
            var projectileProfile = FactoryManager.Instance.GetFactory<ProjectileFactory>().GetProfileData(projectileId);
            
            Vector3 totarget = target.Position - partPosition;

            float a = Vector3.Dot(targetVelocity, targetVelocity) - (projectileProfile.ProjectileSpeed * projectileProfile.ProjectileSpeed);
            float b = 2 * Vector3.Dot(targetVelocity, totarget);
            float c = Vector3.Dot(totarget, totarget);

            float p = -b / (2 * a);
            float q = (float)Math.Sqrt((b * b) - 4 * a * c) / (2 * a);

            /*if (float.IsNaN(q))
                return totarget.normalized;*/

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

            Vector3 aimSpot = target.Position + targetVelocity * t;
            Vector3 bulletPath = aimSpot - partPosition;
            
            Debug.DrawRay(partPosition, totarget.normalized * 10, Color.yellow, 1f);
            Debug.DrawRay(partPosition, bulletPath.normalized * 10, Color.green, 1f);
            //Debug.Break();

            return bulletPath;
        }

        private float GetProjectileRange(in Part part, in string projectileID)
        {
            var projectileData = FactoryManager.Instance.GetFactory<ProjectileFactory>().GetProfileData(projectileID);

            var range = projectileData.ProjectileRange;

            if (range == 0f)
                return 100 * Constants.gridCellSize;

            //var rangeBoost = GetBoostValue(PART_TYPE.BOOSTRANGE, part);

            return range;//* rangeBoost;
        }

        private static bool ShouldUseGunTurret(in PartRemoteData partRemoteData)
        {
            if (partRemoteData == null)
                return false;

            if (!partRemoteData.TryGetValue<string>(PartProperties.KEYS.Projectile, out var projectileId))
                return false;
            
            var projectileData = FactoryManager.Instance.GetFactory<ProjectileFactory>().GetProfileData(projectileId);

            return !(projectileData is null) && projectileData.FireAtTarget;
        }

        #endregion //Weapons

        //============================================================================================================//

        #region Trigger Parts

        /// <summary>
        /// This should use values similar to an array (ie. starts at [0])
        /// </summary>
        /// <param name="index"></param>
        public void TryTriggerPart(in int index)
        {
            Part GetPart(in BIT_TYPE type)
            {
                var temp = type;
                return _triggerParts.FirstOrDefault(x => x.category == temp);
            }
            
            if (_triggerParts == null || _triggerParts.Count == 0)
                return;

            var bitType = Constants.BIT_ORDER[index];
            var part = GetPart(bitType);

            /*switch (index)
            {
                case 0: //Blue, West
                    part = GetPart(BIT_TYPE.RED);
                    break;
                case 1: //Red, South
                    part =  GetPart(BIT_TYPE.YELLOW);
                    break;
                case 2: //Grey, North
                    part =  GetPart(BIT_TYPE.GREY);
                    break;
                case 3: //Yellow, East
                    part =  GetPart(BIT_TYPE.BLUE);
                    break;
            }*/

            if (part is null)
                return;
            
            //var part = _triggerParts[index];

            switch (part.Type)
            {
                case PART_TYPE.BOMB:
                    TriggerBomb(part);
                    break;
                case PART_TYPE.FREEZE:
                    TriggerFreeze(part);
                    break;
                case PART_TYPE.SHIELD:
                    TriggerShield(part);
                    break;
                case PART_TYPE.RAILGUN:
                    TriggerRailgun(part);
                    break;
                case PART_TYPE.TRACTOR:
                    TriggerTractorBeam(part);
                    break;
                case PART_TYPE.HEAL:
                    TriggerHeal(part);
                    break;
                case PART_TYPE.DECOY:
                    TriggerDecoy(part);
                    break;
                case PART_TYPE.BITSPLOSION:
                    TriggerBitsplosion(part);
                    break;
                case PART_TYPE.HOOVER:
                    TriggerHoover(part);
                    break;
                case PART_TYPE.SABRE:
                    TriggerSabre(part);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Part.Type), _triggerParts[index].Type, null);
            }
        }

        //====================================================================================================================//

        private bool CanUseTriggerPart(in Part part, out PartRemoteData partRemoteData, in bool useTriggerOnTrue = true)
        {
            partRemoteData = null;
            
            if (_triggerPartTimers == null || _triggerPartTimers.Count == 0)
                return false;

            //If the bomb is still recharging, we tell the player that its unavailable
            if (_triggerPartTimers[part] > 0f)
            {
                //AudioController.PlaySound(SOUND.BOMB_CLICK);
                return false;
            }

            partRemoteData = FactoryManager.Instance
                .GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);
            
            if (!partRemoteData.TryGetValue<float>(PartProperties.KEYS.Cooldown, out var cooldown))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Cooldown} missing from {part.Type} remote data");
            }

            if (!CanAffordAmmo(part, partRemoteData, out _))
            {
                //AudioController.PlaySound(SOUND.BOMB_CLICK);
                return false;
            }

            if (!useTriggerOnTrue) 
                return true;

            if (!TryUseAmmo(part, partRemoteData))
                throw new Exception($"Error calling {nameof(TryUseAmmo)}");

            //resource.SubtractAmmo(ammoCost);
            
            _triggerPartTimers[part] = cooldown;

            return true;
        }
        
        private void TriggerBomb(in Part part)
        {
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;

            //Damage all the enemies
            if (!TryGetPartProperty(PartProperties.KEYS.Damage, part, partRemoteData, out var damage))
                throw new ArgumentOutOfRangeException($"Missing {nameof(PartProperties.KEYS.Damage)} on {partRemoteData.name}");
            
            if(!partRemoteData.TryGetValue(PartProperties.KEYS.Radius, out int radius))
                throw new MissingFieldException($"{PartProperties.KEYS.Radius} missing from {part.Type} remote data");

            var enemies = EnemyManager.GetEnemiesInRange(part.transform.position, radius);
            foreach (var enemy in enemies)
            {
                enemy.TryHitAt(enemy.transform.position, damage);
            }
            
            //EnemyManager.DamageAllEnemies(damage);

            CreateBombEffect(part, radius * 2);

            AudioController.PlaySound(SOUND.BOMB_BLAST);
        }

        private void TriggerFreeze(in Part part)
        {
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;
            
            if (!partRemoteData.TryGetValue(PartProperties.KEYS.Time, out float freezeTime))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Time} missing from {part.Type} remote data");
            }
            
            //--------------------------------------------------------------------------------------------------------//

            if(!partRemoteData.TryGetValue(PartProperties.KEYS.Radius, out int radius))
                throw new MissingFieldException($"{PartProperties.KEYS.Radius} missing from {part.Type} remote data");

            var enemies = EnemyManager.GetEnemiesInRange(part.transform.position, radius);

            foreach (var enemy in enemies)
            {
                enemy.SetFrozen(freezeTime);
            }
            
            //Need to pass the diameter not the radius
            CreateFreezeEffect(part, radius * 2f);
            AudioController.PlaySound(SOUND.BOMB_BLAST);
        }

        private void TriggerShield(in Part part)
        {
            void SetShieldSize()
            {
                if (_shieldObject == null)
                    CreateShieldEffect();
                
                _shieldObject.SetActive(true);
            
                //TODO Set the shield Size
                var coordinates = bot.AttachedBlocks
                    .Select(x => new
                    {
                        x = Mathf.Abs(x.Coordinate.x),
                        y = Mathf.Abs(x.Coordinate.y)
                    })
                    .ToArray();
                
                var max = Mathf.Max(
                    coordinates.Max(x => x.x),
                    coordinates.Max(x => x.y)) + 1;

                max *= 2;
                max--;
                
                _shieldObject.transform.localScale = Vector3.one * (max * 1.3f);
            }
            
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;

            //--------------------------------------------------------------------------------------------------------//
            
            if (!partRemoteData.TryGetValue<float>(PartProperties.KEYS.Time, out var seconds))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Time} missing from {part.Type} remote data");
            }
            
            //Set the shielded time
            _shieldTimers[part] = seconds;

            _shieldActive = true;

            SetShieldSize();
        }

        private void TriggerRailgun(in Part part)
        {
            if (bot.Rotating)
                return;
            
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;
            //--------------------------------------------------------------------------------------------------------//

            CreateProjectile(part, partRemoteData, null);

        }

        private void TriggerTractorBeam(in Part part)
        {
            if (!CanUseTriggerPart(part, out _, false))
                return;

            var bit = LevelManager.Instance.ObstacleManager.TryGetBitInColumn(part.transform.position);

            if (bit is null)
                return;

            CanUseTriggerPart(part, out var partRemoteData);
            
            if (!TryGetPartProperty(PartProperties.KEYS.Speed, part, partRemoteData, out var speedMultiplier))
                throw new ArgumentOutOfRangeException();

            bit.AddMove = Vector2.down * speedMultiplier;
            
            //TODO Add functionality

        }
        
        private void TriggerHeal(in Part part)
        {
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;

            if (_healObject == null)
            {
                _healObject = part.gameObject;
            }

            _healActiveTimer = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Time);
        }

        private void TriggerDecoy(in Part part)
        {
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;

            if (bot.DecoyDrone != null)
            {
                bot.DecoyDrone.ChangeHealth(-1000000);
                bot.DecoyDrone = null;
            }

            var decoyDroneHealth = Globals.DecoyDroneHealth;
            
            //FIXME This needs to be moved to a factory
            bot.DecoyDrone = Instantiate(bot._decoyDronePrefab, bot.transform.position, Quaternion.identity).GetComponent<DecoyDrone>();
            bot.DecoyDrone.Init(bot, 10f);
            bot.DecoyDrone.SetupHealthValues(decoyDroneHealth,decoyDroneHealth);
        }

        private void TriggerBitsplosion(in Part part)
        {
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;

            //Damage all the enemies
            /*if (!partRemoteData.TryGetValue(PartProperties.KEYS.Damage, out float damage))
                throw new MissingFieldException($"{PartProperties.KEYS.Damage} missing from {part.Type} remote data");*/
            
            if (!TryGetPartProperty(PartProperties.KEYS.Damage, part, partRemoteData, out var damage))
                throw new ArgumentOutOfRangeException($"Missing {nameof(PartProperties.KEYS.Damage)} on {partRemoteData.name}");

            EnemyManager.DamageAllEnemies(damage);

            var bits = LevelManager.Instance.ObstacleManager.TryGetBitsOnScreen();
            for (int i = 0; i < bits.Count; i++)
            {
                //EnemyManager.DamageAllEnemiesInRange(damage, bits[i].transform.position, 5f);
                CreateBombEffect(bits[i], 5f);
                
                Recycler.Recycle<Bit>(bits[i]);
            }

            AudioController.PlaySound(SOUND.BOMB_BLAST);
        }

        //FIXME This needs to be cleaned
        private void TriggerHoover(in Part part)
        {
            Vector2Int[] GetCoordinates(in int checkIndex, in Vector2Int offset, in Vector2Int startCoordinate)
            {
                var count = 3 + checkIndex;
                var coordinate = startCoordinate;
                coordinate += Vector2Int.right * (startCoordinate.x < 0 ? -checkIndex : checkIndex);

                var outList = new List<Vector2Int>();
                
                for (var i = 0; i < count; i++)
                {
                    outList.Add(coordinate + offset * i);
                }

                return outList.ToArray();
            }
            
            
            if (!CanUseTriggerPart(part, out _, false))
                return;

            var bits = LevelManager.Instance.ObstacleManager.TryGetBitsOnScreen();

            if (bits.IsNullOrEmpty())
                return;

            CanUseTriggerPart(part, out _);

            bits = bits
                .OrderBy(x => Vector2.Distance(x.transform.position, bot.transform.position))
                .ToList();

            var startIndices = new[] {1, 1, 2, 2, 3, 3};

            foreach (var bit in bits)
            {
                var dir = (bit.transform.position - bot.transform.position).normalized;
                var startCoordinate = dir.x > 0 ? new Vector2Int(2, 0) : new Vector2Int(-2, 0);
                var offsets = new Vector2Int(
                    dir.x > 0 ? -1 : 1, 
                    dir.y > 0 ? 1 : -1
                );

                var currentlyAttached = bot.AttachedBlocks;
                var success = false;

                for (int i = 0; i <= 5; i++)
                {
                    var startIndex = startIndices[i];

                    var count = 3 + i;
                    var isOdd = count % 2 == 1;
                    
                    var offsetAmount = isOdd ? new Vector2Int(-1, 1) : new Vector2Int(1, -1);
                    
                    var totalOffset = new Vector2Int(0, 0);
                    var flip = true;

                    var coordinates = GetCoordinates(i, offsets, startCoordinate);

                    for (int ii = 0; ii < count; ii++)
                    {
                        var coordinateIndex = startIndex + (flip ? totalOffset.x : totalOffset.y);
                        
                        //TODO Get coordinate
                        var coordinate = coordinates[coordinateIndex];
                        
                        if (flip) totalOffset.y += offsetAmount.x;
                        else totalOffset.x += offsetAmount.y;

                        flip = !flip;
                        
                        //TODO Check if coordinate is occupied
                        if(currentlyAttached.Any(x => x.Coordinate == coordinate))
                            continue;

                        if (currentlyAttached.HasPathToCore(coordinate))
                        {
                            bot.AttachNewBlock(coordinate, bit, false, false, false, false, false);
                            success = true;
                            break;
                        }

                    }

                    if (success)
                        break;

                    if (i == 5 && success == false)
                        throw new Exception($"Unable to find an attachable Point, {PART_TYPE.HOOVER}");

                }
            }


            bot.CheckAllForCombos();
            bot.ForceCheckMagnets();
            bot.ForceUpdateColliderGeometry();
        }
        
        private void TriggerSabre(in Part part)
        {
            var partPos = part.Position;
            var corePos = _corePart.Position;
            
            void SetupSabre(in int size)
            {
                //TODO Check to see if the object needs to be instantiated
                if (_sabreObject == null)
                {
                    _sabreObject = FactoryManager.Instance.GetFactory<BotFactory>().CreateSabreObject();
                    Physics2D.IgnoreCollision(bot.Collider, _sabreObject.collider);
                }
                else
                {
                    _sabreObject.SetActive(true);
                }

                //TODO Find the direction to the core and invert it

                var dir = (partPos - corePos).normalized;
                var pos = partPos + (dir * (size / 2));

                _sabreObject.SetTransform(pos, dir);

            }
            
            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;
            
            if (!partRemoteData.TryGetValue<float>(PartProperties.KEYS.Time, out var seconds))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Time} missing from {part.Type} remote data");
            }
            
            if (!partRemoteData.TryGetValue<int>(PartProperties.KEYS.Radius, out var minSize))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Radius} missing from {part.Type} remote data");
            }
            if (!partRemoteData.TryGetValue<float>(PartProperties.KEYS.Boost, out var maxSize))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Radius} missing from {part.Type} remote data");
            }
            
            if (!partRemoteData.TryGetValue<float>(PartProperties.KEYS.Damage, out var damage))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Damage} missing from {part.Type} remote data");
            }
            
            _sabreTimers[part] = seconds;

            _sabreActive = true;

            SetupSabre(minSize + 1);
            
            _sabreObject.Init(damage, minSize, maxSize);

        }

        #endregion

        //============================================================================================================//

        #region Armor

        public bool TryHitArmor(ref float damage)
        {
            if (_shieldActive)
            {
                damage = 0f;
                return true;
            }
            
            var armors = bot.AttachedBlocks
                .OfType<Part>()
                .Where(x => x.Type == PART_TYPE.ARMOR && x.Disabled == false)
                .ToArray();

            if (armors.IsNullOrEmpty())
                return false;
            
            var partRemoteData = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.ARMOR);

            if (!partRemoteData.TryGetValue<float>(PartProperties.KEYS.Multiplier, out var multiplier))
            {
                return false;
            }

            damage *= 1.0f - multiplier;

            return true;
        }

        #endregion 

        //Find Bits/Values to burn
        //============================================================================================================//

        #region Process Bit

        
        /*/// <summary>
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
                return 0;#1#

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


            if (targetBit.level >= 2)
            {
                if (Globals.SendExcessResourceToBase)
                {
                    var excessResource = FactoryManager.Instance
                        .GetFactory<BitAttachableFactory>()
                        .GetBitRemoteData(bitType)
                        .levels[targetBit.level - 1]
                        .resources;
                    Debug.Log("Send value " + excessResource + " to base.");
                }
                else
                {
                    targetBit.DecreaseLevel();
                }
            }
            else
            {
                //TODO May want to play around with the order of operations here
                StartCoroutine(RefineBitCoroutine(targetBit,
                    part.transform,
                    orphans,
                    1.6f,
                    () =>
                    {
                        bot.DestroyAttachable<Bit>(targetBit);
                    }));
            }


            SessionDataProcessor.Instance.LiquidProcessed(targetBit.Type, amountProcessed);
            AudioController.PlaySound(SOUND.BIT_REFINED);
            bot.ForceCheckMagnets();
            
            if (part.Type == PART_TYPE.REFINER)
            {
                CreateRefinerEffect(part, bitType);
            }

            return amountProcessed;
        }

        private Bit GetFurthestBitToBurn(PartRemoteData partRemoteData, BIT_TYPE type)
        {
            if (!useBurnRate)
                return null;

            if (partRemoteData.burnRate == 0f)
                return null;

            return GetFurthestBitToBurn(type);
        }

        private Bit GetFurthestBitToBurn(BIT_TYPE type)
        {
            return bot.attachedBlocks.OfType<Bit>()
                .Where(b => b.Type == type)
                .GetFurthestAttachable(Vector2Int.zero);
        }

        private float GetValueToBurn(PartRemoteData partRemoteData, BIT_TYPE type)
        {
            if (!useBurnRate)
                return default;

            var value = partRemoteData.burnRate == 0
                ? default
                : PlayerDataManager.GetResource(type).liquid;
            return value;
        }*/

        #endregion //Process Bit

        //Boosts
        //====================================================================================================================//

        #region Boosts

        //FIXME This is very efficient for finding the parts
        /*private float GetBoostValue(PART_TYPE boostPart, in Part fromPart)
        {
            var boosts = _parts
                .Where(x => !x.Disabled /*&& !x.Destroyed#1# && x.Type == boostPart)
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
            foreach (var remoteData in beside.Select(part => partRemoteData))
            {
                if (!remoteData.TryGetValue(PartProperties.KEYS.Multiplier, out float mult))
                    continue;

                if (mult > maxBoost)
                    maxBoost = mult;
            }

            return maxBoost;
        }*/

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

        //Ammo
        //====================================================================================================================//

        private bool CanAffordAmmo(in Part part, in PartRemoteData partRemoteData, out float ammoCost, float additional = 1f)
        {
            var ammoMultiplier = part.Patches.GetPatchMultiplier(PATCH_TYPE.EFFICIENCY);
            var ammoResource = PlayerDataManager.GetResource(partRemoteData.category);
            var currentAmmo = ammoResource.Ammo;
            ammoCost = partRemoteData.ammoUseCost * ammoMultiplier * additional;

            return ammoCost <= currentAmmo;
        }
        
        private bool TryUseAmmo(in Part part, in PartRemoteData partRemoteData, float additional = 1f)
        {
            if (!CanAffordAmmo(part, partRemoteData, out var ammoCost, additional))
                return false;
            
            var ammoResource = PlayerDataManager.GetResource(partRemoteData.category);
            ammoResource.SubtractAmmo(ammoCost);
            
            return true;
        }

        //====================================================================================================================//

        private bool TryGetPartProperty(in PartProperties.KEYS key, in Part part, in PartRemoteData partRemoteData, out float value)
        {
            if (!partRemoteData.TryGetValue(key, out value))
                return false;
            
            switch (key)
            {
                case PartProperties.KEYS.Heal:
                case PartProperties.KEYS.Damage:
                case PartProperties.KEYS.Speed:
                    value *= part.Patches.GetPatchMultiplier(PATCH_TYPE.POWER);
                    break;
                case PartProperties.KEYS.Radius:
                    value *= part.Patches.GetPatchMultiplier(PATCH_TYPE.RANGE);
                    break;
                case PartProperties.KEYS.Time:
                case PartProperties.KEYS.Cooldown:
                    value *= part.Patches.GetPatchMultiplier(PATCH_TYPE.FIRE_RATE);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(key), key, null);
            }

            value = (float)Math.Round(value, 2);
            
            return true;
        }

        //Checking for recycled extras
        //============================================================================================================//

        #region Part Dictionary Recycling

        private void TryClearPartDictionaries()
        {
            /*CheckShouldRecycle(ref _shields, (ShieldData data) =>
            {
                Recycler.Recycle<Shield>(data.gameObject);
            });*/
            
            CheckShouldRecycle(ref _flashes, (FlashSprite data) =>
            {
                Recycler.Recycle<FlashSprite>(data.gameObject);
            });
            CheckShouldRecycle(ref _triggerPartTimers, (Part _) =>
            {
                //var index = _triggerParts.FindIndex(0, _triggerParts.Count, x => x == part);
                //GameUI.ShowIcon(index, false);
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
        private static void CheckShouldRecycle<T>(ref Dictionary<Bit, T> partDictionary, Action<T> OnRecycleCallback)
        {
            if (partDictionary.IsNullOrEmpty())
                return;

            var copy = new Dictionary<Bit, T>(partDictionary);
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
        
        private Dictionary<Part, Bit> _repairTarget;
        private Dictionary<Bit, GameObject> _repairEffects;
        
        private FlashSprite GetAlertIcon(Part part)
        {
            if(_flashes == null)
                _flashes = new Dictionary<Part, FlashSprite>();

            if (_flashes.ContainsKey(part))
                return _flashes[part];

            
            var burnType = FactoryManager.Instance.PartsRemoteData.GetRemoteData(part.Type).category;
            var bitColor = burnType.GetColor();

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

        private void CreateBombEffect(in IAttachable attachable, in float range)
        {
           
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.BOMB);

            effect.transform.position = attachable.transform.position;
            
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

        /*private void CreateRefinerEffect(in Part part, in BIT_TYPE bitType)
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
        }*/

        private void CreateRepairEffect(in Bit bit)
        {
            if(_repairEffects.IsNullOrEmpty())
                _repairEffects = new Dictionary<Bit, GameObject>();

            if (_repairEffects.ContainsKey(bit))
                return;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.REPAIR);
            
            effect.transform.SetParent(bit.transform, false);
            
            _repairEffects.Add(bit, effect);
        }
        
        /*private void CreateBoostRateEffect(in Part part)
        {
            if(_boostEffects.IsNullOrEmpty())
                _boostEffects = new Dictionary<Part, GameObject>();

            if (_boostEffects.ContainsKey(part))
                return;
            
            var effect = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.RATE_BOOST);
            
            effect.transform.SetParent(part.transform, false);
            
            _boostEffects.Add(part, effect);
        }*/
        
        private void CreateShieldEffect()
        {
            var shield = FactoryManager.Instance.GetFactory<EffectFactory>()
                .CreatePartEffect(EffectFactory.PART_EFFECT.SHIELD).transform;
            
            shield.SetParent(bot.transform, false);
            shield.localPosition = Vector3.zero;

            _shieldObject = shield.gameObject;
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
                _repairEffects = new Dictionary<Bit, GameObject>();
            }
            
            if(_shieldObject != null)
                Destroy(_shieldObject);

            if (_sabreObject != null)
            {
                Recycler.Recycle<Sabre>(_sabreObject);
                _sabreObject = null;
            }
            
            /*if (!_boostEffects.IsNullOrEmpty())
            {
                var effectsValues = _boostEffects.Values;

                foreach (var boost in effectsValues)
                {
                    Destroy(boost);
                }
                
                _boostEffects = new Dictionary<Part, GameObject>();
            }*/

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
            /*CheckShouldRecycle(ref _boostEffects, (GameObject data) =>
            {
                Destroy(data.gameObject);
            });*/
        }

        #endregion //Effects

        //============================================================================================================//

        /*public float GetVampireValue()
        {
            var part = _parts.FirstOrDefault(x => x.Disabled == false && x.Type == PART_TYPE.VAMPIRE);
            
            if (part == null || part.Disabled)
                return 0f;

            var value = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.VAMPIRE).defaultValue;

            return value;
        }*/

        //====================================================================================================================//

        //FIXME Constantly calling this can be expensive, might be better doing a count at the start/Part Setup
        private int GetUpgradersAroundPart(in Part part)
        {
            return _parts
                .GetAttachablesAround(part)
                .OfType<Part>()
                .Count(x => x.Type == PART_TYPE.UPGRADER);
        }

        public List<Bot.DataTest> GetWildcardParts()
        {
            var wildCards = _parts?.Where(x => x.Type == PART_TYPE.WILDCARD).ToArray();

            if (wildCards.IsNullOrEmpty())
                return null;

            var bitsToCheck = bot.AttachedBlocks.OfType<Bit>().ToArray();

            var outList = new List<Bot.DataTest>();
            foreach (var wildCard in wildCards)
            {
                
                outList.Add(new Bot.DataTest
                {
                    Coordinate = wildCard.Coordinate,
                    IsEndPiece = bitsToCheck.GetAttachablesAround(wildCard).Count == 1
                });
            }

            return outList;
        }

        //====================================================================================================================//
    }
}
