using System;
using StarSalvager;
using StarSalvager.Factories;
using StarSalvager.UI;
using StarSalvager.Utilities.Saving;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class PurchasePatchUIElement : UIElement<Purchase_PatchData>
    {
        [SerializeField] private Button purchaseButton;

        [SerializeField] private TMP_Text titleText;
        
        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += CheckCanAfford;
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= CheckCanAfford;
        }

        public override void Init(Purchase_PatchData data)
        {
            this.data = data;

            var patchName = FactoryManager.Instance.PatchRemoteData.GetRemoteData(data.PatchData.Type).name;
            titleText.text = $"{patchName} {data.PatchData.Level + 1}\nCost: {data.cost}";

            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(OnPurchasePressed);

            CheckCanAfford();
        }

        private void CheckCanAfford()
        {
            purchaseButton.interactable = PlayerDataManager.GetComponents() >= data.cost;
        }
        
        private void OnPurchasePressed()
        {
            var currentComponents = PlayerDataManager.GetComponents();
            if (currentComponents < data.cost)
                return;

            currentComponents -= data.cost;

            PlayerDataManager.SetComponents(currentComponents);
            PlayerDataManager.AddPatchToStorage(data.PatchData);

        }
    }

    public struct Purchase_PatchData : IEquatable<Purchase_PatchData>
    {
        public int cost;
        public PatchData PatchData;

        #region IEquatable

        public bool Equals(Purchase_PatchData other)
        {
            return cost == other.cost && PatchData.Equals(other.PatchData);
        }

        public override bool Equals(object obj)
        {
            return obj is Purchase_PatchData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (cost * 397) ^ PatchData.GetHashCode();
            }
        }

        #endregion //IEquatable
    }

    [Serializable]
    public class PurchasePatchUIElementScrollView : UIElementContentScrollView<PurchasePatchUIElement, Purchase_PatchData>
    {
    }
}
