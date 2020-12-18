using UnityEngine;
using StarSalvager.Factories.Data;
using System;
using Recycling;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Projectiles;
using StarSalvager.Utilities;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;

namespace StarSalvager.AI
{
    //TODO: Handle proper setting of the collision tag
    public class ProjectileAttachable : Projectile, IAttachable, ICustomRecycle
    {
        //IAttachable Properties
        //============================================================================================================//

        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }

        public bool CountAsConnectedToCore => false;
        public bool CanShift => true;
        public bool CountTowardsMagnetism => false;

        //ICanDetach Properties
        //====================================================================================================================//

        public int AttachPriority => 10000;

        public bool PendingDetach { get; set; }

        public IAttachable iAttachable => this;

        //EnemyAttachable Properties
        //============================================================================================================//

        [SerializeField]
        private LayerMask collisionMask;

        private Bot _attachedBot;
        private new IAttachable _target;
        private Vector2Int _targetCoordinate;

        //IAttachable Functions
        //============================================================================================================//

        public void SetAttached(bool isAttached)
        {
            if (!isAttached) PendingDetach = false;

            //I can't assume that it will always be attached/Detached,as we need to ensure that the move is legal before setting all the values   

            //If the bot is telling us to detach, first we need to make sure we can't take the position of our old target
            //This is determined by whether or not it has a path to the core.
            if (isAttached)
            {
                Attached = true;
                collider.usedByComposite = true;
                return;
            }

            if (TryMoveToTargetPosition())
                return;

            Attached = false;
            collider.usedByComposite = false;

            _target = null;
            _attachedBot = null;
            transform.rotation = Quaternion.identity;
        }

        private bool TryMoveToTargetPosition()
        {
            //If the enemy didn't kill the bit, we shouldn't more to its position
            if (!DidIDestroyBit())
                return false;

            if (_target == null)
                return false;

            if (!_attachedBot.CoordinateHasPathToCore(_target.Coordinate))
                return false;

            if (_attachedBot.CoordinateOccupied(_target.Coordinate))
                return false;

            if (!_attachedBot.TryAttachNewBlock(_target.Coordinate, this, false, true, false))
                return false;

            if (!TryUpdateTarget())
                return false;

            return true;
        }

        private bool TryUpdateTarget()
        {
            if (_attachedBot is null)
            {
                SetAttached(false);
                return false;
            }

            //We set the max distance here because we want to ensure we're attacking something right next to us
            _target = _attachedBot.GetClosestAttachable(Coordinate, 1f);

            if (_target == null)
            {
                SetAttached(false);
                return false;
            }

            //TEST_TARGET = target.gameObject;
            //Debug.Log($"{gameObject.name} has new target. TARGET : {TEST_TARGET.gameObject.name}", TEST_TARGET);

            RotateTowardsTarget(_target);

            return true;
        }

        private bool DidIDestroyBit()
        {
            var health = _target as IHealth;
            var recyclable = _target as IRecycled;

            if (health?.CurrentHealth > 0)
                return false;

            return _target.Attached || !recyclable.IsRecycled;
        }


        //============================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            if (GameManager.IsState(GameState.LevelEndWave))
                return;

            if (Attached)
                return;

            var bot = gameObject.GetComponent<Bot>();

            if (bot.Rotating)
            {
                //Recycler.Recycle<Bit>(this);
                return;
            }

            var dir = (worldHitPoint - (Vector2)transform.position).ToVector2Int();

            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;

            //Debug.Log($"Direction: {dir}, Ray Direction: {rayDirection}");

            if (dir != rayDirection && dir != Vector2Int.zero)
                return;

            //Long ray compensates for the players high speed
            var rayLength = Constants.gridCellSize * 3f;
            var rayStartPosition = (Vector2)transform.position + -rayDirection * (rayLength / 2f);


            //Checking ray against player layer mask
            var hit = Physics2D.Raycast(rayStartPosition, rayDirection, rayLength, collisionMask.value);

            //If nothing was hit, ray failed, thus no reason to continue
            if (hit.collider == null)
            {
                /*Debug.DrawRay(rayStartPosition, rayDirection * rayLength, Color.yellow, 1f);
                SSDebug.DrawArrowRay(rayStartPosition, rayDirection * rayLength, Color.yellow);*/
                return;
            }

            /*Debug.DrawRay(hit.point, Vector2.up, Color.red);
            Debug.DrawRay(rayStartPosition, rayDirection * rayLength, Color.green);*/

            _attachedBot = bot;

            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            var inDirection = (-rayDirection).ToDirection();
            var attached = bot.TryAddNewAttachable(this, inDirection, hit.point);

            if (!attached)
            {
                _attachedBot = null;
                return;
            }

            TryUpdateTarget();
        }

        //ICustomRotate functions
        //============================================================================================================//

        public void CustomRotate(Quaternion _)
        {
            //We don't want to rotate the Attachable enemy because they need to face specific directions to
            //indicate their attack direction
        }

        private void RotateTowardsTarget(IAttachable Target)
        {
            if (_target == null)
                return;

            var dir = (Target.Coordinate - Coordinate).ToDirection();
            var AddRotation = Vector3.zero;

            switch (dir)
            {
                case DIRECTION.LEFT:
                    AddRotation = Vector3.forward * 270f;
                    break;
                case DIRECTION.UP:
                    AddRotation = Vector3.forward * 180f;
                    break;
                case DIRECTION.RIGHT:
                    AddRotation = Vector3.forward * 90f;
                    break;
                case DIRECTION.DOWN:
                    AddRotation = Vector3.zero;
                    break;
                default:
                    dir = (-Coordinate).ToDirection();
                    break;
            }

            Debug.Log($"{gameObject.name} Rotate to Direction: {dir}", gameObject);

            transform.rotation = Quaternion.Euler(AddRotation);
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
    }
}