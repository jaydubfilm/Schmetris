using System;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Float_Value", menuName = "Star Salvager/Scriptable Objects/Value Types/Float Value")]
    public class FloatScriptableObject : ScriptableObject
    {
        [SerializeField]
        private float m_value;

        public float GetValue()
        {
            return m_value;
        }
    }
}