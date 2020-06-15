using System;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Vector3_Value", menuName = "Star Salvager/Scriptable Objects/Value Types/Vector3 Value")]
    public class Vector3ScriptableObject : ScriptableObject
    {
        [SerializeField]
        private Vector3 m_value;

        public Vector3 GetValue()
        {
            return m_value;
        }
    }
}