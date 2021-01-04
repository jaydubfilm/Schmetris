using System;
using Recycling;
using UnityEngine;

namespace StarSalvager
{
    [Obsolete]
    public class ScrapyardComponent : MonoBehaviour, ICustomRecycle
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


        //ICustomRecycle Functions
        //============================================================================================================//

        public virtual void CustomRecycle(params object[] args)
        {
            //SetAttached(false);
        }

        //====================================================================================================================//
    }
}

