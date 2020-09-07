using Recycling;
using Sirenix.OdinInspector;
using UnityEngine;

namespace StarSalvager
{
    public class ScrapyardComponent : MonoBehaviour, IComponent, IAttachable, ICustomRecycle
    {
        public new Transform transform
        {
            get
            {
                if (_transform == null)
                    _transform = gameObject.GetComponent<Transform>();

                return _transform;
            }
        }
        private Transform _transform;
        
        //IComponent Properties
        //============================================================================================================//
        public COMPONENT_TYPE Type { get; set; }

        public int level => 0;

        //IAttachable Properties
        //============================================================================================================//
        
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }
        public bool CountAsConnectedToCore => true;
        public bool CanDisconnect => true;
        public bool CanShift => true;
        public bool CountTowardsMagnetism => true;

        //IAttachableFunctions
        //============================================================================================================//
        
        public void SetAttached(bool isAttached)
        {
            Attached = isAttached;
        }
        

        //ICustomRecycle Functions
        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            SetAttached(false);
        }
        
        //============================================================================================================//


    }
}

