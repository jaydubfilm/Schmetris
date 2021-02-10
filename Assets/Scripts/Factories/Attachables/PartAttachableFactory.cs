using System;
using Recycling;
using StarSalvager.Factories.Data;
using StarSalvager.ScriptableObjects;
using StarSalvager.Utilities.Extensions;
using StarSalvager.Utilities.JsonDataTypes;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace StarSalvager.Factories
{
    //FIXME This needs to be cleaned up, feels messy
    public class PartAttachableFactory : AttachableFactoryBase<PartProfile, PART_TYPE>
    {
        public enum PART_OPTION_TYPE
        {
            BasicWeapon,
            PowerWeapon,
            Any
        }
        
        private RemotePartProfileScriptableObject remotePartData;

        public PartAttachableFactory(AttachableProfileScriptableObject factoryProfile, RemotePartProfileScriptableObject remotePartData) : base(factoryProfile)
        {
            this.remotePartData = remotePartData;
        }

        //============================================================================================================//

        public void UpdatePartData(PART_TYPE partType, int level, ref ScrapyardPart part)
        {
            var remoteData = remotePartData.GetRemoteData(partType);
            var profile = factoryProfile.GetProfile(partType);
            var sprite = profile.GetSprite(level);

            var color = remoteData.category == BIT_TYPE.NONE
                ? Color.white
                : FactoryManager.Instance.BitProfileData.GetProfile(remoteData.category).color;

            part.SetSprite(sprite);
            part.SetColor(color);
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

        public static void SelectPartOptions(ref PART_TYPE[] options, in PART_OPTION_TYPE partOptionType)
        {
            var partTypes = new List<PART_TYPE>();

            switch (partOptionType)
            {
                case PART_OPTION_TYPE.BasicWeapon:
                    partTypes = new List<PART_TYPE>
                    {
                        PART_TYPE.GUN,
                        PART_TYPE.RAILGUN
                    };
                    break;
                case PART_OPTION_TYPE.PowerWeapon:
                    partTypes = new List<PART_TYPE>
                    {
                        PART_TYPE.FREEZE,
                        PART_TYPE.BOMB
                    };
                    break;
                case PART_OPTION_TYPE.Any:
                    partTypes = new List<PART_TYPE>
                    {
                        PART_TYPE.GUN,
                        PART_TYPE.SNIPER,
                        PART_TYPE.RAILGUN,
                        PART_TYPE.BOMB,
                        PART_TYPE.FREEZE,
                        PART_TYPE.ARMOR,
                        PART_TYPE.SHIELD,
                        PART_TYPE.REPAIR
                    };
                    break;
            }

            for (int i = 0; i < options.Length; i++)
            {
                var option = partTypes[Random.Range(0, partTypes.Count)];
                partTypes.Remove(option);

                options[i] = option;
            }

            /*if (exclusionType.HasValue)
            {
                partType.Remove(exclusionType.Value);
            }

            if (partType.Count != 0) 
                return partType[Random.Range(0, partType.Count)];
            
            throw new ArgumentException("No valid part types to return");*/
            
            /*Debug.LogError("No valid part types to return");
            
            return PART_TYPE.GUN;*/
        }

        /*public static PART_TYPE GetWreckPartTypeOption(PART_TYPE? exclusionType = null)
        {
            var partType = new List<PART_TYPE>
            {
                PART_TYPE.GUN,
                PART_TYPE.SNIPER,
                PART_TYPE.RAILGUN,
                PART_TYPE.BOMB,
                PART_TYPE.FREEZE,
                PART_TYPE.ARMOR,
                PART_TYPE.SHIELD,
                PART_TYPE.REPAIR
            };


            if (exclusionType.HasValue)
            {
                var index = partType.FindIndex(x => x == exclusionType.Value);
                partType.RemoveAt(index);
            }

            if (partType.Count == 0)
            {
                throw new ArgumentException("No valid part types to return");
                //return PART_TYPE.GUN;
            }

            return partType[Random.Range(0, partType.Count)];
        }*/

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
            
            var remoteData = remotePartData.GetRemoteData(type);
            var color = remoteData.category == BIT_TYPE.NONE
                ? Color.white
                : FactoryManager.Instance.BitProfileData.GetProfile(remoteData.category).color;

            temp.SetSprite(sprite);
            temp.SetColor(color);
            temp.LoadBlockData(partData);
            temp.LockRotation = remote.lockRotation;
            temp.partColor = color;

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
            var type = (PART_TYPE) partData.Type;
            var profile = factoryProfile.GetProfile(type);
            var sprite = profile.GetSprite(0);


            if (!Recycler.TryGrab(out ScrapyardPart temp))
            {
                temp = CreateScrapyardObject<ScrapyardPart>();
            }
            
            var remoteData = remotePartData.GetRemoteData(type);
            var color = remoteData.category == BIT_TYPE.NONE
                ? Color.white
                : FactoryManager.Instance.BitProfileData.GetProfile(remoteData.category).color;

            temp.LoadBlockData(partData);
            temp.SetSprite(sprite);
            temp.SetColor(color);

            var gameObject = temp.gameObject;
            gameObject.name = $"{temp.Type}";

            return gameObject;
        }

        public void SetOverrideSprite(in IPart toOverride, PART_TYPE overrideType)
        {
            var remoteData = remotePartData.GetRemoteData(overrideType);
            var color = remoteData.category == BIT_TYPE.NONE
                ? Color.white
                : FactoryManager.Instance.BitProfileData.GetProfile(remoteData.category).color;
            
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
                    scrapyardPart.SetColor(color);
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
