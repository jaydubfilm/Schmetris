using System;
using System.Collections.Generic;
using System.Linq;
using StarSalvager.AI;
using StarSalvager.Prototype;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class AttachableListExtensions
    {
        //============================================================================================================//


        public static IAttachable GetAttachableAtCoordinates<T>(this IEnumerable<T> attachedBlocks,
            Vector2Int coordinate) where T : IAttachable
        {
            return attachedBlocks.FirstOrDefault(a => a.Coordinate == coordinate);
        }

        //============================================================================================================//


        #region Path to Core Checks

        /// <summary>
        /// Returns whether or not this AttachableBase has a clear path to the core.
        /// </summary>
        /// <param name="attachedBlocks"></param>
        /// <param name="checking"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        public static bool HasPathToCore(this IEnumerable<IAttachable> attachedBlocks, IAttachable checking,
            List<Vector2Int> toIgnore = null)
        {
            var travelled = new List<Vector2Int>();
            //Debug.LogError("STARTED TO CHECK HERE");
            return PathAlgorithm(attachedBlocks, checking, toIgnore, ref travelled);
        }

        private static bool PathAlgorithm(IEnumerable<IAttachable> attachedBlocks, IAttachable current,
            ICollection<Vector2Int> toIgnore, ref List<Vector2Int> travelled)
        {
            //If we're on (0, 0) we've reached the core, so go back up through 
            if (current.Coordinate == Vector2Int.zero)
                return true;

            //Get list of attachables around the current attachable
            var attachablesAround = attachedBlocks.GetAttachablesAround(current);

            for (var i = 0; i < attachablesAround.Count; i++)
            {
                //If there's no attachable, keep going
                if (attachablesAround[i] == null)
                    continue;

                // If ignore list contains this Coordinate, keep going
                if (toIgnore != null && toIgnore.Contains(attachablesAround[i].Coordinate))
                {
                    //Debug.LogError($"toIgnore contains {attachablesAround[i].Coordinate}");
                    attachablesAround[i] = null;
                    continue;
                }

                if (attachablesAround[i].CountAsConnectedToCore == false)
                {
                    attachablesAround[i] = null;
                    continue;
                }

                // If we've not already been at this Coordinate, keep going
                if (!travelled.Contains(attachablesAround[i].Coordinate))
                    continue;

                //Debug.LogError($"travelled already contains {around[i].Coordinate}");
                attachablesAround[i] = null;
            }

            //Check to see if the list is completely null
            if (attachablesAround.All(ab => ab == null))
            {
                //Debug.LogError($"FAILED. Nothing around {current}", current);
                return false;
            }

            //If everything checks out, lets say we've been here
            travelled.Add(current.Coordinate);

            //Get a list of all non-null Attachables ordered by the shortest distance to the core
            var closestAttachables = attachablesAround.Where(ab => ab != null)
                .OrderBy(ab => Vector2Int.Distance(Vector2Int.zero, ab.Coordinate));


            var result = false;
            //Go through all of the attachables (Closest to Furthest) until we run out
            foreach (var attachableBase in closestAttachables)
            {
                result = PathAlgorithm(attachedBlocks, attachableBase, toIgnore, ref travelled);

                //Debug.LogError($"{result} when checking {current.gameObject.name} to {attachableBase.gameObject.name}");

                //If something reached the core, just stop looping and let the system know
                if (result)
                    break;
            }

            //Debug.LogError($"Failed Totally at {current.Coordinate}", current);
            return result;
        }


        /// <summary>
        /// Returns whether or not this AttachableBase has a clear path to the core.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="checking"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        public static bool HasPathToCore(this IEnumerable<IAttachable> attachedBlocks, Vector2Int checking,
            List<Vector2Int> toIgnore = null)
        {
            var travelled = new List<Vector2Int>();
            //Debug.LogError("STARTED TO CHECK HERE");
            return PathAlgorithm(attachedBlocks, checking, toIgnore, ref travelled);
        }

        private static bool PathAlgorithm(IEnumerable<IAttachable> attachedBlocks, Vector2Int current,
            ICollection<Vector2Int> toIgnore, ref List<Vector2Int> travelled)
        {
            //If we're on (0, 0) we've reached the core, so go back up through 
            if (current == Vector2Int.zero)
                return true;

            //Get list of attachables around the current attachable
            var attachablesAround = attachedBlocks.GetAttachablesAround(current);

            for (var i = 0; i < attachablesAround.Count; i++)
            {
                //If there's no attachable, keep going
                if (attachablesAround[i] == null)
                    continue;

                // If ignore list contains this Coordinate, keep going
                if (toIgnore != null && toIgnore.Contains(attachablesAround[i].Coordinate))
                {
                    //Debug.LogError($"toIgnore contains {attachablesAround[i].Coordinate}");
                    attachablesAround[i] = null;
                    continue;
                }

                if (attachablesAround[i].CountAsConnectedToCore == false)
                {
                    attachablesAround[i] = null;
                    continue;
                }

                // If we've not already been at this Coordinate, keep going
                if (!travelled.Contains(attachablesAround[i].Coordinate))
                    continue;

                //Debug.LogError($"travelled already contains {around[i].Coordinate}");
                attachablesAround[i] = null;
            }

            //Check to see if the list is completely null
            if (attachablesAround.All(ab => ab == null))
            {
                //Debug.LogError($"FAILED. Nothing around {current}", current);
                return false;
            }

            //If everything checks out, lets say we've been here
            travelled.Add(current);

            //Get a list of all non-null Attachables ordered by the shortest distance to the core
            var closestAttachables = attachablesAround.Where(ab => ab != null)
                .OrderBy(ab => Vector2Int.Distance(Vector2Int.zero, ab.Coordinate));


            var result = false;
            //Go through all of the attachables (Closest to Furthest) until we run out
            foreach (var attachableBase in closestAttachables)
            {
                result = PathAlgorithm(attachedBlocks, attachableBase, toIgnore, ref travelled);

                //Debug.LogError($"{result} when checking {current.gameObject.name} to {attachableBase.gameObject.name}");

                //If something reached the core, just stop looping and let the system know
                if (result)
                    break;
            }

            //Debug.LogError($"Failed Totally at {current.Coordinate}", current);
            return result;
        }

        #endregion //Path to Core Checks

        //============================================================================================================//

        public static List<BlockData> GetBlockDatas<T>(this IEnumerable<T> attachables) where T : IAttachable
        {
            return attachables.OfType<ISaveable>().GetBlockDatas();
        }

        public static List<BlockData> GetBlockDatas(this IEnumerable<ISaveable> saveables)
        {
            return saveables.Select(x => x.ToBlockData()).ToList();
        }

        public static bool FindUnoccupiedCoordinate<T>(this List<T> attachedBlocks, DIRECTION direction,
            ref Vector2Int coordinate) where T : IAttachable
        {
            var check = coordinate;
            var exists = attachedBlocks
                .Any(b => b.Coordinate == check);

            if (!exists)
                return false;

            coordinate += direction.ToVector2Int();

            return attachedBlocks.FindUnoccupiedCoordinate(direction, ref coordinate);
        }

        public static bool FindUnoccupiedCoordinate(this List<OrphanMoveData> orphanMoveDatas, DIRECTION direction,
            OrphanMoveData omd, ref Vector2Int coordinate)
        {
            var check = coordinate;

            var exists = orphanMoveDatas
                .Any(b => b.intendedCoordinates == check && b != omd);

            if (!exists)
                return false;

            coordinate += direction.ToVector2Int();

            return orphanMoveDatas.FindUnoccupiedCoordinate(direction, omd, ref coordinate);
        }

        //============================================================================================================//

        /*public static void SolveCoordinateOverlap(this List<IAttachable> blocks, DIRECTION fromDirection, ref Vector2Int coordinate)
        {
            switch (fromDirection)
            {
                case DIRECTION.LEFT:
                    blocks.CoordinateOccupied(DIRECTION.RIGHT, ref coordinate);
                    break;
                case DIRECTION.UP:
                    blocks.CoordinateOccupied(DIRECTION.DOWN, ref coordinate);
                    break;
                case DIRECTION.RIGHT:
                    blocks.CoordinateOccupied(DIRECTION.LEFT, ref coordinate);
                    break;
                case DIRECTION.DOWN:
                    blocks.CoordinateOccupied(DIRECTION.UP, ref coordinate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(fromDirection), fromDirection, null);
            }
        }
        
        public static void SolveCoordinateOverlap(this List<OrphanMoveData> orphanMoveData, DIRECTION moveDirection, OrphanMoveData omd, ref Vector2Int coordinate)
        {
            switch (moveDirection)
            {
                case DIRECTION.LEFT:
                    orphanMoveData.CoordinateOccupied(DIRECTION.RIGHT, omd, ref coordinate);
                    break;
                case DIRECTION.UP:
                    orphanMoveData.CoordinateOccupied(DIRECTION.DOWN, omd, ref coordinate);
                    break;
                case DIRECTION.RIGHT:
                    orphanMoveData.CoordinateOccupied(DIRECTION.LEFT, omd, ref coordinate);
                    break;
                case DIRECTION.DOWN:
                    orphanMoveData.CoordinateOccupied(DIRECTION.UP, omd, ref coordinate);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moveDirection), moveDirection, null);
            }
        }*/

        //============================================================================================================//

        /// <summary>
        /// Finds the closest attachable based on the average center of the collection, and returns its coordinate
        /// </summary>
        /// <param name="blocks"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Vector2Int GetCollectionCenterCoordinate<T>(this IEnumerable<T> blocks) where T: IAttachable
        {
            return blocks.GetCollectionCenterAttachable().Coordinate;
        }
        /// <summary>
        /// Finds the closest attachable based on the average center of the collection, and returns its world position
        /// </summary>
        /// <param name="blocks"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Vector2 GetCollectionCenterCoordinateWorldPosition<T>(this IEnumerable<T> blocks) where T: IAttachable
        {
            return blocks.GetCollectionCenterAttachable().transform.position;
        }

        public static Vector2 GetCollectionCenterPosition<T>(this IEnumerable<T> blocks) where T : IAttachable
        {
            var attachables = blocks.ToList();
            var averagePosition = attachables
                .Aggregate(Vector3.zero, (current, block) => current + block.transform.position) / attachables.Count;

            return averagePosition;
        }

        /// <summary>
        /// Finds the closest attachable based on the average center of the collection
        /// </summary>
        /// <param name="blocks"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetCollectionCenterAttachable<T>(this IEnumerable<T> blocks) where T: IAttachable
        {
            var blockList = blocks.ToList();
            var averagePosition = blockList.GetCollectionCenterPosition();

            averagePosition /= blockList.Count;

            var closest = blockList.GetClosestAttachable(averagePosition);

            return closest;
        }

        public static T GetClosestAttachable<T>(this List<T> blocks, Vector2 checkPosition, bool ignoreDestroyed = false) where T : IAttachable
        {
            if (blocks.Count == 1)
                return blocks[0];

            IAttachable selected = null;

            var smallestDist = 999f;

            foreach (var attached in blocks)
            {
                if(ignoreDestroyed && attached is Part part && part.Destroyed)
                    continue;
                //attached.SetColor(Color.white);

                var dist = Vector2.Distance(attached.transform.position, checkPosition);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return (T) selected;
        }

        public static T GetClosestAttachable<T>(this List<T> blocks, IAttachable attachable) where T : IAttachable
        {
            if (blocks.Count == 1)
                return blocks[0];

            var checkPosition = attachable.Coordinate;

            IAttachable selected = null;

            var smallestDist = 999f;

            foreach (var attached in blocks)
            {
                if (attached.transform == attachable.transform)
                    continue;

                //attached.SetColor(Color.white);

                var dist = Vector2.Distance(attached.transform.position, checkPosition);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return (T) selected;
        }

        public static T GetFurthestAttachable<T>(this IEnumerable<T> blocks, Vector2Int coordinate)
            where T : IAttachable
        {
            IAttachable selected = null;

            var largestDist = -999f;

            foreach (var attached in blocks)
            {
                if (attached.Coordinate == coordinate)
                    continue;

                //attached.SetColor(Color.white);

                var dist = Vector2.Distance(attached.transform.position, coordinate);
                if (dist < largestDist)
                    continue;

                largestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return (T) selected;
        }

        public static IEnumerable<T> Find<T>(this IEnumerable<T> blocks, IEnumerable<Vector2Int> coordinates)
            where T : IAttachable
        {
            return coordinates
                .Select(coordinate => blocks
                    .FirstOrDefault(x => x.Coordinate == coordinate))
                .Where(found => found != null)
                .ToArray();
        }

        //============================================================================================================//
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="attachables"></param>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<IAttachable> GetAttachablesAround(this IEnumerable<IAttachable> attachables,
            IAttachable from, bool includeCorners = false)
        {
            var enumerable = attachables as IAttachable[] ?? attachables.ToArray();

            var outList = new List<IAttachable>
            {
                enumerable.GetAttachableNextTo(from, DIRECTION.LEFT),
                enumerable.GetAttachableNextTo(from, DIRECTION.UP),
                enumerable.GetAttachableNextTo(from, DIRECTION.RIGHT),
                enumerable.GetAttachableNextTo(from, DIRECTION.DOWN)
            };

            if (includeCorners)
            {
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, 1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, 1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, -1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, -1)));
            }

            return outList;
        }

        public static List<IAttachable> GetAttachablesAround<T>(this IEnumerable<T> attachables,
            IAttachable from, bool includeCorners = false) where T : IAttachable
        {
            var enumerable = attachables as T[] ?? attachables.ToArray();

            var outList = new List<IAttachable>
            {
                enumerable.GetAttachableNextTo(from, DIRECTION.LEFT),
                enumerable.GetAttachableNextTo(from, DIRECTION.UP),
                enumerable.GetAttachableNextTo(from, DIRECTION.RIGHT),
                enumerable.GetAttachableNextTo(from, DIRECTION.DOWN)
            };

            if (includeCorners)
            {
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, 1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, 1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, -1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, -1)));
            }

            return outList;
        }

        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="attachables"></param>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<IAttachable> GetAttachablesAround(this IEnumerable<IAttachable> attachables,
            Vector2Int from, bool includeCorners = false)
        {
            var enumerable = attachables as IAttachable[] ?? attachables.ToArray();

            var outList = new List<IAttachable>
            {
                enumerable.GetAttachableNextTo(from, DIRECTION.LEFT),
                enumerable.GetAttachableNextTo(from, DIRECTION.UP),
                enumerable.GetAttachableNextTo(from, DIRECTION.RIGHT),
                enumerable.GetAttachableNextTo(from, DIRECTION.DOWN)
            };

            if (includeCorners)
            {
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, 1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, 1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, -1)));
                outList.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, -1)));
            }

            return outList;
        }

        //FIXME I should be able check this without the expensive use of the distance function
        public static List<T> GetAttachablesAroundInRadius<T>(this IEnumerable<IAttachable> attachables,
            IAttachable from, int radius) where T : IAttachable
        {
            return attachables.GetAttachablesAroundInRadius<T>(from.Coordinate, radius);
        }

        public static List<T> GetAttachablesAroundInRadius<T>(this IEnumerable<IAttachable> attachables,
            Vector2Int center, int radius) where T : IAttachable
        {
            bool InRadius(Vector2Int origin, Vector2Int compare, int rad)
            {
                var offset = compare - origin;
                offset.x = Mathf.Abs(offset.x);
                offset.y = Mathf.Abs(offset.y);

                return offset.x <= rad && offset.y <= rad;
            }

            var enumerable = attachables as IAttachable[] ?? attachables.ToArray();

            return enumerable
                .Where(a => a.gameObject.activeInHierarchy)
                .OfType<T>()
                .Where(a => a.CountAsConnectedToCore)
                .Where(a => InRadius(center, a.Coordinate, radius))
                .ToList();
        }

        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Vector2Int> GetCoordinatesAround(this IEnumerable<IAttachable> attachables, IAttachable from,
            bool includeCorners = false)
        {
            var enumerable = attachables as IAttachable[] ?? attachables.ToArray();

            var check = new List<IAttachable>
            {
                enumerable.GetAttachableNextTo(from, DIRECTION.LEFT),
                enumerable.GetAttachableNextTo(from, DIRECTION.UP),
                enumerable.GetAttachableNextTo(from, DIRECTION.RIGHT),
                enumerable.GetAttachableNextTo(from, DIRECTION.DOWN)
            };

            if (includeCorners)
            {
                check.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, 1)));
                check.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, 1)));
                check.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(1, -1)));
                check.Add(enumerable.GetAttachableNextTo(from, new Vector2Int(-1, -1)));
            }

            return check
                .Where(ab => ab != null)
                .Select(ab => ab.Coordinate)
                .ToList();

        }

        //============================================================================================================//

        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachables,
            Vector2Int coordinate, DIRECTION direction)
        {
            var from = attachables.FirstOrDefault(x => x.Coordinate == coordinate);
            return attachables.GetAttachableInDirection(from, direction.ToVector2Int());
        }
        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachables,
            IAttachable from, DIRECTION direction)
        {
            return attachables.GetAttachableInDirection(from, direction.ToVector2Int());
        }

        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachables,
            IAttachable from, Vector2 direction)
        {
            return attachables.GetAttachableInDirection(from, direction.ToDirection());
        }

        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachables, 
            IAttachable from,
            Vector2Int direction)
        {
            var coordinate = from.Coordinate;
            var attachable = from;

            var attachablesArray = attachables.ToArray();

            while (true)
            {
                coordinate += direction;

                var temp = attachablesArray.FirstOrDefault(a => a.Coordinate == coordinate);

                if (temp == null)
                    break;

                attachable = temp;
            }

            //return attachableBases.FirstOrDefault(a => a.Coordinate == coord);

            return attachable;
        }

        //============================================================================================================//


        /// <summary>
        /// Returns an AttachableBase in the specified direction from the target Attachable
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttachableNextTo<T>(this IEnumerable<T> attachables, IAttachable from, DIRECTION direction)
            where T : IAttachable
        {
            return attachables.GetAttachableNextTo(from, direction.ToVector2Int());
        }

        public static T GetAttachableNextTo<T>(this IEnumerable<T> attachables, IAttachable from, Vector2Int direction)
            where T : IAttachable
        {
            var coord = from.Coordinate + direction;

            return attachables.FirstOrDefault(a => a.Coordinate == coord);
        }

        /// <summary>
        /// Returns an AttachableBase in the specified direction from the target Attachable
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAttachable GetAttachableNextTo(this IEnumerable<IAttachable> attachables, Vector2Int from,
            DIRECTION direction)
        {
            return attachables.GetAttachableNextTo(from, direction.ToVector2Int());
        }

        public static IAttachable GetAttachableNextTo(this IEnumerable<IAttachable> attachables, Vector2Int from,
            Vector2Int direction)
        {
            var coord = from + direction;

            return attachables.FirstOrDefault(a => a.Coordinate == coord);
        }
        //============================================================================================================//

        public static void GetAllAttachedBits(this List<IAttachable> attachables, IAttachable current,
            IAttachable[] toIgnore, ref List<IAttachable> outAttachables)
        {
            var attachablesAround = attachables.GetAttachablesAround(current);

            outAttachables.Add(current);

            foreach (var attachable in attachablesAround)
            {
                if (attachable == null)
                    continue;

                //if (!attachable.CanShift)
                //    continue;

                if (toIgnore != null && toIgnore.Contains(attachable))
                    continue;

                if (outAttachables.Contains(attachable))
                    continue;

                attachables.GetAllAttachedBits(attachable, toIgnore, ref outAttachables);
            }

        }

        public static void GetAllConnectedDetachables<T>(this List<T> attachables,
            ICanDetach current,
            ICanDetach[] toIgnore,
            ref List<T> outDetachables) where T : IAttachable
        {
            var detachablesAround = attachables.GetAttachablesAround(current.iAttachable).OfType<T>().OfType<ICanDetach>();

            outDetachables.Add((T)current);

            foreach (var canDetach in detachablesAround)
            {

                if (toIgnore != null && toIgnore.Contains(canDetach))
                    continue;

                if (outDetachables.Contains((T)canDetach))
                    continue;

                attachables.GetAllConnectedDetachables(canDetach, toIgnore, ref outDetachables);
            }
        }
        
        public static void GetAllConnectedDetachables(this List<IAttachable> attachables,
            ICanDetach current,
            ICanDetach[] toIgnore,
            ref List<ICanDetach> outDetachables)
        {
            var detachablesAround = attachables.GetAttachablesAround(current.iAttachable).OfType<ICanDetach>();

            outDetachables.Add(current);

            foreach (var canDetach in detachablesAround)
            {
                if (toIgnore != null && toIgnore.Contains(canDetach))
                    continue;

                if (outDetachables.Contains(canDetach))
                    continue;

                attachables.GetAllConnectedDetachables(canDetach, toIgnore, ref outDetachables);
            }
        }


        //============================================================================================================//


        /// <summary>
        /// Algorithm function that fills the BitList with every Bit in the specified direction that matches the level
        /// and type.
        /// </summary>
        /// <param name="canCombos"></param>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="coordinate"></param>
        /// <param name="direction"></param>
        /// <param name="iCanCombos"></param>
        /// <returns></returns>
        public static bool ComboCountAlgorithm<T>(this IEnumerable<ICanCombo> canCombos, T type, int level,
            Vector2Int coordinate, Vector2Int direction,
            ref List<ICanCombo> iCanCombos) where T : Enum
        {
            var nextCoords = coordinate + direction;

            //Try and get the attachableBase Bit at the new Coordinate

            IEnumerable<ICanCombo> combos = canCombos as ICanCombo[] ?? canCombos.ToArray();
            if (!(combos
                .FirstOrDefault(a => a.Coordinate == nextCoords) is ICanCombo<T> nextBit))
                return false;

            //We only care about bits that share the same type
            if (!nextBit.Type.Equals(type))
                return false;

            //We only care about bits that share the same level
            if (nextBit.level != level)
                return false;

            //Add the bit to our combo check list
            iCanCombos.Add(nextBit);

            //Keep checking in this direction
            return combos.ComboCountAlgorithm(type, level, nextCoords, direction, ref iCanCombos);
        }

        //============================================================================================================//

        /// <summary>
        /// Checks to see if the list contains the list of IAttachables/ISaveables.
        /// This assumes that the shape is connected, without any free floating pieces.
        /// </summary>
        /// <param name="attachables"></param>
        /// <param name="comparison"></param>
        /// <param name="upgrading"></param>
        /// <typeparam name="T">Constrain search to this ISaveable type</typeparam>
        /// <returns>Does the list contain the passed shape</returns>
        public static bool Contains<T>(this List<IAttachable> attachables, IEnumerable<T> comparison, out List<Vector2Int> upgrading)
            where T : IAttachable, ISaveable
        {
            return attachables.Contains<T>(comparison.OfType<ISaveable>().GetBlockDatas(), out upgrading);
        }

        /// <summary>
        /// Checks to see if the list contains the list of BlockData
        /// This assumes that the shape is connected, without any free floating pieces.
        /// </summary>
        /// <param name="attachables"></param>
        /// <param name="comparison"></param>
        /// <param name="upgrading"></param>
        /// <typeparam name="T">Constrain search to this ISaveable type</typeparam>
        /// <returns>Does the list contain the passed shape</returns>
        public static bool Contains<T>(this List<IAttachable> attachables, IReadOnlyList<BlockData> comparison, out List<Vector2Int> upgrading)
            where T : IAttachable, ISaveable
        {
            //--------------------------------------------------------------------------------------------------------//

            List<BlockData> ResetLevels(IReadOnlyList<BlockData> data)
            {
                var newList = new List<BlockData>(data);
                for (var i = 0; i < data.Count; i++)
                {
                    var temp = data[i];
                    temp.Level = 0;
                    newList[i] = temp;
                }

                return newList;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            upgrading = null;

            //Don't bother if the lists are empty
            if (comparison.Count == 0 || attachables.Count == 0)
                return false;

            var original = attachables.OfType<T>().GetBlockDatas();
            var count = original?.Count;

            //Check to see if our filtered list will match our expected count
            if (!count.HasValue || count.Value < comparison.Count)
                return false;

            //--------------------------------------------------------------------------------------------------------//

            var _original = ResetLevels(original);
            var _compare = ResetLevels(comparison);

            var startingPoints = _original.Where(x => x.Equals(_compare[0])).ToList();

            foreach (var startingPoint in startingPoints)
            {
                var temp1 = new List<Vector2Int>();
                upgrading = new List<Vector2Int>();
                
                var check = TraversalContains(startingPoint.Coordinate - _compare[0].Coordinate, _compare[0], _compare[0].Coordinate, DIRECTION.UP, _original,
                    _compare, ref temp1, ref upgrading);

                if (check)
                    return true;
            }

            return false;

            //--------------------------------------------------------------------------------------------------------//
        }

        private static bool TraversalContains(
            Vector2Int startingCoordinate, 
            BlockData toCompare,
            Vector2Int currentCoordinate,
            DIRECTION currentDirection,
            IReadOnlyCollection<BlockData> originalList,
            IReadOnlyCollection<BlockData> compareList,
            ref List<Vector2Int> traversedCompared,
            ref List<Vector2Int> upgrading)
        {
            //Ensure we haven't already been here, and that we're not storing doubles
            if (traversedCompared.Contains(currentCoordinate))
            {
                //TODO Try a different direction
                return false;
            }

            var found = originalList
                .FirstOrDefault(x => x.Coordinate == startingCoordinate + currentCoordinate && x.Type == toCompare.Type);

            //If no one was at that location, that shapes don't match
            if (string.IsNullOrEmpty(found.ClassType))
                return false;

            traversedCompared.Add(currentCoordinate);
            upgrading.Add(found.Coordinate);

            //If we've checked everything, we're good to return
            if (traversedCompared.Count == compareList.Count)
                return true;

            Vector2Int nextCoordinate;
            DIRECTION nextDirection = currentDirection;
            while (true)
            {
                nextCoordinate = currentCoordinate + nextDirection.ToVector2Int();

                var nextCheck = compareList.FirstOrDefault(x => x.Coordinate == nextCoordinate);

                //There was no block found in this direction
                if (traversedCompared.Contains(nextCoordinate) || string.IsNullOrEmpty(nextCheck.ClassType))
                {
                    nextDirection = ((int) nextDirection + 1).ClampIntToDirection();

                    if (nextDirection == currentDirection) return false;

                    continue;
                }

                var result = TraversalContains(
                    startingCoordinate,
                    nextCheck,
                    nextCoordinate,
                    nextDirection,
                    originalList,
                    compareList,
                    ref traversedCompared,
                    ref upgrading);

                if (result) return true;

                //Rotate direction after finishing this path
                nextDirection = ((int)nextDirection + 1).ClampIntToDirection();

                if (nextDirection == currentDirection) return false;
            }
        }

        //============================================================================================================//



    }
}