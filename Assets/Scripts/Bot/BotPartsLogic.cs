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
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Projectiles;
using StarSalvager.Utilities.Helpers;
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

        //====================================================================================================================//

        private class CooldownData
        {
            public float Value => current / max;

            public readonly float max;
            public float current { get; private set; }

            public CooldownData(in float max, in bool startNow = false)
            {
                this.max = max;
                current = startNow ? this.max : 0f;
            }

            public void StartCooldown()
            {
                current = max;
            }

            public bool HasCooldown(in bool update = true)
            {
                if (current <= 0f)
                {
                    current = 0;
                    return false;
                }

                if(update)
                    current -= Time.deltaTime;

                return true;
            }

        }

        //====================================================================================================================//

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

        //Fire Line Properties
        //====================================================================================================================//

        [SerializeField, Required, BoxGroup("Fire Line")]
        private LineRenderer fireLineRenderer;
        [SerializeField, Range(4, 90), BoxGroup("Fire Line"), LabelText("Gun Circle Segments")]
        private int slices = 36;

        //Magnet Properties
        //====================================================================================================================//

        [SerializeField, BoxGroup("Magnets")] public bool useMagnet = true;
        [SerializeField, BoxGroup("Magnets")] public MAGNET currentMagnet = MAGNET.DEFAULT;

        //==============================================================================================================//

        [ShowInInspector, BoxGroup("Bot Part Data"), ReadOnly]
        public int MagnetCount { get; private set; }

        //==============================================================================================================//

        private bool _shieldActive;

        private GameObject _shieldObject;
        private GameObject _healObject;
        private float _healActiveTimer;

        private Dictionary<Part, Transform> _turrets;
        private Dictionary<Part, CollidableBase> _gunTargets;

        private List<Part> _parts;
        private List<Part> _triggerParts;

        private Dictionary<Part, bool> _playingSounds;

        private Dictionary<Part, FlashSprite> _flashes;

        private Dictionary<Part, CooldownData> _partCooldownTimers;
        private Dictionary<Part, float> _shieldTimers;


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

        #region Unity Functions

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
        //==============================================================================================================//
        public void TrySwapPart(in DIRECTION direction)
        {
            BIT_TYPE category;
            switch (direction)
            {
                case DIRECTION.UP:
                    category = Constants.BIT_ORDER[0];
                    break;
                case DIRECTION.DOWN:
                    category = Constants.BIT_ORDER[1];
                    break;
                case DIRECTION.LEFT:
                    category = Constants.BIT_ORDER[3];
                    break;
                case DIRECTION.RIGHT:
                    category = Constants.BIT_ORDER[4];
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            TrySwapPart(category);
        }
        public void TrySwapPart(in BIT_TYPE category)
        {
            var temp = category;
            var partIndex = bot.AttachedBlocks.FindIndex(x =>
                x is Part p && p.Type != PART_TYPE.EMPTY &&  p.category == temp);


            if (partIndex < 0 || !(bot.AttachedBlocks[partIndex] is Part part))
                return;

            var partsRemote = FactoryManager.Instance.PartsRemoteData;

            //Get list of available parts that are
            var storedPartData = PlayerDataManager
                .GetCurrentPartsInStorage()
                .OfType<PartData>()
                .Where(x => x.Type != (int) part.Type)
                .FirstOrDefault(x => part.category == partsRemote.GetRemoteData((PART_TYPE) x.Type).category);

            if(storedPartData.Type == 0)
                return;

            StartCoroutine(PartSwapCoroutine(part, storedPartData));

        }

        private IEnumerator PartSwapCoroutine(Part part, PartData swapToPart)
        {
            var cooldown = Globals.PartSwapTime / 2f;
            var t = cooldown;
            var startScale = Vector3.one;
            var endScale = new Vector3(1, 0f, 1f);

            //--------------------------------------------------------------------------------------------------------//

            _partCooldownTimers[part] = new CooldownData(cooldown, true);
            var targetTrans = part.transform;

            while (t > 0f)
            {
                targetTrans.localScale = Vector3.Lerp(startScale, endScale, 1f - (t / cooldown));

                t -= Time.deltaTime;

                yield return null;
            }

            //--------------------------------------------------------------------------------------------------------//

            var coordinate = part.Coordinate;
            var pos = part.Position;

            PlayerDataManager.RemovePartFromStorage(swapToPart);

            var blockData = part.ToBlockData();
            blockData.Coordinate = Vector2Int.zero;

            PlayerDataManager.AddPartToStorage(blockData);
            Recycler.Recycle<Part>(part);

            var newPart = FactoryManager.Instance
                .GetFactory<PartAttachableFactory>()
                .CreateObject<Part>(swapToPart);

            ClearList();

            bot.AttachNewBlock(coordinate, newPart,
                false,
                false,
                false,
                false,
                true);

            PlayerDataManager.SetBlockData(bot.GetBlockDatas());

            newPart.transform.localScale = endScale;
            newPart.transform.position = pos;

            //--------------------------------------------------------------------------------------------------------//

            //_partCooldownTimers[newPart] = cooldown;
            _partCooldownTimers[newPart] = new CooldownData(cooldown, true);
            targetTrans = newPart.transform;
            t = cooldown;

            while (t > 0f)
            {
                targetTrans.localScale = Vector3.Lerp(endScale, startScale, 1f - (t / cooldown));

                t -= Time.deltaTime;

                yield return null;
            }

            var partRemoteData = FactoryManager.Instance.PartsRemoteData.GetRemoteData(newPart.Type);

            //Checks if the part will need to use a cooldown value, if not we'll remove it
            if(TryGetPartProperty(PartProperties.KEYS.Cooldown, newPart, partRemoteData, out cooldown))
                _partCooldownTimers[newPart] = new CooldownData(cooldown);
            else
                _partCooldownTimers.Remove(newPart);

            //--------------------------------------------------------------------------------------------------------//

        }

        //====================================================================================================================//


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


            //--------------------------------------------------------------------------------------------------------//

            void SetIcon(in int index, in BIT_TYPE bitType)
            {
                var type = bitType;
                var part = _parts.FirstOrDefault(x => x.category == type);

                GameUI.SetIconImage(index, part is null ? PART_TYPE.EMPTY : part.Type);


                var partsInStorage = PlayerDataManager.GetCurrentPartsInStorage();
                var switchPart = partsInStorage
                    .OfType<PartData>()
                    .Select(x => new
                    {
                        type = (PART_TYPE)x.Type,
                        partRemote.GetRemoteData(x.Type).category
                    })
                    .FirstOrDefault(x => x.category == type);


                GameUI.SetSecondIconImage(index, switchPart is null? PART_TYPE.EMPTY : switchPart.type);
            }

            //--------------------------------------------------------------------------------------------------------//

            if (_partCooldownTimers == null)
                _partCooldownTimers = new Dictionary<Part, CooldownData>();

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

                if (TryGetPartProperty(PartProperties.KEYS.Cooldown, part, partRemoteData, out var cooldown) && !_partCooldownTimers.ContainsKey(part))
                {
                    _partCooldownTimers.Add(part, new CooldownData(cooldown));
                }

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

                        _corePart = part;
                        PlayerDataManager.OnCapacitiesChanged?.Invoke();

                        break;
                    case PART_TYPE.SHIELD:
                        if (_shieldTimers == null)
                            _shieldTimers = new Dictionary<Part, float>();

                        if (_shieldTimers.ContainsKey(part))
                            break;

                        _shieldTimers.Add(part, 0f);
                        break;

                    case PART_TYPE.REPAIR:
                        _repairTarget.Add(part, null);
                        break;
                    case PART_TYPE.GUN:
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.MISSILE:
                    InitFireLine(part, partRemoteData);

                        /*if (!_partCooldownTimers.ContainsKey(part))
                        {
                            var cooldown = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Cooldown);

                            _partCooldownTimers.Add(part, new CooldownData(cooldown * cooldownBoost));
                        }*/

                        _gunTargets.Add(part, null);

                        if (Globals.UseCenterFiring == false && ShouldUseGunTurret(partRemoteData))
                            CreateTurretEffect(part);
                        break;
                    case PART_TYPE.BLASTER:
                    case PART_TYPE.RAILGUN:
                        InitFireLine(part, partRemoteData);
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

            /*foreach (var triggerPart in _triggerParts)
            {
                var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>()
                    .GetRemoteData(triggerPart.Type);

                if (_partCooldownTimers.ContainsKey(triggerPart))
                    continue;

                if (!partRemoteData.TryGetValue(PartProperties.KEYS.Cooldown, out float triggerCooldown))
                    throw new ArgumentException($"Remote data for {partRemoteData.name} does not contain a value for {nameof(PartProperties.KEYS.Cooldown)}");

                _partCooldownTimers.Add(triggerPart, new CooldownData(triggerCooldown));
            }*/

            SetupGunRangeValues();

            /*if (!_turrets.IsNullOrEmpty())
            {
                foreach (var kvp in _turrets)
                {
                    var turret = kvp.Value.gameObject;
                    turret.GetComponent<SpriteRenderer>().color = kvp.Key.Disabled ? Color.gray : Color.white;
                }
            }*/

            bot.ForceCheckMagnets();
        }

        #endregion //Init Parts

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
                        //BlasterUpdate(part, partRemoteData, deltaTime);
                        UpdateFireLine(part, partRemoteData);
                        break;
                    //------------------------------------------------------------------------------------------------//
                    case PART_TYPE.SNIPER:
                    case PART_TYPE.MISSILE:
                    case PART_TYPE.TRIPLESHOT:
                    case PART_TYPE.GUN:
                        GunUpdate(part, partRemoteData, deltaTime);
                        UpdateFireLine(part, partRemoteData);
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
                    //--------------------------------------------------------------------------------------------------------//
                    case PART_TYPE.RAILGUN:
                        UpdateFireLine(part, partRemoteData);
                        break;
                    //--------------------------------------------------------------------------------------------------------//
                }

                if (!_partCooldownTimers.TryGetValue(part, out var cooldownData))
                    continue;

                //Wait for the shield to be inactive before the cooldown can begin
                if (part.Type == PART_TYPE.SHIELD && _shieldActive)
                    return;

                //Wait for the Sabre to be inactive before the cooldown can begin
                if (part.Type == PART_TYPE.SABRE && _sabreActive)
                    return;

                foreach (var triggerPart in _triggerParts)
                {
                    //var partRemoteData = GetPartData(triggerPart);
                    TriggerPartUpdates(triggerPart, null, deltaTime);
                }

                var uiIndex = Constants.BIT_ORDER.ToList().FindIndex(x => x == partRemoteData.category);
                var fill = 1f - cooldownData.Value;
                GameUI.SetFill(uiIndex, fill);
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
            //var timer = _partCooldownTimers[part];

            if (_partCooldownTimers[part].HasCooldown())
            {
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



            if (!TryGetPartProperty(PartProperties.KEYS.Heal, part, partRemoteData, out var repairAmount))
                throw new ArgumentOutOfRangeException();

            _partCooldownTimers[part].StartCooldown();
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

        /*private void BlasterUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            //--------------------------------------------------------------------------------------------//
            /*if (!_partCooldownTimers.ContainsKey(part))
            {
                var cooldown = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Cooldown);

                _partCooldownTimers.Add(part, new CooldownData(cooldown));
            }*/

            //Cooldown
            //--------------------------------------------------------------------------------------------//



            if (_partCooldownTimers[part].HasCooldown())
            {
                return;
            }

            _partCooldownTimers[part].StartCooldown();

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
        }*/

        private void GunUpdate(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {
            //TODO Need to determine if the shoot type is looking for enemies or not
            //--------------------------------------------------------------------------------------------//



            //Aim the Turret Effect
            //--------------------------------------------------------------------------------------------//

            var target = _gunTargets[part];

            /*if (target &&
                target.IsRecycled == false &&
                /*!_turrets.IsNullOrEmpty() &&
                _turrets.TryGetValue(part, out var turretTransform)#1#)
            {
                var targetTransform = target.transform;
                var normDirection = (targetTransform.position - part.transform.position).normalized;
                turretTransform.up = normDirection;
            }
            else */if (target && target.IsRecycled)
            {
                _gunTargets[part] = null;
            }

            //TODO This needs to fire every x Seconds
            //--------------------------------------------------------------------------------------------//

            //FIXME This now might more sense to count down instead of counting up


            if (_partCooldownTimers[part].HasCooldown())
            {
                return;
            }


            //Check if we have a target before removing resources
            //--------------------------------------------------------------------------------------------//

            if (!_gunRanges.TryGetValue(part, out var range))
            {
                range = 150f;
            }

            //TODO: Make us able to pass multiple tags so a shot can hit multiple types of targets
            CollidableBase fireTarget = EnemyManager.GetClosestEnemy(part.transform.position, range);
            var tag = TagsHelper.ENEMY;
            //TODO Determine if this fires at all times or just when there are active enemies in range
            if (fireTarget == null)
            {
                fireTarget = LevelManager.Instance.ObstacleManager.GetClosestDestructableCollidable(part.transform.position, range);
                if (fireTarget == null)
                {
                    return;
                }

                if (fireTarget is SpaceJunk)
                {
                    tag = TagsHelper.SPACE_JUNK;
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

            _partCooldownTimers[part].StartCooldown();

            //--------------------------------------------------------------------------------------------//
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

        private void CreateProjectile(in Part part, in PartRemoteData partRemoteData, in CollidableBase collidableTarget, params string[] collisionTags)
        {
            var patches = part.Patches;
            var rangeBoost = patches.GetPatchMultiplier(PATCH_TYPE.RANGE);
            //var damageBoost = patches.GetPatchMultiplier(PATCH_TYPE.POWER);


            var projectileId = partRemoteData.GetDataValue<string>(PartProperties.KEYS.Projectile);

            var damage = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Damage);


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
                    collisionTags,
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
            var projectileProfile = ProjectileFactory.GetProfile(projectileId);

            var totarget = target.Position - partPosition;

            var a = Vector3.Dot(targetVelocity, targetVelocity) - (projectileProfile.ProjectileSpeed * projectileProfile.ProjectileSpeed);
            var b = 2 * Vector3.Dot(targetVelocity, totarget);
            var c = Vector3.Dot(totarget, totarget);

            var p = -b / (2 * a);
            var q = (float)Math.Sqrt((b * b) - 4 * a * c) / (2 * a);

            /*if (float.IsNaN(q))
                return totarget.normalized;*/

            var t1 = p - q;
            var t2 = p + q;
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
#if UNITY_EDITOR
            Debug.DrawRay(partPosition, totarget.normalized * 10, Color.yellow, 1f);
            Debug.DrawRay(partPosition, bulletPath.normalized * 10, Color.green, 1f);
#endif


            return bulletPath;
        }

        private float GetProjectileRange(in Part part, in string projectileID)
        {
            var projectileData = ProjectileFactory.GetProfile(projectileID);

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

            var projectileData = ProjectileFactory.GetProfile(projectileId);

            return !(projectileData is null) && projectileData.FireAtTarget;
        }

        #endregion //Weapons

        //============================================================================================================//

        #region Trigger Parts

        private void TriggerPartUpdates(in Part part, in PartRemoteData partRemoteData, in float deltaTime)
        {

            if (!_partCooldownTimers.TryGetValue(part, out var cooldownData))
                return;

            if (!cooldownData.HasCooldown())
                return;

            //Wait for the shield to be inactive before the cooldown can begin
            if (part.Type == PART_TYPE.SHIELD && _shieldActive)
                return;

            //Find the index of the ui element to show cooldown
            var tempPart = part;
            var uiIndex = Constants.BIT_ORDER.ToList().FindIndex(x => x == tempPart.category);//_triggerParts.FindIndex(0, _triggerParts.Count, x => x == tempPart);

            //Get the max cooldown value
            //--------------------------------------------------------------------------------------------------------//

            /*if (!partRemoteData.TryGetValue(PartProperties.KEYS.Cooldown, out float triggerCooldown))
                throw new ArgumentException($"Remote data for {partRemoteData.name} does not contain a value for {nameof(PartProperties.KEYS.Cooldown)}");*/

            //Update the timer value for this frame
            //--------------------------------------------------------------------------------------------------------//

            var fill = 1f - cooldownData.Value;
            GameUI.SetFill(uiIndex, fill);

            //--------------------------------------------------------------------------------------------------------//
        }
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
                case PART_TYPE.BLASTER:
                    TriggerBlaster(part);
                    break;
                case PART_TYPE.BLITZ:
                    TriggerBlitz(part);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(Part.Type), _triggerParts[index].Type, null);
            }
        }

        //====================================================================================================================//

        private bool CanUseTriggerPart(in Part part, out PartRemoteData partRemoteData, in bool useTriggerOnTrue = true)
        {
            partRemoteData = null;

            if (_partCooldownTimers.IsNullOrEmpty())
                return false;

            //If the bomb is still recharging, we tell the player that its unavailable
            if (_partCooldownTimers[part].HasCooldown(false))
            {
                return false;
            }

            partRemoteData = FactoryManager.Instance
                .GetFactory<PartAttachableFactory>()
                .GetRemoteData(part.Type);

            /*if (!partRemoteData.TryGetValue<float>(PartProperties.KEYS.Cooldown, out var cooldown))
            {
                throw new MissingFieldException($"{PartProperties.KEYS.Cooldown} missing from {part.Type} remote data");
            }*/

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

            _partCooldownTimers[part].StartCooldown();

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
            //--------------------------------------------------------------------------------------------------------//

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

            //--------------------------------------------------------------------------------------------------------//

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
            const float WIDTH = 5f;
            const float LENGTH = 50f;
            const float TIME = 0.4f;

            if (bot.Rotating)
                return;

            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;
            //--------------------------------------------------------------------------------------------------------//
            var startPosition = Globals.UseCenterFiring ? bot.transform.position : part.Position;

            var direction = Vector3.up;

            var lineShrink = FactoryManager.Instance
                .GetFactory<EffectFactory>()
                .CreateObject<LineShrink>();

            lineShrink.Init(startPosition, startPosition + direction * LENGTH, WIDTH, TIME);

            AudioController.PlaySound(SOUND.BIT_EXPLODE);

            bot.cinemachineImpulseSource.GenerateImpulse(Random.Range(1f, 2f));
            GameUI.FlashNeonBorder(Random.Range(TIME,  TIME * 2f));

            var currentPos = startPosition + direction * (LENGTH / 2f);
            var size = new Vector2(WIDTH, LENGTH);
            var enemies = EnemyManager.GetEnemiesInBounds(new Bounds(currentPos, size));

            if (enemies.Count <= 0)
                return;

            TryGetPartProperty(PartProperties.KEYS.Damage, part, partRemoteData, out var damage);

            foreach (var enemy in enemies)
            {
                enemy.TryHitAt(enemy.Position, damage);
            }

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
            if (!TryGetPartProperty(PartProperties.KEYS.Damage, part, partRemoteData, out var damage))
                throw new ArgumentOutOfRangeException($"Missing {nameof(PartProperties.KEYS.Damage)} on {partRemoteData.name}");

            EnemyManager.DamageAllEnemies(damage);

            var bits = LevelManager.Instance.ObstacleManager.TryGetBitsOnScreen();
            for (var i = 0; i < bits.Count; i++)
            {
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

                for (var i = 0; i <= 5; i++)
                {
                    var startIndex = startIndices[i];

                    var count = 3 + i;
                    var isOdd = count % 2 == 1;

                    var offsetAmount = isOdd ? new Vector2Int(-1, 1) : new Vector2Int(1, -1);

                    var totalOffset = new Vector2Int(0, 0);
                    var flip = true;

                    var coordinates = GetCoordinates(i, offsets, startCoordinate);

                    for (var ii = 0; ii < count; ii++)
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

        //[SerializeField]
        //private BlasterProjectile blasterProjectilePrefab;
        private void TriggerBlaster(in Part part)
        {

            //--------------------------------------------------------------------------------------------------------//

            BlasterProjectile CreateBlasterEffect()
            {
                return FactoryManager.Instance
                    .GetFactory<EffectFactory>()
                    .CreateEffect(EffectFactory.EFFECT.CURVE_LINE)
                    .GetComponent<BlasterProjectile>();
            }

            //--------------------------------------------------------------------------------------------------------//

            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;

            var fromPosition = Globals.UseCenterFiring ? bot.transform.position : part.Position;

            var range = partRemoteData.GetDataValue<int>(PartProperties.KEYS.Radius);
            var degrees = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Degrees);
            var fireTime = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Time);
            var damage = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Damage);

            var rot = part.transform.eulerAngles.z + 90;

            var blasterProjectile = CreateBlasterEffect();
            blasterProjectile.transform.position = fromPosition;
            blasterProjectile.Init(rot, degrees, range, fireTime);

            var dotThreshold = 1f / (180 / degrees);
            var enemies = EnemyManager.GetEnemiesInCone(fromPosition, range, part.transform.up.normalized, dotThreshold);
            foreach (var enemy in enemies)
            {
                enemy.TryHitAt(enemy.Position, damage);
            }

        }

        private void TriggerBlitz(in Part part)
        {

            if (!CanUseTriggerPart(part, out var partRemoteData))
                return;

            var fromPosition = Globals.UseCenterFiring ? bot.transform.position : part.Position;

            var projectile = partRemoteData.GetDataValue<string>(PartProperties.KEYS.Projectile);
            var damage = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Damage);

            CreateProjectile(part, partRemoteData, null, TagsHelper.ENEMY, TagsHelper.ASTEROID);


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

            var partRemoteData = PART_TYPE.ARMOR.GetRemoteData();

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

        #region Ammo

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

        #endregion //Ammo

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
            CheckShouldRecycle(ref _flashes, (FlashSprite data) =>
            {
                Recycler.Recycle<FlashSprite>(data.gameObject);
            });
            CheckShouldRecycle(ref _partCooldownTimers, (Part _) => { });
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

        /*private FlashSprite GetAlertIcon(Part part)
        {
            if(_flashes == null)
                _flashes = new Dictionary<Part, FlashSprite>();

            if (_flashes.ContainsKey(part))
                return _flashes[part];


            var bitColor = part.Type.GetCategory().GetColor();

            var flash = FlashSprite.Create(part.transform, Vector3.zero, bitColor);

            _flashes.Add(part, flash);

            return _flashes[part];
        }*/

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
            /*if (!_turrets.IsNullOrEmpty())
            {
                var turrets = _turrets.Values;
                foreach (var turret in turrets)
                {
                    if(!turret)
                        continue;

                    Destroy(turret.gameObject);
                }

                _turrets = new Dictionary<Part, Transform>();
            }*/

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
            /*CheckShouldRecycle(ref _turrets, (Transform data) =>
            {
                Destroy(data.gameObject);
            });*/

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

        //Fireline
        //====================================================================================================================//

        private void InitFireLine(in Part part, in PartRemoteData partRemoteData)
        {
            var firePosition = Globals.UseCenterFiring ? Vector3.zero : part.transform.localPosition;

            var loop = false;
            var worldSpace = false;
            Vector3[] points;

            switch (part.Type)
            {
                case PART_TYPE.SABRE:
                case PART_TYPE.SNIPER:
                    fireLineRenderer.gameObject.SetActive(false);
                    return;
                case PART_TYPE.GUN:
                {
                    var projectileId = partRemoteData.GetDataValue<string>(PartProperties.KEYS.Projectile);
                    var range = FactoryManager.Instance.ProjectileProfile.GetProjectileProfileData(projectileId)
                        .ProjectileRange;

                    var pointList = new List<Vector3>();
                    var degree = 360f / slices;

                    for (var i = 0; i < slices; i++)
                    {
                        var point = firePosition + (Vector3)Mathfx.GetAsPointOnCircle(i * degree, range);
                        pointList.Add(point);
                    }

                    loop = true;
                    points = pointList.ToArray();
                    break;
                }
                case PART_TYPE.BLASTER:
                {
                    //Need to take into consideration the current rotation of the blaster in case the part is reinitialized after rotation
                    var rot = part.transform.eulerAngles.z + 90;

                    var degrees = partRemoteData.GetDataValue<float>(PartProperties.KEYS.Degrees);
                    var range = partRemoteData.GetDataValue<int>(PartProperties.KEYS.Radius);

                    var rightDeg = (degrees / 2f) + rot;
                    var leftDeg = (360f - (degrees / 2f)) + rot;

                    points = new[]
                    {
                        (Vector3)Mathfx.GetAsPointOnCircle(leftDeg, range),
                        firePosition,
                        (Vector3)Mathfx.GetAsPointOnCircle(rightDeg, range),
                    };
                    break;
                }
                case PART_TYPE.RAILGUN:
                    points = new[]
                    {
                        firePosition,
                        Vector3.up * 100
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            fireLineRenderer.gameObject.SetActive(true);
            fireLineRenderer.loop = loop;
            fireLineRenderer.useWorldSpace = worldSpace;
            fireLineRenderer.positionCount = points.Length;
            fireLineRenderer.SetPositions(points);
        }

        private void UpdateFireLine(in Part part, in PartRemoteData partRemoteData)
        {
            switch (part.Type)
            {
                case PART_TYPE.BLASTER:
                case PART_TYPE.SABRE:
                case PART_TYPE.SNIPER:
                    return;
                case PART_TYPE.GUN:
                case PART_TYPE.RAILGUN:
                    fireLineRenderer.transform.rotation = Quaternion.identity;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

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
