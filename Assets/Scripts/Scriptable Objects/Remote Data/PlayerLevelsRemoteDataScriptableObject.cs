using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using UnityEngine;

using PlayerLevelRemoteData = StarSalvager.Factories.Data.PlayerLevelRemoteData;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Player Levels Remote", menuName = "Star Salvager/Scriptable Objects/Player Levels Data")]
    public class PlayerLevelsRemoteDataScriptableObject : ScriptableObject
    {
        //Properties
        //====================================================================================================================//

        public List<PART_TYPE> PartsUnlockedAtStart => partsUnlockedAtStart;
        [FoldoutGroup("Starting Unlocks"), TitleGroup("Starting Unlocks/Parts"), SerializeField, ValueDropdown("GetImplementedParts", IsUniqueList = true)]
        private List<PART_TYPE> partsUnlockedAtStart;

        public List<PatchData> PatchesUnlockedAtStart => patchesUnlockedAtStart
            .Select(x => new PatchData
        {
            Type = x.Type,
            Level = x.Level - 1
        }).ToList();
        [FoldoutGroup("Starting Unlocks"), TitleGroup("Starting Unlocks/Patches"), SerializeField, TableList]
        private List<PatchData> patchesUnlockedAtStart;

        //====================================================================================================================//
        
        [SerializeField, BoxGroup("Experience Points"), Range(0f,1f)]
        private float levelXPConstant = 0.915f;
        private float _levelXPConstant => 1f - levelXPConstant;
        
        [InfoBox("Due to the complexity of Patch Data Setting, these will not auto remove from available options like Parts", InfoMessageType.Warning)]
        [SerializeField, ListDrawerSettings(ShowPaging = false), OnValueChanged("FillLevelSummary", true)]
        private List<PlayerLevelRemoteData> playerLevelRemoteDatas;

        //Other Functions
        //====================================================================================================================//

        public IEnumerable<PlayerLevelRemoteData.UnlockData> GetUnlocksForLevel(in int level)
        {
            var unlockData =  new List<PlayerLevelRemoteData.UnlockData>(level >= playerLevelRemoteDatas.Count
                ? playerLevelRemoteDatas[playerLevelRemoteDatas.Count - 1].unlockData
                : playerLevelRemoteDatas[level].unlockData);

            //Ensure that we lower the level to follow indexing rules
            for (var i = 0; i < unlockData.Count; i++)
            {
                var data = unlockData[i];
                if (data.Unlock == PlayerLevelRemoteData.UNLOCK_TYPE.PART)
                    continue;

                data.Level--;
                unlockData[i] = data;
            }

            return unlockData;
        }
        
        public IEnumerable<PlayerLevelRemoteData.UnlockData> GetUnlocksUpToLevel(in int level)
        {
            var outList = new List<PlayerLevelRemoteData.UnlockData>();

            for (var i = 0; i <= level; i++)
            {
                if (i >= playerLevelRemoteDatas.Count)
                    break;
                outList.AddRange(playerLevelRemoteDatas[level].unlockData);
            }

            //Ensure that we lower the level to follow indexing rules
            for (var i = 0; i < outList.Count; i++)
            {
                var data = outList[i];
                if (data.Unlock == PlayerLevelRemoteData.UNLOCK_TYPE.PART)
                    continue;

                data.Level--;
                outList[i] = data;
            }

            return outList;
        }

        //Level XP Calculations
        //====================================================================================================================//
        
        #region Level XP Calculations

        //XP Info Considerations: https://www.youtube.com/watch?v=MCPruAKSG0g
        //Alt option: https://gamedev.stackexchange.com/a/20946
        //Based on: https://gamedev.stackexchange.com/a/13639
        private static int GetCurrentLevelConstant(in float constant, in int xp)
        {
            //level = constant * sqrt(XP)
            //level = (sqrt(100(2experience+25))+50)/100

            return Mathf.RoundToInt(constant * Mathf.Sqrt(xp));
        }

        private static int GetExperienceReqForLevelConstant(in float constant, in int level)
        {
            //XP = (level / constant)^2
            //experience =(level^2+level)/2*100-(level*100)

            return Mathf.RoundToInt(Mathf.Pow(level / constant, 2));
        }

        public static int GetCurrentLevel(in int xp)
        {
            var playerLevelRemoteDatas = FactoryManager.Instance.PlayerLevelsRemoteData.playerLevelRemoteDatas;
            
            for (int i = 0; i < playerLevelRemoteDatas.Count; i++)
            {
                if (xp > playerLevelRemoteDatas[i].xpRequired)
                    continue;

                return i;
            }

            return 0;
        }
        
        public static int GetXPForLevel(in int level)
        {
            var playerLevelRemoteDatas = FactoryManager.Instance.PlayerLevelsRemoteData.playerLevelRemoteDatas;
            
            return level >= playerLevelRemoteDatas.Count
                ? playerLevelRemoteDatas[playerLevelRemoteDatas.Count - 1].xpRequired
                : playerLevelRemoteDatas[level].xpRequired;
        }

        #endregion //Level XP Calculations

        //Unity Editor
        //====================================================================================================================//

        #region Unity Editor

#if UNITY_EDITOR

        [Serializable]
        private struct LevelSummaryData
        {
            [DisplayAsString]
            public int level;
            [VerticalGroup("Unlocks"), HideLabel, DisplayAsString]
            public string parts;
            [VerticalGroup("Unlocks"), HideLabel, DisplayAsString]
            public string patches;
        }
        
        
        [SerializeField, TableList(HideToolbar = true, AlwaysExpanded = true), FoldoutGroup("Levels Summary"), HideLabel]
       
        private List<LevelSummaryData> test;

        [SerializeField, PropertyOrder(-900), FoldoutGroup("Levels Summary")]
        private AnimationCurve levelCurve;
        
        [DisplayAsString, ShowInInspector, PropertyOrder(-1000), FoldoutGroup("Levels Summary")] 
        public int totalLevels { get; private set; }

        [OnInspectorInit]
        private void UpdateValues()
        {
            totalLevels = GetTotalStarsRequired();
        }

        [Button, PropertyOrder(-1100)] 
        private void GenerateLevelData()
        {
            var count = GetTotalStarsRequired();
            var maxXP = GetExperienceReqForLevelConstant(_levelXPConstant, count);

            levelCurve = new AnimationCurve();
            
            for (var i = 0; i < count; i++)
            {
                if(playerLevelRemoteDatas.Count == i)
                    playerLevelRemoteDatas.Add(new PlayerLevelRemoteData
                    {
                        level = i + 1,
                        xpRequired = GetExperienceReqForLevelConstant(_levelXPConstant, i + 1)
                    });
                else if(playerLevelRemoteDatas[i].overrideXPRequired == false)
                {
                    playerLevelRemoteDatas[i].xpRequired = GetExperienceReqForLevelConstant(_levelXPConstant, i + 1);
                }


                levelCurve.AddKey(i / (float) count, playerLevelRemoteDatas[i].xpRequired / (float) maxXP);
            }

        }
        
        private static int GetTotalStarsRequired()
        {
            var sum = FindObjectOfType<FactoryManager>()
                .PersistentUpgrades.Upgrades
                .Sum(x => x.Levels
                    .Sum(y => y.cost));

            return sum;
        }

        //====================================================================================================================//
        
        public IEnumerable GetRemainingPartOptions(in int levelIndex)
        {
            var index = levelIndex;
            var list = RemotePartProfileScriptableObject.GetImplementedParts(false) as ValueDropdownList<PART_TYPE>;
            var partUnlockedUpTo = playerLevelRemoteDatas
                .Where(x => x.level <= index)
                .SelectMany(x => x.unlockData
                    .Where(y => y.Unlock == PlayerLevelRemoteData.UNLOCK_TYPE.PART)
                    .Select(y => y.PartType))
                .ToList();
            partUnlockedUpTo.AddRange(partsUnlockedAtStart);
            partUnlockedUpTo = partUnlockedUpTo.Distinct().ToList();

            return list?.Where(x => !partUnlockedUpTo.Contains(x.Value));

        }
        
        private IEnumerable GetImplementedParts() => RemotePartProfileScriptableObject.GetImplementedParts(false);

        //====================================================================================================================//
        

        [OnInspectorInit]
        private void FillLevelSummary()
        {
            var unlocksUpTo = playerLevelRemoteDatas
                .Select(x => new
                {
                    Level = x.level,
                    Parts = x.unlockData
                        .Where(y => y.Unlock == PlayerLevelRemoteData.UNLOCK_TYPE.PART)
                        .Select(y => y.PartType),
                    Patches = x.unlockData
                        .Where(y => y.Unlock == PlayerLevelRemoteData.UNLOCK_TYPE.PATCH)
                        .Where(y => y.PatchType != PATCH_TYPE.EMPTY)
                        .Select(y => new PatchData
                        {
                            Type = (int)y.PatchType,
                            Level = y.Level
                        })
                }).ToList();

            var outList = new List<LevelSummaryData>();
            foreach (var unlockData in unlocksUpTo)
            {
                if(unlockData.Parts.IsNullOrEmpty() && unlockData.Patches.IsNullOrEmpty())
                    continue;
                
                outList.Add(new LevelSummaryData
                {
                    level = unlockData.Level,
                    parts = $"Parts: {string.Join(", ", unlockData.Parts)}",
                    patches = $"Patches: {string.Join(", ", unlockData.Patches)}",
                });
                
            }

            test = outList;
        }

        /*private bool AlreadyHasPatch()
        {
            for (int i = playerLevelRemoteDatas.Count - 1; i >= 0; i++)
            {
                playerLevelRemoteDatas
            }
        }*/
#endif

        #endregion //Unity Editor

        //====================================================================================================================//
        
    }
}
