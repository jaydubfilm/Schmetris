using Sirenix.OdinInspector;
using UnityEngine;
using StarSalvager.UI.Elements;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using TMPro;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class PostGameUI : MonoBehaviour
    {
        //====================================================================================================================//

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

        [Button]
        public void ShowPostGameUI()
        {
            //TODO Get Pre XP & current XP, determine how many levels were gained
            var currentXP = PlayerDataManager.GetXP();
            var startXP = currentXP - PlayerDataManager.GetXPThisRun();

            var startLevel = PlayerSaveAccountData.GetCurrentLevel(startXP);
            var newLevel = PlayerSaveAccountData.GetCurrentLevel(currentXP);
            var starsEarned = newLevel - startLevel;

            //TODO Get Combos Made (Each color, including silver)
            //TODO Get enemies killed (assuming they give xp)
        }

        //====================================================================================================================//
        
        
    }
}
