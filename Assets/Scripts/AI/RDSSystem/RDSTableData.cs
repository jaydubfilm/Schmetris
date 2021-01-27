using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

        public int DropChance => m_dropChance;

        public int NumDrops => m_numDrops;

        public bool EvenWeighting => m_weightingType == WEIGHTING_TYPE.Even;

        public List<RDSLootData> RDSLootDatas => m_rdsLootDatas;

        [FoldoutGroup("$Name"), SerializeField] private string Name = "Loot Drop";

        [HorizontalGroup("$Name/Row1"), SerializeField, OnValueChanged("UpdateProbability"), LabelWidth(50), ToggleLeft]
        private bool AlwaysDrops;

        [HorizontalGroup("$Name/Row1"), SerializeField, Range(0, 100), SuffixLabel("%", true), DisableIf("AlwaysDrops"), LabelWidth(60), LabelText("Chance")]
        private int m_dropChance;
        

        [FoldoutGroup("$Name"), SerializeField, LabelText("Drop Count")]
        private int m_numDrops;

        [FoldoutGroup("$Name"), SerializeField, OnValueChanged("UpdateWeightType")]
        private WEIGHTING_TYPE m_weightingType;

        [TitleGroup("$Name/Loot Possibilities"), SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true), PropertyOrder(100), Space(10f)]
        private List<RDSLootData> m_rdsLootDatas;


        #region Unity Editor

        private int _dropChance;
        
        [HorizontalGroup("$Name/Loot Possibilities/Row1", Width = 100),Button("Add Possibility", ButtonSizes.Small), PropertyOrder(90)]
        private void AddToLootData()
        {
            m_rdsLootDatas.Add(new RDSLootData());
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
#endif


        }

        
        
        private void UpdateWeightType()
        {
            for (int i = 0; i < m_rdsLootDatas.Count; i++)
            {
                m_rdsLootDatas[i].showProbability = m_weightingType == WEIGHTING_TYPE.Weighted;
            }
        }
        
        private void UpdateProbability()
        {
            if (AlwaysDrops)
            {
                _dropChance = m_dropChance;
                m_dropChance = 100;
                return;
            }

            m_dropChance = _dropChance;
        }

        #endregion //Unity Editor

    }
}