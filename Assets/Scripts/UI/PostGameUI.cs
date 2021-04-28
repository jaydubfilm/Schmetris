using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.UI.Elements;
using StarSalvager.Values;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class PostGameUI : MonoBehaviour
    {
        //====================================================================================================================//

       [SerializeField, OnValueChanged("GetXP"), HorizontalGroup("Row1")]
        private int Level = 1;
        [SerializeField, HorizontalGroup("Row1"), DisplayAsString, LabelWidth(20)]
        private int xp;

        [OnInspectorInit]
        private void GetXP()
        {
            xp = PlayerSaveAccountData.GetExperienceReqForLevel(Level);
        }
        
        [SerializeField,DisplayAsString, HorizontalGroup("Row2"), LabelText("Level")] private int level = 1;
        [SerializeField, HorizontalGroup("Row2"), LabelWidth(20), OnValueChanged("GetLevel")]
        private int XP;
        
        [OnInspectorInit]
        private void GetLevel()
        {
            level = PlayerSaveAccountData.GetCurrentLevel(XP);
        }
        
        [SerializeField]
        private XPUIElementScrollView xpElementScrollview;

        [SerializeField] private Slider xpSlider;
        [SerializeField] private TMP_Text starCountText;

        //====================================================================================================================//
        
        // Start is called before the first frame update
        private void Start()
        {

        }

        //====================================================================================================================//

        private void SetupUI()
        {
            
        }

        public static void ShowPostGameUI()
        {
            
        }

        //====================================================================================================================//
        
        
    }
}
