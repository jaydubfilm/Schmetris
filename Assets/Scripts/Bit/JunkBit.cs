using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Values;
using StarSalvager.Utilities;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StarSalvager
{
    //FIXME This can be combined with the Bit class a little
    public class JunkBit : CollidableBase, IAttachable, IHealth, ISaveable<BitData>, IObstacle, ICustomRecycle, ICanBeHit, IRotate, ICanDetach
    {
        public IAttachable iAttachable => this;
        
        [SerializeField]
        private LayerMask collisionMask;

        //IAttachable properties
        //============================================================================================================//

        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }

        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool Rotating => _rotating;
        private bool _rotating;

        public int RotateDirection => _rotateDirection;
        private int _rotateDirection = 1;

        public bool CountAsConnectedToCore => true;
        public bool CanDisconnect => true;

        [ShowInInspector, ReadOnly]
        public bool CanShift => true;

        public int AttachPriority => 0;
        public bool PendingDetach { get; set; }

        public bool CountTowardsMagnetism => true;

        //IHealth Properties
        //============================================================================================================//

        public float StartingHealth { get; private set; }
        [ShowInInspector, ReadOnly, ProgressBar(0,"StartingHealth")]
        public float CurrentHealth { get; private set; }

        //IObstacle Properties
        //============================================================================================================//
        public bool CanMove => !Attached;

        public bool IsRegistered { get; set; } = false;

        public bool IsMarkedOnGrid { get; set; } = false;

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
            collider.usedByComposite = isAttached;

            if (!isAttached) PendingDetach = false;
        }


        //IRotate Functions
        //============================================================================================================//

        public void SetRotating(bool isRotating)
        {
            _rotating = isRotating;
            
            //Only need to set the rotation value when setting rotation to true
            if(_rotating)
                _rotateDirection = Random.Range(-1, 2);
        }

        //IHealth Functions
        //============================================================================================================//

        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
            
            SetColor(Color.white);
        }

        public void ChangeHealth(float amount)
        {
            //float previousHealth = _currentHealth;

            CurrentHealth += amount;

            if (CurrentHealth <= 0)
            {
                Recycler.Recycle<JunkBit>(this);
                return;
            }

            ////TODO - temporary demo color change, remove later
            //if (previousHealth > _currentHealth)
            //{
            //    SetColor(Color.Lerp(renderer.color, Color.black, 0.2f));
            //}
        }

        //ICanBeHit Functions
        //============================================================================================================//
        
        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);
            
            return true;
        }

        //Bit Functions
        //============================================================================================================//

        //FIXME Need to fix the attach location issues
        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                this.Bounce(worldHitPoint, transform.position, bot.MostRecentRotate);
                AudioController.PlaySound(SOUND.BIT_BOUNCE);
                return;
            }

            var dir = (worldHitPoint - (Vector2)transform.position).ToVector2Int();

            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;
            
            if (dir != rayDirection && dir != Vector2Int.zero)
                return;

            if (!TryFindClosestCollision(rayDirection.ToDirection(), collisionMask, out var point))
                return;

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            bot.TryAddNewAttachable(this, inDirection, point);
        }

        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            SetAttached(false);
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            SetRotating(false);

            SetSortingLayer(LayerHelper.ACTORS);

            PendingDetach = false;
        }


        //ISaveable Functions
        //============================================================================================================//

        public BitData ToBlockData()
        {
            //throw new NotImplementedException();
            return new BitData
            {
                Type = (int)BIT_TYPE.NONE,
                //ClassType = GetType().Name,
                Coordinate = Coordinate
            };
        }

        public void LoadBlockData(IBlockData blockData)
        {
            throw new NotImplementedException();
            //if (!(blockData is JunkBitData junkBitData))
            //    throw new Exception();
            
            //Coordinate = junkBitData.Coordinate;
        }


        //IHasBounds Functions
        //====================================================================================================================//

        public Bounds GetBounds()
        {
            return new Bounds
            {
                center = transform.position,
                size = Vector2.one * Constants.gridCellSize
            };
        }

        //====================================================================================================================//

        IBlockData ISaveable.ToBlockData()
        {
            return ToBlockData();
        }
    }
}
