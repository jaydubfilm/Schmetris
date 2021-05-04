using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using UnityEngine;

using PlayerLevelRemoteData = StarSalvager.Factories.Data.PlayerLevelRemoteData;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Player Levels Remote", menuName = "Star Salvager/Scriptable Objects/Player Levels Data")]
    public class PlayerLevelsRemoteDataScriptableObject : ScriptableObject
    {
        [SerializeField, BoxGroup("Experience Points"), Range(0f,1f)]
        private float levelXPConstant = 0.915f;
        private float _levelXPConstant => 1f - levelXPConstant;
        
        [SerializeField, ListDrawerSettings(ShowPaging = false)]
        private List<PlayerLevelRemoteData> playerLevelRemoteDatas;
        
        //XP Info Considerations: https://www.youtube.com/watch?v=MCPruAKSG0g
        //Alt option: https://gamedev.stackexchange.com/a/20946
        //Based on: https://gamedev.stackexchange.com/a/13639
        public int GetCurrentLevel(in int xp)
        {
            //level = constant * sqrt(XP)
            //level = (sqrt(100(2experience+25))+50)/100
            var constant = _levelXPConstant;

            return Mathf.RoundToInt(constant * Mathf.Sqrt(xp));
        }

        public int GetExperienceReqForLevel(in int level)
        {
            //XP = (level / constant)^2
            //experience =(level^2+level)/2*100-(level*100)
            
            var constant = _levelXPConstant;

            return Mathf.RoundToInt(Mathf.Pow(level / constant, 2));
        }

#if UNITY_EDITOR

        [SerializeField, PropertyOrder(-900)]
        private AnimationCurve levelCurve;
        
        [DisplayAsString, ShowInInspector, PropertyOrder(-1000)] 
        public int totalLevels { get; private set; }

        [OnInspectorInit]
        private void UpdateValues()
        {
            totalLevels = GetTotalStarsRequired();
        }

        [Button, PropertyOrder(-1000)] 
        private void GenerateLevelData()
        {
            var count = GetTotalStarsRequired();
            var maxXP = GetExperienceReqForLevel(count);

            levelCurve = new AnimationCurve();
            
            for (var i = 0; i < count; i++)
            {
                if(playerLevelRemoteDatas.Count == i)
                    playerLevelRemoteDatas.Add(new PlayerLevelRemoteData
                    {
                        level = i + 1,
                        xpRequired = GetExperienceReqForLevel(i + 1)
                    });
                else if(playerLevelRemoteDatas[i].overrideXPRequired == false)
                {
                    playerLevelRemoteDatas[i].xpRequired = GetExperienceReqForLevel(i + 1);
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
        
#endif
    }
}
