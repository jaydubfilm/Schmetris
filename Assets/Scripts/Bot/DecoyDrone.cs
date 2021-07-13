using System;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.AI;
using StarSalvager.Values;
using StarSalvager.Audio;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Utilities.Particles;
using UnityEngine;


namespace StarSalvager
{
    public class DecoyDrone : BotBase
    {
        //IBot Properties
        //====================================================================================================================//

        //Decoy should not be rotating
        public override bool Rotating => false;

        //DecoyDrone Properties
        //====================================================================================================================//

        private Bot _bot;

        private Vector2 _positionMoveUpwards;

        //Unity Functions
        //====================================================================================================================//

        // Update is called once per frame
        private void Update()
        {
            if (transform == null)
                return;
            
            transform.position = Vector2.Lerp(transform.position, _positionMoveUpwards, Time.deltaTime);
        }

        //DecoyDrone Functions
        //====================================================================================================================//

        public void Init(in Bot bot, in float speed)
        {
            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();

            _bot = bot;
            _positionMoveUpwards = (Vector2) transform.position + (Vector2.up * (speed * Constants.gridCellSize));
            

            var emptyPart = partFactory.CreateObject<Part>(
                new PartData
                {
                    Type = (int)PART_TYPE.EMPTY,
                    Coordinate = Vector2Int.zero
                });
            emptyPart.gameObject.name = $"{PART_TYPE.EMPTY}_{Vector2Int.zero}";
            
            AttachNewBlock(Vector2Int.zero, emptyPart);
        }

        //IHealth Functions
        //====================================================================================================================//

        public override void ChangeHealth(float amount)
        {
            CurrentHealth += amount;

            
            if (CurrentHealth > 0)
                return;
            
            _bot.DecoyDrone = null;

            var copy = new List<IAttachable>(AttachedBlocks);

            foreach (var attachable in copy)
            {
                switch (attachable)
                {
                    case Part part:
                        Recycler.Recycle<Part>(part);
                        break;
                    case EnemyAttachable enemyAttachable:
                        ForceDetach(enemyAttachable);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(attachable), attachable, null);
                }
            }
            
            Destroy(gameObject);
        }

        //====================================================================================================================//

        public override bool TryHitAt(Vector2 worldPosition, float damage)
        {
            return TryHitAt(damage, true);
        }
        public override void TryHitAt(IAttachable closestAttachable, float damage, bool withSound = true)
        {
            TryHitAt(damage, withSound);
        }
        
        public bool TryHitAt(in float damage, in bool withSound)
        {
            ChangeHealth(-damage);
            
            //Here we check to make sure to not display tiny values of damage
            var check = Mathf.Abs(damage);
            if(!(check > 0 && check < 1f))
                FloatingText.Create($"{damage}", transform.position, Color.red);

            
            if(withSound && CurrentHealth > 0) 
                AudioController.PlaySound(SOUND.ENEMY_IMPACT);

            return true;
        }

        //IBot Functions
        //====================================================================================================================//
        
        public override bool TryAddNewAttachable(IAttachable attachable, DIRECTION connectionDirection, Vector2 collisionPoint)
        {
            switch (attachable)
            {
                //FIXME This seems to be wanting to attach to the wrong direction
                case EnemyAttachable enemyAttachable:
                {
                    //----------------------------------------------------------------------------------------------------//

                    var closestAttachable = AttachedBlocks.GetClosestAttachable(collisionPoint);

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

                    //Add these to the block depending on its relative position
                    AttachAttachableToExisting(enemyAttachable, closestAttachable, connectionDirection);
                    break;
                }
            }

            return true;
        }
        public override void AttachAttachableToExisting(IAttachable newAttachable, IAttachable existingAttachable,
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
        
        public override void AttachNewBlock(Vector2Int coordinate, IAttachable newAttachable,
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

            //We want to avoid having the same element multiple times in the list
            if(!AttachedBlocks.Contains(newAttachable))
                AttachedBlocks.Add(newAttachable);
        }

        public override void ForceDetach(ICanDetach canDetach)
        {
            canDetach.transform.parent = null;

            if (LevelManager.Instance && canDetach is IObstacle obstacle)
                LevelManager.Instance.ObstacleManager.AddObstacleToListAndParentToWorldRoot(obstacle);

            if (!(canDetach is IAttachable attachable))
                return;

            AttachedBlocks.Remove(attachable);
            attachable.SetAttached(false);

            CompositeCollider2D.GenerateGeometry();
        }

        public override IAttachable GetClosestAttachable(Vector2Int checkCoordinate, float maxDistance = 999)
        {
            return AttachedBlocks[0];
        }
        

        //====================================================================================================================//
        
   }
}