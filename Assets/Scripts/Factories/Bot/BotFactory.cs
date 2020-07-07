﻿using Recycling;
using UnityEngine;

namespace StarSalvager.Factories
{
    public class BotFactory : FactoryBase
    {
        private readonly GameObject prefab;
        private readonly GameObject scrapyardPrefab;
        
        //============================================================================================================//

        public BotFactory(GameObject prefab, GameObject scrapyardPrefab)
        {
            this.prefab = prefab;
            this.scrapyardPrefab = scrapyardPrefab;
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


        public GameObject CreateScrapyardGameObject()
        {
            var outData = !Recycler.TryGrab<ScrapyardBot>(out GameObject gameObject) ? Object.Instantiate(scrapyardPrefab) : gameObject;
            outData.name = "ScrapyardBot";
            return outData;
        }

        public T CreateScrapyardObject<T>()
        {
            return CreateScrapyardGameObject().GetComponent<T>();
        }

        //============================================================================================================//

    }
}


