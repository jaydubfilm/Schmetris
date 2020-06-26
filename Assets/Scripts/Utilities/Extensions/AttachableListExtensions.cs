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
        
        public static bool CoordinateOccupied(this List<Bot.OrphanMoveData> orphanMoveDatas, DIRECTION direction, Bot.OrphanMoveData omd, ref Vector2Int coordinate)
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
        
        public static void SolveCoordinateOverlap(this List<Bot.OrphanMoveData> orphanMoveData, DIRECTION moveDirection, Bot.OrphanMoveData omd, ref Vector2Int coordinate)
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

            return selected;
        }
        
        //============================================================================================================//
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetAttachablesAround<T>(this List<T> attachableBases, IAttachable from) where T: IAttachable
        {
            return new List<T>
            {
                attachableBases.GetAttachableInDirectionOf<T>(from, DIRECTION.LEFT),
                attachableBases.GetAttachableInDirectionOf<T>(from, DIRECTION.UP),
                attachableBases.GetAttachableInDirectionOf<T>(from, DIRECTION.RIGHT),
                attachableBases.GetAttachableInDirectionOf<T>(from, DIRECTION.DOWN)
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
                attachableBases.GetAttachableInDirectionOf<IAttachable>(from, DIRECTION.LEFT),
                attachableBases.GetAttachableInDirectionOf<IAttachable>(from, DIRECTION.UP),
                attachableBases.GetAttachableInDirectionOf<IAttachable>(from, DIRECTION.RIGHT),
                attachableBases.GetAttachableInDirectionOf<IAttachable>(from, DIRECTION.DOWN)
            };

            return check
                .Where(ab => ab != null)
                .Select(ab => ab.Coordinate)
                .ToList();

        }
        
        /// <summary>
        /// Returns an AttachableBase in the specified direction from the target Attachable
        /// </summary>
        /// <param name="from"></param>
        /// <param name="direction"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetAttachableInDirectionOf<T>(this IEnumerable<T> attachableBases, IAttachable from, DIRECTION direction) where T: IAttachable
        {
            var coord = from.Coordinate + direction.ToVector2Int();

            return attachableBases.FirstOrDefault(a => a.Coordinate == coord);
        }
        
        public static void GetAllAttachedBits<T>(this List<IAttachable> attachableBases, IAttachable current, IAttachable[] toIgnore, ref List<T> bits) where T: IAttachable
        {
            var bitsAround = attachableBases.GetAttachablesAround(current);

            bits.Add((T)current);
            
            foreach (var bit in bitsAround)
            {
                if (bit == null)
                    continue;

                if (toIgnore != null && toIgnore.Contains(bit))
                    continue;
                
                if(bits.Contains((T)bit))
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
            ref List<IAttachable> bitList)
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