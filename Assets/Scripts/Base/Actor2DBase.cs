using Recycling;
using UnityEngine;

namespace StarSalvager
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class Actor2DBase : MonoBehaviour, IRecycled
    {
        //IRecycle Properties
        //====================================================================================================================//
        
        public bool IsRecycled { get; set; }

        //Actor2DBase Properties
        //====================================================================================================================//
            
        public new SpriteRenderer renderer
        {
            get
            {
                if (_renderer == null)
                    _renderer = gameObject.GetComponent<SpriteRenderer>();

                return _renderer;
            }
        }
        private SpriteRenderer _renderer;
        
        public new Transform transform
        {
            get
            {
                if (_transformSet) return _transform;

                _transform = gameObject.GetComponent<Transform>();
                _transformSet = _transform != null;

                return _transform;
            }
        }
        private Transform _transform;
        private bool _transformSet;

        //Actor2DBase Properties
        //====================================================================================================================//
        
        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        public virtual void SetColor(Color color)
        {
            renderer.color = color;
        }
        
        public virtual void SetSortingLayer(string sortingLayerName, int sortingOrder = 0)
        {
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder;
        }

        //====================================================================================================================//
        
    }
}
