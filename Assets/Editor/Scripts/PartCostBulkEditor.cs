using System;
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
            [DisplayAsString, ShowInInspector, PropertyOrder(-1000)] 
            [TableColumnWidth(75, Resizable = true)]
            public string Name => $"{type}_{partLevel}";
            
            [TableColumnWidth(45, Resizable = false)]
            public int unlock;
            [TableColumnWidth(70, Resizable = false)]
            public float hp;

            [HideInInspector, HideInTables]
            public PART_TYPE type;
            [HideInInspector, HideInTables]
            public int index;
            [HideInInspector, HideInTables]
            public int partLevel;
            
            
            [TableColumnWidth(50, Resizable = false)]
            public int gadget;
            [TableColumnWidth(50, Resizable = false)]
            public int gizmo;
            [TableColumnWidth(50, Resizable = false)]
            public int thingy;
            [TableColumnWidth(50, Resizable = false)]
            public int doohickey;
            [TableColumnWidth(50, Resizable = false)]
            public int whatcha;
            
            [GUIColor(1,0.35f,0.35f)]
            public int red;
            [GUIColor(0.35f,1f,0.35f)]
            public int green;
            [GUIColor(0.35f,0.35f,1f)]
            public int blue;
            public int grey;
            [GUIColor(1f,1f,0.35f)]
            public int yellow;

            
        }

        private static PartCostBulkEditor _window;
        
        [MenuItem("Window/Star Salvager/Bulk Part Cost Editor")]
        public static void BulkPartCostEditor()
        {
            _window = GetWindow<PartCostBulkEditor>("Bulk Part Cost Editor", true);
            _window.Show();

            _window._partCostDatas = ToPartCostDataList(FindObjectOfType<FactoryManager>().PartsRemoteData.partRemoteData);

            /*if (Selection.activeObject != null && Selection.activeObject.GetType() == typeof(Texture2D))
                spriteTools.spritesheet = (Texture2D) Selection.activeObject;*/
        }

        [Button(ButtonSizes.Large), HorizontalGroup("Row1")]
        private void RefreshList()
        {
            _window._partCostDatas = ToPartCostDataList(FindObjectOfType<FactoryManager>().PartsRemoteData.partRemoteData);
        }
        
        [Button(ButtonSizes.Large), HorizontalGroup("Row1")]
        private void SaveList()
        {
            var list = FindObjectOfType<FactoryManager>().PartsRemoteData;

            for (int i = 0; i < _partCostDatas.Length; i++)
            {
                var partCostData = _partCostDatas[i];
                
                var lvlData = list.partRemoteData[partCostData.index].levels[partCostData.partLevel];

                lvlData.health = partCostData.hp;
                lvlData.unlockLevel = partCostData.unlock;

                FillData(partCostData, ref lvlData);

                list.partRemoteData[partCostData.index].levels[partCostData.partLevel] = lvlData;
            }


            //var core0 = _partCostDatas[0];
//
//
            //var lvlData= list.partRemoteData[0].levels[0];
            //
            //lvlData.health = core0.hp;
//
            //list.partRemoteData[0].levels[0] = lvlData;
            
            EditorUtility.SetDirty(list);
            AssetDatabase.SaveAssets();

            //_window._partCostDatas = ToPartCostDataList(FindObjectOfType<FactoryManager>().PartsRemoteData.partRemoteData);
        }

        [SerializeField, TableList(AlwaysExpanded = true,HideToolbar = true, CellPadding = 10)]
        private PartCostData[] _partCostDatas;


        private static PartCostData[] ToPartCostDataList(IReadOnlyList<PartRemoteData> partRemoteDatas)
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


                        gadget = components[COMPONENT_TYPE.GADGET],
                        gizmo = components[COMPONENT_TYPE.GIZMO],
                        doohickey = components[COMPONENT_TYPE.DOHICKEY],
                        whatcha = components[COMPONENT_TYPE.CALLIT],
                        thingy = components[COMPONENT_TYPE.THINGY],

                        red = bits[BIT_TYPE.RED],
                        green = bits[BIT_TYPE.GREEN],
                        blue = bits[BIT_TYPE.BLUE],
                        grey = bits[BIT_TYPE.GREY],
                        yellow = bits[BIT_TYPE.YELLOW],

                    });
                } 
            }

            return outData.ToArray();
        }

        private static void FillData(PartCostData partCostData, ref PartLevelData partLevelData)
        {
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.GADGET, partCostData.gadget, ref partLevelData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.GIZMO, partCostData.gizmo, ref partLevelData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.CALLIT, partCostData.whatcha, ref partLevelData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.DOHICKEY, partCostData.doohickey, ref partLevelData);
            FillCost(CraftCost.TYPE.Component, (int) COMPONENT_TYPE.THINGY, partCostData.thingy, ref partLevelData);
            
            
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.RED, partCostData.red, ref partLevelData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.GREEN, partCostData.green, ref partLevelData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.GREY, partCostData.grey, ref partLevelData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.YELLOW, partCostData.yellow, ref partLevelData);
            FillCost(CraftCost.TYPE.Bit, (int) BIT_TYPE.BLUE, partCostData.blue, ref partLevelData);

        }

        private static void FillCost(CraftCost.TYPE craftType, int type, int amount, ref PartLevelData partLevelData)
        {
            var cost = new CraftCost { resourceType = craftType, type = type, amount = amount};

            if (amount <= 0)
            {
                if (!partLevelData.cost.Contains(cost))
                    return;
                
                partLevelData.cost.Remove(cost);
            }
            else
            {
                if(!partLevelData.cost.Contains(cost))
                    partLevelData.cost.Add(cost);
                else
                {
                    var index = partLevelData.cost.IndexOf(cost);

                    partLevelData.cost[index] = cost;
                }
            }
            
            
        }
    }
}


