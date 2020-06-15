using System;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Int_Value", menuName = "Star Salvager/Scriptable Objects/Value Types/Int Value")]
    public class IntScriptableObject : ScriptableObject
    {
        [SerializeField]
        private int m_value;

        public int GetValue()
        {
            return m_value;
        }
    }
}