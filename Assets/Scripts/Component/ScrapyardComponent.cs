using Recycling;
using Sirenix.OdinInspector;
using StarSalvager.Factories;
using StarSalvager.Utilities.Debugging;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Values;
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

        //IAttachable Properties
        //============================================================================================================//
        
        [ShowInInspector, ReadOnly]
        public Vector2Int Coordinate { get; set; }
        [ShowInInspector, ReadOnly]
        public bool Attached { get; set; }
        public bool CountAsConnected => true;
        public bool CanDisconnect => true;
        public bool CanShift => true;
        
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

