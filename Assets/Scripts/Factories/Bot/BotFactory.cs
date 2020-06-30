using Recycling;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class BotFactory : FactoryBase
    {
        private readonly GameObject prefab;
        
        //============================================================================================================//

        public BotFactory(GameObject prefab)
        {
            this.prefab = prefab;
        }
        
        //============================================================================================================//

        
        public override GameObject CreateGameObject()
        {
            return !Recycler.TryGrab<Bot>(out GameObject gameObject) ? Object.Instantiate(prefab) : gameObject;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }
        
        //============================================================================================================//

    }
}


