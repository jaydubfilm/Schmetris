using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using StarSalvager.Cameras;
using StarSalvager.Factories;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using StarSalvager.Values;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace StarSalvager.UI.Wreckyard
{
    public class PartDetailsUI : MonoBehaviour
    {
        //Properties
        //====================================================================================================================//
        
        [SerializeField, FoldoutGroup("Part Details Window")]
        private RectTransform partDetailsContainerRectTransform;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private Image partImage;

        private Image _partBorderImage;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partNameText;
        [SerializeField, FoldoutGroup("Part Details Window")]
        private Image partCategoryImage;
        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partCategoryText;
        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partUseTypeText;
        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partDescriptionText;

        //[FormerlySerializedAs("PatchUis")] [SerializeField, FoldoutGroup("Part Details Window")]
        //private DroneDesignUI.PatchUI[] patchUis;

        [SerializeField, FoldoutGroup("Part Details Window")]
        private TMP_Text partDetailsText;


        public bool HoveringStoragePartUIElement { get; set; }

        //Unity Functions
        //====================================================================================================================//

        private void Start()
        {
            HidePartDetails();

            _partBorderImage = PartAttachableFactory.CreateUIPartBorder(partImage, BIT_TYPE.WHITE);
        }

        //PartDetailsUI Functions
        //====================================================================================================================//
        public void HidePartDetails()
        {
            ShowPartDetails(false, null);
            HoveringStoragePartUIElement = false;
        }
        public void ShowPartDetails(bool show, in ScrapyardPart scrapyardPart)
        {
            var screenPoint = show
                ? CameraController.Camera.WorldToScreenPoint(scrapyardPart.transform.position + Vector3.right)
                : Vector3.zero;
            
            var partData = show ? scrapyardPart.ToBlockData() : new PartData();

            ShowPartDetails(show, partData, screenPoint);
        }
        
        public void ShowPartDetails(bool show, in PartData partData, in RectTransform rectTransform)
        {
            HoveringStoragePartUIElement = show;
            
            var screenPoint = show ? RectTransformUtility.WorldToScreenPoint(null,
                (Vector2) rectTransform.position + Vector2.right * rectTransform.sizeDelta.x)
                    : Vector2.zero;

            ShowPartDetails(show, partData, screenPoint);
        }

        private void ShowPartDetails(in bool show, in PartData partData, in Vector2 screenPoint)
        {

            //--------------------------------------------------------------------------------------------------------//

            void SetRectSize(in TMP_Text tmpText, in float multiplier = 1.388f)
            {
                tmpText.ForceMeshUpdate();

                var lineCount = tmpText.GetTextInfo(tmpText.text).lineCount;
                var lineSize = tmpText.fontSize * multiplier;
                var rectTrans = (RectTransform)tmpText.transform;
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
            
            partDetailsContainerRectTransform.gameObject.SetActive(show);

            if (!show)
                return;
            
            var canvasRect = GetComponentInParent<Canvas>().transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null,
                out var localPoint);

            partDetailsContainerRectTransform.anchoredPosition = localPoint;

            //====================================================================================================================//

            var partType = (PART_TYPE) partData.Type;
            var partRemote = partType.GetRemoteData();
            var partProfile = partType.GetProfileData();

            //====================================================================================================================//

            partNameText.text = partRemote.name;
            partUseTypeText.text = partRemote.isManual ? "Manually Triggered" : "Automatic";
            partDescriptionText.text = partRemote.description;
            
            partImage.sprite = partProfile.Sprite;
            partImage.color = Globals.UsePartColors ? partRemote.category.GetColor() : Color.white;
            
            partCategoryImage.color = partRemote.category.GetColor();
            partCategoryText.text = partRemote.category.GetCategoryName();
            _partBorderImage.sprite = partType.GetBorderSprite();

            partDetailsText.text = partData.GetPartDetails(partRemote);

            /*for (var i = 0; i < partData.Patches.Count; i++)
            {
                if (i >= patchUis.Length)
                    break;
                
                var patchData = partData.Patches[i];
                var type = (PATCH_TYPE) patchData.Type;

                patchUis[i].backgroundImage.enabled = type != PATCH_TYPE.EMPTY;

                patchUis[i].text.text = type == PATCH_TYPE.EMPTY
                    ? string.Empty
                    : $"{patchRemoteData.GetRemoteData(type).name} {patchData.Level + 1}";

            }*/

            //====================================================================================================================//

            //Resize the details text to accomodate the text
            StartCoroutine(ResizeDelayedCoroutine(partDetailsText, partDescriptionText));
            
            partDetailsContainerRectTransform.TryFitInScreenBounds(canvasRect, 20f);
            
        }
        //====================================================================================================================//
        
    }
}
