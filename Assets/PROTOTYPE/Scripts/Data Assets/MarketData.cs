using UnityEngine;

namespace StarSalvager.Prototype
{
    [System.Obsolete("Prototype Only Script")]
//Controls brick display and prices in the scrapyard
    [CreateAssetMenu(fileName = "New Market Data", menuName = "Market Data")]
    public class MarketData : ScriptableObject
    {
        //Brick display name
        public string marketName;

        //Brick prefab
        public GameObject brick;

        //Markat data dependent on the level of the player's brick
        public MarketLevelData[] brickLevels;
    }

//Stores market buy and sell rates for this type of brick
    [System.Obsolete("Prototype Only Script")]
    [System.Serializable]
    public class MarketLevelData
    {
        public int buyPrice;
        public int sellPrice;
    }
}