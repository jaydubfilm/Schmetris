using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class AttachableListExtensions
    {
        //============================================================================================================//
        
        public static bool CoordinateOccupied<T>(this List<T> attachedBlocks, DIRECTION direction, ref Vector2Int coordinate) where T: IAttachable
        {
            var check = coordinate;
            var exists = attachedBlocks
                .Any(b => b.Coordinate == check);

            if (!exists)
                return false;

            coordinate += direction.ToVector2Int();

            return attachedBlocks.CoordinateOccupied(direction, ref coordinate);
        }
        
        public static bool CoordinateOccupied(this List<OrphanMoveData> orphanMoveDatas, DIRECTION direction, OrphanMoveData omd, ref Vector2Int coordinate)
        {
            var check = coordinate;
            
            var exists = orphanMoveDatas
                .Any(b => b.intendedCoordinates == check && b != omd);

            if (!exists)
                return false;

            coordinate += direction.ToVector2Int();

            return orphanMoveDatas.CoordinateOccupied(direction, omd, ref coordinate);
        }
        
        //============================================================================================================//
        
        public static void SolveCoordinateOverlap(this List<IAttachable> blocks, DIRECTION fromDirection, ref Vector2Int coordinate)
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
        }
        
        //============================================================================================================//
        
        public static IAttachable GetClosestAttachable<T>(this List<T> blocks, Vector2 checkPosition) where T: IAttachable
        {
            if (blocks.Count == 1)
                return blocks[0];
            
            IAttachable selected = null;

            var smallestDist = 999f;

            foreach (var attached in blocks)
            {
                //attached.SetColor(Color.white);

                var dist = Vector2.Distance(attached.transform.position, checkPosition);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = attached;
            }

            //selected.SetColor(Color.magenta);

            return (T)selected;
        }
        
        public static IAttachable GetClosestAttachable<T>(this List<T> blocks, IAttachable attachable) where T: IAttachable
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

            return (T)selected;
        }
        
        //============================================================================================================//
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<IAttachable> GetAttachablesAround(this List<IAttachable> attachableBases, IAttachable from)
        {
            return new List<IAttachable>
            {
                attachableBases.GetAttachableNextTo(from, DIRECTION.LEFT),
                attachableBases.GetAttachableNextTo(from, DIRECTION.UP),
                attachableBases.GetAttachableNextTo(from, DIRECTION.RIGHT),
                attachableBases.GetAttachableNextTo(from, DIRECTION.DOWN)
            };
        }
        
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Vector2Int> GetCoordinatesAround(this List<IAttachable> attachableBases, IAttachable from)
        {
            var check = new List<IAttachable>
            {
                attachableBases.GetAttachableNextTo(from, DIRECTION.LEFT),
                attachableBases.GetAttachableNextTo(from, DIRECTION.UP),
                attachableBases.GetAttachableNextTo(from, DIRECTION.RIGHT),
                attachableBases.GetAttachableNextTo(from, DIRECTION.DOWN)
            };

            return check
                .Where(ab => ab != null)
                .Select(ab => ab.Coordinate)
                .ToList();

        }

        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachableBases,
            IAttachable from, DIRECTION direction)
        {
            return attachableBases.GetAttachableInDirection(from, direction.ToVector2Int());
        }
        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachableBases,
            IAttachable from, Vector2 direction)
        {
            return attachableBases.GetAttachableInDirection(from, direction.ToDirection());
        }
        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachableBases, IAttachable from, Vector2Int direction)
        {
            var coordinate = from.Coordinate;
            var attachable = from;

            var attachables = attachableBases.ToArray();
            
            while (true)
            {
                coordinate += direction;
                
                var temp = attachables.FirstOrDefault(a => a.Coordinate == coordinate);

                if (temp == null)
                    break;

                attachable = temp;
            }

            //return attachableBases.FirstOrDefault(a => a.Coordinate == coord);

            return attachable;
        }
        
        /// <summary>
        /// Returns an AttachableBase in the specified direction from the target Attachable
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IAttachable GetAttachableNextTo(this IEnumerable<IAttachable> attachableBases, IAttachable from, DIRECTION direction)
        {
            var coord = from.Coordinate + direction.ToVector2Int();

            return attachableBases.FirstOrDefault(a => a.Coordinate == coord);
        }
        
        public static void GetAllAttachedBits(this List<IAttachable> attachableBases, IAttachable current, IAttachable[] toIgnore, ref List<IAttachable> bits)
        {
            var bitsAround = attachableBases.GetAttachablesAround(current);

            bits.Add(current);
            
            foreach (var bit in bitsAround)
            {
                if (bit == null)
                    continue;

                if (!bit.CanShift)
                    continue;

                if (toIgnore != null && toIgnore.Contains(bit))
                    continue;
                
                if(bits.Contains(bit))
                    continue;

                attachableBases.GetAllAttachedBits(bit, toIgnore, ref bits);
            }

        }
        
        
        //============================================================================================================//
        
        
        

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
        public static bool ComboCountAlgorithm(this List<IAttachable> attachableBases, BIT_TYPE type, int level, Vector2Int coordinate, Vector2Int direction,
            ref List<Bit> bitList)
        {
            var nextCoords = coordinate + direction;

            //Try and get the attachableBase Bit at the new Coordinate
            var nextBit = attachableBases
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
            return attachableBases.ComboCountAlgorithm(type, level, nextCoords, direction, ref bitList);
        }

        
        
        //============================================================================================================//


    }
}