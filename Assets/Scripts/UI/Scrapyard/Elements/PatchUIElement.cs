using System;
using StarSalvager.Factories;
using TMPro;
using UnityEngine;

namespace StarSalvager.UI.Wreckyard
{
    [Obsolete]
    public class PatchUIElement : UIElement<Patch_Storage>/*,IBeginDragHandler, IDragHandler, IEndDragHandler*/
    {
        [SerializeField] private TMP_Text titleText;

        private static DroneDesigner droneDesigner
        {
            get
            {
                if (_droneDesigner == null)
                    _droneDesigner = FindObjectOfType<DroneDesigner>();
                
                return _droneDesigner;
            }
        }
        private static DroneDesigner _droneDesigner;
        
        private Canvas _canvas;
        private Vector2 _offset;

        private RectTransform _originalParent;
        private int _originalIndex;

        public override void Init(Patch_Storage data)
        {
            _canvas = GetComponentInParent<Canvas>();

            
            
            this.data = data;

            var patchName = FactoryManager.Instance.PatchRemoteData.GetRemoteData((PATCH_TYPE) data.PatchData.Type)
                .name;

            titleText.text = $"{patchName} {data.PatchData.Level + 1}";

            _originalParent = transform.parent as RectTransform;
            _originalIndex = transform.GetSiblingIndex();

        }

        public void ResetInScrollview()
        {
            transform.SetParent(_originalParent);
            transform.SetSiblingIndex(_originalIndex);
        }

        //====================================================================================================================//
        
        /*public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out var worldPoint);

            _offset = transform.position - worldPoint;
            
            transform.SetParent(_canvas.transform, true);
            
            droneDesigner.BeginDragPatch(this);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvas.transform as RectTransform,
                Input.mousePosition,
                null,
                out var worldPoint);
            
            //throw new NotImplementedException();
            transform.position = worldPoint + (Vector3)_offset;
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            //throw new NotImplementedException();
            droneDesigner.EndDragPatch();
        }*/

        //====================================================================================================================//
        
    }

    [Obsolete]
    public class Patch_Storage : IEquatable<Patch_Storage>
    {
        public int storageIndex;
        public PatchData PatchData;


        public bool Equals(Patch_Storage other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return PatchData.Equals(other.PatchData) && storageIndex == other.storageIndex;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Patch_Storage) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    
    [System.Serializable]
    public class PatchUIElementScrollView: UIElementContentScrollView<PatchUIElement, Patch_Storage>
    {}
}
