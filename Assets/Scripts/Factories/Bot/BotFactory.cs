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
            var outData = !Recycler.TryGrab<Bot>(out GameObject gameObject) ? Object.Instantiate(prefab) : gameObject;
            outData.name = "Bot";
            return outData;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }
        
        //============================================================================================================//

    }
}


