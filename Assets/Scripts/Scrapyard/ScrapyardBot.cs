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

namespace StarSalvager
{
    public class ScrapyardBot : MonoBehaviour, ICustomRecycle
    {
        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_RotSpeed;

        //============================================================================================================//

        public List<IAttachable> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());

        [SerializeField, ReadOnly, Space(10f), ShowInInspector]
        private List<IAttachable> _attachedBlocks;

        private List<ScrapyardPart> _parts;

        //============================================================================================================//
        public bool Rotating => _rotating;

        private bool _rotating;
        private float targetRotation;

        public bool IsRecoveryDrone => _isRecoveryDrone;
        private bool _isRecoveryDrone;

        //============================================================================================================//

        private new Rigidbody2D rigidbody
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

        public void InitBot(bool isRecoveryDrone)
        {
            _isRecoveryDrone = isRecoveryDrone;
            var startingHealth = FactoryManager.Instance.PartsRemoteData.GetRemoteData(PART_TYPE.CORE).levels[0].health;
            //Add core component
            var core = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<IAttachable>(
                new BlockData
                {
                    Type = (int)PART_TYPE.CORE,
                    Coordinate = Vector2Int.zero,
                    Level = 0,
                    Health = startingHealth
                });

            AttachNewBit(Vector2Int.zero, core);
        }

        public void InitBot(IEnumerable<IAttachable> botAttachables, bool isRecoveryDrone)
        {
            _isRecoveryDrone = isRecoveryDrone;
            foreach (var attachable in botAttachables)
            {
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
                targetRotation += toRotate;
            }
            else
            {
                targetRotation = rigidbody.rotation + toRotate;
            }

            targetRotation = MathS.ClampAngle(targetRotation);

            foreach (var attachedBlock in attachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;



        }

        #endregion //Input Solver

        //============================================================================================================//

        #region Movement

        private bool rotate;

        private void RotateBot()
        {
            var rotation = rigidbody.rotation;

            //Rotates towards the target rotation.
            float rotationAmount;
            rotationAmount = Globals.BotRotationSpeed;
            rotation = Mathf.MoveTowardsAngle(rotation, targetRotation, rotationAmount * Time.fixedDeltaTime);
            rigidbody.rotation = rotation;

            //Here we check how close to the final rotation we are.
            var remainingDegrees = Mathf.Abs(Mathf.DeltaAngle(rotation, targetRotation));

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
            rigidbody.rotation = targetRotation;
            targetRotation = 0f;


            TryRotateBits();
            rotate = false;
            _rotating = false;
        }

        private void TryRotateBits()
        {
            if (rotate)
                return;

            var check = (int)targetRotation;
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
                deg = targetRotation + 180;
            }

            var rot = Quaternion.Euler(0, 0, deg);

            foreach (var attachedBlock in attachedBlocks)
            {
                if (attachedBlock is ICustomRotate customRotate)
                {
                    customRotate.CustomRotate(rot);
                    continue;
                }

                //attachedBlock.RotateSprite(MostRecentRotate);
                attachedBlock.transform.localRotation = rot;
            }

            rotate = true;
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

            newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";
            attachedBlocks.Add(newAttachable);

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
            var attachable = attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate);
            //TODO - think of a better place to handle this selling event
            if (refund)
            {
                switch (attachable)
                {
                    case ScrapyardBit _:
                        throw new ArgumentOutOfRangeException(nameof(attachable), attachable, null);
                    case ScrapyardPart scrapyardPart:
                        PlayerPersistentData.PlayerData.AddResources(scrapyardPart.Type, scrapyardPart.level, true);
                        UpdatePartsList();
                        break;
                }
            }

            if (attachable is null)
                return; 
            
            DestroyAttachable(attachable);
            UpdatePartsList();
        }

        public void RemoveAllBits()
        {
            for (int i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (attachedBlocks[i] is ScrapyardBit)
                {
                    DetachBit(attachedBlocks[i]);
                }
            }
        }
        
        public void RemoveAllComponents()
        {
            for (int i = attachedBlocks.Count - 1; i >= 0; i--)
            {
                if (attachedBlocks[i] is Component)
                {
                    DetachBit(attachedBlocks[i]);
                }
            }
        }

        private void DetachBit(IAttachable attachable)
        {
            attachable.transform.parent = null;

            DestroyAttachable(attachable);
        }

        private void RemoveAttachable(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);
        }

        private void DestroyAttachable(IAttachable attachable, bool refundCost = true)
        {
            attachedBlocks.Remove(attachable);
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
            attachedBlocks.Remove(attachable);
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
            var toSolve = new List<IAttachable>(attachedBlocks);

            foreach (var attachableBase in toSolve)
            {
                if (!attachedBlocks.Contains(attachableBase))
                    continue;

                var hasPathToCore = attachedBlocks.HasPathToCore(attachableBase);

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

        public float powerDraw { get; private set; }

        private int maxParts { get; set; }

        public bool AtPartCapacity => _parts.Count >= maxParts + 1;
        public string PartCapacity => $"{_parts.Count - 1 }/{maxParts }";

        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        private void UpdatePartsList()
        {
            _parts = attachedBlocks.OfType<ScrapyardPart>().ToList();

            UpdatePartData();
        }

        /// <summary>
        /// Called to update the bot about relevant data to function.
        /// </summary>
        private void UpdatePartData()
        {
            PlayerPersistentData.PlayerData.ClearLiquidCapacity(_isRecoveryDrone);
            magnetCount = 0;
            maxParts = 0;
            powerDraw = 0f;
            
            var capacities = new Dictionary<BIT_TYPE, int>
            {
                {BIT_TYPE.RED, 0},
                {BIT_TYPE.BLUE, 0},
                {BIT_TYPE.YELLOW, 0},
                {BIT_TYPE.GREEN, 0},
                {BIT_TYPE.GREY, 0},
            };

            foreach (var part in _parts)
            {
                int value;

                
                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(part.Type).levels[part.level];

                powerDraw += partData.powerDraw;
                
                switch (part.Type)
                { 
                    case PART_TYPE.CORE:
                        
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.RED] += value;
                            capacities[BIT_TYPE.GREEN] += value;
                            capacities[BIT_TYPE.GREY] += value;
                            capacities[BIT_TYPE.YELLOW] += value;
                        }
                        
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
                        {
                            magnetCount += value;
                        }

                        if (partData.TryGetValue(DataTest.TEST_KEYS.PartCapacity, out int intValue))
                        {
                            maxParts = intValue;
                        }
                        break;
                    case PART_TYPE.MAGNET: 
                    
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Magnet, out value))
                        {
                            magnetCount += value;
                        }
                        break;
                    //Determine if we need to setup the shield elements for the bot
                    //FIXME I'll need a way of disposing of the shield visual object
                    case PART_TYPE.SHIELD:
                        break;
                    case PART_TYPE.STORE:
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.RED] += value;
                            capacities[BIT_TYPE.GREEN] += value;
                            capacities[BIT_TYPE.GREY] += value;
                        }
                        break;
                    case PART_TYPE.STORERED:
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.RED] += value;
                        }
                        break;
                    case PART_TYPE.STOREGREEN:
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.GREEN] += value;
                        }
                        break;
                    case PART_TYPE.STOREGREY:
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.GREY] += value;
                        }
                        break;
                    case PART_TYPE.STOREYELLOW:
                        if (partData.TryGetValue(DataTest.TEST_KEYS.Capacity, out value))
                        {
                            capacities[BIT_TYPE.YELLOW] += value;
                        }
                        break;
                }
            }

            //Force only updating once I know all capacities
            PlayerPersistentData.PlayerData.SetCapacities(capacities, _isRecoveryDrone);
        }

        #endregion //Parts

        //============================================================================================================//

        #region Custom Recycle

        public void CustomRecycle(params object[] args)
        {
            foreach (var attachable in attachedBlocks)
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

            attachedBlocks.Clear();
        }

        #endregion //Custom Recycle
    }
}