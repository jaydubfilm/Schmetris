using System;
using System.Linq;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace StarSalvager.UI.Scrapyard
{
    //FIXME This needs to be cleaned up once approved
    public class PartCardElement : ButtonReturnUIElement<TEST_PartUpgrd, TEST_PartUpgrd>
    {
        public bool canUsePart { get; private set; }
        public string partName { get; private set; }

        private Toggle _toggle;
        
        [SerializeField]
        private Image overlayImage;

        [SerializeField]
        private TMP_Text titleText;

        [SerializeField] 
        private TMP_Text patchText;
        [SerializeField]
        private TMP_Text descriptionText;

        [SerializeField]
        private Image partImage;
        [SerializeField]
        private Button purchaseButton;
        [SerializeField]
        private Image glowImage;

        //PartCardElement Functions
        //====================================================================================================================//

        public override void Init(TEST_PartUpgrd data, Action<TEST_PartUpgrd> onPressedCallback)
        {
            this.data = data;
            
            //Set Button
            //--------------------------------------------------------------------------------------------------------//
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                if (!(contentScrollView is UpgradeUIElementScrollView upgradeUIElementScrollView))
                    throw new Exception();
                
                upgradeUIElementScrollView.SetElementSelected(this);
            });
            
            //Set Purchase Button Info
            //--------------------------------------------------------------------------------------------------------//
            
            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(() =>
            {
                onPressedCallback?.Invoke(this.data);
            });

            //Setup Visuals
            //--------------------------------------------------------------------------------------------------------//
            
            var partType = (PART_TYPE)data.PartData.Type;
            var partRemote = FactoryManager.Instance.PartsRemoteData;
            var partProfile = FactoryManager.Instance.PartsProfileData;

            partName = $"{partRemote.GetRemoteData(partType).name}";
            //ShowPreviewChanges(this.data.PartData);
            
            partImage.sprite = partProfile.GetProfile(partType).Sprite;
            partImage.color = partRemote.GetRemoteData(partType).category.GetColor();

            SetSelected(false);

            var canUse = CanSelectPart(data.PurchasePatchData.PatchData);
            button.interactable = canUse;
            
            overlayImage.gameObject.SetActive(!canUse);

            canUsePart = canUse;
            
        }
        
        private bool CanSelectPart(in PatchData patchData)
        {
            var patchType = (PATCH_TYPE)patchData.Type;
            //Determine if the patches are all full
            if (data.PartData.Patches.All(x => x.Type != (int) PATCH_TYPE.EMPTY))
                return false;
            
            //Determine if this patch can fit on this part
            var patchRemoteDataData = FactoryManager.Instance.PatchRemoteData.GetRemoteData(patchType);
            var partType = (PART_TYPE) data.PartData.Type;

            return patchRemoteDataData.fitsAnyPart || patchRemoteDataData.allowedParts.Contains(partType);
        }

        
        
        //====================================================================================================================//

        public void SetSelected(in bool selected)
        {
            purchaseButton.gameObject.SetActive(selected);
            glowImage.gameObject.SetActive(selected);

            if (!selected)
            {
                ShowPreviewChanges(data.PartData);
                return;
            }

            ShowPreviewChanges(data.PartData, data.PurchasePatchData.PatchData);

            /*
            var partData = new PartData(data.PartData);
            
            partData.AddPatch(data.PurchasePatchData.PatchData);
            ShowPreviewChanges(partData);*/

        }

        private void ShowPreviewChanges(in PartData partData)
        {
            titleText.text = partName;
            patchText.text = partData.GetPatchNames();
            descriptionText.text = partData.GetPartDetails();
        }
        
        private void ShowPreviewChanges(in PartData partData, in PatchData patchData)
        {
            titleText.text = partName;
            patchText.text = partData.GetPatchNames();
            descriptionText.text = partData.GetPartDetailsPatchPreview(patchData);
        }
        
    }

    //====================================================================================================================//
    
    #region Extra Data

    public class TEST_PartUpgrd : IEquatable<TEST_PartUpgrd>
    {
        public bool isAttached;
        public Purchase_PatchData PurchasePatchData;
        public PartData PartData;
        public int itemIndex;

        public bool Equals(TEST_PartUpgrd other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return isAttached == other.isAttached && PurchasePatchData.Equals(other.PurchasePatchData) &&
                   PartData.Equals(other.PartData) && itemIndex == other.itemIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TEST_PartUpgrd) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = isAttached.GetHashCode();
                hashCode = (hashCode * 397) ^ PurchasePatchData.GetHashCode();
                hashCode = (hashCode * 397) ^ PartData.GetHashCode();
                hashCode = (hashCode * 397) ^ itemIndex;
                return hashCode;
            }
        }
    }

    [Serializable]
    public class UpgradeUIElementScrollView : UIElementContentScrollView<PartCardElement, TEST_PartUpgrd>
    {
        public void SetElementSelected(in PartCardElement partCardElement)
        {
            foreach (var cardElement in Elements)
            {
                cardElement.SetSelected(partCardElement.Equals(cardElement));
            }
        }

        public override void SortList()
        {
            var newOder = Elements
                .OrderByDescending(x => x.canUsePart)
                    .ThenBy(x => x.partName)
                .ToArray();

            for (var i = 0; i < newOder.Length; i++)
            {
                var partCardElement = newOder[i];
                partCardElement.transform.SetSiblingIndex(i);
                
            }
        }
        
    }

    #endregion //Extra Data

}