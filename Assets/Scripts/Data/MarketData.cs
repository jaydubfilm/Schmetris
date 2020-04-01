using UnityEngine;

[CreateAssetMenu(fileName = "New Market Data", menuName = "Market Data")]
public class MarketData : ScriptableObject
{
    //public string marketName;
    public GameObject brick;
    public MarketLevelData[] brickLevels;
}

[System.Serializable]
public class MarketLevelData
{
    public int buyPrice;
    public int sellPrice;
}
