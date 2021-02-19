using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.AI;
using StarSalvager.Values;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Prototype;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Particles;
using UnityEngine;


namespace StarSalvager
{
    public class DecoyDrone : CollidableBase, IBot, IAttachable, IHealth, ICanBeHit
    {
        //[NonSerialized]
        //public Bot bot;

        //IBot Properties
        //====================================================================================================================//

        public List<IAttachable> AttachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<IAttachable>());
        [SerializeField, ReadOnly, Space(10f), ShowInInspector]
        private List<IAttachable> _attachedBlocks;
        
        public Collider2D Collider => collider;
        //Decoy should not be rotating
        public bool Rotating => false;

        //IAttachable Properties
        //====================================================================================================================//
        
        public Vector2Int Coordinate { get; set; }
        public bool Attached => false;
        public bool CountAsConnectedToCore => true;
        public bool CanShift => false;
        public bool CountTowardsMagnetism => false;

        //IHealth Properties
        //====================================================================================================================//

        public float StartingHealth { get; private set; }
        public float CurrentHealth { get; private set; }

        //DecoyDrone Properties
        //====================================================================================================================//

        private Bot _bot;

        //private float _timer = 0.0f;
        //private float m_timeAlive = 5.0f;
        private Vector2 m_positionMoveUpwards;

        //Unity Functions
        //====================================================================================================================//

        // Update is called once per frame
        private void Update()
        {
            /*if (_timer >= m_timeAlive)
            {
                bot.DecoyDrone = null;
                Destroy(gameObject);
            }*/

            if (transform == null)
                return;
            
            transform.position = Vector2.Lerp(transform.position, m_positionMoveUpwards, Time.deltaTime);
            /*_timer += Time.deltaTime;*/
        }
        
        //DecoyDrone Functions
        //====================================================================================================================//

        public void Init(in Bot bot, in float speed)
        {
            _bot = bot;
            m_positionMoveUpwards = (Vector2) transform.position + (Vector2.up * (speed * Constants.gridCellSize));
        }

        //IHealth Functions
        //====================================================================================================================//
        
        public void SetupHealthValues(float startingHealth, float currentHealth)
        {
            StartingHealth = startingHealth;
            CurrentHealth = currentHealth;
        }

        public void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            FloatingText.Create($"{amount}", transform.position, amount > 0 ? Color.green : Color.red);
            
            if (CurrentHealth > 0)
                return;
            
            _bot.DecoyDrone = null;
            Destroy(gameObject);
        }

        //CollidableBase Functions
        //====================================================================================================================//

        protected override void OnCollide(GameObject gameObject, Vector2 worldHitPoint) { }

        public bool TryHitAt(Vector2 worldPosition, float damage)
        {
            ChangeHealth(-damage);
            
            var explosion = FactoryManager.Instance.GetFactory<EffectFactory>().CreateEffect(EffectFactory.EFFECT.EXPLOSION);
            explosion.transform.position = worldPosition;
            
            var particleScaling = explosion.GetComponent<ParticleSystemGroupScaling>();
            var time = particleScaling.AnimationTime;

            Destroy(explosion, time);
            
            if(CurrentHealth > 0)
                AudioController.PlaySound(SOUND.ENEMY_IMPACT);

            return true;
        }

        //IBot Functions
        //====================================================================================================================//
        
        public bool TryAddNewAttachable(IAttachable attachable, DIRECTION connectionDirection, Vector2 collisionPoint)
        {
            if (Rotating)
                return false;

            IAttachable closestAttachable = null;

            switch (attachable)
            {
                //FIXME This seems to be wanting to attach to the wrong direction
                case EnemyAttachable enemyAttachable:
                {
                    //Get the coordinate of the collision
                    //var bitCoordinate = GetRelativeCoordinate(enemyAttachable.transform.position);

                    //----------------------------------------------------------------------------------------------------//

                    closestAttachable = AttachedBlocks.GetClosestAttachable(collisionPoint);

                    if (closestAttachable is EnemyAttachable)
                    {
                        return false;
                    }

                    if (enemyAttachable is BorrowerEnemy borrowerEnemy && !(closestAttachable is Bit bit))
                    {
                        closestAttachable = borrowerEnemy.FindClosestBitOnBot();
                        if (closestAttachable == null)
                        {
                            return false;
                        }
                    }

                    //FIXME This isn't sufficient to prevent multiple parasites using the same location
                    var potentialCoordinate = closestAttachable.Coordinate + connectionDirection.ToVector2Int();
                    if (AttachedBlocks.Count(x => x.Coordinate == potentialCoordinate) > 1)
                        return false;

                    /*legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out _);

                    //----------------------------------------------------------------------------------------------------//

                    if (!legalDirection)
                    {
                        //Make sure that the attachable isn't overlapping the bot before we say its impossible to
                        if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                            return false;
                    }*/

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(enemyAttachable, closestAttachable, connectionDirection);
                    break;
                }
            }

            return true;
        }
        public void AttachAttachableToExisting(IAttachable newAttachable, IAttachable existingAttachable,
            DIRECTION direction,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool checkMagnet = true,
            bool playSound = true,
            bool updatePartList = true)
        {

            if (newAttachable is BorrowerEnemy)
            {
                direction = GetAvailableConnectionDirection(existingAttachable.Coordinate, direction);
            }

            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();

            //Checks for attempts to add attachable to occupied location
            if (AttachedBlocks.Any(a => a.Coordinate == coordinate /*&& !(a is Part part && part.Destroyed)*/))
            {
                var onAttachable = AttachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate);
                Debug.Log(
                    $"Prevented attaching {newAttachable.gameObject.name} to occupied location {coordinate}\n Occupied by {onAttachable.gameObject.name}",
                    newAttachable.gameObject);

                if (newAttachable is BorrowerEnemy)
                {
                    return;
                }

                AttachToClosestAvailableCoordinate(coordinate,
                    newAttachable,
                    direction,
                    checkForCombo,
                    updateColliderGeometry);
                return;
            }

            newAttachable.Coordinate = coordinate;

            newAttachable.SetAttached(true);
            newAttachable.transform.position =
                transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            //We want to avoid having the same element multiple times in the list
            if(!AttachedBlocks.Contains(newAttachable))
                AttachedBlocks.Add(newAttachable);

        }
        
        public void AttachToClosestAvailableCoordinate(Vector2Int coordinate, IAttachable newAttachable, DIRECTION desiredDirection, bool checkForCombo,
            bool updateColliderGeometry)
        {

            var directions = new[]
            {
                //Cardinal Directions
                Vector2Int.left,
                Vector2Int.up,
                Vector2Int.right,
                Vector2Int.down,

                //Corners
                new Vector2Int(-1,-1),
                new Vector2Int(-1,1),
                new Vector2Int(1,-1),
                new Vector2Int(1,1),
            };

            var avoid = desiredDirection.Reflected().ToVector2Int();

            var dist = 1;
            while (true)
            {
                for (var i = 0; i < directions.Length; i++)
                {

                    var check = coordinate + (directions[i] * dist);
                    if (AttachedBlocks.Any(x => x.Coordinate == check))
                        continue;

                    //We need to make sure that the piece wont be floating
                    if (!AttachedBlocks.HasPathToCore(check))
                        continue;
                    //Debug.Log($"Found available location for {newAttachable.gameObject.name}\n{coordinate} + ({directions[i]} * {dist}) = {check}");
                    AttachNewBlock(check, newAttachable, checkForCombo, updateColliderGeometry);
                    return;
                }

                if (dist++ > 10)
                    break;

            }
        }
        
        public void AttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
            bool checkForCombo = true,
            bool updateColliderGeometry = true,
            bool checkMagnet = true,
            bool playSound = true,
            bool updatePartList = true)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * Constants.gridCellSize);
            newAttachable.transform.SetParent(transform);

            //newAttachable.gameObject.name = $"Block {attachedBlocks.Count}";

            //We want to avoid having the same element multiple times in the list
            if(!AttachedBlocks.Contains(newAttachable))
                AttachedBlocks.Add(newAttachable);
        }
        
        private DIRECTION GetAvailableConnectionDirection(Vector2Int existingAttachableCoordinate, DIRECTION direction)
        {
            var coordinate = existingAttachableCoordinate + direction.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return direction;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.UP.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.UP;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.RIGHT.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.RIGHT;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.LEFT.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.LEFT;
            }

            coordinate = existingAttachableCoordinate + DIRECTION.DOWN.ToVector2Int();
            //Checks for attempts to add attachable to occupied location
            if (!AttachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                return DIRECTION.DOWN;
            }

            return direction;
        }

        public void ForceDetach(ICanDetach attachable)
        {
            throw new System.NotImplementedException();
        }

        public bool CoordinateHasPathToCore(Vector2Int coordinate)
        {
            throw new System.NotImplementedException();
        }

        public bool CoordinateOccupied(Vector2Int coordinate)
        {
            throw new System.NotImplementedException();
        }

        public bool TryAttachNewBlock(Vector2Int coordinate, IAttachable newAttachable, bool checkForCombo = true,
            bool updateColliderGeometry = true, bool updatePartList = true)
        {
            throw new System.NotImplementedException();
        }

        public IAttachable GetClosestAttachable(Vector2Int checkCoordinate, float maxDistance = 999)
        {
            return this;
        }

        public void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true)
        {
            TryHitAt(transform.position, damage);
        }

        //IAttachable Functions
        //====================================================================================================================//
        

        public Bounds GetBounds()
        {
            throw new System.NotImplementedException();
        }


        public void SetAttached(bool isAttached)
        {
            throw new System.NotImplementedException();
        }

        //====================================================================================================================//
        
   }
}