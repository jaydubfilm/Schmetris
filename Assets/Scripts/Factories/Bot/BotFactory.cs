using Recycling;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class BotFactory : FactoryBase
    {
        private readonly GameObject prefab;
        
        private readonly Sabre sabrePrefab;
        
        //============================================================================================================//

        public BotFactory(GameObject prefab, Sabre sabrePrefab/*, GameObject shieldPrototypePrefab, GameObject alertIconPrefab*/)
        {
            this.prefab = prefab;
            this.sabrePrefab = sabrePrefab;
        }
        //============================================================================================================//
        
        public override GameObject CreateGameObject()
        {
            var outData = !Recycler.TryGrab<Bot>(out GameObject gameObject) 
                ? Object.Instantiate(prefab) 
                : gameObject;
            
            outData.name = nameof(Bot);
             
            return outData;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }

        //============================================================================================================//

        public Sabre CreateSabreObject()
        {
            var outData = !Recycler.TryGrab(out Sabre sabre)
                ? Object.Instantiate(sabrePrefab)
                : sabre;
            
            outData.name = nameof(Sabre);
            return outData;
        }

    }
}


