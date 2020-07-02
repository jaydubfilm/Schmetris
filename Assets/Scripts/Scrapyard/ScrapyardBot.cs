using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Constants;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager
{
    public class ScrapyardBot : MonoBehaviour
    {
        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_RotSpeed;

        //============================================================================================================//

        public List<IAttachable> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());

        [SerializeField, ReadOnly, Space(10f), ShowInInspector]
        private List<IAttachable> _attachedBlocks;

        private List<Part> _parts;

        //============================================================================================================//
        public bool Rotating => _rotating;

        private bool _rotating;
        private float targetRotation;

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

        private void Start()
        {
            InitBot();
        }

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
            //Add core component
            var core = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<IAttachable>(
                new BlockData
                {
                    Type = (int)PART_TYPE.CORE,
                    Coordinate = Vector2Int.zero,
                    Level = 0,
                });

            AttachNewBit(Vector2Int.zero, core);
        }

        public void InitBot(IEnumerable<IAttachable> botAttachables)
        {
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

            //If we're already rotating, we need to add the direction to the target
            if (Rotating)
            {
                targetRotation += toRotate;
            }
            else
            {
                targetRotation = rigidbody.rotation + toRotate;
            }

            foreach (var attachedBlock in attachedBlocks)
            {
                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;

        }

        #endregion //Input Solver

        //============================================================================================================//

        #region Movement

        private void RotateBot()
        {
            var rotation = rigidbody.rotation;

            //Rotates towards the target rotation.
            //rotation = Quaternion.RotateTowards(rotation, targetRotation, TEST_RotSpeed * Time.deltaTime);
            rotation = Mathf.MoveTowardsAngle(rotation, targetRotation, TEST_RotSpeed * Time.fixedDeltaTime);
            rigidbody.rotation = rotation;

            //Here we check how close to the final rotation we are.
            var remainingDegrees = Mathf.Abs(Mathf.DeltaAngle(rotation, targetRotation));

            //If we're within 1deg we will count it as complete, otherwise continue to rotate.
            if (remainingDegrees > 1f)
                return;

            _rotating = false;

            //Force set the rotation to the target, in case the bot is not exactly on target
            rigidbody.rotation = targetRotation;
            targetRotation = 0f;
        }

        #endregion //Movement

        //============================================================================================================//

        #region Attach Bits

        public void AttachNewBit(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true, bool updateColliderGeometry = true)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3)(Vector2.one * coordinate * Values.gridCellSize);
            newAttachable.transform.SetParent(transform);

            newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";
            attachedBlocks.Add(newAttachable);

            if (newAttachable is Part)
                UpdatePartsList();
        }

        #endregion //Attach Bits

        //============================================================================================================//

        #region Detach Bits

        public void RemoveAttachableAt(Vector2Int coordinate)
        {
            if (attachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                DestroyAttachable(attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate));
            }
        }

        private void DetachBit(IAttachable attachable)
        {
            attachable.transform.parent = null;

            RemoveAttachable(attachable);
        }

        private void RemoveAttachable(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);
        }

        private void DestroyAttachable(IAttachable attachable)
        {
            attachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

            switch (attachable)
            {
                case Bit _:
                    Recycler.Recycle<Bit>(attachable.gameObject);
                    break;
                case Part _:
                    Recycler.Recycle<Part>(attachable.gameObject);
                    break;
                case EnemyAttachable _:
                    Recycler.Recycle<EnemyAttachable>(attachable.gameObject);
                    break;
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

        /// <summary>
        /// Called when new Parts are added to the attachable List. Allows for a short list of parts to exist to ease call
        /// cost for updating the Part behaviour
        /// </summary>
        private void UpdatePartsList()
        {
            _parts = attachedBlocks.OfType<Part>().ToList();

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
                var partData = FactoryManager.Instance.GetFactory<PartAttachableFactory>().GetRemoteData(PART_TYPE.MAGNET);

                switch (part.Type)
                {
                    case PART_TYPE.MAGNET:
                    case PART_TYPE.CORE:
                        magnetCount += partData.data[part.level];
                        break;
                }
            }
        }

        #endregion //Parts
    }
}