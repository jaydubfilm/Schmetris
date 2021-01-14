using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Recycling;
using StarSalvager.AI;
using StarSalvager.Factories;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.Utilities.Extensions
{
    public static class IBlockDataListExtensions
    {
        public static void CheckForOrphansFromProcessing(
            this IEnumerable<IBlockData> blockDatas,
            BitData targetBit,
            ref List<OrphanMoveBlockData> orphanMoveData)
        {
            var blocks = blockDatas.ToArray();
            var bits = blocks.OfType<BitData>().ToArray();
            
            //Check around moving bits (Making sure to exclude the one that doesn't move)
            //----------------------------------------------------------------------------------------------------//

            //Get all the attachableBases around the specified attachable
            var bitsAround = bits.GetBlocksAround(targetBit);

            //Don't want to bother checking the block that we know will not move
            if (bitsAround.Contains(targetBit))
                bitsAround.Remove(targetBit);
            
            var coordinatesToIgnore = new List<Vector2Int>
            {
                targetBit.Coordinate
            };
            var blocksToIgnore = new List<BitData>
            {
                targetBit
            };

            //Double check that the neighbors are connected to the core
            //----------------------------------------------------------------------------------------------------//

            foreach (var bitData in bitsAround)
            {
                //Ignore the ones that we know are good
                //------------------------------------------------------------------------------------------------//

                if (bitData.Coordinate == Vector2Int.zero)
                    continue;
                
                if (bitData.Equals(targetBit))
                    continue;
                

                //Make sure that we haven't already determined this element to be moved
                if (orphanMoveData != null && orphanMoveData.Any(omd => omd.blockData.Equals(bitData)))
                    continue;

                var omdGroup = new List<OrphanMoveBlockData>();

                //Check that we're connected to the core
                //------------------------------------------------------------------------------------------------//

                var hasPathToCore = blocks.HasPathToCore(bitData, coordinatesToIgnore);

                if (hasPathToCore)
                    continue;
                
                var attachedToOrphan = new List<BitData>();
                bits.GetAllConnectedBlocks(
                    bitData,
                    blocksToIgnore,
                    ref attachedToOrphan);
                
                //Get the basic data about the current movingBit
                //----------------------------------------------------------------------------------------------------//
                var closest = attachedToOrphan.ToList().GetClosestBlockData(targetBit);

                var dif = targetBit.Coordinate - closest.Coordinate;
                var travelDirection = dif.ToDirection();
                var travelDistance = dif.magnitude;


                if (travelDirection == DIRECTION.NULL)
                    throw new Exception();

                var travelDistInt = (int) travelDistance;

                //We've got an orphan, record all of the necessary data
                //------------------------------------------------------------------------------------------------//

                if (orphanMoveData == null)
                    orphanMoveData = new List<OrphanMoveBlockData>();


                var newOrphanCoordinate =
                    bitData.Coordinate + travelDirection.ToVector2Int() * travelDistInt;

                //If someone is already planning to move to the target position, just choose one spot back
                if (orphanMoveData.Count > 0 &&
                    orphanMoveData.Any(x => x.intendedCoordinates == newOrphanCoordinate))
                {
                    newOrphanCoordinate += travelDirection.Reflected().ToVector2Int();
                    travelDistInt--;
                }

                var attached = attachedToOrphan.OfType<IBlockData>().ToList();
                
                blocks.SolveOrphanGroupPositionChange(bitData,
                    attached,
                    newOrphanCoordinate,
                    travelDirection,
                    travelDistInt,
                    new List<IBlockData>
                    {
                        targetBit
                    }, 
                    ref omdGroup);
                
                orphanMoveData.AddRange(omdGroup);
            }
        }

        private static void SolveOrphanGroupPositionChange(this IEnumerable<IBlockData> blockDatas,
            IBlockData mainOrphan,
            IReadOnlyList<IBlockData> orphanGroup,
            Vector2Int targetCoordinate,
            DIRECTION travelDirection,
            int travelDistance,
            IReadOnlyCollection<IBlockData> movingBits,
            ref List<OrphanMoveBlockData> orphanMoveData)
        {

            if (orphanGroup.Count == 1)
            {
                blockDatas.SolveOrphanPositionChange(mainOrphan, targetCoordinate, travelDirection, travelDistance,
                    movingBits,
                    ref orphanMoveData);
                return;
            }


            //Debug.LogError($"Moving Orphan group, Count: {orphanGroup.Count}");

            //var lastLocation = Vector2Int.zero;

            var distances = new float[orphanGroup.Count];

            var index = -1;
            var shortestDistance = 999f;


            for (var i = 0; i < orphanGroup.Count; i++)
            {
                var orphan = orphanGroup[i];
                var relative = orphan.Coordinate - mainOrphan.Coordinate;
                var desiredLocation = targetCoordinate + relative;

                var stayingBlocks = new List<IBlockData>(blockDatas);
                if (!movingBits.IsNullOrEmpty())
                {
                    //Check only the Bits on the Bot that wont be moving
                    foreach (var attachableBase in movingBits)
                    {
                        stayingBlocks.Remove(attachableBase);
                    }
                }


                //Checks to see if this orphan can travel unimpeded to the destination
                //If it cannot, set the destination to the block beside that which is blocking it.
                var hasClearPath = IsPathClear(stayingBlocks,
                    movingBits,
                    travelDistance,
                    orphan.Coordinate,
                    travelDirection,
                    desiredLocation,
                    out var clearCoordinate);

                if (!hasClearPath && clearCoordinate == Vector2Int.zero)
                    distances[i] = 999f;
                else if (!hasClearPath)
                    distances[i] = Vector2Int.Distance(orphan.Coordinate, clearCoordinate);
                else
                    distances[i] = Vector2Int.Distance(orphan.Coordinate, desiredLocation);

                if (distances[i] > shortestDistance)
                    continue;

                //index = i;
                shortestDistance = distances[i];
            }

            //Debug.LogError($"Shortest to move {orphanGroup[index].gameObject.name}, Distance: {shortestDistance}");
            //Debug.Break();

            foreach (var orphan in orphanGroup)
            {
                //var relative = orphan.Coordinate - mainOrphan.Coordinate;
                //var desiredLocation = targetCoordinate + relative;

                var newCoordinate = orphan.Coordinate + travelDirection.ToVector2Int() * (int) shortestDistance;

                orphanMoveData.Add(new OrphanMoveBlockData
                {
                    blockData = orphan,
                    moveDirection = travelDirection,
                    distance = shortestDistance,
                    startingCoordinates = orphan.Coordinate,
                    intendedCoordinates = newCoordinate
                });
            }
        }

        private static bool IsPathClear(IReadOnlyCollection<IBlockData> stayingBlocks,
            IEnumerable<IBlockData> toIgnore, 
            int distance, 
            Vector2Int currentCoordinate, 
            DIRECTION moveDirection,
            Vector2Int targetCoordinate, 
            out Vector2Int clearCoordinate)
        {
            //var distance = (int) orphanMoveData.distance;
            var coordinate = currentCoordinate;

            //If the distance starts at zero, its already at the TargetCoordinate
            clearCoordinate = distance == 0 ? targetCoordinate : Vector2Int.zero;

            while (distance > 0)
            {
                coordinate += moveDirection.ToVector2Int();
                var occupied = stayingBlocks
                    .Where(x => !toIgnore.Contains(x))
                    .FirstOrDefault(x => x.Coordinate == coordinate);

                if (occupied == null)
                    clearCoordinate = coordinate;

                distance--;
            }

            return targetCoordinate == clearCoordinate;
        }
        
                /// <summary>
        /// Solve the position change required for a single orphan. If moving a group ensure you use SolveOrphanGroupPositionChange
        /// </summary>
        /// <param name="orphanedBit"></param>
        /// <param name="targetCoordinate"></param>
        /// <param name="travelDirection"></param>
        /// <param name="travelDistance"></param>
        /// <param name="movingBits"></param>
        /// <param name="orphanMoveData"></param>
        /// <param name="lastLocation"></param>
        private static void SolveOrphanPositionChange(this IEnumerable<IBlockData> attachables, 
                    IBlockData orphanedBit, 
                    Vector2Int targetCoordinate, 
                    DIRECTION travelDirection,
                    int travelDistance, 
                    IReadOnlyCollection<IBlockData> movingBits, 
                    ref List<OrphanMoveBlockData> orphanMoveData)
        {
            //Loop ensures that the orphaned blocks which intend on moving, are able to reach their destination without any issues.

            //Check only the Bits on the Bot that wont be moving
            var stayingBlocks = new List<IBlockData>(attachables);
            foreach (var attachableBase in movingBits)
            {
                stayingBlocks.Remove(attachableBase);
            }

            //Checks to see if this orphan can travel unimpeded to the destination
            //If it cannot, set the destination to the block beside that which is blocking it.
            var hasClearPath = IsPathClear(stayingBlocks, movingBits, travelDistance, orphanedBit.Coordinate,
                travelDirection, targetCoordinate, out var clearCoordinate);

            //If there's no clear solution, then we will try and solve the overlap here
            if (!hasClearPath && clearCoordinate == Vector2Int.zero)
            {
                return;
                //Debug.LogError("Orphan has no clear path to intended Position");
                throw new Exception("NEED TO LOOK AT WHAT IS HAPPENING HERE");

                //Make sure that there's no overlap between orphans new potential positions & existing staying Bits
                //stayingBlocks.SolveCoordinateOverlap(travelDirection, ref desiredLocation);
            }
            else if (!hasClearPath)
            {
                //Debug.LogError($"Path wasn't clear. Setting designed location to {clearCoordinate} instead of {desiredLocation}");
                targetCoordinate = clearCoordinate;
            }
            
            //lastPosition = targetCoordinate;

            orphanMoveData.Add(new OrphanMoveBlockData
            {
                blockData = orphanedBit,
                moveDirection = travelDirection,
                distance = travelDistance,
                startingCoordinates = orphanedBit.Coordinate,
                intendedCoordinates = targetCoordinate
            });
        }
        
 //============================================================================================================//

        /// <summary>
        /// Function will review and detach any blocks that no longer have a connection to the core.
        /// </summary>
        public static bool CheckHasDisconnects(this IEnumerable<IBlockData> blockDatas)
        {
            var toSolve = new List<IBlockData>(blockDatas);

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
        public static List<T> GetBlocksAround<T>(this IEnumerable<T> blockDatas,
            T from, bool includeCorners = false) where T: IBlockData
        {
            var enumerable = blockDatas as T[] ?? blockDatas.ToArray();

            var outList = new List<T>
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

        public static IBlockData GetBlockDataNextTo(this IEnumerable<IBlockData> blockDatas, Vector2Int from,
            DIRECTION direction)
        {
            return blockDatas.GetBlockDataNextTo(from, direction.ToVector2Int());
        }

        public static IBlockData GetBlockDataNextTo(this IEnumerable<IBlockData> blockDatas, Vector2Int from,
            Vector2Int direction)
        {
            var coord = from + direction;

            return blockDatas.FirstOrDefault(a => a.Coordinate == coord);
        }
        public static T GetBlockDataNextTo<T>(this IEnumerable<T> blockDatas, Vector2Int from,
            DIRECTION direction) where T: IBlockData
        {
            return blockDatas.GetBlockDataNextTo(from, direction.ToVector2Int());
        }
        public static T GetBlockDataNextTo<T>(this IEnumerable<T> blockDatas, Vector2Int from,
            Vector2Int direction) where T: IBlockData
        {
            var coord = from + direction;

            return blockDatas.FirstOrDefault(a => a.Coordinate == coord);
        }

        //============================================================================================================//

        #region Path to Core Checks

        /// <summary>
        /// Returns whether or not this AttachableBase has a clear path to the core.
        /// </summary>
        /// <param name="blockDatas"></param>
        /// <param name="checking"></param>
        /// <param name="toIgnore"></param>
        /// <returns></returns>
        public static bool HasPathToCore(this IEnumerable<IBlockData> blockDatas,
            IBlockData checking,
            List<Vector2Int> toIgnore = null)
        {
            var travelled = new List<Vector2Int>();
            //Debug.LogError("STARTED TO CHECK HERE");
            return PathAlgorithm(blockDatas, checking, toIgnore, ref travelled);
        }

        private static bool PathAlgorithm(IEnumerable<IBlockData> attachedBlockDatas, IBlockData current,
            ICollection<Vector2Int> toIgnore, ref List<Vector2Int> travelled)
        {
            //If we're on (0, 0) we've reached the core, so go back up through
            if (current.Coordinate == Vector2Int.zero)
                return true;

            //Get list of attachables around the current attachable
            var blockDatasAround = attachedBlockDatas.GetBlocksAround(current);

            for (var i = 0; i < blockDatasAround.Count; i++)
            {
                //If there's no attachable, keep going
                if (blockDatasAround[i] == null || blockDatasAround[i].Coordinate == Vector2Int.zero)
                    continue;

                // If ignore list contains this Coordinate, keep going
                if (toIgnore != null && toIgnore.Contains(blockDatasAround[i].Coordinate))
                {
                    //Debug.LogError($"toIgnore contains {attachablesAround[i].Coordinate}");
                    blockDatasAround[i] = null;
                    continue;
                }

                if (blockDatasAround[i].ClassType == nameof(EnemyAttachable))
                {
                    throw new NotImplementedException();
                    /*blockDatasAround[i] = null;
                    continue;*/
                }

                // If we've not already been at this Coordinate, keep going
                if (!travelled.Contains(blockDatasAround[i].Coordinate))
                    continue;

                //Debug.LogError($"travelled already contains {around[i].Coordinate}");
                blockDatasAround[i] = null;
            }

            //Check to see if the list is completely null
            if (blockDatasAround.All(ab => ab == null))
            {
                //Debug.LogError($"FAILED. Nothing around {current}", current);
                return false;
            }

            //If everything checks out, lets say we've been here
            travelled.Add(current.Coordinate);

            //Get a list of all non-null Attachables ordered by the shortest distance to the core
            var closestBlockDatas = blockDatasAround
                .Where(ab => ab != null && ab.ClassType != null)
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

        #endregion //Path to Core Checks

        public static void CreateBotPreview(this List<IBlockData> blockDatas, in RectTransform containerRect)
        {
            Transform[] allChildren = containerRect.GetComponentsInChildren<Transform>();
            if (allChildren.Length > 0)
            {
                for (int i = allChildren.Length - 1; i >= 0; i--)
                {
                    if (allChildren[i] == containerRect.transform)
                    {
                        continue;
                    }

                    Image image = allChildren[i].GetComponent<Image>();
                    if (image != null)
                    {
                        Recycler.Recycle<Image>(image);
                    }
                    else
                    {
                        GameObject.Destroy(allChildren[i]);
                    }
                }
            }

            if (blockDatas == null)
            {
                return;
            }

            Image CreateImageObject(object className, object typeName, object extra = null)
            {
                var temp = new GameObject($"{className}_{typeName}{(extra != null ? $"_{extra}" : string.Empty)}");
                return temp.AddComponent<Image>();
            }

            void BotDisplaySetPosition(RectTransform newImageRect, int xOffset, int yOffset)
            {
                newImageRect.pivot = new Vector2(0.5f, 0.5f);
                newImageRect.anchoredPosition = new Vector2Int(xOffset * 50, yOffset * 50);
                newImageRect.sizeDelta = new Vector2(50, 50);
                newImageRect.localScale = Vector3.one;
            }

            Image imageObject;
            RectTransform rect;
            Transform botDisplayRectTransform = containerRect.transform;

            var damageProfile = FactoryManager.Instance.DamageProfile;
            var partFactory = FactoryManager.Instance.GetFactory<PartAttachableFactory>();
            var bitFactory = FactoryManager.Instance.GetFactory<BitAttachableFactory>();
            var componentFactory = FactoryManager.Instance.GetFactory<ComponentFactory>();
            var crateFactory = FactoryManager.Instance.GetFactory<CrateFactory>();


            foreach (var blockData in blockDatas)
            {
                if (!Recycler.TryGrab(out imageObject))
                {
                    imageObject = CreateImageObject(blockData.ClassType, blockData.Type);
                }

                rect = (RectTransform)imageObject.transform;
                rect.SetParent(botDisplayRectTransform, false);

                BotDisplaySetPosition(rect, blockData.Coordinate.x, blockData.Coordinate.y);

                switch (blockData)
                {
                    case PartData _:
                        imageObject.sprite = partFactory.GetProfileData((PART_TYPE) blockData.Type).GetSprite();
                        break;
                    case BitData bitData:
                        imageObject.sprite = bitFactory.GetBitProfile((BIT_TYPE)blockData.Type).GetSprite(bitData.Level);
                        var startingHealth = bitFactory.GetBitRemoteData((BIT_TYPE)blockData.Type).levels[bitData.Level].health;

                        float healthPercentage = bitData.Health / startingHealth;

                        var damageSprite = damageProfile.GetDetailSprite(healthPercentage);

                        if (damageSprite == null || bitData.Health <= 0)
                            continue;

                        if (!Recycler.TryGrab(out Image damageImage))
                        {
                            damageImage = CreateImageObject(bitData.ClassType, bitData.Type, "Damage");
                        }

                        var damageRect = (RectTransform)imageObject.transform;

                        damageRect.SetParent(botDisplayRectTransform, false);
                        BotDisplaySetPosition(damageRect, bitData.Coordinate.x, bitData.Coordinate.y);

                        damageImage.sprite = damageSprite;

                        break;
                    case JunkBitData _:
                        imageObject.sprite = bitFactory.GetJunkBitSprite();
                        continue;
                    case CrateData crateData:
                        imageObject.sprite = crateFactory.GetCrateSprite(crateData.Level);
                        continue;
                }
            }

            if (blockDatas.Count > 0)
                return;

            if (!Recycler.TryGrab(out imageObject))
            {
                imageObject = CreateImageObject(nameof(Part), PART_TYPE.CORE);
            }

            rect = (RectTransform)imageObject.transform;
            rect.SetParent(botDisplayRectTransform, false);

            BotDisplaySetPosition(rect, 0, 0);

            imageObject.sprite = partFactory.GetProfileData(PART_TYPE.CORE).GetSprite();
        }

        public static List<IAttachable> ImportBlockDatas(this List<IBlockData> blockDatas, bool inScrapyardForm)
        {
            var attachables = new List<IAttachable>();

            foreach (var blockData in blockDatas)
            {
                switch (blockData)
                {
                    case BitData bitData:
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateScrapyardObject<ScrapyardBit>(bitData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<Bit>(bitData));
                        }
                        break;
                    case PartData partData:
                        if (inScrapyardForm)
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateScrapyardObject<ScrapyardPart>(partData));
                        }
                        else
                        {
                            attachables.Add(FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<Part>(partData));
                        }
                        break;
                    case JunkBitData _:
                        var junkBit = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateJunkObject<JunkBit>();
                        junkBit.Coordinate = blockData.Coordinate;
                        attachables.Add(junkBit);
                        break;
                    case CrateData crateData:
                        var crate = FactoryManager.Instance.GetFactory<CrateFactory>().CreateCrateObject(crateData);
                        attachables.Add(crate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(blockData.ClassType), blockData.ClassType, null);
                }
            }

            return attachables;
        }
        
        //Get All Connected Detachables
        //====================================================================================================================//

        #region Get All Connected Detachables
        
        public static void GetAllConnectedBlocks<T>(this IEnumerable<T> blocks,
            T current,
            IEnumerable<T> toIgnore,
            ref List<T> outBlocks) where T: IBlockData
        {
            blocks.ToList().GetAllConnectedBlocks(current, toIgnore.ToArray(), ref outBlocks);
        }

        public static void GetAllConnectedBlocks<T>(this List<T> blocks,
            T current,
            T[] toIgnore,
            ref List<T> outBlocks) where T : IBlockData
        {
            var blocksAround = blocks.GetBlocksAround(current);

            outBlocks.Add(current);

            foreach (var blockData in blocksAround)
            {
                if (blockData.Coordinate == Vector2Int.zero)
                    continue;
                
                if (toIgnore != null && toIgnore.Contains(blockData))
                    continue;

                if (outBlocks.Contains(blockData))
                    continue;

                blocks.GetAllConnectedBlocks(blockData, toIgnore, ref outBlocks);
            }
        }
        
        public static void GetAllConnectedBlocks(this List<IBlockData> blocks,
            IBlockData current,
            IBlockData[] toIgnore,
            ref List<IBlockData> outBlocks)
        {
            var blocksAround = blocks.GetBlocksAround(current);

            outBlocks.Add(current);

            foreach (var canDetach in blocksAround)
            {
                if (toIgnore != null && toIgnore.Contains(canDetach))
                    continue;

                if (outBlocks.Contains(canDetach))
                    continue;

                blocks.GetAllConnectedBlocks(canDetach, toIgnore, ref outBlocks);
            }
        }

        #endregion //Get All Connected Detachables
        
        //Get Closest Attachable
        //====================================================================================================================//

        #region Get Closest Attachable

        public static T GetClosestBlockData<T>(this List<T> blocks, IBlockData currentBlockData) where T : IBlockData
        {
            if (blocks.Count == 1)
                return blocks[0];

            //var checkPosition = attachable.Coordinate;

            IBlockData selected = null;

            var smallestDist = 999f;

            foreach (var blockData in blocks)
            {
                if (blockData.Equals(currentBlockData))
                    continue;

                //attached.SetColor(Color.white);

                var dist = Vector2Int.Distance(blockData.Coordinate, currentBlockData.Coordinate);
                if (dist > smallestDist)
                    continue;

                smallestDist = dist;
                selected = blockData;
            }

            //selected.SetColor(Color.magenta);

            return (T) selected;
        }

        #endregion //Get Closest Attachable
    }
}
