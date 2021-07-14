using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.PersistentUpgrades.Data;
using StarSalvager.UI.Hints;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Inputs;
using StarSalvager.Utilities.Interfaces;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace StarSalvager.UI.PersistentUpgrades
{
    public class PersistentUpgradesUI : MonoBehaviour, IBuildNavigationProfile
    {
        //Properties
        //====================================================================================================================//

        #region Properties

        [SerializeField] private RectTransform buttonContainerPrefab;
        [SerializeField] private UpgradeUIElement upgradeUIElementPrefab;

        [SerializeField] private RectTransform scrollContentRect;

        [SerializeField] private Slider progressSlider;
        [SerializeField] private TMP_Text starCountText;

        [SerializeField] private Button backButton;

        private Dictionary<UPGRADE_TYPE, RectTransform> _uiElementContainers;
        private Dictionary<UpgradeData, UpgradeUIElement> _uiElements;

        [SerializeField, BoxGroup("Hover Window")]
        private RectTransform upgradeDetailsContainerRectTransform;

        [SerializeField, BoxGroup("Hover Window")]
        private TMP_Text upgradeTitleText;

        [SerializeField, BoxGroup("Hover Window")]
        private TMP_Text upgradeDescriptionText;


        private bool _showingHint;
        private void IsShowingHint(bool showingHint) => _showingHint = showingHint;
        #endregion //Properties

        //Unity Engine
        //====================================================================================================================//

        #region Unity Functions

        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += OnValueChanged;
            HintManager.OnShowingHintAction += IsShowingHint;
            OnValueChanged();

            //Make sure that we hide the pop-up window when its not needed
            ShowUpgradeDetails(false, new UpgradeData(), null);
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= OnValueChanged;
            HintManager.OnShowingHintAction -= IsShowingHint;
        }

        #endregion //Unity Functions

        //====================================================================================================================//

        public void SetupPersistentUpgradesUI()
        {
            if (HintManager.CanShowHint(HINT.STAR))
                HintManager.TryShowHint(HINT.STAR);

            //--------------------------------------------------------------------------------------------------------//

            UpgradeUIElement CreateElement(in RectTransform parent, in UpgradeData upgradeData, in int index)
            {
                var temp = Instantiate(upgradeUIElementPrefab, parent, false);
                temp.transform.SetSiblingIndex(index);
                //temp.Init(upgradeData);
                _uiElements.Add(upgradeData, temp);

                return temp;
            }

            //--------------------------------------------------------------------------------------------------------//

            if (_uiElementContainers.IsNullOrEmpty())
                _uiElementContainers = new Dictionary<UPGRADE_TYPE, RectTransform>();

            if (_uiElements.IsNullOrEmpty())
                _uiElements = new Dictionary<UpgradeData, UpgradeUIElement>();

            var upgrades = FactoryManager.Instance.PersistentUpgrades.Upgrades;

            foreach (var upgradeRemoteData in upgrades)
            {
                if (!_uiElementContainers.TryGetValue(upgradeRemoteData.upgradeType, out var container))
                {
                    container = Instantiate(buttonContainerPrefab, scrollContentRect, false);
                    _uiElementContainers.Add(upgradeRemoteData.upgradeType, container);
                }

                for (var i = 1; i < upgradeRemoteData.Levels.Count; i++)
                {
                    var data = new UpgradeData(upgradeRemoteData.upgradeType, upgradeRemoteData.bitType, i);
                    if (!_uiElements.TryGetValue(data, out var element))
                    {
                        element = CreateElement(container, data, i - 1);
                        element.Init(data, TryPurchaseUpgrade, HoverUpgradeElement);

                    }
                    else
                        element.TryUpdate();

                }
            }

            //--------------------------------------------------------------------------------------------------------//
            OnValueChanged();
        }

        //Callback Functions
        //====================================================================================================================//

        #region Callback Functions

        private void OnValueChanged()
        {
            starCountText.text = $"{PlayerDataManager.GetStars()}{TMP_SpriteHelper.STAR_ICON}";
            progressSlider.value = 0;
        }

        private void TryPurchaseUpgrade(UpgradeData upgradeData)
        {
            if (_showingHint)
                return;

            var remoteData = FactoryManager.Instance.PersistentUpgrades
                .GetRemoteData(upgradeData.Type, upgradeData.BitType);

            var cost = remoteData.Levels[upgradeData.Level].cost;

            if (PlayerDataManager.GetStars() < cost)
                return;

            Alert.ShowAlert("Purchase",
                $"Are you sure you want to purchase {remoteData.name} Level {upgradeData.Level} for {cost}{TMP_SpriteHelper.STAR_ICON}?",
                "Buy",
                "Cancel",
                answer =>
                {
                    ShowUpgradeDetails(false, new UpgradeData(), null);

                    if (answer == false)
                    {
                        UISelectHandler.SetBuildTarget(GetComponent<MainMenuv2>());
                        return;
                    }

                    if (!PlayerDataManager.TrySubtractStars(cost))
                        throw new Exception("Failed attempting to Purchase Upgrade");

                    PlayerDataManager.SetUpgradeLevel(
                        upgradeData.Type,
                        upgradeData.Level,
                        upgradeData.BitType);

                    PlayerDataManager.SavePlayerAccountData();

                    SetupPersistentUpgradesUI();
                    UISelectHandler.SetBuildTarget(GetComponent<MainMenuv2>());
                });
        }

        private void HoverUpgradeElement(UpgradeData upgradeData, RectTransform rectTransform)
        {
            if (_showingHint)
                return;
            
            ShowUpgradeDetails(rectTransform != null, upgradeData, rectTransform);
        }

        private void ShowUpgradeDetails(bool show, in UpgradeData upgradeData, in RectTransform rectTransform)
        {
            var screenPoint = show
                ? RectTransformUtility.WorldToScreenPoint(null,
                    (Vector2) rectTransform.position + Vector2.right * rectTransform.sizeDelta.x)
                : Vector2.zero;

            ShowUpgradeDetails(show, upgradeData, screenPoint);
        }

        private void ShowUpgradeDetails(in bool show, in UpgradeData upgradeData, in Vector2 screenPoint)
        {

            //--------------------------------------------------------------------------------------------------------//

            void SetRectSize(in TMP_Text tmpText, in float multiplier = 1.388f)
            {
                tmpText.ForceMeshUpdate();

                var lineCount = tmpText.GetTextInfo(tmpText.text).lineCount;
                var lineSize = tmpText.fontSize * multiplier;
                var rectTrans = (RectTransform) tmpText.transform;
                var sizeDelta = rectTrans.sizeDelta;

                if (tmpText.GetComponent<LayoutElement>() is LayoutElement layoutElement)
                {
                    sizeDelta.y = Mathf.Max(layoutElement.minHeight, lineSize * lineCount);
                    layoutElement.preferredHeight = sizeDelta.y;
                }
                else
                {
                    sizeDelta.y = lineSize * lineCount;
                }


                rectTrans.sizeDelta = sizeDelta;
            }

            IEnumerator ResizeDelayedCoroutine(params TMP_Text[] args)
            {
                foreach (var tmpText in args)
                {
                    tmpText.ForceMeshUpdate();
                }

                yield return new WaitForEndOfFrame();

                foreach (var tmpText in args)
                {
                    SetRectSize(tmpText);
                }
            }

            //--------------------------------------------------------------------------------------------------------//

            upgradeDetailsContainerRectTransform.gameObject.SetActive(show);

            if (!show)
                return;

            var canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null,
                out var localPoint);

            upgradeDetailsContainerRectTransform.anchoredPosition = localPoint;

            //====================================================================================================================//

            upgradeTitleText.text = upgradeData.GetUpgradeTitleText();
            upgradeDescriptionText.text = upgradeData.GetUpgradeDetailText();

            //====================================================================================================================//

            //Resize the details text to accomodate the text
            StartCoroutine(ResizeDelayedCoroutine(upgradeDescriptionText));

            upgradeDetailsContainerRectTransform.TryFitInScreenBounds(canvasRect, 20f);

        }

        #endregion //Callback Functions

        //====================================================================================================================//

        //FIXME I will want to implement a full navigation for tiers and their elements
        public NavigationProfile BuildNavigationProfile()
        {
            var selectables = new List<Selectable>
            {
                backButton
            };
            var overrides = new List<NavigationOverride>();

            Selectable last = null;
            Selectable first = null;

            var uiElements = _uiElementContainers.Values.ToList();
            var lastSet = false;
            for (int i = uiElements.Count - 1; i >= 0 ; i--)
            {
                var tempSelectables = uiElements[i].GetComponentsInChildren<Selectable>();

                selectables.AddRange(tempSelectables);

                //Get the first element selectable, moving up through each tier
                first = tempSelectables.FirstOrDefault(x => x.interactable && x.enabled);

                //Check to see if an element in the current row is selectable
                if (lastSet)
                    continue;
                
                if (!tempSelectables.Any(x => x.interactable && x.enabled))
                    continue;

                last = tempSelectables.LastOrDefault(x => x.interactable && x.enabled);

                foreach (var selectable in tempSelectables)
                {
                    overrides.Add(new NavigationOverride
                    {
                        FromSelectable = selectable,
                        DownTarget = backButton
                    });
                }

                lastSet = true;
            }

            overrides.Add(new NavigationOverride
            {
                FromSelectable = backButton,
                UpTarget = last
            });

            return new NavigationProfile(first,
                selectables,
                overrides,
                new[]
                {
                    new NavigationRestriction
                    {
                        FromDirection = NavigationRestriction.DIRECTION.RIGHT |
                                        NavigationRestriction.DIRECTION.LEFT |
                                        NavigationRestriction.DIRECTION.DOWN,
                        Selectable = backButton
                    }
                });
        }
    }
}
