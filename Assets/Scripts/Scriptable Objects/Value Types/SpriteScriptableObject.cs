using System;
using UnityEngine;

namespace StarSalvager.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Sprite_Value", menuName = "Star Salvager/Scriptable Objects/Value Types/Sprite Value")]
    public class SpriteScriptableObject : ScriptableObject
    {
        [SerializeField]
        private Sprite m_value;

        public Sprite GetValue()
        {
            return m_value;
        }
    }
}