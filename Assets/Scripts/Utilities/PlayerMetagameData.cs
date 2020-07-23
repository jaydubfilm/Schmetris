using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager
{
    public class PlayerMetagameData : MonoBehaviour
    {
        public Dictionary<int, int> maxSectorProgression = new Dictionary<int, int>();

        public void AddSectorProgression(int sector, int waveAt)
        {
            if (maxSectorProgression.ContainsKey(sector))
                maxSectorProgression[sector] = Mathf.Max(maxSectorProgression[sector], waveAt);
            else
                maxSectorProgression.Add(sector, waveAt);
        }
    }
}