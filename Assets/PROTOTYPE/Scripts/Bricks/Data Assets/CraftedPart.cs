﻿using UnityEngine;

namespace StarSalvager.Prototype
{

    [System.Obsolete("Prototype Only Script")]
//Component requiring crafting to upgrade - Could potentially be combined with MarketData assets
    public class CraftedPart : MonoBehaviour
    {
        //Costs associated with upgrading bot to associated level
        public string[] scrapyardName;
        public int[] moneyToCraft;
        public int[] redToCraft;
        public int[] blueToCraft;
        public int[] greenToCraft;
        public int[] yellowToCraft;
        public int[] greyToCraft;
        public Sprite[] basePartToCraft;
    }
}