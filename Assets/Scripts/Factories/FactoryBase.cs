using UnityEngine;

namespace StarSalvager.Factories
{
    /// <summary>
    /// Base Factory class that can be inherited to create Objects easily
    /// </summary>
    public abstract class FactoryBase
    {
        /// <summary>
        /// Create GameObject version of preset Prefab
        /// </summary>
        /// <returns></returns>
        public abstract GameObject CreateGameObject();
        /// <summary>
        /// Create GameObject of preset prefab but return UnityEngine.Object of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public abstract T CreateObject<T>() where T: MonoBehaviour;
    }   
}
