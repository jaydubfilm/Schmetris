using System;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "String_Value", menuName = "Star Salvager/Scriptable Objects/Value Types/String Value")]
    public class StringScriptableObject : ScriptableObject
    {
        [SerializeField]
        private string m_value;

        public string GetValue()
        {
            return m_value;
        }
    }
}