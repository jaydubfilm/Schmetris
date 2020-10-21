﻿using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using UnityEngine;

namespace StarSalvager.Factories
{
    //FIXME This needs to be cleaned up, feels messy
    public class PartAttachableFactory : AttachableFactoryBase<PartProfile, PART_TYPE>
    {
        private RemotePartProfileScriptableObject remotePartData;

        public PartAttachableFactory(AttachableProfileScriptableObject factoryProfile, RemotePartProfileScriptableObject remotePartData) : base(factoryProfile)
        {
            this.remotePartData = remotePartData;
        }

        //============================================================================================================//

        public void UpdatePartData(PART_TYPE partType, int level, ref ScrapyardPart part)
        {
            var profile = factoryProfile.GetProfile(partType);
            var sprite = profile.GetSprite(level);
            
            part.SetLevel(level);
            part.SetSprite(sprite);
        }

        public bool CheckLevelExists(PART_TYPE partType, int level)
        {
            return factoryProfile.GetProfile(partType).Sprites.Length > level;
        }

        //============================================================================================================//

        public PartRemoteData GetRemoteData(PART_TYPE partType)
        {
            return remotePartData.GetRemoteData(partType);
        }
        
        public PartProfile GetProfileData(PART_TYPE partType)
        {
            return factoryProfile.GetProfile(partType);
        }
        
        //============================================================================================================//

        public GameObject CreateGameObject(BlockData blockData)
        {
            var remote = remotePartData.GetRemoteData((PART_TYPE) blockData.Type);
            var profile = factoryProfile.GetProfile((PART_TYPE)blockData.Type);
            var sprite = profile.GetSprite(blockData.Level);
            var startingHealth = remote.levels[blockData.Level].health;//.health[blockData.Level];

            
            //--------------------------------------------------------------------------------------------------------//

            Part temp;
            //If there is an animation associated with this profile entry, create the animated version of the prefab
            if (profile.animation != null)
            {
                if (!Recycler.TryGrab(out AnimatedPart anim))
                {
                    anim = CreateAnimatedObject<AnimatedPart>();
                }
                
                anim.SimpleAnimator.SetAnimation(profile.animation);
                temp = anim;
            }
            else
            {
                if (!Recycler.TryGrab(out temp))
                {
                    temp = CreateObject<Part>();
                }
            }
            
            //--------------------------------------------------------------------------------------------------------//

            //var temp = Object.Instantiate(factoryProfile.Prefab).GetComponent<Part>();
            temp.SetSprite(sprite);
            temp.LoadBlockData(blockData);


            //temp.StartingHealth =
            temp.SetupHealthValues(startingHealth, blockData.Health);

            temp.gameObject.name = $"{temp.Type}_{temp.level}";
            return temp.gameObject;
        }
        public T CreateObject<T>(BlockData blockData)
        {
            var temp = CreateGameObject(blockData);

            return temp.GetComponent<T>();

        }

        //============================================================================================================//

        public GameObject CreateGameObject(PART_TYPE partType, int level = 0)
        {
            var startingHealth = remotePartData.GetRemoteData(partType).levels[level].health;
            var blockData = new BlockData
            {
                Level = level,
                Type = (int) partType,
                Health = startingHealth
            };

            return CreateGameObject(blockData);
        }

        public T CreateObject<T>(PART_TYPE partType, int level = 0)
        {
            var temp = CreateGameObject(partType, level);

            return temp.GetComponent<T>();
        }

        //============================================================================================================//

        public GameObject CreateScrapyardGameObject(BlockData blockData)
        {
            var profileData = (PartProfileScriptableObject)factoryProfile;
            
            var remote = remotePartData.GetRemoteData((PART_TYPE)blockData.Type);
            var profile = factoryProfile.GetProfile((PART_TYPE)blockData.Type);
            var sprite = profile.GetSprite(blockData.Level);
            var startingHealth = remote.levels[blockData.Level].health;
            
            //var temp = Object.Instantiate(factoryProfile.ScrapyardPrefab).GetComponent<ScrapyardPart>();

            if (!Recycler.TryGrab(out ScrapyardPart temp))
            {
                temp = CreateScrapyardObject<ScrapyardPart>();
            }
            
            temp.LoadBlockData(blockData);
            temp.SetSprite(temp.Destroyed ? profileData.GetDamageSprite(blockData.Level) : sprite);
            temp.SetupHealthValues(startingHealth, blockData.Health);


            var gameObject = temp.gameObject;
            gameObject.name = $"{temp.Type}_{temp.level}";
            
            return gameObject;
        }

        public void SetOverrideSprite(in IPart toOverride, PART_TYPE overrideType)
        {
            if (toOverride.Destroyed)
                return;
            
            var profile = factoryProfile.GetProfile(overrideType);
            var sprite = profile.GetSprite(toOverride.level);

            //TODO This should be the same function
            switch (toOverride)
            {
                case Part part:
                    part.SetSprite(sprite);
                    break;
                case ScrapyardPart scrapyardPart:
                    scrapyardPart.SetSprite(sprite);
                    break;
            }
            
            
        }

        //============================================================================================================//

        public GameObject CreateScrapyardGameObject(PART_TYPE partType, int level = 0)
        {
            var startingHealth = remotePartData.GetRemoteData(partType).levels[level].health;
            var blockData = new BlockData
            {
                Level = level,
                Type = (int)partType,
                Health = startingHealth
            };

            return CreateScrapyardGameObject(blockData);
        }

        public T CreateScrapyardObject<T>(PART_TYPE partType, int level = 0)
        {
            var temp = CreateScrapyardGameObject(partType, level);

            return temp.GetComponent<T>();
        }
        
        public T CreateScrapyardObject<T>(BlockData blockData)
        {
            var temp = CreateScrapyardGameObject(blockData);

            return temp.GetComponent<T>();
        }
        
        //============================================================================================================//
        
        public GameObject CreateScrapyardGameObject()
        {
            return Object.Instantiate(factoryProfile.ScrapyardPrefab);
        }
        
        public T CreateScrapyardObject<T>()
        {
            var temp = CreateScrapyardGameObject();

            return temp.GetComponent<T>();
        }

        //============================================================================================================//

        public GameObject CreateAnimatedGameObject()
        {
            return Object.Instantiate(factoryProfile.AnimatedPrefab);
        }
        
        public T CreateAnimatedObject<T>()
        {
            var temp = CreateAnimatedGameObject();

            return temp.GetComponent<T>();
        }
        
        public override GameObject CreateGameObject()
        {
            return Object.Instantiate(factoryProfile.Prefab);
        }

        public override T CreateObject<T>()
        {
            var temp = CreateGameObject();

            return temp.GetComponent<T>();
        }

        //============================================================================================================//
    }
}
