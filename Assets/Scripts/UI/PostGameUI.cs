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
using UnityEngine.EventSystems;
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
        
        private Coroutine _postGameCoroutine;

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
                
                if (_postGameCoroutine == null) 
                    return;
                StopCoroutine(_postGameCoroutine);
                _postGameCoroutine = null;
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

            xpSlider.minValue = PlayerSaveAccountData.GetExperienceReqForLevel(startLevel - 1);
            xpSlider.maxValue = PlayerSaveAccountData.GetExperienceReqForLevel(startLevel);
            
            xpSlider.value = startXP;

            xpSliderText.text = $"{startXP}/{xpSlider.maxValue}";
            starCountText.text = $"{startStars}{TMP_SpriteHelper.STAR_ICON}";
            
            postGameWindow.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(closeButton.gameObject);

            _postGameCoroutine = StartCoroutine(PostGameUICoroutine(startXP, startStars));
        }

        private IEnumerator PostGameUICoroutine(int currentXP, int currentStars)
        {
            const float QUICK_PAUSE = 0.25f;
            const float MED_PAUSE = 0.35f;
            const float LONG_PAUSE = 0.4f;
            //--------------------------------------------------------------------------------------------------------//
            void SetupCurrencyElement(in Sprite iconSprite, in int count)
            {
                var data = new XPData
                {
                    Sprite = iconSprite,
                    Count = count,
                    XpPerCount = 0
                };
                var currenciesXPElement = xpElementScrollview.AddElement(data);
                currenciesXPElement.transform.SetSiblingIndex(0);
                currenciesXPElement.Init(data);
                currenciesXPElement.SetCount(count);
            }
            
            float UpdateXP(in int addXp)
            {
                var levelPause = false;
                currentXP += addXp;
                if (currentXP > xpSlider.maxValue)
                {
                    var level = PlayerSaveAccountData.GetCurrentLevel(currentXP);
                    xpSlider.minValue = PlayerSaveAccountData.GetExperienceReqForLevel(level - 1);
                    xpSlider.maxValue = PlayerSaveAccountData.GetExperienceReqForLevel(level);
                    

                    currentStars++;
                    starCountText.text = $"{currentStars}{TMP_SpriteHelper.STAR_ICON}";
                    //return 1f;
                    levelPause = true;
                }

                xpSlider.value = currentXP;
                xpSliderText.text = $"{currentXP}/{xpSlider.maxValue}";

                return levelPause ? MED_PAUSE : QUICK_PAUSE;
            }

            //--------------------------------------------------------------------------------------------------------//
            
            yield return new WaitForSeconds(MED_PAUSE);
            
            var combos = PlayerDataManager.GetCombosMadeThisRun();
            foreach (var i in combos)
            {
                if (i.Value == 0)
                    continue;
                
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

            //--------------------------------------------------------------------------------------------------------//
            
            //Based on the remaining XP assume (FOR NOW) that its from completing waves
            var waveXp = PlayerDataManager.GetXP() - currentXP;

            if (waveXp > 0)
            {
                var waveXPData = new XPData
                {
                    Sprite = null,
                    Count = 1,
                    XpPerCount = waveXp
                };

                var waveXPElement = xpElementScrollview.AddElement(waveXPData);
                waveXPElement.transform.SetSiblingIndex(0);
                waveXPElement.Init(waveXPData);
                waveXPElement.SetCountTextUnformatted("Completed Waves");
                waveXPElement.SetXP(waveXp);

                UpdateXP(waveXp);
            }
            
            //--------------------------------------------------------------------------------------------------------//

            var factoryManager = FactoryManager.Instance;
            yield return new WaitForSeconds(QUICK_PAUSE);
            SetupCurrencyElement(factoryManager.gearsSprite, PlayerDataManager.GetGearsThisRun());
            yield return new WaitForSeconds(QUICK_PAUSE);
            SetupCurrencyElement(factoryManager.silverSprite, PlayerDataManager.GetSilverThisRun());
            yield return new WaitForSeconds(QUICK_PAUSE);
            SetupCurrencyElement(factoryManager.stardustSprite, PlayerDataManager.GetXPThisRun());
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
