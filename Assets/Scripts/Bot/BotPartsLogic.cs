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
        private int _magnetOverride;

        //==============================================================================================================//

        private bool _shieldActive;
        private bool _vampirismActive;
        
        private Dictionary<Part, Transform> _turrets;
        private Dictionary<Part, CollidableBase> _gunTargets;
        
        private List<Part> _parts;
        private List<Part> _triggerParts;
        private static int MAXTriggerParts => 4;

        private Dictionary<Part, bool> _playingSounds;

        private Dictionary<Part, float> _projectileTimers;
        private Dictionary<Part, FlashSprite> _flashes;
        private Dictionary<Part, float> _triggerPartTimers;
        
        private Dictionary<Part, float> _shieldTimers;
        private Dictionary<Part, float> _vampireTimers;

        private Dictionary<Part, Asteroid> _asteroidTargets;
        private Dictionary<Part, SpaceJunk> _spaceJunkTargets;

        private Dictionary<Part, float> _gunRanges;

        private static PartAttachableFactory _partAttachableFactory;

        #endregion //Properties

        //Unity Functions
        //==============================================================================================================//

        private void Start()
        {
            _partAttachableFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
        }

        //====================================================================================================================//
        
        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        public void PopulatePartsList()
        {
            _parts = bot.attachedBlocks.OfType<Part>().ToList();
            _triggerParts = _parts
                .Where(p =>
                    p.Type == PART_TYPE.BOMB || 
                    p.Type == PART_TYPE.FREEZE || 
                    p.Type == PART_TYPE.SHIELD || 
                    p.Type == PART_TYPE.VAMPIRE)
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
            if (_magnetOverride > 0)
            {
                MagnetCount = _magnetOverride;
            }

            _gunTargets = new Dictionary<Part, CollidableBase>();
            _repairTarget = new Dictionary<Part, Bit>();

            TryClearPartDictionaries();
            CheckShouldRecycleEffects();

            //Update the Game UI for the Smart Weapons
            //--------------------------------------------------------------------------------------------------------//

            GameUI.ResetIcons();

            for (int i = 0; i < MAXTriggerParts; i++)
            {
                if (i >= _triggerParts.Count)
                    break;

                var partActive = HasPartGrade(_triggerParts[i], out _);

                GameUI.SetIconImage(i, _triggerParts[i].Type);
                GameUI.ShowIcon(i, true);
                GameUI.SetInteractable(i, partActive);
            }

            //--------------------------------------------------------------------------------------------------------//

            FindObjectOfType<GameUI>();
            MagnetCount = 0;

            int value;
            foreach (var part in _parts)
            {
                if (part.Type == PART_TYPE.EMPTY)
                {
                    continue;
                }

                var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(part.Type);

                part.Disabled = !HasPartGrade(partRemoteData, out _);

                //Destroyed or disabled parts should not contribute to the stats of the bot anymore
                if (part.Disabled)
                    continue;

                switch (part.Type)
                {
                    case PART_TYPE.CORE:
                        if (_magnetOverride > 0)
                            break;

                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Magnet, out value))
                        {
                            MagnetCount += value;
                        }

                        if (HasPartGrade(partRemoteData, out var floatValue))
                        {
                            MagnetCount += (int)floatValue;
                        }

                        break;
                    case PART_TYPE.SHIELD:
                        if (_shieldTimers == null)
                            _shieldTimers = new Dictionary<Part, float>();

                        if (_shieldTimers.ContainsKey(part))
                            break;
                        
                        _shieldTimers.Add(part, 0f);                        
                        break;
                    case PART_TYPE.VAMPIRE:
                        if (_vampireTimers == null)
                            _vampireTimers = new Dictionary<Part, float>();

                        if (_vampireTimers.ContainsKey(part))
                            break;
                        
                        _vampireTimers.Add(part, 0f);                        
                        break;
                    
                    case PART_TYPE.REPAIR:
                        _repairTarget.Add(part, null);
                        break;

                    case PART_TYPE.GUN:
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.MISSILE:
                        
                        _gunTargets.Add(part, null);
                        
                        if (ShouldUseGunTurret(partRemoteData))
                            CreateTurretEffect(part);
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
                    case PART_TYPE.CORE:
                        CoreUpdate(part, partRemoteData);
                        break;
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
                    case PART_TYPE.VAMPIRE:
                        VampireUpdate(part, partRemoteData, deltaTime);
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

        private void CoreUpdate(in Part part, in PartRemoteData partRemoteData)
        {
            if (_magnetOverride > 0)
                return;

            MagnetCount = 0;

            if (partRemoteData.TryGetValue(PartProperties.KEYS.Magnet, out int value))
            {
                MagnetCount += value;
            }

            if (HasPartGrade(partRemoteData, out var magnet))
            {
                MagnetCount += (int)magnet;
            }
        }

        private void RepairUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            var repairTarget = _repairTarget[part];


            //FIXME I don't think using linq here, especially twice is the best option
            //TODO This needs to fire every x Seconds
            IHealth toRepair = bot.attachedBlocks.OfType<Bit>()
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
            
            if (!HasPartGrade(partRemoteData, out var repairAmount))
            {
                return;
            }

            if (repairAmount == 0)
                return;
            

            //Increase the health of this part depending on the current level of the repairer
            toRepair.ChangeHealth(repairAmount * deltaTime);

            TryPlaySound(part, SOUND.REPAIRER_PULSE, toRepair.CurrentHealth < toRepair.StartingHealth);
        }

        private void ShieldUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            if (!_shieldActive)
                return;

            var timer = _shieldTimers[part];

            timer -= deltaTime;

            if (timer <= 0f)
                _shieldActive = false;

            _shieldTimers[part] = timer;
        }
        
        private void VampireUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            if (!_vampirismActive)
                return;

            var timer = _vampireTimers[part];

            timer -= deltaTime;

            if (timer <= 0f)
                _vampirismActive = false;

            _vampireTimers[part] = timer;
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


            //TODO Determine if this fires at all times or just when there are active enemies in range



            //Use resources
            //--------------------------------------------------------------------------------------------//


            //--------------------------------------------------------------------------------------------//

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
            var cooldownBoost = GetPatchMultiplier(part, PATCH_TYPE.FIRE_RATE);

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
                    var direction = (fireTarget.transform.position + ((Vector3) Random.insideUnitCircle * 3) -
                                     part.transform.position).normalized;

                    var lineShrink = FactoryManager.Instance.GetFactory<EffectFactory>()
                        .CreateObject<LineShrink>();

                    var chance = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Probability);
                    var didHitTarget = Random.value <= chance;


                    lineShrink.Init(part.transform.position,
                        didHitTarget
                            ? fireTarget.transform.position
                            : part.transform.position + direction * 100);

                    if (didHitTarget)
                    {
                        var damage = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Damage);
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
        
        private void TriggerPartUpdates(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            //TODO This still needs to account for multiple bombs
            if (!_triggerPartTimers.TryGetValue(part, out var timer))
                return;

            if (timer <= 0f)
                return;
            
            //Find the index of the ui element to show cooldown
            var tempPart = part;
            var uiIndex = _triggerParts.FindIndex(0, _triggerParts.Count, x => x == tempPart);

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

            if (timer < 1f)
                return;

            var hasPartGrade = HasPartGrade(part, out _);
            
            GameUI.SetInteractable(uiIndex, fill >= 1f && hasPartGrade);
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
            var rangeBoost = GetPatchMultiplier(part, PATCH_TYPE.RANGE);
            var damageBoost = GetPatchMultiplier(part, PATCH_TYPE.DAMAGE);
            
            
            var projectileId = partRemoteData.GetDataValue<string>(PartProperties.KEYS.Projectile);
            var damage = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Damage);

            if ((part.Type == PART_TYPE.GUN || part.Type == PART_TYPE.SNIPER) &&
                HasPartGrade(partRemoteData, out var multiplier))
            {
                damage *= multiplier;
            }


            var position = part.transform.position;
            var shootDirection = ShouldUseGunTurret(partRemoteData)
                ? GetAimedProjectileAngle(collidableTarget, part, projectileId)
                : part.transform.up.normalized;

            var vampireCaster = _parts.Any(x => x.Type == PART_TYPE.VAMPIRE) ? bot : null;

            //TODO Might need to add something to change the projectile used for each gun piece
            FactoryManager.Instance.GetFactory<ProjectileFactory>()
                .CreateObjects<Projectile>(
                    projectileId,
                    position,
                    collidableTarget,
                    shootDirection,
                    damage * damageBoost,
                    rangeBoost,
                    collisionTag,
                    vampireCaster,
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

            //var rangeBoost = GetBoostValue(PART_TYPE.BOOSTRANGE, part);

            return range;//* rangeBoost;
        }

        private static bool ShouldUseGunTurret(in PartRemoteData partRemoteData)
        {
            var projectileId = partRemoteData.GetDataValue<string>(PartProperties.KEYS.Projectile);
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
        public void TryTriggerPart(int index)
        {
            if (_triggerParts == null || _triggerParts.Count == 0)
                return;
            //TODO Need to check the capacity of smart weapons on the bot
            if (index - 1 > MAXTriggerParts)
                return;

            if (index >= _triggerParts.Count)
                return;

            var part = _triggerParts[index];

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
                case PART_TYPE.VAMPIRE:
                    TriggerVampire(part);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Part.Type), _triggerParts[index].Type, null);
            }
        }

        private void TriggerBomb(in Part part)
        {
            if (_triggerPartTimers == null || _triggerPartTimers.Count == 0)
                return;

            //If the bomb is still recharging, we tell the player that its unavailable
            if (_triggerPartTimers[part] > 0f)
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);

            if (!HasPartGrade(partRemoteData, out var multiplier))
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }


            //Set the cooldown time
            if (partRemoteData.TryGetValue(PartProperties.KEYS.Cooldown, out float cooldown))
            {
                _triggerPartTimers[part] = cooldown;
            }

            //Damage all the enemies
            if (!partRemoteData.TryGetValue(PartProperties.KEYS.Damage, out float damage))
            {
                return;
            }

            damage *= multiplier;

            EnemyManager.DamageAllEnemies(damage);

            CreateBombEffect(part, 50f);

            AudioController.PlaySound(SOUND.BOMB_BLAST);
        }

        private void TriggerFreeze(in Part part)
        {
            if (_triggerPartTimers == null || _triggerPartTimers.Count == 0)
                return;

            //If the bomb is still recharging, we tell the player that its unavailable
            if (_triggerPartTimers[part] > 0f)
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);
            
            if (!HasPartGrade(partRemoteData, out var freezeTime))
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            //Set the cooldown time
            if (partRemoteData.TryGetValue(PartProperties.KEYS.Cooldown, out float cooldown))
            {
                _triggerPartTimers[part] = cooldown;
            }

            partRemoteData.TryGetValue(PartProperties.KEYS.Radius, out int radius);

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
            if (_triggerPartTimers.IsNullOrEmpty())
                return;

            //If the bomb is still recharging, we tell the player that its unavailable
            if (_triggerPartTimers[part] > 0f)
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);

            if (!HasPartGrade(partRemoteData, out var seconds))
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            //Set the cooldown time
            if (partRemoteData.TryGetValue(PartProperties.KEYS.Cooldown, out float cooldown))
            {
                _triggerPartTimers[part] = cooldown;
            }

            //Set the shielded time
            _shieldTimers[part] = seconds;

            _shieldActive = true;
        }

        private void TriggerVampire(in Part part)
        {
            if (_triggerPartTimers.IsNullOrEmpty())
                return;

            //If the bomb is still recharging, we tell the player that its unavailable
            if (_triggerPartTimers[part] > 0f)
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);
            
            if (!HasPartGrade(partRemoteData, out var seconds))
            {
                AudioController.PlaySound(SOUND.BOMB_CLICK);
                return;
            }

            //Set the cooldown time
            if (partRemoteData.TryGetValue(PartProperties.KEYS.Cooldown, out float cooldown))
            {
                _triggerPartTimers[part] = cooldown;
            }

            //Set the vampirism time
            _vampireTimers[part] = seconds;

            _vampirismActive = true;
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
            
            var armors = bot.attachedBlocks
                .OfType<Part>()
                .Where(x => x.Type == PART_TYPE.ARMOR && x.Disabled == false)
                .ToArray();

            if (armors.IsNullOrEmpty())
                return false;
            
            var partRemoteData = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.ARMOR);

            if (!HasPartGrade(partRemoteData, out var multiplier))
            {
                return false;
            }

            for (int i = 0; i < armors.Length; i++)
            {
                damage *= 1.0f - multiplier;
            }

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

        //Patches
        //====================================================================================================================//

        private static float GetPatchMultiplier(in Part part, in PATCH_TYPE patchType)
        {
            //Find out
            var pType = (int) patchType;
            var patches = part.Patches.Where(x => x.Type == pType).ToList().AsReadOnly();
            
            if (patches.Count == 0)
                return 1;

            var remoteData = FactoryManager.Instance.PatchRemoteData;
            
            var total = 0f;
            foreach (var patchData in patches)
            {
                var data = remoteData.GetRemoteData(patchType)
                    .GetDataValue<float>(patchData.Level, PartProperties.KEYS.Probability);

                total += data;
            }

            switch (patchType)
            {
                case PATCH_TYPE.DAMAGE:
                case PATCH_TYPE.RANGE:
                    return 1 + total;
                case PATCH_TYPE.FIRE_RATE:
                case PATCH_TYPE.EFFICIENCY:
                    return  1 - total;
                
                case PATCH_TYPE.DURATION:
                case PATCH_TYPE.CRITICAL:
                case PATCH_TYPE.ELECTRIC:
                case PATCH_TYPE.CORROSIVE:
                case PATCH_TYPE.REINFORCED:
                case PATCH_TYPE.SPECIALIST:
                    throw new NotImplementedException($"{patchType} not yet implemented");
                default:
                    throw new ArgumentOutOfRangeException(nameof(patchType), patchType, null);
            }
        }

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
            foreach (var remoteData in beside.Select(part => partRemoteData))
            {
                if (!remoteData.TryGetValue(PartProperties.KEYS.Multiplier, out float mult))
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
            /*CheckShouldRecycle(ref _shields, (ShieldData data) =>
            {
                Recycler.Recycle<Shield>(data.gameObject);
            });*/
            
            CheckShouldRecycle(ref _flashes, (FlashSprite data) =>
            {
                Recycler.Recycle<FlashSprite>(data.gameObject);
            });
            CheckShouldRecycle(ref _triggerPartTimers, (Part part) =>
            {
                var index = _triggerParts.FindIndex(0, _triggerParts.Count, x => x == part);
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

        public float GetVampireValue()
        {
            var part = _parts.FirstOrDefault(x => x.Disabled == false && x.Type == PART_TYPE.VAMPIRE);
            
            if (part == null || part.Disabled)
                return 0f;

            return !HasPartGrade(part, out var value) ? 0f : value;
        }

        //====================================================================================================================//
        

        private bool HasPartGrade(in Part part, out float value)
        {
            var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);

            return HasPartGrade(partRemoteData, out value);
        }
        
        private bool HasPartGrade(in PartRemoteData partRemoteData, out float value)
        {
            var types = partRemoteData.partGrade.Types;
            var count = types.Count;
            
            var levels = new int[count];

            for (var i = 0; i < count; i++)
            {
                levels[i] = types[i] == BIT_TYPE.NONE
                    ? bot.attachedBlocks.GetHighestLevelBit()
                    : bot.attachedBlocks.GetHighestLevelBit(types[i]);
            }

            var minLevel = levels.Min();
            
            //var bitLevel = bot.attachedBlocks.GetHighestLevelBit(partRemoteData.partGrade.Type);
            var active = partRemoteData.HasPartGrade(minLevel, out value);

            return active;
        }
    }
}
