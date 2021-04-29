using System;
using System.Collections;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using UnityEngine;
using StarSalvager.UI.Elements;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Saving;
using StarSalvager.Values;
using TMPro;
using UnityEngine.UI;

namespace StarSalvager.UI
{
    public class PostGameUI : MonoBehaviour
    {
        //====================================================================================================================//

        [SerializeField, Required] private GameObject postGameWindow;
        
        [SerializeField]
        private XPUIElementScrollView xpElementScrollview;

        [SerializeField] private Slider xpSlider;
        [SerializeField] private TMP_Text xpSliderText;
        [SerializeField] private TMP_Text starCountText;

        [SerializeField]
        private Button closeButton;

        //====================================================================================================================//

        private void OnEnable()
        {
            if (!PlayerDataManager.ShouldShownSummary())
                return;

            ShowPostGameUI();
        }

        // Start is called before the first frame update
        private void Start()
        {
            SetupUI();
            postGameWindow.SetActive(false);
        }

        //====================================================================================================================//

        private void SetupUI()
        {
            closeButton.onClick.AddListener(() =>
            {
                postGameWindow.SetActive(false);
            });
        }

        [Button, DisableInEditorMode]
        public void ShowPostGameUI()
        {
            xpElementScrollview.ClearElements();
            
            var startStars = PlayerDataManager.GetStars() - PlayerDataManager.GetStarsThisRun();
            var currentXP = PlayerDataManager.GetXP();
            var startXP = currentXP - PlayerDataManager.GetXPThisRun();

            var startLevel = PlayerSaveAccountData.GetCurrentLevel(startXP);
            var newLevel = PlayerSaveAccountData.GetCurrentLevel(currentXP);
            
            //TODO Need to add stars somewhere!
            
            xpSlider.minValue = PlayerSaveAccountData.GetExperienceReqForLevel(startLevel);
            xpSlider.maxValue = PlayerSaveAccountData.GetExperienceReqForLevel(startLevel + 1);

            xpSlider.value = startXP;

            xpSliderText.text = $"{startXP}/{xpSlider.maxValue}";
            starCountText.text = $"{startStars}{TMP_SpriteHelper.STAR_ICON}";
            
            postGameWindow.SetActive(true);
            StartCoroutine(PostGameUICoroutine(startXP, startStars));
        }

        private IEnumerator PostGameUICoroutine(int currentXP, int currentStars)
        {
            
            //--------------------------------------------------------------------------------------------------------//
            
            float UpdateXP(in int addXp)
            {
                currentXP += addXp;
                if (currentXP > xpSlider.maxValue)
                {
                    var level = PlayerSaveAccountData.GetCurrentLevel(currentXP);
                    xpSlider.minValue = PlayerSaveAccountData.GetExperienceReqForLevel(level);
                    xpSlider.maxValue = PlayerSaveAccountData.GetExperienceReqForLevel(level + 1);

                    currentStars++;
                    starCountText.text = $"{currentStars}{TMP_SpriteHelper.STAR_ICON}";
                    return 1f;
                }

                xpSlider.value = currentXP;
                xpSliderText.text = $"{currentXP}/{xpSlider.maxValue}";

                return 0.25f;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            yield return new WaitForSeconds(2f);
            
            var combos = PlayerDataManager.GetCombosMadeThisRun();
            foreach (var i in combos)
            {
                var xp = FactoryManager.Instance.ComboRemoteData.GetRemoteData(i.Key.ComboType).points;
                //TODO Spawn Text XP Element
                var data = new XPData
                {
                    Sprite = i.Key.BitType.GetSprite(i.Key.FromLevel),
                    Count = i.Value,
                    XpPerCount = xp
                };

                var trackedElement = xpElementScrollview.AddElement(data);
                trackedElement.transform.SetSiblingIndex(0);
                trackedElement.Init(data);

                for (int ii = 0; ii < i.Value; ii++)
                {
                    trackedElement.SetCount(ii + 1);
                    trackedElement.SetXP((ii + 1) * xp);
                    
                    var pause = UpdateXP(xp);

                    yield return new WaitForSeconds(pause);
                }
                //outString += $"{i.Key.BitType}[{i.Key.FromLevel}] x{i.Value}\t+{xp * i.Value}xp\n";
            }

        }

        //====================================================================================================================//

        [Button("Print Summary String"), DisableInEditorMode]
        private void ShowPostGameUIString()
        {
            var outString = string.Empty;
            //TODO Get Pre XP & current XP, determine how many levels were gained
            var currentXP = PlayerDataManager.GetXP();
            var startXP = currentXP - PlayerDataManager.GetXPThisRun();

            var startLevel = PlayerSaveAccountData.GetCurrentLevel(startXP);
            var newLevel = PlayerSaveAccountData.GetCurrentLevel(currentXP);
            var starsEarned = newLevel - startLevel;

            outString += $"XP - From {startXP} to {currentXP}\n";
            outString += $"Level - From {startLevel} to {newLevel}\n";
            outString += $"Earned Stars {starsEarned}\n";

            outString += "\n";
            //TODO Get Combos Made (Each color, including silver)
            var combos = PlayerDataManager.GetCombosMadeThisRun();
            foreach (var i in combos)
            {
                var xp = FactoryManager.Instance.ComboRemoteData.GetRemoteData(i.Key.ComboType).points;
                outString += $"{i.Key.BitType}[{i.Key.FromLevel}] x{i.Value}\t+{xp * i.Value}xp\n";
            }

            Debug.Log(outString);
        }
        
    }
}
