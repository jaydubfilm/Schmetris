using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using UnityEngine.InputSystem;
using Input = StarSalvager.Utilities.Inputs.Input;

namespace StarSalvager
{
    public class Bot : AttachableBase, IInput
    {
        public class OrphanMoveData
        {
            public AttachableBase attachableBase;
            public DIRECTION moveDirection;
            public float distance;
            public Vector2Int intendedCoordinates;
        }

        //============================================================================================================//
        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_BitSize = 1.28f;

        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_Speed;

        [SerializeField, BoxGroup("PROTOTYPE")]
        public float TEST_RotSpeed;

        //============================================================================================================//

        public List<AttachableBase> attachedBlocks => _attachedBlocks ?? (_attachedBlocks = new List<AttachableBase>());

        [SerializeField, ReadOnly, Space(10f)] private List<AttachableBase> _attachedBlocks;

        //============================================================================================================//

        public bool Moving => _moving;
        private bool _moving;

        public DIRECTION MoveDirection => _moveDirection;

        private DIRECTION _moveDirection;
        //public bool HasValidInput => _currentInput != 0f;

        private Vector2 targetPosition;
        private float _currentInput;

        public float DelayedAutoStartTime = 0.2f;
        private float _dasTimer;


        public bool Rotating => _rotating;

        private bool _rotating;
        private float targetRotation;

        private CompositeCollider2D CompositeCollider2D;
        private new Rigidbody2D rigidbody;

        //============================================================================================================//

        #region Unity Functions

        // Start is called before the first frame update
        private void Start()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            CompositeCollider2D = GetComponent<CompositeCollider2D>();
            useCollision = false;

            //Mark as Core coordinate
            Coordinate = Vector2Int.zero;
            attachedBlocks.Add(this);

            InitInput();
        }

        // Update is called once per frame
        private void Update()
        {


            if (UnityEngine.Input.GetKeyDown(KeyCode.Equals))
                TEST_Speed += 100;
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Minus))
                TEST_Speed -= 100;
        }

        private void FixedUpdate()
        {
            if (Moving)
                MoveBot();

            if (Rotating)
                RotateBot();
        }

        //private void OnGUI()
        //{
        //    GUI.Box(new Rect(10,10,100,50), $"Speed: {TEST_Speed}" );
        //}

        private void OnDestroy()
        {
            DeInitInput();
        }

        #endregion //Unity Functions

        //============================================================================================================//

        #region Input Solver

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
                if (attachedBlock is Bot)
                    continue;

                attachedBlock.RotateCoordinate(rotation);
            }

            _rotating = true;

        }

        public void Move(float direction)
        {
            if (direction < 0)
                Move(DIRECTION.LEFT);
            else if (direction > 0)
                Move(DIRECTION.RIGHT);
        }

        public void Move(DIRECTION direction)
        {
            Vector2 toMove;
            switch (direction)
            {
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    toMove = direction.ToVector2Int();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            _moveDirection = direction;

            if (Moving)
            {
                targetPosition += toMove * TEST_BitSize;
            }
            else
            {
                targetPosition = (Vector2) transform.position + toMove * TEST_BitSize;
                _dasTimer = 0f;
            }

            _moving = true;
        }

        #endregion //Input Solver

        //============================================================================================================//

        //TODO Might want to use Rigidbody motion instead of Transform. Investigate.

        #region Movement

        private void MoveBot()
        {

            var position = rigidbody.position;

            //TODO See if this will be enough for the current setup, or if we will need something more robust.
            position = Vector2.MoveTowards(position, targetPosition, TEST_Speed * Time.fixedDeltaTime);

            //Using MovePosition() for the kinematic object since I still want it to interpolate nicely there (In the physics) 
            rigidbody.MovePosition(position);

            var remainingDistance = Vector2.Distance(position, targetPosition);
            if (remainingDistance > 0.2f)
                return;



            if (_currentInput != 0)
            {
                if (_dasTimer < DelayedAutoStartTime)
                {
                    _dasTimer += Time.deltaTime;
                    return;
                }

                Move(_currentInput);
                return;
            }

            _moving = false;
            rigidbody.position = targetPosition;
            targetPosition = Vector2.zero;
            _moveDirection = DIRECTION.NULL;
            _dasTimer = 0f;
        }

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

        #region Check For Legal Bit Attach

        //FIXME Might want to have it so that the checks don't consider ups & downs at all
        public bool TryAddNewAttachable(AttachableBase attachable, DIRECTION connectionDirection, Vector2 point)
        {
            if (Rotating)
                return false;

            if (attachable is Bit bit)
            {
                bool legalDirection;
                var direction = DIRECTION.NULL;


                //TODO Need to get the coordinate of the collision
                var bitCoordinate = GetRelativeCoordinate(bit.transform.position);

                //----------------------------------------------------------------------------------------------------//

                var closestAttachable = GetClosestAttachable(point);
                legalDirection = CheckLegalCollision(bitCoordinate, closestAttachable.Coordinate, out direction);

                //----------------------------------------------------------------------------------------------------//

                if (!legalDirection)
                {
                    //Make sure that the attachable isn't overlapping the bot before we say its impossible to 
                    if (!CompositeCollider2D.OverlapPoint(attachable.transform.position))
                        return false;
                }

                //TODO Need to check if its legal to attach (Within threshold of connection)
                switch (bit.Type)
                {
                    case BIT_TYPE.BLACK:
                        //TODO Destroy both this and collided Bit
                        Destroy(attachable.gameObject);
                        break;
                    case BIT_TYPE.BLUE:
                    case BIT_TYPE.GREEN:
                    case BIT_TYPE.GREY:
                    case BIT_TYPE.RED:
                    case BIT_TYPE.YELLOW:

                        //TODO Add these to the block depending on its relative position
                        AttachNewBitToExisting(bit, closestAttachable, connectionDirection);

                        break;
                    case BIT_TYPE.WHITE:
                        //TODO Destroy collided Bit
                        //TODO Try and shift collided row (Depending on direction)
                        Destroy(attachable.gameObject);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            //TODO Need to add other options here (ie Enemy) 



            return true;
        }

        public AttachableBase GetClosestAttachable(Vector2Int checkCoordinate)
        {
            AttachableBase selected = null;

            var smallestDist = 999f;

            foreach (var attached in attachedBlocks)
            {
                //attached.SetColor(Color.white);

                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return selected;
        }

        public AttachableBase GetClosestAttachable(Vector2 checkPosition)
        {
            AttachableBase selected = null;

            var smallestDist = 999f;

            foreach (var attached in attachedBlocks)
            {
                //attached.SetColor(Color.white);

                var dist = Vector2.Distance(attached.transform.position, checkPosition);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return selected;
        }

        /// <summary>
        /// Returns the 2 closest objects
        /// </summary>
        /// <param name="checkCoordinate"></param>
        /// <returns></returns>
        public AttachableBase[] GetClosestAttachables(Vector2Int checkCoordinate)
        {
            AttachableBase[] selected = new AttachableBase[2];

            var smallestDist = 999f;

            foreach (var attached in attachedBlocks)
            {
                //attached.SetColor(Color.white);

                var dist = Vector2Int.Distance(attached.Coordinate, checkCoordinate);

                if (dist > smallestDist)
                    continue;



                smallestDist = dist;
                selected[1] = selected[0];
                selected[0] = attached;
            }

            //selected[0].SetColor(Color.magenta);
            //selected[1]?.SetColor(Color.cyan);



            return selected;
        }

        private Vector2Int GetRelativeCoordinate(Vector2 worldPosition)
        {
            var botPosition = (Vector2) transform.position;

            var calculated = (worldPosition - botPosition) / TEST_BitSize;
            return new Vector2Int(
                Mathf.RoundToInt(calculated.x),
                Mathf.RoundToInt(calculated.y));
        }

        //FIXME Need to check if the spot that is being checked is already occupied
        private bool CheckLegalCollision(Vector2Int lhs, Vector2Int rhs, out DIRECTION direction)
        {
            direction = (lhs - rhs).ToDirection();

            //Debug.Log($"Checking Direction: {direction}");

            switch (direction)
            {
                case DIRECTION.NULL:
                    return false;
                case DIRECTION.LEFT:
                case DIRECTION.RIGHT:
                    return _moveDirection == direction;
                case DIRECTION.UP:
                    return _moveDirection == DIRECTION.NULL;
                case DIRECTION.DOWN:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            //return direction != DIRECTION.NULL;
        }

        #endregion //Check For Legal Bit Attach

        //============================================================================================================//

        #region Attach Bits

        public void AttachNewBit(Vector2Int coordinate, AttachableBase newAttachable)
        {
            newAttachable.Coordinate = coordinate;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * TEST_BitSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            CompositeCollider2D.GenerateGeometry();
        }

        public void AttachNewBitToExisting(AttachableBase newAttachable, AttachableBase existingAttachable,
            DIRECTION direction)
        {
            var coordinate = existingAttachable.Coordinate + direction.ToVector2Int();

            //Checks for attempts to add attachable to occupied location
            if (attachedBlocks.Any(a => a.Coordinate == coordinate))
            {
                Debug.LogError($"Prevented attaching {newAttachable.gameObject.name} to occupied location {coordinate}",
                    newAttachable);
                PushNewBit(newAttachable, direction, existingAttachable.Coordinate);
                return;
            }

            newAttachable.Coordinate = coordinate;

            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * coordinate * TEST_BitSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            CheckForCombosAround(coordinate);

            CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewBit(AttachableBase newAttachable, DIRECTION direction)
        {
            var newCoord = direction.ToVector2Int();

            attachedBlocks.CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * TEST_BitSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            CompositeCollider2D.GenerateGeometry();
        }

        public void PushNewBit(AttachableBase newAttachable, DIRECTION direction, Vector2Int startCoord)
        {
            var newCoord = startCoord + direction.ToVector2Int();

            attachedBlocks.CoordinateOccupied(direction, ref newCoord);

            newAttachable.Coordinate = newCoord;
            newAttachable.SetAttached(true);
            newAttachable.transform.position = transform.position + (Vector3) (Vector2.one * newCoord * TEST_BitSize);
            newAttachable.transform.SetParent(transform);

            attachedBlocks.Add(newAttachable);

            CompositeCollider2D.GenerateGeometry();
        }

        #endregion //Attach Bits

        //============================================================================================================//

        #region Puzzle Checks

        private void CheckForCombosAround(Vector2Int coordinate)
        {
            CheckForCombosAround(attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate && a is Bit) as Bit);
        }

        private void CheckForCombosAround(IEnumerable<Bit> bits)
        {
            foreach (var bit in bits)
            {
                CheckForCombosAround(bit);
            }
        }
        private void CheckForCombosAround(Bit bit)
        {
            if (bit.level >= 2)
                return;

            //fills lists with the combos to be checked
            //FIXME Need to consider each direction separately
            var horizontalBits = new List<AttachableBase>();
            var verticalBits = new List<AttachableBase>();

            ComboCountAlgorithm(bit, DIRECTION.LEFT, ref horizontalBits);
            ComboCountAlgorithm(bit, DIRECTION.RIGHT, ref horizontalBits);
            horizontalBits.Add(bit);

            ComboCountAlgorithm(bit, DIRECTION.UP, ref verticalBits);
            ComboCountAlgorithm(bit, DIRECTION.DOWN, ref verticalBits);
            verticalBits.Add(bit);

            var horizontalCount = horizontalBits.Count;
            var verticalCount = verticalBits.Count;

            //Debug.Log($"Horizontal Count: {horizontalCount}\nVertical Count: {verticalCount}");

            //TODO Need to prioritize the greater of the 2

            if (horizontalCount < 3 && verticalCount < 3)
                return;

            //If either are 3 or Greater, assume it is a combo
            //TODO I don't consider complex combinations (Ls Ts or 5)
            if (horizontalCount > verticalCount)
                SimpleComboSolver(horizontalBits);
            else if (horizontalCount < verticalCount)
                SimpleComboSolver(verticalBits);
            else
            {
                //TODO Decide what to do if both are equal
                Debug.LogError($"Weird combo at {bit.gameObject.name} doesnt have a solution yet", bit);
            }
        }

        private void ComboCountAlgorithm(Bit target, DIRECTION direction, ref List<AttachableBase> bitList)
        {
            ComboCountAlgorithm(target.Type, target.level, target.Coordinate, direction.ToVector2Int(),
                ref bitList);
        }

        /// <summary>
        /// Algorithm function that fills the BitList with every Bit in the specified direction that matches the level
        /// and type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="coordinate"></param>
        /// <param name="direction"></param>
        /// <param name="bitList"></param>
        /// <returns></returns>
        private bool ComboCountAlgorithm(BIT_TYPE type, int level, Vector2Int coordinate, Vector2Int direction,
            ref List<AttachableBase> bitList)
        {
            var nextCoords = coordinate + direction;

            //Try and get the attachableBase Bit at the new Coordinate
            var nextBit = attachedBlocks
                .FirstOrDefault(a => a.Coordinate == nextCoords && a is Bit) as Bit;

            if (nextBit == null)
                return false;

            //We only care about bits that share the same type
            if (nextBit.Type != type)
                return false;

            //We only care about bits that share the same level
            if (nextBit.level != level)
                return false;

            //Add the bit to our combo check list
            bitList.Add(nextBit);

            //Keep checking in this direction
            return ComboCountAlgorithm(type, level, nextCoords, direction, ref bitList);
        }

        /// <summary>
        /// Solves movement and upgrade logic to do with simple combos of blocks.
        /// </summary>
        /// <param name="comboBits"></param>
        /// <exception cref="Exception"></exception>
        private void SimpleComboSolver(IReadOnlyCollection<AttachableBase> comboBits)
        {
            AttachableBase closestToCore = null;
            var shortest = 999f;

            //Decide who gets to upgrade
            //--------------------------------------------------------------------------------------------------------//

            foreach (var bit in comboBits)
            {
                //Need to make sure that if we choose this block, that it is connected to the core one way or another
                var hasPath = this.HasPathToCore(bit as Bit,
                    comboBits.Where(ab => ab != bit)
                        .Select(b => b.Coordinate)
                        .ToList());

                //If there's no path, we cannot use this bit
                if (!hasPath)
                    continue;


                var dist = Vector2Int.Distance(bit.Coordinate, Vector2Int.zero);
                if (!(dist < shortest))
                    continue;

                shortest = dist;
                closestToCore = bit;
            }

            //Make sure that things are working
            //--------------------------------------------------------------------------------------------------------//

            //If no block was selected, then we've had a problem
            if (closestToCore == null)
                throw new Exception("No Closest Core Found");

            //See if anyone else needs to move
            //--------------------------------------------------------------------------------------------------------//

            //Get a list of Bits that will be moving (Blocks that are not the chosen closest to core)
            var movingBits = comboBits
                .Where(ab => ab != closestToCore).ToArray();

            //Get a list of orphans that may need move when we are moving our bits
            var orphans = new List<OrphanMoveData>();
            CheckForOrphans(movingBits, closestToCore, ref orphans);

            //Move everyone who we've determined need to move
            //--------------------------------------------------------------------------------------------------------//

            //Move all of the components that need to be moved
            StartCoroutine(MoveTowardsCoroutine(
                movingBits,
                closestToCore,
                orphans.ToArray(),
                1f,
                () =>
                {
                    var bit = closestToCore as Bit;

                    //We need to update the positions and level before we move them in case we interact with bits while they're moving

                    //bit.IncreaseLevel();

                    CheckForCombosAround(bit);
                    CheckForCombosAround(orphans.Select(x => x.attachableBase as Bit));
                }));

            //--------------------------------------------------------------------------------------------------------//
        }

        /// <summary>
        /// Get any Bit/Bits that will be orphaned by the bits which will be moving
        /// </summary>
        /// <param name="movingBits"></param>
        /// <param name="bitToUpgrade"></param>
        /// <param name="orphanMoveData"></param>
        /// <returns></returns>
        private void CheckForOrphans(AttachableBase[] movingBits,
            AttachableBase bitToUpgrade, ref List<OrphanMoveData> orphanMoveData)
        {
            //List<OrphanMoveData> orphanMoveData = null;

            //Check against all the bits that will be moving
            //--------------------------------------------------------------------------------------------------------//

            foreach (var movingBit in movingBits)
            {
                //Get the basic data about the current movingBit
                //----------------------------------------------------------------------------------------------------//

                var dif = bitToUpgrade.Coordinate - movingBit.Coordinate;
                var travelDirection = dif.ToDirection();
                var travelDistance = dif.magnitude;

                //Debug.Log($"Travel Direction: {travelDirection} distance {travelDistance}");


                //Check around moving bits (Making sure to exclude the one that doesn't move)
                //----------------------------------------------------------------------------------------------------//

                //Get all the attachableBases around the specified attachable
                var bitsAround = this.GetAttachablesAround<AttachableBase>(movingBit);

                //Don't want to bother checking the block that we know will not move
                if (bitsAround.Contains(bitToUpgrade))
                    bitsAround.Remove(bitToUpgrade);

                //Double check that the neighbors are connected to the core
                //----------------------------------------------------------------------------------------------------//

                foreach (var bit in bitsAround)
                {
                    //Ignore the ones that we know are good
                    //------------------------------------------------------------------------------------------------//
                    if (bit == null)
                        continue;

                    if (bit == bitToUpgrade)
                        continue;

                    if (movingBits.Contains(bit))
                        continue;

                    //Make sure that we haven't already determined this element to be moved
                    if (orphanMoveData != null && orphanMoveData.Any(omd => omd.attachableBase == bit))
                        continue;

                    //Check that we're connected to the core
                    //------------------------------------------------------------------------------------------------//

                    var hasPathToCore = this.HasPathToCore(bit,
                        movingBits
                            .Select(b => b.Coordinate)
                            .ToList());

                    if (hasPathToCore)
                        continue;

                    //We've got an orphan, record all of the necessary data
                    //------------------------------------------------------------------------------------------------//

                    var newOrphanCoordinate =
                        bit.Coordinate + travelDirection.ToVector2Int() * (int) travelDistance;

                    var attachedToOrphan = new List<AttachableBase>();
                    this.GetAllAttachedBits(bit, movingBits, ref attachedToOrphan);

                    //Debug.LogError($"Orphan Attached Count: {attachedToOrphan.Count}");
                    //Debug.Break();

                    //Debug.Log($"{newOrphanCoordinate} = {bit.Coordinate} + {travelDirection.ToVector2Int()} * {(int) travelDistance}");

                    if (orphanMoveData == null)
                        orphanMoveData = new List<OrphanMoveData>();

                    //------------------------------------------------------------------------------------------------//

                    //Loop ensures that the orphaned blocks which intend on moving, are able to reach their destination without any issues.
                    foreach (var orphan in attachedToOrphan)
                    {
                        var relative = orphan.Coordinate - bit.Coordinate;
                        var desiredLocation = newOrphanCoordinate + relative;

                        //Check only the Bits on the Bot that wont be moving
                        var stayingBlocks = new List<AttachableBase>(attachedBlocks);
                        foreach (var attachableBase in movingBits)
                        {
                            stayingBlocks.Remove(attachableBase);
                        }

                        //Checks to see if this orphan can travel unimpeded to the destination
                        //If it cannot, set the destination to the block beside that which is blocking it.
                        //TODO Once the desired location changes, I should 
                        var hasClearPath = IsPathClear(stayingBlocks, movingBits, (int)travelDistance, orphan.Coordinate,
                            travelDirection, desiredLocation, out var clearCoordinate);

                        //If there's no clear solution, then we will try and solve the overlap here
                        if (!hasClearPath && clearCoordinate == Vector2Int.zero)
                        {
                            //Debug.LogError("Orphan has no clear path to intended Position");
                            
                            //Make sure that there's no overlap between orphans new potential positions & existing staying Bits
                            stayingBlocks.SolveCoordinateOverlap(travelDirection, ref desiredLocation);
                        }
                        else if (!hasClearPath)
                        {
                            //Debug.LogError($"Path wasn't clear. Setting designed location to {clearCoordinate} instead of {desiredLocation}");
                            desiredLocation = clearCoordinate;
                        }
                        


                        orphanMoveData.Add(new OrphanMoveData
                        {
                            attachableBase = orphan,
                            moveDirection = travelDirection,
                            distance = travelDistance,
                            intendedCoordinates = desiredLocation
                        });
                    }

                    //------------------------------------------------------------------------------------------------//

                    //Debug.LogError($"{bit.gameObject.name} Has Path: {hasPathToCore}", bit);
                    //Debug.Break();

                    //Debug.Log($"{bit.gameObject.name} Has Path: {hasPathToCore}", bit);
                }

            }

        }

        private bool IsPathClear(List<AttachableBase> stayingBlocks, IEnumerable<AttachableBase> toIgnore, int distance, Vector2Int currentCoordinate, DIRECTION moveDirection, Vector2Int targetCoordinate, out Vector2Int clearCoordinate)
        {
            //var distance = (int) orphanMoveData.distance;
            var coordinate = currentCoordinate;
            
            clearCoordinate = Vector2Int.zero;
            
            while (distance > 0)
            {
                coordinate += moveDirection.ToVector2Int();
                var occupied = stayingBlocks.Where(x => !toIgnore.Contains(x)).FirstOrDefault(x => x.Coordinate == coordinate);

                //Debug.LogError($"Occupied: {occupied == null} at {coordinate} distance {distance}");
                
                if (occupied == null)
                    clearCoordinate = coordinate;
                
                //if(occupied != null)
                //    Debug.LogError($"{occupied.gameObject.name} is at {coordinate}", occupied);

                distance--;
            }

            return targetCoordinate == clearCoordinate;
        }

    /// <summary>
        /// Coroutine used to move all of the relevant Bits (Bits to be upgraded, orphans) to their appropriate locations
        /// at the specified speed, and when finished trigger the Callback.
        /// </summary>
        /// <param name="movingBits"></param>
        /// <param name="target"></param>
        /// <param name="orphans"></param>
        /// <param name="speed"></param>
        /// <param name="OnFinishedCallback"></param>
        /// <returns></returns>
        private IEnumerator MoveTowardsCoroutine(AttachableBase[] movingBits, AttachableBase target,
            OrphanMoveData[] orphans, float speed, Action OnFinishedCallback)
        {
            //Prepare Bits to be moved
            //--------------------------------------------------------------------------------------------------------//
            
            (target as Bit)?.IncreaseLevel();
            
            foreach (var bit in movingBits)
            {
                //We need to disable the collider otherwise they can collide while moving
                //I'm also assuming that if we've confirmed the upgrade, and it cannot be cancelled
                attachedBlocks.Remove(bit);
                bit.SetColliderActive(false);
            }

            foreach (var omd in orphans)
            {
                omd.attachableBase.Coordinate = omd.intendedCoordinates;
                omd.attachableBase.SetColliderActive(false);
            }
            
            //We're going to want to regenerate the shape while things are moving
            CompositeCollider2D.GenerateGeometry();
            
            //--------------------------------------------------------------------------------------------------------//

            var t = 0f;
            var targetTransform = target.transform;

            var bitTransforms = movingBits.Select(ab => ab.transform).ToArray();
            
            //--------------------------------------------------------------------------------------------------------//

            //Move bits towards target
            while (t <= 1f)
            {
                //Move the main blocks related to the upgrading
                //----------------------------------------------------------------------------------------------------//
                
                foreach (var bt in bitTransforms)
                {
                    bt.localPosition =
                        Vector2.Lerp(bt.localPosition, targetTransform.localPosition, t);
                }

                //Move the orphans into their new positions
                //----------------------------------------------------------------------------------------------------//
                
                foreach (var omd in orphans)
                {
                    var bitTransform = omd.attachableBase.transform;
                    
                    var position = transform.InverseTransformPoint(
                        (Vector2) transform.position + (Vector2) omd.intendedCoordinates * TEST_BitSize);
                    
                    //Debug.Log($"Start {bitTransform.position} End {position}");

                    bitTransform.localPosition = Vector2.Lerp(bitTransform.localPosition,
                        position, t);
                }
                
                //----------------------------------------------------------------------------------------------------//

                t += Time.deltaTime * speed;

                yield return null;
            }
            
            //Wrap up things now that everyone is in place
            //--------------------------------------------------------------------------------------------------------//

            //Once all bits are moved, remove from list and dispose
            foreach (var bit in movingBits)
            {
                Destroy(bit.gameObject);
            }

            //Re-enable the colliders on our orphans
            foreach (var moveData in orphans)
            {
                
                moveData.attachableBase.SetColliderActive(true);
            }
            
            //Now that everyone is where they need to be, wrap things up
            //--------------------------------------------------------------------------------------------------------//

            CompositeCollider2D.GenerateGeometry();


            OnFinishedCallback?.Invoke();
            
            //--------------------------------------------------------------------------------------------------------//
        }


        #endregion //Puzzle Checks

        //============================================================================================================//

        #region Attachable Overrides
        
        protected override void OnCollide(GameObject _)
        {
        }

        public override BlockData ToBlockData()
        {
            throw new NotImplementedException();
        }

        public override void LoadBlockData(BlockData _)
        {
            throw new NotImplementedException();
        }
        
        #endregion //Attachable Overrides

        //============================================================================================================//

        //TODO This needs to be fleshed out further

        #region Input

        public void InitInput()
        {

            Input.Actions.Default.SideMovement.Enable();
            Input.Actions.Default.SideMovement.performed += SideMovement;

            Input.Actions.Default.Rotate.Enable();
            Input.Actions.Default.Rotate.performed += Rotate;

        }

        public void DeInitInput()
        {
            Input.Actions.Default.SideMovement.Disable();
            Input.Actions.Default.SideMovement.performed -= SideMovement;

            Input.Actions.Default.Rotate.Disable();
            Input.Actions.Default.Rotate.performed -= Rotate;
        }

        private void SideMovement(InputAction.CallbackContext ctx)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
            {
                _currentInput = 0f;
                return;
            }

            _currentInput = ctx.ReadValue<float>();

            Move(_currentInput);

        }

        private void Rotate(InputAction.CallbackContext ctx)
        {
            if (UnityEngine.Input.GetKey(KeyCode.LeftAlt))
                return;

            var rot = ctx.ReadValue<float>();

            if (rot < 0)
                Rotate(ROTATION.CCW);
            else if (rot > 0)
                Rotate(ROTATION.CW);
        }

        #endregion //Input

        //============================================================================================================//


    }
}