using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
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

            part.SetSprite(sprite);
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

        public PART_TYPE GetWreckPartTypeOption()
        {
            List<PART_TYPE> partType = new List<PART_TYPE>();

            partType.Add(PART_TYPE.GUN);
            partType.Add(PART_TYPE.SNIPER);
            partType.Add(PART_TYPE.BOMB);
            partType.Add(PART_TYPE.FREEZE);
            partType.Add(PART_TYPE.ARMOR);
            partType.Add(PART_TYPE.SHIELD);
            partType.Add(PART_TYPE.REPAIR);

            return partType[Random.Range(0, partType.Count)];
        }

        //============================================================================================================//

        public GameObject CreateGameObject(PartData partData)
        {
            var type = (PART_TYPE) partData.Type;
            var remote = remotePartData.GetRemoteData(type);
            var profile = factoryProfile.GetProfile(type);
            var sprite = profile.GetSprite(0);
            //var startingHealth = remote.levels[partData.Level].health;//.health[blockData.Level];


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

                temp.gameObject.name = $"{nameof(AnimatedPart)}_{type}";
            }
            else
            {
                if (!Recycler.TryGrab(out temp))
                {
                    temp = CreateObject<Part>();
                }

                temp.gameObject.name = $"{nameof(Part)}_{type}";
            }

            //--------------------------------------------------------------------------------------------------------//

            temp.SetSprite(sprite);
            temp.LoadBlockData(partData);
            temp.LockRotation = remote.lockRotation;

            temp.gameObject.name = $"{temp.Type}";
            return temp.gameObject;
        }
        public T CreateObject<T>(PartData partData)
        {
            var temp = CreateGameObject(partData);

            return temp.GetComponent<T>();

        }

        //============================================================================================================//

        public GameObject CreateGameObject(PART_TYPE partType)
        {
            var patchSockets = remotePartData.GetRemoteData(partType).PatchSockets;
            var blockData = new PartData
            {
                Type = (int) partType,
                Patches = new PatchData[patchSockets]
            };

            return CreateGameObject(blockData);
        }

        public T CreateObject<T>(PART_TYPE partType)
        {
            var temp = CreateGameObject(partType);

            return temp.GetComponent<T>();
        }

        //============================================================================================================//

        public GameObject CreateScrapyardGameObject(PartData partData)
        {
            var profile = factoryProfile.GetProfile((PART_TYPE)partData.Type);
            var sprite = profile.GetSprite(0);


            if (!Recycler.TryGrab(out ScrapyardPart temp))
            {
                temp = CreateScrapyardObject<ScrapyardPart>();
            }

            temp.LoadBlockData(partData);
            temp.SetSprite(sprite);

            var gameObject = temp.gameObject;
            gameObject.name = $"{temp.Type}";

            return gameObject;
        }

        public void SetOverrideSprite(in IPart toOverride, PART_TYPE overrideType)
        {

            var profile = factoryProfile.GetProfile(overrideType);
            var sprite = profile.GetSprite(0);

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

        public GameObject CreateScrapyardGameObject(PART_TYPE partType)
        {
            var patchSockets = remotePartData.GetRemoteData(partType).PatchSockets;
            var blockData = new PartData
            {
                Type = (int)partType,
                Patches = new PatchData[patchSockets]
            };

            return CreateScrapyardGameObject(blockData);
        }

        public T CreateScrapyardObject<T>(PART_TYPE partType)
        {
            var temp = CreateScrapyardGameObject(partType);

            return temp.GetComponent<T>();
        }

        public T CreateScrapyardObject<T>(PartData partData)
        {
            var temp = CreateScrapyardGameObject(partData);

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
