using System.Collections;
using System.Collections.Generic;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;
using UnityEngine.EventSystems;

namespace StarSalvager.UI.Wreckyard
{
    public class PartChoiceButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private static DroneDesignUI DroneDesignUI
        {
            get
            {
                if(_droneDesignUI == null)
                    _droneDesignUI = FindObjectOfType<DroneDesignUI>();

                return _droneDesignUI;
            }
        }
        private static DroneDesignUI _droneDesignUI;
        
        private PartDetailsUI PartDetailsUI
        {
            get
            {
                if (_partDetailsUI == null)
                    _partDetailsUI = FindObjectOfType<PartDetailsUI>();

                return _partDetailsUI;
            }
        }
        private PartDetailsUI _partDetailsUI;
        
        private PART_TYPE _partType;
        private PartData _partData;

        private new RectTransform transform;

        public void SetPartType(in PART_TYPE partType)
        {
            if(transform == null)
                transform = gameObject.transform as RectTransform;

            _partType = partType;
            _partData = new PartData
            {
                Type = (int) _partType,
                Coordinate = Vector2Int.zero,
                Patches = new List<PatchData>()
            };
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            PartDetailsUI.ShowPartDetails(true, _partData, transform);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            PartDetailsUI.ShowPartDetails(false, new PartData(), null);
        }
    }
}
