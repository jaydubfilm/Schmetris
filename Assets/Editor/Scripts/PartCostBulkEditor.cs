﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using StarSalvager.Factories;
using StarSalvager.Factories.Data;
using UnityEditor;
using UnityEngine;

namespace StarSalvager.Editor
{
    public class PartCostBulkEditor : OdinEditorWindow
    {
        [Serializable]
        struct PartCostData
        {
            
            [HideInInspector, HideInTables]
            public PART_TYPE type;
            [HideInInspector, HideInTables]
            public int index;
            [HideInInspector, HideInTables]
            public int partLevel;
            
            
            [DisplayAsString, ShowInInspector, PropertyOrder(-1000)] 
            [TableColumnWidth(75, Resizable = true)]
            public string Name => $"{type}_{partLevel}";
            
            [TableColumnWidth(45, Resizable = false)]
            public int unlock;
            [TableColumnWidth(70, Resizable = false)]
            public float hp;

            [DisableIf("Lock")]
            public int priority;

            [SuffixLabel("KW/sec", true)]
            public float powerDraw;

            [DisableIf("Lock")]
            public BIT_TYPE burnType;
            [SuffixLabel("/sec", true), DisableIf("NoBurnType")]
            public float burnRate;
            
            
            [TableColumnWidth(50, Resizable = false)]
            public int nut;
            [TableColumnWidth(50, Resizable = false)]
            public int bolt;
            [TableColumnWidth(50, Resizable = false)]
            public int coil;
            [TableColumnWidth(50, Resizable = false)]
            public int chip;
            [TableColumnWidth(50, Resizable = false)]
            public int fusor;
            
            [GUIColor(1,0.35f,0.35f)]
            public int red;
            [GUIColor(0.35f,1f,0.35f)]
            public int green;
            [GUIColor(0.35f,0.35f,1f)]
            public int blue;
            public int grey;
            [GUIColor(1f,1f,0.35f)]
            public int yellow;

            public bool Lock => partLevel > 0;
            public bool NoBurnType => burnType == BIT_TYPE.NONE;
        }

        private static PartCostBulkEditor _window;
        
        [MenuItem("Window/Star Salvager/Bulk Part Cost Editor")]
        public static void BulkPartCostEditor()
        {
            _window = GetWindow<PartCostBulkEditor>("Bulk Part Cost Editor", true);
            _window.Show();

            //_window._partCostDatas = ToPartCostDataList(FindObjectOfType<FactoryManager>().PartsRemoteData.partRemoteData);

            /*if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(Texture2D))
                spriteTools.spritesheet = (Texture2D) Selection.activeObject;*/
        }

        [Button(ButtonSizes.Large), HorizontalGroup("Row1")]
        private void RefreshList()
        {
            //_window._partCostDatas = ToPartCostDataList(FindObjectOfType<FactoryManager>().PartsRemoteData.partRemoteData);
        }
        
        [Button(ButtonSizes.Large), HorizontalGroup("Row1")]
        private void SaveList()
        {
            /*var list = FindObjectOfType<FactoryManager>().PartsRemoteData;

            for (int i = 0; i < _partCostDatas.Length; i++)
            {
                var partCostData = _partCostDatas[i];
                
                var lvlData = list.partRemoteData[partCostData.index].levels[partCostData.partLevel];

                if (partCostData.partLevel == 0)
                {
                    list.partRemoteData[partCostData.index].priority = partCostData.priority;
                    list.partRemoteData[partCostData.index].burnType = partCostData.burnType;
                }

                lvlData.health = partCostData.hp;
                lvlData.powerDraw = partCostData.powerDraw;
                lvlData.unlockLevel = partCostData.unlock;
                lvlData.burnRate = partCostData.burnRate;

                FillData(partCostData, ref lvlData);

                list.partRemoteData[partCostData.index].levels[partCostData.partLevel] = lvlData;
            }*/


            //var core0 = _partCostDatas[0];
//
//
            //var lvlData= list.partRemoteData[0].levels[0];
            //
            //lvlData.health = core0.hp;
//
            //list.partRemoteData[0].levels[0] = lvlData;
            
            //EditorUtility.SetDirty(list);
            //AssetDatabase.SaveAssets();

            //_window._partCostDatas = ToPartCostDataList(FindObjectOfType<FactoryManager>().PartsRemoteData.partRemoteData);
        }

        //[SerializeField, TableList(AlwaysExpanded = true,HideToolbar = true, CellPadding = 10)]
        //private PartCostData[] _partCostDatas;


        /*private static PartCostData[] ToPartCostDataList(IReadOnlyList<PartRemoteData> partRemoteDatas)
        {
            var outData = new List<PartCostData>();

            for (var i = 0; i < partRemoteDatas.Count; i++)
            {
                var partRemoteData = partRemoteDatas[i];

                for (var j = 0; j < partRemoteData.levels.Count; j++)
                {
                    var costs = partRemoteData.levels[j].cost;

                    var components = Enum.GetValues(typeof(COMPONENT_TYPE)).Cast<COMPONENT_TYPE>().ToDictionary(
                        componentType => componentType,
                        componentType => costs.FirstOrDefault(x =>
                            x.resourceType == CraftCost.TYPE.Component && x.type == (int) componentType).amount);

                    var bits = Enum.GetValues(typeof(BIT_TYPE)).Cast<BIT_TYPE>().ToDictionary(bitType => bitType,
                        bitType => costs
                            .FirstOrDefault(x => x.resourceType == CraftCost.TYPE.Bit && x.type == (int) bitType)
                            .amount);

                    outData.Add(new PartCostData
                    {
                        index = i,
                        type = partRemoteData.partType,
                        partLevel = j,
                        
                        
                        unlock = partRemoteData.levels[j].unlockLevel,
                        hp = partRemoteData.levels[j].health,
                        
                        priority = partRemoteData.priority,
                        burnType = partRemoteData.burnType,
                        
                        burnRate = partRemoteData.levels[j].burnRate,
                        
                        powerDraw = partRemoteData.levels[j].powerDraw,


                        nut = components[COMPONENT_TYPE.NUT],
                        bolt = components[COMPONENT_TYPE.BOLT],
                        chip = components[COMPONENT_TYPE.CHIP],
                        fusor = components[COMPONENT_TYPE.FUSOR],
                        coil = components[COMPONENT_TYPE.COIL],

                        red = bits[BIT_TYPE.RED],
                        green = bits[BIT_TYPE.GREEN],
                        blue = bits[BIT_TYPE.BLUE],
                        grey = bits[BIT_TYPE.GREY],
                        yellow = bits[BIT_TYPE.YELLOW],

                    });
                } 
            }

            return outData.ToArray();
        }*/

        /*private static void FillData(PartCostData partCostData, ref PartRemoteData partRemoteData)
        {
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.NUT, partCostData.nut, ref partRemoteData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.BOLT, partCostData.bolt, ref partRemoteData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.FUSOR, partCostData.fusor, ref partRemoteData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.CHIP, partCostData.chip, ref partRemoteData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.COIL, partCostData.coil, ref partRemoteData);
            
            
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.RED, partCostData.red, ref partRemoteData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.GREEN, partCostData.green, ref partRemoteData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.GREY, partCostData.grey, ref partRemoteData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.YELLOW, partCostData.yellow, ref partRemoteData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.BLUE, partCostData.blue, ref partRemoteData);

        }*/

        /*private static void FillCost(CraftCost.TYPE craftType, int type, int amount, ref PartRemoteData partRemoteData)
        {
            var cost = new CraftCost { resourceType = craftType, type = type, amount = amount};

            if (amount <= 0)
            {
                if (!partRemoteData.cost.Contains(cost))
                    return;
                
                partRemoteData.cost.Remove(cost);
            }
            else
            {
                if(!partRemoteData.cost.Contains(cost))
                    partRemoteData.cost.Add(cost);
                else
                {
                    var index = partRemoteData.cost.IndexOf(cost);

                    partRemoteData.cost[index] = cost;
                }
            }
            
        }*/
    }
}


