using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class BlockDataExtensions
    {

        //============================================================================================================//

        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        public static bool CheckHasDisconnects(this IEnumerable<BlockData> blockDatas)
        {
            var toSolve = new List<BlockData>(blockDatas);

            foreach (var blockData in toSolve)
            {
                if (!blockDatas.Contains(blockData))
                    continue;

                var hasPathToCore = blockDatas.HasPathToCore(blockData);

                if (hasPathToCore)
                    continue;

                return true;
            }

            return false;
        }

        //============================================================================================================//
        /// <summary>
        /// Returns a list of all AttachableBase types around the from block
        /// </summary>
        /// <param name="blockDatas"></param>
        /// <param name="from"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<BlockData> GetAttachablesAround(this IEnumerable<BlockData> blockDatas,
            BlockData from, bool includeCorners = false)
        {
            var enumerable = blockDatas as BlockData[] ?? blockDatas.ToArray();

            var outList = new List<BlockData>()
            {
                enumerable.GetBlockDataNextTo(from.Coordinate, DIRECTION.LEFT),
                enumerable.GetBlockDataNextTo(from.Coordinate, DIRECTION.UP),
                enumerable.GetBlockDataNextTo(from.Coordinate, DIRECTION.RIGHT),
                enumerable.GetBlockDataNextTo(from.Coordinate, DIRECTION.DOWN)
            };

            if (includeCorners)
            {
                outList.Add(enumerable.GetBlockDataNextTo(from.Coordinate, new Vector2Int(-1, 1)));
                outList.Add(enumerable.GetBlockDataNextTo(from.Coordinate, new Vector2Int(1, 1)));
                outList.Add(enumerable.GetBlockDataNextTo(from.Coordinate, new Vector2Int(1, -1)));
                outList.Add(enumerable.GetBlockDataNextTo(from.Coordinate, new Vector2Int(-1, -1)));
            }

            return outList;
        }

        //============================================================================================================//

        public static BlockData GetBlockDataNextTo(this IEnumerable<BlockData> blockDatas, Vector2Int from,
            DIRECTION direction)
        {
            return blockDatas.GetBlockDataNextTo(from, direction.ToVector2Int());
        }

        public static BlockData GetBlockDataNextTo(this IEnumerable<BlockData> blockDatas, Vector2Int from,
            Vector2Int direction)
        {
            var coord = from + direction;

            return blockDatas.FirstOrDefault(a => a.Coordinate == coord);
        }

        //============================================================================================================//

        #region Path to Core Checks

        /// <summary>
        /// Returns whether or not this AttachableBase has a clear path to the core.
        /// </summary>
        /// <param name="attachedBlockDatas"></param>
        /// <param name="checking"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        public static bool HasPathToCore(this IEnumerable<BlockData> attachedBlockDatas, BlockData checking,
            List<Vector2Int> toIgnore = null)
        {
            var travelled = new List<Vector2Int>();
            //Debug.LogError("STARTED TO CHECK HERE");
            return PathAlgorithm(attachedBlockDatas, checking, toIgnore, ref travelled);
        }

        private static bool PathAlgorithm(IEnumerable<BlockData> attachedBlockDatas, BlockData current,
            ICollection<Vector2Int> toIgnore, ref List<Vector2Int> travelled)
        {
            //If we're on (0, 0) we've reached the core, so go back up through 
            if (current.Coordinate == Vector2Int.zero)
                return true;

            //Get list of attachables around the current attachable
            var blockDatasAround = attachedBlockDatas.GetAttachablesAround(current);

            for (var i = 0; i < blockDatasAround.Count; i++)
            {
                //If there's no attachable, keep going
                if (blockDatasAround[i].ClassType == null)
                    continue;

                // If ignore list contains this Coordinate, keep going
                if (toIgnore != null && toIgnore.Contains(blockDatasAround[i].Coordinate))
                {
                    //Debug.LogError($"toIgnore contains {attachablesAround[i].Coordinate}");
                    blockDatasAround[i] = new BlockData();
                    continue;
                }

                if (blockDatasAround[i].ClassType == "EnemyAttachable")
                {
                    blockDatasAround[i] = new BlockData();
                    continue;
                }

                // If we've not already been at this Coordinate, keep going
                if (!travelled.Contains(blockDatasAround[i].Coordinate))
                    continue;

                //Debug.LogError($"travelled already contains {around[i].Coordinate}");
                blockDatasAround[i] = new BlockData();
            }

            //Check to see if the list is completely null
            if (blockDatasAround.All(ab => ab.ClassType == null))
            {
                //Debug.LogError($"FAILED. Nothing around {current}", current);
                return false;
            }

            //If everything checks out, lets say we've been here
            travelled.Add(current.Coordinate);

            //Get a list of all non-null Attachables ordered by the shortest distance to the core
            var closestBlockDatas = blockDatasAround.Where(ab => ab.ClassType != null)
                .OrderBy(ab => Vector2Int.Distance(Vector2Int.zero, ab.Coordinate));


            var result = false;
            //Go through all of the attachables (Closest to Furthest) until we run out
            foreach (var attachableBase in closestBlockDatas)
            {
                result = PathAlgorithm(attachedBlockDatas, attachableBase, toIgnore, ref travelled);

                //Debug.LogError($"{result} when checking {current.gameObject.name} to {attachableBase.gameObject.name}");

                //If something reached the core, just stop looping and let the system know
                if (result)
                    break;
            }

            //Debug.LogError($"Failed Totally at {current.Coordinate}", current);
            return result;
        }


        /*
        /// <summary>
        /// Returns whether or not this AttachableBase has a clear path to the core.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="checking"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        public static bool HasPathToCore(this IEnumerable<BlockData> attachedBlocks, Vector2Int checking,
            List<Vector2Int> toIgnore = null)
        {
            var travelled = new List<Vector2Int>();
            //Debug.LogError("STARTED TO CHECK HERE");
            return PathAlgorithm(attachedBlocks, checking, toIgnore, ref travelled);
        }

        private static bool PathAlgorithm(IEnumerable<BlockData> attachedBlocks, Vector2Int current,
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
        */

        #endregion //Path to Core Checks

        public static List<IAttachable> ImportBlockDatas(this List<BlockData> blockDatas, bool inScrapyardForm)
        {
            List<IAttachable> attachables = new List<IAttachable>();

            foreach (BlockData blockData in blockDatas)
            {
                switch (blockData.ClassType)
                {
                    case nameof(Bit):
                    case nameof(ScrapyardBit):
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<ScrapyardBit>(blockData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(blockData));
                        }
                        break;
                    case nameof(Component):
                        //case nameof(ScrapyardBit):
                        var component = FactoryManager.Instance.GetFactory<ComponentAttachableFactory>()
                            .CreateObject<Component>(blockData);
                            attachables.Add(component);
                            /*if (inScrapyardForm)
                            {
                                throw new NotImplementedException();
                            }
                            else
                            {
                                
                            }*/
                        break;
                    case nameof(Part):
                    case nameof(ScrapyardPart):
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(blockData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<Part>(blockData));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(blockData.ClassType), blockData.ClassType, null);
                }
            }

            return attachables;
        }
    }
}