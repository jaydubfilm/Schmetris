using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Values;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using StarSalvager.Utilities.Math;
using StarSalvager.Utilities.Saving;
using StarSalvager.Factories.Data;
using StarSalvager.Parts.Data;

namespace StarSalvager
{
    public class ScrapyardBot : MonoBehaviour, ICustomRecycle
    {
        [SerializeField, BoxGroup("PROTOTYPE")]
        public float testRotSpeed;

        //============================================================================================================//

        public List<IAttachable> AttachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());

        [SerializeField, ReadOnly, Space(10f), ShowInInspector]
        private List<IAttachable> _attachedBlocks;

        private List<ScrapyardPart> _parts;

        //============================================================================================================//
        public bool Rotating => _rotating;

        private bool _rotating;
        private float _targetRotation;

        //============================================================================================================//

        private new Rigidbody2D Rigidbody
        {
            get
            {
                if (!_rigidbody)
                    _rigidbody = GetComponent<Rigidbody2D>();

                return _rigidbody;
            }
        }
        private Rigidbody2D _rigidbody;

        //============================================================================================================//

        #region Unity Functions

        private void FixedUpdate()
        {
            if (Rotating)
                RotateBot();
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Init Bot

        public void InitBot()
        {
            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            //var startingHealth = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.CORE).levels[0].health;
            //Add core component
            var core = partFactory.CreateScrapyardObject<ScrapyardPart>(
                new PartData
                {
                    Type = (int)PART_TYPE.CORE,
                    Coordinate = Vector2Int.zero,
                });

            if(Globals.IsRecoveryBot) partFactory.SetOverrideSprite(core, PART_TYPE.RECOVERY);

            AttachNewBit(Vector2Int.zero, core);
        }

        public void InitBot(IEnumerable<IAttachable> botAttachables)
        {
            foreach (var attachable in botAttachables)
            {
                if(attachable is Part part && part.Type == PART_TYPE.CORE && Globals.IsRecoveryBot)
                    FactoryManager.Instance.GetFactory<PartAttachableFactory>().SetOverrideSprite(part, PART_TYPE.RECOVERY);
                else if(attachable is ScrapyardPart scrapyardPart && scrapyardPart.Type == PART_TYPE.CORE && Globals.IsRecoveryBot)
                    FactoryManager.Instance.GetFactory<PartAttachableFactory>().SetOverrideSprite(scrapyardPart, PART_TYPE.RECOVERY);

                AttachNewBit(attachable.Coordinate, attachable);
            }
        }

        #endregion // Init Bot

        //============================================================================================================//

        #region Input Solver

        public void Rotate(float direction)
        {
            if (Input.GetKey(KeyCode.LeftAlt))
                return;

            if (direction < 0)
                Rotate(ROTATION.CCW);
            else if (direction > 0)
                Rotate(ROTATION.CW);
        }

        /// <summary>
        /// Triggers a rotation 90deg in the specified direction. If the player is already rotating, it adds 90deg onto
        /// the target rotation.
        /// </summary>
        /// <param name="rotation"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void Rotate(ROTATION rotation)
        {
            float toRotate = rotation.ToAngle();

            //TODO Need to do the angle clamps here to prevent TargetRotation from going over bounds

            //If we're already rotating, we need to add the direction to the target
            if (Rotating)
            {
                _targetRotation += toRotate;
            }
            else
            {
                _targetRotation = Rigidbody.rotation + toRotate;
            }

            _targetRotation = MathS.ClampAngle(_targetRotation);

            foreach (var attachedBlock in AttachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;



        }

        #endregion //Input Solver

        //============================================================================================================//

        #region Movement

        private bool _rotate;

        private void RotateBot()
        {
            var rotation = Rigidbody.rotation;

            //Rotates towards the target rotation.
            float rotationAmount;
            rotationAmount = Globals.BotRotationSpeed;
            rotation = Mathf.MoveTowardsAngle(rotation, _targetRotation, rotationAmount * Time.fixedDeltaTime);
            Rigidbody.rotation = rotation;

            //Here we check how close to the final rotation we are.
            var remainingDegrees = Mathf.Abs(Mathf.DeltaAngle(rotation, _targetRotation));

            //TODO Here we'll need to rotate the sprites & Coordinates after a certain threshold is met for that rotation

            if (remainingDegrees > 10f)
                return;

            TryRotateBits();


            //If we're within 1deg we will count it as complete, otherwise continue to rotate.
            if (remainingDegrees > 1f)
                return;

            //Ensures that the Attachables are correctly rotated
            //NOTE: This is a strict order-of-operations as changing will cause rotations to be incorrect
            //--------------------------------------------------------------------------------------------------------//
            //Force set the rotation to the target, in case the bot is not exactly on target
            Rigidbody.rotation = _targetRotation;
            _targetRotation = 0f;


            TryRotateBits();
            _rotate = false;
            _rotating = false;
        }

        private void TryRotateBits()
        {
            if (_rotate)
                return;

            var check = (int)_targetRotation;
            float deg;
            if (check == 180)
            {
                deg = 180;
            }
            else if (check == 0f || check == 360)
            {
                deg = 0;
            }
            else
            {
                deg = _targetRotation + 180;
            }

            var rot = Quaternion.Euler(0, 0, deg);

            foreach (var attachedBlock in AttachedBlocks)
            {
                if (attachedBlock is ICustomRotate customRotate)
                {
                    customRotate.CustomRotate(rot);
                    continue;
                }

                //attachedBlock.RotateSprite(MostRecentRotate);
                attachedBlock.transform.localRotation = rot;
            }

            _rotate = true;
        }

        #endregion //Movement

        //============================================================================================================//

        #region Attach Bits

        public void AttachNewBit(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            //newAttachable.gameObject.name = $"Block {AttachedBlocks.Count}";
            AttachedBlocks.Add(newAttachable);

            switch (newAttachable)
            {
                case Part _:
                    throw new ArgumentOutOfRangeException(nameof(newAttachable), newAttachable, null);
                case ScrapyardPart _:
                    UpdatePartsList();
                    break;
            }

        }

        #endregion //Attach Bits

        //============================================================================================================//

        #region Detach Bits

        public void TryRemoveAttachableAt(Vector2Int coordinate, bool refund)
        {
            var attachable = AttachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate);
            //TODO - think of a better place to handle this selling event
            if (refund)
            {
                switch (attachable)
                {
                    case ScrapyardBit _:
                        throw new ArgumentOutOfRangeException(nameof(attachable), attachable, null);
                    case ScrapyardPart scrapyardPart:
                        PlayerDataManager.AddPartResources(scrapyardPart.Type, 0, true);
                        UpdatePartsList();
                        break;
                }
            }

            if (attachable is null)
                return;

            DestroyAttachable(attachable);
            UpdatePartsList();
        }

        public void RemoveDetachables()
        {
            for (int i = AttachedBlocks.Count - 1; i >= 0; i--)
            {
                switch (AttachedBlocks[i])
                {
                    case Component _:
                    case ScrapyardBit _:
                        DetachBit(AttachedBlocks[i]);
                        break;
                }
            }
        }

        /*public void RemoveAllBits()
        {
            for (int i = AttachedBlocks.Count - 1; i >= 0; i--)
            {
                if (AttachedBlocks[i] is ScrapyardBit)
                {
                    DetachBit(AttachedBlocks[i]);
                }
            }
        }

        public void RemoveAllComponents()
        {
            for (int i = AttachedBlocks.Count - 1; i >= 0; i--)
            {
                if (AttachedBlocks[i] is Component)
                {
                    DetachBit(AttachedBlocks[i]);
                }
            }
        }*/

        private void DetachBit(IAttachable attachable)
        {
            attachable.transform.parent = null;

            DestroyAttachable(attachable);
        }

        private void RemoveAttachable(IAttachable attachable)
        {
            AttachedBlocks.Remove(attachable);
            attachable.SetAttached(false);
        }

        private void DestroyAttachable(IAttachable attachable, bool refundCost = true)
        {
            AttachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

            switch (attachable)
            {
                case ScrapyardBit _:
                    Recycler.Recycle<ScrapyardBit>(attachable.gameObject);
                    break;
                case ScrapyardPart _:
                    Recycler.Recycle<ScrapyardPart>(attachable.gameObject);
                    break;
                case Component _:
                    Recycler.Recycle<Component>(attachable.gameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attachable), attachable, null);
            }
        }

        /// <summary>
        /// Removes the attachable, and will recycle it under the T bin.
        /// </summary>
        /// <param name="attachable"></param>
        /// <typeparam name="T"></typeparam>
        private void DestroyAttachable<T>(IAttachable attachable) where T : IAttachable
        {
            AttachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

            Recycler.Recycle<T>(attachable.gameObject);
        }

        #endregion //Detach Bits

        //============================================================================================================//

        #region Check for New Disconnects

        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        public bool CheckHasDisconnects()
        {
            var toSolve = new List<IAttachable>(AttachedBlocks);

            foreach (var attachableBase in toSolve)
            {
                if (!AttachedBlocks.Contains(attachableBase))
                    continue;

                var hasPathToCore = AttachedBlocks.HasPathToCore(attachableBase);

                if (hasPathToCore)
                    continue;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        /*public void CheckForDisconnects()
        {
            var toSolve = new List<IAttachable>(attachedBlocks);

            foreach (var attachableBase in toSolve)
            {
                if (!attachedBlocks.Contains(attachableBase))
                    continue;

                var hasPathToCore = attachedBlocks.HasPathToCore(attachableBase);

                if (hasPathToCore)
                    continue;

                var attachedBits = new List<IAttachable>();
                attachedBlocks.GetAllAttachedBits(attachableBase, null, ref attachedBits);

                if (attachedBits.Count == 1)
                {
                    DetachBit(attachedBits[0]);
                    continue;
                }


                DetachBits(attachedBits);
            }
        }*/

        #endregion //Check for New Disconnects

        //============================================================================================================//

        #region Parts

        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        private float coreHeat;
        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("/s", Overlay = true)]
        private float coolSpeed;
        [SerializeField, BoxGroup("Bot Part Data"), DisableInPlayMode, SuffixLabel("s", Overlay = true)]
        private float coolDelay;
        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly]
        private float coolTimer;

        [SerializeField, BoxGroup("Bot Part Data"), ReadOnly, Space(10f)]
        private int magnetCount;

        public float PowerDraw { get; private set; }

        private int MAXParts { get; set; }

        public bool AtPartCapacity => _parts.Count >= MAXParts + 1;
        public string PartCapacity => $"{_parts.Count - 1 }/{MAXParts }";

        public List<BIT_TYPE> UsedResourceTypes { get; private set; }

        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        private void UpdatePartsList()
        {
            _parts = AttachedBlocks.OfType<ScrapyardPart>().ToList();

            UpdatePartData();
        }

        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void UpdatePartData()
        {
            magnetCount = 0;
            MAXParts = 0;
            PowerDraw = 0f;

            var liquidCapacities = new Dictionary<BIT_TYPE, int>
            {
                {BIT_TYPE.RED, 0},
                {BIT_TYPE.BLUE, 0},
                {BIT_TYPE.YELLOW, 0},
                {BIT_TYPE.GREEN, 0},
                {BIT_TYPE.GREY, 0},
            };

            UsedResourceTypes = new List<BIT_TYPE>();

            foreach (var part in _parts)
            {
                int value;

                var partRemoteData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type);

                PowerDraw += partRemoteData.powerDraw;

                if(!UsedResourceTypes.Contains(partRemoteData.burnType))
                    UsedResourceTypes.Add(partRemoteData.burnType);

                if(partRemoteData.powerDraw > 0f && !UsedResourceTypes.Contains(BIT_TYPE.YELLOW))
                    UsedResourceTypes.Add(BIT_TYPE.YELLOW);

                switch (part.Type)
                {
                    case PART_TYPE.CORE:

                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.RED] += value;
                            liquidCapacities[BIT_TYPE.GREEN] += value;
                            liquidCapacities[BIT_TYPE.GREY] += value;
                            liquidCapacities[BIT_TYPE.YELLOW] += value;
                            liquidCapacities[BIT_TYPE.BLUE] += value;
                        }

                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Magnet, out value))
                        {
                            magnetCount += value;
                        }

                        if (partRemoteData.TryGetValue(PartProperties.KEYS.PartCapacity, out int intValue))
                        {
                            MAXParts = intValue;
                        }
                        break;
                    case PART_TYPE.MAGNET:

                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Magnet, out value))
                        {
                            magnetCount += value;
                        }
                        break;
                    //Determine if we need to setup the shield elements for the bot
                    //FIXME I'll need a way of disposing of the shield visual object
                    case PART_TYPE.SHIELD:
                        break;
                    case PART_TYPE.STORE:
                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.RED] += value;
                            liquidCapacities[BIT_TYPE.GREEN] += value;
                            liquidCapacities[BIT_TYPE.GREY] += value;
                        }
                        break;
                    case PART_TYPE.STORERED:
                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.RED] += value;
                        }
                        break;
                    case PART_TYPE.STOREGREEN:
                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.GREEN] += value;
                        }
                        break;
                    case PART_TYPE.STOREGREY:
                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.GREY] += value;
                        }
                        break;
                    case PART_TYPE.STOREYELLOW:
                        if (partRemoteData.TryGetValue(PartProperties.KEYS.Capacity, out value))
                        {
                            liquidCapacities[BIT_TYPE.YELLOW] += value;
                        }
                        break;
                }
            }

            //Force update capacities, once new values determined
            foreach (var capacity in liquidCapacities)
            {
                PlayerDataManager.GetResource(capacity.Key).SetLiquidCapacity(capacity.Value);
            }
        }

        #endregion //Parts

        //============================================================================================================//

        #region Custom Recycle

        public void CustomRecycle(params object[] args)
        {
            foreach (var attachable in AttachedBlocks)
            {
                switch (attachable)
                {
                    case ScrapyardBit _:
                        Recycler.Recycle<ScrapyardBit>(attachable.gameObject);
                        break;
                    case ScrapyardPart _:
                        Recycler.Recycle<ScrapyardPart>(attachable.gameObject);
                        break;
                    default:
                        Destroy(attachable.gameObject);
                        break;
                }
            }

            AttachedBlocks.Clear();
        }

        #endregion //Custom Recycle
    }
}
