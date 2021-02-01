using Recycling;
using StarSalvager.Utilities;
using StarSalvager.Values;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class BotFactory : FactoryBase
    {
        private readonly GameObject prefab;
        /*private readonly GameObject shieldPrototypePrefab;
        private readonly GameObject alertIconPrefab;*/
        private readonly GameObject scrapyardPrefab;
        
        //============================================================================================================//

        public BotFactory(GameObject prefab, GameObject scrapyardPrefab/*, GameObject shieldPrototypePrefab, GameObject alertIconPrefab*/)
        {
            this.prefab = prefab;
            this.scrapyardPrefab = scrapyardPrefab;

            /*this.shieldPrototypePrefab = shieldPrototypePrefab;
            this.alertIconPrefab = alertIconPrefab;*/
        }
        
        //============================================================================================================//

        /*public Shield CreateShield()
        {
            var outData = !Recycler.TryGrab<Shield>(out Shield shield) ? Object.Instantiate(shieldPrototypePrefab).GetComponent<Shield>() : shield;
            return outData;
        }

        public FlashSprite CreateAlertIcon()
        {
            var outData = !Recycler.TryGrab<FlashSprite>(out FlashSprite flashSprite) ? Object.Instantiate(alertIconPrefab).GetComponent<FlashSprite>() : flashSprite;
            return outData;
        }*/
        
        //============================================================================================================//
        
        public override GameObject CreateGameObject()
        {
            var outData = !Recycler.TryGrab<Bot>(out GameObject gameObject) 
                ? Object.Instantiate(prefab) 
                : gameObject;
            
            outData.name = nameof(Bot);

            /*if (outData.GetComponent<IHealth>() is IHealth iHealth)
            {
                var startingHealth = Globals.BotStartingHealth;
                iHealth.SetupHealthValues(startingHealth,startingHealth);
            }*/
             
            return outData;
        }

        public override T CreateObject<T>()
        {
            return CreateGameObject().GetComponent<T>();
        }

        //============================================================================================================//


        public GameObject CreateScrapyardGameObject()
        {
            var outData = !Recycler.TryGrab<ScrapyardBot>(out GameObject gameObject)
                ? Object.Instantiate(scrapyardPrefab)
                : gameObject;
            
            
            outData.name = nameof(ScrapyardBot);
            return outData;
        }

        public T CreateScrapyardObject<T>()
        {
            return CreateScrapyardGameObject().GetComponent<T>();
        }

        //============================================================================================================//

    }
}


