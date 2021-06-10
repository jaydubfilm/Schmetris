using System;
using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Audio;
using StarSalvager.Audio.Enemies;
using StarSalvager.Factories;
using StarSalvager.Utilities.Analytics;
using StarSalvager.Utilities.Animations;
using StarSalvager.Utilities.Enemies;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Particles;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.AI
{
    public abstract class EnemyAttachable : Enemy, IAttachable, ICustomRotate, IWasBumped, ICanDetach 
    {
        private static readonly int DEFAULT = Animator.StringToHash("Default");
        private static readonly int ATTACK  = Animator.StringToHash("Attack");

        //IAttachable Properties
        //============================================================================================================//

        public bool IsAttachable => true;

        

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
        
        
        private EnemyDecoy _enemyDecoy;

        protected BotBase AttachedBot;
        protected IAttachable Target;
        private Vector2Int _targetCoordinate;
        
        

        //Unity Functions
        //============================================================================================================//

        /*protected void Update()
        {
            if (HintManager.CanShowHint(HINT.PARASITE))
            {
                if (CameraController.IsPointInCameraRect(transform.position, Constants.VISIBLE_GAME_AREA))
                {
                    HintManager.TryShowHint(HINT.PARASITE, 1f, this);
                }
            }

            ProcessFireLogic();
            
            if(GameTimer.IsPaused || !GameManager.IsState(GameState.LevelActive) || GameManager.IsState(GameState.LevelActiveEndSequence) || Disabled)
                return;
            
            /*if (FreezeTime > 0)
            {
                FreezeTime -= Time.deltaTime;
                return;
            }
            
            if (!Attached)
                return;

            if (GameManager.IsState(GameState.LevelEndWave))
            {
                _target = null;
                _attachedBot.ForceDetach(this);
                return;
            }

            EnsureTargetValidity();

            m_fireTimer += Time.deltaTime;

            if (m_fireTimer < 1 / m_enemyData.RateOfFire)
                return;

            m_fireTimer -= 1 / m_enemyData.RateOfFire;
            
            FireAttack();#1#
            
        }*/

        //IAttachable Functions
        //============================================================================================================//

        public virtual void SetAttached(bool isAttached)
        {
            if (!isAttached) 
                PendingDetach = false;
            
            //I can't assume that it will always be attached/Detached,as we need to ensure that the move is legal before setting all the values   
            
            //If the bot is telling us to detach, first we need to make sure we can't take the position of our old target
            //This is determined by whether or not it has a path to the core.
            if (isAttached)
            {
                Attached = true;
                collider.usedByComposite = true;
                StateAnimator.ChangeState(ATTACK);

                if (_enemyDecoy == null)
                    _enemyDecoy = FactoryManager.Instance.GetFactory<EnemyFactory>().CreateEnemyDecoy();
                
                _enemyDecoy.Setup(this, AttachedBot.Collider);

                return;
            }
            
            if(TryMoveToTargetPosition())
                return;
            
            if (_enemyDecoy != null)
                _enemyDecoy.Disable();
            
            Attached = false;
            collider.usedByComposite = false;
            StateAnimator.ChangeState(DEFAULT);

            Target = null;
            AttachedBot = null;
            transform.rotation = Quaternion.identity;
            
            LevelManager.Instance.EnemyManager.ReParentEnemy(this);
        }

        //Enemy Overrides
        //====================================================================================================================//
        
        protected override Vector2 GetMovementDirection(Vector2 playerLocation)
        {
            Vector2 direction;
            switch (currentState)
            {
                case STATE.FLEE:
                    direction = (Vector2) transform.position - playerLocation;
                    break;
                default:
                    direction = playerLocation - (Vector2)transform.position;
                    break;
            }

            return direction.normalized;
        }
        
        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint)
        {
            if (Disabled)
                return;

            if (currentState == STATE.IDLE)
                return;

            if (GameManager.IsState(GameState.LevelEndWave))
                return;
            
            if(Attached)
                return;

            var botBase = gameObject.GetComponent<BotBase>();

            if (botBase.Rotating)
            {
                return;
            }

            //Checks to see if the player is moving in the correct direction to bother checking, and if so,
            //return the direction to shoot the ray
            if (!TryGetRayDirectionFromBot(Globals.MovingDirection, out var rayDirection))
                return;

            var dir = rayDirection.ToDirection();

            TryFindClosestCollision(dir, collisionMask, out var point);

            AttachedBot = botBase;
            
            //Here we flip the direction of the ray so that we can tell the Bot where this piece might be added to
            //var inDirection = (-rayDirection).ToDirection();
            var attached = botBase.TryAddNewAttachable(this, dir.Reflected(), point);

            if (!attached)
            {
                AttachedBot = null;
                return;
            }

            TryUpdateTarget();
        }

        protected override bool TryGetRayDirectionFromBot(DIRECTION direction, out Vector2 rayDirection)
        {
            rayDirection = Vector2.zero;
            //Returns the opposite direction based on the current players move direction.
            switch (direction)
            {
                case DIRECTION.NULL:
                    var norm = MostRecentMovementDirection.normalized;

                    if (Mathf.Abs(norm.x) > Mathf.Abs(norm.y))
                    {
                        rayDirection = norm.x < 0f ? Vector2.left : Vector2.right;
                    }
                    else if(norm.y == 0f)
                    {
                        rayDirection = Vector2.down;
                    }
                    else
                    {
                        rayDirection = norm.y < 0f ? Vector2.down : Vector2.up;
                    }

                    return true;
                case DIRECTION.LEFT:
                    rayDirection = Vector2.right;
                    return true;
                case DIRECTION.RIGHT:
                    rayDirection = Vector2.left;
                    return true;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        //Attachable Enemy Movement when Attacking
        //============================================================================================================//

        public void CheckUpdateTarget()
        {
            TryUpdateTarget();
        }

        protected void EnsureTargetValidity()
        {
            //If our target has been destroyed (Killed/Recycled) we want to move to its position
            //This would occur if this wasn't attempted to be detached,
            //meaning it was sitting in a legal position that didn't require it to be detached
            //FIXME This may be an issue with those attached to shapes that get detached?
            //if (_target is IRecycled recyclable && recyclable.IsRecycled)
            //{
//
            //}

            switch (Target)
            {
                case IRecycled recyclable when recyclable.IsRecycled:
                    break;
                case Part _:
                    return;
                    /*Target = null;
                    AttachedBot.ForceDetach(this);
                    return;*/
            }

            //Here we're making sure that the target is still part of what we're attacking
            if (Target.transform.parent != transform.parent)
            {
                Target = null;
                AttachedBot.ForceDetach(this);
                return;
            }
            //TODO Need to account for bits that move due to combo solve
            

            //if (targetDirection != (this.Coordinate - target.Coordinate))
            //{
            //    RotateTowardsTarget(target);
            //    return;
            //}

            /*if (attackFromCoordinate != this.Coordinate)
            {
                UpdateTarget();
                return;
            }*/

            /*//We also want to make sure that we aren't currently targeting something that we shouldn't be
            if (target.Attached == false)
            {
                target = null;
                attachedBot.ForceDetach(this);
                return;
            }*/

            if (!(Target is EnemyAttachable)) 
                return;

            TryUpdateTarget();
        }

        private bool TryMoveToTargetPosition()
        {
            if (Target == null)
                return false;

            if (AttachedBot == null)
                return false;

            //If the enemy didn't kill the bit, we shouldn't more to its position
            if (!DidIDestroyBit())
                return false;

            if (!AttachedBot.CoordinateHasPathToCore(Target.Coordinate))
                return false;

            if (AttachedBot.CoordinateOccupied(Target.Coordinate))
                return false;

            if (!AttachedBot.TryAttachNewBlock(Target.Coordinate, this, false, true))
                return false;

            if (!TryUpdateTarget())
                return false;
            
            return true;
        }

        private bool TryUpdateTarget()
        {
            if (AttachedBot is null)
            {
                SetAttached(false);
                return false;
            }
            
            //We set the max distance here because we want to ensure we're attacking something right next to us
            Target = AttachedBot.GetClosestAttachable(Coordinate, 1f);

            if (Target == null)
            {
                SetAttached(false);
                return false;
            }

            //TEST_TARGET = target.gameObject;
            //Debug.Log($"{gameObject.name} has new target. TARGET : {TEST_TARGET.gameObject.name}", TEST_TARGET);

            RotateTowardsTarget(Target);

            return true;
        }
        
        private bool DidIDestroyBit()
        {
            var health = Target as IHealth;
            var recyclable = Target as IRecycled; 
            
            if (health?.CurrentHealth > 0)
                return false;
            
            return Target.Attached  || !recyclable.IsRecycled;
        }

        //IHealth functions
        //============================================================================================================//

        public override void ChangeHealth(float amount)
        {
            CurrentHealth += amount;
            
            if (amount < 0)
            {
                FloatingText.Create($"{Mathf.Abs(amount)}", transform.position, Color.red);
            }

            if (CurrentHealth > 0)
                return;

            if (AttachedBot != null)
            {
                AttachedBot.ForceDetach(this);
                AttachedBot = null;
            }
            
            transform.parent = LevelManager.Instance.ObstacleManager.WorldElementsRoot;
            KilledEnemy();
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
            if (this.Target == null)
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

        //IWasBumped Functions
        //====================================================================================================================//
        
        public virtual void OnBumped()
        {
            if (!Attached || Disabled)
                return;
            
            //IF I wanted to change targets after moving, meaning I stay attached, I would call this
            //CheckUpdateTarget();
            
            //TODO Need to disable enemy after it was bumped
            Target = null;
            AttachedBot.ForceDetach(this);

            StateAnimator.ChangeState(StateAnimator.DEFAULT);
            StateAnimator.Pause();
            Disabled = true;
        }

        //ICustomRecycle functions
        //============================================================================================================//

        public override void CustomRecycle(params object[] args)
        {
            base.CustomRecycle(args);
            
            _enemyDecoy = null;
            AttachedBot = null;
            Target = null;
            PendingDetach = false;
            SetAttached(false);
        }

        //============================================================================================================//

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
