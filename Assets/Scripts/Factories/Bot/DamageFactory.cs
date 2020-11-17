using Recycling;
using UnityEngine;

namespace StarSalvager.Factories
{
    [System.Obsolete("Use the Effects Factory")]
    public class DamageFactory //: FactoryBase
    {
        /*private readonly GameObject damagePrefab;
        
        //============================================================================================================//

        public DamageFactory(GameObject damagePrefab)
        {
            this.damagePrefab = damagePrefab;
        }
        
        //============================================================================================================//

        
        public override GameObject CreateGameObject()
        {
            return !Recycler.TryGrab<Damage>(out GameObject gameObject) ? Object.Instantiate(damagePrefab) : gameObject;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }
        
        //============================================================================================================//*/
    }
}


