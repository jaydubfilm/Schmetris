﻿using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <param name="bot"></param>
        /// <param name="checking"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        public static bool HasPathToCore(this IEnumerable<IAttachable> attachedBlocks, IAttachable checking, List<Vector2Int> toIgnore = null)
        {
            var travelled = new List<Vector2Int>();
            //Debug.LogError("STARTED TO CHECK HERE");
            return PathAlgorithm(attachedBlocks, checking, toIgnore, ref travelled);
        }
        
        private static bool PathAlgorithm(IEnumerable<IAttachable> attachedBlocks, IAttachable current, ICollection<Vector2Int> toIgnore, ref List<Vector2Int> travelled)
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
        
        
        #endregion //Path to Core Checks
        
        //============================================================================================================//

        
        public static bool FindUnoccupiedCoordinate<T>(this List<T> attachedBlocks, DIRECTION direction, ref Vector2Int coordinate) where T: IAttachable
        {
            var check = coordinate;
            var exists = attachedBlocks
                .Any(b => b.Coordinate == check);

            if (!exists)
                return false;

            coordinate += direction.ToVector2Int();

            return attachedBlocks.FindUnoccupiedCoordinate(direction, ref coordinate);
        }
        
        public static bool FindUnoccupiedCoordinate(this List<OrphanMoveData> orphanMoveDatas, DIRECTION direction, OrphanMoveData omd, ref Vector2Int coordinate)
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
        /// <param name="attachables"></param>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<IAttachable> GetAttachablesAround(this IEnumerable<IAttachable> attachables, IAttachable from)
        {
            var enumerable = attachables as IAttachable[] ?? attachables.ToArray();
            
            return new List<IAttachable>
            {
                enumerable.GetAttachableNextTo(from, DIRECTION.LEFT),
                enumerable.GetAttachableNextTo(from, DIRECTION.UP),
                enumerable.GetAttachableNextTo(from, DIRECTION.RIGHT),
                enumerable.GetAttachableNextTo(from, DIRECTION.DOWN)
            };
        }
        
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<Vector2Int> GetCoordinatesAround(this IEnumerable<IAttachable> attachables, IAttachable from)
        {
            var enumerable = attachables as IAttachable[] ?? attachables.ToArray();
            
            var check = new List<IAttachable>
            {
                enumerable.GetAttachableNextTo(from, DIRECTION.LEFT),
                enumerable.GetAttachableNextTo(from, DIRECTION.UP),
                enumerable.GetAttachableNextTo(from, DIRECTION.RIGHT),
                enumerable.GetAttachableNextTo(from, DIRECTION.DOWN)
            };

            return check
                .Where(ab => ab != null)
                .Select(ab => ab.Coordinate)
                .ToList();

        }
        
        //============================================================================================================//

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
        public static IAttachable GetAttachableInDirection(this IEnumerable<IAttachable> attachables, IAttachable from, Vector2Int direction)
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
        public static IAttachable GetAttachableNextTo(this IEnumerable<IAttachable> attachables, IAttachable from, DIRECTION direction)
        {
            var coord = from.Coordinate + direction.ToVector2Int();

            return attachables.FirstOrDefault(a => a.Coordinate == coord);
        }
        
        //============================================================================================================//

        public static void GetAllAttachedBits(this List<IAttachable> attachables, IAttachable current, IAttachable[] toIgnore, ref List<IAttachable> bits)
        {
            var bitsAround = attachables.GetAttachablesAround(current);

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

                attachables.GetAllAttachedBits(bit, toIgnore, ref bits);
            }

        }
        
        
        //============================================================================================================//


        /// <summary>
        /// Algorithm function that fills the BitList with every Bit in the specified direction that matches the level
        /// and type.
        /// </summary>
        /// <param name="attachables"></param>
        /// <param name="type"></param>
        /// <param name="level"></param>
        /// <param name="coordinate"></param>
        /// <param name="direction"></param>
        /// <param name="bitList"></param>
        /// <returns></returns>
        public static bool ComboCountAlgorithm(this List<IAttachable> attachables, BIT_TYPE type, int level, Vector2Int coordinate, Vector2Int direction,
            ref List<Bit> bitList)
        {
            var nextCoords = coordinate + direction;

            //Try and get the attachableBase Bit at the new Coordinate
            var nextBit = attachables
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
            return attachables.ComboCountAlgorithm(type, level, nextCoords, direction, ref bitList);
        }

        
        
        //============================================================================================================//


    }
}