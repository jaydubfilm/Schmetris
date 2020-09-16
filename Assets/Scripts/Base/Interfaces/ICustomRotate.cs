using UnityEngine;

namespace StarSalvager
{
    public interface ICustomRotate
    {
        /// <summary>
        /// Used to replace the default rotation given to IAttachables 
        /// </summary>
        void CustomRotate(Quaternion rotation);
    }
}

