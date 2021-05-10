using System;
using StarSalvager;
using StarSalvager.Factories;
using StarSalvager.UI;
using StarSalvager.Utilities.Helpers;
using StarSalvager.Utilities.Saving;
using StarSalvager.Utilities.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    public class PurchasePatchUIElement : ButtonReturnUIElement<Purchase_PatchData, Purchase_PatchData>
    {
        //[SerializeField] private Button purchaseButton;

        [SerializeField] private TMP_Text buyButtonText;
        [SerializeField] private TMP_Text titleText;
        
        private void OnEnable()
        {
            PlayerDataManager.OnValuesChanged += CheckCanAfford;
            CheckCanAfford();
        }

        private void OnDisable()
        {
            PlayerDataManager.OnValuesChanged -= CheckCanAfford;
        }

        public override void Init(Purchase_PatchData data, Action<Purchase_PatchData> onButtonPressed)
        {
            this.data = data;

            var patchName = FactoryManager.Instance.PatchRemoteData.GetRemoteData(data.PatchData.Type).name;
            titleText.text = $"{patchName} {data.PatchData.Level + 1}";

            buyButtonText.text = $"{data.gears}{TMP_SpriteHelper.GEAR_ICON}";
            if (data.silver > 0) buyButtonText.text += $"\n{data.silver}{TMP_SpriteHelper.SILVER_ICON}";

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                onButtonPressed?.Invoke(this.data);
            });

            CheckCanAfford();
        }

        private void CheckCanAfford()
        {
            button.interactable = PlayerDataManager.GetGears() >= data.gears &&
                                  PlayerDataManager.GetSilver() >= data.silver;
        }
        
        /*private void OnPurchasePressed()
        {
            var currentComponents = PlayerDataManager.GetGears();
            if (currentComponents < data.cost)
                return;

            currentComponents -= data.cost;

            PlayerDataManager.SetGears(currentComponents);
            PlayerDataManager.AddPatchToStorage(data.PatchData);

        }*/
    }

    public struct Purchase_PatchData : IEquatable<Purchase_PatchData>
    {
        public int index;
        public int gears;
        public int silver;
        public PatchData PatchData;

        #region IEquatable

        public bool Equals(Purchase_PatchData other)
        {
            return index == other.index && gears == other.gears && silver == other.silver && PatchData.Equals(other.PatchData);
        }

        public override bool Equals(object obj)
        {
            return obj is Purchase_PatchData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = index;
                hashCode = (hashCode * 397) ^ gears;
                hashCode = (hashCode * 397) ^ silver;
                hashCode = (hashCode * 397) ^ PatchData.GetHashCode();
                return hashCode;
            }
        }
        #endregion //IEquatable

    }

    [Serializable]
    public class PurchasePatchUIElementScrollView : UIElementContentScrollView<PurchasePatchUIElement, Purchase_PatchData>
    {
    }
}
