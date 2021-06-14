using Recycling;
using UnityEngine;

namespace StarSalvager
{
    //Explore this as solution to removing the requirement: http://answers.unity.com/answers/874150/view.html
    //[RequireComponent(typeof(SpriteRenderer))]
    public abstract class Actor2DBase : MonoBehaviour, IRecycled, ISetSpriteLayer
    {
        public virtual Vector3 Position => transform.position;
        
        //IRecycle Properties
        //====================================================================================================================//
        
        public bool IsRecycled { get; set; }

        //Actor2DBase Properties
        //====================================================================================================================//
            
        public new SpriteRenderer renderer
        {
            get
            {
                if (_spriteRenderer == null)
                    _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

                return _spriteRenderer;
            }
        }
        private SpriteRenderer _spriteRenderer;
        
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
        
        public virtual void SetSprite(Sprite sprite)
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
