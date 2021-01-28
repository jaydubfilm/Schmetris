using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [TitleGroup("$Name/Loot Possibilities"), SerializeField, TableList(AlwaysExpanded = true, HideToolbar = true), PropertyOrder(100), Space(10f), OnValueChanged("UpdateChance")]
        private List<RDSLootData> m_rdsLootDatas;


        #region Unity Editor

        private int _dropChance;
        
        [HorizontalGroup("$Name/Loot Possibilities/Row1", Width = 100),Button("Add Possibility", ButtonSizes.Small), PropertyOrder(90)]
        private void AddToLootData()
        {
            m_rdsLootDatas.Add(new RDSLootData());
            UpdateChance();
            
#if UNITY_EDITOR
            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();
#endif
        }
        public void UpdateChance()
        {
            switch (m_weightingType)
            {
                case WEIGHTING_TYPE.Even:
                    var count = m_rdsLootDatas.Count;
                    for (int i = 0; i < count; i++)
                    {
                        m_rdsLootDatas[i].percentChance = 1f / count;
                    }
                    break;
                case WEIGHTING_TYPE.Weighted:
                    var total = m_rdsLootDatas.Sum(x => x.Weight);
                    for (int i = 0; i < m_rdsLootDatas.Count; i++)
                    {
                        m_rdsLootDatas[i].percentChance = (float)m_rdsLootDatas[i].Weight / total;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            

        }
        
        
        private void UpdateWeightType()
        {
            for (int i = 0; i < m_rdsLootDatas.Count; i++)
            {
                m_rdsLootDatas[i].showProbability = m_weightingType == WEIGHTING_TYPE.Weighted;
            }

            UpdateChance();
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

        public void EditorUpdateChildren()
        {
            for (var i = 0; i < m_rdsLootDatas.Count; i++)
            {
                m_rdsLootDatas[i].EditorSetParentContainer(this);
            }
            
            UpdateChance();
        }

        #endregion //Unity Editor

    }
}