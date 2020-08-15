using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StarSalvager.Values
{
    public class PlayerMetadata
    {
        private int maxSaveSlots = 3;
        
        public List<int> saveFileLastAccessedOrder = new List<int>();

        public int ActivateNextEmptySaveFile()
        {
            int index = saveFileLastAccessedOrder.Count;
            if (index >= maxSaveSlots)
            {
                index = maxSaveSlots - 1;
            }
            else
            {
                saveFileLastAccessedOrder.Add(index);
            }
            MoveSaveFileToFront(index);
            return GetSaveFileAtIndex(0);
        }

        public void MoveSaveFileToFront(int index)
        {
            int value = saveFileLastAccessedOrder[index];
            saveFileLastAccessedOrder.RemoveAt(index);
            saveFileLastAccessedOrder.Insert(0, value);
        }

        public int GetSaveFileAtIndex(int index)
        {
            return saveFileLastAccessedOrder[index];
        }
    }
}