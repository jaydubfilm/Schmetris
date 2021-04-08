using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager.ScriptableObjects.Procedural
{
    [CreateAssetMenu(fileName = "Stage Profile Data", menuName = "Star Salvager/Scriptable Objects/Stage Profile Data")]
    public class StageProfileDataScriptableObject : ScriptableObject
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
        
        //public 

        //====================================================================================================================//
        
        [MinMaxSlider(0,500)]
        public Vector2Int bitSpawnsPerMin;

        //public 
    }
}
