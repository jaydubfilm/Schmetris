using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    [Serializable]
    public class RDSTableData
    {
        public enum WEIGHTING_TYPE
        {
            Even,
            Weighted
        }

        [FoldoutGroup("Loot Table"), SerializeField, Range(0, 100)]
        private int m_dropChance;

        [FoldoutGroup("Loot Table"), SerializeField]
        private int m_numDrops;

        [FoldoutGroup("Loot Table"), SerializeField]
        private WEIGHTING_TYPE m_weightingType;

        [FoldoutGroup("Loot Table"), LabelText("Loot Possibilities"), SerializeField]
        private List<RDSLootData> m_rdsLootDatas;



        public int DropChance => m_dropChance;

        public int NumDrops => m_numDrops;

        public bool EvenWeighting => m_weightingType == WEIGHTING_TYPE.Even;

        public List<RDSLootData> RDSLootDatas => m_rdsLootDatas;
    }
}