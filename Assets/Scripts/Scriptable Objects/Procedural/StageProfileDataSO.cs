using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager;
using StarSalvager.AI;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Stage Profile", menuName = "Star Salvager/Procedural/Stage Profile")]
    public class StageProfileDataSO : ScriptableObject
    {


        //Enums
        //====================================================================================================================//

        public enum TYPE
        {
            ASTEROID,
            BUMPER,
            CLOUD
        }

        //Structs
        //====================================================================================================================//

        [Serializable]
        public struct StageData
        {
            [InfoBox("Clouds are not yet implemented", InfoMessageType.Warning, VisibleIf = "@type == TYPE.CLOUD")]
            [HorizontalGroup("Type"), HideLabel]
            public TYPE type;

            [HorizontalGroup("Type"), ShowIf("type", TYPE.ASTEROID), LabelWidth(40)]
            public ASTEROID_SIZE size;

            [MinMaxSlider(0, 500, true), DisableIf("type", TYPE.CLOUD)]
            public Vector2Int spawnsPerMin;

#if UNITY_EDITOR

            private bool HideCheck() => type == TYPE.ASTEROID;
#endif
        }

        //Properties
        //====================================================================================================================//

        public string name;

        [MinMaxSlider(0, 500, true)] public Vector2Int bitSpawnsPerMin;

        [Tooltip("Dynamic spawning takes into consideration the players current state to decide what to spawn")]
        public bool useDynamicBitSpawning = true;

        #region Fixed Bit Spawing

        [HideIfGroup("useDynamicBitSpawning")]
        [BoxGroup("useDynamicBitSpawning/Bit Spawn Ratios"), LabelText("Red Bit %"), LabelWidth(75), Space(10f)]
        [SerializeField, Required, ProgressBar(0, 100, ColorGetter = "@GetColor(BIT_TYPE.RED)"),
         OnValueChanged("BalanceBits")]
        private int redBitsPercentage;

        [HideIfGroup("useDynamicBitSpawning")]
        [BoxGroup("useDynamicBitSpawning/Bit Spawn Ratios"), LabelText("Blue Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, ColorGetter = "@GetColor(BIT_TYPE.BLUE)"),
         OnValueChanged("BalanceBits")]
        private int blueBitsPercentage;

        [HideIfGroup("useDynamicBitSpawning")]
        [BoxGroup("useDynamicBitSpawning/Bit Spawn Ratios"), LabelText("Green Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, ColorGetter = "@GetColor(BIT_TYPE.GREEN)"),
         OnValueChanged("BalanceBits")]
        private int greenBitsPercentage;

        [HideIfGroup("useDynamicBitSpawning")]
        [BoxGroup("useDynamicBitSpawning/Bit Spawn Ratios"), LabelText("Yellow Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, ColorGetter = "@GetColor(BIT_TYPE.YELLOW)"),
         OnValueChanged("BalanceBits")]
        private int yellowBitsPercentage;

        [HideIfGroup("useDynamicBitSpawning")]
        [BoxGroup("useDynamicBitSpawning/Bit Spawn Ratios"), LabelText("Grey Bit %"), LabelWidth(75)]
        [SerializeField, Required, ProgressBar(0, 100, ColorGetter = "@GetColor(BIT_TYPE.GREY)"),
         OnValueChanged("BalanceBits")]
        private int greyBitsPercentage;

        #endregion //Fixed Bit Spawing

        [TableList(AlwaysExpanded = true)] public List<StageData> stageData;

        //Unity Editor
        //====================================================================================================================//

#if UNITY_EDITOR

        private Color GetColor(BIT_TYPE bitType) => bitType.GetColor();

        private void BalanceBits()
        {

            var sum = redBitsPercentage + blueBitsPercentage + greenBitsPercentage + yellowBitsPercentage +
                      greyBitsPercentage;

            redBitsPercentage = Mathf.RoundToInt(((float) redBitsPercentage / sum) * 100f);
            blueBitsPercentage = Mathf.RoundToInt(((float) blueBitsPercentage / sum) * 100f);
            greenBitsPercentage = Mathf.RoundToInt(((float) greenBitsPercentage / sum) * 100f);
            yellowBitsPercentage = Mathf.RoundToInt(((float) yellowBitsPercentage / sum) * 100f);
            greyBitsPercentage = Mathf.RoundToInt(((float) greyBitsPercentage / sum) * 100f);

        }

        [HideIfGroup("useDynamicBitSpawning")]
        [Button("Balance Bits"), BoxGroup("useDynamicBitSpawning/Bit Spawn Ratios")]
        private void Reset()
        {
            redBitsPercentage =
                blueBitsPercentage = greenBitsPercentage = yellowBitsPercentage = greyBitsPercentage = 20;
        }

#endif

        //====================================================================================================================//

    }
}