using System;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Vector2_Value", menuName = "Star Salvager/Scriptable Objects/Value Types/Vector2 Value")]
    public class Vector2ScriptableObject : ScriptableObject
    {
        [SerializeField]
        private Vector2 m_value;

        public Vector2 GetValue()
        {
            return m_value;
        }
    }
}