using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Recycling;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Utilities.Extensions
{
    public static class BotExtensions
    {
        //============================================================================================================//
        
        #region Import/Export
        
        public static string ExportLayout(this Bot bot)
        {
            //TODO Need to consider that there will be parts & bits attached to the bot

            var data = bot.attachedBlocks.OfType<ISaveable>().Select(x => x.ToBlockData())
                .ToArray();

            var blah = JsonConvert.SerializeObject(data, Formatting.None);
            
            Debug.Log(blah);
            
            
            return string.Empty;
        }
        public static void ImportLayout(this Bot bot, string jsonLayout)
        {
            var loadedBlocks = JsonConvert.DeserializeObject<List<BlockData>>(jsonLayout);

            foreach (var block in loadedBlocks)
            {
                IAttachable attachable;
                switch (block.ClassType)
                {
                    case nameof(Bit):
                        attachable = FactoryManager.Instance.GetFactory<BitAttachableFactory>().CreateObject<IAttachable>(block);
                        break;
                    case nameof(Part):
                        attachable = FactoryManager.Instance.GetFactory<PartAttachableFactory>().CreateObject<IAttachable>(block);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(block.ClassType), block.ClassType, null);
                }
                
                bot.AttachNewBlock(attachable.Coordinate, attachable);
            }
            
        }
        
        #endregion //Import/Export
        
        //============================================================================================================//
        
        public static List<BlockData> GetBlockDatas(this Bot bot)
        {
            var blockDatas = new List<BlockData>();
            
            var attachables = new List<IAttachable>(bot.attachedBlocks);
            //var ignoreAttachables = bot.PendingDetach == null
            //    ? new List<IAttachable>()
            //    : new List<IAttachable>(bot.PendingDetach);

            foreach (var attachable in attachables.Where(attachable => !(attachable is ICanDetach iCanDetach && iCanDetach.PendingDetach)))
            {
                if (attachable is ISaveable saveable)
                {
                    blockDatas.Add(saveable.ToBlockData());
                }
            }

            return blockDatas;
        }

        public static List<BlockData> GetBlockDatas(this ScrapyardBot bot)
        {
            var blockDatas = new List<BlockData>();

            var attachables = new List<IAttachable>(bot.AttachedBlocks);

            foreach (var attachable in attachables)
            {
                if (attachable is ISaveable saveable)
                {
                    blockDatas.Add(saveable.ToBlockData());
                }
            }

            return blockDatas;
        }


        //============================================================================================================//

        /// <summary>
        /// Fill ref List with all Bits of similar level & type in specified direction.
        /// </summary>
        /// <param name="bot"></param>
        /// <param name="target"></param>
        /// <param name="direction"></param>
        /// <param name="iCanCombos"></param>
        public static void ComboCount<T>(this Bot bot, ICanCombo<T> target, DIRECTION direction, ref List<ICanCombo> iCanCombos) where T: Enum
        {
            var combo = bot.attachedBlocks.OfType<ICanCombo>();
            combo.ComboCountAlgorithm(target.Type, target.level, target.Coordinate, direction.ToVector2Int(),
                ref iCanCombos);
        }


        //====================================================================================================================//

        public static void SetColliderActive(this Bot bot, bool state)
        {
            foreach (var collidableBase in bot.attachedBlocks.OfType<CollidableBase>())
            {
                collidableBase.SetColliderActive(state);
            }
        }


        //====================================================================================================================//

        public static void MoveOrphanPieces(this Bot bot,IReadOnlyList<OrphanMoveData> orphans, Action onFinishedCallback)
        {
            bot.StartCoroutine(MoveOrphanPiecesCoroutine(bot, orphans, bot.TEST_MergeTime, onFinishedCallback));
        }
        
        private static IEnumerator MoveOrphanPiecesCoroutine(Bot bot,
            IReadOnlyList<OrphanMoveData> orphans,
            float seconds,
            Action onFinishedCallback)
        {
            var compositeCollider2D = (CompositeCollider2D) bot.Collider;
            var transform = bot.transform;
            
            //Prepare Bits to be moved
            //--------------------------------------------------------------------------------------------------------//

            foreach (var omd in orphans)
            {
                omd.attachableBase.Coordinate = omd.intendedCoordinates;
                (omd.attachableBase as Bit)?.SetColliderActive(false);
                
                if (omd.attachableBase is ICanCombo iCanCombo)
                    iCanCombo.IsBusy = true;
            }
            
            //We're going to want to regenerate the shape while things are moving
            compositeCollider2D.GenerateGeometry();
            
            //--------------------------------------------------------------------------------------------------------//

            var t = 0f;
            

            //Same as above but for Orphans
            //--------------------------------------------------------------------------------------------------------//

            var orphanTransforms = orphans.Select(bt => bt.attachableBase.transform).ToArray();
            var orphanTransformPositions = orphanTransforms.Select(bt => bt.localPosition).ToArray();
            /*var orphanTargetPositions = orphans.Select(o =>
                transform.InverseTransformPoint((Vector2) transform.position +
                                                (Vector2) o.intendedCoordinates * Constants.gridCellSize)).ToArray();*/
            var orphanTargetPositions = orphans.Select(o =>
                (Vector2) o.intendedCoordinates * Constants.gridCellSize).ToArray();
            //--------------------------------------------------------------------------------------------------------//


            //Move bits towards target
            while (t / seconds <= 1f)
            {
                var td = t / seconds;

                //Move the orphans into their new positions
                //----------------------------------------------------------------------------------------------------//
                
                for (var i = 0; i < orphans.Count; i++)
                {
                    var bitTransform = orphanTransforms[i];
                   
                    //Debug.Log($"Start {bitTransform.position} End {position}");

                    bitTransform.localPosition = Vector2.Lerp(orphanTransformPositions[i],
                        orphanTargetPositions[i], td);
                    
                    SSDebug.DrawArrow(bitTransform.position,transform.TransformPoint(orphanTargetPositions[i]), Color.red);
                }
                
                //----------------------------------------------------------------------------------------------------//

                t += Time.deltaTime;

                yield return null;
            }

            //Re-enable the colliders on our orphans, and ensure they're in the correct position
            for (var i = 0; i < orphans.Count; i++)
            {
                var attachable = orphans[i].attachableBase;
                orphanTransforms[i].localPosition = orphanTargetPositions[i];
                
                if(attachable is CollidableBase collidableBase)
                    collidableBase.SetColliderActive(true);

                if (attachable is ICanCombo canCombo)
                    canCombo.IsBusy = false;
            }
            
            //Now that everyone is where they need to be, wrap things up
            //--------------------------------------------------------------------------------------------------------//

            compositeCollider2D.GenerateGeometry();

            onFinishedCallback?.Invoke();
            
            //--------------------------------------------------------------------------------------------------------//
        }

    }
}

