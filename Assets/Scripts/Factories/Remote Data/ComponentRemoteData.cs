using System;
using Sirenix.OdinInspector;

namespace StarSalvager.Factories.Data
{
    [Serializable]
    public class ComponentRemoteData : RemoteDataBase
    {
        [FoldoutGroup("$name")] public string name;

        [FoldoutGroup("$name")] public COMPONENT_TYPE componentType;
        
        [FoldoutGroup("$name")] public float health;

        #region IEquatable

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(RemoteDataBase other)
        {
            if (other is ComponentRemoteData componentRemote)
                return componentType == componentRemote.componentType;


            return false;
        }

        /// <summary>
        /// This only compares Type and not all individual properties
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((PartRemoteData) obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
            /*unchecked
            {
                var hashCode = (name != null ? name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int)bitType;
                hashCode = (hashCode * 397) ^ (health != null ? health.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (resource != null ? resource.GetHashCode() : 0);
                return hashCode;
            }*/
        }

        #endregion //IEquatable
    }
}



